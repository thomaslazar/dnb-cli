#!/usr/bin/env bash
# Smoke test: exercises every dnb-cli command against the live DNB SRU endpoint.
# Tests the actual AOT binary, not `dotnet run`.
#
# Usage:
#   bash tools/smoke-test.sh                  # builds Release AOT then runs against live DNB
#   CLI=./path/to/dnb-cli bash tools/smoke-test.sh
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Known-good DNB samples used as test targets
KNOWN_ISBN_BUTCHER=9783837165890   # Jim Butcher, Die dunklen Fälle des Harry Dresden - Blendwerk, Bd. 15 (audiobook)
KNOWN_ISBN_AARONOVITCH=9783423221870  # Ben Aaronovitch, Die Meerjungfrauen von Aberdeen, Bd. 10
NOT_FOUND_ISBN=9780000000002

if [ -z "${CLI:-}" ]; then
    echo "CLI not set — building Release AOT binary..."
    RID="$(uname -s | tr '[:upper:]' '[:lower:]' | sed 's/darwin/osx/')-$(uname -m | sed -e 's/x86_64/x64/' -e 's/aarch64/arm64/' -e 's/arm64/arm64/')"
    dotnet publish "$REPO_ROOT/src/DnbCli/DnbCli.csproj" \
        -c Release -r "$RID" --self-contained true /p:PublishAot=true \
        -o "$REPO_ROOT/src/DnbCli/bin/smoke-test" \
        --nologo -v quiet
    CLI="$REPO_ROOT/src/DnbCli/bin/smoke-test/dnb-cli"
fi

if [ ! -x "$CLI" ]; then
    echo "ERROR: binary not found or not executable at: $CLI" >&2
    exit 1
fi

echo "Binary: $CLI"
echo "Binary size: $(du -h "$CLI" | cut -f1)"
echo ""

PASS=0
FAIL=0
pass() { echo "  PASS: $1"; PASS=$((PASS + 1)); }
fail() { echo "  FAIL: $1 — $2"; FAIL=$((FAIL + 1)); }

assert_jq() {
    local label="$1" expr="$2" json="$3"
    if echo "$json" | python3 -c "
import sys, json
d = json.loads(sys.stdin.read())
assert $expr
" 2>/dev/null; then
        pass "$label"
    else
        fail "$label" "assertion failed: $expr"
        echo "    response: ${json:0:300}"
    fi
}

assert_exit() {
    local label="$1" expected="$2" actual="$3"
    if [ "$expected" = "$actual" ]; then pass "$label"
    else fail "$label" "expected exit $expected, got $actual"
    fi
}

# === help screens ===
echo "=== Help Screens ==="
for cmd in "" "lookup" "search" "self-test" "changelog"; do
    label="help: dnb-cli $cmd --help"
    output=$($CLI $cmd --help 2>&1) || true
    if echo "$output" | grep -q "Description:\|Usage:"; then pass "$label"
    else fail "$label" "no help text"; fi
done

# === self-test ===
echo "=== self-test ==="
if $CLI self-test > /dev/null 2>&1; then pass "self-test exits 0"
else fail "self-test exits 0" "non-zero exit"; fi

# === changelog ===
echo "=== changelog ==="
out="$($CLI changelog)" && pass "changelog runs" || fail "changelog runs" "non-zero"
echo "$out" | grep -q "^## " && pass "changelog starts with ##" || fail "changelog starts with ##" "no h2"

# DNB's SRU endpoint rate-limits aggressive bursts with HTTP 429. Space the
# live-hitting assertions out so a CI run from a single IP isn't seen as a
# burst. Two seconds between DNB hits empirically clears the limiter.
dnb_pause() { sleep 2; }

# === lookup --isbn (hit) ===
echo "=== lookup --isbn (hit) ==="
dnb_pause
out=$($CLI lookup --isbn "$KNOWN_ISBN_BUTCHER"); ec=$?
assert_exit "lookup hit exit 0" 0 $ec
assert_jq "lookup hit returns Butcher ISBN" "'$KNOWN_ISBN_BUTCHER' in d['isbns']" "$out"
assert_jq "lookup hit has contributors" "len(d['contributors']) > 0" "$out"

# === lookup --isbn (miss) ===
echo "=== lookup --isbn (miss) ==="
dnb_pause
set +e; out=$($CLI lookup --isbn "$NOT_FOUND_ISBN" 2>/dev/null); ec=$?; set -e
assert_exit "lookup miss exit 2" 2 $ec
if [ "$(echo "$out" | tr -d '[:space:]')" = "null" ]; then pass "lookup miss stdout is null"
else fail "lookup miss stdout is null" "got: ${out:0:100}"; fi

# === search ===
echo "=== search ==="
dnb_pause
out=$($CLI search --title "Blendwerk" --limit 3); ec=$?
assert_exit "search hit exit 0" 0 $ec
assert_jq "search has results array" "isinstance(d['results'], list) and len(d['results']) > 0" "$out"
assert_jq "search totalResults is int" "isinstance(d['totalResults'], int)" "$out"

# === search no-results ===
echo "=== search no-results ==="
dnb_pause
set +e; out=$($CLI search --title "DEFINITELYNOTAREALSEARCHQUERYYYY" 2>/dev/null); ec=$?; set -e
assert_exit "search no-results exit 2" 2 $ec
assert_jq "search no-results totalResults=0" "d['totalResults'] == 0" "$out"

# === search bad-input ===
echo "=== search bad-input ==="
set +e; $CLI search > /dev/null 2>&1; ec=$?; set -e
assert_exit "search no flags exit 3" 3 $ec

echo ""
echo "========================================"
echo "Smoke test: $PASS passed, $FAIL failed"
echo "========================================"
[ "$FAIL" -eq 0 ]

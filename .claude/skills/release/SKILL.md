---
name: release
description: Create a new dnb-cli release with human review gates. Creates release branch, generates changelog, opens PR for CI validation, then tags and publishes after merge.
disable-model-invocation: true
allowed-tools:
  - Bash
  - Read
  - Write
  - Glob
  - Grep
  - Edit
  - AskUserQuestion
---

# Release dnb-cli

Multi-step release workflow with human gates. You drive each step, pause at gates for human approval before proceeding. Never skip a gate.

## Step 1: Preflight

Verify prerequisites:

```bash
# Must be on main
BRANCH=$(git branch --show-current)
[ "$BRANCH" = "main" ] || { echo "ERROR: must be on main, currently on $BRANCH"; exit 1; }

# Working tree must be clean
git diff --quiet && git diff --cached --quiet || { echo "ERROR: working tree not clean"; git status --short; exit 1; }

# Pull latest
git pull

# Format check
dotnet format DnbCli.sln --verify-no-changes

# Unit tests must pass
dotnet test tests/DnbCli.Tests/DnbCli.Tests.csproj

# Build AOT binary and run self-test (catches AOT serialization issues)
dotnet publish src/DnbCli/DnbCli.csproj -c Release -r linux-x64 --self-contained true /p:PublishAot=true -o ./publish
./publish/dnb-cli self-test
```

Run the smoke test against live DNB:

```bash
CLI=./publish/dnb-cli bash tools/smoke-test.sh
rm -rf publish/
```

If any check fails, stop and report. Do not proceed.

Determine the version number:
- Get the last tag: `git describe --tags --abbrev=0 2>/dev/null || echo "none"`
- Read commits since last tag
- Propose a version based on conventional commits:
  - Any `feat:` commits → bump MINOR
  - Only `fix:`, `docs:`, `test:`, `ci:`, `chore:` → bump PATCH

**GATE: Ask the human to confirm the version number.** Show the proposed version and commit summary. Wait for response.

## Step 2: Create Release Branch and Bump Version

```bash
VERSION="v{version}"
VERSION_NUM="${VERSION#v}"
git checkout -b "release/${VERSION}"
```

Bump `<Version>` in `src/DnbCli/DnbCli.csproj` to `${VERSION_NUM}`. Verify:

```bash
grep "<Version>" src/DnbCli/DnbCli.csproj    # must print <Version>{VERSION_NUM}</Version>
dotnet publish src/DnbCli/DnbCli.csproj -c Release -r linux-x64 --self-contained true /p:PublishAot=true -o ./publish
./publish/dnb-cli --version                   # must print exactly: {VERSION_NUM}
rm -rf publish/
```

Commit:

```bash
git add src/DnbCli/DnbCli.csproj
git commit -m "chore: bump version to ${VERSION_NUM}"
```

## Step 3: Generate Release Notes

Generate `release-notes.md` (gitignored draft) with Highlights and Changes sections:

```bash
LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "")
RANGE="${LAST_TAG:+${LAST_TAG}..}HEAD"
git log --oneline $RANGE --pretty="- %s" | grep -E "^- (feat|fix|refactor|docs|test|ci|chore):" | sort
```

Format:

```markdown
## {version} — YYYY-MM-DD

### Highlights
- ...

### Features
- feat: ...

### Fixes
- fix: ...
```

**GATE: Open `release-notes.md`, ask human to review.** Edit per feedback.

Prepend to `CHANGELOG.md` (keeping the existing top-of-file header):

```bash
git add CHANGELOG.md
git commit -m "docs: add v{version} changelog entry"
```

## Step 4: Open PR for CI Validation

```bash
git push -u origin "release/${VERSION}"
gh pr create --title "release: ${VERSION}" --body "Release ${VERSION}. See CHANGELOG.md for details." --base main
```

Watch CI:

```bash
for i in $(seq 1 10); do
    RUN_ID=$(gh run list --branch "release/${VERSION}" --limit 1 --json databaseId -q '.[0].databaseId')
    [ -n "$RUN_ID" ] && break
    sleep 3
done
gh run watch "$RUN_ID" --exit-status
gh run view "$RUN_ID" --json jobs --jq '.jobs[] | "\(.name)\t\(.conclusion)"'
```

If CI fails, show `gh run view "$RUN_ID" --log-failed` and stop.

**GATE: CI green. Ask human to review and merge the PR.** Show the URL. Wait.

## Step 5: Tag and Create GitHub Release

After merge:

```bash
git checkout main
git pull
gh release create "${VERSION}" --title "${VERSION}" --notes-file release-notes.md
rm release-notes.md
```

**GATE: Confirm the release was created.** Show URL.

## Step 6: Wait for Release CI

The release triggers CI which builds all 6 platforms and attaches binaries:

```bash
for i in $(seq 1 10); do
    RUN_ID=$(gh run list --limit 5 --json databaseId,event -q '[.[] | select(.event=="release")] | .[0].databaseId')
    [ -n "$RUN_ID" ] && break
    sleep 3
done
gh run watch "$RUN_ID" --exit-status
```

If CI fails, show failure details and stop.

## Step 7: Verify

Download and test one binary:

```bash
gh release download "${VERSION}" --pattern "dnb-cli-linux-x64" --dir /tmp/release-verify
chmod +x /tmp/release-verify/dnb-cli-linux-x64
/tmp/release-verify/dnb-cli-linux-x64 self-test
rm -rf /tmp/release-verify
```

**GATE: Ask the human to verify the GitHub Release page** — all 6 binaries attached, release notes render correctly.

## Step 8: Done

Report:
- Release URL
- Version number
- Self-test result
- Changelog committed to repo

## Rules

- NEVER skip a human gate
- NEVER proceed past a failed check
- This skill may commit without asking — commit steps are part of the defined workflow (overrides the ask-before-commit rule in CLAUDE.md)
- Homebrew tap and .deb upload are deferred to a later release (see `docs/ROADMAP.md`); when added, extend Step 6 to watch the additional job

# dnb-cli

A command-line interface for the Deutsche Nationalbibliothek (DNB) SRU catalogue. Built for agent-driven metadata lookup on German-language titles.

Native AOT binary. No runtime required. ~10 MB.

> **Note:** This tool was built using agentic software engineering (AI-assisted coding) and reviewed by a human. See the git history for details.

## Features

- **JSON-only output** — stdout is always valid JSON, errors go to stderr
- **Native AOT** — single self-contained binary, no .NET runtime needed
- **MARC21-XML parsing** — direct, faithful mapping of DNB records to flat JSON
- **Translators, original-language linkage, GND identifiers** — all surfaced with role tags
- **Structured exit codes** — `0` hit, `2` no-results, `3` bad input, `4` network, `5` upstream

## Installation

### Install script (macOS / Linux)

```bash
curl -fsSL https://raw.githubusercontent.com/thomaslazar/dnb-cli/main/install.sh | bash
```

Installs to `~/.local/bin/dnb-cli`. Override with environment variables:

```bash
# specific version
curl -fsSL https://raw.githubusercontent.com/thomaslazar/dnb-cli/main/install.sh | DNB_CLI_VERSION=v0.1.0 bash

# custom directory
curl -fsSL https://raw.githubusercontent.com/thomaslazar/dnb-cli/main/install.sh | DNB_CLI_INSTALL_DIR=/usr/local/bin bash
```

### Install script (Windows)

```powershell
irm https://raw.githubusercontent.com/thomaslazar/dnb-cli/main/install.ps1 | iex
```

### Download a release

Grab the binary for your platform from the [latest release](https://github.com/thomaslazar/dnb-cli/releases/latest):

| Platform | Binary |
|----------|--------|
| Linux x64 | `dnb-cli-linux-x64` |
| Linux ARM64 | `dnb-cli-linux-arm64` |
| macOS Apple Silicon | `dnb-cli-osx-arm64` |
| macOS Intel | `dnb-cli-osx-x64` |
| Windows x64 | `dnb-cli-win-x64.exe` |
| Windows ARM64 | `dnb-cli-win-arm64.exe` |

## Quick start

```bash
dnb lookup --isbn 9783837165890
dnb search --title "Blendwerk*" --contributor "Butcher" --limit 5
dnb self-test
```

## Commands

| Command | Description |
|---------|-------------|
| `lookup --isbn <isbn>` | Look up a record by ISBN (10 or 13 digits, hyphens optional) |
| `lookup --id <dnbId>` | Look up a record by DNB record identifier (the `001` field) |
| `search --title <text>` | Search by title (CQL `TIT`); use trailing `*` for prefix-match |
| `search --contributor <text>` | Search by person in any contributor role — author, translator, illustrator, editor, narrator (CQL `PER`) |
| `search --year <yyyy>` | Search by year of publication (CQL `JHR`) |
| `search --series <text>` | Search by series (CQL `WOE`) |
| `search --any <text>` | Search any field (CQL `WOE`) |
| `search --limit N` | Cap results, 1–100 (default 20) |
| `search --page N` | Page number, 1-based (default 1) |
| `self-test` | Verify AOT binary integrity (no network required) |
| `changelog [--all]` | Print release notes from the bundled `CHANGELOG.md` |
| `--version` | Print version |
| `--help` | Show help |

**Global flags** (work on every command): `--pretty` (indent JSON), `--timeout <ms>` (HTTP timeout), `--verbose` (debug logging to stderr).

**Exit codes:**

| Code | Meaning |
|------|---------|
| `0` | Hit — `lookup` found a record, `search` returned ≥1 result |
| `1` | Generic / unexpected error |
| `2` | No results — DNB returned 200 with zero records |
| `3` | Bad input — malformed ISBN, missing required flag, value out of range |
| `4` | Network / transport error — timeout, DNS, connection refused |
| `5` | Upstream / DNB error — HTTP 5xx, SRU diagnostic, malformed XML |

stdout is always valid JSON. On error categories 1–5, `lookup` emits the literal `null`; `search` emits its envelope with empty `results`. Diagnostics go to stderr with a timestamp + level prefix.

## Configuration

There is no configuration file. The DNB SRU endpoint is fixed and anonymous. Behavior is controlled entirely by CLI flags and a single env var:

| Env var | Equivalent flag | Meaning |
|---|---|---|
| `DNB_TIMEOUT_MS` | `--timeout` | HTTP timeout in milliseconds (default 10000) |

## Logging

Errors and warnings go to stderr with a timestamp + level prefix:

```
2026-05-24T14:23:45.123Z ERROR DNB returned diagnostic: unsupported index 'XYZ'
2026-05-24T14:23:45.123Z WARN  no results
```

## License

MIT

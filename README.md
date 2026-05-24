# dnb-cli

A command-line interface for the Deutsche Nationalbibliothek (DNB) SRU catalogue. Built for agent-driven metadata fallback on German-language titles — pipe nothing in, get JSON out.

Native AOT binary. No runtime required. ~10 MB.

> **Note:** This tool was built using agentic software engineering (AI-assisted coding) and reviewed by a human. See the git history for details.

## Features

- **JSON-only output** — stdout is always valid JSON, errors go to stderr
- **Native AOT** — single self-contained binary, no .NET runtime needed
- **MARC21-XML parsing** — direct, faithful mapping of DNB records to flat JSON
- **Series + Bandnummer** — the killer feature for German manga that other providers miss
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
dnb lookup --isbn 9783753931104
dnb search --title "Naruto*" --author "Kishimoto" --limit 5
dnb self-test
```

## Commands

<!-- Commands table is filled in by Task 21 -->

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

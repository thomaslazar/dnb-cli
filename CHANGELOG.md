# Changelog

All notable changes to dnb-cli are documented in this file.

## v0.1.0 — 2026-06-01

Initial public release of dnb-cli — a Native AOT .NET 10 command-line tool
for the Deutsche Nationalbibliothek (DNB) SRU catalogue. Built for
agent-driven metadata lookup on German-language titles.

### Highlights

- Four commands: `lookup` (by ISBN or DNB-ID), `search` (combinable CQL flags), `self-test`, `changelog`.
- MARC21-XML parsing of DNB SRU responses into flat camelCase JSON: title block, contributors with relator-role codes, series + Bandnummer, language pairs, GND identifiers, raw keyword passthrough.
- JSON-only stdout with structured exit codes (0 hit / 2 no-results / 3 bad input / 4 network / 5 upstream); diagnostics on stderr.
- Native AOT publish for six platforms (linux-x64, linux-arm64, osx-x64, osx-arm64, win-x64, win-arm64), ~11 MB self-contained binary.
- Build-time codegen surfaces response-shape JSON samples in each command's `--help` output, matching real stdout key naming.

### Features

- feat: add ConsoleOutput helper with pretty-print support
- feat: add CqlBuilder for ISBN/ID/search queries
- feat: add DnbService HTTP layer with typed exceptions
- feat: add NLog setup, exit-code constants, and NLog test collection
- feat: add changelog command reading embedded CHANGELOG.md
- feat: add data models and source-generated JSON context
- feat: add install.sh and install.ps1 download scripts
- feat: add lookup command for ISBN and DNB-ID
- feat: add search command with combinable CQL flags
- feat: add self-test command with embedded MARC fixture
- feat: finalize Program.cs with global options and top-level error handler
- feat: generate response-shape samples for --help output
- feat: parse 100/700 contributors with GND id normalization
- feat: parse 490/830 series with case-insensitive dedup
- feat: parse 655 genres, 650 subjects, 653 keywords
- feat: parse SRU search envelope with totalResults and per-record iteration
- feat: parse core record fields (id, isbn, title, languages, publication, extent, description)
- feat: port HelpExtensions for custom help sections
- feat: scaffold DnbCli + DnbCli.Tests projects
- feat: scaffold MARC parser and detect SRU diagnostic responses

### Fixes

- fix: emit camelCase keys in --help Response shape samples
- fix: pace smoke-test queries to avoid DNB rate limiting
- fix: use legacy .sln format for parity with abs-cli and CI
- fix: use lowercase isbn= index for DNB SRU CQL queries

### Refactors

- refactor: rename --author flag to --contributor with explicit role list in help
- refactor: swap reference specimens to Jim Butcher and Ben Aaronovitch

### Tests, docs, CI, chore

- test: add smoke-test script against live DNB SRU endpoint
- docs: add release skill and dev-container/testing/releasing notes
- docs: add roadmap, design spec, and implementation plan
- docs: complete README Commands table and exit-code reference
- ci: add build matrix with unit tests, live smoke test, and release upload
- chore: add devcontainer with .NET 10 AOT toolchain, Claude Code, and statusline
- chore: bootstrap repository with baseline files

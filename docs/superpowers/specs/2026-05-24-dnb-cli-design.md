# dnb-cli — Design Spec (v1)

**Date:** 2026-05-24
**Status:** approved for implementation planning
**Companion docs:** [`ROADMAP.md`](../../ROADMAP.md)

## 1. Purpose

`dnb-cli` is a fallback metadata tool the user's agent harness invokes when the primary metadata path (Audible / Google Books, surfaced via `abs-cli metadata search`) fails or returns low-confidence matches for German-language titles — primarily **manga**.

The Deutsche Nationalbibliothek (DNB) catalogue is the authoritative, librarian-curated source for anything published in German, under CC0. `dnb-cli` exposes the DNB SRU endpoint as a small, JSON-only CLI shaped exactly like its sibling tool [`abs-cli`](https://github.com/thomaslazar/abs-cli) so the agent and the human maintainer can treat both tools identically.

### Why this exists

The agent's primary lookup fails on German books in specific, predictable ways:
- Wrong editions for manga volumes
- Missing series / Bandnummer
- Missing translators
- Wrong author when there are namesakes

DNB carries all of these fields, in MARC21 form, accurately curated.

### Non-goals (v1)

- Cover image lookup (no provider; agent's harness handles covers via other providers)
- Description / blurb data (DNB catalogues bibliography, not marketing copy — empty on virtually all manga records)
- Authentication / token management (DNB SRU is anonymous)
- Persistent cache (DNB is fast and free; agent doesn't re-query the same ISBN within a sweep)
- Config file at `~/.dnb-cli/` (no per-user state worth persisting)
- Raw MARC XML command (the `marcSource` URL in every record is curl-able for that need)
- Output formats other than JSON (`--pretty` only varies indentation)
- Retries on transient network errors (agent retries at its layer)
- CQL `or` / `not` operators in search (rare need; agent can call twice and merge)
- Automatic wildcard injection in search terms

## 2. Stack & runtime

- **.NET 10, C#, Native AOT** — single self-contained binary, ~10 MB, no runtime required
- **System.CommandLine** for argument parsing
- **`System.Net.Http.HttpClient`** for HTTP
- **`System.Xml.XmlReader`** for MARC21-XML parsing (no third-party MARC library — DNB's MARC21-xml is plain XML and we only consume ~10 fields)
- **`System.Text.Json` source-generated serializers** for AOT compatibility

Stack choice is dictated by [`project-mirror-abs-cli-conventions`](memory) — `dnb-cli` must shape identically to `abs-cli` so the agent calls both the same way and the maintainer works on both the same way.

## 3. CLI surface

```
dnb lookup --isbn <isbn>              # ISBN → record or null
dnb lookup --id <dnbId>               # DNB-ID → record or null
dnb search --title <text>             # combinable flags AND together
           --author <text>
           --year <yyyy>
           --series <text>
           --any <text>               # CQL WOE — search any field
           [--limit N (1-100, default 20)]
           [--page N (default 1)]

dnb self-test                         # AOT validation, no network
dnb changelog [--all]                 # print release notes from bundled CHANGELOG.md
dnb --version
dnb --help
```

**Global flags** (work on every command):

| Flag | Default | Meaning |
|---|---|---|
| `--pretty` | off | indent JSON; default is compact |
| `--timeout <ms>` | `10000` | HTTP timeout per request |
| `--verbose` | off | extra stderr logging |

**Environment variables:**

| Var | Equivalent flag |
|---|---|
| `DNB_TIMEOUT_MS` | `--timeout` |

**Validation rules:**

- `lookup`: exactly one of `--isbn` / `--id` required; both supplied → exit 3.
- `search`: at least one of `--title` / `--author` / `--year` / `--series` / `--any` required → else exit 3.
- ISBN format: accept 10 or 13 digits, hyphens optional, normalize to bare digits before query. Invalid → exit 3.
- `--limit`: 1–100; outside range → exit 3.

## 4. Output schema

### 4.1 Single record (used by `lookup` and as each entry of `search.results[]`)

```json
{
  "dnbId": "1356869467",
  "isbns": ["9783753931104"],
  "title": {
    "main": "YoRHa - Abstieg 11941",
    "subtitle": "Eine NieR:Automata Story",
    "partNumber": null,
    "partName": null,
    "uniform": "YoRHa Shinjuwan Koka Sakusen Kiroku 04",
    "statementOfResponsibility": "Taro Yoko, Megumu Soramichi"
  },
  "series": [
    { "name": "YoRHa - Abstieg 11941", "volume": "4" }
  ],
  "languages": {
    "publication": ["ger"],
    "original": ["jpn"]
  },
  "contributors": [
    { "name": "Yokoo, Tarō",       "role": "aut", "roleLabel": "Verfasser",  "gndId": "1253467463" },
    { "name": "Soramichi, Megumu", "role": "aut", "roleLabel": "Verfasser",  "gndId": "1176427385" },
    { "name": "Lange, Markus",     "role": "trl", "roleLabel": "Übersetzer", "gndId": null }
  ],
  "publication": {
    "place": "Hamburg",
    "publisher": "Altraverse",
    "date": "2025"
  },
  "edition": "1. Auflage",
  "extent": "228 Seiten",
  "description": null,
  "genres": ["Comic"],
  "subjects": [],
  "keywords": [
    "(Produktform)Electronic book text",
    "(Zielgruppe)ab 16 Jahre",
    "(BISAC Subject Heading)CGN004050",
    "Krieg",
    "Square Enix",
    "Maschinen",
    "Roboter",
    "Androiden",
    "NieR:Automata",
    "Videospiel",
    "Yoko Taro"
  ],
  "marcSource": "https://services.dnb.de/sru/dnb?version=1.1&operation=searchRetrieve&query=IDN%3D1356869467&recordSchema=MARC21-xml"
}
```

**Notes on the schema:**

- All fields are always present, even when their value is `null` / `[]`. The agent never needs `field in obj` checks.
- `description` stays in the schema as documentation of a possible-but-rare field. DNB may populate it on non-manga records (novels, academic works).
- `print` vs. `ebook` editions of the same volume are surfaced as separate records — never deduped. The agent picks. Format leaks via `extent` ("Online-Ressource") and via the `(Produktform)…` keyword.
- `keywords` includes the raw `(Prefix)Value` entries DNB embeds (e.g. `(Zielgruppe)`, `(Lesealter)`, `(VLB-WN)…`). The agent decides what to do with them; `dnb-cli` does not promote them to structured fields.

### 4.2 Search envelope (used by `search`)

```json
{
  "query": "TIT=YoRHa and TIT=Abstieg",
  "totalResults": 9,
  "returnedResults": 5,
  "page": 1,
  "limit": 5,
  "results": [ {…full record…}, {…} ]
}
```

- `query` is the CQL string actually sent to DNB (post-build, pre-encoding) — useful for debugging and for the agent to understand what was queried.
- `totalResults` is DNB's `<srw:numberOfRecords>` (total available, not just this page).
- `returnedResults` is the number of records in `results[]`.
- On zero hits: same shape, `results: []`, `totalResults: 0`. Exit 2.

### 4.3 lookup not-found shape

`lookup` emits the literal JSON `null` on stdout, exits 2. (Search emits an empty envelope; lookup is a singleton and `null` is the cleaner representation.)

## 5. MARC → JSON mapping

| JSON field | MARC source | Notes |
|---|---|---|
| `dnbId` | `001` (controlfield) | bare string |
| `isbns[]` | `020 $a` across all `020` fields | hyphens stripped; deduped; if only `$9` present, use that |
| `title.main` | `245 $a` | trailing ISBD punctuation trimmed: ` /`, ` :`, ` =`, ` ;`, ` ,` (MARC titles often end in these as field-separator marks) |
| `title.subtitle` | `245 $b` | nullable |
| `title.partNumber` | `245 $n` (joined `, ` if multiple) | nullable |
| `title.partName` | `245 $p` (joined `, ` if multiple) | nullable |
| `title.uniform` | `240 $a` | original-language title in romanization |
| `title.statementOfResponsibility` | `245 $c` | raw German prose |
| `series[].name` | `490 $a` and `830 $a` | de-duped on case-insensitive whitespace-collapsed `(name, volume)` tuple |
| `series[].volume` | `490 $v` and `830 $v` | raw string; may include padding ("05") |
| `languages.publication[]` | `041 $a` (all values) | ISO 639-2/B codes |
| `languages.original[]` | `041 $h` (all values) | ISO 639-2/B codes |
| `contributors[].name` | `100 $a` and `700 $a` | |
| `contributors[].role` | `$4` (relator code) | e.g. `aut`, `trl`, `edt` |
| `contributors[].roleLabel` | `$e` | German label, e.g. `"Verfasser"`, `"Übersetzer"` |
| `contributors[].gndId` | `$0` | regex out the bare number from `(DE-588)X`, `https://d-nb.info/gnd/X`, or `(DE-101)X` (first match) |
| `publication.place` | `264 $a` (ind2=1) | |
| `publication.publisher` | `264 $b` (ind2=1) | |
| `publication.date` | `264 $c` (ind2=1, fallback ind2=4 if missing) | raw string, no int parsing |
| `edition` | `250 $a` | |
| `extent` | `300 $a` | raw string (e.g. `"228 Seiten"`, `"Online-Ressource, 174 Seiten"`) |
| `description` | `520 $a` (+ `$b` joined with a space) | nullable |
| `genres[]` | `655 $a` (any `$2`) | |
| `subjects[]` | `650 $a` (any `$2`) | |
| `keywords[]` | `653 $a` | raw, including `(Prefix)Value` entries |
| `marcSource` | constructed URL | `https://services.dnb.de/sru/dnb?version=1.1&operation=searchRetrieve&query=IDN%3D<dnbId>&recordSchema=MARC21-xml` |

**Edge cases the parser must handle (observed in live DNB responses):**

- **Volume in `245 $n` vs. baked into `245 $a`** — older records use `$n` (`"3"`), newer ones bake the volume into the title string (`"YoRHa - Abstieg 11941 04"`). The parser surfaces `$n` as `title.partNumber` only; volumes baked into titles are not extracted. Volume is also separately reported in `series[].volume` when 490/830 are present.
- **`100 $0` returning three GND forms** — `[(DE-588)X, https://d-nb.info/gnd/X, (DE-101)X]`. Parser takes the first GND-588 match, falls back to GND-101, falls back to first match in any URI form, falls back to `null`.
- **Multiple `020` ISBNs** — one record can have both ISBN-13 and ISBN-10 as separate `020` fields. Parser strips hyphens, deduplicates, and returns all in `isbns[]`.
- **Multiple `264` fields** — publication (`ind2=1`) and copyright (`ind2=4`). Parser prefers `ind2=1`; for `date` specifically, falls back to `ind2=4` if `ind2=1` has no `$c`.
- **`041 $a` / `$h` with multiple values** — German translations of Japanese works typically have one of each; multi-language editions return arrays.
- **SRU `<diag:diagnostic>` in response** — DNB returns HTTP 200 with a diagnostic element on malformed queries. Parser detects this before record iteration and raises `DnbUpstreamException` → exit 5 with diagnostic message verbatim on stderr.

## 6. Error handling & exit codes

| Exit | Meaning | stdout | stderr | When |
|---|---|---|---|---|
| **0** | Hit | result JSON (record or envelope) | — | `lookup` found a record; `search` returned ≥1 result |
| **1** | Generic / unexpected | `null` | `<ts> ERROR <msg>` | catch-all — bug, panic, unknown exception |
| **2** | No results | `null` (lookup) or empty envelope (search) | `<ts> WARN no results` | DNB returned 200 with zero records |
| **3** | Bad input | `null` | `<ts> ERROR <msg>` | malformed ISBN, missing required flag, mutually-exclusive flags both set, no search flags supplied, `--limit` out of range |
| **4** | Network / transport | `null` | `<ts> ERROR <msg>` | timeout, DNS failure, connection refused, TLS error |
| **5** | Upstream / DNB error | `null` | `<ts> ERROR <msg>` | HTTP 5xx, SRU `<diag:diagnostic>` response, malformed/unparseable XML |

**stdout always emits valid JSON.** Even on error categories 1–5, stdout is the literal `null` (`search` envelope when applicable). The agent never has to handle empty stdout or unparseable stdout — exit code carries the semantic, stderr carries the message.

**stderr format:** `<ISO-timestamp> <LEVEL> <message>` — matching abs-cli exactly.

## 7. Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Commands (System.CommandLine)                              │
│    LookupCommand, SearchCommand, SelfTestCommand,           │
│    ChangelogCommand                                         │
└──────────────────────┬──────────────────────────────────────┘
                       │ parsed args → service call
┌──────────────────────▼──────────────────────────────────────┐
│  DnbService                                                 │
│    BuildCqlQuery(...) → HttpClient → raw XML response       │
└──────────────────────┬──────────────────────────────────────┘
                       │ XML stream
┌──────────────────────▼──────────────────────────────────────┐
│  MarcXmlParser     (System.Xml.XmlReader, no third-party)   │
│    DetectDiagnostic(stream) → throws DnbUpstreamException   │
│    ParseSearchResponse(stream) → SearchEnvelope             │
│    ParseRecord(xElement) → DnbRecord                        │
└──────────────────────┬──────────────────────────────────────┘
                       │ DnbRecord / SearchEnvelope
┌──────────────────────▼──────────────────────────────────────┐
│  JSON serializer (System.Text.Json source-generated)        │
└─────────────────────────────────────────────────────────────┘
```

No DI container. No plugins. No cache. No config file.

## 8. Repo layout

```
dnb-cli/
├─ .claude/
│  ├─ settings.json               (permissions allowlist + Frieren spinner verbs)
│  └─ skills/release/SKILL.md     (multi-step release workflow with human gates)
├─ .devcontainer/                 (parity with abs-cli: .NET 10 AOT toolchain, gh CLI, Claude Code, statusline)
│  ├─ devcontainer.json
│  ├─ Dockerfile
│  ├─ post-create.sh
│  └─ statusline.sh
├─ .editorconfig                  (from dotnet/runtime)
├─ .github/workflows/             (build matrix, format check, release)
├─ CHANGELOG.md                   (owned by release skill, never edited on feature branches)
├─ CLAUDE.md                      ("be brief", commit rules, smoke-test rules)
├─ Directory.Build.props
├─ DnbCli.sln
├─ LICENSE
├─ README.md                      (Commands table, install, examples)
├─ docs/
│  ├─ ROADMAP.md                  (deferred items)
│  ├─ dev-container.md            (devcontainer usage notes)
│  ├─ dnb-sru-reference.md        (CQL indices, MARC21-XML notes)
│  ├─ releasing.md                (release process — links to .claude/skills/release)
│  ├─ testing.md                  (unit / smoke / self-test layers)
│  └─ superpowers/                (specs/ + plans/)
├─ install.sh                     (POSIX installer — downloads binary from GH Releases)
├─ install.ps1                    (Windows installer — downloads binary from GH Releases)
├─ src/DnbCli/
│  ├─ Commands/                   (LookupCommand.cs, SearchCommand.cs, HelpExtensions.cs, ResponseExamples.g.cs, ...)
│  ├─ Dnb/                        (DnbService.cs, CqlBuilder.cs, DnbException.cs)
│  ├─ Marc/                       (MarcXmlParser.cs, MarcXmlConstants.cs)
│  ├─ Models/                     (DnbRecord, SearchEnvelope, Contributor, JsonContext, ...)
│  ├─ Output/                     (LogSetup.cs, ConsoleOutput.cs, ExitCodes.cs)
│  ├─ Resources/                  (selftest-record.xml — embedded for self-test)
│  ├─ Services/                   (ChangelogReader.cs)
│  └─ Program.cs
├─ tests/DnbCli.Tests/
│  ├─ NLogCollection.cs           (xUnit collection that disables parallel execution for globals-touching tests)
│  ├─ Marc/                       (parser tests against saved XML fixtures)
│  ├─ Dnb/                        (CqlBuilder + DnbService table tests)
│  ├─ Json/                       (serialization round-trip)
│  ├─ Output/                     (LogSetup + ConsoleOutput tests)
│  ├─ Commands/                   (Lookup/Search command tests with mocked HttpClient)
│  ├─ Services/                   (ChangelogReader tests)
│  └─ fixtures/                   (yorha.xml, nagatoro.xml, yorha-vol4-ebook.xml, diagnostic.xml, empty.xml, ...)
├─ temp/                          (gitignored: reference checkouts, e.g. `temp/abs-cli/`)
└─ tools/
   ├─ smoke-test.sh               (live DNB hits with known-good ISBNs)
   └─ GenerateResponseExamples/   (build-time codegen — emits Commands/ResponseExamples.g.cs from Models/*.cs)
      ├─ GenerateResponseExamples.csproj
      ├─ Program.cs
      └─ SampleJsonWalker.cs
```

**Differences from abs-cli's layout:**

- **No `docker/`** — DNB is a hosted public service; no local server needed for testing.
- **No `tools/` beyond `smoke-test.sh`** — abs-cli has additional release tooling here; `dnb-cli` doesn't need it in v1.
- **`.devcontainer/` slimmed** — drops `ffmpeg` from the Dockerfile and the `docker-outside-of-docker` feature; both are abs-cli-specific (audio fixtures, docker-compose stack).
- **`temp/`** mirrors abs-cli's `temp/audiobookshelf/` pattern — it holds the abs-cli reference checkout (`temp/abs-cli/`) used as the canonical source for repo shape and conventions. Gitignored.

## 9. Testing strategy

Three layers, same shape as abs-cli:

- **Unit (`dotnet test`)** — fast, no network:
  - `MarcXmlParser` against saved fixtures (the YoRHa + Nagatoro responses captured during design, plus a `<diag:diagnostic>` fixture)
  - `CqlBuilder` table tests covering all flag combinations
  - JSON serialization round-trip on the full schema
- **Smoke (`tools/smoke-test.sh`)** — live DNB hits:
  - Known-good ISBNs: `9783753931104` (YoRHa Bd. 4), `9783959561754` (Nagatoro Bd. 5)
  - Assert exit 0, non-empty `contributors`, expected series name
  - One known-not-found ISBN → assert exit 2
  - One malformed CQL → assert exit 5
  - Per the mirrored CLAUDE.md rule: smoke test must run and pass before any PR is marked "smoke test passed"
- **AOT validation (`dnb self-test`)** — in-process happy-path:
  - Bakes a small XML fixture as a resource
  - Exercises parse → serialize end-to-end
  - Verifies the AOT binary actually runs the pipeline (catches AOT-incompatible reflection paths early)
  - No network required

## 10. Distribution

**v1 ships:**

- GitHub Releases with binaries: `dnb-cli-{linux,osx,win}-{x64,arm64}{,.exe}`
- GitHub Actions: build matrix per platform, `dotnet format --verify-no-changes`, unit tests, smoke test on Linux against live DNB, release-on-tag
- `dotnet publish … /p:PublishAot=true` per platform
- `install.sh` (POSIX) and `install.ps1` (Windows) — modelled directly on the abs-cli installers; download the platform-matched binary from the latest GitHub Release, install to `~/.local/bin/dnb-cli` (or `$ABS_CLI_INSTALL_DIR` equivalent: `DNB_CLI_INSTALL_DIR`), support `DNB_CLI_VERSION` override

**Deferred to v2** (tracked in [`ROADMAP.md`](../../ROADMAP.md)):

- Homebrew tap (`brew tap thomaslazar/dnb-cli`)
- `.deb` package

Homebrew tap + .deb are setup work *outside* the repo itself and shouldn't gate the first usable release. Direct binary download via GitHub Releases plus the install scripts covers all six target platforms for v1.

## 11. Development conventions

Inherited from abs-cli verbatim — see [`project-mirror-abs-cli-conventions`](memory) memory:

- `CLAUDE.md` main rule: **"be brief"**
- **Conventional Commits**: `type: subject` (types `feat fix docs test ci refactor chore`); imperative, lowercase, no period, ~72 chars
- **NO `Co-Authored-By:`** lines, NO Claude attribution in commits/PRs/anywhere
- **Ask before committing.** Never auto-commit after edits.
- **After PR creation**: present URL as a clickable link
- **Pre-PR verification**: smoke test must pass against live DNB
- **Post-PR verification**: watch CI to green; PR is "done" at all-checks-green
- **README Commands table**: any PR adding/renaming/removing a CLI verb or user-visible flag MUST update the Commands table in the same change
- **`dotnet format`** enforced in CI; run before committing
- **No unnecessary blank lines** inside method bodies

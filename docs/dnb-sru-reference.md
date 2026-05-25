# DNB SRU — reference notes

## Endpoint

`https://services.dnb.de/sru/dnb`

Anonymous read access. No registration, no token, no rate-limit headers documented. Polite use: hard-coded User-Agent in `dnb-cli` identifies the tool.

## Query parameters

| Param | Value | Notes |
|---|---|---|
| `version` | `1.1` | SRU protocol version |
| `operation` | `searchRetrieve` | Only operation we use |
| `query` | CQL string, URL-encoded | See indices below |
| `recordSchema` | `MARC21-xml` | We always use MARC21-xml; never request other schemas |
| `maximumRecords` | 1–100 | DNB caps at 100 per request |
| `startRecord` | 1-based | Used for pagination (`(page - 1) * limit + 1`) |

## CQL indices used by dnb-cli

| Index | Maps to flag | Meaning |
|---|---|---|
| `isbn` | (used internally by `lookup --isbn`) | ISBN — index is lowercase per DNB SRU, others are uppercase |
| `IDN` | (used internally by `lookup --id`) | DNB record identifier (`001` field) |
| `TIT` | `--title` | Title |
| `PER` | `--contributor` | Person in any contributor role — author, translator, illustrator, editor, narrator |
| `JHR` | `--year` | Year of publication |
| `WOE` | `--series` and `--any` | Any word in any field |

Operator: `and` between flags (we do not emit `or` / `not` in v1).

Wildcards: `*` is the only supported truncation, suffix-only. Agent controls wildcarding via input (`--title "Foo*"`); we do not auto-inject.

## Diagnostic responses

DNB returns HTTP 200 even on malformed queries. Look for `<diag:diagnostic xmlns:diag="http://services.dnb.de/sru/dnb/diag">` in the response body. When present, parsing fails with `DnbUpstreamException` mapped to exit 5.

## MARC21-XML field cheat sheet

See `docs/superpowers/specs/2026-05-24-dnb-cli-design.md` §5 for the full mapping table. Key fields:

| Tag | Subfields | JSON path |
|---|---|---|
| `001` | (controlfield) | `dnbId` |
| `020` | `$a` | `isbns[]` (strip hyphens, dedupe) |
| `041` | `$a` (lang), `$h` (orig lang) | `languages.publication[]` / `languages.original[]` |
| `100` / `700` | `$a` name, `$4` relator, `$e` German label, `$0` GND ID | `contributors[]` |
| `240` | `$a` | `title.uniform` |
| `245` | `$a/$b/$n/$p/$c` | `title.main/subtitle/partNumber/partName/statementOfResponsibility` |
| `250` | `$a` | `edition` |
| `264` | `$a/$b/$c` (ind2=1 publication, fall back ind2=4 for date) | `publication.{place,publisher,date}` |
| `300` | `$a` | `extent` |
| `490` / `830` | `$a` series name, `$v` volume | `series[]` |
| `520` | `$a` (+ `$b`) | `description` |
| `650` | `$a` | `subjects[]` |
| `653` | `$a` | `keywords[]` (raw, including `(Prefix)Value` entries) |
| `655` | `$a` | `genres[]` |

## Known sample IDs (used in tests and smoke test)

| ISBN | DNB-ID | Title |
|---|---|---|
| `9783837165890` | `1314588753` | Jim Butcher, Die dunklen Fälle des Harry Dresden - Blendwerk, Bd. 15 (audiobook) |
| `9783423221870` | `1395994714` | Ben Aaronovitch, Die Meerjungfrauen von Aberdeen, Bd. 10 |

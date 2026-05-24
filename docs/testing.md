# Testing

Three layers:

## 1. Unit tests (`dotnet test`)

Located at `tests/DnbCli.Tests/`. Run with:

```bash
dotnet test tests/DnbCli.Tests/DnbCli.Tests.csproj
```

Covers:
- MARC parser against saved fixture XML
- CQL builder table tests
- JSON serialization round-trips
- Command handler assertions (via mocked `HttpClient`)
- NLog `LogSetup` invariants

Tests that mutate global NLog state are decorated with `[Collection("NLog")]` (see `tests/DnbCli.Tests/NLogCollection.cs`) so they run serially.

## 2. Smoke test (`tools/smoke-test.sh`)

Hits the live DNB SRU endpoint with known-good ISBNs:

- `9783753931104` — YoRHa Bd. 4 (e-book)
- `9783959561754` — Nagatoro Bd. 5
- `0000000000000` — known-not-found (asserts exit 2)

Run with:

```bash
bash tools/smoke-test.sh
```

It first builds a Release AOT binary if `$CLI` is unset.

**Required before any PR per CLAUDE.md.** Unit tests don't exercise HTTP, CQL escaping, or end-to-end shape; only the smoke test does.

## 3. AOT validation (`dnb self-test`)

Built into the binary itself. Exercises parse + serialize against a baked-in fixture, no network:

```bash
./publish/dnb-cli self-test
```

CI runs this on every platform build matrix entry so AOT-incompatible reflection paths are caught early.

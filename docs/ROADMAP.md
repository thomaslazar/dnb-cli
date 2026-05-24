# dnb-cli — Roadmap

Items intentionally deferred from v1 to keep the first release small. Each entry is one we discussed and chose *not* to build now, with the reason for deferral.

## Deferred from v1

### Distribution

- **Homebrew tap** (`brew tap thomaslazar/dnb-cli`) — setup work outside the repo itself; not needed for first usable release. v1 ships direct binary downloads via GitHub Releases plus the `install.sh` / `install.ps1` scripts.
- **`.deb` package** — same rationale; nice-to-have, not load-bearing for v1.

### Functionality

- **Raw MARC XML output command** (`dnb raw --id …`) — the `marcSource` URL in every record is curl-able if anyone needs raw MARC.
- **CQL `or` / `not` operators in search** — rare need; agent can call twice and merge.

### Future considerations (no commitment)

- Could add CQL operator support (`--title-or "A|B"`) if multi-term searches become common.

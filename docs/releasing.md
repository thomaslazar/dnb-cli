# Releasing dnb-cli

Releases are cut via the `/release` skill (`.claude/skills/release/SKILL.md`). The skill is a multi-step workflow with human gates at the version choice, release-notes review, PR merge, and final GitHub-release verification.

## High-level flow

1. **Preflight** — clean working tree, format check, unit tests, AOT self-test, smoke test.
2. **Branch + version bump** — create `release/v{version}`, bump `<Version>` in `src/DnbCli/DnbCli.csproj`, verify `dnb-cli --version` reports the new number.
3. **Release notes** — generate `release-notes.md` from conventional-commit log since last tag, prepend to `CHANGELOG.md` ([[changelog-owned-by-release]] applies).
4. **PR** — open `release/v{version}` → `main`, wait for CI green, human merges.
5. **Tag + GitHub Release** — `gh release create` with `--notes-file release-notes.md`.
6. **Release CI** — the matrix builds 6 platforms and uploads binaries to the release.
7. **Verify** — download one binary, run `self-test`.

## Why a skill, not a one-shot command

Each step has a gate where a human signs off (version number, release-notes content, PR merge, final binaries). The skill enforces the gates and the order. See the skill source for exact bash invocations per step.

## Deferred from v1

The skill currently skips Homebrew tap and .deb package steps (deferred — see `docs/ROADMAP.md`). When those land, extend the skill's Step 6 to watch the additional jobs.

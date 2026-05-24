# CLAUDE.md

## Main rule
be brief

## Git Conventions

- **Always ask the user before committing.** Do not commit automatically after making changes.
- **Conventional Commits** format required: `type: subject`
- Types: `feat`, `fix`, `docs`, `test`, `ci`, `refactor`, `chore`
- Subject line: imperative mood, lowercase, no period, max ~72 chars
- Body (optional): explain *why*, not *what*. Wrap at 72 chars.
- Do NOT include `Co-Authored-By:` lines in commit messages.
- Do NOT add "Generated with Claude Code" or similar attribution lines to PRs, commits, or any auto-generated content.
- After creating a pull request, always present the PR URL as a clickable link (plain URL on its own line or markdown link format) so the user can open it directly.

Examples:

    feat: add search command with combinable CQL flags
    fix: strip ISBD trailing punctuation from title.main
    docs: clarify GND ID normalization for $0 multi-form input
    test: add fixture for malformed CQL diagnostic response

## Pre-PR verification

- Run `tools/smoke-test.sh` against the live DNB SRU endpoint before opening any PR. Unit tests and `self-test` are not enough — regressions in HTTP, CQL escaping, or response parsing only surface against the live service.
- DNB is anonymous and free; no compose stack required. Default endpoint is `https://services.dnb.de/sru/dnb`.
- Only mark "smoke test passed" in a PR description after actually running it. Do not copy the checkbox forward unverified.

## Post-PR verification

- After `gh pr create`, watch CI until every check is in a terminal state. A PR is not done at "PR open" — it is done at "all required checks green." Surface the result back to the user without prompting.
- `gh pr checks <num>` for one-shot status; `gh run watch <run-id>` or a polling Monitor for long-running jobs.

## Code Formatting

- `.editorconfig` (from dotnet/runtime) enforces style. CI checks with `dotnet format --verify-no-changes`.
- Run `dotnet format DnbCli.sln` after writing or modifying C# files.
- If formatting check fails in CI, run the format command and commit the fix.
- **No unnecessary blank lines** inside method bodies: no blanks between consecutive `AddCommand`/`AddOption` calls, no blank before `return` after setup calls, no blanks between consecutive variable declarations of the same kind. Keep methods compact.

## Command implementation conventions

- **README Commands table.** Any PR that adds, renames, or removes a CLI verb, OR adds/removes a user-visible flag on an existing command, MUST update the Commands table in `README.md` in the same change.
- **stdout is always valid JSON.** Errors, no-results, and unhandled exceptions all emit the literal `null` on stdout (or an empty search envelope). The exit code carries the semantic. Stderr carries human-readable diagnostics.

## DNB Source Reference

- DNB SRU is documented in `docs/dnb-sru-reference.md` (CQL indices, MARC21 mapping cheat sheet, sample queries).
- The abs-cli sibling project is cloned to `temp/abs-cli/` (gitignored) and used as the canonical reference for repo shape, CLI conventions, and CI pipeline.

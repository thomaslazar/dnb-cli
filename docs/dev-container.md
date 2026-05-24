# Dev Container

dnb-cli ships a `.devcontainer/` so contributors can develop with an identical toolchain regardless of host OS. The Mac host doesn't need any .NET tooling — everything runs inside the container.

## What it bundles

- `mcr.microsoft.com/devcontainers/dotnet:10.0` base — .NET 10 SDK
- `clang` + `zlib1g-dev` for Native AOT compilation
- `python3` + `python3-pip` for `tools/smoke-test.sh` JSON assertions
- GitHub CLI feature (`gh`)
- Claude Code CLI (native installer)
- peon-ping (audio relay)
- superpowers plugin (brainstorming, planning, TDD, debugging, code review)
- Colour-coded statusline in Claude Code

## First-time setup

Open the dnb-cli folder in VS Code and run **"Dev Containers: Rebuild and Reopen in Container"**. First build is 3–6 minutes.

After rebuild, verify in the container terminal:
```bash
dotnet --list-sdks
which gh
which claude
```

## When to rebuild

Any change to `.devcontainer/Dockerfile`, `devcontainer.json`, or `post-create.sh` needs a manual rebuild. The rule of thumb: if the change affects what's installed in the container, rebuild. Comment-only edits don't need it.

See the [[devcontainer-rebuild-handoff]] memory for the strict rebuild protocol.

## Bind mounts

The host's `~/.claude/projects` and `~/.openpeon/packs` are bind-mounted so Claude Code sessions and peon-ping packs stay in sync across host and container.

# Spec-Kit and SpecKit Companion setup

Last Updated: 2026-11-17

What is installed, how the pieces fit together, how to use them day to day, and how to keep them
current. Agent-facing rules live in
`.github/instructions/workflow/spec-kit.instructions.md`; this page is for humans.

## What is installed

| Piece | Version | Where |
| --- | --- | --- |
| `specify` CLI (GitHub Spec Kit) | 1.4.1 | `uv` tool install, `specify.exe` on PATH |
| Copilot integration | agents + prompts layout | `.github/agents/`, `.github/prompts/` |
| SpecKit Companion (spec-kit extension) | 0.28.0 | `.specify/extensions/companion/` |
| SpecKit Companion (VS Code extension) | 0.39.0 | VS Code marketplace install |
| Idea Assessment (`assess`) | 1.0.2 | `.specify/extensions/assess/` |
| Bug Triage (`bug`) | 1.0.2 | `.specify/extensions/bug/` |
| Git Branching (`git`) | 1.0.5 | `.specify/extensions/git/` |
| GitHub Integration (`github`) | 1.0.5 | `.specify/extensions/github/` |
| Coding Agent Context (`agent-context`) | 1.0.3 | `.specify/extensions/agent-context/` |
| `constitution-sync` preset | 1.0.0 | `.specify/presets/constitution-sync/` |
| Workflows | `speckit`, `speckit-companion` | `.specify/workflows/` |

That is 43 commands and 30 hooks, all reachable from `/` in Copilot chat.

## What changed on 2026-11-17

The repo had been scaffolded by spec-kit 1.0.7 and never refreshed. `specify integration status`
reported 13 missing and 12 modified managed files, and two commands were broken outright:

- `/speckit.constitution` called `.specify/scripts/powershell/resolve-template.ps1`, which did not
  exist.
- `/speckit.tasks` called `.specify/scripts/powershell/setup-tasks.ps1`, which did not exist.

Refreshing the CLI to 1.4.1 and re-running the integration upgrade fixed both, brought the nine
core agents up to current content, restored the `.github/prompts/speckit.*.prompt.md` selectors, and
created `.vscode/settings.json`. `/speckit.taskstoissues` was removed from spec-kit core in 1.3.0;
it now ships as `speckit.github.taskstoissues` and is installed.

## Commands you will actually use

| Command | What it does |
| --- | --- |
| `/speckit.companion.specify` | Write `spec.md` and classify the change size |
| `/speckit.companion.plan` | Write `plan.md` (plus research and data model when the size warrants) |
| `/speckit.companion.tasks` | Write a dependency-ordered `tasks.md` |
| `/speckit.companion.implement` | Work through `tasks.md` and journal each task |
| `/speckit.converge` | Compare code against spec, plan, and tasks, then append what is left |
| `/speckit.companion.status` | Where the active spec stands and what comes next |
| `/speckit.companion.resume` | Continue from the last completed step |
| `/speckit.companion.doctor` | Read-only health check of the recorded run |
| `/speckit.companion.auto` | The whole pipeline with no pauses |
| `/speckit.analyze`, `/speckit.clarify`, `/speckit.checklist` | Consistency pass, clarifying questions, custom checklist |
| `/speckit.github.taskstoissues` | Turn `tasks.md` into GitHub issues |
| `/speckit.git.feature`, `/speckit.git.validate`, `/speckit.git.commit` | Branch and commit helpers |
| `/speckit.agent-context.update` | Refresh the managed block at the end of `.github/copilot-instructions.md` |
| `/speckit.assess.intake` … `.decide` | Gate an idea before it becomes a spec |
| `/speckit.bug.assess`, `.fix`, `.test` | Bug triage against the codebase |

Every command is also available as a skill: `/speckit-companion-plan` for
`/speckit.companion.plan`, and so on. The skill is a one-line wrapper that points at the agent, so
it can never drift from the real command.

## How a run is recorded

The Companion writes `specs/<NNN>-<slug>/.spec-context.json` — the canonical record of step,
status, decisions, task journal, verification, coverage, and history. Two mechanisms feed it:

- **Command hooks** in `.specify/extensions.yml`: the command templates run
  `speckit.companion.before-step` / `after-*` around each phase.
- **Runtime events** in `.github/hooks/speckit.json`: Copilot calls `.specify/events.py` on
  `userPromptSubmitted` and `agentStop`, so a step can be recorded with no model in the loop.

The VS Code sidebar, the Spec Editor, and the Companion canvas all read that file.

## The panel you see in VS Code

`.vscode/settings.json` now tells the Companion this project uses Copilot, not Claude
(`speckit.aiProvider: copilot`), points it at `specs` and defaults the board to the
`speckit-companion` workflow. Without that, the Companion dispatches commands in Claude format.

## Keeping it current

```powershell
specify self check
uv tool install specify-cli --force --from "git+https://github.com/github/spec-kit.git@v1.4.1"
specify integration upgrade copilot --integration-options="--commands"
'y' | specify extension add companion --from https://github.com/alfredoperez/speckit-companion/releases/download/companion-latest/companion.zip --force
specify extension update assess bug git github agent-context
pwsh -File tools\spec-kit\sync-command-skills.ps1 -RemoveStale
pwsh -File tools\spec-kit\verify-setup.ps1
```

`sync-command-skills.ps1` is the repository's own tool: it regenerates the 45 `.github/Skills`
wrappers from whatever agents are installed and can delete wrappers whose command has gone.

`verify-setup.ps1` is the other one: 20 checks covering the CLI version, the integration manifest,
the six extensions, the preset, the hook registration, the VS Code settings the Companion reads, the
skill wrappers, and a live read of the active spec. It exits non-zero when something is missing, so
it is safe to run in CI.

**One trap:** `specify integration upgrade copilot` regenerates `.vscode/settings.json` from its
template, which drops `speckit.aiProvider`, `speckit.specDirectories` and `speckit.defaultWorkflow`.
`verify-setup.ps1` catches that, and the values are in `.vscode/settings.json` in git to re-apply.

## Health check

```powershell
pwsh -File tools\spec-kit\verify-setup.ps1
python .specify/extensions/companion/scripts/doctor.py --all
```

Current state of this repo's four specs:

| Spec | Status |
| --- | --- |
| 001-heat-lot-management | completed |
| 002-receiving-analytics-two-tab | completed |
| 003-agent-toolkit-book | completed |
| 004-reconciliation-refactor | implementing — still open |

The doctor reports two historical issues on the completed specs: one `implement` step closed with
no recorded verification, and one closed by `ai` rather than by the extension hook. Both are
records of how those runs happened, not setup faults. New runs record verification through
`write-context.py --verified`, and the hooks stamp the step boundaries.

## Known, accepted quirks

- `specify integration status` always lists `.github/agents/speckit.constitution.agent.md` and its
  prompt as *modified*. That is the `constitution-sync` preset overriding the stock command on
  purpose. Any other modified file is real drift.
- `.github/prompts/speckit/speckit.module-specify.prompt.md` is a local command with no spec-kit
  counterpart; it stays in its own folder so upgrades never touch it.
- `.github/agents/speckit.companion.append.agent.md` is a local agent — the installed Companion
  0.28.0 does not provide an `append` command, so it has a skill wrapper but no prompt selector.
- `.github/copilot-agents.json` still points at `_bmad/module-agents/**` files that are not in the
  repository. That predates this work and is unrelated to spec-kit.

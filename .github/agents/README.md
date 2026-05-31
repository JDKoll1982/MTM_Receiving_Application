# Agent Library

This folder contains agent definitions used by repository workflows.

## Agent Groups

- Repository specialists: `mtm-module-improvement-auditor.agent.md`,
  `winui3-expert.agent.md`, `prompt-builder.agent.md`
- Speckit workflow agents: `speckit.*.agent.md`

## Usage Rules

- Keep agent files small and role-specific.
- Put repository rules in `.github/copilot-instructions.md`, not inside every agent file.
- If an agent needs detailed coding rules, point it to instruction files instead of duplicating
  the rules inline.
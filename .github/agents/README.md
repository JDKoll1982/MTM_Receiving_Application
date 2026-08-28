<!-- 
[DOC-META-START]
- File Name: README.md
- Description: Index of the .github/agents library: repository specialists, speckit agents, and usage rules.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 14-17: # Agent Library
  - Line 18-23: ## Agent Groups
  - Line 24-29: ## Usage Rules
- Critical Notes: Keep agents role-specific; put shared rules in copilot-instructions.md.
[DOC-META-END]
-->

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
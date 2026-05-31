# GitHub Copilot Prompts

This directory contains the active prompt library for the repository. Prompt files are now grouped
by workflow so they can be found and maintained without relying on one large flat folder.

## Categories

- `copilotforms/` — prompts paired with CopilotForms export workflows
- `design/` — diagram and mockup generation prompts
- `maintenance/` — repository maintenance, documentation, and release prompts
- `quality/` — review and optimization prompts
- `speckit/` — specification and planning workflow prompts
- `testing/` — test generation and verification prompts
- `workflow/` — general-purpose prompts such as MCP triage and noob mode

## Start Points

- Repository documentation work:
  `.github/prompts/maintenance/mtm-documentation.prepare.prompt.md`
- AI documentation reset or cleanup:
  `.github/prompts/maintenance/mtm-github-reset-documents.prompt.md`
- Test generation:
  `.github/prompts/testing/mtm-generate-all-tests.prompt.md`
- CopilotForms intake:
  `.github/prompts/copilotforms/`
- Spec-driven work:
  `.github/prompts/speckit/`

## Standards

- Prompt file authoring rules live in `.github/instructions/workflow/prompt.instructions.md`.
- Tool-selection rules live in `.github/instructions/tooling/mcp-tooling.instructions.md`.
- Repository-wide behavior rules live in `.github/copilot-instructions.md`.

## Maintenance Rules

- Keep prompt files focused on one workflow.
- Prefer replacing stale prompts over expanding them with layered exceptions.
- If a prompt points to instructions, those instruction paths must exist.
- When a prompt becomes historical or superseded, archive it instead of leaving it as active drift.
**Maintainer:** MTM Development Team

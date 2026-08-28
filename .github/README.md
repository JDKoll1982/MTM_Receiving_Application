<!-- 
[DOC-META-START]
- File Name: README.md
- Description: Map of active AI customization surfaces under .github (instructions, prompts, agents, archive) and maintenance rules.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 16-19: # AI Customization Map
  - Line 20-28: ## Active Surfaces
  - Line 29-42: ## Taxonomy
  - Line 43-49: ## Maintenance Rules
  - Line 50-53: ## Historical Snapshot
- Critical Notes: Keep archive content out of search and active instruction discovery.
[DOC-META-END]
-->

# AI Customization Map

This directory contains the active AI customization surfaces for the repository.

## Active Surfaces

- `copilot-instructions.md` — repository-wide rules and non-negotiable constraints
- `instructions/` — categorized custom instruction files
- `prompts/` — categorized prompt library
- `agents/` — repository-specific and imported agent definitions
- `copilot-agents.json` — Copilot agent registration metadata
- `archive/` — historical snapshots that should not be treated as active guidance

## Taxonomy

- `instructions/architecture/` — MVVM, DAO, CQRS, dialogs, converters, settings
- `instructions/database/` — SQL Server and MySQL query rules
- `instructions/documentation/` — spec, markdown, doc-maintenance, doc-update rules
- `instructions/languages/` — language-specific guidance
- `instructions/quality/` — review, performance, security, style, comments
- `instructions/testing/` — test-writing and validation strategy
- `instructions/tooling/` — MCP, WinApp, Serena, skill authoring
- `instructions/workflow/` — prompts, research, process, and workflow coordination
- `instructions/copilotforms/` — CopilotForms export-specific handlers
- `prompts/copilotforms/`, `prompts/testing/`, `prompts/maintenance/`, `prompts/speckit/`,
  `prompts/design/`, `prompts/quality/`, `prompts/workflow/` — active prompt categories

## Maintenance Rules

- Prefer one authoritative file per topic.
- If a file becomes historical, move it to the archive or replace it with a smaller active file.
- If you move prompt or instruction files, update path references and the relevant README.
- Keep archive content out of search and active instruction discovery.

## Historical Snapshot

The pre-rewrite `.github` snapshot for this cleanup is stored under
`.github/archive/003-ai-documentation-update/root-snapshot/`.
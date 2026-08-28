---
name: copilotforms-logging-refactor
description: 'Read a saved CopilotForms logging refactor export and adjust logging behavior accordingly.'
agent: agent
argument-hint: 'Link a logging refactor export from docs/CopilotForms/outputs/logging-refactor and note any compliance or privacy requirements'
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-logging-refactor.prompt.md
- Description: Read a saved CopilotForms logging refactor export and adjust logging behavior accordingly.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 8: # CopilotForms Logging Refactor
  - Line 12: ## Workflow
  - Line 21: ## Output Expectations
- Critical Notes: Preserve privacy and avoid logging sensitive values; keep severity choices deliberate.
[DOC-META-END]
-->

# CopilotForms Logging Refactor

Read the linked export from `docs/CopilotForms/outputs/logging-refactor/` and modify logging behavior accordingly.

## Workflow

1. Read the linked export completely.
2. Identify the target logging mode and desired outcome.
3. Inspect only the relevant services, ViewModels, or workflows listed in the export.
4. Adjust logging with minimal behavior impact.
5. Preserve privacy and avoid logging sensitive values.
6. Validate against the export’s logging verification steps.
7. Summarize what logging was added, removed, or restructured.

## Output Expectations

- Keep severity choices deliberate and consistent.
- Preserve useful failure signal.
- Explain tradeoffs when reducing or removing logs.

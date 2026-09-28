---
name: copilotforms-documentation-change
description: 'Read a saved CopilotForms documentation change export and create or update the requested documentation.'
agent: agent
argument-hint: 'Link a documentation change export from docs/CopilotForms/outputs/documentation-change and note the intended audience'
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-documentation-change.prompt.md
- Description: Read a saved CopilotForms documentation change export and create or update the requested documentation.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 8: # CopilotForms Documentation Change
  - Line 12: ## Workflow
  - Line 20: ## Output Expectations
- Critical Notes: Plain, accurate, task-focused documentation with no speculative statements.
[DOC-META-END]
-->

# CopilotForms Documentation Change

Read the linked export from `docs/CopilotForms/outputs/documentation-change/` and update documentation accordingly.

## Workflow

1. Read the linked export completely.
2. Use the stated audience to set the writing depth and tone.
3. Inspect the listed source files and current docs before editing.
4. Update only the documentation needed to satisfy the request.
5. Keep docs aligned with the codebase and architecture.
6. Summarize what changed and how the result was validated.

## Output Expectations

- Plain, accurate, task-focused documentation.
- No speculative statements about behavior that were not verified.

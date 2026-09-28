---
name: copilotforms-new-feature-request
description: "Read a saved CopilotForms new feature request export and implement the requested capability."
agent: agent
argument-hint: "Link a new feature request export from docs/CopilotForms/outputs/new-feature-request and add any extra constraints"
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-new-feature-request.prompt.md
- Description: Read a saved CopilotForms new feature request export and implement the requested capability.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 8: # CopilotForms New Feature Request
  - Line 12: ## Workflow
- Critical Notes: Implement the new capability with the smallest architecture-aligned changes.
[DOC-META-END]
-->

# CopilotForms New Feature Request

Read the linked export from `docs/CopilotForms/outputs/new-feature-request/` and use it as the structured input for implementation.

## Workflow

1. Read the linked export completely.
2. Separate the user problem, desired capability, and implementation constraints.
3. Inspect the smallest relevant existing workflow first using the listed files and feature metadata.
4. Implement the new capability with the smallest architecture-aligned changes that satisfy the request.
5. Validate using the acceptance criteria and verification steps from the export.
6. Summarize the delivered capability, code changes, and any residual gaps.

---
name: copilotforms-ui-change-logic-change
description: "Read a saved CopilotForms combined UI and logic export and implement both layers together."
agent: agent
argument-hint: "Link a combined UI/logic export from docs/CopilotForms/outputs/ui-change-logic-change and add any extra constraints"
---

# CopilotForms UI Change + Logic Change

Read the linked export from `docs/CopilotForms/outputs/ui-change-logic-change/` and use it as the structured input for implementation.

## Workflow

1. Read the linked export completely.
2. Separate the screen problem from the rule problem.
3. Inspect the smallest relevant UI and workflow path first using the listed files and feature metadata.
4. Implement the smallest reliable set of changes that keeps the UI and behavior aligned.
5. Validate using the acceptance criteria and verification needs from the export.
6. Summarize the UI changes, logic changes, and residual risk.

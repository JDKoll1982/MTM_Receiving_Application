---
name: copilotforms-feature-removal-request
description: "Read a saved CopilotForms feature removal export and remove or retire the requested capability safely."
agent: agent
argument-hint: "Link a feature removal export from docs/CopilotForms/outputs/feature-removal-request and add any extra cleanup constraints"
---

# CopilotForms Feature Removal Request

Read the linked export from `docs/CopilotForms/outputs/feature-removal-request/` and use it as the structured input for implementation.

## Workflow

1. Read the linked export completely.
2. Separate the removal reason, removal scope, and preserve rules.
3. Inspect the smallest relevant user path and supporting code path first using the listed files and feature metadata.
4. Remove or retire the capability with the smallest safe cleanup that satisfies the request.
5. Validate using the acceptance criteria and validation steps from the export.
6. Summarize what was removed, what was preserved, and any residual follow-up.
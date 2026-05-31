---
name: copilotforms-configuration-environment-issue
description: "Read a saved CopilotForms configuration or environment export and diagnose the reported setup issue."
agent: agent
argument-hint: "Link a configuration/environment export from docs/CopilotForms/outputs/configuration-environment-issue and add any current observations"
---

# CopilotForms Configuration / Environment Issue

Read the linked export from `docs/CopilotForms/outputs/configuration-environment-issue/` and use it as the structured input for diagnosis and repair.

## Workflow

1. Read the linked export completely.
2. Separate environment facts, config sources, and inferred causes.
3. Inspect the smallest relevant startup, configuration, or tooling path first.
4. Fix the smallest root cause that explains the mismatch.
5. Validate using the verification steps from the export.
6. Summarize the cause, fix, and any environment-specific follow-up.

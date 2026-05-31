---
name: copilotforms-debugging-logging
description: "Read a saved CopilotForms debugging plus logging export and diagnose the issue while improving observability."
agent: agent
argument-hint: "Link a debugging/logging export from docs/CopilotForms/outputs/debugging-logging and add any current observations"
---

# CopilotForms Debugging + Logging

Read the linked export from `docs/CopilotForms/outputs/debugging-logging/` and use it as the structured input for diagnosis and repair.

## Workflow

1. Read the linked export completely.
2. Separate observed symptoms from the logging gaps.
3. Inspect the smallest relevant code path first using the listed files and repro steps.
4. Fix the root cause and improve logging only where it materially helps diagnosis or support.
5. Validate using the verification steps from the export.
6. Summarize root cause, logging changes, and any residual risk.

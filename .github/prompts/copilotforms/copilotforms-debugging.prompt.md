---
name: copilotforms-debugging
description: 'Read a saved CopilotForms debugging export and diagnose or fix the reported issue.'
agent: agent
argument-hint: 'Link a debugging export from docs/CopilotForms/outputs/debugging and add any current observations'
---

# CopilotForms Debugging

Read the linked export from `docs/CopilotForms/outputs/debugging/` and use it as the structured input for diagnosis and repair.

## Workflow

1. Read the linked export completely.
2. Separate observed facts from suspected causes.
3. Inspect the smallest relevant code path first using the listed files, feature metadata, and repro steps.
4. Identify the root cause that best explains the reported behavior.
5. Implement the smallest reliable fix.
6. Validate using the verification steps from the export.
7. Summarize root cause, code changes, and any residual risk.

## Output Expectations

- Prefer root-cause fixes over broad defensive edits.
- Preserve unrelated behavior.
- Call out any missing evidence that limits confidence.

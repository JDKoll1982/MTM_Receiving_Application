---
name: copilotforms-naming-consistency-cleanup
description: "Read a saved CopilotForms naming cleanup export and align names without changing intended behavior."
agent: agent
argument-hint: "Link a naming cleanup export from docs/CopilotForms/outputs/naming-consistency-cleanup and add any extra rename constraints"
---

# CopilotForms Naming / Consistency Cleanup

Read the linked export from `docs/CopilotForms/outputs/naming-consistency-cleanup/` and use it as the structured input for semantic cleanup.

## Workflow

1. Read the linked export completely.
2. Confirm which names are misleading today and what they should communicate instead.
3. Inspect the smallest relevant code path first using the listed artifacts and files.
4. Apply safe, semantics-aware cleanup while preserving behavior.
5. Validate using the verification steps from the export.
6. Summarize the rename scope, edits made, and any residual consistency gaps.

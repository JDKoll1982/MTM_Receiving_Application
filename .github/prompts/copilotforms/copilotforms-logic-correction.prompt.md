---
name: copilotforms-logic-correction
description: 'Read a saved CopilotForms logic correction export and align the feature behavior with the intended rules.'
agent: agent
argument-hint: 'Link a logic correction export from docs/CopilotForms/outputs/logic-correction and note any hard constraints'
---

# CopilotForms Logic Correction

Read the linked export from `docs/CopilotForms/outputs/logic-correction/` and correct the feature behavior so it matches the intended workflow.

## Workflow

1. Read the linked export completely.
2. Compare intended behavior, actual behavior, and desired rule set.
3. Inspect the minimum set of files needed to understand the current logic.
4. Update logic to match the exported rules while preserving architecture boundaries.
5. Validate against the exported test cases and regression constraints.
6. Summarize what changed in behavior and what remained intentionally unchanged.

## Output Expectations

- Treat the exported intent as authoritative unless it conflicts with explicit codebase rules.
- Avoid unrelated refactors.
- Explain any unresolved ambiguity.

---
name: copilotforms-improvement-refactor
description: 'Read a saved CopilotForms improvement refactor export and perform the requested maintainability refactor.'
agent: agent
argument-hint: 'Link an improvement refactor export from docs/CopilotForms/outputs/improvement-refactor and describe any non-negotiable boundaries'
---

# CopilotForms Improvement Refactor

Read the linked export from `docs/CopilotForms/outputs/improvement-refactor/` and refactor the targeted code for maintainability or structure improvements.

## Workflow

1. Read the linked export completely.
2. Confirm the requested scope and risk level.
3. Inspect the smallest set of types and call paths needed to understand the current structure.
4. Refactor only within the requested scope.
5. Preserve behavior unless the export explicitly allows behavior changes.
6. Validate against the exported success criteria and listed tests.
7. Summarize the refactor shape, behavior preservation, and follow-up debt if any remains.

## Output Expectations

- Keep the refactor coherent and minimal.
- Respect repo architecture and naming patterns.
- Do not expand the scope without explaining why.

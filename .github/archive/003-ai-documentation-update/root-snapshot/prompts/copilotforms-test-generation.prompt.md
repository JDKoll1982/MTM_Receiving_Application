---
name: copilotforms-test-generation
description: 'Read a saved CopilotForms test generation export and add the requested tests.'
agent: agent
argument-hint: 'Link a test generation export from docs/CopilotForms/outputs/test-generation and note any framework constraints'
---

# CopilotForms Test Generation

Read the linked export from `docs/CopilotForms/outputs/test-generation/` and add the requested tests.

## Workflow

1. Read the linked export completely.
2. Confirm the requested test type and target scope.
3. Inspect only the code needed to understand the target behavior.
4. Add tests that cover the exported scenarios.
5. Keep mocks and test structure aligned with repo conventions.
6. Summarize what was covered, what was intentionally skipped, and any remaining gaps.

## Output Expectations

- Tests first, no unrelated production refactors unless necessary for testability.
- Preserve naming and assertion patterns already used by the repo.

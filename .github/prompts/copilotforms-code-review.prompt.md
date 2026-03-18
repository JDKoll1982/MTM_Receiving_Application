---
name: copilotforms-code-review
description: 'Read a saved CopilotForms code review export and perform a focused review.'
agent: ask
argument-hint: 'Link a code review export from docs/CopilotForms/outputs/code-review and note any priority concerns'
---

# CopilotForms Code Review

Read the linked export from `docs/CopilotForms/outputs/code-review/` and perform a focused code review.

## Workflow

1. Read the linked export completely.
2. Use the scoped files or modules to limit review spread.
3. Prioritize findings by severity and risk.
4. Focus on correctness, regressions, architecture violations, security issues, and missing tests.
5. Present findings first with file references.
6. Keep summary secondary.

## Output Expectations

- Findings first.
- Ordered by severity.
- Include testing gaps and residual risks.

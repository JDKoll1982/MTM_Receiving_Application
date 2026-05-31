---
description: 'Interpret CopilotForms code review exports linked from docs/CopilotForms/outputs/code-review and use them to drive risk-focused reviews.'
applyTo: 'docs/CopilotForms/outputs/code-review/**/*.{md,json}'
---

# CopilotForms Code Review Exports

When a linked file from `docs/CopilotForms/outputs/code-review/` is present:

- Treat the export as a structured review brief.
- Prioritize findings over summaries.
- Focus on defects, regressions, architecture violations, security risks, data risks, and testing gaps.
- Use the requested scope and concerns to keep the review targeted.
- If no findings are discovered, state that explicitly and note any residual risk or untested areas.

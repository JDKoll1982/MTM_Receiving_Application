---
description: 'Interpret CopilotForms code review exports linked from docs/CopilotForms/outputs/code-review and use them to drive risk-focused reviews.'
applyTo: 'docs/CopilotForms/outputs/code-review/**/*.{md,json}'
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-code-review.instructions.md
- Description: Interpret CopilotForms code review exports linked from docs/CopilotForms/outputs/code-review and use them to drive risk-focused reviews.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-14: # CopilotForms Code Review Exports
- Critical Notes: Treat the export as a structured review brief and prioritize findings over summaries.
[DOC-META-END]
-->

# CopilotForms Code Review Exports

When a linked file from `docs/CopilotForms/outputs/code-review/` is present:

- Treat the export as a structured review brief.
- Prioritize findings over summaries.
- Focus on defects, regressions, architecture violations, security risks, data risks, and testing gaps.
- Use the requested scope and concerns to keep the review targeted.
- If no findings are discovered, state that explicitly and note any residual risk or untested areas.

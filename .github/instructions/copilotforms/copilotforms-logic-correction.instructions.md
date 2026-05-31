---
description: 'Interpret CopilotForms logic correction exports linked from docs/CopilotForms/outputs/logic-correction and use them to align feature behavior with user intent.'
applyTo: 'docs/CopilotForms/outputs/logic-correction/**/*.{md,json}'
---

# CopilotForms Logic Correction Exports

When a linked file from `docs/CopilotForms/outputs/logic-correction/` is present:

- Treat the export as the source of truth for intended behavior.
- Compare current code behavior against the exported intent before editing.
- Preserve documented architecture boundaries while changing the logic.
- Use the desired rule set and test cases in the export as the implementation target.
- Avoid broad rewrites when a narrower logic fix can satisfy the intended workflow.
- Call out any ambiguity between business intent and current data contracts before making speculative changes.

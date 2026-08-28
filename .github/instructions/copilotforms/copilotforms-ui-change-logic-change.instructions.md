---
description: "Interpret CopilotForms combined UI and logic exports linked from docs/CopilotForms/outputs/ui-change-logic-change and use them to keep both layers aligned."
applyTo: "docs/CopilotForms/outputs/ui-change-logic-change/**/*.{md,json}"
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-ui-change-logic-change.instructions.md
- Description: Interpret CopilotForms combined UI and logic exports linked from docs/CopilotForms/outputs/ui-change-logic-change and use them to keep both layers aligned.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-15: # CopilotForms UI Change + Logic Change Exports
- Critical Notes: Treat as one combined request and keep the UI and underlying logic aligned in the final implementation.
[DOC-META-END]
-->

# CopilotForms UI Change + Logic Change Exports

When a linked file from `docs/CopilotForms/outputs/ui-change-logic-change/` is present:

- Treat the export as a structured combined request, not as two unrelated tasks.
- Distinguish clearly between requested UI changes and requested workflow or rule changes.
- Use the listed files and feature metadata to inspect the smallest end-to-end path first.
- Keep the UI and the underlying logic aligned in the final implementation.
- Respect architecture boundaries while implementing both sides of the request.
- Use the export's acceptance criteria and verification needs as the minimum validation bar.

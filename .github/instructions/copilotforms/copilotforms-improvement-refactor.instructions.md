---
description: 'Interpret CopilotForms improvement refactor exports linked from docs/CopilotForms/outputs/improvement-refactor and use them to drive scoped maintainability refactors.'
applyTo: 'docs/CopilotForms/outputs/improvement-refactor/**/*.{md,json}'
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-improvement-refactor.instructions.md
- Description: Interpret CopilotForms improvement refactor exports linked from docs/CopilotForms/outputs/improvement-refactor and use them to drive scoped maintainability refactors.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-15: # CopilotForms Improvement Refactor Exports
- Critical Notes: Preserve behavior unless explicitly requested; respect service boundaries, DAO rules, and MVVM separation.
[DOC-META-END]
-->

# CopilotForms Improvement Refactor Exports

When a linked file from `docs/CopilotForms/outputs/improvement-refactor/` is present:

- Treat the export as a scoped refactor brief.
- Focus on maintainability, clarity, structure, duplication reduction, or performance only within the stated scope.
- Preserve behavior unless the export explicitly requests a behavioral change.
- Respect repo architecture rules, especially service boundaries, DAO rules, and MVVM separation.
- Use the success criteria and listed validation items as the completion bar.
- Prefer the smallest coherent refactor that resolves the stated pain points.

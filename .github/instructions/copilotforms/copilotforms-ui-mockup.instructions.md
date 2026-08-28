---
description: 'Interpret CopilotForms UI mockup exports linked from docs/CopilotForms/outputs/ui-mockup and use them to generate visual mockup artifacts intentionally.'
applyTo: 'docs/CopilotForms/outputs/ui-mockup/**/*.{md,json}'
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-ui-mockup.instructions.md
- Description: Interpret CopilotForms UI mockup exports linked from docs/CopilotForms/outputs/ui-mockup and use them to generate visual mockup artifacts intentionally.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-14: # CopilotForms UI Mockup Exports
- Critical Notes: Do not silently convert a mockup request into implementation work unless the user explicitly asks for code.
[DOC-META-END]
-->

# CopilotForms UI Mockup Exports

When a linked file from `docs/CopilotForms/outputs/ui-mockup/` is present:

- Treat the export as the source brief for a visual mockup.
- Prioritize screen purpose, user actions, and visual hierarchy over implementation detail.
- Keep the mockup aligned with WinUI desktop expectations unless the export says otherwise.
- Use the listed sections, fields, and constraints as hard guidance for the mockup artifact.
- Do not silently convert a mockup request into implementation work unless the user explicitly asks for code.

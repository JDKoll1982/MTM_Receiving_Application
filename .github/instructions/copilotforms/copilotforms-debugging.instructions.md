---
description: 'Interpret CopilotForms debugging exports linked from docs/CopilotForms/outputs/debugging and use them as structured bug reports for diagnosis and fixes.'
applyTo: 'docs/CopilotForms/outputs/debugging/**/*.{md,json}'
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-debugging.instructions.md
- Description: Interpret CopilotForms debugging exports linked from docs/CopilotForms/outputs/debugging and use them as structured bug reports for diagnosis and fixes.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-16: # CopilotForms Debugging Exports
- Critical Notes: Fix the smallest root cause that explains the repro; state what is missing if evidence is incomplete.
[DOC-META-END]
-->

# CopilotForms Debugging Exports

When a linked file from `docs/CopilotForms/outputs/debugging/` is present:

- Treat the export as a structured bug report.
- Prioritize exact observed behavior, repro steps, and evidence over assumptions.
- Use `featureId` and any listed files to narrow investigation before editing.
- Distinguish clearly between observed facts, suspected causes, and inferred root cause.
- Fix the smallest root cause that explains the repro instead of layering defensive patches.
- Use the export’s verification steps as the minimum validation bar.
- If the evidence is incomplete, state what is missing rather than inventing a cause.

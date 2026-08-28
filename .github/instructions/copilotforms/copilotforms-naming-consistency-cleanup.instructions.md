---
description: "Interpret CopilotForms naming cleanup exports linked from docs/CopilotForms/outputs/naming-consistency-cleanup and use them for safe semantic cleanup."
applyTo: "docs/CopilotForms/outputs/naming-consistency-cleanup/**/*.{md,json}"
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-naming-consistency-cleanup.instructions.md
- Description: Interpret CopilotForms naming cleanup exports linked from docs/CopilotForms/outputs/naming-consistency-cleanup and use them for safe semantic cleanup.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-15: # CopilotForms Naming / Consistency Cleanup Exports
- Critical Notes: Keep runtime behavior unchanged unless the export explicitly says otherwise; prefer semantics-aware renames.
[DOC-META-END]
-->

# CopilotForms Naming / Consistency Cleanup Exports

When a linked file from `docs/CopilotForms/outputs/naming-consistency-cleanup/` is present:

- Treat the export as a structured naming-alignment request.
- Prioritize the stated target terminology and preserve-behavior constraints.
- Use the listed artifacts and files to find the narrowest safe rename scope.
- Prefer semantics-aware rename operations where available.
- Keep runtime behavior unchanged unless the export explicitly says otherwise.
- Use the export's verification steps as the minimum validation bar.

---
description: "Interpret CopilotForms naming cleanup exports linked from docs/CopilotForms/outputs/naming-consistency-cleanup and use them for safe semantic cleanup."
applyTo: "docs/CopilotForms/outputs/naming-consistency-cleanup/**/*.{md,json}"
---

# CopilotForms Naming / Consistency Cleanup Exports

When a linked file from `docs/CopilotForms/outputs/naming-consistency-cleanup/` is present:

- Treat the export as a structured naming-alignment request.
- Prioritize the stated target terminology and preserve-behavior constraints.
- Use the listed artifacts and files to find the narrowest safe rename scope.
- Prefer semantics-aware rename operations where available.
- Keep runtime behavior unchanged unless the export explicitly says otherwise.
- Use the export's verification steps as the minimum validation bar.

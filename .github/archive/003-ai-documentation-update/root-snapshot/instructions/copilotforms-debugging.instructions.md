---
description: 'Interpret CopilotForms debugging exports linked from docs/CopilotForms/outputs/debugging and use them as structured bug reports for diagnosis and fixes.'
applyTo: 'docs/CopilotForms/outputs/debugging/**/*.{md,json}'
---

# CopilotForms Debugging Exports

When a linked file from `docs/CopilotForms/outputs/debugging/` is present:

- Treat the export as a structured bug report.
- Prioritize exact observed behavior, repro steps, and evidence over assumptions.
- Use `featureId` and any listed files to narrow investigation before editing.
- Distinguish clearly between observed facts, suspected causes, and inferred root cause.
- Fix the smallest root cause that explains the repro instead of layering defensive patches.
- Use the export’s verification steps as the minimum validation bar.
- If the evidence is incomplete, state what is missing rather than inventing a cause.

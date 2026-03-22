---
description: "Interpret CopilotForms configuration or environment exports linked from docs/CopilotForms/outputs/configuration-environment-issue and use them for safe diagnosis."
applyTo: "docs/CopilotForms/outputs/configuration-environment-issue/**/*.{md,json}"
---

# CopilotForms Configuration / Environment Issue Exports

When a linked file from `docs/CopilotForms/outputs/configuration-environment-issue/` is present:

- Treat the export as a structured environment or configuration report.
- Separate confirmed environment facts from inferred causes.
- Use listed config sources, tasks, files, and repro steps to narrow investigation before editing.
- Avoid exposing or hardcoding secrets while repairing the issue.
- Fix the smallest root cause that explains the mismatch.
- Use the export's verification steps as the minimum validation bar.

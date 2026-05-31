---
description: "Interpret CopilotForms performance issue exports linked from docs/CopilotForms/outputs/performance-issue-optimization and use them for targeted diagnosis and optimization."
applyTo: "docs/CopilotForms/outputs/performance-issue-optimization/**/*.{md,json}"
---

# CopilotForms Performance Issue / Optimization Exports

When a linked file from `docs/CopilotForms/outputs/performance-issue-optimization/` is present:

- Treat the export as a structured performance report.
- Distinguish observed impact and measurements from guesses about root cause.
- Use the listed files, hotspots, and context to narrow investigation before editing.
- Prefer root-cause improvements over cosmetic micro-optimizations.
- Preserve functional behavior unless the export explicitly requests behavior changes.
- Use the export's verification steps or measurements as the minimum validation bar.

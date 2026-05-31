---
description: 'Interpret CopilotForms logging refactor exports linked from docs/CopilotForms/outputs/logging-refactor and use them to adjust logging safely and intentionally.'
applyTo: 'docs/CopilotForms/outputs/logging-refactor/**/*.{md,json}'
---

# CopilotForms Logging Refactor Exports

When a linked file from `docs/CopilotForms/outputs/logging-refactor/` is present:

- Treat the export as a logging policy brief for the targeted feature.
- Change only the logging behavior requested: add, reduce, remove, or restructure logging.
- Preserve security and privacy by never introducing sensitive data into logs.
- Keep severity usage consistent with the exported rules and the repo’s existing logging conventions.
- If removing logs, keep enough signal to preserve diagnosability for failures and key state transitions.
- Validate against the export’s logging-specific verification steps.

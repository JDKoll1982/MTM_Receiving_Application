---
description: "Interpret CopilotForms debugging plus logging exports linked from docs/CopilotForms/outputs/debugging-logging and use them for diagnosis with intentional observability changes."
applyTo: "docs/CopilotForms/outputs/debugging-logging/**/*.{md,json}"
---

# CopilotForms Debugging + Logging Exports

When a linked file from `docs/CopilotForms/outputs/debugging-logging/` is present:

- Treat the export as a structured bug report plus observability request.
- Prioritize exact observed behavior, repro steps, and evidence over assumptions.
- Distinguish root-cause fixes from logging improvements and avoid hiding the real defect with logs alone.
- Improve logging only where it materially helps diagnosis, support, or auditability.
- Respect explicit do-not-log or sensitive data constraints.
- Use the export's verification steps as the minimum validation bar.

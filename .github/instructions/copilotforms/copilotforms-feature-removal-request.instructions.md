---
description: 'Interpret CopilotForms feature removal exports linked from docs/CopilotForms/outputs/feature-removal-request and use them to retire capability safely.'
applyTo: 'docs/CopilotForms/outputs/feature-removal-request/**/*.{md,json}'
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-feature-removal-request.instructions.md
- Description: Interpret CopilotForms feature removal exports linked from docs/CopilotForms/outputs/feature-removal-request and use them to retire capability safely.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-15: # CopilotForms Feature Removal Request Exports
- Critical Notes: Remove or retire only what the export places in scope; preserve data integrity and architecture boundaries.
[DOC-META-END]
-->

# CopilotForms Feature Removal Request Exports

When a linked file from `docs/CopilotForms/outputs/feature-removal-request/` is present:

- Treat the export as the source of truth for the requested removal or retirement.
- Prioritize the removal reason, preserve rules, dependency risks, and validation expectations over guessed cleanup ideas.
- Use the listed files and feature context to anchor the cleanup in the nearest real workflow.
- Remove or retire only what the export places in scope.
- Preserve nearby workflows, data integrity, and architecture boundaries while cleaning up references.
- Use the export's acceptance criteria and validation plan as the minimum validation bar.
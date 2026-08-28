---
description: "Interpret CopilotForms new feature request exports linked from docs/CopilotForms/outputs/new-feature-request and use them to implement scoped new capability safely."
applyTo: "docs/CopilotForms/outputs/new-feature-request/**/*.{md,json}"
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-new-feature-request.instructions.md
- Description: Interpret CopilotForms new feature request exports linked from docs/CopilotForms/outputs/new-feature-request and use them to implement scoped new capability safely.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-15: # CopilotForms New Feature Request Exports
- Critical Notes: Prioritize the user problem and explicit scope over guessed implementation ideas; respect out-of-scope items.
[DOC-META-END]
-->

# CopilotForms New Feature Request Exports

When a linked file from `docs/CopilotForms/outputs/new-feature-request/` is present:

- Treat the export as the source of truth for the requested capability.
- Prioritize the user problem, desired capability, and explicit scope over guessed implementation ideas.
- Use the listed files and feature context to anchor the new work in the nearest existing workflow.
- Respect stated architecture boundaries and out-of-scope items.
- Prefer the smallest cohesive implementation that satisfies the acceptance criteria.
- Use the export's verification steps as the minimum validation bar.

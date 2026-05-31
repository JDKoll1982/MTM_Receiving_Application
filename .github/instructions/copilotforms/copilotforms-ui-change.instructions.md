---
description: 'Interpret CopilotForms UI change exports linked from docs/CopilotForms/outputs/ui-change and apply the requested UI updates safely in this repo.'
applyTo: 'docs/CopilotForms/outputs/ui-change/**/*.{md,json}'
---

# CopilotForms UI Change Exports

When a linked file from `docs/CopilotForms/outputs/ui-change/` is present:

- Treat the export as the user’s current UI intent.
- Read the Markdown narrative first and the JSON payload second.
- Use `featureId` to consult `docs/CopilotForms/data/copilot-forms.config.json` for module and file hints.
- Preserve repo architecture: `x:Bind`, MVVM separation, ViewModel to Service to DAO flow, and no business logic in code-behind.
- Keep the change scoped to UI structure, interaction, copy, and accessibility unless the export explicitly requests logic changes.
- Use the acceptance criteria in the export as the primary validation checklist.
- If the export conflicts with architecture rules or current code constraints, keep the architecture intact and explain the gap clearly.

---
name: copilotforms-ui-change
description: 'Read a saved CopilotForms UI change export and implement the requested UI changes.'
agent: agent
argument-hint: 'Link a UI change export from docs/CopilotForms/outputs/ui-change and describe any extra constraints'
---

# CopilotForms UI Change

Read the linked export from `docs/CopilotForms/outputs/ui-change/` and implement the requested UI changes.

## Workflow

1. Read the linked export completely.
2. Read `docs/CopilotForms/data/copilot-forms.config.json` if a `featureId` is present.
3. Inspect only the files needed to understand the targeted screen or controls.
4. Implement the UI changes while preserving MVVM boundaries and `x:Bind` usage.
5. Keep business logic unchanged unless the export explicitly asks for logic changes.
6. Validate the result against the export’s acceptance criteria.
7. Summarize what changed, what was preserved, and any gaps between the request and the codebase.

## Output Expectations

- Make the code changes directly when feasible.
- Keep changes minimal and architecture-safe.
- Report any unclear or conflicting parts of the request.

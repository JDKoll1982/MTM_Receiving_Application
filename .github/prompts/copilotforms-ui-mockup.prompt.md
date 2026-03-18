---
name: copilotforms-ui-mockup
description: 'Read a saved CopilotForms UI mockup export and generate a mockup artifact from it.'
agent: ask
argument-hint: 'Link a UI mockup export from docs/CopilotForms/outputs/ui-mockup and note any desired artifact format'
---

# CopilotForms UI Mockup

Read the linked export from `docs/CopilotForms/outputs/ui-mockup/` and generate a visual mockup artifact from it.

## Workflow

1. Read the linked export completely.
2. Use the purpose, actions, sections, and field list as the content source.
3. Prefer layout clarity and hierarchy over decorative output.
4. Keep the mockup aligned with desktop WinUI expectations unless the export says otherwise.
5. Summarize any assumptions needed to complete the mockup.

## Output Expectations

- Produce the requested mockup artifact only.
- Do not convert the request into implementation code unless explicitly asked.

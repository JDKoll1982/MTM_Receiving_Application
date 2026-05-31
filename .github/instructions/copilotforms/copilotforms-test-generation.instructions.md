---
description: 'Interpret CopilotForms test generation exports linked from docs/CopilotForms/outputs/test-generation and use them to create focused test coverage.'
applyTo: 'docs/CopilotForms/outputs/test-generation/**/*.{md,json}'
---

# CopilotForms Test Generation Exports

When a linked file from `docs/CopilotForms/outputs/test-generation/` is present:

- Treat the export as the test brief.
- Generate tests only for the stated scope.
- Follow repo testing conventions: xUnit, FluentAssertions, and mocking only where appropriate.
- Respect the target test type from the export: unit, integration, or mixed.
- Use the required scenarios as the minimum coverage bar.

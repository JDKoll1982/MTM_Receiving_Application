---
description: 'Interpret CopilotForms test generation exports linked from docs/CopilotForms/outputs/test-generation and use them to create focused test coverage.'
applyTo: 'docs/CopilotForms/outputs/test-generation/**/*.{md,json}'
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-test-generation.instructions.md
- Description: Interpret CopilotForms test generation exports linked from docs/CopilotForms/outputs/test-generation and use them to create focused test coverage.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-14: # CopilotForms Test Generation Exports
- Critical Notes: Generate tests only for the stated scope and follow repo testing conventions (xUnit, FluentAssertions).
[DOC-META-END]
-->

# CopilotForms Test Generation Exports

When a linked file from `docs/CopilotForms/outputs/test-generation/` is present:

- Treat the export as the test brief.
- Generate tests only for the stated scope.
- Follow repo testing conventions: xUnit, FluentAssertions, and mocking only where appropriate.
- Respect the target test type from the export: unit, integration, or mixed.
- Use the required scenarios as the minimum coverage bar.

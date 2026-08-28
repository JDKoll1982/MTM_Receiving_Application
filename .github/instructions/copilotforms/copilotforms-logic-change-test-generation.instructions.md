---
description: "Interpret CopilotForms logic plus tests exports linked from docs/CopilotForms/outputs/logic-change-test-generation and use them to align behavior and coverage."
applyTo: "docs/CopilotForms/outputs/logic-change-test-generation/**/*.{md,json}"
---

<!-- 
[DOC-META-START]
- File Name: copilotforms-logic-change-test-generation.instructions.md
- Description: Interpret CopilotForms logic plus tests exports linked from docs/CopilotForms/outputs/logic-change-test-generation and use them to align behavior and coverage.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-15: # CopilotForms Logic Change + Test Generation Exports
- Critical Notes: Fix the root behavior issue first, then add the smallest focused test set guarding against regression.
[DOC-META-END]
-->

# CopilotForms Logic Change + Test Generation Exports

When a linked file from `docs/CopilotForms/outputs/logic-change-test-generation/` is present:

- Treat the export as a structured behavior-correction request that explicitly includes test work.
- Prioritize intended behavior, actual behavior, desired rules, and required test scenarios.
- Use the listed files and feature context to inspect the smallest relevant logic path first.
- Fix the root behavior issue before broadening into unrelated cleanup.
- Add the smallest focused test set that proves the new rule set and guards against regression.
- Use the export's verification steps as the minimum validation bar.

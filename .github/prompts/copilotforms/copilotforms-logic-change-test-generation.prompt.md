---
name: copilotforms-logic-change-test-generation
description: "Read a saved CopilotForms logic-plus-tests export and implement the logic change together with focused test coverage."
agent: agent
argument-hint: "Link a logic/test export from docs/CopilotForms/outputs/logic-change-test-generation and add any extra testing constraints"
---

# CopilotForms Logic Change + Test Generation

Read the linked export from `docs/CopilotForms/outputs/logic-change-test-generation/` and use it as the structured input for behavior correction and test creation.

## Workflow

1. Read the linked export completely.
2. Separate intended behavior, actual behavior, and required test scenarios.
3. Inspect the smallest relevant code path first using the listed files and feature metadata.
4. Implement the smallest reliable behavior fix.
5. Add focused tests that prove the desired rule set and guard against regression.
6. Validate using the verification steps from the export.
7. Summarize the rule change, test coverage added, and residual risk.

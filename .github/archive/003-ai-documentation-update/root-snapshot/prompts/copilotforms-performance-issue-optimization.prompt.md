---
name: copilotforms-performance-issue-optimization
description: "Read a saved CopilotForms performance export and diagnose or improve the reported performance issue."
agent: agent
argument-hint: "Link a performance export from docs/CopilotForms/outputs/performance-issue-optimization and add any current measurements"
---

# CopilotForms Performance Issue / Optimization

Read the linked export from `docs/CopilotForms/outputs/performance-issue-optimization/` and use it as the structured input for diagnosis and optimization.

## Workflow

1. Read the linked export completely.
2. Distinguish observed impact from guessed causes.
3. Inspect the smallest likely hotspot first using the listed files and context.
4. Make the smallest reliable improvement that addresses the reported bottleneck.
5. Validate using the verification steps or measurements from the export.
6. Summarize the bottleneck, the improvement made, and any residual risk.

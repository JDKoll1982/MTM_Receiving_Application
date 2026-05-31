---
name: copilotforms-database-issue
description: 'Read a saved CopilotForms database issue export and diagnose or fix the database-related problem.'
agent: agent
argument-hint: 'Link a database issue export from docs/CopilotForms/outputs/database-issue and add any current observations'
---

# CopilotForms Database Issue

Read the linked export from `docs/CopilotForms/outputs/database-issue/` and use it as the brief for diagnosis and repair.

## Workflow

1. Read the linked export completely.
2. Identify the affected database system and named objects.
3. Inspect the smallest code and SQL surface needed to verify the problem.
4. Fix the issue while preserving database access rules in this repo.
5. Validate using the exported validation steps.
6. Summarize the root cause, the change, and any data or migration concerns.

## Output Expectations

- Respect stored procedure rules and read-only Visual database rules.
- Keep changes minimal and traceable.

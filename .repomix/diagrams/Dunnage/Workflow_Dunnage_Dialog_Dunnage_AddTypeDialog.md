# Dunnage Dialog Dunnage AddTypeDialog Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["OnSelectIconClick"]
    Start --> Step1
    Decision2{"if (selectedIcon.HasValue)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    End([End])
    Step1 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. OnSelectIconClick.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| OnSelectIconClick | Invoke OnSelectIconClick | n/a | n/a | Method: OnSelectIconClick | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


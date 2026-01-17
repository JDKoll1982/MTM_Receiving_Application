# Dunnage QuickAddTypeDialog.xaml Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["OnSelectIconClick"]
    Start --> Step1
    Step2["OnAddSpecClick"]
    Step1 --> Step2
    Step3["OnRemoveSpecClick"]
    Step2 --> Step3
    End([End])
    Step3 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. OnSelectIconClick.
2. OnAddSpecClick.
3. OnRemoveSpecClick.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| OnSelectIconClick | Invoke OnSelectIconClick | n/a | n/a | Method: OnSelectIconClick | See implementation | 
| OnAddSpecClick | Invoke OnAddSpecClick | n/a | n/a | Method: OnAddSpecClick | See implementation | 
| OnRemoveSpecClick | Invoke OnRemoveSpecClick | n/a | n/a | Method: OnRemoveSpecClick | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


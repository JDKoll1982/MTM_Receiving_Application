# Dunnage AdminPartsView Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["OnPageLoaded"]
    Start --> Step1
    Step2["OnSearchKeyboardAccelerator"]
    Step1 --> Step2
    End([End])
    Step2 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. OnPageLoaded.
2. OnSearchKeyboardAccelerator.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| OnPageLoaded | Invoke OnPageLoaded | n/a | n/a | Method: OnPageLoaded | See implementation | 
| OnSearchKeyboardAccelerator | Invoke OnSearchKeyboardAccelerator | n/a | n/a | Method: OnSearchKeyboardAccelerator | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


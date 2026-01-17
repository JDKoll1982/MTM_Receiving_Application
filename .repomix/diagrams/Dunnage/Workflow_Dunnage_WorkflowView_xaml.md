# Dunnage WorkflowView.xaml Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["HelpButton_Click"]
    Start --> Step1
    Step2["OnBackClick"]
    Step1 --> Step2
    Step3["OnNextClick"]
    Step2 --> Step3
    Step4["OnSaveAndReviewClick"]
    Step3 --> Step4
    End([End])
    Step4 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. HelpButton_Click.
2. OnBackClick.
3. OnNextClick.
4. OnSaveAndReviewClick.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| HelpButton_Click | Invoke HelpButton_Click | n/a | n/a | Method: HelpButton_Click | See implementation | 
| OnBackClick | Invoke OnBackClick | n/a | n/a | Method: OnBackClick | See implementation | 
| OnNextClick | Invoke OnNextClick | n/a | n/a | Method: OnNextClick | See implementation | 
| OnSaveAndReviewClick | Invoke OnSaveAndReviewClick | n/a | n/a | Method: OnSaveAndReviewClick | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


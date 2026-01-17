# Dunnage WorkflowView Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["OnWorkflowStepChanged"]
    Start --> Step1
    Decision2{"if (_helpService == null || _workflowService == null)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["HelpButton_Click"]
    Step1 --> Step2
    Decision3{"if (!result.IsSuccess)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["OnNextClick"]
    Step2 --> Step3
    Decision4{"if (confirmResult == ContentDialogResult.Primary)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["OnSaveAndReviewClick"]
    Step3 --> Step4
    Step5["OnBackClick"]
    Step4 --> Step5
    End([End])
    Step5 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. OnWorkflowStepChanged.
2. HelpButton_Click.
3. OnNextClick.
4. OnSaveAndReviewClick.
5. OnBackClick.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| OnWorkflowStepChanged | Invoke OnWorkflowStepChanged | n/a | n/a | Method: OnWorkflowStepChanged | See implementation | 
| HelpButton_Click | Invoke HelpButton_Click | n/a | n/a | Method: HelpButton_Click | See implementation | 
| OnNextClick | Invoke OnNextClick | n/a | n/a | Method: OnNextClick | See implementation | 
| OnSaveAndReviewClick | Invoke OnSaveAndReviewClick | n/a | n/a | Method: OnSaveAndReviewClick | See implementation | 
| OnBackClick | Invoke OnBackClick | n/a | n/a | Method: OnBackClick | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


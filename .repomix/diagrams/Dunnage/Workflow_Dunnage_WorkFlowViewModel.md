# Dunnage WorkFlowViewModel Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["InitializeWorkflowAsync"]
    Start --> Step1
    Decision2{"if (xamlRoot == null)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["OnWorkflowStepChanged"]
    Step1 --> Step2
    Decision3{"if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["ResetCSVAsync"]
    Step2 --> Step3
    Decision4{"if (!saveResult.IsSuccess)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["ReturnToModeSelection"]
    Step3 --> Step4
    Step5["AddLineAsync"]
    Step4 --> Step5
    End([End])
    Step5 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. InitializeWorkflowAsync.
2. OnWorkflowStepChanged.
3. ResetCSVAsync.
4. ReturnToModeSelection.
5. AddLineAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| InitializeWorkflowAsync | Invoke InitializeWorkflowAsync | n/a | n/a | Method: InitializeWorkflowAsync | See implementation | 
| OnWorkflowStepChanged | Invoke OnWorkflowStepChanged | n/a | n/a | Method: OnWorkflowStepChanged | See implementation | 
| ResetCSVAsync | Invoke ResetCSVAsync | n/a | n/a | Method: ResetCSVAsync | See implementation | 
| ReturnToModeSelection | Invoke ReturnToModeSelection | n/a | n/a | Method: ReturnToModeSelection | See implementation | 
| AddLineAsync | Invoke AddLineAsync | n/a | n/a | Method: AddLineAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


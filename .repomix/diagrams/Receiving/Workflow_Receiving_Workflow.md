# Receiving Workflow Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["OnWorkflowStepChanged"]
    Start --> Step1
    Decision2{"if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.Saving)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["NextStepAsync"]
    Step1 --> Step2
    Decision3{"if (!result.Success)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["PerformSaveAsync"]
    Step2 --> Step3
    Decision4{"if(result.ValidationErrors.Count&gt;0)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["StartNewEntryAsync"]
    Step3 --> Step4
    Step5["ResetCSVAsync"]
    Step4 --> Step5
    Step6["PreviousStep"]
    Step5 --> Step6
    Step7["ReturnToModeSelectionAsync"]
    Step6 --> Step7
    Step8["ShowHelpAsync"]
    Step7 --> Step8
    End([End])
    Step8 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. OnWorkflowStepChanged.
2. NextStepAsync.
3. PerformSaveAsync.
4. StartNewEntryAsync.
5. ResetCSVAsync.
6. PreviousStep.
7. ReturnToModeSelectionAsync.
8. ShowHelpAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| OnWorkflowStepChanged | Invoke OnWorkflowStepChanged | n/a | n/a | Method: OnWorkflowStepChanged | See implementation | 
| NextStepAsync | Invoke NextStepAsync | n/a | n/a | Method: NextStepAsync | See implementation | 
| PerformSaveAsync | Invoke PerformSaveAsync | n/a | n/a | Method: PerformSaveAsync | See implementation | 
| StartNewEntryAsync | Invoke StartNewEntryAsync | n/a | n/a | Method: StartNewEntryAsync | See implementation | 
| ResetCSVAsync | Invoke ResetCSVAsync | n/a | n/a | Method: ResetCSVAsync | See implementation | 
| PreviousStep | Invoke PreviousStep | n/a | n/a | Method: PreviousStep | See implementation | 
| ReturnToModeSelectionAsync | Invoke ReturnToModeSelectionAsync | n/a | n/a | Method: ReturnToModeSelectionAsync | See implementation | 
| ShowHelpAsync | Invoke ShowHelpAsync | n/a | n/a | Method: ShowHelpAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Receiving
- Generated: 2026-01-17


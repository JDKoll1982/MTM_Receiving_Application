# Service ReceivingWorkflow Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["RaiseStatusMessage"]
    Start --> Step1
    Decision2{"if (_currentStep != value)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["StartWorkflowAsync"]
    Step1 --> Step2
    Decision3{"if (existingSession?.HasLoads == true)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["AdvanceToNextStepAsync"]
    Step2 --> Step3
    Decision4{"if (currentUser != null && !string.IsNullOrEmpty(currentUser.DefaultReceivingMode))"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["GenerateLoads"]
    Step3 --> Step4
    Step5["GoToPreviousStep"]
    Step4 --> Step5
    Step6["GoToStep"]
    Step5 --> Step6
    Step7["AddCurrentPartToSessionAsync"]
    Step6 --> Step7
    Step8["ClearUIInputs"]
    Step7 --> Step8
    End([End])
    Step8 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. RaiseStatusMessage.
2. StartWorkflowAsync.
3. AdvanceToNextStepAsync.
4. GenerateLoads.
5. GoToPreviousStep.
6. GoToStep.
7. AddCurrentPartToSessionAsync.
8. ClearUIInputs.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| RaiseStatusMessage | Invoke RaiseStatusMessage | n/a | n/a | Method: RaiseStatusMessage | See implementation | 
| StartWorkflowAsync | Invoke StartWorkflowAsync | n/a | n/a | Method: StartWorkflowAsync | See implementation | 
| AdvanceToNextStepAsync | Invoke AdvanceToNextStepAsync | n/a | n/a | Method: AdvanceToNextStepAsync | See implementation | 
| GenerateLoads | Invoke GenerateLoads | n/a | n/a | Method: GenerateLoads | See implementation | 
| GoToPreviousStep | Invoke GoToPreviousStep | n/a | n/a | Method: GoToPreviousStep | See implementation | 
| GoToStep | Invoke GoToStep | n/a | n/a | Method: GoToStep | See implementation | 
| AddCurrentPartToSessionAsync | Invoke AddCurrentPartToSessionAsync | n/a | n/a | Method: AddCurrentPartToSessionAsync | See implementation | 
| ClearUIInputs | Invoke ClearUIInputs | n/a | n/a | Method: ClearUIInputs | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Receiving
- Generated: 2026-01-17


# IService ReceivingWorkflow Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["RaiseStatusMessage"]
    Start --> Step1
    Step2["StartWorkflowAsync"]
    Step1 --> Step2
    Step3["AdvanceToNextStepAsync"]
    Step2 --> Step3
    Step4["GoToPreviousStep"]
    Step3 --> Step4
    Step5["GoToStep"]
    Step4 --> Step5
    Step6["AddCurrentPartToSessionAsync"]
    Step5 --> Step6
    Step7["SaveSessionAsync"]
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
4. GoToPreviousStep.
5. GoToStep.
6. AddCurrentPartToSessionAsync.
7. SaveSessionAsync.
8. ClearUIInputs.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| RaiseStatusMessage | Invoke RaiseStatusMessage | n/a | n/a | Method: RaiseStatusMessage | See implementation | 
| StartWorkflowAsync | Invoke StartWorkflowAsync | n/a | n/a | Method: StartWorkflowAsync | See implementation | 
| AdvanceToNextStepAsync | Invoke AdvanceToNextStepAsync | n/a | n/a | Method: AdvanceToNextStepAsync | See implementation | 
| GoToPreviousStep | Invoke GoToPreviousStep | n/a | n/a | Method: GoToPreviousStep | See implementation | 
| GoToStep | Invoke GoToStep | n/a | n/a | Method: GoToStep | See implementation | 
| AddCurrentPartToSessionAsync | Invoke AddCurrentPartToSessionAsync | n/a | n/a | Method: AddCurrentPartToSessionAsync | See implementation | 
| SaveSessionAsync | Invoke SaveSessionAsync | n/a | n/a | Method: SaveSessionAsync | See implementation | 
| ClearUIInputs | Invoke ClearUIInputs | n/a | n/a | Method: ClearUIInputs | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Receiving
- Generated: 2026-01-17


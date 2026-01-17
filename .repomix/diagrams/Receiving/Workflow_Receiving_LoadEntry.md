# Receiving LoadEntry Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["ResetToDefaults"]
    Start --> Step1
    Decision2{"if (_workflowService.CurrentStep == Enum_ReceivingWorkflowStep.LoadEntry)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["OnStepChanged"]
    Step1 --> Step2
    Decision3{"if (!validationResult.IsValid)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["CreateLoadsAsync"]
    Step2 --> Step3
    Step4["UpdatePartInfo"]
    Step3 --> Step4
    Step5["ShowHelpAsync"]
    Step4 --> Step5
    Step6["GetTooltip"]
    Step5 --> Step6
    Step7["GetPlaceholder"]
    Step6 --> Step7
    Step8["GetTip"]
    Step7 --> Step8
    End([End])
    Step8 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. ResetToDefaults.
2. OnStepChanged.
3. CreateLoadsAsync.
4. UpdatePartInfo.
5. ShowHelpAsync.
6. GetTooltip.
7. GetPlaceholder.
8. GetTip.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| ResetToDefaults | Invoke ResetToDefaults | n/a | n/a | Method: ResetToDefaults | See implementation | 
| OnStepChanged | Invoke OnStepChanged | n/a | n/a | Method: OnStepChanged | See implementation | 
| CreateLoadsAsync | Invoke CreateLoadsAsync | n/a | n/a | Method: CreateLoadsAsync | See implementation | 
| UpdatePartInfo | Invoke UpdatePartInfo | n/a | n/a | Method: UpdatePartInfo | See implementation | 
| ShowHelpAsync | Invoke ShowHelpAsync | n/a | n/a | Method: ShowHelpAsync | See implementation | 
| GetTooltip | Invoke GetTooltip | n/a | n/a | Method: GetTooltip | See implementation | 
| GetPlaceholder | Invoke GetPlaceholder | n/a | n/a | Method: GetPlaceholder | See implementation | 
| GetTip | Invoke GetTip | n/a | n/a | Method: GetTip | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Receiving
- Generated: 2026-01-17


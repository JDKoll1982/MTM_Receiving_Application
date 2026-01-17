# Dunnage QuantityEntryViewModel Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["OnWorkflowStepChanged"]
    Start --> Step1
    Decision2{"if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.QuantityEntry)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["LoadContextData"]
    Step1 --> Step2
    Decision3{"if (!string.IsNullOrEmpty(SelectedTypeIcon) && Enum.TryParse&lt;MaterialIconKind&gt;(SelectedTypeIcon, true, out var kind))"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["ValidateQuantity"]
    Step2 --> Step3
    Decision4{"if(_workflowService.CurrentSession.Quantity&lt;=0)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["GoNextAsync"]
    Step3 --> Step4
    End([End])
    Step4 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. OnWorkflowStepChanged.
2. LoadContextData.
3. ValidateQuantity.
4. GoNextAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| OnWorkflowStepChanged | Invoke OnWorkflowStepChanged | n/a | n/a | Method: OnWorkflowStepChanged | See implementation | 
| LoadContextData | Invoke LoadContextData | n/a | n/a | Method: LoadContextData | See implementation | 
| ValidateQuantity | Invoke ValidateQuantity | n/a | n/a | Method: ValidateQuantity | See implementation | 
| GoNextAsync | Invoke GoNextAsync | n/a | n/a | Method: GoNextAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


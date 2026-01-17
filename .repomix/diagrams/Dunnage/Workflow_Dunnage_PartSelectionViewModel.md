# Dunnage PartSelectionViewModel Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["OnWorkflowStepChanged"]
    Start --> Step1
    Decision2{"if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.PartSelection)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["InitializeAsync"]
    Step1 --> Step2
    Decision3{"if (!string.IsNullOrEmpty(SelectedTypeIcon) && Enum.TryParse&lt;MaterialIconKind&gt;(SelectedTypeIcon, true, out var kind))"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["LoadPartsAsync"]
    Step2 --> Step3
    Decision4{"if (IsBusy)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["CheckInventoryStatusAsync"]
    Step3 --> Step4
    Step5["UpdateInventoryMessage"]
    Step4 --> Step5
    Step6["SelectPartAsync"]
    Step5 --> Step6
    Step7["QuickAddPartAsync"]
    Step6 --> Step7
    End([End])
    Step7 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. OnWorkflowStepChanged.
2. InitializeAsync.
3. LoadPartsAsync.
4. CheckInventoryStatusAsync.
5. UpdateInventoryMessage.
6. SelectPartAsync.
7. QuickAddPartAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| OnWorkflowStepChanged | Invoke OnWorkflowStepChanged | n/a | n/a | Method: OnWorkflowStepChanged | See implementation | 
| InitializeAsync | Invoke InitializeAsync | n/a | n/a | Method: InitializeAsync | See implementation | 
| LoadPartsAsync | Invoke LoadPartsAsync | n/a | n/a | Method: LoadPartsAsync | See implementation | 
| CheckInventoryStatusAsync | Invoke CheckInventoryStatusAsync | n/a | n/a | Method: CheckInventoryStatusAsync | See implementation | 
| UpdateInventoryMessage | Invoke UpdateInventoryMessage | n/a | n/a | Method: UpdateInventoryMessage | See implementation | 
| SelectPartAsync | Invoke SelectPartAsync | n/a | n/a | Method: SelectPartAsync | See implementation | 
| QuickAddPartAsync | Invoke QuickAddPartAsync | n/a | n/a | Method: QuickAddPartAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


# RoutingWizardStep1ViewModel Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["ValidatePOAsync"]
    Start --> Step1
    Decision2{"if (e.PropertyName == nameof(_containerViewModel.IsEditingFromReview))"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["SwitchToOtherModeAsync"]
    Step1 --> Step2
    Decision3{"if (IsBusy)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["ShowPONotFoundDialogAsync"]
    Step2 --> Step3
    Decision4{"if (string.IsNullOrWhiteSpace(PoNumber))"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["InitializeAsync"]
    Step3 --> Step4
    End([End])
    Step4 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. ValidatePOAsync.
2. SwitchToOtherModeAsync.
3. ShowPONotFoundDialogAsync.
4. InitializeAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| ValidatePOAsync | Invoke ValidatePOAsync | n/a | n/a | Method: ValidatePOAsync | See implementation | 
| SwitchToOtherModeAsync | Invoke SwitchToOtherModeAsync | n/a | n/a | Method: SwitchToOtherModeAsync | See implementation | 
| ShowPONotFoundDialogAsync | Invoke ShowPONotFoundDialogAsync | n/a | n/a | Method: ShowPONotFoundDialogAsync | See implementation | 
| InitializeAsync | Invoke InitializeAsync | n/a | n/a | Method: InitializeAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Routing
- Generated: 2026-01-17


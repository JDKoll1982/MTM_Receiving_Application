# RoutingWizardStep3ViewModel Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["LoadReviewData"]
    Start --> Step1
    Decision2{"if (_containerViewModel.SelectedPOLine != null)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["EditPOSelection"]
    Step1 --> Step2
    Decision3{"else if (_containerViewModel.SelectedOtherReason != null)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["EditRecipientSelection"]
    Step2 --> Step3
    Decision4{"if (_containerViewModel.SelectedRecipient != null)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["NavigateBack"]
    Step3 --> Step4
    Step5["CreateLabelAsync"]
    Step4 --> Step5
    Step6["ShowDuplicateConfirmationAsync"]
    Step5 --> Step6
    End([End])
    Step6 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. LoadReviewData.
2. EditPOSelection.
3. EditRecipientSelection.
4. NavigateBack.
5. CreateLabelAsync.
6. ShowDuplicateConfirmationAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| LoadReviewData | Invoke LoadReviewData | n/a | n/a | Method: LoadReviewData | See implementation | 
| EditPOSelection | Invoke EditPOSelection | n/a | n/a | Method: EditPOSelection | See implementation | 
| EditRecipientSelection | Invoke EditRecipientSelection | n/a | n/a | Method: EditRecipientSelection | See implementation | 
| NavigateBack | Invoke NavigateBack | n/a | n/a | Method: NavigateBack | See implementation | 
| CreateLabelAsync | Invoke CreateLabelAsync | n/a | n/a | Method: CreateLabelAsync | See implementation | 
| ShowDuplicateConfirmationAsync | Invoke ShowDuplicateConfirmationAsync | n/a | n/a | Method: ShowDuplicateConfirmationAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Routing
- Generated: 2026-01-17


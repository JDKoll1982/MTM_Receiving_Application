# RoutingWizardContainerViewModel Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["NavigateToStep2"]
    Start --> Step1
    Decision2{"if (SelectedPOLine == null && SelectedOtherReason == null)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["NavigateToStep3"]
    Step1 --> Step2
    Decision3{"if (SelectedPOLine != null)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["NavigateToStep1"]
    Step2 --> Step3
    Decision4{"if (SelectedRecipient == null)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["NavigateToStep1ForEdit"]
    Step3 --> Step4
    Step5["NavigateBackToStep2"]
    Step4 --> Step5
    Step6["NavigateToStep2ForEdit"]
    Step5 --> Step6
    Step7["CreateLabelAsync"]
    Step6 --> Step7
    Step8["CancelAsync"]
    Step7 --> Step8
    End([End])
    Step8 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. NavigateToStep2.
2. NavigateToStep3.
3. NavigateToStep1.
4. NavigateToStep1ForEdit.
5. NavigateBackToStep2.
6. NavigateToStep2ForEdit.
7. CreateLabelAsync.
8. CancelAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| NavigateToStep2 | Invoke NavigateToStep2 | n/a | n/a | Method: NavigateToStep2 | See implementation | 
| NavigateToStep3 | Invoke NavigateToStep3 | n/a | n/a | Method: NavigateToStep3 | See implementation | 
| NavigateToStep1 | Invoke NavigateToStep1 | n/a | n/a | Method: NavigateToStep1 | See implementation | 
| NavigateToStep1ForEdit | Invoke NavigateToStep1ForEdit | n/a | n/a | Method: NavigateToStep1ForEdit | See implementation | 
| NavigateBackToStep2 | Invoke NavigateBackToStep2 | n/a | n/a | Method: NavigateBackToStep2 | See implementation | 
| NavigateToStep2ForEdit | Invoke NavigateToStep2ForEdit | n/a | n/a | Method: NavigateToStep2ForEdit | See implementation | 
| CreateLabelAsync | Invoke CreateLabelAsync | n/a | n/a | Method: CreateLabelAsync | See implementation | 
| CancelAsync | Invoke CancelAsync | n/a | n/a | Method: CancelAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Routing
- Generated: 2026-01-17


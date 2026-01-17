# Shared NewUserSetupDialog Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["OnDialogLoaded"]
    Start --> Step1
    Decision2{"if (args.Result != ContentDialogResult.Primary)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["DepartmentComboBox_SelectionChanged"]
    Step1 --> Step2
    Decision3{"if (DepartmentComboBox.SelectedItem != null)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["OnCancelButtonClick"]
    Step2 --> Step3
    Decision4{"if (selectedDept == \"Other\")"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["SetLoadingState"]
    Step3 --> Step4
    End([End])
    Step4 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. OnDialogLoaded.
2. DepartmentComboBox_SelectionChanged.
3. OnCancelButtonClick.
4. SetLoadingState.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| OnDialogLoaded | Invoke OnDialogLoaded | n/a | n/a | Method: OnDialogLoaded | See implementation | 
| DepartmentComboBox_SelectionChanged | Invoke DepartmentComboBox_SelectionChanged | n/a | n/a | Method: DepartmentComboBox_SelectionChanged | See implementation | 
| OnCancelButtonClick | Invoke OnCancelButtonClick | n/a | n/a | Method: OnCancelButtonClick | See implementation | 
| SetLoadingState | Invoke SetLoadingState | n/a | n/a | Method: SetLoadingState | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Shared
- Generated: 2026-01-17


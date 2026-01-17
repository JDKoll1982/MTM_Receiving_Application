# Dunnage QuickAddTypeDialog Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["InitializeForEdit"]
    Start --> Step1
    Decision2{"if (NewSpecTypeCombo.SelectedItem is ComboBoxItem item)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["OnAddSpecClick"]
    Step1 --> Step2
    Decision3{"if (!System.Enum.TryParse&lt;MaterialIconKind&gt;(iconName, out var kind))"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["AddSpec"]
    Step2 --> Step3
    Decision4{"if (string.IsNullOrWhiteSpace(name))"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["OnRemoveSpecClick"]
    Step3 --> Step4
    Step5["OnSelectIconClick"]
    Step4 --> Step5
    End([End])
    Step5 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. InitializeForEdit.
2. OnAddSpecClick.
3. AddSpec.
4. OnRemoveSpecClick.
5. OnSelectIconClick.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| InitializeForEdit | Invoke InitializeForEdit | n/a | n/a | Method: InitializeForEdit | See implementation | 
| OnAddSpecClick | Invoke OnAddSpecClick | n/a | n/a | Method: OnAddSpecClick | See implementation | 
| AddSpec | Invoke AddSpec | n/a | n/a | Method: AddSpec | See implementation | 
| OnRemoveSpecClick | Invoke OnRemoveSpecClick | n/a | n/a | Method: OnRemoveSpecClick | See implementation | 
| OnSelectIconClick | Invoke OnSelectIconClick | n/a | n/a | Method: OnSelectIconClick | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


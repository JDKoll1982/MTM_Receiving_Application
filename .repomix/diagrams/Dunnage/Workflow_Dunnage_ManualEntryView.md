# Dunnage ManualEntryView Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["Loads_CollectionChanged"]
    Start --> Step1
    Decision2{"if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["ManualEntryDataGrid_CurrentCellChanged"]
    Step1 --> Step2
    Decision3{"if(ViewModel.Loads.Count&gt;0)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["ManualEntryDataGrid_KeyDown"]
    Step2 --> Step3
    Decision4{"if (e.NewItems?[0] is Model_DunnageLoad newItem)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["ManualEntryDataGrid_Tapped"]
    Step3 --> Step4
    Step5["SelectFirstEditableCell"]
    Step4 --> Step5
    End([End])
    Step5 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. Loads_CollectionChanged.
2. ManualEntryDataGrid_CurrentCellChanged.
3. ManualEntryDataGrid_KeyDown.
4. ManualEntryDataGrid_Tapped.
5. SelectFirstEditableCell.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| Loads_CollectionChanged | Invoke Loads_CollectionChanged | n/a | n/a | Method: Loads_CollectionChanged | See implementation | 
| ManualEntryDataGrid_CurrentCellChanged | Invoke ManualEntryDataGrid_CurrentCellChanged | n/a | n/a | Method: ManualEntryDataGrid_CurrentCellChanged | See implementation | 
| ManualEntryDataGrid_KeyDown | Invoke ManualEntryDataGrid_KeyDown | n/a | n/a | Method: ManualEntryDataGrid_KeyDown | See implementation | 
| ManualEntryDataGrid_Tapped | Invoke ManualEntryDataGrid_Tapped | n/a | n/a | Method: ManualEntryDataGrid_Tapped | See implementation | 
| SelectFirstEditableCell | Invoke SelectFirstEditableCell | n/a | n/a | Method: SelectFirstEditableCell | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


# Dunnage ManualEntryViewModel Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["InitializeAsync"]
    Start --> Step1
    Decision2{"if (typesResult.Success && typesResult.Data != null)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["AddRow"]
    Step1 --> Step2
    Decision3{"if (partsResult.Success && partsResult.Data != null)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["AddMultipleRowsAsync"]
    Step2 --> Step3
    Decision4{"if (xamlRoot == null)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["RemoveRow"]
    Step3 --> Step4
    Step5["AutoFillAsync"]
    Step4 --> Step5
    Step6["SaveToHistoryAsync"]
    Step5 --> Step6
    Step7["SaveAllAsync"]
    Step6 --> Step7
    Step8["ReturnToModeSelectionAsync"]
    Step7 --> Step8
    End([End])
    Step8 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. InitializeAsync.
2. AddRow.
3. AddMultipleRowsAsync.
4. RemoveRow.
5. AutoFillAsync.
6. SaveToHistoryAsync.
7. SaveAllAsync.
8. ReturnToModeSelectionAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| InitializeAsync | Invoke InitializeAsync | n/a | n/a | Method: InitializeAsync | See implementation | 
| AddRow | Invoke AddRow | n/a | n/a | Method: AddRow | See implementation | 
| AddMultipleRowsAsync | Invoke AddMultipleRowsAsync | n/a | n/a | Method: AddMultipleRowsAsync | See implementation | 
| RemoveRow | Invoke RemoveRow | n/a | n/a | Method: RemoveRow | See implementation | 
| AutoFillAsync | Invoke AutoFillAsync | n/a | n/a | Method: AutoFillAsync | See implementation | 
| SaveToHistoryAsync | Invoke SaveToHistoryAsync | n/a | n/a | Method: SaveToHistoryAsync | See implementation | 
| SaveAllAsync | Invoke SaveAllAsync | n/a | n/a | Method: SaveAllAsync | See implementation | 
| ReturnToModeSelectionAsync | Invoke ReturnToModeSelectionAsync | n/a | n/a | Method: ReturnToModeSelectionAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


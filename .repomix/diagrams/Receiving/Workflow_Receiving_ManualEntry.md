# Receiving ManualEntry Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["AutoFillAsync"]
    Start --> Step1
    Decision2{"if (string.IsNullOrWhiteSpace(currentLoad.PartID))"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["AddRow"]
    Step1 --> Step2
    Decision3{"if(i&gt;0)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["AddMultipleRowsAsync"]
    Step2 --> Step3
    Decision4{"if (!string.IsNullOrWhiteSpace(prev.PartID))"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["AddNewLoad"]
    Step3 --> Step4
    Step5["RemoveRow"]
    Step4 --> Step5
    Step6["SaveAsync"]
    Step5 --> Step6
    Step7["ReturnToModeSelectionAsync"]
    Step6 --> Step7
    Step8["ShowHelpAsync"]
    Step7 --> Step8
    End([End])
    Step8 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. AutoFillAsync.
2. AddRow.
3. AddMultipleRowsAsync.
4. AddNewLoad.
5. RemoveRow.
6. SaveAsync.
7. ReturnToModeSelectionAsync.
8. ShowHelpAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| AutoFillAsync | Invoke AutoFillAsync | n/a | n/a | Method: AutoFillAsync | See implementation | 
| AddRow | Invoke AddRow | n/a | n/a | Method: AddRow | See implementation | 
| AddMultipleRowsAsync | Invoke AddMultipleRowsAsync | n/a | n/a | Method: AddMultipleRowsAsync | See implementation | 
| AddNewLoad | Invoke AddNewLoad | n/a | n/a | Method: AddNewLoad | See implementation | 
| RemoveRow | Invoke RemoveRow | n/a | n/a | Method: RemoveRow | See implementation | 
| SaveAsync | Invoke SaveAsync | n/a | n/a | Method: SaveAsync | See implementation | 
| ReturnToModeSelectionAsync | Invoke ReturnToModeSelectionAsync | n/a | n/a | Method: ReturnToModeSelectionAsync | See implementation | 
| ShowHelpAsync | Invoke ShowHelpAsync | n/a | n/a | Method: ShowHelpAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Receiving
- Generated: 2026-01-17


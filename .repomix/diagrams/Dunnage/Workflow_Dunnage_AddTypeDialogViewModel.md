# Dunnage AddTypeDialogViewModel Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["SaveTypeAsync"]
    Start --> Step1
    Decision2{"if (IsBusy || !CanSave)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["AddField"]
    Step1 --> Step2
    Decision3{"if (!typeResult.IsSuccess)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["EditField"]
    Step2 --> Step3
    Decision4{"if (!fieldResult.IsSuccess)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["DeleteField"]
    Step3 --> Step4
    Step5["ValidateTypeName"]
    Step4 --> Step5
    Step6["ValidateFieldName"]
    Step5 --> Step6
    Step7["UpdateCanSave"]
    Step6 --> Step7
    Step8["LoadRecentlyUsedIconsAsync"]
    Step7 --> Step8
    End([End])
    Step8 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. SaveTypeAsync.
2. AddField.
3. EditField.
4. DeleteField.
5. ValidateTypeName.
6. ValidateFieldName.
7. UpdateCanSave.
8. LoadRecentlyUsedIconsAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| SaveTypeAsync | Invoke SaveTypeAsync | n/a | n/a | Method: SaveTypeAsync | See implementation | 
| AddField | Invoke AddField | n/a | n/a | Method: AddField | See implementation | 
| EditField | Invoke EditField | n/a | n/a | Method: EditField | See implementation | 
| DeleteField | Invoke DeleteField | n/a | n/a | Method: DeleteField | See implementation | 
| ValidateTypeName | Invoke ValidateTypeName | n/a | n/a | Method: ValidateTypeName | See implementation | 
| ValidateFieldName | Invoke ValidateFieldName | n/a | n/a | Method: ValidateFieldName | See implementation | 
| UpdateCanSave | Invoke UpdateCanSave | n/a | n/a | Method: UpdateCanSave | See implementation | 
| LoadRecentlyUsedIconsAsync | Invoke LoadRecentlyUsedIconsAsync | n/a | n/a | Method: LoadRecentlyUsedIconsAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


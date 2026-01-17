# Enum DunnageWorkflowStep Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["ModeSelection = 0"]
    Start --> Step1
    Step2["TypeSelection = 1"]
    Step1 --> Step2
    Step3["PartSelection = 2"]
    Step2 --> Step3
    Step4["QuantityEntry = 3"]
    Step3 --> Step4
    Step5["DetailsEntry = 4"]
    Step4 --> Step5
    Step6["Review = 5"]
    Step5 --> Step6
    Step7["ManualEntry = 6"]
    Step6 --> Step7
    Step8["EditMode = 7"]
    Step7 --> Step8
    End([End])
    Step8 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. ModeSelection = 0.
2. TypeSelection = 1.
3. PartSelection = 2.
4. QuantityEntry = 3.
5. DetailsEntry = 4.
6. Review = 5.
7. ManualEntry = 6.
8. EditMode = 7.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| ModeSelection = 0 | Invoke ModeSelection = 0 | n/a | n/a | Method: ModeSelection = 0 | See implementation | 
| TypeSelection = 1 | Invoke TypeSelection = 1 | n/a | n/a | Method: TypeSelection = 1 | See implementation | 
| PartSelection = 2 | Invoke PartSelection = 2 | n/a | n/a | Method: PartSelection = 2 | See implementation | 
| QuantityEntry = 3 | Invoke QuantityEntry = 3 | n/a | n/a | Method: QuantityEntry = 3 | See implementation | 
| DetailsEntry = 4 | Invoke DetailsEntry = 4 | n/a | n/a | Method: DetailsEntry = 4 | See implementation | 
| Review = 5 | Invoke Review = 5 | n/a | n/a | Method: Review = 5 | See implementation | 
| ManualEntry = 6 | Invoke ManualEntry = 6 | n/a | n/a | Method: ManualEntry = 6 | See implementation | 
| EditMode = 7 | Invoke EditMode = 7 | n/a | n/a | Method: EditMode = 7 | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


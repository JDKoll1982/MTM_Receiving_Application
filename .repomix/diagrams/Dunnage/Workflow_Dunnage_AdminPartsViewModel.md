# Dunnage AdminPartsViewModel Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["LoadPartsAsync"]
    Start --> Step1
    Decision2{"if (typesResult.Success && typesResult.Data != null)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["FilterByTypeAsync"]
    Step1 --> Step2
    Decision3{"if (!partsResult.Success)"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["SearchPartsAsync"]
    Step2 --> Step3
    Decision4{"if (SelectedFilterType == null)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["ClearFiltersAsync"]
    Step3 --> Step4
    Step5["LoadPage"]
    Step4 --> Step5
    Step6["OnPageChanged"]
    Step5 --> Step6
    Step7["UpdateNavigationButtons"]
    Step6 --> Step7
    Step8["ShowDeleteConfirmationAsync"]
    Step7 --> Step8
    End([End])
    Step8 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. LoadPartsAsync.
2. FilterByTypeAsync.
3. SearchPartsAsync.
4. ClearFiltersAsync.
5. LoadPage.
6. OnPageChanged.
7. UpdateNavigationButtons.
8. ShowDeleteConfirmationAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| LoadPartsAsync | Invoke LoadPartsAsync | n/a | n/a | Method: LoadPartsAsync | See implementation | 
| FilterByTypeAsync | Invoke FilterByTypeAsync | n/a | n/a | Method: FilterByTypeAsync | See implementation | 
| SearchPartsAsync | Invoke SearchPartsAsync | n/a | n/a | Method: SearchPartsAsync | See implementation | 
| ClearFiltersAsync | Invoke ClearFiltersAsync | n/a | n/a | Method: ClearFiltersAsync | See implementation | 
| LoadPage | Invoke LoadPage | n/a | n/a | Method: LoadPage | See implementation | 
| OnPageChanged | Invoke OnPageChanged | n/a | n/a | Method: OnPageChanged | See implementation | 
| UpdateNavigationButtons | Invoke UpdateNavigationButtons | n/a | n/a | Method: UpdateNavigationButtons | See implementation | 
| ShowDeleteConfirmationAsync | Invoke ShowDeleteConfirmationAsync | n/a | n/a | Method: ShowDeleteConfirmationAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


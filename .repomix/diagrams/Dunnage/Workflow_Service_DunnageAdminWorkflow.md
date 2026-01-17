# Service DunnageAdminWorkflow Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["NavigateToSectionAsync"]
    Start --> Step1
    Decision2{"if (_currentSection != value)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["NavigateToHubAsync"]
    Step1 --> Step2
    Decision3{"if (!await CanNavigateAwayAsync())"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["CanNavigateAwayAsync"]
    Step2 --> Step3
    Decision4{"if (!_isDirty)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["MarkDirty"]
    Step3 --> Step4
    Step5["MarkClean"]
    Step4 --> Step5
    End([End])
    Step5 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. NavigateToSectionAsync.
2. NavigateToHubAsync.
3. CanNavigateAwayAsync.
4. MarkDirty.
5. MarkClean.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| NavigateToSectionAsync | Invoke NavigateToSectionAsync | n/a | n/a | Method: NavigateToSectionAsync | See implementation | 
| NavigateToHubAsync | Invoke NavigateToHubAsync | n/a | n/a | Method: NavigateToHubAsync | See implementation | 
| CanNavigateAwayAsync | Invoke CanNavigateAwayAsync | n/a | n/a | Method: CanNavigateAwayAsync | See implementation | 
| MarkDirty | Invoke MarkDirty | n/a | n/a | Method: MarkDirty | See implementation | 
| MarkClean | Invoke MarkClean | n/a | n/a | Method: MarkClean | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


# Dunnage Dialog AddToInventoriedListDialog Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["LoadPartsAsync"]
    Start --> Step1
    Decision2{"if (result.IsSuccess && result.Data != null)"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["PartIdComboBox_TextSubmitted"]
    Step1 --> Step2
    Decision3{"if (!string.IsNullOrWhiteSpace(args.Text))"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    End([End])
    Step2 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. LoadPartsAsync.
2. PartIdComboBox_TextSubmitted.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| LoadPartsAsync | Invoke LoadPartsAsync | n/a | n/a | Method: LoadPartsAsync | See implementation | 
| PartIdComboBox_TextSubmitted | Invoke PartIdComboBox_TextSubmitted | n/a | n/a | Method: PartIdComboBox_TextSubmitted | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Dunnage
- Generated: 2026-01-17


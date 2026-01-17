# Dao SettingsDiagnostics Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["GetTablesAsync"]
    Start --> Step1
    Step2["GetStoredProceduresAsync"]
    Step1 --> Step2
    End([End])
    Step2 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. GetTablesAsync.
2. GetStoredProceduresAsync.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| GetTablesAsync | Invoke GetTablesAsync | n/a | n/a | Method: GetTablesAsync | See implementation | 
| GetStoredProceduresAsync | Invoke GetStoredProceduresAsync | n/a | n/a | Method: GetStoredProceduresAsync | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Settings.DeveloperTools
- Generated: 2026-01-17


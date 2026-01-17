# Enum SettingsScope Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["System = 0"]
    Start --> Step1
    Step2["User = 1"]
    Step1 --> Step2
    End([End])
    Step2 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. System = 0.
2. User = 1.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| System = 0 | Invoke System = 0 | n/a | n/a | Method: System = 0 | See implementation | 
| User = 1 | Invoke User = 1 | n/a | n/a | Method: User = 1 | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Settings.Core
- Generated: 2026-01-17


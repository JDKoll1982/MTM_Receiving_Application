# UpdateShipmentCommandValidator Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["PartNumber"]
    Start --> Step1
    Step2["ReceivedSkidCount"]
    Step1 --> Step2
    End([End])
    Step2 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. PartNumber.
2. ReceivedSkidCount.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| PartNumber | Invoke PartNumber | n/a | n/a | Method: PartNumber | See implementation | 
| ReceivedSkidCount | Invoke ReceivedSkidCount | n/a | n/a | Method: ReceivedSkidCount | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Volvo
- Generated: 2026-01-17


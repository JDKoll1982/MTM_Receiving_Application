# Volvo ShipmentEntry.xaml Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["OnLoaded"]
    Start --> Step1
    Step2["AddPartButton_Click"]
    Step1 --> Step2
    Step3["ReportDiscrepancyButton_Click"]
    Step2 --> Step3
    Step4["ViewDiscrepancyButton_Click"]
    Step3 --> Step4
    Step5["RemoveDiscrepancyButton_Click"]
    Step4 --> Step5
    Step6["RemoveSelectedPartButton_Click"]
    Step5 --> Step6
    End([End])
    Step6 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. OnLoaded.
2. AddPartButton_Click.
3. ReportDiscrepancyButton_Click.
4. ViewDiscrepancyButton_Click.
5. RemoveDiscrepancyButton_Click.
6. RemoveSelectedPartButton_Click.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| OnLoaded | Invoke OnLoaded | n/a | n/a | Method: OnLoaded | See implementation | 
| AddPartButton_Click | Invoke AddPartButton_Click | n/a | n/a | Method: AddPartButton_Click | See implementation | 
| ReportDiscrepancyButton_Click | Invoke ReportDiscrepancyButton_Click | n/a | n/a | Method: ReportDiscrepancyButton_Click | See implementation | 
| ViewDiscrepancyButton_Click | Invoke ViewDiscrepancyButton_Click | n/a | n/a | Method: ViewDiscrepancyButton_Click | See implementation | 
| RemoveDiscrepancyButton_Click | Invoke RemoveDiscrepancyButton_Click | n/a | n/a | Method: RemoveDiscrepancyButton_Click | See implementation | 
| RemoveSelectedPartButton_Click | Invoke RemoveSelectedPartButton_Click | n/a | n/a | Method: RemoveSelectedPartButton_Click | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Volvo
- Generated: 2026-01-17


# Receiving POEntry Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["ResetToDefaults"]
    Start --> Step1
    Decision2{"if (string.IsNullOrWhiteSpace(PoNumber))"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["PoTextBoxLostFocus"]
    Step1 --> Step2
    Decision3{"if (value.StartsWith(\"PO-\", StringComparison.OrdinalIgnoreCase))"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["LoadPOAsync"]
    Step2 --> Step3
    Decision4{"if(numberPart.All(char.IsDigit)&&numberPart.Length&lt;=6)"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    Step4["ToggleNonPO"]
    Step3 --> Step4
    Step5["LookupPartAsync"]
    Step4 --> Step5
    Step6["ShowHelpAsync"]
    Step5 --> Step6
    Step7["GetTooltip"]
    Step6 --> Step7
    Step8["GetPlaceholder"]
    Step7 --> Step8
    End([End])
    Step8 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. ResetToDefaults.
2. PoTextBoxLostFocus.
3. LoadPOAsync.
4. ToggleNonPO.
5. LookupPartAsync.
6. ShowHelpAsync.
7. GetTooltip.
8. GetPlaceholder.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| ResetToDefaults | Invoke ResetToDefaults | n/a | n/a | Method: ResetToDefaults | See implementation | 
| PoTextBoxLostFocus | Invoke PoTextBoxLostFocus | n/a | n/a | Method: PoTextBoxLostFocus | See implementation | 
| LoadPOAsync | Invoke LoadPOAsync | n/a | n/a | Method: LoadPOAsync | See implementation | 
| ToggleNonPO | Invoke ToggleNonPO | n/a | n/a | Method: ToggleNonPO | See implementation | 
| LookupPartAsync | Invoke LookupPartAsync | n/a | n/a | Method: LookupPartAsync | See implementation | 
| ShowHelpAsync | Invoke ShowHelpAsync | n/a | n/a | Method: ShowHelpAsync | See implementation | 
| GetTooltip | Invoke GetTooltip | n/a | n/a | Method: GetTooltip | See implementation | 
| GetPlaceholder | Invoke GetPlaceholder | n/a | n/a | Method: GetPlaceholder | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Receiving
- Generated: 2026-01-17


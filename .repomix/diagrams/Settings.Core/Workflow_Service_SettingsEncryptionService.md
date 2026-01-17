# Service SettingsEncryptionService Workflow

## Diagram (Mermaid)

```mermaid
flowchart TD
    Start([Start])
    Step1["Encrypt"]
    Start --> Step1
    Decision2{"if (string.IsNullOrWhiteSpace(cipherText))"}
    Step1 --> Decision2
    Yes2["Yes path"]
    No2["No path"]
    Decision2 -->|Yes| Yes2
    Decision2 -->|No| No2
    Yes2 --> Step1
    No2 --> Step1
    Step2["Decrypt"]
    Step1 --> Step2
    Decision3{"if (!string.IsNullOrWhiteSpace(directory))"}
    Step2 --> Decision3
    Yes3["Yes path"]
    No3["No path"]
    Decision3 -->|Yes| Yes3
    Decision3 -->|No| No3
    Yes3 --> Step2
    No3 --> Step2
    Step3["RotateKey"]
    Step2 --> Step3
    Decision4{"if (File.Exists(KeyPath))"}
    Step3 --> Decision4
    Yes4["Yes path"]
    No4["No path"]
    Decision4 -->|Yes| Yes4
    Decision4 -->|No| No4
    Yes4 --> Step3
    No4 --> Step3
    End([End])
    Step3 --> End
```

## Things to fix

- None detected.

## User-Friendly Steps

1. Encrypt.
2. Decrypt.
3. RotateKey.

## Required Info for Fixing Incorrect Workflows

| Step | UI / Action | Command / Query | Validator Rules (Actual) | Handler / Data Path | Actual Data (from code) |
|---|---|---|---|---|---|
| Encrypt | Invoke Encrypt | n/a | n/a | Method: Encrypt | See implementation | 
| Decrypt | Invoke Decrypt | n/a | n/a | Method: Decrypt | See implementation | 
| RotateKey | Invoke RotateKey | n/a | n/a | Method: RotateKey | See implementation | 

## Source

- Repomix file: C:\Users\johnk\source\repos\MTM_Receiving_Application\.repomix\outputs\code-only\repomix-output-code-only.md
- Type: Settings.Core
- Generated: 2026-01-17


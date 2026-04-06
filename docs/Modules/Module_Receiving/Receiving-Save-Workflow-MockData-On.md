# Receiving Save Workflow - Mock Data On

Last Updated: 2026-04-05

This diagram shows the save path when test data mode is on.
It includes the extra step that writes a matching test transaction so location checking can still be tried later.

```mermaid
flowchart TD
  Start([User chooses Save]) --> Validate{Are all entries complete and valid?}
  Validate -->|No| ShowValidation[Show the missing or incorrect items]
  ShowValidation --> ValidationEnd([Save stops])

  Validate -->|Yes| SaveRows[Write the receiving rows to the saved label list]
  SaveRows --> SaveWorked{Did the main save finish successfully?}

  SaveWorked -->|No| ShowSaveError[Show a save error message]
  ShowSaveError --> SaveFailEnd([Save stops])

  SaveWorked -->|Yes| BuildTestRows[Create matching test transactions from the saved rows]
  BuildTestRows --> WriteTestRows[Add or refresh those rows in the test data file]
  WriteTestRows --> TestWriteWorked{Did the test data file update succeed?}

  TestWriteWorked -->|Yes| MarkSuccess[Mark the save as successful]
  TestWriteWorked -->|No| KeepSaveAndWarn[Keep the main save successful and add a warning note]

  MarkSuccess --> FinalSuccess([Save completes])
  KeepSaveAndWarn --> FinalSuccess
```

## Plain-English Summary

- The first half is the same as the normal save flow.
- After the main save succeeds, the app creates test transactions that match the saved material, order, date, and location.
- Those test transactions are written into the mock data file.
- If that extra test-data write fails, the main save still stays successful, but the app records a warning so the issue is visible.
- This lets the later location-check feature work in test mode without needing a live system.

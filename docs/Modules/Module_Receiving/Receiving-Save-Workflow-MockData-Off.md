# Receiving Save Workflow - Mock Data Off

Last Updated: 2026-04-05

This diagram shows the normal save path when test data mode is off.

```mermaid
flowchart TD
  Start([User chooses Save]) --> Validate{Are all entries complete and valid?}
  Validate -->|No| ShowValidation[Show the missing or incorrect items]
  ShowValidation --> ValidationEnd([Save stops])

  Validate -->|Yes| SaveRows[Write the receiving rows to the saved label list]
  SaveRows --> SaveWorked{Did the save finish successfully?}

  SaveWorked -->|No| ShowSaveError[Show a save error message]
  ShowSaveError --> SaveFailEnd([Save stops])

  SaveWorked -->|Yes| CountRows[Count how many rows were added]
  CountRows --> WarnIfSkipped{Were any rows skipped because they already existed?}

  WarnIfSkipped -->|Yes| ShowWarning[Keep the save successful and add a warning note]
  WarnIfSkipped -->|No| MarkSuccess[Mark the save as successful]
  ShowWarning --> MarkSuccess

  MarkSuccess --> FinalSuccess([Save completes])
```

## Plain-English Summary

- The save starts only if every row passes the normal checks.
- If the data is not ready, the user sees what needs to be fixed and the save stops.
- If the data is ready, the app saves the receiving rows to the saved label list.
- If that save fails, the user sees an error and nothing else happens.
- If it succeeds, the app finishes the save and can also show a warning if some rows were already there.

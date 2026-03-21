# Module_Receiving — Architecture Inventory

Last Updated: 2026-03-21

This document summarizes the March 2026 code analysis of the Receiving module.
It captures the main screens, ViewModels, services, DAOs, models, workflow paths,
and enum dependencies that shape the receiving experience.

---

## Views And Screen Roles

| View                                 | Primary Role                                   |
| ------------------------------------ | ---------------------------------------------- |
| `View_Receiving_Workflow.xaml`       | Workflow shell that hosts the active step view |
| `View_Receiving_ModeSelection.xaml`  | Choose Guided, Manual, or Edit mode            |
| `View_Receiving_POEntry.xaml`        | Enter PO and resolve available parts           |
| `View_Receiving_ManualEntry.xaml`    | Freeform grid-based receiving entry            |
| `View_Receiving_EditMode.xaml`       | Search, page, and edit existing load data      |
| `View_Receiving_LoadEntry.xaml`      | Enter number of loads and receiving location   |
| `View_Receiving_WeightQuantity.xaml` | Enter weight or quantity for each load         |
| `View_Receiving_HeatLot.xaml`        | Enter optional heat/lot values per load        |
| `View_Receiving_PackageType.xaml`    | Select package type and package counts         |
| `View_Receiving_Review.xaml`         | Review accumulated loads before save           |

---

## ViewModels

| ViewModel                            | Main Responsibility                              | Notable Properties / Commands                                               |
| ------------------------------------ | ------------------------------------------------ | --------------------------------------------------------------------------- |
| `ViewModel_Receiving_Workflow`       | Orchestrates state machine and screen visibility | `CurrentStepTitle`, visibility flags, `LastSaveResult`, save progress state |
| `ViewModel_Receiving_ModeSelection`  | Mode choice and default-mode persistence         | Guided / Manual / Edit commands, default-mode toggles                       |
| `ViewModel_Receiving_POEntry`        | PO lookup and part selection                     | `PoNumber`, `Parts`, `SelectedPart`, PO status state                        |
| `ViewModel_Receiving_ManualEntry`    | Grid-driven manual entry                         | `Loads`, `SelectedLoad`, add/remove/autofill/save commands                  |
| `ViewModel_Receiving_LoadEntry`      | Load count and optional location                 | `NumberOfLoads`, `Location`, preset location state                          |
| `ViewModel_Receiving_WeightQuantity` | Per-load quantity entry and warnings             | `Loads`, `WarningMessage`, `PoQuantityInfo`                                 |
| `ViewModel_Receiving_HeatLot`        | Heat/lot step                                    | `Loads`, autofill command                                                   |
| `ViewModel_Receiving_PackageType`    | Package type and package defaults                | `Loads`, package-type selector, save-as-default state                       |
| `ViewModel_Receiving_Review`         | Final review and save trigger                    | `Loads`, single/table toggle, add-another, save command                     |
| `ViewModel_Receiving_EditMode`       | Edit saved loads with paging and filtering       | `Loads`, `SearchText`, `CurrentDataSource`, pagination state                |

---

## Service Interfaces And Runtime Roles

| Service                        | Role                                                                 |
| ------------------------------ | -------------------------------------------------------------------- |
| `IService_ReceivingWorkflow`   | Workflow state machine, navigation, session save, and reset          |
| `IService_MySQL_Receiving`     | Batch save, history lookup, label-data reads and updates             |
| `IService_ReceivingValidation` | Field validation, PO/part checks, same-day warnings, location checks |
| `IService_MySQL_ReceivingLine` | Insert or update line-level label data                               |
| `IService_XLSWriter`           | Write, read, clear, and verify XLS label files                       |
| `IService_QualityHoldWarning`  | Detect and surface quality hold restrictions                         |
| `IService_ReceivingSettings`   | Read runtime settings and text values by key                         |
| `Service_SessionManager`       | Session persistence and restore support                              |

---

## DAO Inventory

| DAO                         | Main Data Responsibility                                   |
| --------------------------- | ---------------------------------------------------------- |
| `Dao_ReceivingLoad`         | Save batch receiving loads with transactional insert logic |
| `Dao_ReceivingLine`         | Insert receiving label-data rows through stored procedures |
| `Dao_QualityHold`           | Persist and read quality-hold acknowledgments              |
| `Dao_PackageTypePreference` | Store package-type preferences and prefix rules            |
| `Dao_ReceivingLabelData`    | Query current and historical label data for Edit Mode      |

The DAO layer follows the project rule set: instance-based classes, stored-procedure-first MySQL writes, and result-object error handling.

---

## Core Models

| Model                                                                       | Purpose                            | Key Fields                                                                                                                                                |
| --------------------------------------------------------------------------- | ---------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Model_ReceivingLoad`                                                       | Main in-memory receiving record    | `LoadID`, `PartID`, `PoNumber`, `LoadNumber`, `WeightQuantity`, `HeatLotNumber`, `InitialLocation`, `PackagesPerLoad`, `PackageType`, quality-hold fields |
| `Model_ReceivingSession`                                                    | Session container for the workflow | Session ID, user, load list, totals                                                                                                                       |
| `Model_ReceivingLine`                                                       | Label-data record shape            | Quantity, part, PO, employee, heat, location, vendor, part description                                                                                    |
| `Model_QualityHold`                                                         | Quality restriction record         | Load ID, restriction type, acknowledgment metadata                                                                                                        |
| `Model_SaveResult`                                                          | Save outcome                       | Success state, counts, messages, file paths                                                                                                               |
| `Model_ReceivingValidationResult`                                           | Validation outcome                 | Success / warning state and message                                                                                                                       |
| `Model_XLSWriteResult`, `Model_XLSDeleteResult`, `Model_XLSExistenceResult` | XLS file operation outcomes        | File paths, existence flags, delete counts                                                                                                                |

---

## Workflow Paths

### Guided Path

`ModeSelection` → `POEntry` → optional `PartSelection` → `LoadEntry` → `WeightQuantityEntry` → `HeatLotEntry` → `PackageTypeEntry` → `Review` → `Saving` → `Complete`

### Manual Path

`ModeSelection` → `ManualEntry` → `Saving` → `Complete`

### Edit Path

`ModeSelection` or post-save edit access → `EditMode` → save changes or return to review context

### Dynamic Branches

- `PartSelection` appears only when a PO resolves to multiple parts.
- Startup can bypass `ModeSelection` entirely based on restored session state, user preference, or settings.
- `Review` can branch into `Add Another Part/PO`, preserving reviewed loads while resetting current entry inputs.

---

## Main Validation And Error Gates

| Step              | Main Blocking Conditions                                 |
| ----------------- | -------------------------------------------------------- |
| PO Entry          | Missing PO in PO mode, unresolved part                   |
| Load Entry        | Invalid load count, invalid or missing location fallback |
| Weight / Quantity | Invalid per-load quantity values                         |
| Heat / Lot        | Invalid heat/lot content after blank normalization       |
| Package Type      | Missing package type or invalid package counts           |
| Manual Entry      | Unacknowledged quality hold                              |

Warnings can also be raised without blocking, especially for same-day receiving and quantity exceeding PO quantity.

---

## Relevant Enum Dependencies

The current Receiving workflow depends on these confirmed enums:

| Enum                         | Purpose                                                        |
| ---------------------------- | -------------------------------------------------------------- |
| `Enum_ReceivingWorkflowStep` | Workflow state machine and step routing                        |
| `Enum_DataSourceType`        | Edit Mode source selection: memory, current labels, or history |
| `Enum_PackageType`           | Package classification for receiving loads                     |
| `Enum_ErrorSeverity`         | Error-reporting severity across services and DAOs              |
| `Enum_ValidationSeverity`    | Warning versus blocking validation outcomes                    |

All confirmed enum dependencies in this analysis resolve back to `Module_Core`.

---

## Related Documents

- `UI-Conditional-Guards.md`
- `Settings-Feature-Flags.md`

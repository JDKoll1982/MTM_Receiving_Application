# Module_Receiving vs Old_Module_Receiving - Business Logic Comparison

**Generated:** 2025-01-30  
**Purpose:** Identify business logic features in Old_Module_Receiving that may need to be ported to the new Module_Receiving implementation

---

## User Notes

Validation Logic Change 1: All validation needs to happen after losing focus on PO entry field, not just on save and not while typing.


---

## Executive Summary

The **Old_Module_Receiving** was a monolithic MVVM implementation with direct service-to-DAO architecture. The **new Module_Receiving** uses CQRS pattern with MediatR handlers, validators, and SQL Server database. This document captures business logic differences to help decide which features from the old implementation should be ported.

### Key Architectural Differences

| Aspect | Old_Module_Receiving | New Module_Receiving | Notes |
|--------|---------------------|---------------------|-------|
| **Pattern** | MVVM with Services | CQRS with MediatR Handlers | Complete architectural shift |
| **Database** | MySQL only | SQL Server (primary) + MySQL (legacy) | Database migration in progress |
| **Validation** | Service-based inline validation | FluentValidation validators | More structured approach |
| **State Management** | Workflow service with state machine | Handler-based with session persistence | Different state management paradigm |
| **Error Handling** | Exception-based with try-catch | Result objects with error messages | Non-exception model preferred |

---

## 1. Workflow and State Management

### 1.1 Session Persistence and Restoration

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **JSON Session File Persistence** | Missing | Old version persists `Model_ReceivingSession` to `%APPDATA%\MTM_Receiving_Application\session.json` for crash recovery | Need: `CommandHandler_Receiving_Shared_Save_SessionToFile`, `QueryHandler_Receiving_Shared_Load_SessionFromFile` |
| ☐ **Auto-Restore on Startup** | Missing | Old version automatically restores incomplete sessions on app launch | Need: Integration in `Service_OnStartup_AppLifecycle` or startup handler |
| ☐ **Session Cleanup After Save** | Missing | Old version deletes session file after successful database save to prevent stale data | Need: Add to `CommandHandler_Receiving_Shared_Save_Transaction` completion logic |
| ☐ **Session Expiration Logic** | Missing | No time-based session expiration in old version, but could be valuable addition | Enhancement opportunity |
| ℹ️ **Core Service Available** | - | `IService_SessionManager` exists in Module_Core with `ClearSessionAsync()`, `SessionExists()`, `GetSessionFilePath()` | Can be leveraged for implementation |

**Decision Required:** Should session restoration be implemented for wizard mode crash recovery?

---

### 1.2 Workflow Step State Machine

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Step Transition Validation** | Partial | Old version validates data completeness before allowing step advancement | New version has validators but may not enforce blocking transitions |
| ☐ **Step History / Back Navigation** | Missing | Old version tracks `GoToPreviousStep()` with context-aware back button behavior | Need: Navigation history stack in orchestration ViewModel |
| ☐ **Conditional Step Skipping** | Missing | Old version skips mode selection if user has `DefaultReceivingMode` preference | Need: User preference-based workflow routing |
| ☐ **Step Title Dynamic Updates** | Partial | Old version updates window title per step (e.g., "Receiving - PO Entry") | Check if new wizard orchestration updates titles |
| ☐ **Multi-Path Workflow Support** | Missing | Old version supports Guided/Manual/Edit mode paths from single orchestrator | New version focuses on wizard mode only |

**Decision Required:** Do we need multi-mode support (Manual Entry, Edit Mode) or wizard-only focus?

---

## 2. Data Entry Modes

### 2.1 Manual Entry Mode

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Manual Entry ViewModel** | Missing | `ViewModel_Receiving_ManualEntry` - free-form grid entry without wizard steps | Not present in new Module_Receiving |
| ☐ **Manual Entry View** | Missing | `View_Receiving_ManualEntry.xaml` - data grid for bulk entry | Not present in new Module_Receiving |
| ☐ **Bypass Step Validation** | Missing | Manual mode goes directly from entry → review → save | Not applicable if manual mode not needed |
| ☐ **Grid-Based Load Entry** | Missing | Old version allows entering multiple loads in a grid without step-by-step wizard | New version only has wizard mode |

**Decision Required:** Is manual mode needed, or is wizard mode sufficient for all use cases?

---

### 2.2 Edit Mode

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Edit Mode ViewModel** | Missing | `ViewModel_Receiving_EditMode` - allows modifying previously saved loads | Not present in new Module_Receiving |
| ☐ **Edit Mode View** | Missing | `View_Receiving_EditMode.xaml` - loads existing data for editing | Not present in new Module_Receiving |
| ☐ **Load Existing Records** | Missing | Query MySQL to retrieve previously saved loads for editing | Would need new query handler |
| ☐ **Update vs Insert Logic** | Partial | Old version has `UpdateReceivingLoadsAsync()` separate from `SaveReceivingLoadsAsync()` | New version has update handler but no edit UI workflow |
| ☐ **Delete Load Capability** | Partial | Old version supports deleting individual loads before saving | New version has delete handler but no UI integration |

**Decision Required:** Is edit mode required for production, or do users only create new entries?

---

## 3. Quality Hold Management

### 3.1 Quality Hold Detection and Warnings

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Restricted Part Detection** | Missing | Old version detects `MMFSR` (Sheet) and `MMCSR` (Coil) parts requiring quality inspection | Pattern: `partID.Contains("MMFSR") || partID.Contains("MMCSR")` |
| ☐ **First Warning Dialog** | Missing | Shows "Acknowledgment 1 of 2" immediately when restricted part entered | Need: `Service_QualityHoldWarning.CheckAndWarnAsync()` equivalent |
| ☐ **Second Warning at Save** | Missing | Shows "Acknowledgment 2 of 2" before committing to database | Need: Pre-save validation check in save handler |
| ☐ **Quality Hold Blocking** | Missing | Old version prevents saving if user hasn't acknowledged BOTH warnings | Need: Add to `Validator_Receiving_Shared_ValidateOn_SaveTransaction` |
| ☐ **Quality Contact Instructions** | Missing | Warning includes "Contact Quality NOW" and "DO NOT sign paperwork" instructions | Need: User-facing content in dialog |
| ☐ **Load-Level Hold Tracking** | Partial | Old version has `IsQualityHoldRequired`, `IsQualityHoldAcknowledged`, `QualityHoldRestrictionType` on load model | Check if new entity model has these fields |

**Impact:** High - Quality holds are critical safety/compliance requirement. Missing this could cause quality issues.

---

### 3.2 Quality Hold Database Tracking

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Quality Hold Table** | Unknown | Old version has `receiving_quality_holds` table with acknowledgment tracking | Need: Check if SQL Server schema includes this table |
| ☐ **Insert Quality Hold Record** | Partial | `Dao_QualityHold.InsertQualityHoldAsync()` creates hold record when restricted part saved | Need: Handler to insert hold record during save |
| ☐ **Quality Acknowledgment Update** | Partial | `UpdateQualityHoldAcknowledgmentAsync()` records when quality inspector approves | Need: Separate quality inspector workflow (future enhancement?) |
| ☐ **Quality Hold Reporting** | Missing | `GetQualityHoldsByLoadIDAsync()` retrieves hold history | Need: Query handler for quality hold reports |
| ℹ️ **Service Exists** | - | `IService_MySQL_QualityHold` and implementation exist in old contracts | Can be ported to new architecture |

**Decision Required:** Is quality hold tracking required in MVP, or can it be added later?

---

## 4. Package Type Preferences

### 4.1 Part-Based Package Preferences

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Save Last Package Type** | Missing | Old version remembers package type (Skid/Pallet/Box/Crate/Coil/Sheet) per part number | Stored in `package_type_preferences` table |
| ☐ **Auto-Fill Package Type** | Missing | When user enters part, automatically pre-fills last-used package type | Need: Query on part selection to retrieve preference |
| ☐ **Custom Package Type Names** | Missing | Old version supports `CustomTypeName` field for user-defined types | Check if new schema supports custom names |
| ☐ **Preference Update on Save** | Missing | Old version upserts preference when user saves with different package type | Need: Logic in save handler to update preferences |
| ℹ️ **DAO Exists** | Partial | `Dao_Receiving_Repository_PartPreference` exists in new module | Check if it includes package type functionality |

**Business Value:** Medium - Reduces repetitive data entry for frequently received parts

---

### 4.2 User-Based Package Preferences

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **User Default Package Type** | Missing | Old version stores per-user default package type (independent of part) | Stored in `package_preferences` table with `username` key |
| ☐ **Workstation-Specific Defaults** | Missing | Old version tracks preferences by `username` + `workstation` combination | Allows different defaults for different receiving stations |
| ☐ **User Preference UI** | Missing | Settings page to configure user's default package type | Need: Settings module integration |

**Business Value:** Low-Medium - Nice-to-have for user customization

---

## 5. Validation Rules

### 5.1 PO Number Validation

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ✅ **PO Format Validation** | Exists | Regex pattern `^(PO-)?\d{1,6}$` allows optional "PO-" prefix + 1-6 digits | Check `Validator_Receiving_Shared_ValidateIf_ValidPOFormat` |
| ☐ **Non-PO Item Support** | Unknown | Old version has `IsNonPOItem` flag to bypass PO validation | Check if new workflow supports non-PO receiving |
| ☐ **PO-Specific Error Messages** | Partial | Old version returns "PO number must be numeric (up to 6 digits) or in PO-###### format" | Verify error message clarity in new validators |

---

### 5.2 Load Entry Validation

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ✅ **Number of Loads Range** | Likely Exists | Must be between 1 and 99 loads | Standard business rule |
| ✅ **Weight/Quantity > 0** | Likely Exists | Weight must be positive decimal | Standard business rule |
| ✅ **Package Count > 0** | Likely Exists | At least 1 package per load | Standard business rule |
| ☐ **Heat/Lot Required** | Unknown | Old version requires heat/lot but allows "Nothing Entered" placeholder on save | Check if new version enforces or allows blank |
| ☐ **Part ID Length Limit** | Unknown | Old version enforces 50 character max on Part ID | Check field length validation |

---

### 5.3 Load-Level Auto-Validation

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Auto Package Count Default** | Missing | When Part ID entered and `PackagesPerLoad == 0`, auto-set to 1 | Old version has `OnPartIDChanged` property trigger |
| ☐ **Auto Package Type by Part** | Missing | If part contains "MMC" → Coil, "MMF" → Sheet, else → Skid default | Old version in `OnPartIDChanged` logic |
| ☐ **Weight Per Package Calculation** | Unknown | Old version calculates `WeightPerPackage = WeightQuantity / PackagesPerLoad` | Check if new model has calculated field |

**Decision Required:** Should auto-defaults be implemented as business logic or left to user entry?

---

## 6. CSV Export Functionality

### 6.1 Local CSV Export

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Local CSV Path** | Unknown | Old version writes to `%APPDATA%\MTM_Receiving_Application\ReceivingData.csv` | Check if new version has CSV export |
| ☐ **CsvHelper Library** | Unknown | Old version uses CsvHelper with custom config | Check dependencies |
| ☐ **Auto-Create Directory** | Missing | Old version creates `%APPDATA%` directory if missing | Needed for robustness |
| ☐ **CSV Write Result Tracking** | Missing | Old version returns `Model_CSVWriteResult` with `LocalSuccess`, `NetworkSuccess`, `RecordsWritten` | Need: Result model for CSV operations |
| ℹ️ **Core Interface Exists** | - | `IService_CSVWriter` exists in Module_Core contracts | Can be implemented for receiving |

---

### 6.2 Network CSV Export

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Network Path Resolution** | Missing | Old version writes to `\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{Username}\` | Environment-specific configuration |
| ☐ **Per-User Network Folder** | Missing | Creates folder per Windows username for file separation | Security/organization requirement |
| ☐ **Graceful Network Failure** | Missing | If network write fails, logs warning but doesn't fail transaction | Non-blocking network save |
| ☐ **Network Directory Creation** | Missing | Auto-creates user folder on network if missing | Permission-dependent feature |

**Decision Required:** Is network CSV still required, or is database-only storage sufficient?

---

### 6.3 CSV Data Format

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **CSV Column Mapping** | Missing | Old version maps `Model_ReceivingLoad` to CSV columns | Need: Define CSV schema |
| ☐ **Date Formatting** | Missing | Old version formats `ReceivedDate` consistently | CsvHelper configuration |
| ☐ **Null Handling** | Missing | Old version handles `null` PO numbers for non-PO items | Empty string vs "N/A" decision |
| ☐ **Header Row** | Missing | CSV includes header row with column names | Standard CSV format |

---

## 7. User Preferences and Defaults

### 7.1 Default Receiving Mode

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **User.DefaultReceivingMode** | Unknown | Old version supports "guided", "manual", "edit" mode defaults | Stored in user profile |
| ☐ **Skip Mode Selection** | Missing | If default mode set, skip mode selection step and go directly to workflow | Workflow routing logic |
| ☐ **Mode Selection UI** | Partial | Old version has `View_Receiving_ModeSelection.xaml` | New version has `View_Receiving_Hub_Display_ModeSelection` but may not store preference |
| ☐ **Invalid Mode Fallback** | Missing | If stored mode is invalid, fallback to showing mode selection | Error handling |

**Business Value:** Medium - Power users can skip mode selection step every time

---

### 7.2 Application Variables

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Model_Application_Variables** | Unknown | Old version has centralized app variables model | Check if new version uses similar pattern |
| ☐ **Connection String Storage** | Exists | Both versions need MySQL connection string | `Helper_Database_Variables` in Module_Core |
| ☐ **CSV Path Configuration** | Missing | Old version has configurable CSV paths | May be hardcoded in old version |

---

## 8. Help System Integration

### 8.1 Contextual Help Content

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Per-Step Help Content** | Unknown | Old version has `_stepTitles` dictionary and help system integration | Check if new wizard has help panel |
| ☐ **Help Button Visibility** | Unknown | Old version toggles help panel via `IService_Help` | Module_Core service available |
| ☐ **Dynamic Help Content** | Missing | Help content changes per workflow step | Need: Content generation per step |
| ℹ️ **Help Service Exists** | - | `IService_Help` and `Helper_WorkflowHelpContentGenerator` exist in Module_Core | Can be leveraged |

---

### 8.2 Settings Integration

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **ReceivingSettingsKeys** | Missing | Old version has centralized settings keys (e.g., "WorkflowHelpText", "WorkflowBackText") | Constants for UI text localization |
| ☐ **ReceivingSettingsDefaults** | Missing | Old version has default values for all settings | Fallback values |
| ☐ **IService_ReceivingSettings** | Missing | Old version loads UI text from settings (future localization support) | Service pattern for settings |
| ☐ **Dynamic UI Text** | Missing | Old version has properties like `WorkflowNextText`, `CompletionSuccessTitleText` loaded from settings | Allows text customization without code changes |

**Business Value:** Low for MVP, High for future (enables localization/branding)

---

## 9. Save Operation Orchestration

### 9.1 Multi-Destination Save

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Three-Part Save Process** | Missing | Old version saves to: (1) MySQL Database, (2) Local CSV, (3) Network CSV | New version likely database-only |
| ☐ **Save Progress Tracking** | Missing | Old version updates `SaveProgressMessage` and `SaveProgressValue` during multi-step save | Progress bar UX |
| ☐ **Partial Success Handling** | Missing | Old version tracks which destinations succeeded/failed in `Model_SaveResult` | User needs to know if CSV failed but DB succeeded |
| ☐ **Save Result Model** | Missing | `Model_SaveResult` with `DatabaseSuccess`, `LocalCSVSuccess`, `NetworkCSVSuccess`, `TotalLoadsSaved` | Need: Comprehensive result object |

---

### 9.2 Save Workflow Steps

| Feature | Status | Description | Implementation Notes |
|---------|--------|-------------|---------------------|
| ☐ **Saving Step UI** | Missing | Old version shows dedicated "Saving" step with progress indicators | `View_Receiving_Workflow` has saving-specific panel |
| ☐ **Completion Step UI** | Partial | Old version shows success/failure summary with save details | Check if new wizard has completion screen |
| ☐ **Error Detail Display** | Unknown | Old version shows which save operations failed | User-friendly error reporting |
| ☐ **Start New Entry Button** | Unknown | After completion, button to reset workflow and start fresh entry | Workflow reset logic |

---

## 10. Data Models and Entities

### 10.1 Old Models Not in New Module

| Model | Purpose | Migration Status |
|-------|---------|------------------|
| ☐ `Model_ReceivingSession` | Session-level data with loads collection | Check if new module has equivalent |
| ☐ `Model_ReceivingLoad` | Individual load/skid with observable properties | New module likely has `Model_Receiving_Entity_Line` |
| ☐ `Model_QualityHold` | Quality hold tracking for restricted parts | Unknown if in new schema |
| ☐ `Model_PackageTypePreference` | Part-based package type memory | Unknown if in new schema |
| ☐ `Model_UserPreference` | User-based package type defaults | Unknown if in new schema |
| ☐ `Model_SaveResult` | Multi-destination save result tracking | Need: Result model |
| ☐ `Model_CSVWriteResult` | CSV export result with path and success flags | Need: CSV result model |
| ☐ `Model_CSVExistenceResult` | CSV file existence check result | Need: File check result model |
| ☐ `Model_CSVDeleteResult` | CSV file deletion result | Need: File deletion result model |
| ☐ `Model_ReceivingValidationResult` | Validation result with `IsValid` and `Message` | FluentValidation replaces this pattern |
| ☐ `Model_ReceivingWorkflowStepResult` | Step transition result with errors collection | Unknown if needed in CQRS pattern |
| ☐ `Model_InforVisualPart` | Part data from ERP system | Check if Module_Core has equivalent |
| ☐ `Model_InforVisualPO` | PO data from ERP system | Check if Module_Core has equivalent |

---

### 10.2 Key Fields Comparison

| Field | Old Model | New Model Status | Notes |
|-------|-----------|-----------------|-------|
| ☐ `LoadID` (Guid) | Guid-based identifier | Unknown if new uses Guid or int | Check new entity primary key |
| ☐ `IsQualityHoldRequired` | Boolean flag | Missing | Critical for quality workflow |
| ☐ `IsQualityHoldAcknowledged` | Boolean flag | Missing | Tracks user acknowledgment |
| ☐ `QualityHoldRestrictionType` | String (MMFSR/MMCSR) | Missing | Displayed in warnings |
| ☐ `IsNonPOItem` | Boolean flag | Unknown | Allows receiving without PO |
| ☐ `PackageTypeName` | String | Unknown | User-facing type name |
| ☐ `PackageType` (Enum) | Enum_PackageType | Check if new has equivalent enum | Enum vs string decision |
| ☐ `WeightPerPackage` | Calculated decimal | Unknown | Auto-calculated field |
| ☐ `SessionID` (Guid) | Session tracking | Unknown | For session persistence |
| ☐ `User` (Model_User) | Associated user | Unknown | Session user tracking |

---

## 11. Enums and Constants

### 11.1 Enums Comparison

| Enum | Old Module | New Module | Status |
|------|-----------|-----------|--------|
| `Enum_ReceivingWorkflowStep` | ModeSelection, POEntry, LoadEntry, WeightQuantityEntry, HeatLotEntry, PackageTypeEntry, Review, Saving, Complete, ManualEntry, EditMode | Check `Enum_Receiving_State_WorkflowStep` | May have different step granularity |
| `Enum_PackageType` | Skid, Pallet, Box, Crate, Coil, Sheet, Custom | Check `Enum_Receiving_Type_PartType` or similar | May need package type enum |
| `Enum_Receiving_Mode_WorkflowMode` | Not in old | New module has this | New architecture addition |
| `Enum_Receiving_State_TransactionStatus` | Not in old | New module has this | New architecture addition |
| `Enum_Receiving_CopyType_FieldSelection` | Not in old | New module has this | Bulk copy operations feature |

---

## 12. Services Architecture

### 12.1 Old Services Not in New Module

| Service | Purpose | Migration Path |
|---------|---------|----------------|
| ☐ `Service_ReceivingWorkflow` | State machine orchestrator for wizard steps | Replaced by orchestration ViewModels + handlers |
| ☐ `Service_SessionManager` | JSON file persistence | Use Module_Core `IService_SessionManager` + create handlers |
| ☐ `Service_ReceivingValidation` | Business rule validation service | Replaced by FluentValidation validators |
| ☐ `Service_QualityHoldWarning` | Quality hold dialog service | Need: New service or ViewModel logic |
| ☐ `Service_MySQL_QualityHold` | Quality hold database operations | Need: Create handlers for quality hold CRUD |
| ☐ `Service_MySQL_PackagePreferences` | Package preference database operations | Partially exists as `Dao_Receiving_Repository_PartPreference` |
| ☐ `Service_MySQL_Receiving` | Load save/update/delete operations | Replaced by transaction handlers |
| ☐ `Service_MySQL_ReceivingLine` | Line-level operations | Replaced by line handlers |
| ☐ `Service_CSVWriter` | CSV export with dual paths | Need: Create CSV export handler or use Module_Core service |
| ☐ `Service_ReceivingSettings` | Settings management | Need: Settings infrastructure |

---

### 12.2 Services Available in Module_Core

| Service | Interface | Usage in Old Module | Can Replace |
|---------|-----------|-------------------|-------------|
| ✅ `IService_SessionManager` | Session file management | Used by `Service_SessionManager` | Yes - base functionality exists |
| ✅ `IService_LoggingUtility` | Logging | Used everywhere | Yes - already used |
| ✅ `IService_ErrorHandler` | Error handling | Used in ViewModels | Yes - already used |
| ✅ `IService_UserSessionManager` | User session tracking | Used for username resolution | Yes - already available |
| ✅ `IService_Window` | Window/XamlRoot access | Used for dialogs | Yes - already available |
| ✅ `IService_Help` | Help panel management | Used for contextual help | Yes - can be integrated |
| ✅ `IService_InforVisual` | ERP data access | Used for PO/Part lookups | Yes - already available |
| ✅ `Helper_Database_StoredProcedure` | SP execution | Used in all DAOs | Yes - already used |

---

## 13. ViewModels and Views

### 13.1 Old ViewModels Not Ported

| ViewModel | View | Purpose | Migration Status |
|-----------|------|---------|------------------|
| ☐ `ViewModel_Receiving_Workflow` | `View_Receiving_Workflow` | Main workflow orchestrator with step visibility logic | Replaced by `ViewModel_Receiving_Wizard_Orchestration_MainWorkflow` |
| ☐ `ViewModel_Receiving_ModeSelection` | `View_Receiving_ModeSelection` | Choose Guided/Manual/Edit mode | New has `ViewModel_Receiving_Hub_Display_ModeSelection` |
| ☐ `ViewModel_Receiving_POEntry` | `View_Receiving_POEntry` | PO number and part selection | New has `ViewModel_Receiving_Wizard_Display_PONumberEntry` |
| ☐ `ViewModel_Receiving_LoadEntry` | `View_Receiving_LoadEntry` | Number of loads entry | New has `ViewModel_Receiving_Wizard_Display_LoadCountEntry` |
| ☐ `ViewModel_Receiving_WeightQuantity` | `View_Receiving_WeightQuantity` | Weight/quantity per load | New has `ViewModel_Receiving_Wizard_Display_LoadDetailsGrid` (more comprehensive) |
| ☐ `ViewModel_Receiving_HeatLot` | `View_Receiving_HeatLot` | Heat/lot number per load | Likely merged into LoadDetailsGrid |
| ☐ `ViewModel_Receiving_PackageType` | `View_Receiving_PackageType` | Package type per load | Likely merged into LoadDetailsGrid |
| ☐ `ViewModel_Receiving_Review` | `View_Receiving_Review` | Review all loads before save | New has `ViewModel_Receiving_Wizard_Display_ReviewSummary` |
| ☐ `ViewModel_Receiving_ManualEntry` | `View_Receiving_ManualEntry` | Manual grid entry mode | **NOT PORTED** |
| ☐ `ViewModel_Receiving_EditMode` | `View_Receiving_EditMode` | Edit existing loads mode | **NOT PORTED** |

---

### 13.2 New ViewModels Not in Old

| ViewModel | Purpose | Notes |
|-----------|---------|-------|
| ✅ `ViewModel_Receiving_Wizard_Display_PartSelection` | Part lookup and selection | May be more sophisticated than old |
| ✅ `ViewModel_Receiving_Wizard_Display_LoadDetailsGrid` | Comprehensive load detail entry | Consolidates weight, heat/lot, package type in one grid |
| ✅ `ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations` | Bulk copy fields to multiple loads | New feature not in old version |
| ✅ `ViewModel_Receiving_Wizard_Dialog_CopyPreviewDialog` | Preview before bulk copy | New feature not in old version |
| ✅ `ViewModel_Receiving_Wizard_Orchestration_SaveOperation` | Save orchestration ViewModel | Dedicated save logic |
| ✅ `ViewModel_Receiving_Wizard_Display_CompletionScreen` | Completion screen | May replace old completion logic |

---

## 14. Bulk Copy Operations (New Feature)

| Feature | Status | Description |
|---------|--------|-------------|
| ✅ **Bulk Copy to Empty Loads** | New Feature | Copy field values (heat/lot, package type, etc.) to loads with empty fields |
| ✅ **Copy Preview Dialog** | New Feature | Show preview of which loads will be affected before applying |
| ✅ **Field Selection Enum** | New Feature | `Enum_Receiving_CopyType_FieldSelection` for choosing which fields to copy |
| ✅ **Copy Validation** | New Feature | `Validator_Receiving_Wizard_ValidateOn_CopyFieldsToEmptyLoads` |
| ✅ **Clear Auto-Filled Fields** | New Feature | `CommandHandler_Receiving_Wizard_Clear_AutoFilledFields` to undo bulk copy |

**Analysis:** This is a productivity enhancement NOT present in old version. Should be retained.

---

## 15. Database Schema Differences

### 15.1 Tables in Old MySQL Schema

| Table | Purpose | Status in New Schema |
|-------|---------|---------------------|
| ☐ `receiving_loads` | Main load data | Check SQL Server equivalent |
| ☐ `receiving_quality_holds` | Quality hold tracking | Unknown |
| ☐ `package_type_preferences` | Part-based package preferences | Unknown |
| ☐ `package_preferences` | User-based package preferences | Unknown |
| ☐ `user_settings` or equivalent | User default receiving mode | Unknown |

---

### 15.2 Stored Procedures to Verify

| Stored Procedure | Purpose | Status in New Database |
|------------------|---------|----------------------|
| ☐ `sp_Receiving_Line_Insert` | Insert receiving line | Check if exists in SQL Server |
| ☐ `sp_Receiving_QualityHolds_Insert` | Insert quality hold | Unknown |
| ☐ `sp_Receiving_QualityHolds_Update` | Update quality hold acknowledgment | Unknown |
| ☐ `sp_Receiving_QualityHolds_GetByLoadID` | Retrieve quality holds | Unknown |
| ☐ `sp_Receiving_PackageTypePreference_Get` | Get package preference | Unknown |
| ☐ `sp_Receiving_PackageTypePreference_Save` | Save package preference | Unknown |
| ☐ `sp_package_preferences_get_by_user` | Get user preference | Unknown |
| ☐ `sp_package_preferences_upsert` | Save user preference | Unknown |

---

## 16. Critical Missing Features Summary

### High Priority (Compliance/Safety)

| Priority | Feature | Impact | Estimated Effort |
|----------|---------|--------|------------------|
| ☐ **P0** | Quality Hold Detection and Warnings | **CRITICAL** - Compliance/safety requirement for MMFSR/MMCSR parts | Medium (2-3 handlers, 2 validators, 1 service, 2 dialogs) |
| ☐ **P0** | Quality Hold Database Tracking | **CRITICAL** - Audit trail for quality inspections | Medium (1 DAO, 2 handlers, 1 table) |
| ☐ **P0** | Quality Hold Blocking on Save | **CRITICAL** - Prevent save without acknowledgment | Small (1 validator enhancement) |

### Medium Priority (User Experience)

| Priority | Feature | Impact | Estimated Effort |
|----------|---------|--------|------------------|
| ☐ **P1** | Session Persistence and Restoration | High - Prevents data loss on crash | Medium (2 handlers, integration with startup) |
| ☐ **P1** | Package Type Preferences (Part-Based) | Medium - Reduces repetitive entry | Small-Medium (1 DAO, 2 handlers, 1 table) |
| ☐ **P2** | CSV Export (Local + Network) | Medium - Backup and reporting | Medium (1 service, 2 handlers, config) |
| ☐ **P2** | Multi-Destination Save Result Tracking | Medium - User needs to know partial failures | Small (1 result model, UI updates) |

### Low Priority (Nice-to-Have)

| Priority | Feature | Impact | Estimated Effort |
|----------|---------|--------|------------------|
| ☐ **P3** | Manual Entry Mode | Low - Wizard mode may be sufficient | Large (Full workflow mode implementation) |
| ☐ **P3** | Edit Mode | Low - Users may only create new entries | Large (Full workflow mode implementation) |
| ☐ **P3** | User Default Receiving Mode | Low - Saves one click per session | Small (1 preference field, routing logic) |
| ☐ **P3** | User Package Preferences | Low - Part-based preferences may be enough | Small (1 table, 2 handlers) |
| ☐ **P3** | Settings-Based UI Text | Low - Enables localization but not needed for MVP | Medium (Settings infrastructure) |
| ☐ **P3** | Contextual Help Integration | Low - Documentation can be external | Small (Help content generation) |

---

## 17. Recommendations

### Phase 1: Critical Features (Required for Production)

1. ☐ **Implement Quality Hold Workflow**
   - Create quality hold detection service
   - Add 2-step warning dialogs (Acknowledgment 1 of 2, Acknowledgment 2 of 2)
   - Add quality hold fields to load entity model
   - Create quality hold database table in SQL Server
   - Create insert/update/query handlers for quality holds
   - Add blocking validation to save transaction

2. ☐ **Session Persistence for Crash Recovery**
   - Create save session handler using Module_Core `IService_SessionManager`
   - Create load session query handler
   - Integrate with app startup lifecycle
   - Add session cleanup on successful save

3. ☐ **Verify All Validation Rules Ported**
   - Ensure PO format validation matches old version
   - Verify non-PO item support exists
   - Confirm heat/lot handling (required vs optional)
   - Validate field length limits match business rules

### Phase 2: User Experience Enhancements

1. ☐ **Package Type Preferences (Part-Based)**
   - Add package type preference table to SQL Server
   - Create DAO for preference CRUD
   - Create query handler to retrieve preference on part selection
   - Create command handler to save preference on transaction save
   - Auto-fill package type when part selected

2. ☐ **CSV Export with Dual Paths**
   - Create CSV export service (or use Module_Core if available)
   - Implement local `%APPDATA%` CSV export
   - Implement network path CSV export with graceful failure
   - Track multi-destination save results
   - Update completion screen to show CSV save status

3. ☐ **Enhanced Save Result Tracking**
   - Create `Model_Receiving_Result_SaveOperation` with database/local CSV/network CSV flags
   - Update save handler to populate result details
   - Update completion screen to show granular success/failure

### Phase 3: Optional Features (Future Consideration)

1. ☐ **Manual Entry Mode** (if business requirement)
   - Create manual entry ViewModel and View
   - Create grid-based load entry UI
   - Add workflow routing for manual mode
   - Bypass step-by-step wizard validation

2. ☐ **Edit Mode** (if business requirement)
   - Create edit mode ViewModel and View
   - Create query handler to load existing loads
   - Add update vs insert logic differentiation
   - Create delete load UI integration

3. ☐ **User Preferences and Customization**
   - Add user default receiving mode preference
   - Create settings UI for user preferences
   - Implement mode selection skip logic
   - Add user package type defaults

---

## 18. Migration Checklist

### Before Implementation

- ☐ Review this document with product owner to prioritize features
- ☐ Confirm which features are MVP requirements vs. nice-to-have
- ☐ Verify SQL Server schema includes all required tables (quality holds, preferences)
- ☐ Confirm network CSV path is still valid and accessible
- ☐ Validate user requirements for manual mode and edit mode

### During Implementation

- ☐ Create task files in `memory-bank/tasks/` for each selected feature
- ☐ Follow CQRS pattern: Create commands/queries, handlers, validators
- ☐ Use Module_Core services where available (SessionManager, CSVWriter, etc.)
- ☐ Add comprehensive validation using FluentValidation
- ☐ Write unit tests for handlers and validators
- ☐ Update memory bank documentation as features are completed

### After Implementation

- ☐ User acceptance testing for quality hold workflow
- ☐ Test session restoration after application crash
- ☐ Verify CSV export to both local and network paths
- ☐ Validate package preference auto-fill behavior
- ☐ Update user documentation with new features

---

## Appendix A: File Mappings

### Old Module Structure
```
Old_Module_Receiving/
├── Contracts/          (Interfaces for services)
├── Data/               (DAOs - MySQL specific)
├── Models/             (Entities, results, validation)
├── Services/           (Business logic services)
├── Settings/           (Settings keys and defaults)
├── ViewModels/         (MVVM ViewModels)
└── Views/              (XAML Views)
```

### New Module Structure
```
Module_Receiving/
├── Data/               (DAOs - SQL Server + MySQL)
├── Handlers/
│   ├── Commands/       (CQRS command handlers)
│   └── Queries/        (CQRS query handlers)
├── Models/
│   ├── Entities/       (Database entities)
│   ├── Enums/          (Enumerations)
│   └── DataTransferObjects/ (DTOs)
├── Requests/
│   ├── Commands/       (CQRS command requests)
│   └── Queries/        (CQRS query requests)
├── Validators/         (FluentValidation validators)
├── ViewModels/         (MVVM ViewModels - organized by mode)
└── Views/              (XAML Views - organized by mode)
```

---

## Appendix B: Key Differences in Architecture

| Aspect | Old Module | New Module |
|--------|-----------|-----------|
| **Pattern** | Service-oriented MVVM | CQRS with MediatR |
| **Validation** | Service methods returning validation results | FluentValidation validators |
| **Data Access** | Direct DAO calls from services | Handlers coordinate DAOs |
| **Error Handling** | Exceptions + try-catch | Result objects with error messages |
| **Workflow** | State machine in service | Orchestration ViewModels + handlers |
| **Database** | MySQL only | SQL Server (primary) + MySQL (legacy) |
| **Testing** | Unknown | Validators and handlers are testable |

---

**End of Comparison Document**

*This document should be reviewed with stakeholders to determine which checkboxes to implement.*

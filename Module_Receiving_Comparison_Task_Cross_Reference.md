# Module_Receiving vs Old_Module_Receiving - Task Cross-Reference

**Generated:** 2025-01-30  
**Purpose:** Map features from Old_Module_Receiving to planned implementation tasks in Module_Receiving

---

## Summary of Findings

After reviewing all 8 phase task files (tasks_phase1.md through tasks_phase8.md), here are the mappings between old module features and new module implementation plans:

---

## ? Features ALREADY IMPLEMENTED (Phases 1-3 Complete)

### Workflow & Session Management
| Old Feature | New Implementation | Status | Phase/Task |
|-------------|-------------------|--------|------------|
| Session persistence to JSON | `CommandRequest_Receiving_Shared_Save_WorkflowSession` | ? Complete | Phase 3, Task 3.2 |
| Session restoration on startup | `QueryRequest_Receiving_Shared_Get_WorkflowSession` | ? Complete | Phase 3, Task 3.3 |
| Workflow session entity model | `Model_Receiving_Entity_WorkflowSession` | ? Complete | Phase 1, Task 1.19 |
| Transaction save operation | `CommandRequest_Receiving_Shared_Save_Transaction` | ? Complete | Phase 3, Task 3.1 |
| Line update operation | `CommandRequest_Receiving_Shared_Update_ReceivingLine` | ? Complete | Phase 3, Task 3.6 |
| Line delete operation | `CommandRequest_Receiving_Shared_Delete_ReceivingLine` | ? Complete | Phase 3, Task 3.7 |

### Database Infrastructure
| Old Feature | New Implementation | Status | Phase/Task |
|-------------|-------------------|--------|------------|
| Transaction table | `tbl_Receiving_Transaction` | ? Complete | Phase 2, Task 2.5 |
| Line table | `tbl_Receiving_Line` | ? Complete | Phase 2, Task 2.6 |
| Workflow session table | `tbl_Receiving_WorkflowSession` | ? Complete | Phase 2, Task 2.7 |
| Part preference table | `tbl_Receiving_PartPreference` | ? Complete | Phase 2, Task 2.8 |
| Audit log table | `tbl_Receiving_AuditLog` | ? Complete | Phase 2, Task 2.9 |
| Transaction DAO | `Dao_Receiving_Repository_Transaction` | ? Complete | Phase 2, Task 2.51 |
| Line DAO | `Dao_Receiving_Repository_Line` | ? Complete | Phase 2, Task 2.52 |
| Part preference DAO | `Dao_Receiving_Repository_PartPreference` | ? Complete | Phase 2, Task 2.54 |
| All 29 stored procedures | Complete set | ? Complete | Phase 2, Tasks 2.11-2.39 |

### Validation Infrastructure
| Old Feature | New Implementation | Status | Phase/Task |
|-------------|-------------------|--------|------------|
| Transaction validation | `Validator_Receiving_Shared_ValidateOn_SaveTransaction` | ? Complete | Phase 3 |
| Line update validation | `Validator_Receiving_Shared_ValidateOn_UpdateReceivingLine` | ? Complete | Phase 3 |
| PO format validation | `Validator_Receiving_Shared_ValidateIf_ValidPOFormat` | ? Complete | Phase 3 |
| Copy fields validation | `Validator_Receiving_Wizard_ValidateOn_CopyFieldsToEmptyLoads` | ? Complete | Phase 3 |

---

## ? Features PLANNED & IN PROGRESS (Phases 4-8)

### Workflow ViewModels & Views
| Old Feature | New Implementation | Status | Phase/Task |
|-------------|-------------------|--------|------------|
| Workflow orchestration | `ViewModel_Receiving_Wizard_Orchestration_MainWorkflow` | ? Complete (ViewModels done) | Phase 4, Task 4.3 |
| Mode selection | `ViewModel_Receiving_Hub_Display_ModeSelection` | ? Complete (ViewModels done) | Phase 4, Task 4.2 |
| PO number entry | `ViewModel_Receiving_Wizard_Display_PONumberEntry` | ? Complete (ViewModels done) | Phase 4, Task 4.4 |
| Part selection | `ViewModel_Receiving_Wizard_Display_PartSelection` | ? Complete (ViewModels done) | Phase 4, Task 4.5 |
| Load count entry | `ViewModel_Receiving_Wizard_Display_LoadCountEntry` | ? Complete (ViewModels done) | Phase 4, Task 4.6 |
| Load details grid | `ViewModel_Receiving_Wizard_Display_LoadDetailsGrid` | ? Complete (ViewModels done) | Phase 4, Task 4.7 |
| Bulk copy operations | `ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations` | ? Complete (ViewModels done) | Phase 4, Task 4.8 |
| Review summary | `ViewModel_Receiving_Wizard_Display_ReviewSummary` | ? Complete (ViewModels done) | Phase 4, Task 4.10 |
| Save operation orchestration | `ViewModel_Receiving_Wizard_Orchestration_SaveOperation` | ? Complete (ViewModels done) | Phase 4, Task 4.11 |
| Completion screen | `ViewModel_Receiving_Wizard_Display_CompletionScreen` | ? Complete (ViewModels done) | Phase 4, Task 4.12 |
| All Wizard Views (XAML) | ~24 views | ? Pending | Phase 6 |

### Manual Entry Mode
| Old Feature | New Implementation | Status | Phase/Task |
|-------------|-------------------|--------|------------|
| Manual entry ViewModels | Manual mode ViewModels | ? Pending | Phase 5 |
| Manual entry Views | Manual mode Views | ? Pending | Phase 6 |
| Direct grid entry | Part of manual mode | ? Pending | Phase 5 |

### Edit Mode
| Old Feature | New Implementation | Status | Phase/Task |
|-------------|-------------------|--------|------------|
| Edit mode ViewModels | Edit mode ViewModels | ? Pending | Phase 5 |
| Edit mode Views | Edit mode Views | ? Pending | Phase 6 |
| Load existing records | Query handlers for edit mode | ? Pending | Phase 5 |

### Package Type Preferences
| Old Feature | New Implementation | Status | Phase/Task |
|-------------|-------------------|--------|------------|
| Part-based package preferences | `tbl_Receiving_PartPreference` + DAO | ? Complete | Phase 2, Tasks 2.8 + 2.54 |
| Save/load preferences | `sp_Receiving_PartPreference_SelectByPart`, `sp_Receiving_PartPreference_Upsert` | ? Complete | Phase 2, Tasks 2.32-2.33 |
| Auto-fill package type on part selection | Part of ViewModel logic | ? Pending | Phase 4 enhancement |

---

## ?? Features NOT YET PLANNED (Require New Tasks)

### Quality Hold Management (CRITICAL P0)
| Old Feature | New Implementation Needed | Priority | Notes |
|-------------|---------------------------|----------|-------|
| Quality hold detection (MMFSR/MMCSR) | **NOT PLANNED** - Need new service | **P0 CRITICAL** | Old: `Service_QualityHoldWarning.IsRestrictedPart()` |
| Quality hold warning dialog (1 of 2) | **NOT PLANNED** - Need dialog | **P0 CRITICAL** | Old: `Service_QualityHoldWarning.CheckAndWarnAsync()` |
| Quality hold confirmation (2 of 2) | **NOT PLANNED** - Need pre-save check | **P0 CRITICAL** | Old: Blocking validation before save |
| Quality hold database table | **NOT PLANNED** - Need table schema | **P0 CRITICAL** | Old: `receiving_quality_holds` table |
| Quality hold DAO | **NOT PLANNED** - Need DAO | **P0 CRITICAL** | Old: `Dao_QualityHold` |
| Quality hold insert/update | **NOT PLANNED** - Need stored procedures | **P0 CRITICAL** | Old: `sp_Receiving_QualityHolds_Insert` |
| Quality hold tracking fields | **NOT PLANNED** - Add to entity model | **P0 CRITICAL** | Old: `IsQualityHoldRequired`, `IsQualityHoldAcknowledged`, `QualityHoldRestrictionType` on load model |

**RECOMMENDATION:** Add Quality Hold as Phase 9 (urgent) or integrate into Phase 7 testing.

### CSV Export Functionality (P1 HIGH)
| Old Feature | New Implementation Needed | Priority | Notes |
|-------------|---------------------------|----------|-------|
| Local CSV export (`%APPDATA%`) | **NOT PLANNED** - Need service | **P1 HIGH** | Old: `Service_CSVWriter.WriteToFileAsync()` |
| Network CSV export (dual-path) | **NOT PLANNED** - Need network writer | **P1 HIGH** | Old: Network path + user folder creation |
| CSV result tracking | **NOT PLANNED** - Need result model | **P1 HIGH** | Old: `Model_CSVWriteResult` with `LocalSuccess`, `NetworkSuccess` |
| Multi-destination save tracking | **NOT PLANNED** - Need save result | **P1 HIGH** | Old: `Model_SaveResult` with DB + local CSV + network CSV flags |
| CSV export on completion | **NOT PLANNED** - Add to save handler | **P1 HIGH** | Old: Part of save orchestration |

**NOTE:** `IService_CSVWriter` interface exists in Module_Core, but Receiving module implementation not found.

**RECOMMENDATION:** Add CSV Export as Phase 10 or integrate into Phase 7/8.

### User Preferences & Settings (P2 MEDIUM)
| Old Feature | New Implementation Needed | Priority | Notes |
|-------------|---------------------------|----------|-------|
| User default receiving mode | **NOT PLANNED** - Need user settings | **P2 MEDIUM** | Old: `User.DefaultReceivingMode` stored in user profile |
| Skip mode selection if default set | **NOT PLANNED** - Need hub routing logic | **P2 MEDIUM** | Old: Workflow startup checks user preference |
| User-based package type preferences | **NOT PLANNED** - Need user prefs table | **P3 LOW** | Old: `package_preferences` table with username/workstation |
| Workstation-specific defaults | **NOT PLANNED** - Need workstation tracking | **P3 LOW** | Old: Per-workstation package type defaults |

**NOTE:** Phase 7 includes Module_Settings.Receiving infrastructure, but not user-specific receiving preferences.

**RECOMMENDATION:** User receiving mode preference could be added to Phase 7 Settings implementation.

### Session Management Enhancements (P2 MEDIUM)
| Old Feature | New Implementation Needed | Priority | Notes |
|-------------|---------------------------|----------|-------|
| Session cleanup on save success | **?? ENHANCEMENT NEEDED** | **P2 MEDIUM** | Add to Phase 3 save handler or Phase 7 integration |
| Auto-restore session on startup | **?? INTEGRATION NEEDED** | **P2 MEDIUM** | Phase 3 query exists, needs startup lifecycle integration (Phase 7) |
| Session expiration logic | **NOT IN OLD VERSION** | **P3 LOW** | Enhancement opportunity (not in old version) |

**NOTE:** Core session persistence handlers are complete (Phase 3), but integration touchpoints need Phase 7 work.

### Validation Enhancements (P2 MEDIUM)
| Old Feature | New Implementation Needed | Priority | Notes |
|-------------|---------------------------|----------|-------|
| Auto-set PackagesPerLoad to 1 on Part entry | **NOT PLANNED** - Add to ViewModel | **P2 MEDIUM** | Old: `OnPartIDChanged` property trigger |
| Auto-set Package Type by part prefix | **NOT PLANNED** - Add to ViewModel | **P2 MEDIUM** | Old: MMC ? Coil, MMF ? Sheet auto-detection |
| Heat/Lot "Nothing Entered" placeholder | **NOT PLANNED** - Add to save logic | **P2 MEDIUM** | Old: Allows blank heat/lot, saves as "Nothing Entered" |
| Weight per package calculation | **NOT PLANNED** - Add calculated field | **P3 LOW** | Old: `WeightPerPackage = WeightQuantity / PackagesPerLoad` |

**RECOMMENDATION:** Add auto-default logic to Phase 4 ViewModels or Phase 6 validation.

### Help System Integration (P3 LOW)
| Old Feature | New Implementation Needed | Priority | Notes |
|-------------|---------------------------|----------|-------|
| Per-step help content | **NOT PLANNED** - Add to orchestration | **P3 LOW** | Old: `_stepTitles` dictionary + help panel |
| Help button integration | **NOT PLANNED** - Add to views | **P3 LOW** | `IService_Help` exists in Module_Core |
| Dynamic help content generation | **NOT PLANNED** - Content generation | **P3 LOW** | Old: Help changes per workflow step |

**NOTE:** `IService_Help` and `Helper_WorkflowHelpContentGenerator` exist in Module_Core.

**RECOMMENDATION:** Add help integration to Phase 6 Views or Phase 7 Integration.

### Settings Localization (P3 LOW)
| Old Feature | New Implementation Needed | Priority | Notes |
|-------------|---------------------------|----------|-------|
| Settings keys for UI text | **NOT PLANNED** - Settings schema | **P3 LOW** | Old: `ReceivingSettingsKeys` constants |
| Dynamic UI text from settings | **NOT PLANNED** - Settings binding | **P3 LOW** | Old: `WorkflowNextText`, `CompletionSuccessTitleText` loaded from settings |
| Settings service abstraction | **PARTIALLY PLANNED** | **P3 LOW** | Phase 7 includes settings infrastructure, but not UI text localization |

**NOTE:** Phase 7 includes `Module_Settings.Receiving` but scope is different (109 functional settings, not UI localization).

---

## ?? Comparison Document Section Updates

Based on phase file analysis, the comparison document should be updated with these cross-references:

### Section 1: Workflow and State Management
- **Session Persistence:** ? Planned Phase 3 (complete)
- **Auto-Restore on Startup:** ? Query exists (Phase 3), needs integration (Phase 7)
- **Session Cleanup:** ?? Enhancement needed in Phase 3 handler
- **Step Transition Validation:** ? Planned Phase 4 Task 4.3
- **Back Navigation:** ? Planned Phase 4 Task 4.3
- **Conditional Step Skipping:** ? NOT PLANNED (would need Phase 4/7 addition)

### Section 2: Data Entry Modes
- **Manual Entry Mode:** ? Planned Phase 5
- **Edit Mode:** ? Planned Phase 5
- **Wizard Mode:** ? Phase 4 ViewModels complete, Phase 6 Views pending

### Section 3: Quality Hold Management
- **All Quality Hold Features:** ? NOT PLANNED - **CRITICAL P0 GAP**

### Section 4: Package Type Preferences
- **Part-Based Preferences:** ? Planned Phase 2 (complete - table + DAO exist)
- **User-Based Preferences:** ? NOT PLANNED - P3 LOW

### Section 5: Validation Rules
- **PO Validation:** ? Planned Phase 3 (validators exist)
- **Load Validation:** ? Planned Phase 3 (validators exist)
- **Auto-Defaults:** ? NOT PLANNED - P2 MEDIUM

### Section 6: CSV Export
- **All CSV Features:** ? NOT PLANNED - **P1 HIGH GAP**

### Section 7: User Preferences
- **Default Receiving Mode:** ? NOT PLANNED - P2 MEDIUM

### Section 8: Help System
- **Contextual Help:** ? NOT PLANNED - P3 LOW (Core service exists)

### Section 9: Save Orchestration
- **Multi-Destination Save:** ? NOT PLANNED - P1 HIGH (tied to CSV export)

### Sections 10-16: Technical Details
- Most enums, models, DAOs, ViewModels, Views are planned or complete
- Some old models (QualityHold, UserPreference, CSVWriteResult, SaveResult) are NOT in new architecture

---

## ?? Recommended Actions

### Immediate (Before Phase 6)
1. ? **Add Quality Hold as Phase 9** - CRITICAL P0 compliance feature
   - Create quality hold table schema
   - Create quality hold DAO + stored procedures
   - Create quality hold detection service
   - Add quality hold warning dialogs to Phase 6 Views
   - Add quality hold validators to Phase 3
   - Add quality hold fields to `Model_Receiving_Entity_ReceivingLoad`

2. ? **Add CSV Export as Phase 10** - HIGH P1 business requirement
   - Implement `IService_CSVWriter` for Module_Receiving
   - Add local CSV export handler
   - Add network CSV export handler
   - Create CSV result models
   - Integrate with Phase 4 save operation

### Short-Term (Phase 7 Integration)
3. ? **Session Management Integration**
   - Add session cleanup to save handler
   - Add startup session restoration to `Service_OnStartup_AppLifecycle`

4. ? **Auto-Default Enhancements**
   - Add PackagesPerLoad = 1 default on Part entry (Phase 4 ViewModels)
   - Add Package Type auto-detection (MMC/MMF logic)
   - Add Heat/Lot "Nothing Entered" placeholder on save

5. ? **User Preference for Default Mode**
   - Add to Phase 7 Settings implementation
   - Add hub routing logic to skip mode selection

### Long-Term (Future Enhancements)
6. ? **Help System Integration** (P3 LOW)
   - Add contextual help to Phase 6 Views
   - Leverage existing `IService_Help` from Module_Core

7. ? **Settings Localization** (P3 LOW)
   - Add UI text to Phase 7 settings schema
   - Enable localization for future internationalization

---

## ?? Implementation Status Summary

| Category | Implemented | Planned | Not Planned | Gap Severity |
|----------|-------------|---------|-------------|--------------|
| Database & DAOs | 100% | 0% | 0% | ? None |
| CQRS Handlers & Validators | 100% | 0% | 0% | ? None |
| Wizard ViewModels | 100% | 0% | 0% | ? None |
| Wizard Views | 0% | 100% | 0% | ? Pending |
| Manual/Edit Modes | 0% | 100% | 0% | ? Pending |
| **Quality Hold Management** | **0%** | **0%** | **100%** | ?? **CRITICAL** |
| **CSV Export** | **0%** | **0%** | **100%** | ?? **HIGH** |
| Package Preferences | 100% | 0% | 0% | ? None |
| Session Management | 90% | 10% | 0% | ?? Integration needed |
| User Preferences | 0% | 0% | 100% | ?? MEDIUM |
| Help System | 0% | 0% | 100% | ?? LOW |
| Settings Localization | 0% | 80% | 20% | ?? LOW |

**Overall Assessment:**
- ? **Core Architecture:** Excellent - CQRS, database, validation complete
- ?? **CRITICAL GAP:** Quality Hold Management (compliance requirement)
- ?? **HIGH GAP:** CSV Export (business requirement)
- ?? **Minor Gaps:** User preferences, help system, auto-defaults

---

**End of Cross-Reference Document**

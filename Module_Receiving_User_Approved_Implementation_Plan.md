# User-Approved Feature Implementation Plan

**Generated:** 2025-01-30  
**Based On:** Module_Receiving_Feature_Cherry_Pick.md user selections  
**Status:** APPROVED FOR IMPLEMENTATION

---

## Executive Summary

User has selected 8 out of 9 proposed features for implementation, with specific modifications to 4 features. Total estimated effort: **69-88 hours**.

### Features APPROVED

1. ? **P0 Quality Hold Management** (33-40 hours) - Full implementation
2. ? **P1 CSV Export** (10-12 hours) - Modified: Single path only
3. ? **P1 Package Type Preferences** (3-5 hours) - As specified
4. ? **P2 Auto-Defaults** (3-5 hours) - Modified: Editable weight calculation
5. ? **P2 Session Management** (4-5 hours) - Modified: Add reset button
6. ? **P2 User Preferences** (5-7 hours) - Modified: Add return-to-mode button
7. ? **P2 Validation on LostFocus** (4-6 hours) - As specified
8. ? **P3 Help System** (6-10 hours) - As specified

### Feature REJECTED

9. ? **P3 Settings Localization** - DO NOT IMPLEMENT per user request

---

## Critical User Requirements (UPDATES)

### UPDATE 1: CSV Export Path

**Original Specification:** Dual-path (local + network) with fallback  
**User Requirement:** Single configurable CSV path only

**Details:**
- User sets CSV export path via settings menu
- Single destination (not local + network)
- Path stored in `tbl_Settings_Receiving` table with key `CSVExportPath`
- CSV export is for printing purposes only
- Blocking behavior: Save fails if CSV export fails (user selected blocking option)
- Enhanced CSV format with all fields

**Implementation Impact:**
- Simplifies Service_Receiving_CSVExport (no dual-path logic)
- Reduces estimated time from 13-15 hours to 10-12 hours
- Completion screen shows single CSV path status (not local + network)
- Settings menu needs CSV path configuration UI

---

### UPDATE 2: Weight Per Package Calculation

**Original Specification:** Display only (UI-only calculation), no entity field  
**User Requirement:** Calculated once but user can edit/override

**Details:**
- Initial calculation: `WeightPerPackage = Weight / PackagesPerLoad`
- Calculation happens automatically when user enters Weight or PackagesPerLoad
- User can manually override the calculated value
- Overridden value is saved to database
- Entity model MUST have `WeightPerPackage` field (decimal, nullable)

**Implementation Impact:**
- Add `WeightPerPackage` property to `Model_Receiving_Entity_ReceivingLoad`
- ViewModel has calculated property that updates field
- XAML TextBox is editable (not read-only)
- User override prevents auto-recalculation

---

### UPDATE 3: Session Management - Reset Button

**Original Specification:** Auto-clear session on save, auto-resume on startup  
**User Requirement:** Add "Reset Session" button to clear current session

**Details:**
- "Reset Session" button accessible in wizard UI
- Clears current module session only (not all modules)
- Prompts user for confirmation before resetting
- Session file: `%APPDATA%\MTM_Receiving_Application\Receiving_session.json`
- Button should be in wizard orchestration view (always accessible)

**Implementation Impact:**
- Add `ResetSessionCommand` to orchestration ViewModel
- Add confirmation dialog before reset
- Clear session file and reset ViewModel state
- Estimated time increases from 3-4 hours to 4-5 hours

---

### UPDATE 4: User Preferences - Return to Mode Selection Button

**Original Specification:** Skip mode selection if preference set  
**User Requirement:** MUST have button to return to mode selection at all times

**Details:**
- Each of the 3 receiving modes (Wizard, Manual, Edit) MUST have a button
- Button accessible at all times (persistent in UI)
- Returns user to mode selection screen
- Works even if user has set a preference to skip mode selection
- Allows user to switch modes mid-workflow (with confirmation if data exists)

**Implementation Impact:**
- Add "Change Mode" or "Return to Mode Selection" button to all mode views
- Button in wizard orchestration, manual entry, and edit mode layouts
- Confirmation dialog if current session has unsaved data
- Navigate to `View_Receiving_Hub_Display_ModeSelection`
- Estimated time increases from 4-6 hours to 5-7 hours

---

### UPDATE 5: Settings Localization

**Original Specification:** Optional P3 LOW feature  
**User Requirement:** DO NOT IMPLEMENT THIS FEATURE

**Details:**
- No internationalization/localization needed
- All UI text will be hardcoded strings in English
- No .resx resource files
- No database storage for UI text
- This is a permanent decision (not "skip for MVP")

**Implementation Impact:**
- Remove all localization tasks from phase plans
- No time allocated
- No files created for this feature

---

## Phase-by-Phase Implementation Plan

### Phase 2: Database & DAOs (Tasks 2.59-2.65) ? APPROVED

**Quality Hold Infrastructure:**

1. ? Task 2.59: Create `tbl_Receiving_QualityHold` table
   - Full audit trail (all acknowledgment fields)
   - User + Final acknowledgment tracking
   - Quality inspector sign-off fields (future)

2. ? Task 2.60: Create `sp_Receiving_QualityHold_Insert`
   - Initial insert with first acknowledgment
   - Returns QualityHoldID

3. ? Task 2.61: Create `sp_Receiving_QualityHold_UpdateFinalAcknowledgment`
   - Updates final acknowledgment flag

4. ? Task 2.62: Create `sp_Receiving_QualityHold_SelectByLineID`
   - Retrieves all quality holds for a line

5. ? Task 2.63: Create `Model_Receiving_Entity_QualityHold`
   - Entity model with all audit trail fields
   - Computed properties for status checks

6. ? Task 2.64: Create `Dao_Receiving_Repository_QualityHold`
   - Instance-based DAO
   - Insert, UpdateFinalAcknowledgment, SelectByLineId methods

7. ? Task 2.65: Add Quality Hold Fields to `Model_Receiving_Entity_ReceivingLoad`
   - IsQualityHoldRequired
   - QualityHoldRestrictionType
   - UserAcknowledgedQualityHold
   - FinalAcknowledgedQualityHold

**Additional Database Tasks for User-Approved Features:**

8. ? Task 2.66: Add `WeightPerPackage` field to `tbl_Receiving_Line`
   - Column: `WeightPerPackage DECIMAL(18,2) NULL`
   - Allow user override of calculated value

9. ? Task 2.67: Add CSV Export Path setting to `tbl_Receiving_Settings`
   - Seed row: `SettingKey = 'CSVExportPath'`, `SettingValue = 'C:\MTM_CSV\'`

10. ? Task 2.68: Verify `PackagesPerLoad` default in `tbl_Receiving_Line`
    - Column: `PackagesPerLoad INT NOT NULL DEFAULT 1`
    - Entity model default value: 1

---

### Phase 3: CQRS Handlers & Validators (8 tasks) ? APPROVED

**Quality Hold Commands/Queries:**

1. ? Task 3.XX: Create `CommandRequest_Receiving_Shared_Create_QualityHold`
   - Command to create quality hold record
   - Returns QualityHoldID

2. ? Task 3.XX: Create `CommandHandler_Receiving_Shared_Create_QualityHold`
   - Maps request to entity
   - Calls DAO insert method

3. ? Task 3.XX: Create `QueryRequest_Receiving_Shared_Get_QualityHoldsByLine`
   - Query to retrieve quality holds for a line

4. ? Task 3.XX: Create `QueryHandler_Receiving_Shared_Get_QualityHoldsByLine`
   - Calls DAO select method

5. ? Task 3.XX: Create `Validator_Receiving_Shared_ValidateIf_RestrictedPart`
   - Configurable part pattern validation (not hardcoded MMFSR/MMCSR)
   - ?? USER REQUIREMENT: Use configurable patterns, not hardcoded

6. ? Task 3.XX: Enhance `Validator_Receiving_Shared_ValidateOn_SaveTransaction`
   - Add quality hold blocking validation
   - Hard block if quality hold not fully acknowledged

**CSV Export Commands:**

7. ? Task 3.XX: Create `CommandRequest_Receiving_Shared_Export_TransactionToCSV`
   - Single CSV path (from settings)
   - ?? USER REQUIREMENT: No dual-path, single configurable destination

8. ? Task 3.XX: Create `Model_Receiving_Result_CSVExport`
   - Single path tracking (not local + network)
   - Success/failure status
   - Error message
   - Records exported count

---

### Phase 4: ViewModels (12 tasks) ? APPROVED

**Quality Hold Service & Integration:**

1. ? Task 4.XX: Create `Service_Receiving_QualityHoldDetection`
   - Configurable part pattern detection (not hardcoded)
   - Two-step warning dialogs (Acknowledgment 1 of 2, 2 of 2)
   - Uses `IService_Window` for XamlRoot

2. ? Task 4.XX: Enhance `ViewModel_Receiving_Wizard_Display_PartSelection`
   - Quality hold detection on part selection
   - Show first warning dialog
   - Set quality hold flags

3. ? Task 4.XX: Enhance `ViewModel_Receiving_Wizard_Orchestration_SaveOperation`
   - Final quality hold validation before save
   - Show second warning dialog
   - Create quality hold records after save
   - ?? USER REQUIREMENT: Hard block (cannot save without acknowledgment)

**CSV Export Service & Integration:**

4. ? Task 4.XX: Create `Service_Receiving_CSVExport`
   - Single configurable CSV path (from settings)
   - Enhanced CSV format with all fields
   - ?? USER REQUIREMENT: No local+network dual-path

5. ? Task 4.XX: Integrate CSV Export into Save Operation
   - Automatic export on every save
   - Blocking behavior: Save fails if CSV fails
   - ?? USER REQUIREMENT: Blocking mode (not non-blocking)

**Auto-Defaults Implementation:**

6. ? Task 4.XX: Add Auto-Defaults to `ViewModel_Receiving_Wizard_Display_LoadDetailsGrid`
   - PackagesPerLoad always defaults to 1 (entity model default)
   - Auto-detect package type (MMC=Coil, MMF=Sheet)
   - Weight per package calculated initially, user can override
   - ?? USER REQUIREMENT: Editable weight per package (not display-only)

7. ? Task 4.XX: Add Heat/Lot Placeholder to Save Operation
   - Replace blank with "Nothing Entered" on save

**Session Management Enhancements:**

8. ? Task 4.XX: Add Session Cleanup to Save Operation
   - Auto-clear session file after successful save
   - Uses `IService_SessionManager` from Module_Core

9. ? Task 4.XX: Add Reset Session Command to Orchestration ViewModel
   - "Reset Session" button in wizard UI
   - Confirmation dialog before reset
   - Clears current module session only
   - ?? USER REQUIREMENT: Reset button for current module

**Package Preferences Integration:**

10. ? Task 4.XX: Add Package Preference Auto-Fill to Part Selection
    - Auto-fill package type on part selection (silent)
    - User can override

11. ? Task 4.XX: Add Package Preference Save on Explicit Change
    - Save preference only when user explicitly changes
    - Prompt to update preference on override
    - ?? USER REQUIREMENT: Explicit save only, with prompt

**User Preferences - Return to Mode Selection:**

12. ? Task 4.XX: Add "Return to Mode Selection" Button to All Modes
    - Button in wizard orchestration ViewModel
    - Button in manual entry ViewModel (when implemented)
    - Button in edit mode ViewModel (when implemented)
    - Confirmation if unsaved data exists
    - ?? USER REQUIREMENT: Must be accessible at all times in all 3 modes

---

### Phase 6: Views (XAML) (8 tasks) ? APPROVED

**CSV Export Results UI:**

1. ? Task 6.XX: Add CSV Export Results to Completion Screen XAML
   - Single CSV path display (not local + network)
   - Status indicator
   - ?? USER REQUIREMENT: Modified to show single path only

2. ? Task 6.XX: Add "Open CSV Folder" Command to Completion ViewModel
   - Single button (not local + network buttons)
   - Opens folder containing CSV file

**Validation on LostFocus:**

3. ? Task 6.XX: Add LostFocus Validation to PO Number Entry View
   - TextBox has LostFocus event handler
   - No UpdateSourceTrigger=PropertyChanged
   - Inline error message display

4. ? Task 6.XX: Add LostFocus Validation to All Step 2 Load Details Grid Fields
   - Weight/Quantity, Heat/Lot, Packages Per Load TextBoxes
   - All editable DataGrid columns
   - Inline error messages

**Session Management UI:**

5. ? Task 6.XX: Add "Reset Session" Button to Wizard Orchestration View
   - Button in wizard toolbar/command bar
   - Always accessible
   - Confirmation dialog on click
   - ?? USER REQUIREMENT: Module-specific reset button

**User Preferences UI:**

6. ? Task 6.XX: Add "Return to Mode Selection" Button to Wizard View
   - Persistent button in wizard UI
   - Always accessible
   - Confirmation if unsaved data
   - ?? USER REQUIREMENT: Must be in all 3 mode views

7. ? Task 6.XX: Add "Return to Mode Selection" Button to Manual Entry View
   - Same as above for manual entry mode

8. ? Task 6.XX: Add "Return to Mode Selection" Button to Edit Mode View
   - Same as above for edit mode

---

### Phase 7: Integration & Testing (10 tasks) ? APPROVED

**DI Registration:**

1. ? Task 7.XX: Register Quality Hold Service and CSV Export Service
   - Quality hold detection service as Singleton
   - CSV export service as Singleton
   - Quality hold DAO as Singleton

**Session Management Integration:**

2. ? Task 7.XX: Integrate Session Restoration into App Startup
   - Auto-resume without prompt
   - No expiration check
   - ?? USER REQUIREMENT: Auto-resume (not prompt)

**User Preferences Integration:**

3. ? Task 7.XX: Add User Preference Database Table
   - `tbl_Settings_Receiving_UserPreferences`
   - Username, DefaultReceivingMode fields

4. ? Task 7.XX: Create User Preference DAO
   - Get/Upsert methods for user preferences

5. ? Task 7.XX: Add User Preference Check to Hub Orchestration
   - Skip mode selection if preference set
   - Direct navigation to preferred mode
   - ?? USER REQUIREMENT: Skip logic, but return button always available

**CSV Export Settings Integration:**

6. ? Task 7.XX: Add CSV Path Configuration to Settings Menu
   - Settings page in Module_Settings.Receiving
   - User can set/change CSV export path
   - Default path: `C:\MTM_CSV\`
   - ?? USER REQUIREMENT: User-configurable path via settings menu

**Help System Integration:**

7. ? Task 7.XX: Create Contextual Help Content for Each Wizard Step
   - Help content for Step 1 (PO/Part selection)
   - Help content for Step 2 (Load details)
   - Help content for Step 3 (Review/Save)

8. ? Task 7.XX: Add Expandable Help Panel to Each View
   - Uses `IService_Help` from Module_Core
   - Expandable panel in wizard views
   - Contextual content per step
   - ?? USER REQUIREMENT: Expandable panel (not dialog)

**Validation Integration:**

9. ? Task 7.XX: Verify All LostFocus Validation Handlers
   - All textboxes validate on LostFocus only
   - Comprehensive validation (format + business rules)
   - Inline error messages below fields
   - ?? USER REQUIREMENT: All fields, comprehensive validation

**Package Preferences Integration:**

10. ? Task 7.XX: Test Package Preference Auto-Fill and Save Logic
    - Auto-fill on part selection works
    - Save only on explicit user change
    - Prompt to update preference on override

---

### Phase 8: Testing (2 tasks) ? APPROVED

1. ? Task 8.XX: Create Quality Hold End-to-End Integration Test
   - Test configurable part pattern detection
   - Test two-step acknowledgment workflow
   - Test hard block on save without acknowledgment
   - Test quality hold record creation
   - ?? USER REQUIREMENT: Test configurable patterns, not hardcoded

2. ? Task 8.XX: Create CSV Export End-to-End Integration Test
   - Test single configurable path export
   - Test automatic export on save
   - Test blocking behavior (save fails if CSV fails)
   - Test enhanced CSV format
   - ?? USER REQUIREMENT: Test single path, blocking behavior

---

## Implementation Checklist by Priority

### Phase 1 - MUST IMPLEMENT (P0)

- [ ] Quality Hold database infrastructure (7 tasks, Phase 2)
- [ ] Quality Hold CQRS commands/queries/validators (6 tasks, Phase 3)
- [ ] Quality Hold detection service (1 task, Phase 4)
- [ ] Quality Hold ViewModel integration (2 tasks, Phase 4)
- [ ] Quality Hold DI registration (1 task, Phase 7)
- [ ] Quality Hold testing (1 task, Phase 8)

**Total:** 18 tasks, 33-40 hours

### Phase 2 - STRONGLY RECOMMENDED (P1)

- [ ] CSV Export result model (1 task, Phase 3)
- [ ] CSV Export service with single path (1 task, Phase 4)
- [ ] CSV Export integration into save (1 task, Phase 4)
- [ ] CSV Export results UI (2 tasks, Phase 6)
- [ ] CSV Export settings integration (1 task, Phase 7)
- [ ] CSV Export DI registration (included in Task 7.XX)
- [ ] CSV Export testing (1 task, Phase 8)
- [ ] Package preferences auto-fill (2 tasks, Phase 4)
- [ ] Package preferences testing (1 task, Phase 7)

**Total:** 10 tasks, 16-20 hours

### Phase 3 - RECOMMENDED (P2)

- [ ] Auto-defaults implementation (2 tasks, Phase 4)
- [ ] Auto-defaults database fields (2 tasks, Phase 2)
- [ ] Session management enhancements (2 tasks, Phase 4)
- [ ] Session management UI (1 task, Phase 6)
- [ ] Session management integration (1 task, Phase 7)
- [ ] User preferences database (2 tasks, Phase 7)
- [ ] User preferences UI buttons (3 tasks, Phase 6)
- [ ] User preferences integration (1 task, Phase 7)
- [ ] Validation on LostFocus (2 tasks, Phase 6)
- [ ] Validation integration testing (1 task, Phase 7)

**Total:** 17 tasks, 18-23 hours

### Phase 4 - OPTIONAL (P3)

- [ ] Help system content creation (1 task, Phase 7)
- [ ] Help system expandable panel (1 task, Phase 7)

**Total:** 2 tasks, 6-10 hours

### DO NOT IMPLEMENT

- ? Settings Localization (all tasks removed)

---

## Summary: What Gets Built

### 1. Quality Hold Management (FULL)

**Database:**
- `tbl_Receiving_QualityHold` table with full audit trail
- 3 stored procedures (Insert, UpdateFinalAcknowledgment, SelectByLineID)
- `Model_Receiving_Entity_QualityHold` entity
- `Dao_Receiving_Repository_QualityHold` DAO

**Business Logic:**
- `Service_Receiving_QualityHoldDetection` with configurable part patterns
- Two-step acknowledgment dialogs (1 of 2, 2 of 2)
- Hard block validator (cannot save without acknowledgment)
- Quality hold record creation on save

**CQRS:**
- Create quality hold command + handler
- Get quality holds by line query + handler
- Restricted part validator (configurable patterns)
- Save transaction validator enhancement

**UI:**
- First warning dialog on part selection
- Second warning dialog before save
- Quality hold status indicators

---

### 2. CSV Export (SINGLE PATH)

**Service:**
- `Service_Receiving_CSVExport` with single configurable path
- Enhanced CSV format with all fields
- Blocking behavior (save fails if export fails)

**Configuration:**
- CSV path stored in `tbl_Receiving_Settings` (key: `CSVExportPath`)
- Settings menu UI to configure path
- Default path: `C:\MTM_CSV\`

**CQRS:**
- Export transaction to CSV command
- CSV export result model

**UI:**
- Automatic export on every save
- Completion screen shows CSV path and status
- "Open CSV Folder" button

---

### 3. Package Type Preferences

**Functionality:**
- Auto-fill package type on part selection (silent)
- Save preference only when user explicitly changes
- Prompt to update preference on override

**Implementation:**
- Uses existing `Dao_Receiving_Repository_PartPreference` (already exists)
- Integration in `ViewModel_Receiving_Wizard_Display_PartSelection`
- Integration in `ViewModel_Receiving_Wizard_Orchestration_SaveOperation`

---

### 4. Auto-Defaults and Calculations

**PackagesPerLoad:**
- Always defaults to 1 (entity model default value)
- No 0 state allowed

**Package Type Auto-Detection:**
- MMC prefix ? Coil
- MMF prefix ? Sheet

**Weight Per Package:**
- Initial calculation: `Weight / PackagesPerLoad`
- User can edit/override calculated value
- Overridden value saved to database
- Database field: `WeightPerPackage DECIMAL(18,2) NULL`

**Heat/Lot Placeholder:**
- Blank values replaced with "Nothing Entered" on save

---

### 5. Session Management

**Auto-Clear on Save:**
- Session file deleted after successful save

**Auto-Resume on Startup:**
- Session automatically restored without prompt
- No expiration check

**Reset Session Button:**
- "Reset Session" button in wizard UI
- Clears current module session only
- Confirmation dialog before reset

---

### 6. User Preferences

**Database Storage:**
- `tbl_Settings_Receiving_UserPreferences` table
- Fields: Username, DefaultReceivingMode

**Skip Mode Selection:**
- Direct navigation to preferred mode on startup
- Stored in database per user

**Return to Mode Selection:**
- "Return to Mode Selection" button in all 3 modes (Wizard, Manual, Edit)
- Always accessible
- Confirmation if unsaved data exists

**No UI for Preference Management:**
- Set programmatically only (no settings page for this specific feature)

---

### 7. Validation on LostFocus

**All Textboxes:**
- Validation triggers on LostFocus only
- No PropertyChanged validation
- Applies to: PO Number, Part Number, Load Count, Weight, Heat/Lot, Packages Per Load

**Error Display:**
- Inline error messages below fields
- Clear validation state indicators

**Validation Scope:**
- Comprehensive validation (format + business rules)
- Uses FluentValidation validators

---

### 8. Help System

**Contextual Help Content:**
- Help content for each wizard step
- Step-specific guidance

**Expandable Help Panel:**
- Uses `IService_Help` from Module_Core
- Expandable panel in wizard views
- Contextual content per step

---

## Files to Update with User Selections

### Phase Task Files

1. ? **tasks_phase2.md** - Add quality hold tasks 2.59-2.68
2. ? **tasks_phase3.md** - Add quality hold + CSV export CQRS tasks
3. ? **tasks_phase4.md** - Add all approved ViewModel integration tasks
4. ? **tasks_phase6.md** - Add all approved View/XAML tasks
5. ? **tasks_phase7.md** - Add all approved integration tasks
6. ? **tasks_phase8.md** - Add approved testing tasks

### Comparison Documents

1. ? **Module_Receiving_vs_Old_Module_Receiving_Comparison.md** - Mark approved features as "PLANNED"
2. ? **Module_Receiving_Comparison_Task_Cross_Reference.md** - Move approved features to "PLANNED" sections

---

## Next Steps

1. **Review this document** to confirm all user selections are accurately captured
2. **Update phase task files** with approved tasks (tasks_phase2.md through tasks_phase8.md)
3. **Update comparison documents** to reflect planned status
4. **Begin implementation** starting with Phase 2 (Quality Hold database infrastructure)

---

**END OF USER-APPROVED FEATURE IMPLEMENTATION PLAN**


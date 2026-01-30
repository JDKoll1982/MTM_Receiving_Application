# Module_Receiving Feature Cherry-Pick Board

**Purpose:** Select which missing features to implement and how to implement them  
**Status:** `[ ]` = Not Selected | `[x]` = Selected for Implementation  
**Instructions:** Check features you want, then check implementation options within each feature

---

## P0 CRITICAL FEATURES

### 1. Quality Hold Management (Compliance Requirement)

| Implement? | Feature Component | Implementation Options | Files to Modify | Notes |
|------------|-------------------|------------------------|-----------------|-------|
| - [x] | **Database Infrastructure** | | | Required foundation |
| | | - [x] Full audit trail (all acknowledgments tracked) | tasks_phase2.md ? Tasks 2.59-2.65 | Recommended: Complete compliance |
| | | - [ ] Minimal (user acknowledgment only) | tasks_phase2.md ? Modified Tasks 2.59-2.65 | Faster to implement |
| - [x] | **Detection Service** | | | Core business logic |
| | | - [ ] MMFSR + MMCSR detection (sheet + coil) | tasks_phase4.md ? Task 4.XX | Matches old system |
| | | - [x] Configurable part patterns (future-proof) | tasks_phase4.md ? Enhanced Task 4.XX | More flexible |
| - [x] | **Warning Dialogs** | | | User interaction |
| | | - [x] Two-step acknowledgment (1 of 2, 2 of 2) | tasks_phase4.md ? Service_Receiving_QualityHoldDetection | Old system pattern |
| | | - [ ] Single confirmation before save | tasks_phase4.md ? Simplified approach | Faster UX |
| - [x] | **Save Blocking** | | | Prevents non-compliant saves |
| | | - [x] Hard block (cannot save without acknowledgment) | tasks_phase3.md ? Validator enhancement | Strict compliance |
| | | - [ ] Soft warning (warn but allow override) | tasks_phase3.md ? Modified validator | More flexible |
| - [x] | **Quality Hold Records** | | | Database tracking |
| | | - [x] Create record for every restricted part | tasks_phase4.md ? SaveOperation integration | Complete audit trail |
| | | - [ ] Flag on load only (no separate table) | Entity model update only | Simpler implementation |

**Estimated Effort:** 33-40 hours (full) | 15-20 hours (minimal)  
**Dependencies:** None  
**Recommendation:** Implement FULL version - compliance features should not be compromised

---

## P1 HIGH PRIORITY FEATURES

UPDATE: CSV is for printing purposes only, no "Local CSV" needed, only a single CSV path that can be set by the user via the settings menu, so the "Dual-path" option is not needed.

### 2. CSV Export (Business Requirement)

| Implement? | Feature Component | Implementation Options | Files to Modify | Notes |
|------------|-------------------|------------------------|-----------------|-------|
| - [x] | **Export Service** | | | Core export logic |
| | | - [ ] Dual-path (local + network) with fallback | tasks_phase4.md ? Service_Receiving_CSVExport | Old system pattern |
| | | - [ ] Local only (simplified) | tasks_phase4.md ? Simplified service | Network often fails |
| | | - [x] Configurable destinations (user choice) | tasks_phase4.md ? Enhanced service | Most flexible |
| - [x] | **Export Timing** | | | When to export |
| | | - [x] Automatic on every save | tasks_phase4.md ? SaveOperation integration | Old system behavior |
| | | - [ ] Manual export button (user initiated) | tasks_phase6.md ? Add export button | User control |
| | | - [ ] Both automatic + manual option | Both modifications | Best of both |
| - [ ] | **CSV Format** | | | File structure |
| | | - [ ] Match old system format (compatibility) | Service with ReceivingLineCSVMap | Ensures compatibility |
| | | - [x] Enhanced format (more fields) | Modified CSVMap | Future-proof |
| - [x] | **Failure Handling** | | | When export fails |
| | | - [ ] Non-blocking (save succeeds, CSV is bonus) | tasks_phase4.md ? Current design | Database is source of truth |
| | | - [x] Blocking (save fails if CSV fails) | Modified SaveOperation | Stricter validation |
| - [x] | **Result Display** | | | User feedback |
| | | - [x] Detailed results (CSV path, SQL DB, status) | tasks_phase6.md ? Completion screen enhancement | Full transparency |
| | | - [ ] Simple success/fail message | Minimal UI update | Cleaner UI |
| - [x] | **Folder Access** | | | User convenience |
| | | - [x] "Open Folder" buttons for local and network | tasks_phase6.md ? Commands added | Helpful for users |
| | | - [ ] No folder access (just show paths) | Display only | Simpler |

**Estimated Effort:** 13-15 hours (full) | 6-8 hours (local only)  
**Dependencies:** None  
**Recommendation:** Dual-path with non-blocking + detailed results - matches old system

---

### 3. Package Type Preferences (Part-Based)

| Implement? | Feature Component | Implementation Options | Files to Modify | Notes |
|------------|-------------------|------------------------|-----------------|-------|
| - [x] | **Auto-Fill from Preference** | | | Load saved preference |
| | | - [x] Auto-fill on part selection (silent) | tasks_phase4.md ? PartSelection ViewModel | Old system behavior |
| | | - [ ] Suggest preference (user confirms) | tasks_phase4.md ? Enhanced with prompt | More transparent |
| - [x] | **Save Preference Timing** | | | When to save |
| | | - [ ] On every transaction save | tasks_phase4.md ? SaveOperation integration | Always learns |
| | | - [x] Only when user explicitly changes | User-initiated save | Prevents accidental updates |
| - [x] | **Preference Override** | | | User control |
| | | - [ ] Allow override without saving preference | Standard behavior | User can change per transaction |
| | | - [x] Prompt to update preference on override | Enhanced UX | Helps users maintain preferences |

**Estimated Effort:** 3-5 hours  
**Dependencies:** DAO already exists (Phase 2 complete)  
**Recommendation:** Auto-fill + save on transaction - simple and effective

---

## P2 MEDIUM PRIORITY FEATURES

UPDATE: for "Display only (no entity field) | UI-only calculation | Lighter implementation |", this field should be calculated once but allow the user to edit/override it if needed.

### 4. Auto-Defaults and Calculations

| Implement? | Feature Component | Implementation Options | Files to Modify | Notes |
|------------|-------------------|------------------------|-----------------|-------|
| - [x] | **PackagesPerLoad Default** | | | Convenience feature |
| | | - [ ] Auto-set to 1 when Part entered (if 0) | tasks_phase4.md ? LoadDetailsGrid ViewModel | Old system behavior |
| | | - [x] Always default to 1 (no 0 state) | Entity model default value | Simpler |
| - [x] | **Package Type Auto-Detection** | | | MMC/MMF logic |
| | | - [x] Auto-detect based on part prefix (MMC=Coil, MMF=Sheet) | tasks_phase4.md ? LoadDetailsGrid ViewModel | Old system pattern |
| | | - [ ] No auto-detection (user always selects) | No change needed | Manual only |
| - [x] | **Weight Per Package Calculation** | | | Automatic calculation |
| | | - [ ] Auto-calculate (Weight divided by PackagesPerLoad) | tasks_phase4.md ? Add calculated property | Helpful for users |
| | | - [x] Display only (no entity field) | UI-only calculation | Lighter implementation |
| - [x] | **Heat/Lot Placeholder** | | | Handle blank values |
| | | - [x] Replace blank with "Nothing Entered" on save | tasks_phase4.md ? SaveOperation logic | Old system behavior |
| | | - [ ] Require Heat/Lot (no blanks allowed) | Validator enhancement | Stricter data quality |
| | | - [ ] Allow blank (no placeholder) | No change needed | Cleanest data |

**Estimated Effort:** 3-5 hours  
**Dependencies:** None  
**Recommendation:** Implement all auto-defaults - significant UX improvement with minimal effort

---

### 5. Session Management Enhancements

UPDATE: needs a "Reset Session" button to clear current session if user desires it.  Will only reset session for current module, not all modules.

| Implement? | Feature Component | Implementation Options | Files to Modify | Notes |
|------------|-------------------|------------------------|-----------------|-------|
| - [x] | **Session Cleanup on Save** | | | Prevent stale data |
| | | - [x] Auto-clear session file after successful save | tasks_phase4.md ? SaveOperation enhancement | Old system behavior |
| | | - [ ] Keep session until user exits | No change needed | Allows review |
| - [x] | **Session Restoration on Startup** | | | Crash recovery |
| | | - [ ] Prompt user to resume session | tasks_phase7.md ? Startup lifecycle integration | Old system pattern |
| | | - [x] Auto-resume without prompt | Modified startup logic | Seamless recovery |
| | | - [ ] No restoration (start fresh always) | No change needed | Simplest |
| - [x] | **Session Expiration** | | | Handle old sessions |
| | | - [ ] Expire sessions older than X days | tasks_phase7.md ? Enhanced logic | Prevents old data |
| | | - [x] No expiration (always offer restore) | Current design | Simpler |

**Estimated Effort:** 3-4 hours  
**Dependencies:** Session handlers already exist (Phase 3 complete)  
**Recommendation:** Cleanup on save + prompt for restore - balances safety and convenience

---

### 6. User Preferences (Default Receiving Mode)

UPDATE: Each of the 3 Module_Receiving Modes MUST have a button that can be accessed at all times in order to return to the "Mode Selection" screen, even if the user has set a preference to skip the mode selection.  This is to allow the user to change modes at any time.

| Implement? | Feature Component | Implementation Options | Files to Modify | Notes |
|------------|-------------------|------------------------|-----------------|-------|
| - [x] | **Preference Storage** | | | Where to store |
| | | - [x] Database table (tbl_Settings_Receiving_UserPreferences) | tasks_phase7.md ? Task 7.XX | Old system approach |
| | | - [ ] User settings file (JSON) | Module_Core approach | Simpler |
| - [x] | **Mode Selection Skip** | | | Bypass hub |
| | | - [x] Skip mode selection if preference set | tasks_phase7.md ? Hub orchestration modification | Old system behavior |
| | | - [ ] Always show mode selection (preference as default) | Hub shows selection but pre-selects | More transparent |
| - [ ] | **Preference Management UI** | | | User control |
| | | - [ ] Settings page in Module_Settings.Receiving | tasks_phase7.md ? Settings module integration | Full featured |
| | | - [ ] Quick toggle on mode selection screen | Hub view modification | Simpler |
| | | - [x] No UI (set programmatically only) | Minimal implementation | For power users |

**Estimated Effort:** 4-6 hours (full) | 2-3 hours (minimal)  
**Dependencies:** Settings infrastructure (Phase 7)  
**Recommendation:** Database storage + skip with settings page - complete solution

---

### 7. Validation on LostFocus (UX Enhancement)

| Implement? | Feature Component | Implementation Options | Files to Modify | Notes |
|------------|-------------------|------------------------|-----------------|-------|
| - [x] | **Validation Timing** | | | When to validate |
| | | - [x] All textboxes validate on LostFocus only | tasks_phase6.md ? All view modifications | Old system + better UX |
| | | - [ ] Critical fields only (PO, Part) | Selective view modifications | Lighter implementation |
| | | - [ ] Keep current (PropertyChanged validation) | No change needed | Real-time feedback |
| - [ ] | **Error Display** | | | How to show errors |
| | | - [x] Inline error message below field | View XAML modifications | Clear and immediate |
| | | - [ ] InfoBar at top of form | Minimal view changes | Less intrusive |
| | | - [ ] Notification service only | No view changes | Simplest |
| - [ ] | **Validation Scope** | | | What to validate |
| | | - [ ] Format only (basic validation) | Lightweight validators | Fast |
| | | - [x] Format + business rules (comprehensive) | Full validators | More thorough |

**Estimated Effort:** 4-6 hours (all fields) | 2-3 hours (critical only)  
**Dependencies:** None  
**Recommendation:** All textboxes + inline errors - professional UX

---

## P3 LOW PRIORITY FEATURES

### 8. Help System Integration

| Implement? | Feature Component | Implementation Options | Files to Modify | Notes |
|------------|-------------------|------------------------|-----------------|-------|
| - [x] | **Help Content** | | | Documentation |
| | | - [x] Contextual help per workflow step | tasks_phase6.md ? All views + content generation | Old system approach |
| | | - [ ] Single help page (generic) | Minimal implementation | Simpler |
| | | - [ ] External documentation link | No implementation needed | Easiest |
| - [x] | **Help Panel** | | | UI integration |
| | | - [x] Expandable panel on each view | IService_Help integration | Full featured |
| | | - [ ] Help button ? dialog | Simple dialog implementation | Moderate |
| | | - [ ] No in-app help | No change needed | External docs only |

**Estimated Effort:** 6-10 hours (full) | 2-3 hours (dialog only)  
**Dependencies:** Content creation  
**Recommendation:** SKIP for MVP - external documentation is sufficient

---

### 9. Settings Localization (Future i18n) - DO NOT IMPLEMENT

UPDATE: DO NOT IMPLEMENT THIS FEATURE

| Implement? | Feature Component | Implementation Options | Files to Modify | Notes |
|------------|-------------------|------------------------|-----------------|-------|
| - [ ] | **UI Text Storage** | | | For future translation |
| | | - [ ] All UI text in settings database | tasks_phase7.md ? Settings schema expansion | Old system approach |
| | | - [ ] Resource files (.resx) for localization | Standard .NET approach | Better practice |
| | | - [ ] Hardcoded strings (no localization) | No change needed | Simplest |
| - [ ] | **Scope** | | | What to localize |
| | | - [ ] All text (buttons, labels, messages) | Comprehensive implementation | Complete solution |
| | | - [ ] Messages and errors only | Partial implementation | Key areas |
| | | - [ ] No localization planned | No change needed | English only |

**Estimated Effort:** 8-12 hours (full) | Not applicable (skip)  
**Dependencies:** Internationalization requirements  
**Recommendation:** SKIP - No i18n requirement mentioned, premature optimization

---

## Summary Table: Quick Priority View

| Feature | Priority | Estimated Effort | Recommendation | Phase Impact |
|---------|----------|------------------|----------------|--------------|
| - [x] Quality Hold Management | P0 CRITICAL | 33-40 hours | **MUST IMPLEMENT** | Phase 2,3,4,8 |
| - [x] CSV Export | P1 HIGH | 13-15 hours | **STRONGLY RECOMMENDED** | Phase 3,4,6,7,8 |
| - [x] Package Type Preferences | P1 HIGH | 3-5 hours | Recommended | Phase 4 |
| - [x] Auto-Defaults | P2 MEDIUM | 3-5 hours | Recommended | Phase 4 |
| - [x] Session Management Enhancements | P2 MEDIUM | 3-4 hours | Recommended | Phase 4,7 |
| - [x] User Preferences (Default Mode) | P2 MEDIUM | 4-6 hours | Optional | Phase 7 |
| - [x] Validation on LostFocus | P2 MEDIUM | 4-6 hours | Recommended | Phase 6 |
| - [x] Help System | P3 LOW | 6-10 hours | **SKIP** | N/A |
| - [ NO ] Settings Localization | P3 LOW | 8-12 hours | **SKIP** | N/A |

**Total if ALL implemented:** 77-103 hours  
**Recommended subset (P0+P1+critical P2):** 60-75 hours

---

## File Update References

When you check features above, these files will be updated:

| File | Update Type | Features Affected |
|------|-------------|-------------------|
| `Module_Receiving_vs_Old_Module_Receiving_Comparison.md` | Documentation | Add "PLANNED" status to selected features |
| `Module_Receiving_Comparison_Task_Cross_Reference.md` | Cross-reference | Move features from "NOT PLANNED" to "PLANNED" sections |
| `Module_Receiving/tasks_phase1-8_required_updates.md` | Implementation guide | Mark selected tasks as "APPROVED FOR IMPLEMENTATION" |
| `Module_Receiving/tasks_phase2.md` | Add database tasks | Quality Hold tables/SPs (if selected) |
| `Module_Receiving/tasks_phase3.md` | Add CQRS tasks | Quality Hold commands/queries, CSV commands (if selected) |
| `Module_Receiving/tasks_phase4.md` | Add ViewModel tasks | Service integrations, auto-defaults (if selected) |
| `Module_Receiving/tasks_phase6.md` | Add View tasks | UI enhancements, LostFocus (if selected) |
| `Module_Receiving/tasks_phase7.md` | Add integration tasks | User preferences, session restoration (if selected) |
| `Module_Receiving/tasks_phase8.md` | Add test tasks | Quality Hold tests, CSV tests (if selected) |

---

## Usage Instructions

1. **Review each feature section** and check the main "Implement?" checkbox if you want that feature
2. **Within each checked feature**, select your preferred implementation options
3. **Save this file** with your selections
4. **Notify me** and I will:
   - Update the comparison documents to reflect "PLANNED" status
   - Update the cross-reference document with new task mappings
   - Mark selected tasks in `tasks_phase1-8_required_updates.md` as approved
   - Optionally generate updated phase task files with new tasks inserted

**Example:**

```markdown
- [x] Quality Hold Management
  - [x] Full audit trail
  - [x] Two-step acknowledgment
  - [x] Hard block (strict compliance)
```

I will add all Quality Hold tasks to Phase 2,3,4,8 with full implementation

---

**Last Updated:** 2025-01-30  
**Status:** Awaiting User Selection

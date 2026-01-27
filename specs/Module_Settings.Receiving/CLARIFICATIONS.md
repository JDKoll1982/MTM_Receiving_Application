# Module_Settings.Receiving - Clarifications and Edge Cases

**Version:** 1.1  
**Last Updated:** 2026-01-25  
**Purpose:** Document unresolved questions, edge cases, and implementation decisions needed

---

## 🎯 Overview

This document captures questions and edge cases discovered during specification analysis that require business owner clarification or design decisions before implementation.

---

## 🔒 CRITICAL ARCHITECTURAL PRINCIPLE

**Session Isolation Rule:**
> **User A's actions SHALL NEVER alter User B's unsaved work.**

**Implications:**
1. **Active Workflows** (Wizard Mode, Manual Mode):
   - Each user's session is completely isolated
   - Settings changes by Admin apply to NEW sessions only
   - Current session continues with snapshot of settings from session start
   
2. **Edit Mode** (ONLY exception):
   - Operates on SAVED transactions (not unsaved work)
   - User A editing Transaction #123 may create conflict if User B also edits #123
   - Optimistic locking handles concurrent edits of same saved transaction

3. **Settings Changes**:
   - Apply immediately to database
   - Cached for new sessions
   - Do NOT affect active sessions (unsaved work protected)

**This principle resolves Edge Cases 1, 2, and 8 immediately.**

---

## 📋 Part Number Management

### Edge Case 1: Concurrent Part Configuration Changes ✅ RESOLVED

**Scenario:**
- User A is in Settings, editing part MMC0001000 configuration
- User B is in Receiving workflow, currently receiving MMC0001000
- User A changes Part Type from "Coils" to "Tubing" and saves
- User B is still mid-workflow with the part loaded

**RESOLUTION (Session Isolation Principle):**
- **Option B: Session-scoped** - User B continues with original config, new config applies next session
- User B's unsaved work is NEVER altered
- Settings changes take effect when User B:
  1. Saves current transaction and starts new one
  2. Switches workflow modes (new session)
  3. Closes and reopens application

**Implementation:**
```csharp
// On workflow session start
public class Service_ReceivingSession
{
    private readonly Dictionary<string, PartConfiguration> _sessionConfig = new();
    
    public async Task StartSessionAsync()
    {
        // Snapshot all relevant settings at session start
        _sessionConfig = await _settingsService.GetCurrentConfigurationAsync();
        // Session continues with this snapshot, immune to settings changes
    }
}
```

**No user action required - session isolation is automatic.**

---

### Edge Case 2: Part Number with Multiple Quality Hold Configurations ✅ RESOLVED

**Scenario:**
- Part MMC0001000 is marked as Quality Hold Required in Settings
- User starts receiving this part, acknowledges Quality Hold dialog
- Mid-workflow, admin disables Quality Hold for MMC0001000
- User continues to next load of same part

**RESOLUTION (Session Isolation Principle):**
- **Option A: Session-locked** - Quality Hold status frozen for current session
- Once user acknowledges QH at session start, status remains for entire session
- Admin's change to disable QH applies to NEW sessions only
- User's unsaved work continues with original QH requirements

**Implementation:**
```csharp
public class Model_Session_QualityHoldState
{
    public string PartNumber { get; set; }
    public bool QualityHoldRequired { get; set; }
    public DateTime AcknowledgedAt { get; set; }
    public int UserId { get; set; }
    
    // Frozen for session - NOT refreshed from database
}
```

**Session QH state is immutable once acknowledged.**

---

### Edge Case 3: Default Receiving Location Conflicts

**Scenario:**
- ERP (Infor Visual) shows MMC0001000 default location: V-C0-01
- Settings override shows MMC0001000 default location: V-C0-02
- User receives this part

**Questions:**
1. Which takes absolute precedence? (Currently spec says Settings > ERP)
2. Should UI indicate the source of the default location?
3. Should there be a "Use ERP Default" button to reset Settings override?

**Current Assumption:** Settings overrides ERP always. **Confirm?**

---

### Edge Case 4: Deleted Part Numbers

**Scenario:**
- Part MMC0001000 has Settings configuration (Part Type, Location, Quality Hold)
- Part MMC0001000 is deleted/deactivated in ERP
- User tries to receive MMC0001000

**Questions:**
1. Should Settings configuration be automatically cleaned up?
2. Should system warn "Part inactive but Settings exist"?
3. Allow receiving inactive parts with warning, or block completely?

**Options:**
- **A**: Allow with warning - "Part inactive in ERP, proceed with Settings config?"
- **B**: Block receiving - "Part inactive, cannot receive"
- **C**: Soft block - "Part inactive. Override requires supervisor PIN?"

**Recommendation Needed:** What's company policy on inactive part receiving?

---

## 📦 Package Type Preferences

### Edge Case 5: Package Type Auto-Assignment Conflicts

**Scenario:**
- Part MMC0001000 has automatic Part Type: Coils (from prefix)
- Settings override assigned Part Type: Tubing
- User creates Package Type Preference for MMC0001000 → "Bundle"

**Questions:**
1. Does Package Type Preference apply to actual Part Type (Tubing) or auto-detected type (Coils)?
2. Should Package Type Preference be aware of Part Type overrides?
3. Should changing Part Type warn "Package Type Preference may need update"?

**Current Assumption:** Package Type Preference is independent of Part Type. **Confirm?**

---

### Edge Case 6: Missing Package Type Preference

**Scenario:**
- User enters part MMC0005000
- No Package Type Preference exists for this part
- System must auto-select default package type

**Questions:**
1. What's the default Package Type selection logic?
   - Default to "Skid" for all parts?
   - Default based on Part Type (Coils → Skid, Flat Stock → Pallet)?
   - Prompt user to select?
2. Should system prompt user to save Package Type Preference for future use?

**Options:**
- **A**: Auto-select by Part Type mapping (Coils → Skid, Flat → Pallet, etc.)
- **B**: Auto-select "Skid" as universal default
- **C**: Prompt user and ask "Save as preference for future?"

**Recommendation Needed:** What provides best UX while maintaining data quality?

---

## 📍 Receiving Location Dynamics

### Edge Case 7: Invalid Location Validation

**Scenario:**
- User sets Default Receiving Location for MMC0001000 to "INVALID-LOC"
- "INVALID-LOC" doesn't exist in Infor Visual valid locations

**Questions:**
1. Should Settings validate location against ERP before saving?
2. If validation fails, block save or allow with warning?
3. What if location is valid today but deleted from ERP tomorrow?

**Options:**
- **A**: Strict validation - Block save if location invalid
- **B**: Warn but allow - "Location not found in ERP, save anyway?"
- **C**: No validation - Accept any string, validate at receiving time

**Current Spec:** Warning but allow save. **Confirm this is acceptable?**

---

### Edge Case 8: Session Override Persistence Across Modes ✅ RESOLVED

**Scenario:**
- User in Wizard Mode receiving MMC0001000
- User manually overrides location from V-C0-01 to RECV
- User saves transaction, immediately starts Manual Mode
- User receives MMC0001000 again in Manual Mode

**RESOLUTION (Session Isolation + Workflow Principle):**
- **Session overrides clear when transaction is saved**
- Each workflow mode starts a new session with fresh settings snapshot
- Location override is specific to that single transaction
- Next transaction (even same part) reverts to default location from settings

**Definition of "Session End":**
1. Save transaction (commit to database) → **Session ends, overrides cleared**
2. Switch workflow modes → **New session starts, fresh settings snapshot**
3. Close application → **Session ends**

**Session overrides are transaction-scoped, NOT persistent.**

---

## ⚠️ Quality Hold Procedures

### Edge Case 9: Quality Hold Procedure Text Updates

**Scenario:**
- Company updates Quality Hold procedures (new step added)
- Procedure text is hardcoded in application
- User sees old procedure text until app is updated

**Questions:**
1. Should Quality Hold procedure text be configurable in Settings?
2. Should there be an "Edit Quality Hold Procedures" admin function?
3. Or is procedure text deployment-managed (app update required)?

**Options:**
- **A**: Configurable text - Admin can edit in Settings (more flexible)
- **B**: Hardcoded text - Requires app deployment (more controlled)
- **C**: Hybrid - Default text in app, Settings override available

**Recommendation Needed:** What level of procedure text control is needed?

---

## 📋 Cross-Referenced from Module_Receiving

### Edge Case 10: Part Type Modifier Configuration (Module_Receiving Edge Case 8)

**Scenario:**
- User receives non-standard part "XYZ-CUSTOM-001" that doesn't match any prefix rules
- System defaults to "Miscellaneous" type
- Admin wants to create custom Part Type modifier for "XYZ-" prefix

**Requirements from Module_Receiving Spec:**
- Accept non-standard parts as-is, assign to "Miscellaneous" type
- Admin should be able to configure custom Part Type modifiers in Settings

**Questions:**
1. Should there be a "Part Type Modifiers" settings page?
2. What's the UI for defining prefix → Part Type mappings?
3. Should it support regex patterns or just simple prefix matching?
4. Can one part number have multiple prefix rules? (precedence order?)

**Proposed Solution:**
Create `01-Settings-Categories/part-type-modifiers.md` specification with:
- Prefix Pattern field (e.g., "XYZ-", "MMC0001*", regex support?)
- Target Part Type dropdown (Coils, Flat Stock, Tubing, etc.)
- Priority/Precedence order (drag-and-drop ranking)
- Active/Inactive toggle

**Action Required:** Create Part Type Modifiers specification document

---

### Edge Case 11: PO Number Format Configuration (Module_Receiving Edge Case 9)

**Scenario:**
- User enters PO "123456" (missing standard format)
- System attempts auto-standardization: "123456" → "00123456"
- What if company changes PO format from 8-digit to 10-digit?

**Requirements from Module_Receiving Spec:**
- Enforce standard format with override option
- PO format should be configurable in Settings

**Questions:**
1. Should PO Number format be defined in Settings?
2. What format configuration options are needed?
   - Length (8-digit, 10-digit, variable?)
   - Prefix rules (e.g., "PO-" prefix required?)
   - Auto-padding rules (left-pad with zeros?)
3. Should there be multiple allowed formats? (legacy + new format)
4. Override capability - admin-only or all users?

**Proposed Solution:**
Create `01-Settings-Categories/po-number-format.md` specification with:
- Format template field (e.g., "00000000" = 8-digit zero-padded)
- Prefix/Suffix options
- Validation rules (regex pattern?)
- Override permission level (Admin, Supervisor, All Users)
- Legacy format support (allow multiple valid formats)

**Action Required:** Create PO Number Format specification document

---

### Edge Case 12: Keyboard Shortcuts Configuration (Module_Receiving Edge Case 18)

**Scenario:**
- User wants to disable certain keyboard shortcuts that conflict with other apps
- Admin wants to customize keyboard shortcuts for different user groups

**Requirements from Module_Receiving Spec:**
- Document all keyboard shortcuts with configurable enable/disable
- User preference or admin-enforced defaults?

**Questions:**
1. Should keyboard shortcuts be per-user preference or system-wide setting?
2. What level of customization is allowed?
   - Enable/Disable individual shortcuts only?
   - Remap shortcuts to different keys?
3. Should there be preset profiles (e.g., "Excel-style", "VS Code-style")?
4. What's the conflict resolution if user has multiple shortcuts for same action?

**Proposed Solution:**
Create `01-Settings-Categories/keyboard-shortcuts.md` specification with:
- List of all available shortcuts
- Enable/Disable toggles per shortcut
- Optional: Remap functionality (advanced)
- User vs. Admin setting scope
- Shortcut help dialog (Ctrl+? or F1)

**Action Required:** Create Keyboard Shortcuts specification document

---

## 🔒 Module_Settings.Core Requirements

### Edge Case 13: Permission Levels (Module_Receiving Edge Case 19)

**Scenario:**
- Different user roles need different Edit Mode access levels
- Some users can view, some can edit, some can delete

**Requirements from Module_Receiving Spec:**
- Permission levels and role-based access control
- Module_Core feature configurable per module

**Questions:**
1. What permission levels are needed for Receiving module?
   - View Only (read historical transactions)
   - Edit (modify existing transactions)
   - Delete/Void (remove transactions)
   - Admin (full access + Settings)
2. How are permission levels assigned? (per user, per role, per group?)
3. Should permissions be time-based? (e.g., edit allowed within 24 hours only?)
4. Audit trail for permission changes?

**Note:** This requires Module_Settings.Core documentation for centralized permission management system.

**Action Required:** 
- Document in Module_Settings.Core (cross-module permission system)
- Reference from Module_Settings.Receiving for Receiving-specific roles

---

## ✅ Resolved Decisions

**From Session Isolation Principle:**
- ✅ **Edge Case 1:** Part configuration changes apply to new sessions only (session-scoped)
- ✅ **Edge Case 2:** Quality Hold status frozen once acknowledged for session
- ✅ **Edge Case 8:** Session overrides clear on transaction save (transaction-scoped)

**From Module_Receiving clarifications (referenced here for traceability):**
- ✅ **Edge Case 8 (Part Types):** Non-standard parts → Miscellaneous type, custom modifiers needed (see Edge Case 10 above)
- ✅ **Edge Case 9 (PO Format):** Enforce standard format with override, needs Settings config (see Edge Case 11 above)
- ✅ **Edge Case 18 (Shortcuts):** Document all, configurable enable/disable (see Edge Case 12 above)
- ✅ **Edge Case 19 (Permissions):** Role-based access, Module_Core feature (see Edge Case 13 above)

---

## ⏳ Pending Business Owner Clarification

**Still Requiring Decisions:**
- Edge Case 3: Default Receiving Location Conflicts (Settings vs. ERP precedence) - **Current assumption: Settings > ERP, awaiting confirmation**
- Edge Case 4: Deleted Part Numbers (allow receiving inactive parts?)
- Edge Case 5: Package Type Auto-Assignment Conflicts
- Edge Case 6: Missing Package Type Preference (default selection logic)
- Edge Case 7: Invalid Location Validation (strict vs. warning)
- Edge Case 9: Quality Hold Procedure Text Updates (configurable vs. hardcoded)

---

## 📝 Action Items

### Immediate (Module_Settings.Receiving)
1. ⏳ Create `01-Settings-Categories/part-type-modifiers.md`
2. ⏳ Create `01-Settings-Categories/po-number-format.md`
3. ⏳ Create `01-Settings-Categories/keyboard-shortcuts.md`

### Future (Module_Settings.Core)
4. ⏳ Create Module_Settings.Core specifications
5. ⏳ Document centralized permission management system

### Pending Business Owner Clarification
- Edge Cases 1-9 (original Module_Settings.Receiving questions)

---

**Last Updated:** 2026-01-25  
**Status:** Cross-referenced with Module_Receiving, Action items identified  
**Next Steps:** Create missing specification documents for new settings categories

### Edge Case 10: Quality Hold Mid-Workflow Acknowledgment

**Scenario:**
- User enters Part MMC0003000 (Quality Hold required)
- User acknowledges Quality Hold dialog
- User enters 5 loads of MMC0003000
- User goes to Review screen
- User edits grid to add 6th load of MMC0003000

**Questions:**
1. Should Quality Hold dialog re-appear for the 6th load?
2. Is acknowledgment session-wide or per-load?
3. Should Review screen show "QH Acknowledged" indicator?

**Current Assumption:** Acknowledgment is session-wide (once per session per part). **Confirm?**

---

## 🔄 Workflow Preferences

### Edge Case 11: Default Workflow Mode Selection

**Scenario:**
- User sets default workflow mode to "Wizard Mode"
- User completes transaction, app returns to Hub
- Next transaction auto-enters Wizard Mode without Hub display

**Questions:**
1. Should default mode skip Hub selection entirely?
2. Should there be "Always ask" option (show Hub every time)?
3. Can user temporarily override default mode without changing Settings?

**Options:**
- **A**: Skip Hub - Default mode auto-loads
- **B**: Show Hub with default highlighted - User can change or proceed
- **C**: Conditional skip - Press Ctrl+Enter to force Hub display

**Recommendation Needed:** What provides best balance of efficiency and flexibility?

---

### Edge Case 12: Auto-Save Behavior in Manual Mode

**Scenario:**
- User enables "Auto-Save" workflow preference
- User in Manual Mode with 50-load grid partially filled
- Auto-save triggers while user is mid-entry

**Questions:**
1. When does auto-save trigger? (Time-based? Row-based? On idle?)
2. Does auto-save validate entire grid or only filled rows?
3. What if auto-save finds validation errors - block or warn?

**Current Spec:** Auto-save not yet specified. **Need full auto-save requirements.**

---

## 🔧 Advanced Settings

### Edge Case 13: CSV Export Path Configuration

**Scenario:**
- User sets custom CSV export path to network drive "\\\\server\\share\\csv"
- Network drive is offline/disconnected
- User completes receiving transaction

**Questions:**
1. Should system validate CSV path accessibility before saving Settings?
2. What's fallback behavior if network path fails at export time?
3. Should there be primary + backup CSV path configuration?

**Current Spec:** Graceful degradation to local path. **Confirm multi-path support needed?**

---

### Edge Case 14: Debug/Verbose Logging Mode

**Scenario:**
- Admin enables "Verbose Logging" in Advanced Settings
- Application logs significantly more data
- Performance impacts observed

**Questions:**
1. Should verbose logging auto-disable after X hours?
2. Should there be a file size limit for log files?
3. Should verbose mode warn about performance impact?

**Current Spec:** Verbose logging not yet specified. **Need logging requirements.**

---

## 🎨 UI/UX Considerations

### Edge Case 15: Settings Search/Filter

**Scenario:**
- User needs to find settings for a specific part number
- Part Number Management list contains 1000+ parts

**Questions:**
1. Should there be a search/filter function?
2. How should search work? (Part number, Part type, Location, etc.)
3. Should there be bulk edit capability for multiple parts?

**Recommendation Needed:** What level of search/filter/bulk operations is required?

---

### Edge Case 16: Settings Import/Export

**Scenario:**
- Company wants to migrate Settings from test environment to production
- 1000+ part configurations exist

**Questions:**
1. Should Settings be exportable to file (JSON, CSV)?
2. Should Settings be importable with validation?
3. How to handle conflicts during import?

**Recommendation Needed:** Is Settings import/export a required feature?

---

## ✅ Decisions Needed Summary

**High Priority:**
1. [Edge Case 1](#edge-case-1-concurrent-part-configuration-changes) - Concurrent configuration changes
2. [Edge Case 2](#edge-case-2-part-number-with-multiple-quality-hold-configurations) - Quality Hold session locking
3. [Edge Case 4](#edge-case-4-deleted-part-numbers) - Inactive part receiving policy
4. [Edge Case 9](#edge-case-9-quality-hold-procedure-text-updates) - Quality Hold text configurability
5. [Edge Case 13](#edge-case-13-csv-export-path-configuration) - CSV path failover strategy

**Medium Priority:**
6. [Edge Case 5](#edge-case-5-package-type-auto-assignment-conflicts) - Package Type / Part Type relationship
7. [Edge Case 6](#edge-case-6-missing-package-type-preference) - Default Package Type logic
8. [Edge Case 11](#edge-case-11-default-workflow-mode-selection) - Default mode Hub behavior

**Low Priority (Can defer):**
9. [Edge Case 15](#edge-case-15-settings-search-filter) - Settings search/filter UX
10. [Edge Case 16](#edge-case-16-settings-import-export) - Settings import/export

---

## 📝 Notes

- Document decisions made in this section as they're resolved
- Link to additional documentation when detailed specifications are created
- Update "Last Updated" date when adding new edge cases

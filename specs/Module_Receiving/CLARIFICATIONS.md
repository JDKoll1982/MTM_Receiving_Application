# Module_Receiving - Clarifications and Edge Cases

**Version:** 1.1  
**Last Updated:** 2026-01-25  
**Purpose:** Document unresolved questions, edge cases, and implementation decisions needed for the Receiving module

---

## 🎯 Overview

This document captures questions and edge cases discovered during specification analysis that require business owner clarification or design decisions before implementation.

---

## 🔒 CRITICAL ARCHITECTURAL PRINCIPLE

**Session Isolation Rule:**
> **User A's actions SHALL NEVER alter User B's unsaved work.**

**Application to Receiving Module:**

1. **Wizard Mode / Manual Mode** (Unsaved Work):
   - Each user's active workflow is completely isolated
   - Settings changes by Admin do NOT affect active sessions
   - User continues with settings snapshot from session start
   - Changes apply when user starts NEW session (after save)

2. **Edit Mode** (Saved Work - ONLY Exception):
   - Multiple users CAN edit same saved transaction (optimistic locking)
   - Concurrent edit of Transaction #123 shows conflict warning
   - Last save wins with warning: "Transaction modified by another user"

3. **Session Boundaries:**
   - **Session starts:** User selects workflow mode
   - **Session ends:** User saves transaction, switches mode, or closes app
   - **Settings snapshot:** Captured at session start, immutable during session

**This principle ensures data integrity and prevents user confusion.**

See: Module_Settings.Receiving/CLARIFICATIONS.md for settings-specific implications.

---

## ✅ Decisions Summary

### Resolved Decisions

**Workflow & Navigation:**
- ✅ [Edge Case 1](#edge-case-1-mode-switching-mid-workflow) - Mode switching behavior: **Allow with data migration and warning**
- ✅ [Edge Case 2](#edge-case-2-workflow-cancellation-data-retention) - Session recovery: **No retention - immediate discard on cancel**

**Load Management:**
- ✅ [Edge Case 3](#edge-case-3-load-number-auto-calculation-with-manual-override) - Load number sequencing: **Not manually changeable**
- ✅ [Edge Case 4](#edge-case-4-load-number-gaps-in-edit-mode) - Load renumbering in Edit Mode: **Auto-renumber to sequential without gaps**
- ✅ [Edge Case 5](#edge-case-5-uneven-load-division-with-remainder-1) - Uneven load division: **Warn user, allow manual adjustment**

**Part & PO Handling:**
- ✅ [Edge Case 6](#edge-case-6-zero-packages-per-load) - Zero packages validation: **Not allowed, validate on field exit**
- ✅ [Edge Case 7](#edge-case-7-part-number-case-sensitivity) - Part number case handling: **Case-insensitive search, standardized display**
- ✅ [Edge Case 8](#edge-case-8-non-standard-part-number-formats-includes-new-features) - Non-standard part support: **Accept as-is, assign to "Miscellaneous" type**
- ✅ [Edge Case 9](#edge-case-9-po-number-auto-standardization-failure-includes-new-features) - PO format validation: **Enforce standard format with override option**

**Location & Quality:**
- ✅ [Edge Case 10](#edge-case-10-location-override-across-multiple-parts) - Location override scope: **Per-part default maintained**
- ✅ [Edge Case 11](#edge-case-11-quality-hold-for-non-po-receiving) - Non-PO Quality Hold: **Universal procedures apply**
- ✅ [Edge Case 12](#edge-case-12-quality-hold-acknowledgment-timeout) - QH timeout policy: **No expiration, valid for entire session**

**Copy Operations:**
- ✅ [Edge Case 13](#edge-case-13-copy-with-validation-failures) - Copy with existing data: **Push down existing loads**
- ✅ [Edge Case 14](#edge-case-14-copy-source-load-selection-includes-new-features) - Copy source UX: **Right-click context menu with Advanced Copy**

**Edit Mode & Audit:**
- ✅ [Edge Case 15](#edge-case-15-edit-mode-load-count-changes--audit-trail-access) - Load change tracking: **Detailed audit trail with UI access points**
- ✅ [Edge Case 16](#edge-case-16-edit-mode-re-export-triggering) - CSV re-export workflow: **Manual export with load selection checkboxes**

**Performance & UX:**
- ✅ [Edge Case 17](#edge-case-17-grid-performance-with-100-loads) - Grid performance: **Use pagination/virtualization (Service_Pagination.cs)**
- ✅ [Edge Case 18](#edge-case-18-keyboard-shortcuts-conflicting-new-features) - Keyboard shortcuts: **Document all shortcuts with configurable enable/disable**

**Security:**
- ✅ [Edge Case 20](#edge-case-20-concurrent-editing-of-same-transaction) - Concurrent edit handling: **Optimistic locking with conflict warning**

### Pending Implementation Documentation

**Requires Module_Settings.Receiving Documentation:**
1. ✅ [Edge Case 8](#edge-case-8-non-standard-part-number-formats-includes-new-features) - Part Type modifier creation → **Documented in Module_Settings.Receiving/CLARIFICATIONS.md Edge Case 10**
2. ✅ [Edge Case 9](#edge-case-9-po-number-auto-standardization-failure-includes-new-features) - PO Number format configuration → **Documented in Module_Settings.Receiving/CLARIFICATIONS.md Edge Case 11**
3. ✅ [Edge Case 18](#edge-case-18-keyboard-shortcuts-conflicting-new-features) - Keyboard shortcuts enable/disable setting → **Documented in Module_Settings.Receiving/CLARIFICATIONS.md Edge Case 12**

**Requires Module_Settings.Core Documentation:**
4. ✅ [Edge Case 19](#edge-case-19-edit-mode-permission-levels) - Permission levels and role-based access control → **Documented in Module_Settings.Receiving/CLARIFICATIONS.md Edge Case 13** (requires Module_Core spec creation)

**Requires UI/UX Documentation Updates:**
5. [Edge Case 14](#edge-case-14-copy-source-load-selection-includes-new-features) - Advanced Copy functionality
6. [Edge Case 15](#edge-case-15-edit-mode-load-count-changes--audit-trail-access) - Load History UI access points
7. [Edge Case 16](#edge-case-16-edit-mode-re-export-triggering) - Export CSV with load selection UI
8. [Edge Case 17](#edge-case-17-grid-performance-with-100-loads) - Grid pagination/virtualization
9. [Edge Case 18](#edge-case-18-keyboard-shortcuts-conflicting-new-features) - Keyboard shortcuts help dialog

**Action Items for Module_Settings.Receiving:**
- ⏳ Create specification: `01-Settings-Categories/part-type-modifiers.md`
- ⏳ Create specification: `01-Settings-Categories/po-number-format.md`
- ⏳ Create specification: `01-Settings-Categories/keyboard-shortcuts.md`

**Action Items for Module_Settings.Core:**
- ⏳ Create Module_Settings.Core specification structure
- ⏳ Document centralized permission management system

---

## 📝 Notes

- All business logic decisions have been resolved ✅
- Settings module cross-references documented in Module_Settings.Receiving/CLARIFICATIONS.md ✅
- Implementation can proceed with documented answers
- New settings specifications needed:
  - Part Type Modifiers (prefix → type mapping)
  - PO Number Format configuration
  - Keyboard Shortcuts configuration
- Module_Settings.Core spec needed for centralized permission system
- UI/UX specifications needed for new features (Advanced Copy, Load History, etc.)
- Last Updated: 2026-01-25
- Cross-reference: Module_Settings.Receiving/CLARIFICATIONS.md Edge Cases 10-13 for settings-related requirements

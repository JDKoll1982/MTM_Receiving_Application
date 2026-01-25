# Module_Dunnage - Clarifications and Edge Cases

**Version:** 1.0  
**Last Updated**: 2026-01-25  
**Purpose:** Document unresolved questions, edge cases, and implementation decisions needed for Module_Dunnage

---

## 🎯 Overview

This document captures questions and edge cases discovered during Module_Dunnage analysis that require business owner clarification or design decisions before full implementation.

---

## 📋 Dunnage Type Management

### Edge Case 1: Dunnage Type Deactivation During Active Workflow

**Scenario:**
- Admin deactivates dunnage type "Wood Pallet 48x40"
- Multiple users are currently in Guided Mode with this type selected
- Users are at different workflow steps

**Questions:**
1. Should active workflows using deactivated type be invalidated immediately?
2. Should users be warned and allowed to complete current transaction?
3. Should save be blocked if type becomes inactive mid-workflow?
4. How to handle pending session data referencing inactive type?

**Options:**
- **A**: Block immediately - Invalidate all sessions, force mode selection restart
- **B**: Allow completion - Let current transactions finish, block new selections
- **C**: Soft warning - Warn users but allow completion with audit note

**Recommendation Needed:** What's the expected behavior for configuration changes during active use?

---

### Edge Case 2: Duplicate Dunnage Type Names

**Scenario:**
- Admin attempts to create type "Pallet" when "pallet" already exists (different case)
- Or "Wood Pallet" vs "Pallet - Wood" (similar but different names)

**Questions:**
1. Is type name case-sensitive or case-insensitive?
2. Should similarity checking warn admin (fuzzy matching)?
3. What about soft-deleted types - can names be reused?
4. Should type names have format validation (max length, allowed characters)?

**Options:**
- **A**: Case-insensitive unique - Block "Pallet" if "pallet" exists
- **B**: Case-sensitive unique - Allow "Pallet" and "pallet" as different types
- **C**: Fuzzy match warning - Warn on similar names but allow

**Recommendation Needed:** What level of duplicate prevention is appropriate?

---

### Edge Case 3: Type Icon Missing or Invalid

**Scenario:**
- Dunnage type has no icon specified
- Icon identifier references non-existent icon resource
- Icon file is corrupted or unavailable

**Questions:**
1. What's the fallback icon when none specified?
2. Should icon validation occur before type save?
3. Can administrators upload custom icon images?
4. Should there be a "default" icon library?

**Current Assumption:** Model_IconDefinition handles icons. **Confirm icon management approach?**

---

## 📊 Dynamic Specification Fields

### Edge Case 4: Spec Field Type Change After Data Exists

**Scenario:**
- Spec field "Quantity" is type "Text"
- Historical data exists with values like "10 boxes", "5-10", "unknown"
- Admin changes field type to "Number"

**Questions:**
1. How to migrate existing text values to number type?
2. Should type changes be blocked if data exists?
3. What happens to invalid values after type change?
4. Should data migration be automatic or manual?

**Options:**
- **A**: Block type changes - Cannot change type once data exists
- **B**: Migrate with validation - Attempt conversion, flag failures
- **C**: Soft delete old, create new - Preserve old field, create new with new type

**Recommendation Needed:** What's acceptable for data migration complexity?

---

### Edge Case 5: Required Spec Field Becomes Optional (or Vice Versa)

**Scenario:**
- Spec field "Supplier" is currently optional
- Admin changes it to required
- Historical data exists with blank "Supplier" values

**Questions:**
1. Does requirement change apply retroactively to historical data?
2. Should edit mode validate old transactions against new requirements?
3. Should new requirement only apply to new transactions?
4. How to handle validation failures in edit mode?

**Options:**
- **A**: Retroactive - All data must meet new requirements (strict)
- **B**: Forward-only - Only new transactions validated with new rules
- **C**: Warn in edit - Old data shows warnings but can still save

**Recommendation Needed:** What's the compliance requirement for requirement changes?

---

### Edge Case 6: Spec Field Deletion with Existing Data

**Scenario:**
- Spec field "Condition" has data in 1000+ historical transactions
- Admin deletes "Condition" field

**Questions:**
1. Should deletion be blocked if data exists?
2. Should soft-delete be used instead of hard-delete?
3. What happens to historical data values for deleted field?
4. Should deleted fields still display in edit mode view?

**Options:**
- **A**: Block deletion - Cannot delete fields with data
- **B**: Soft delete - Field marked inactive but data preserved
- **C**: Hard delete with archive - Data moved to archive table

**Recommendation Needed:** What's the data retention policy?

---

### Edge Case 7: Dropdown Spec Option Changes

**Scenario:**
- Dropdown field "Condition" has options: New, Used, Damaged
- Admin removes "Used" option
- Historical data exists with "Used" value

**Questions:**
1. What happens to historical records with removed option?
2. Should option removal be blocked if data exists?
3. Should removed options still display in edit mode (read-only)?
4. How to handle validation when editing old records?

**Options:**
- **A**: Block removal - Cannot remove options with data
- **B**: Soft remove - Option inactive but preserved for old data
- **C**: Allow with migration - Prompt admin to migrate data to different option

**Recommendation Needed:** What's expected behavior for dropdown option lifecycle?

---

## 🔗 Part-Type Associations

### Edge Case 8: Part-Type Association Removal During Active Workflow

**Scenario:**
- Part "Tube Assembly A123" is associated with type "Wood Pallet"
- User in Guided Mode selects "Wood Pallet", sees "Tube Assembly A123" in part list
- Admin removes association
- User hasn't selected part yet

**Questions:**
1. Should part list refresh automatically when associations change?
2. Should removal be blocked if active workflows reference it?
3. What if user already selected the part before removal?

**Options:**
- **A**: Immediate effect - Part disappears from list mid-workflow
- **B**: Session-locked - Association frozen for active workflows
- **C**: Block removal - Cannot remove while active workflows exist

**Recommendation Needed:** What's the expected real-time behavior?

---

### Edge Case 9: Part Associated with No Types

**Scenario:**
- Part "Component X" is created but not associated with any types
- User searches for parts to receive

**Questions:**
1. Should parts without type associations be visible?
2. Should part creation require at least one type association?
3. Can "orphan" parts be received through manual entry mode?

**Options:**
- **A**: Require association - Part must have >= 1 type to be active
- **B**: Allow orphans - Parts can exist without types (admin mode only)
- **C**: Warning only - Warn admin but allow orphan parts

**Recommendation Needed:** What's the minimum viable part configuration?

---

## ⚙️ Workflow Mode Selection and Switching

### Edge Case 10: Default Mode Preference Invalid

**Scenario:**
- User's default mode is set to "guided"
- Guided mode is disabled system-wide (hypothetically)
- User logs in

**Questions:**
1. Should system fall back to mode selection screen?
2. Should user be warned about invalid default?
3. Can workflow modes be disabled selectively?

**Current Handling:** Falls back to mode selection if invalid  
**Confirmation Needed:** Is mode disabling a requirement?

---

### Edge Case 11: Workflow Mode Switching Mid-Session

**Scenario:**
- User starts in Guided Mode, completes Type and Part selection
- User wants to switch to Manual Entry Mode to add more loads quickly

**Questions:**
1. Is mode switching allowed mid-workflow?
2. If allowed, how is partial data migrated to new mode?
3. Should user be warned about potential data loss?
4. Are there mode combinations that don't allow switching?

**Options:**
- **A**: Block switching - Must complete or cancel current workflow
- **B**: Allow with data migration - Preserve entered data when switching
- **C**: Allow with warning - Warn about data loss, user confirms

**Recommendation Needed:** What level of workflow flexibility is needed?

---

## 📈 Quantity and Validation

### Edge Case 12: Zero or Negative Quantities

**Scenario:**
- User enters quantity = 0 or negative value
- Validation currently requires quantity > 0

**Questions:**
1. Is zero quantity ever valid (e.g., placeholder entry)?
2. Should negative quantities be supported for returns/adjustments?
3. Is there a maximum reasonable quantity (1000, 10000)?
4. Should quantity validation differ by workflow mode?

**Current Rule:** Quantity must be > 0  
**Recommendation Needed:** Are return/adjustment transactions in scope?

---

### Edge Case 13: Extremely Large Quantities

**Scenario:**
- User enters quantity = 999,999,999

**Questions:**
1. What's the maximum reasonable quantity?
2. Should warning appear for unusually large values?
3. Should admin be able to set quantity limits per type?

**Options:**
- **A**: Hard limit - Max 10,000 (configurable)
- **B**: Soft limit - Warn above 1,000 but allow
- **C**: No limit - Any positive integer allowed

**Recommendation Needed:** What's the practical maximum dunnage quantity?

---

## 💾 CSV Export and Data Persistence

### Edge Case 14: CSV Export Network Failure Communication

**Scenario:**
- Network path unavailable during CSV export
- Local export succeeds
- User may not notice network export failed

**Questions:**
1. Should prominent warning/error be shown to user?
2. Should retry mechanism exist for network export?
3. Should user be prompted to manually trigger network export later?
4. Should failed network exports be queued and retried?

**Current Handling:** Graceful degradation (local succeeds)  
**Recommendation Needed:** What level of network export reliability is required?

---

### Edge Case 15: CSV Path Configuration Invalid

**Scenario:**
- Admin configures network CSV path as "\\\\invalid\\path\\dunnage.csv"
- Path doesn't exist or is inaccessible
- User completes transaction and attempts save/export

**Questions:**
1. Should CSV paths be validated before saving configuration?
2. What if path is valid during configuration but invalid at export time?
3. Should there be "Test Path" button in settings?

**Options:**
- **A**: Validate on config - Test path accessibility before saving
- **B**: Validate on use - Only discover invalid paths during export
- **C**: Best effort - Attempt export, gracefully degrade if fails

**Recommendation Needed:** What level of path validation is appropriate?

---

## 🔍 Edit Mode and Historical Data

### Edge Case 16: Edit Mode Concurrent Editing

**Scenario:**
- User A opens transaction #12345 in Edit Mode
- User B opens same transaction #12345 in Edit Mode
- Both users make changes and save

**Questions:**
1. Is optimistic or pessimistic locking used?
2. Who wins in conflict (last write wins, first write wins)?
3. Are users warned about concurrent edits?
4. Should edit locking be implemented?

**Options:**
- **A**: Optimistic locking - Last save wins, warn on conflict
- **B**: Pessimistic locking - First user locks, second user read-only
- **C**: No locking - Allow concurrent edits, audit trail shows both

**Recommendation Needed:** What's acceptable conflict resolution strategy?

---

### Edge Case 17: Edit Mode Search Performance with Large Dataset

**Scenario:**
- Database contains 100,000+ historical dunnage transactions
- User performs broad search (e.g., all transactions in last year)
- Search returns 50,000 results

**Questions:**
1. Should search results be paginated?
2. What's the maximum result count before pagination required?
3. Should search have required filter criteria (e.g., must select date range)?
4. Should there be search timeout limits?

**Options:**
- **A**: Pagination required - Max 100 results per page
- **B**: Limit results - Max 1000 results, warn if more exist
- **C**: Lazy load - Load results on demand as user scrolls

**Recommendation Needed:** What's the expected search result set size?

---

## 🎨 UI/UX and Performance

### Edge Case 18: Manual Entry Grid Performance with Large Load Count

**Scenario:**
- User adds 500+ loads in Manual Entry Mode grid
- Grid rendering and validation performance degrades

**Questions:**
1. What's the practical maximum load count per transaction?
2. Should UI virtualization be used for large grids?
3. Should there be warning for large transactions?
4. Should grid validation be debounced or on-demand?

**Options:**
- **A**: Hard limit - Max 100 loads per transaction
- **B**: Soft limit - Warn above 50 loads, allow up to 200
- **C**: No limit - Use virtualization and performance optimization

**Recommendation Needed:** What's the expected maximum transaction size?

---

### Edge Case 19: Icon Picker Custom Icons

**Scenario:**
- Administrator wants to use custom icon not in default library
- Current icon definitions may be limited

**Questions:**
1. Can administrators upload custom icon images?
2. What image formats are supported (PNG, SVG, JPG)?
3. Should icon size/resolution be validated?
4. Where are custom icons stored?

**Options:**
- **A**: Fixed library - Only predefined icons allowed
- **B**: Upload support - Allow custom icon uploads with validation
- **C**: Icon URL - Allow referencing external icon URLs

**Recommendation Needed:** Is custom icon support required?

---

## 🔐 Security and Permissions

### Edge Case 20: Admin Function Permission Levels

**Scenario:**
- Different admin roles need different permission levels
- Some users can manage types but not parts
- Some can view inventory but not modify

**Questions:**
1. What permission levels exist?
2. Should permissions be role-based or user-based?
3. Can standard users access read-only admin views?
4. Should audit trail track permission-based actions?

**Current Spec:** Permission model not defined  
**Recommendation Needed:** What's the required RBAC (Role-Based Access Control) structure?

---

## ✅ Decisions Needed Summary

**Critical (Blocking Implementation):**
1. [Edge Case 4](#edge-case-4-spec-field-type-change-after-data-exists) - Spec field type migration
2. [Edge Case 6](#edge-case-6-spec-field-deletion-with-existing-data) - Spec field deletion policy
3. [Edge Case 16](#edge-case-16-edit-mode-concurrent-editing) - Concurrent edit handling
4. [Edge Case 20](#edge-case-20-admin-function-permission-levels) - Permission model

**High Priority:**
5. [Edge Case 1](#edge-case-1-dunnage-type-deactivation-during-active-workflow) - Type deactivation during active use
6. [Edge Case 5](#edge-case-5-required-spec-field-becomes-optional-or-vice-versa) - Requirement changes retroactivity
7. [Edge Case 7](#edge-case-7-dropdown-spec-option-changes) - Dropdown option lifecycle
8. [Edge Case 11](#edge-case-11-workflow-mode-switching-mid-session) - Workflow mode switching
9. [Edge Case 12](#edge-case-12-zero-or-negative-quantities) - Return/adjustment support

**Medium Priority:**
10. [Edge Case 2](#edge-case-2-duplicate-dunnage-type-names) - Type name uniqueness rules
11. [Edge Case 8](#edge-case-8-part-type-association-removal-during-active-workflow) - Association removal behavior
12. [Edge Case 14](#edge-case-14-csv-export-network-failure-communication) - Network export reliability
13. [Edge Case 18](#edge-case-18-manual-entry-grid-performance-with-large-load-count) - Maximum transaction size

**Low Priority (UX Enhancement):**
14. [Edge Case 3](#edge-case-3-type-icon-missing-or-invalid) - Icon fallback behavior
15. [Edge Case 9](#edge-case-9-part-associated-with-no-types) - Orphan parts handling
16. [Edge Case 13](#edge-case-13-extremely-large-quantities) - Maximum quantity limits
17. [Edge Case 17](#edge-case-17-edit-mode-search-performance-with-large-dataset) - Search result pagination
18. [Edge Case 19](#edge-case-19-icon-picker-custom-icons) - Custom icon support

---

## 📝 Notes

- Document decisions made in this section as they're resolved
- Link to detailed specifications when created
- Update "Last Updated" date when adding new edge cases
- Cross-reference with Module_Settings.Dunnage/CLARIFICATIONS.md for settings-related edge cases

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Total Edge Cases:** 20 (4 Critical, 5 High, 4 Medium, 7 Low Priority)

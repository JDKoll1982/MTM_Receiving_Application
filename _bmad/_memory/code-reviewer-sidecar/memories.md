# Code Reviewer - Session Memories

**Agent:** Code Review Sentinel  
**Created:** January 5, 2026  
**Last Updated:** January 5, 2026

---

## Current Session State

**Module Under Review:** None  
**CODE_REVIEW Version:** N/A  
**Total Issues:** 0  
**Fixed:** 0  
**Remaining:** 0  
**Last Action:** Agent initialized  
**Last Fix Applied:** N/A  
**Session Started:** January 5, 2026

---

## Module Analysis History

### Module_Volvo

**Review Versions:**

- V1 (January 5, 2026): 26 issues → 18 fixed (69% complete)
  - Archived: `Archived_Code_Reviews/CODE_REVIEW_V1_20260105_143000.md`
  - Key fixes: Transaction management, stored procedure migration, path validation
  - Documentation generated: VolvoSettings.md, service-volvo.instructions.md, service-volvo-masterdata.instructions.md

**Patterns Discovered:**

- Heavy use of component explosion calculations (BOM traversal)
- CSV generation for label printing (LabelView 2022 integration)
- Email formatting with discrepancy tables
- Dual-database pattern (MySQL + SQL Server read-only)
- Status constants: `VolvoShipmentStatus` class pattern

**Hardcoded Values Found:**

- MaxCsvLines: 10000 (Service_Volvo.cs)
- CSV directory: %APPDATA%\MTM_Receiving_Application\Volvo\Labels
- Max suggestions: 20 (ViewModel_Volvo_ShipmentEntry.cs)
- Min/Max skid count: 1-99
- Email greeting: "Good morning,"
- Email signature: "Thank you,\nEmployee #{EmployeeNumber}"

**Architecture Notes:**

- Service_Volvo handles core business logic
- Service_VolvoMasterData manages parts catalog (CRUD)
- ViewModel_Volvo_ShipmentEntry is the main entry form
- Component explosion is critical path - performance matters
- All database ops must use stored procedures (no raw SQL)

---

## Module_Routing

**Status:** Not yet analyzed  
**Notes:** Scheduled for future review

---

## Module_Reporting

**Status:** Not yet analyzed  
**Notes:** Scheduled for future review

---

## Lessons Learned

### Transaction Management

- Always wrap multi-insert operations in MySqlTransaction
- Rollback on ANY failure to prevent partial data
- Example: Service_Volvo.SaveShipmentAsync required transaction for shipment + lines

### Build Error Patterns

- Missing using statements after adding MySqlConnection
- Orphaned code blocks after refactoring
- Path issues in stored procedure calls

### Dependency Detection

- Stored proc must exist before DAO can reference it
- Constants class must exist before service can use it
- Validation method in service before ViewModel calls it

### Fix Grouping Insights

- Group by file reduces context switching
- Group by dependency prevents build breaks
- Smart grouping > strict severity order

---

## User Preferences

**Communication Style:** Minimal chatter, focus on execution  
**Build Frequency:** After each fix (strict)  
**Test Execution:** Skip (too time-consuming)  
**Documentation Detail:** Comprehensive (GitHub Copilot instruction format)  
**Flags Used:** None yet

---

## Statistics

**Total Modules Analyzed:** 1 (Module_Volvo)  
**Total Issues Found:** 26  
**Total Issues Fixed:** 18  
**Total Stored Procs Created:** 1 (sp_volvo_shipment_update)  
**Total Service Docs Generated:** 2  
**Total Settings Docs Generated:** 1  
**Average Fix Time:** ~3 minutes per fix  
**Build Failures Encountered:** 2 (all recovered)

---

## Notes for Next Session

- Module_Volvo still has 8 unfixed issues (database-dependent, performance, authorization)
- User may request new module analysis or resume Module_Volvo
- Consider implementing batch fix mode for performance issues
- Explore auto-detection of circular dependencies in component relationships

---

## Knowledge Base

### Critical File Locations

- Constitution: `.specify/memory/constitution.md`
- MVVM Pattern: `.github/instructions/mvvm-pattern.instructions.md`
- DAO Pattern: `.github/instructions/dao-pattern.instructions.md`
- Window Sizing: `.github/instructions/window-sizing.instructions.md`
- Copilot Instructions: `.github/copilot-instructions.md`

### Database Conventions

- MySQL: `mtm_receiving_application` database
- SQL Server: `VISUAL/MTMFG` database (READ ONLY)
- Stored procs: `Database/StoredProcedures/{Module}/sp_{operation}.sql`
- Migrations: `Database/Migrations/{###}_{description}.sql`

### Severity Emoji Mapping

- 🔴 = CRITICAL
- 🟡 = SECURITY
- 🟠 = DATA
- 🔵 = QUALITY
- 🟣 = PERFORMANCE
- 🔧 = MAINTAIN
- 🟢 = EDGE CASE
- 🎨 = UI DESIGN
- 👤 = UX
- 🟤 = LOGGING/DOCS

---

**End of Memories** - Updated automatically each session

# Tasks: Database Setup + Data Layer - Settings System

**Priority**: CRITICAL (Week 1-2)  
**Phase**: Foundation  
**Status**: In Progress

**Note**: This phase now includes DAO implementation since DAOs are required to test stored procedures effectively.

---

## Prerequisites

### Files to Read

**Specification Documents:**

- `specs/005-settings-system-redesign/SPECIFICATION.md` - Complete specification
- `specs/005-settings-system-redesign/README.md` - Overview and features
- `specs/005-settings-system-redesign/DEVELOPER_GUIDE.md` - Implementation guide

**Database Scripts:**

- `Database/Schemas/12_settings_system_schema.sql` - Complete schema (Implemented)
- `Database/StoredProcedures/Settings/*.sql` - Settings stored procedures (Implemented)

> **Note**: This repository implements Settings data layer under `Module_Settings` (models/DAOs/views). Several tasks below reference `Module_Core/...` paths from the spec; the equivalent implementations exist under `Module_Settings/...`.

**Mockup & Testing:**

- `specs/005-settings-system-redesign/mockups/10-dev-database-test.svg` - Test view mockup
- `specs/005-settings-system-redesign/mockups/10-dev-database-test.md` - Test view implementation guide

**Reference:**

- All `Module_*/SETTABLE_OBJECTS_REPORT.md` files - Source data for settings migration

### Instructions to Follow

**Database Layer:**

- `#file:.github/instructions/database-layer.instructions.md` - Database patterns
- `#file:.github/instructions/database-helpers.instructions.md` - Helper usage
- `#file:.github/instructions/dao-pattern.instructions.md` - DAO architecture
- `#file:.github/instructions/dao-instance-pattern.instructions.md` - Instance-based DAOs

**MVVM & Code Patterns:**

- `#file:.github/instructions/mvvm-models.instructions.md` - Model creation
- `#file:.github/instructions/mvvm-viewmodels.instructions.md` - ViewModel patterns
- `#file:.github/instructions/mvvm-views.instructions.md` - View/XAML patterns

**Documentation:**

- `#file:.github/instructions/markdown-documentation.instructions.md` - Doc standards
- `#file:.github/instructions/plantuml-guide.instructions.md` - Diagram standards

### MCP Servers to Use

**Filesystem Operations:**

- `#file:docs/mcp/02-tool-catalog.md` - MCP Filesystem tools reference
- Use `mcp_filesystem_*` tools for reading/writing SQL files and C# code

**Code Navigation:**

- Use Serena MCP for exploring existing database patterns
- `mcp_oraios_serena_onboarding` - Understand current DB structure
- `mcp_oraios_serena_find_symbol` - Find existing stored procedure patterns
- `mcp_oraios_serena_find_referencing_symbols` - Find DAO usage patterns

---

## Format: `[ID] [P?] Description`

- **[P]**: Can run in parallel
- File paths are absolute from repository root

---

## Phase 1: Database Schema Design

**Goal**: Create complete MySQL schema with all tables, indexes, and constraints

- [x] **DB001** [P] Review all SETTABLE_OBJECTS reports and extract 79 settings into spreadsheet
- [x] **DB002** [P] Design `system_settings` table structure with all columns per spec
- [x] **DB003** [P] Design `user_settings` table with foreign keys to system_settings
- [x] **DB004** [P] Design `settings_audit_log` table with change tracking columns
- [x] **DB005** [P] Design `package_type_mappings` table per Receiving requirements
- [x] **DB006** [P] Design `package_types` table for CRUD operations (from modal requirements)
- [x] **DB007** [P] Design `routing_rules` table with pattern matching support
- [x] **DB008** [P] Design `scheduled_reports` table with schedule string parsing
- [x] **DB009** Create complete `Database/Schemas/12_settings_system_schema.sql` with all 7 tables
- [x] **DB010** Add all indexes: category, scope, permission_level, priority, next_run_date
- [x] **DB011** Add all foreign key constraints with ON DELETE/UPDATE rules
- [x] **DB012** Add unique constraints: (category, setting_key), (name), (code), (match_type, pattern)
- [x] **DB013** Add column comments for all tables describing purpose and usage

**Checkpoint**: Schema file ready for deployment testing

---

## Phase 2: Initial Data Migration

**Goal**: Seed database with 79 settings from SETTABLE_OBJECTS reports

- [ ] **DB014** Create INSERT statements for System Settings category (6 settings)
- [ ] **DB015** [P] Create INSERT statements for Security category (6 settings)
- [ ] **DB016** [P] Create INSERT statements for ERP Integration category (6 settings)
- [ ] **DB017** [P] Create INSERT statements for Receiving category (3 settings)
- [ ] **DB018** [P] Create INSERT statements for Dunnage category (2 settings)
- [ ] **DB019** [P] Create INSERT statements for Routing category (10 settings)
- [ ] **DB020** [P] Create INSERT statements for Volvo category (1 setting)
- [ ] **DB021** [P] Create INSERT statements for Reporting category (4 settings)
- [ ] **DB022** [P] Create INSERT statements for User Defaults (remaining settings)
- [x] **DB023** Add package type mappings: MCC→Coils, MMF→Sheets, DEFAULT→Skids
- [ ] **DB024** Add validation rules JSON for all settings requiring validation
- [ ] **DB025** Test INSERT statements execute without errors on clean database

**Checkpoint**: All 79 settings seeded successfully

---

## Phase 3: Stored Procedures - System Settings

**Goal**: Create all CRUD operations for system settings

- [x] **DB026** [P] Create `sp_SystemSettings_GetAll` - Return all settings
- [x] **DB027** [P] Create `sp_SystemSettings_GetByCategory` - Filter by category with optional sub_category
- [x] **DB028** [P] Create `sp_SystemSettings_GetByKey` - Get single setting by category + key
- [x] **DB029** Create `sp_SystemSettings_UpdateValue` - Update with audit logging
- [x] **DB030** Create `sp_SystemSettings_ResetToDefault` - Reset with audit logging
- [x] **DB031** Create `sp_SystemSettings_SetLocked` - Lock/unlock with audit logging
- [x] **DB032** Test all system settings procedures with sample data
- [x] **DB033** Verify audit log entries created for all update operations

**Checkpoint**: System settings CRUD operations complete and tested

---

## Phase 4: Stored Procedures - User Settings

**Goal**: Create user preference override operations

- [x] **DB034** [P] Create `sp_UserSettings_Get` - Get with fallback to system default
- [x] **DB035** [P] Create `sp_UserSettings_GetAllForUser` - All preferences for user
- [x] **DB036** Create `sp_UserSettings_Set` - Create/update override with audit
- [x] **DB037** Create `sp_UserSettings_Reset` - Remove single override
- [x] **DB038** Create `sp_UserSettings_ResetAll` - Clear all user overrides
- [x] **DB039** Test user settings procedures with sample user data
- [x] **DB040** Verify cascade behavior when system setting is updated/deleted

**Checkpoint**: User settings override system complete

---

## Phase 5: Stored Procedures - Package Operations

**Goal**: Create package type and mapping CRUD operations

- [x] **DB041** [P] Create `sp_PackageTypeMappings_GetAll` - All active mappings
- [x] **DB042** [P] Create `sp_PackageTypeMappings_GetByPrefix` - Lookup with default fallback
- [x] **DB043** Create `sp_PackageTypeMappings_Insert` - Add new mapping
- [x] **DB044** Create `sp_PackageTypeMappings_Update` - Modify mapping
- [x] **DB045** Create `sp_PackageTypeMappings_Delete` - Soft delete
- [x] **DB046** [P] Create `sp_PackageType_GetAll` - List all package types
- [x] **DB047** [P] Create `sp_PackageType_GetById` - Get single package type
- [x] **DB048** Create `sp_PackageType_Insert` - Add new type with validation
- [x] **DB049** Create `sp_PackageType_Update` - Modify type
- [x] **DB050** Create `sp_PackageType_Delete` - Delete with usage validation
- [x] **DB051** Create `sp_PackageType_UsageCount` - Count packages using this type
- [x] **DB052** Test all package procedures with sample data

**Checkpoint**: Package type system operational

---

## Phase 6: Stored Procedures - Routing Rules

**Goal**: Create routing rule CRUD with pattern matching

- [x] **DB053** [P] Create `sp_RoutingRule_GetAll` - List all active rules ordered by priority
- [x] **DB054** [P] Create `sp_RoutingRule_GetById` - Get single rule
- [x] **DB055** Create `sp_RoutingRule_Insert` - Add rule with pattern and priority
- [x] **DB056** Create `sp_RoutingRule_Update` - Modify rule
- [x] **DB057** Create `sp_RoutingRule_Delete` - Delete rule

- [x] **DB058** Create `sp_RoutingRule_FindMatch` - Find matching rule (for testing)
- [x] **DB058** Create `sp_RoutingRule_GetByPartNumber` - Find matching rule (Part Number wrapper for spec)
- [ ] **DB059** Test wildcard pattern matching (* support)
- [ ] **DB060** Test priority ordering (lower numbers first)

**Checkpoint**: Routing rules system operational

---

## Phase 7: Stored Procedures - Scheduled Reports

**Goal**: Create scheduled report CRUD operations

- [x] **DB061** [P] Create `sp_ScheduledReport_GetAll` - List all reports
- [x] **DB062** [P] Create `sp_ScheduledReport_GetActive` - Only active reports with next_run_date
- [x] **DB063** [P] Create `sp_ScheduledReport_GetById` - Get single report
- [x] **DB064** Create `sp_ScheduledReport_Insert` - Add report with schedule parsing
- [x] **DB065** Create `sp_ScheduledReport_Update` - Modify report and recalculate next_run_date
- [x] **DB066** Create `sp_ScheduledReport_Delete` - Delete report
- [x] **DB067** Create `sp_ScheduledReport_UpdateLastRun` - Mark as executed
- [ ] **DB068** Test schedule string parsing (Daily, Weekly, Monthly)

**Checkpoint**: Scheduled reports system operational

---

## Phase 8: Stored Procedures - Audit Log

**Goal**: Create audit trail query operations

- [x] **DB069** [P] Create `sp_SettingsAuditLog_Get` - Get history with filters
- [x] **DB070** [P] Create `sp_SettingsAuditLog_GetBySetting` - History for specific setting
- [x] **DB071** [P] Create `sp_SettingsAuditLog_GetByUser` - Changes by specific user
- [ ] **DB072** Test audit log queries with various date ranges

**Checkpoint**: Audit trail queries operational

---

## Phase 9: Data Layer - Models

**Goal**: Create all model classes for settings system

- [x] **DB073** [P] Create `Module_Settings/Models/Model_SystemSetting.cs` with all properties per spec
- [x] **DB074** [P] Create `Module_Settings/Models/Model_UserSetting.cs` with navigation properties
- [x] **DB075** [P] Create `Module_Settings/Models/Model_SettingValue.cs` helper class with type conversions
- [x] **DB076** [P] Create `Module_Settings/Models/Model_PackageTypeMapping.cs`
- [x] **DB077** [P] Create `Module_Settings/Models/Model_PackageType.cs`
- [x] **DB078** [P] Create `Module_Settings/Models/Model_RoutingRule.cs`
- [x] **DB079** [P] Create `Module_Settings/Models/Model_ScheduledReport.cs`
- [x] **DB080** [P] Create `Module_Settings/Models/Model_SettingsAuditLog.cs`
- [x] **DB081** [P] Create `Module_Settings/Models/Model_ConnectionTestResult.cs` (for test view)
- [x] **DB082** [P] Create `Module_Settings/Models/Model_TableTestResult.cs` (for test view)
- [x] **DB083** [P] Create `Module_Settings/Models/Model_StoredProcedureTestResult.cs` (for test view)
- [x] **DB084** [P] Create `Module_Settings/Models/Model_DaoTestResult.cs` (for test view)
- [ ] **DB085** Verify all models use `[ObservableProperty]` and inherit from `ObservableObject`
- [ ] **DB086** Verify all models have data annotations matching database constraints

**Checkpoint**: All model classes created

---

## Phase 10: Data Layer - DAOs

**Goal**: Create instance-based DAOs for all settings operations

### System Settings DAO

- [x] **DB087** Create `Module_Settings/Data/Dao_SystemSettings.cs` skeleton with constructor
- [x] **DB088** Implement `GetAllAsync()` calling `sp_SystemSettings_GetAll`
- [x] **DB089** Implement `GetByCategoryAsync(string category)` calling `sp_SystemSettings_GetByCategory`
- [x] **DB090** Implement `GetByKeyAsync(string category, string key)` calling `sp_SystemSettings_GetByKey`
- [x] **DB091** Implement `UpdateValueAsync(int id, string newValue, int userId)` with audit
- [x] **DB092** Implement `ResetToDefaultAsync(int id, int userId)` with audit
- [x] **DB093** Implement `SetLockedAsync(int id, bool locked, int userId)` with audit
- [x] **DB094** Implement `MapFromDataRow()` helper method
- [ ] **DB095** Test Dao_SystemSettings with all operations

### User Settings DAO

- [x] **DB096** Create `Module_Settings/Data/Dao_UserSettings.cs` skeleton
- [x] **DB097** Implement `GetAsync(int userId, string category, string key)` with fallback
- [x] **DB098** Implement `GetAllForUserAsync(int userId)`
- [x] **DB099** Implement `SetAsync(int userId, int settingId, string value)` with audit
- [x] **DB100** Implement `ResetAsync(int userId, int settingId)` remove override
- [x] **DB101** Implement `ResetAllAsync(int userId)` clear all user overrides
- [ ] **DB102** Test Dao_UserSettings with sample user data

### Package Type DAO

- [x] **DB103** Create `Module_Settings/Data/Dao_PackageType.cs` skeleton
- [x] **DB104** Implement `GetAllAsync()` calling `sp_PackageType_GetAll`
- [x] **DB105** Implement `GetByIdAsync(int id)` calling `sp_PackageType_GetById`
- [x] **DB106** Implement `InsertAsync(Model_PackageType packageType)` with validation
- [x] **DB107** Implement `UpdateAsync(Model_PackageType packageType)`
- [x] **DB108** Implement `DeleteAsync(int id)` with usage validation
- [x] **DB109** Implement `GetUsageCountAsync(int id)` calling `sp_PackageType_UsageCount`
- [ ] **DB110** Test Dao_PackageType CRUD operations

### Package Type Mapping DAO

- [x] **DB111** Create `Module_Settings/Data/Dao_PackageTypeMappings.cs` skeleton
- [x] **DB112** Implement `GetAllAsync()` active mappings
- [x] **DB113** Implement `GetByPrefixAsync(string prefix)` with default fallback
- [x] **DB114** Implement `InsertAsync(Model_PackageTypeMapping mapping)`
- [x] **DB115** Implement `UpdateAsync(Model_PackageTypeMapping mapping)`
- [x] **DB116** Implement `DeleteAsync(int id)` soft delete
- [ ] **DB117** Test Dao_PackageTypeMapping operations

### Routing Rule DAO

- [x] **DB118** Create `Module_Settings/Data/Dao_RoutingRule.cs` skeleton
- [x] **DB119** Implement `GetAllAsync()` ordered by priority
- [x] **DB120** Implement `GetByIdAsync(int id)`
- [x] **DB121** Implement `InsertAsync(Model_RoutingRule rule)` with pattern validation
- [x] **DB122** Implement `UpdateAsync(Model_RoutingRule rule)`
- [x] **DB123** Implement `DeleteAsync(int id)`
- [x] **DB124** Implement `GetByPartNumberAsync(string partNumber)` for testing pattern matching
- [ ] **DB125** Test Dao_RoutingRule with wildcard patterns

> **Note**: This repo supports both `FindMatchAsync(string matchType, string value)` and `GetByPartNumberAsync(string partNumber)` (spec wrapper).

### Scheduled Report DAO

- [x] **DB126** Create `Module_Settings/Data/Dao_ScheduledReport.cs` skeleton
- [x] **DB127** Implement `GetAllAsync()`
- [x] **DB128** Implement `GetActiveAsync()` only active with next_run_date
- [x] **DB129** Implement `GetByIdAsync(int id)`
- [x] **DB130** Implement `InsertAsync(Model_ScheduledReport report)` with schedule parsing
- [x] **DB131** Implement `UpdateAsync(Model_ScheduledReport report)` recalculate next_run_date
- [x] **DB132** Implement `DeleteAsync(int id)`
- [x] **DB133** Implement `UpdateLastRunAsync(int id, DateTime lastRun)`
- [ ] **DB134** Test Dao_ScheduledReport schedule parsing

> **Note**: This repo supports both `GetDueAsync()` and `GetActiveAsync()` (spec wrapper), and `UpdateLastRunAsync(int id, DateTime lastRunDate, DateTime nextRunDate)`.

## Phase 11: Development Test View

**Goal**: Create database test view for validation

- [x] **DB135** Create `Module_Settings/ViewModels/ViewModel_Settings_DatabaseTest.cs` skeleton
- [x] **DB136** Implement connection test logic (implemented directly in ViewModel)
- [x] **DB137** Implement schema validation (table existence checks)
- [x] **DB138** Implement stored procedure tests (existence checks)
- [x] **DB139** Implement DAO tests (executes DAO read ops)
- [x] **DB140** Implement test logging functionality
- [ ] **DB141** Implement export results command
- [x] **DB141** Implement export results command
- [x] **DB142** Create `Module_Settings/Views/View_Settings_DatabaseTest.xaml` per mockup (page-based)
- [x] **DB143** Implement test view code-behind (`View_Settings_DatabaseTest.xaml.cs`)
- [x] **DB144** Add "Settings DB Test" button to MainWindow.xaml (dev mode only)
- [x] **DB145** Register Settings DB Test ViewModel in DI container
- [ ] **DB146** Test full database validation workflow

**Checkpoint**: Database test view operational

---

## Phase 12: Deployment and Validation

**Goal**: Deploy schema and validate all operations

- [ ] **DB147** Validate and update `Database/Deploy/Deploy-Database-GUI-Fixed.ps1` to include settings system deployment
- [x] **DB147** Validate and update `Database/Deploy/Deploy-Database-GUI-Fixed.ps1` to include settings system deployment (script deploys all schema + stored procedure .sql files)
- [ ] **DB148** Deploy schema to test database
- [ ] **DB149** Deploy all stored procedures to test database
- [ ] **DB150** Run seed data INSERT statements
- [ ] **DB151** Execute all stored procedures with test data
- [ ] **DB152** Verify all 79 settings exist in system_settings table
- [ ] **DB153** Verify all indexes created correctly
- [ ] **DB154** Verify all foreign keys enforce constraints
- [ ] **DB155** Test audit logging works for all operations
- [ ] **DB156** Run database test view and verify all tests pass
- [ ] **DB157** Document any schema differences from specification
- [ ] **DB158** Create rollback script for emergency revert

**Checkpoint**: Database foundation complete and validated

---

## Testing Checklist

### Schema Validation

- [ ] All 7 tables created successfully
- [ ] All indexes exist and are used in queries
- [ ] All foreign keys enforce referential integrity
- [ ] All unique constraints prevent duplicates
- [ ] Column types match specification

### Data Validation

- [ ] 79 settings inserted successfully
- [ ] All categories present (System, Security, ERP, Receiving, Dunnage, Routing, Volvo, Reporting)
- [ ] All permission levels represented
- [ ] All data types represented (string, int, boolean, json, path, password, email)
- [ ] Package type mappings work correctly

### Stored Procedure Validation

- [ ] All 41 stored procedures execute without errors
- [ ] System settings CRUD operations work
- [ ] User settings override system works
- [ ] Package operations work with validation
- [ ] Routing rules work with pattern matching
- [ ] Scheduled reports work with schedule parsing
- [ ] Audit log captures all changes
- [ ] Locked settings cannot be modified
- [ ] Sensitive settings flagged correctly

### DAO Validation

- [ ] All 7 DAOs execute stored procedures correctly
- [ ] All DAOs return `Model_Dao_Result<T>` types
- [ ] All DAOs handle errors without throwing exceptions
- [ ] All DAOs use instance-based pattern
- [ ] All DAOs registered in DI container
- [ ] MapFromDataRow methods work for all models

### Development Test View Validation

- [ ] Database connection test works
- [ ] All 7 tables validated successfully
- [ ] All 41 stored procedures tested
- [ ] All 7 DAOs operational
- [ ] Test logs capture all operations
- [ ] Export results functionality works
- [ ] View accessible only in development mode

### Performance Validation

- [ ] All queries use indexes (check EXPLAIN output)
- [ ] No full table scans on large tables
- [ ] Audit log inserts don't slow down updates
- [ ] DAO operations complete in < 100ms

---

## Success Criteria

✅ **Database + Data Layer Ready When:**

1. All 7 tables deployed with correct schema
2. All 79 settings seeded from SETTABLE_OBJECTS reports
3. All 25+ stored procedures tested and working
4. All 12 model classes created with ObservableProperty
5. All 7 DAOs operational and returning Model_Dao_Result
6. Database test view validates all operations successfully
7. Audit logging captures all changes
8. No SQL errors in deployment or execution
9. All DAOs registered in DI container
10. Rollback script tested and ready

---

## Next Phase

Once this phase is complete, proceed to:

- `tasks-02-services.md` - Business logic services (Settings, Cache, Encryption)
- `tasks-03-ui.md` - ViewModels and Views implementation

---

## Notes

- Use MySQL Workbench to visualize foreign key relationships
- Test all stored procedures in isolation before integration
- Keep backup of test data for regression testing
- Document any deviations from specification in comments
- Use database test view throughout development to verify changes
- DAOs must be instance-based and registered as singletons in DI
- All database operations must be async
- Never throw exceptions from DAOs - return failure results

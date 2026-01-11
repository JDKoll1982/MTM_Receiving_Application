# Settings System Redesign - Deliverables

This folder contains the complete redesign specification for the MTM Receiving Application's settings system.

## 📋 Overview

The settings system has been redesigned to:

- ✅ Migrate all 79 settable objects from hardcoded values and `appsettings.json` to MySQL database
- ✅ Implement role-based access control (User, Operator, Admin, Developer, Super Admin)
- ✅ Support user-specific preference overrides
- ✅ Provide complete audit trail of all configuration changes
- ✅ Encrypt sensitive credentials (passwords, API keys)
- ✅ Auto-save with validation and inline error display
- ✅ Modern WinUI 3 interface with search/filter capabilities

## 📁 Deliverables

### 1. Mockups (`mockups/`)

#### Main Navigation

- **`settings-mode-selection.svg/.md`** - Redesigned mode selection page with 9 category cards

#### Individual Settings Pages (9 pages)

- **`01-system-settings.svg/.md`** - Development, database, logging
- **`02-security-session.svg/.md`** - Authentication, session, encryption
- **`03-erp-integration.svg/.md`** - Infor Visual connection, caching
- **`04-receiving.svg/.md`** - Workflow, package types (DataGrid)
- **`05-dunnage.svg/.md`** - Tracking, inventory alerts
- **`06-routing.svg/.md`** - Auto-routing rules, locations
- **`07-volvo.svg/.md`** - EDI, master data sync
- **`08-reporting.svg/.md`** - Export formats, scheduled reports
- **`09-user-preferences.svg/.md`** - Theme, fonts, personalization

#### Modal Dialogs (10 modals)

- **`01-system-modal-test-db.svg/.md`** - MySQL connection test
- **`02-security-modal-rotate-key.svg/.md`** - Encryption key rotation
- **`03-erp-modal-test-connection.svg/.md`** - Infor Visual test
- **`04-receiving-modal-add-type.svg/.md`** - Add/edit package type
- **`04-receiving-modal-delete-confirm.svg/.md`** - Delete confirmation
- **`06-routing-modal-add-rule.svg/.md`** - Add/edit routing rule
- **`07-volvo-modal-sync.svg/.md`** - Manual sync trigger
- **`08-reporting-modal-schedule.svg/.md`** - Schedule report
- **`09-preferences-modal-reset.svg/.md`** - Reset confirmation
- **`MODAL_INDEX.md`** - Complete modal & view reference guide

#### Development Tools (1 view)

- **`10-dev-database-test.svg/.md`** - Comprehensive database validation and testing tool

**Total:** 29 SVG files + 29 MD files = **58 mockup files**

### 2. Templates (`templates/`)

- **`SettingsPageTemplate.xaml`** - Reusable WinUI 3 template for all settings pages
  - Grouped card layout
  - Auto-save with debounce
  - Inline validation errors
  - Tooltips for help text
  - File/folder pickers with test connection
  - Password fields with show/hide
  - Locked settings overlay
  - Search/filter box
  - Reset to defaults button

### 3. Database Schema (`../../Database/Schemas/`)

- **`settings_system_schema.sql`** - Complete database schema
  - `system_settings` - All configurable settings (79 rows seeded)
  - `user_settings` - User-specific overrides
  - `settings_audit_log` - Change tracking
  - `package_type_mappings` - Part prefix → package type rules
  - All indexes and foreign keys
  - Initial data migration from `appsettings.json`

### 4. Stored Procedures (`../../Database/StoredProcedures/`)

- **`sp_SettingsSystem.sql`** - All CRUD operations (25+ procedures)
  - System settings: Get, Update, Reset, Lock/Unlock
  - User settings: Get, Set, Reset (individual & all)
  - Package mappings: Get, Insert, Update, Delete
  - Package types: CRUD with usage validation
  - Routing rules: CRUD with pattern matching
  - Scheduled reports: CRUD with schedule parsing
  - Audit log: Get history
  - All procedures enforce locking rules and log changes

### 5. Specification (`SPECIFICATION.md`)

- **Complete implementation plan** including:
  - Architecture overview
  - Data models (C# classes with sample code)
  - DAO patterns (instance-based)
  - Service layer design
  - ViewModel architecture
  - Security & encryption strategy
  - Migration plan (6-week phased approach)
  - Testing plan
  - Rollback plan
  - Success criteria

## 🎯 Key Features

### Role-Based Access Control

| Role | Can Access | Can Modify |
|------|-----------|------------|
| User | Own preferences | Own user settings |
| Operator | Module settings | Own preferences + operational toggles |
| Admin | Most settings | System settings (non-critical) |
| Developer | Dev/test settings | Mock flags, retry logic |
| Super Admin | All settings | Everything including credentials |

### Settings Categories (79 Total Settings)

| Category | Count | Examples |
|----------|-------|----------|
| System | 6 | MockData, Environment, Database Retries |
| Security | 6 | Timeouts, PIN Length, Lockout Delay |
| ERP Integration | 6 | Server, Database, Credentials, Site ID |
| Receiving | 3 | Default Mode, Grid Delays |
| Dunnage | 2 | Default Mode, Grid Delay |
| Routing | 10 | CSV Paths, Validation, Duplicate Detection |
| Volvo | 1 | History Filter Options |
| Reporting | 4 | Export Path, CSV Formats |
| User Preferences | Variable | Per-user workflow defaults |

### Data Types Supported

- `string` - Text values
- `int` - Numeric values
- `boolean` - Toggle switches
- `json` - Complex objects
- `path` - File/folder paths
- `password` - Encrypted credentials
- `email` - Email addresses

### Validation Rules

Settings can define JSON validation rules:

```json
{
  "min": 1,
  "max": 100,
  "pattern": "^[A-Z0-9]+$",
  "allowed_values": ["option1", "option2"],
  "required": true,
  "path_must_exist": true
}
```

## 🚀 Implementation Phases

### Phase 1: Database Setup (Week 1)

- Deploy schema and stored procedures
- Seed initial data
- Test all database operations

### Phase 2: Data Layer (Week 2)

- Create models, DAOs, services
- Register in DI container
- Unit tests

### Phase 3: UI Development (Week 3-4)

- Create ViewModels for all 9 categories
- Build Views using template
- Implement auto-save and validation

### Phase 4: Security (Week 5)

- Implement encryption
- Add role checks
- Create audit log viewer

### Phase 5: Migration & Cleanup (Week 6)

- Remove hardcoded values
- Update all code references
- Integration and UAT testing

## 📊 Migration Summary

### Before (Current State)

- ❌ Settings scattered across `appsettings.json`, hardcoded constants, and inline code
- ❌ No user preferences
- ❌ No audit trail
- ❌ Credentials stored in plain text
- ❌ No role-based access
- ❌ Requires code changes for configuration updates

### After (Redesigned State)

- ✅ All settings in centralized MySQL database
- ✅ User-specific overrides supported
- ✅ Complete audit trail with who/what/when
- ✅ Sensitive data encrypted
- ✅ Role-based permissions enforced
- ✅ Administrators can change settings via UI without code deployment

## 🔒 Security Enhancements

1. **Encryption**: Sensitive settings (passwords, API keys) encrypted using AES-256 with machine-specific DPAPI key
2. **Audit Trail**: All changes logged with user, timestamp, IP address, and workstation
3. **Locked Settings**: Admins can lock critical settings to prevent accidental changes
4. **Permission Levels**: UI and service layer enforce role-based access
5. **Masked Display**: Passwords shown as `********` with reveal toggle

## 📖 Usage Example

```csharp
// Get a setting value (with user override if applicable)
var setting = await _settingsService.GetSettingAsync("Receiving", "DefaultReceivingMode", userId: 123);
string mode = setting.Data.AsString(); // "guided"

// Save a setting (creates audit log entry)
await _settingsService.SaveSettingAsync(
    "Routing", 
    "CsvRetryMaxAttempts", 
    "5", 
    userId: 456
);

// Reset user preference to system default
await _settingsService.ResetSettingAsync(
    "Receiving", 
    "DefaultReceivingMode", 
    userId: 123, 
    isUserOverride: true
);
```

## 🧪 Testing Checklist

- [ ] Database schema deploys without errors
- [ ] All stored procedures execute successfully
- [ ] DAOs return expected results
- [ ] Service validation catches invalid values
- [ ] Encryption/decryption works correctly
- [ ] Role-based access prevents unauthorized changes
- [ ] User overrides take precedence over system defaults
- [ ] Audit log captures all changes
- [ ] UI auto-save works without data loss
- [ ] Search/filter performs as expected
- [ ] Locked settings cannot be modified
- [ ] Package type mappings editable via DataGrid
- [ ] File/folder pickers launch correctly
- [ ] Test connection validates paths
- [ ] Password fields mask/reveal properly

## 📝 Next Steps

1. **Review** this specification with stakeholders
2. **Approve** database schema and UI design
3. **Schedule** Phase 1 database deployment
4. **Assign** development tasks to team
5. **Set up** development environment
6. **Begin** implementation following phased approach

## 📞 Questions?

Contact the development team for clarifications on:

- Database schema design
- ViewModel architecture
- Security implementation
- Migration timeline
- Testing requirements

---

**Status:** ✅ Design Complete - Ready for Implementation  
**Last Updated:** January 10, 2026  
**Specification Version:** 1.0

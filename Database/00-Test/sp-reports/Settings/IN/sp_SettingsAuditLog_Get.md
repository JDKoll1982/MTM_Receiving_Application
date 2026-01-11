# sp_SettingsAuditLog_Get

**Category:** Settings
**Parameter Types:** IN
**Generated:** 2026-01-11 14:13:05

## Usage Summary

**Total Usages:** 6

## Usage Details

| File | Class | Method | Line | Has Callers | Caller Count |
|------|-------|--------|------|-------------|--------------|
| Module_Settings\Data\Dao_SettingsAuditLog.cs | Dao_SettingsAuditLog | `GetAsync` | 39 | ✓ | 3 |
| Module_Settings\Data\Dao_SettingsAuditLog.cs | Dao_SettingsAuditLog | `GetBySettingAsync` | 58 | ✓ | 1 |
| Module_Settings\Data\Dao_SettingsAuditLog.cs | Dao_SettingsAuditLog | `GetByUserAsync` | 77 | ✓ | 2 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 547 | ✓ | 1 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 548 | ✓ | 1 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 549 | ✓ | 1 |

## Code Samples

```csharp
// Module_Settings\Data\Dao_SettingsAuditLog.cs:39
"sp_SettingsAuditLog_Get",

// Module_Settings\Data\Dao_SettingsAuditLog.cs:58
"sp_SettingsAuditLog_GetBySetting",

// Module_Settings\Data\Dao_SettingsAuditLog.cs:77
"sp_SettingsAuditLog_GetByUser",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:547
"sp_SettingsAuditLog_Get",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:548
"sp_SettingsAuditLog_GetBySetting",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:549
"sp_SettingsAuditLog_GetByUser"

```

## Method References

### Dao_SettingsAuditLog.GetAsync
**Called by (3 references):**
- Same file (1 calls)
- Module_Settings\Data\Dao_UserSettings.cs
- Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs

### Dao_SettingsAuditLog.GetBySettingAsync
**Called by (1 references):**
- Same file (1 calls)

### Dao_SettingsAuditLog.GetByUserAsync
**Called by (2 references):**
- Module_Receiving\Data\Dao_PackageTypePreference.cs
- Same file (1 calls)

### ViewModel_Settings_DatabaseTest.TestStoredProceduresAsync
**Called by (1 references):**
- Same file (2 calls)



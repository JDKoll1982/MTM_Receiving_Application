# sp_ScheduledReport_Update

**Category:** Settings
**Parameter Types:** IN
**Generated:** 2026-01-11 13:34:59

## Usage Summary

**Total Usages:** 4

## Usage Details

| File | Class | Method | Line | Has Callers | Caller Count |
|------|-------|--------|------|-------------|--------------|
| Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `UpdateAsync` | 120 | ✓ | 19 |
| Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `UpdateLastRunAsync` | 142 | ✓ | 1 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 542 | ✓ | 1 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 543 | ✓ | 1 |

## Code Samples

```csharp
// Module_Settings\Data\Dao_ScheduledReport.cs:120
"sp_ScheduledReport_Update",

// Module_Settings\Data\Dao_ScheduledReport.cs:142
"sp_ScheduledReport_UpdateLastRun",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:542
"sp_ScheduledReport_Update",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:543
"sp_ScheduledReport_UpdateLastRun",

```

## Method References

### Dao_ScheduledReport.UpdateAsync
**Called by (19 references):**
- Module_Core\Services\Database\Service_MySQL_Dunnage.cs
- Module_Dunnage\Data\Dao_DunnageLoad.cs
- Module_Dunnage\Data\Dao_DunnagePart.cs
- Module_Dunnage\Data\Dao_DunnageSpec.cs
- Module_Dunnage\Data\Dao_DunnageType.cs
- Module_Dunnage\Data\Dao_InventoriedDunnage.cs
- Module_Dunnage\ViewModels\ViewModel_Dunnage_AdminInventoryViewModel.cs
- Module_Settings\Data\Dao_PackageType.cs
- Module_Settings\Data\Dao_PackageTypeMappings.cs
- Module_Settings\Data\Dao_RoutingRule.cs
- Same file (1 calls)
- Module_Volvo\Data\Dao_VolvoPart.cs
- Module_Volvo\Data\Dao_VolvoShipment.cs
- Module_Volvo\Data\Dao_VolvoShipmentLine.cs
- Module_Volvo\Interfaces\IDao_VolvoPart.cs
- Module_Volvo\Interfaces\IDao_VolvoShipment.cs
- Module_Volvo\Interfaces\IDao_VolvoShipmentLine.cs
- Module_Volvo\Services\Service_Volvo.cs
- Module_Volvo\Services\Service_VolvoMasterData.cs

### Dao_ScheduledReport.UpdateLastRunAsync
**Called by (1 references):**
- Same file (1 calls)

### ViewModel_Settings_DatabaseTest.TestStoredProceduresAsync
**Called by (1 references):**
- Same file (2 calls)



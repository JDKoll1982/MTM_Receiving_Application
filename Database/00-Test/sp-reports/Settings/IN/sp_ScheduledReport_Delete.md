# sp_ScheduledReport_Delete

**Category:** Settings
**Parameter Types:** IN
**Generated:** 2026-01-11 16:28:36

## Usage Summary

**Total Usages:** 2

## Usage Details

| File | Class | Method | Line | Has Callers | Caller Count |
|------|-------|--------|------|-------------|--------------|
| Module_Settings\Data\Dao_ScheduledReport.cs | Dao_ScheduledReport | `DeleteAsync` | 160 | ✓ | 15 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 545 | ✓ | 1 |

## Code Samples

```csharp
// Module_Settings\Data\Dao_ScheduledReport.cs:160
"sp_ScheduledReport_Delete",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:545
"sp_ScheduledReport_Delete",

```

## Method References

### Dao_ScheduledReport.DeleteAsync
**Called by (15 references):**
- Module_Core\Services\Database\Service_MySQL_Dunnage.cs
- Module_Dunnage\Data\Dao_DunnageCustomField.cs
- Module_Dunnage\Data\Dao_DunnageLoad.cs
- Module_Dunnage\Data\Dao_DunnagePart.cs
- Module_Dunnage\Data\Dao_DunnageType.cs
- Module_Dunnage\Data\Dao_InventoriedDunnage.cs
- Module_Dunnage\ViewModels\ViewModel_Dunnage_AdminInventoryViewModel.cs
- Module_Settings\Data\Dao_PackageType.cs
- Module_Settings\Data\Dao_PackageTypeMappings.cs
- Module_Settings\Data\Dao_RoutingRule.cs
- Same file (1 calls)
- Module_Volvo\Data\Dao_VolvoShipment.cs
- Module_Volvo\Data\Dao_VolvoShipmentLine.cs
- Module_Volvo\Interfaces\IDao_VolvoShipmentLine.cs
- Module_Volvo\Services\Service_Volvo.cs

### ViewModel_Settings_DatabaseTest.TestStoredProceduresAsync
**Called by (1 references):**
- Same file (2 calls)



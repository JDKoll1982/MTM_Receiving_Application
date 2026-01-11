# sp_PackageTypeMappings_Delete

**Category:** Settings
**Parameter Types:** IN
**Generated:** 2026-01-11 14:13:05

## Usage Summary

**Total Usages:** 2

## Usage Details

| File | Class | Method | Line | Has Callers | Caller Count |
|------|-------|--------|------|-------------|--------------|
| Module_Settings\Data\Dao_PackageTypeMappings.cs | Dao_PackageTypeMappings | `DeleteAsync` | 117 | ✓ | 15 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 527 | ✓ | 1 |

## Code Samples

```csharp
// Module_Settings\Data\Dao_PackageTypeMappings.cs:117
"sp_PackageTypeMappings_Delete",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:527
"sp_PackageTypeMappings_Delete",

```

## Method References

### Dao_PackageTypeMappings.DeleteAsync
**Called by (15 references):**
- Module_Core\Services\Database\Service_MySQL_Dunnage.cs
- Module_Dunnage\Data\Dao_DunnageCustomField.cs
- Module_Dunnage\Data\Dao_DunnageLoad.cs
- Module_Dunnage\Data\Dao_DunnagePart.cs
- Module_Dunnage\Data\Dao_DunnageType.cs
- Module_Dunnage\Data\Dao_InventoriedDunnage.cs
- Module_Dunnage\ViewModels\ViewModel_Dunnage_AdminInventoryViewModel.cs
- Module_Settings\Data\Dao_PackageType.cs
- Same file (1 calls)
- Module_Settings\Data\Dao_RoutingRule.cs
- Module_Settings\Data\Dao_ScheduledReport.cs
- Module_Volvo\Data\Dao_VolvoShipment.cs
- Module_Volvo\Data\Dao_VolvoShipmentLine.cs
- Module_Volvo\Interfaces\IDao_VolvoShipmentLine.cs
- Module_Volvo\Services\Service_Volvo.cs

### ViewModel_Settings_DatabaseTest.TestStoredProceduresAsync
**Called by (1 references):**
- Same file (2 calls)



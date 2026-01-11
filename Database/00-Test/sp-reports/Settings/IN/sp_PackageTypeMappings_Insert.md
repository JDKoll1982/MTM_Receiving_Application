# sp_Receiving_PackageTypeMappings_Insert

**Category:** Settings
**Parameter Types:** IN
**Generated:** 2026-01-11 16:28:36

## Usage Summary

**Total Usages:** 2

## Usage Details

| File | Class | Method | Line | Has Callers | Caller Count |
|------|-------|--------|------|-------------|--------------|
| Module_Settings\Data\Dao_PackageTypeMappings.cs | Dao_PackageTypeMappings | `InsertAsync` | 75 | ✓ | 21 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 525 | ✓ | 1 |

## Code Samples

```csharp
// Module_Settings\Data\Dao_PackageTypeMappings.cs:75
"sp_Receiving_PackageTypeMappings_Insert",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:525
"sp_Receiving_PackageTypeMappings_Insert",

```

## Method References

### Dao_PackageTypeMappings.InsertAsync
**Called by (21 references):**
- Module_Core\Services\Database\Service_MySQL_Dunnage.cs
- Module_Dunnage\Data\Dao_DunnageCustomField.cs
- Module_Dunnage\Data\Dao_DunnageLoad.cs
- Module_Dunnage\Data\Dao_DunnagePart.cs
- Module_Dunnage\Data\Dao_DunnageSpec.cs
- Module_Dunnage\Data\Dao_DunnageType.cs
- Module_Dunnage\Data\Dao_InventoriedDunnage.cs
- Module_Dunnage\Views\View_Dunnage_Dialog_AddToInventoriedListDialog.xaml.cs
- Module_Settings\Data\Dao_PackageType.cs
- Same file (1 calls)
- Module_Settings\Data\Dao_RoutingRule.cs
- Module_Settings\Data\Dao_ScheduledReport.cs
- Module_Volvo\Data\Dao_VolvoPart.cs
- Module_Volvo\Data\Dao_VolvoPartComponent.cs
- Module_Volvo\Data\Dao_VolvoShipment.cs
- Module_Volvo\Data\Dao_VolvoShipmentLine.cs
- Module_Volvo\Interfaces\IDao_VolvoPart.cs
- Module_Volvo\Interfaces\IDao_VolvoShipment.cs
- Module_Volvo\Interfaces\IDao_VolvoShipmentLine.cs
- Module_Volvo\Services\Service_Volvo.cs
- Module_Volvo\Services\Service_VolvoMasterData.cs

### ViewModel_Settings_DatabaseTest.TestStoredProceduresAsync
**Called by (1 references):**
- Same file (2 calls)

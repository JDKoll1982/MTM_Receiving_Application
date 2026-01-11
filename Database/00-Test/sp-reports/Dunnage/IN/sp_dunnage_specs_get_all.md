# sp_dunnage_specs_get_all

**Category:** Dunnage
**Parameter Types:** None
**Generated:** 2026-01-11 13:34:59

## Usage Summary

**Total Usages:** 2

## Usage Details

| File | Class | Method | Line | Has Callers | Caller Count |
|------|-------|--------|------|-------------|--------------|
| Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `GetAllAsync` | 41 | ✓ | 21 |
| Module_Dunnage\Data\Dao_DunnageSpec.cs | Dao_DunnageSpec | `GetAllSpecKeysAsync` | 163 | ✓ | 5 |

## Code Samples

```csharp
// Module_Dunnage\Data\Dao_DunnageSpec.cs:41
"sp_dunnage_specs_get_all",

// Module_Dunnage\Data\Dao_DunnageSpec.cs:163
"sp_dunnage_specs_get_all_keys",

```

## Method References

### Dao_DunnageSpec.GetAllAsync
**Called by (21 references):**
- Module_Core\Services\Database\Service_MySQL_Dunnage.cs
- Module_Core\Services\Database\Service_MySQL_Receiving.cs
- Module_Dunnage\Data\Dao_DunnageLoad.cs
- Module_Dunnage\Data\Dao_DunnagePart.cs
- Same file (1 calls)
- Module_Dunnage\Data\Dao_DunnageType.cs
- Module_Dunnage\Data\Dao_InventoriedDunnage.cs
- Module_Dunnage\ViewModels\ViewModel_Dunnage_AdminInventoryViewModel.cs
- Module_Dunnage\Views\View_Dunnage_Dialog_AddToInventoriedListDialog.xaml.cs
- Module_Receiving\Data\Dao_ReceivingLoad.cs
- Module_Settings\Data\Dao_PackageType.cs
- Module_Settings\Data\Dao_PackageTypeMappings.cs
- Module_Settings\Data\Dao_RoutingRule.cs
- Module_Settings\Data\Dao_ScheduledReport.cs
- Module_Settings\Data\Dao_SystemSettings.cs
- Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs
- Module_Volvo\Data\Dao_VolvoPart.cs
- Module_Volvo\Interfaces\IDao_VolvoPart.cs
- Module_Volvo\Interfaces\IDao_VolvoShipment.cs
- Module_Volvo\Services\Service_Volvo.cs
- Module_Volvo\Services\Service_VolvoMasterData.cs

### Dao_DunnageSpec.GetAllSpecKeysAsync
**Called by (5 references):**
- Module_Core\Contracts\Services\IService_MySQL_Dunnage.cs
- Module_Core\Services\Database\Service_MySQL_Dunnage.cs
- Same file (1 calls)
- Module_Dunnage\Services\Service_DunnageCSVWriter.cs
- Module_Dunnage\ViewModels\ViewModel_Dunnage_ManualEntryViewModel.cs



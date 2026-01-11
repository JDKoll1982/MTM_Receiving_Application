# sp_SystemSettings_GetAll

**Category:** Settings
**Parameter Types:** None
**Generated:** 2026-01-11 16:28:36

## Usage Summary

**Total Usages:** 2

## Usage Details

| File | Class | Method | Line | Has Callers | Caller Count |
|------|-------|--------|------|-------------|--------------|
| Module_Settings\Data\Dao_SystemSettings.cs | Dao_SystemSettings | `GetAllAsync` | 31 | ✓ | 21 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 501 | ✓ | 1 |

## Code Samples

```csharp
// Module_Settings\Data\Dao_SystemSettings.cs:31
"sp_SystemSettings_GetAll",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:501
"sp_SystemSettings_GetAll",

```

## Method References

### Dao_SystemSettings.GetAllAsync
**Called by (21 references):**
- Module_Core\Services\Database\Service_MySQL_Dunnage.cs
- Module_Core\Services\Database\Service_MySQL_Receiving.cs
- Module_Dunnage\Data\Dao_DunnageLoad.cs
- Module_Dunnage\Data\Dao_DunnagePart.cs
- Module_Dunnage\Data\Dao_DunnageSpec.cs
- Module_Dunnage\Data\Dao_DunnageType.cs
- Module_Dunnage\Data\Dao_InventoriedDunnage.cs
- Module_Dunnage\ViewModels\ViewModel_Dunnage_AdminInventoryViewModel.cs
- Module_Dunnage\Views\View_Dunnage_Dialog_AddToInventoriedListDialog.xaml.cs
- Module_Receiving\Data\Dao_ReceivingLoad.cs
- Module_Settings\Data\Dao_PackageType.cs
- Module_Settings\Data\Dao_PackageTypeMappings.cs
- Module_Settings\Data\Dao_RoutingRule.cs
- Module_Settings\Data\Dao_ScheduledReport.cs
- Same file (1 calls)
- Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs
- Module_Volvo\Data\Dao_VolvoPart.cs
- Module_Volvo\Interfaces\IDao_VolvoPart.cs
- Module_Volvo\Interfaces\IDao_VolvoShipment.cs
- Module_Volvo\Services\Service_Volvo.cs
- Module_Volvo\Services\Service_VolvoMasterData.cs

### ViewModel_Settings_DatabaseTest.TestStoredProceduresAsync
**Called by (1 references):**
- Same file (2 calls)



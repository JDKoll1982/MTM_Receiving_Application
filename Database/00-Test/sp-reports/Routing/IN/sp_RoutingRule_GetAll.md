# sp_RoutingRule_GetAll

**Category:** Routing
**Parameter Types:** None
**Generated:** 2026-01-11 16:28:36

## Usage Summary

**Total Usages:** 2

## Usage Details

| File | Class | Method | Line | Has Callers | Caller Count |
|------|-------|--------|------|-------------|--------------|
| Module_Settings\Data\Dao_RoutingRule.cs | Dao_RoutingRule | `GetAllAsync` | 31 | ✓ | 21 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 529 | ✓ | 1 |

## Code Samples

```csharp
// Module_Settings\Data\Dao_RoutingRule.cs:31
"sp_RoutingRule_GetAll",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:529
"sp_RoutingRule_GetAll",

```

## Method References

### Dao_RoutingRule.GetAllAsync
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
- Same file (1 calls)
- Module_Settings\Data\Dao_ScheduledReport.cs
- Module_Settings\Data\Dao_SystemSettings.cs
- Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs
- Module_Volvo\Data\Dao_VolvoPart.cs
- Module_Volvo\Interfaces\IDao_VolvoPart.cs
- Module_Volvo\Interfaces\IDao_VolvoShipment.cs
- Module_Volvo\Services\Service_Volvo.cs
- Module_Volvo\Services\Service_VolvoMasterData.cs

### ViewModel_Settings_DatabaseTest.TestStoredProceduresAsync
**Called by (1 references):**
- Same file (2 calls)



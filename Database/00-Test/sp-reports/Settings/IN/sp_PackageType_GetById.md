# sp_PackageType_GetById

**Category:** Settings
**Parameter Types:** IN
**Generated:** 2026-01-11 14:13:05

## Usage Summary

**Total Usages:** 2

## Usage Details

| File | Class | Method | Line | Has Callers | Caller Count |
|------|-------|--------|------|-------------|--------------|
| Module_Settings\Data\Dao_PackageType.cs | Dao_PackageType | `GetByIdAsync` | 49 | ✓ | 13 |
| Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs | ViewModel_Settings_DatabaseTest | `TestStoredProceduresAsync` | 517 | ✓ | 1 |

## Code Samples

```csharp
// Module_Settings\Data\Dao_PackageType.cs:49
"sp_PackageType_GetById",

// Module_Settings\ViewModels\ViewModel_Settings_DatabaseTest.cs:517
"sp_PackageType_GetById",

```

## Method References

### Dao_PackageType.GetByIdAsync
**Called by (13 references):**
- Module_Core\Services\Database\Service_MySQL_Dunnage.cs
- Module_Dunnage\Data\Dao_DunnageLoad.cs
- Module_Dunnage\Data\Dao_DunnagePart.cs
- Module_Dunnage\Data\Dao_DunnageSpec.cs
- Module_Dunnage\Data\Dao_DunnageType.cs
- Same file (1 calls)
- Module_Settings\Data\Dao_RoutingRule.cs
- Module_Settings\Data\Dao_ScheduledReport.cs
- Module_Volvo\Data\Dao_VolvoPart.cs
- Module_Volvo\Data\Dao_VolvoShipment.cs
- Module_Volvo\Interfaces\IDao_VolvoShipment.cs
- Module_Volvo\Services\Service_Volvo.cs
- Module_Volvo\Services\Service_VolvoMasterData.cs

### ViewModel_Settings_DatabaseTest.TestStoredProceduresAsync
**Called by (1 references):**
- Same file (2 calls)



# sp_seed_user_default_modes

**Category:** Authentication
**Parameter Types:** IN
**Generated:** 2026-01-11 13:34:59

## Usage Summary

**Total Usages:** 2

## Usage Details

| File | Class | Method | Line | Has Callers | Caller Count |
|------|-------|--------|------|-------------|--------------|
| Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `CreateNewUserAsync` | 127 | ✓ | 4 |
| Module_Core\Data\Authentication\Dao_User.cs | Dao_User | `CreateNewUserAsync` | 133 | ✓ | 4 |

## Code Samples

```csharp
// Module_Core\Data\Authentication\Dao_User.cs:127
// Defaults are seeded server-side by sp_CreateNewUser -> sp_seed_user_default_modes.

// Module_Core\Data\Authentication\Dao_User.cs:133
"sp_seed_user_default_modes",

```

## Method References

### Dao_User.CreateNewUserAsync
**Called by (4 references):**
- Module_Core\Contracts\Services\IService_Authentication.cs
- Same file (1 calls)
- Module_Core\Services\Authentication\Service_Authentication.cs
- Module_Shared\ViewModels\ViewModel_Shared_NewUserSetup.cs



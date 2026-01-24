# MVVM Architecture Rules (MTM Receiving Application)

Last Updated: 2026-01-24

This document expands on the MVVM rules enforced by the `mvvm-architecture-enforcer` skill.

## Layer separation

### View (XAML)

- UI markup only
- Use `{x:Bind}` for binding
- No business logic in `.xaml.cs`

### ViewModel

- `partial` class
- Inherit from `ViewModel_Shared_Base`
- Call Services only (never DAOs)
- Hold UI state (`IsBusy`, `StatusMessage`, observable properties)

### Service

- Interface-based (`IService_*`)
- Orchestrates DAOs and business logic
- Handles `Model_Dao_Result` failures from DAOs

### DAO

- Instance-based (not static)
- No exceptions thrown outward
- Returns `Model_Dao_Result` / `Model_Dao_Result<T>`
- MySQL: stored procedures only
- SQL Server / Infor Visual: read-only

## Binding rules

- Use `{x:Bind}` only.
- Always set `Mode` explicitly.
- For TwoWay text entry, set `UpdateSourceTrigger=PropertyChanged` when appropriate.

## Data access rules

- ViewModels must not access connection strings.
- ViewModels must not access `Helper_Database_*`.
- Services define business operations.
- DAOs contain the stored procedure calls.

## Runtime binding audit hint

If you see `{Binding ...}` in XAML, treat it as a violation and convert to `{x:Bind ...}`.

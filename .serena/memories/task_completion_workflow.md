# Task Completion Workflow

Last Updated: 2026-03-21

When a task is completed, follow these steps before reporting done:

## 1. Build Verification

```powershell
dotnet build MTM_Receiving_Application.slnx
```

Must succeed with **zero errors and zero warnings** before proceeding.

## 2. Test Execution

```powershell
dotnet test MTM_Receiving_Application.slnx
```

- All existing tests must still pass.
- Add new tests for any new functionality.

## 3. Architecture Validation

Verify no forbidden patterns were introduced:

- No `Dao_*` references in `Module_*/ViewModels/` files
- No `static class Dao_` anywhere in the codebase
- No `{Binding ` in XAML files (only `{x:Bind` allowed)
- No `INSERT`/`UPDATE`/`DELETE` against SQL Server tables
- All new async methods end with `Async` suffix
- All new ViewModels are `partial` classes inheriting from `ViewModel_Shared_Base`

## 4. Memory-Bank Update (if applicable)

Update `memory-bank/activeContext.md` with what was completed and what is next.
Update `memory-bank/progress.md` if a milestone was reached.

## 5. Code Quality

- Remove unused `using` directives
- Ensure all new public APIs have XML doc comments
- Verify explicit accessibility modifiers on all new members
- Confirm naming conventions are followed (`ViewModel_Module_Feature`, `Dao_EntityName`, etc.)

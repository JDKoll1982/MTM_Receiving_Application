# Guardrails

Last Updated: 2026-11-11

Rules that must hold for every change in this module. The first block is the repository house rules; the
second is specific to VISUAL automation.

## Repository house rules

- Layering is View -> ViewModel -> Service -> DAO -> Database. No ViewModel may call a DAO directly.
- All business logic lives in services, never in `.xaml.cs`.
- MySQL writes go through stored procedures only. No raw MySQL SQL in C#.
- Infor Visual SQL Server is strictly read-only: never INSERT, UPDATE, or DELETE.
- XAML uses `x:Bind` only. Runtime `{Binding}` is not permitted.
- DAOs return `Model_Dao_Result` instead of throwing.
- Async methods end in `Async`.
- Tests use xUnit plus FluentAssertions, named `MethodName_ShouldResult_WhenCondition`.
- Documentation carries a `Last Updated: YYYY-MM-DD` line within the first five lines.
- No secrets, connection strings, or credentials in code or docs.

This module has no DAO and no database, so the DAO rule applies only in spirit: expected failures come
back as `Model_Dao_Result`, and nothing throws for a foreseeable condition.

## Module-specific rules

1. **Never click Save.** No code may click or invoke a Save control on the VISUAL window.
2. **Never send a save key sequence.** No `Ctrl+S`, no `F9`, no menu accelerator, no synthetic click that
   lands on a save button.
3. **Never widen the writable set casually.** `Helper_VisualAutomation_FieldMap._writable` may only hold
   variables whose binding is `Enum_VisualVariableSafety.ReceiptOnly`. `IsSafeToWrite` encodes this, and a
   test fails if the set ever includes another safety class.
4. **Never write to Infor Visual SQL Server.** Table and column names in the field map are documentation
   metadata, not a target.
5. **Never type without focus.** Keep the foreground check and the re-check; a write that cannot confirm
   focus must fail with a message, not type blind.
6. **Never leave a typed value behind.** New write probes must pass `restoreOriginal: true` and must
   report the restore result.
7. **Never add a save path "for testing".** The absence of a save path is the module's main safety claim.
8. **No runtime `{Binding}`.** Card and dialog templates are typed (`x:DataType`) and use `x:Bind`.
9. **No logic in `.xaml.cs`.** Code-behind may resolve the view model and open dialogs; nothing else.
10. **Do not touch anything outside the module** except its tests.

If a requirement appears to conflict with rules 1 to 4, stop and raise it rather than working around it.

## Before you commit

- [ ] Did the change add any code path that could click Save or send a save key sequence? It must not.
- [ ] Did it touch anything outside `Module_VisualAutomation/` and its test folder? It must not.
- [ ] Did it add a package, DAO, connection string, or SQL? It must not.
- [ ] Did XAML keep `x:Bind` with a typed template, and did `.xaml.cs` stay free of logic?
- [ ] Did the build and the VisualAutomation unit tests pass?
- [ ] Did the docs you invalidated get a new `Last Updated` date?

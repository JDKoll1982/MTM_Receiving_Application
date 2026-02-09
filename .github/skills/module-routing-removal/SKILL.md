---
name: module-routing-removal
description: Remove or replace module routing references across the repo. Use when deprecating modules, renaming routes, or cleaning navigation entries. Keywords: routing, navigation, module removal, DI, menu, docs.
---

# Module Routing Removal Skill

## Purpose

Systematically identify, remove, or replace module routing references across the MTM Receiving Application codebase. This includes:
- Navigation registrations and calls
- Navigation hubs and steps (Model_SettingsNavigationStep, etc.)
- DI registrations and service providers tied to views/viewmodels
- XAML pages and their x:Class declarations
- Menu entries, toolbars, and command bindings
- Documentation, specs, and tests that reference removed routes

## When to use

- Removing or deprecating a module (e.g., `Module_Dunnage`) ✅
- Replacing a module with another or consolidating routes ✅
- Renaming routes or view types as part of refactor ✅
- Cleaning stale navigation items in settings or menus ✅

## Preconditions / Pre-Flight Checklist

Before making changes, confirm:
1. **Target Module Name** (e.g., `Module_Reporting`)
2. **Action Type**: `remove` or `replace`
3. **Replacement Target** (if `replace`) — fully qualified view or module name
4. **Scope**: `full` (repo-wide) or `targeted` (list of files/dirs)
5. Create a branch: `git checkout -b routing/<action>-<module>`
6. Run a full build & tests baseline: `dotnet build` and `dotnet test`
7. Create a PR checklist and automated tests plan with the owner

---

## Quick References (search patterns)

Use these patterns to locate routing references. Adapt to your preferred search tool (rg, git grep, Select-String, VS Code search).

- Navigate(Type) calls:
  - Regex: `Navigate\(typeof\(Views\.View_[A-Za-z0-9_]+\)\)`
- Model navigation step definitions:
  - Regex: `new\s+Model_SettingsNavigationStep\([^,]+,\s*typeof\(Views\.View_[A-Za-z0-9_]+\)\)`
- Navigation via service provider or Resolve:
  - `Resolve` or `ServiceProvider.GetService` and `NavigateUsingServiceProvider` usages
- XAML pages:
  - `x:Class="[^"]+Module_[A-Za-z0-9_.]+\.Views\.View_[A-Za-z0-9_]+"`
- DI registrations in `App.xaml.cs` or DI modules:
  - `services.AddSingleton<.*Dao_` or `services.AddTransient<View_` or `services.AddTransient<ViewModel_`
- Docs & specs:
  - Search for `Module_<ModuleName>`, `View_` or human-friendly names in `/docs`, `README.md`, `specs/`
- Tests:
  - Search test files for `View_` and `Navigation` (e.g., `Service_NavigationTests`)

---

## Step-by-step Workflow

### 1) Discovery (Non-destructive)
1. Grep the repo with all patterns above and list matches (file, line, snippet).
2. Use `find_referencing_symbols` (or `git grep`) to find callers of view types and related services.
3. Build a summary table of findings:
   - Type: View / ViewModel / DI / XAML / Docs / Test
   - File path
   - Action required (remove / replace / manual review)
4. Present summary to stakeholders for approval before changes.

### 2) Plan the Edit
- For `remove`: Decide whether to delete files or keep them as archived (prefer archiving unless storage is a concern).
- For `replace`: Decide replacement target for each reference (e.g., `typeof(Views.View_New)`), and whether to also update UI strings.
- Identify tests to update or add; create a validation checklist with owners.

### 3) Execute Replacements (prefer small commits)
Follow these rules:
- Make minimal changes per commit (e.g., update navigation steps in one commit).
- Always update tests that reference removed types in the same commit.
- Use automated replace tools where safe, but verify each result with a focused review.

Typical code edits:
- Replace `Navigate(typeof(Views.View_Old))` with `Navigate(typeof(Views.View_New))` or remove call.
- In `Model_SettingsNavigationStep` arrays, remove or replace the `Model_SettingsNavigationStep` entry.
- Remove DI registrations for views/viewmodels if views are deleted.
- Remove or update `x:Class` / XAML files (if deleting, remove both XAML and .xaml.cs and any resources)
- Update docs: specs, README, module docs, and Quick_refs under `Module_*` docs

### 4) Tests & Build Verification
- Run `dotnet build` and `dotnet test` immediately after changes
- Add/modify tests that assert navigation behaviour (existing `Service_NavigationTests` should still pass)
- Add integration/UI smoke tests for the navigation hub(s) you modified (manual or automated)

### 5) Manual UI Verification
- Launch the app (`dotnet run` or run in VS)
- Exercise navigation points that previously targeted the removed/changed module
- Confirm:
  - No missing-type exceptions
  - Menu items or hub steps removed/replaced
  - Back navigation and breadcrumbs still work

### 6) Documentation & Changelog
- Add changelog entry in `CHANGELOG.md` with `Added/Changed/Removed` sections
- Update module docs `Module_*` docs and `specs/` where examples referenced the old route
- Add a short migration note in `docs/` if replacement is cross-cutting

### 7) PR & Review
- Create PR with the branch and include the summary table of changes and validation checklist
- Run CI (build + test + markdown lint)
- Request review from module owners and UI/QA

### 8) Rollout & Monitoring
- Merge after approvals and green CI
- Monitor logs for navigation errors (search for "Navigation failed" logs) for 24–48 hours

### 9) Rollback Plan
- If critical errors appear, revert the PR and re-open a follow-up issue
- Keep the branch available for patching (don't delete until stable)

---

## Automation Snippets

PowerShell (discovery):

```powershell
# Find C# references to a view name (case-insensitive)
$pattern = 'Views\.View_OldModule'  # replace with actual view or module pattern
Get-ChildItem -Path . -Recurse -Include *.cs,*.xaml,*.md,*.json | Select-String -Pattern $pattern | Format-Table Path,LineNumber,Line -AutoSize
```

Batch replace (careful — test in a branch):

```powershell
# Replace 'typeof(Views.View_Old)' with 'typeof(Views.View_New)' in .cs files
Get-ChildItem -Path . -Recurse -Include *.cs | ForEach-Object {
  (Get-Content $_.FullName) -replace 'typeof\(Views\.View_Old\)', 'typeof(Views.View_New)' | Set-Content $_.FullName
}
```

Prefer using an editor/IDE with rename-refactor for typed symbols (safer than regex replace).

---

## Risk & Safety Notes

- Do not remove a view type that is referenced by other modules without confirming callers.
- Ensure DI removal does not remove shared services used elsewhere.
- Avoid blind text-only replacements for symbols with identical names but different namespaces.
- Always run tests and do manual UI checks after navigation edits.

---

## Examples

1) Replace a navigation step in a settings navigation hub

Before:
```csharp
new Model_SettingsNavigationStep("Overview", typeof(Views.View_OldModuleOverview)),
```
After:
```csharp
// Removed Overview step for old module
// or
new Model_SettingsNavigationStep("Overview", typeof(Views.View_NewOverview)),
```

2) Remove a DI registration in `App.xaml.cs`

Before:
```csharp
services.AddTransient<View_OldModule>();
```
After:
```csharp
// Removed View_OldModule registration because module deprecated
```

---

## Validation Checklist (must complete before merge)
- [ ] Branch created and baseline build/tests pass
- [ ] All references located and categorized
- [ ] Code edits are minimal, per-file commits
- [ ] Unit tests updated/passing
- [ ] Integration/UI smoke tests added or validated
- [ ] Docs & changelog updated
- [ ] PR description includes the summary table
- [ ] Post-merge monitoring plan documented

---

## Useful files to check
- `App.xaml.cs` (DI registrations)
- `Module_*/Views/` and `Module_*/ViewModels/` (view & navigation hubs)
- `Module_* / docs / copilot / QUICK_REF.md`
- `specs/` and `docs/` for references
- `MTM_Receiving_Application.Tests/` for navigation-related tests

---

## When to escalate / ask the team
- If the target view is used by multiple modules and a safe replacement is unclear
- If database or stored-procedure changes are required by the new route
- If UI/UX changes are required across multiple screens

---

## References
- Repository search: use `git grep` / `rg` / VS Code search
- `Service_NavigationTests` to validate navigation service behavior

---

**Notes:** Keep changes small and reversible. Prefer replacing with service-based navigation (resolve view by Type) rather than hard-coded strings. Document every removal with PR notes and a migration step in `docs/`.

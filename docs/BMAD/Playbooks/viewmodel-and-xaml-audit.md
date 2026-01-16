# Playbook: ViewModel and XAML Audit

Goal: Enforce MVVM purity and x:Bind usage across Views/ViewModels.

## Prep

- Docent `QA` to count Views/ViewModels; `AM` to list bindings/commands.
- Inspect repomix snapshot for View_*and ViewModel_* pairs, especially in Module_Receiving and Module_Dunnage.

## Checks

- ViewModels are partial, inherit ViewModel_Shared_Base/ObservableObject, use [ObservableProperty]/[RelayCommand], and have no DAO calls.
- Views use `x:Bind` (no `{Binding}`) with correct Mode and UpdateSourceTrigger where needed.
- Code-behind (.xaml.cs) contains only UI glue (no business logic or DB/service access).
- Commands guard IsBusy and call mediator/services only through injected dependencies.

## Steps

1) **Binding scan**: Replace `{Binding}` with `x:Bind`; ensure OneWay/TwoWay modes correct.
2) **Code-behind review**: Move any business logic to ViewModel; keep event handlers minimal.
3) **ViewModel validation**: Add missing `[ObservableProperty]`/`[RelayCommand]`; ensure constructor DI only; remove service locator/static access.
4) **Mediator adoption**: Where services are called directly, route through mediator per `Playbooks/cqrs-migration.md`.

## Agents & Tools

- dev-tech-writer for documenting binding changes; dev-developer for refactors.
- Docent `AM` to confirm binding summaries after fixes.

## Done Criteria

- No business logic in code-behind; all bindings use x:Bind.
- ViewModels comply with MVVM rules and depend only on injected services/mediator.

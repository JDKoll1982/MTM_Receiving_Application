# Violation Catalog (BMAD + Serena Targets)

Use this list to seed Docent/Serena runs and create BMAD tasks. Focus on the referenced folders, then validate with Docent `QA`/`AM` outputs and the repomix snapshot.

## 1) Module_Core service bloat

- Evidence: Module_Core/Services contains Dunnage, Receiving, Volvo, and Reporting services that belong in their modules.
- Risk: Violates modular boundaries and makes Module_Core non-reusable.
- Action: See `Playbooks/module-core-decoupling.md`.

## 2) CQRS/Mediator missing in ViewModels

- Evidence: ViewModels in Module_Receiving and Module_Dunnage call multi-method services instead of MediatR handlers (per repomix inventory).
- Risk: Hard to test, cross-cutting concerns not enforced.
- Action: See `Playbooks/cqrs-migration.md`.

## 3) DAO compliance check

- Evidence: Multiple Dao_*.cs classes exist; must confirm all use stored procedures and return Model_Dao_Result without throws.
- Risk: Raw SQL or exceptions leaking breaks constitution and DB rules.
- Action: See `Playbooks/dao-and-db-checks.md`.

## 4) MVVM/x:Bind purity

- Evidence: Views have code-behind files; need to ensure no business logic and bindings use x:Bind, not Binding.
- Risk: Runtime binding errors, UI/business coupling.
- Action: See `Playbooks/viewmodel-and-xaml-audit.md`.

## 5) Library-first enforcement

- Evidence: Mapping/resilience/guards are manual in several services; add AutoMapper/Mapster, Polly, Ardalis.GuardClauses where justified.
- Risk: Custom service sprawl and inconsistent error handling.
- Action: Integrate per relevant playbook (CQRS migration, module decoupling).

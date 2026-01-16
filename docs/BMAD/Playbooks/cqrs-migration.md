# Playbook: CQRS/MediatR Migration

Goal: Convert service-heavy ViewModels to mediator-first handlers with pipeline behaviors.

## Prep

- Run Docent `QA` then `AM` on target module to list ViewModels and services.
- Mark offending ViewModels from [repomix-output-code-only.xml](repomix-output-code-only.xml) (e.g., ViewModel_Receiving_*classes using Service_Receiving*).

## Steps

1) **Identify operations**: For each service method used by a ViewModel, define a Command or Query record.
2) **Create handlers**: One handler per operation; inject DAO + logger + error handler; return Model_Dao_Result<T> or similar.
3) **Wire pipeline behaviors**: Ensure ValidationBehavior, LoggingBehavior, AuditBehavior are registered.
4) **Add validators**: FluentValidation validators per command/query; fail fast with friendly messages.
5) **Refactor ViewModels**: Inject `IMediator`; replace direct service calls with `Send(new XxxCommand/Query(...))`; keep `[RelayCommand]` wrappers and IsBusy guards.
6) **Tests**: Handler unit tests (happy path + failure path); validator tests; ViewModel tests with mediator mock.

## Agents & Tools

- Docent `AM` for inventories.
- dev-developer for TDD flow; dev-quick-flow for rapid refactor.
- plan-architect for mapping service methods → commands/queries.

## Done Criteria

- ViewModel depends only on mediator plus infra services.
- Each command/query has validator, handler, and tests.
- Logging/validation/audit behaviors triggered on every call.

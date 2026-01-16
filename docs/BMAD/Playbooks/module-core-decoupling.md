# Playbook: Module_Core Decoupling

Goal: Evict module-specific services from Module_Core and re-home them inside their respective modules.

## Prep

- Docent `QA`/`AM` on Module_Core to list services and their consumers.
- From repomix snapshot, note offenders: Service_Dunnage*, Service_Receiving*, Service_Volvo*, Service_Reporting*, Service_Navigation with module-specific flows.

## Steps

1) **Classify services**: Infra-only stays (ErrorHandler, LoggingUtility, Dispatcher, Window/Navigation shell); module-specific moves.
2) **Create module services**: In Module_{Name}/Services, create interfaces + implementations mirroring existing behavior but scoped to the module.
3) **Adjust DI**: Register moved services in module DI (App.xaml.cs) under module namespace; remove module-specific registrations from Module_Core.
4) **Update consumers**: ViewModels/handlers update constructor injections to the new module service interfaces.
5) **Delete/retire Core copies**: After consumers are migrated and tests pass, remove old Module_Core implementations.

## Agents & Tools

- plan-architect for dependency map; Docent `AM` for inventories.
- dev-developer for refactors; build-agent-creator if scaffolding new module agents.

## Done Criteria

- Module_Core contains only generic infra services.
- Each module compiles independently with its own services and DAOs.
- DI graph reflects new ownership; no module-specific types registered from Module_Core.

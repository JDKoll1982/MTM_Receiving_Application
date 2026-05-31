---
description: 'Module dependency guidance for MTM covering shared code placement, module boundaries, and cross-module communication constraints.'
applyTo: 'Module_*/**/*.{cs,xaml},Infrastructure/DependencyInjection/**/*.cs'
---

# Module Coupling Constraints

## Core Rules

- Keep `Module_Core` foundational.
- Use `Module_Shared` only for genuinely cross-module presentation or shared UI assets.
- Keep module-specific business logic inside the owning module.
- Avoid circular dependencies and hidden cross-module reach-through.

## Preferred Pattern

- share contracts or abstractions intentionally
- route cross-module behavior through explicit service or facade boundaries
- register module services in centralized infrastructure wiring

## Avoid

- copying logic between modules when a shared abstraction should exist
- moving module-specific rules into `Module_Shared`
- direct reach-through from one module into another module's low-level internals
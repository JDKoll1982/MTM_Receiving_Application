---
description: 'CQRS pipeline behavior guidance for MTM covering MediatR behaviors, registration order, and cross-cutting concerns.'
applyTo: 'Module_Core/Behaviors/**/*.cs,Infrastructure/DependencyInjection/**/*Cqrs*.cs,Module_*/Services/**/*.cs'
---

# CQRS Behaviors And Pipeline

Use this file when changing MediatR pipeline behaviors or handler wiring.

## Current Pattern

- Logging, validation, and audit concerns run as pipeline behaviors.
- Behavior registration order matters because validation should fail early and logging should keep
  enough context to explain failures.
- Cross-cutting concerns belong in behaviors when they apply broadly across handlers.

## Rules

- Keep business decisions in handlers or services, not in generic behaviors.
- Keep behaviors focused on one cross-cutting responsibility.
- Update registration code when a new behavior is introduced or reordered.
- Validate how a new behavior interacts with logging, validation, and audit flow.
---
description: 'Error handling guidance for MTM viewmodels, services, and DAOs, including escalation boundaries and user-facing error flow.'
applyTo: 'Module_*/ViewModels/**/*.cs,Module_*/Services/**/*.cs,Module_*/Data/**/*.cs'
---

# Error Handling

## Responsibility Split

- DAOs return failure results for expected operational problems.
- Services translate DAO failures into application behavior and logging.
- ViewModels decide what the user should see through the error-handler service.

## Rules

- Do not swallow exceptions silently.
- Do not throw from DAOs for expected query or stored-procedure failures.
- Use `IService_ErrorHandler` for user-facing error display.
- Keep exception severity and context explicit when escalating unexpected failures.

## Preferred Pattern

- Validate early.
- Return structured failure data from lower layers.
- Catch unexpected exceptions at the layer that can add useful context.
- Keep log messages actionable and scoped to the operation that failed.
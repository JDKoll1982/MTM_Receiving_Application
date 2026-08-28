---
description: 'Testing strategy for MTM Receiving Application covering unit-vs-integration decisions, naming, fixtures, and validation expectations.'
applyTo: 'MTM_Receiving_Application.Tests/**/*.cs,**/*Tests.cs'
---

<!-- 
[DOC-META-START]
- File Name: testing-strategy.instructions.md
- Description: Testing strategy for MTM Receiving Application covering unit-vs-integration decisions, naming, fixtures, and validation expectations.
- Last Updated: 2026-08-28
- Quick TOC:
  - Line 6-9: # MTM Testing Strategy
  - Line 10-19: ## Core Rules
  - Line 20-28: ## Decision Guide
  - Line 29-35: ## Repository Patterns
  - Line 36-39: ## Validation
- Critical Notes: Use xUnit with FluentAssertions; integration tests use IAsyncLifetime and prefix disposable data with TEST-.
[DOC-META-END]
-->

# MTM Testing Strategy

Use this file for test-writing work in the repository.

## Core Rules

- Prefer xUnit with FluentAssertions.
- Use unit tests when dependencies can be mocked cleanly.
- Use integration tests for DAOs, concrete database interactions, and behavior that depends on real
  infrastructure wiring.
- Keep tests narrow and behavior-based. Do not write one oversized test per class.
- Use method names in the form `MethodName_ShouldResult_WhenCondition`.
- Do not add Arrange/Act/Assert comments unless a test is unusually hard to follow.

## Decision Guide

- Validator only: unit test
- ViewModel with mockable service or mediator boundary: unit test
- Handler with interface-only dependencies: unit test
- DAO or stored-procedure integration: integration test
- UI workflow requiring real XAML or windowing: prefer focused UI or integration coverage only when
  needed

## Repository Patterns

- Use `IAsyncLifetime` for integration setup and cleanup.
- Prefix disposable database test data with `TEST-`.
- Mock dependencies at service boundaries instead of mocking domain logic.
- Validate failure paths, not only success paths.

## Validation

- Run the narrowest matching test filter first.
- Only widen to module or suite-level execution after the focused slice passes.
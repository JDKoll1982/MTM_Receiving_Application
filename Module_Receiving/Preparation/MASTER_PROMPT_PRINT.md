<style>
@media print {
  .page-break { page-break-after: always; }
  .keep-together { page-break-inside: avoid; }
  .avoid-break-before { page-break-before: avoid; }
  h1 { page-break-before: always; }
  h1:first-of-type { page-break-before: avoid; }
  h2 { page-break-after: avoid; }
  pre, .mermaid { page-break-inside: avoid; margin: 1em 0; }
  
  /* Scale Mermaid diagrams to fit 8.5x11 page width */
  .mermaid, pre.mermaid {
    max-width: 100%;
    transform: scale(0.75);
    transform-origin: top left;
    margin-bottom: 2em;
  }
  
  /* Keep diagram containers properly sized */
  .keep-together {
    max-width: 100%;
    overflow: visible;
  }
}
</style>

# Module_Receiving Ground-Up Rebuild Implementation Guide

**Version:** 2.0.0 | **Date:** January 15, 2026 | **Estimated Effort:** 6-8 weeks

---

## Executive Summary

This document outlines a complete architectural redesign of Module_Receiving using modern patterns and industry-standard libraries. The goal is to create a highly modular, testable, and maintainable codebase that reduces service bloat in Module_Core while maintaining strict architectural compliance.

**Key Objectives:**

- Reduce Module_Core service count by approximately 50%
- Implement industry-standard CQRS (Command Query Responsibility Segregation) pattern
- Improve testability through proper abstraction
- Enable true modular independence
- Establish blueprint for modernizing other modules

---

## Current State Analysis

### Problems to Solve

1. **Service Bloat** - Too many Receiving-specific services residing in shared Module_Core infrastructure
2. **Tight Coupling** - ViewModels, Services, and Data Access Objects are not properly abstracted
3. **Limited Modularity** - Module_Receiving depends heavily on Module_Core
4. **Testing Challenges** - Difficult to test due to tight coupling and lack of proper interfaces
5. **Maintenance Burden** - Changes to Receiving logic create ripple effects across Module_Core

### What Works Well

- Strict Model-View-ViewModel (MVVM) architecture with partial ViewModels
- Instance-based Data Access Objects returning structured result objects
- Dependency Injection configured centrally
- WinUI 3 with compile-time data binding
- Stored procedures for all MySQL database operations

### What Needs Improvement

- Service layer is monolithic (single service with 10+ methods)
- Navigation is custom-built and tightly coupled to specific implementation
- Validation logic is scattered across ViewModels, Services, and custom validators
- Logging lacks structured context and semantic properties
- CSV export functionality uses custom writer instead of proven libraries

<div class="page-break"></div>

## Target Architecture

### Modern Architecture Stack

**Selected Patterns:**

1. **CQRS (Command Query Responsibility Segregation)** - Separates read operations (Queries) from write operations (Commands), improving clarity and testability
2. **Structured Logging** - Adds semantic context to log entries for better diagnostics
3. **Declarative Validation** - Strongly-typed validation rules that are composable and testable
4. **Type-Safe CSV Export** - Mature library for CSV operations
5. **Resilience Patterns** (Optional) - Retry policies and circuit breakers for database operations

**Library Justification:**

- **MediatR (607M+ downloads)** - Industry standard for implementing mediator pattern and CQRS
- **Serilog (2.3B+ downloads)** - Most popular structured logging framework for .NET
- **FluentValidation (741M+ downloads)** - De facto standard for validation in .NET
- **CsvHelper (34M+ downloads)** - Mature, feature-rich CSV library

### Architectural Layers Overview

**Post-Rebuild Structure:**

```mermaid
graph TD
    Views[Views - XAML UI Only]
    ViewModels[ViewModels - Presentation Logic]
    Handlers[Handlers - CQRS Implementation]
    Queries[Queries - Read Operations]
    Commands[Commands - Write Operations]
    Behaviors[Behaviors - Cross-Cutting Concerns]
    Validators[Validators - FluentValidation Rules]
    Services[Services - Navigation & Orchestration]
    Data[Data - Instance-Based DAOs]
    Models[Models - Data Transfer Objects]
    Defaults[Defaults - Configuration & Presets]
    
    Views --> ViewModels
    ViewModels --> Handlers
    Handlers --> Queries
    Handlers --> Commands
    Handlers --> Behaviors
    Commands --> Validators
    Handlers --> Data
    Data --> Models
    Services --> ViewModels
    Defaults --> Models
```

### Data Flow Transformation

<div class="keep-together">

**Current (Service Pattern):**

```mermaid
flowchart TD
    ViewModel[ViewModel] --> Service[Service<br/>10+ methods]
    Service --> DAO[DAO]
    DAO --> DB[(Database)]
```

</div>

<div class="keep-together">

**New (CQRS Pattern):**

```mermaid
flowchart TD
    ViewModel[ViewModel] --> Mediator[Mediator]
    Mediator --> Handler[Handler<br/>Single Responsibility]
    Handler --> Validator[Validator]
    Handler --> Logger[Logger]
    Handler --> DAO[DAO]
    DAO --> DB[(Database)]
```

</div>
</div>

<div class="page-break"></div>
<div class="avoid-break-before">

**Benefits:**

- Each handler is a single class with one responsibility
- Easy to add cross-cutting concerns (logging, validation) via pipeline behaviors
- Handler classes are highly testable with mocked dependencies
- Reduces large service files into many small, focused handler classes

---

## Constitutional Constraints

These architectural principles are non-negotiable and must be maintained throughout the rebuild:

### I. MVVM Architecture

- ViewModels SHALL NOT directly call Data Access Objects
- ViewModels SHALL NOT access database helpers or connection strings
- All data access MUST flow through Service or Mediator layer
- All ViewModels MUST be partial classes using source generators
- All data binding MUST use compile-time binding

### II. Database Layer

- All MySQL operations MUST use stored procedures (no raw SQL in application code)
- All Data Access Objects MUST return structured result objects
- Data Access Objects MUST be instance-based and registered in Dependency Injection
- SQL Server (Infor Visual) is READ ONLY - no write operations permitted

### III. Dependency Injection

- All services MUST be registered in central configuration
- Constructor injection REQUIRED for all dependencies
- Service locator pattern is FORBIDDEN

### IV. Error Handling

- Use centralized error handler for user-facing errors
- Use structured logging for all diagnostic information
- Data Access Objects MUST NOT throw exceptions (return failure results instead)

### V. Code Quality

- Explicit accessibility modifiers required on all members
- Braces required for all control flow statements
- Async methods MUST end with "Async" suffix
- XML documentation comments required for all public APIs

### VI. Documentation

- All diagrams MUST use PlantUML (no ASCII art)
- Architecture documents MUST be updated when behavior changes
<div class="page-break"></div>ask tracking required with status updates

---

## Implementation Strategy

### Phase 1: Foundation & Setup (Week 1)

**Objective:** Install packages, create folder structure, configure dependency injection

**Key Tasks:**

1. Install NuGet packages (MediatR, Serilog, FluentValidation, CsvHelper)
2. Create new folder structure for Handlers, Validators, Defaults
3. Configure Serilog for file-based structured logging
4. Register MediatR with pipeline behaviors
5. Register FluentValidation auto-discovery

**Deliverables:**

- All packages installed and configured
- Folder structure established
- Dependency injection configured
<div class="page-break"></div>ogging outputs to daily rolling log files

---

### Phase 2: Models & Validation (Week 1-2)

**Objective:** Review existing models and create declarative validators

**Key Tasks:**

1. Review all existing model classes
2. Create FluentValidation validators for each model
3. Define validation rules with custom error messages
4. Create default configuration models for presets
5. Write unit tests for validators

**Validation Approach:**

Instead of scattered validation logic in ViewModels and Services, validation rules are defined in dedicated validator classes that are:

- Strongly-typed (compile-time checked)
- Composable (rules can be shared and combined)
- Testable (easy to verify validation behavior)
- Centralized (single source of truth for validation rules)

**Deliverables:**

- All models documented
- Validators created for each model
- Unit tests for validation rules
- Default configuration values defined

<div class="page-break"></div>

### Phase 3: CQRS Handlers (Week 2-3)

**Objective:** Replace Service methods with MediatR handlers

**Migration Pattern:**

**Before:** Single service class with multiple methods (InsertLine, UpdateLine, GetLines, DeleteLine, etc.)

**After:** Separate handler classes:

- GetReceivingLinesQuery + GetReceivingLinesHandler (read operation)
- InsertReceivingLineCommand + InsertReceivingLineHandler (write operation)
- UpdateReceivingLineCommand + UpdateReceivingLineHandler (write operation)
- DeleteReceivingLineCommand + DeleteReceivingLineHandler (write operation)

**Key Concepts:**

**Queries (Read Operations):**

- Retrieve data without modifying state
- Can be cached or optimized differently than commands
- Return data transfer objects or result wrappers

**Commands (Write Operations):**

- Modify application state
- Include validation before execution
- Return success/failure result objects

**Pipeline Behaviors (Cross-Cutting Concerns):**

- Logging Behavior - Automatically logs all handler executions
- Validation Behavior - Automatically validates commands before execution
- Transaction Behavior (Optional) - Wraps commands in database transactions

**Deliverables:**

- All service methods migrated to Query/Command handlers
- ViewModels updated to use Mediator instead of direct service calls
- Pipeline behaviors implemented and tested
- Unit tests for all handlers

<div class="page-break"></div>

### Phase 4: ViewModels & Navigation (Week 3-4)

**Objective:** Refactor ViewModels to use Mediator pattern

**ViewModel Changes:**

**Before:** ViewModels injected multiple specific services

**After:** ViewModels inject single Mediator interface

**Benefits:**

- Reduced coupling to specific service implementations
- Easier to mock for testing (mock Mediator instead of multiple services)
- Clearer separation of concerns
- Easier to add new operations without modifying ViewModel

**Navigation Strategy:**

Two approaches considered:

1. **Keep Custom Service (Simplest)** - Retain existing navigation service, remove data access logic
2. **Use Navigation Library** - Adopt third-party ViewModel-based navigation

**Recommendation:** Start with custom navigation for MVP, evaluate migration to library later.

**Deliverables:**

- All ViewModels refactored to use Mediator
- Logging updated to use structured logging framework
- Navigation strategy implemented and working
- All ViewModels properly registered in dependency injection

<div class="page-break"></div>

### Phase 5: Services Cleanup (Week 4)

**Objective:** Remove or relocate Receiving-specific services from Module_Core

**Services to Remove/Replace:**

1. MySQL Receiving Line Service → Replaced by MediatR handlers
2. Receiving Validation Service → Replaced by FluentValidation validators
3. Custom CSV Writer → Replaced by CsvHelper-based generic export service
4. Custom Logging Utility → Replaced by Serilog structured logging

**Services to Keep (Shared across all modules):**

- Error Handler Service
- Window Management Service
- UI Thread Dispatcher Service

**Deliverables:**

- Receiving-specific services removed from Module_Core
- Generic CSV export service created
- All dependency injection registrations updated
- Zero compilation errors

<div class="page-break"></div>

### Phase 6: Testing & Documentation (Week 5)

**Objective:** Achieve 80% test coverage and update all documentation

**Testing Strategy:**

**Unit Tests:**

- ViewModels with mocked Mediator
- Handlers with mocked Data Access Objects
- Validators with test data

**Integration Tests:**

- Data Access Objects against test database
- End-to-end workflow scenarios

**Documentation Updates:**

- Module README with new architecture overview
- Architecture document with design decisions
- Changelog with all changes documented
- Updated Copilot instructions with new patterns

**Deliverables:**

- 80% unit test coverage achieved
- Integration tests for all Data Access Objects
- All documentation updated
- Code review completed and approved

<div class="page-break"></div>

## Success Metrics

### Quantitative Goals

- Reduce Module_Core service count by 50% (from ~15 to 7-8 services)
- Achieve 80%+ test coverage for Module_Receiving
- Reduce average service file size from 500 lines to under 100 lines (handlers)
- Maintain or improve application performance
- Zero architectural constraint violations

### Qualitative Goals

- **Modularity:** Module_Receiving is 100% self-contained
- **Testability:** Easy to mock Mediator interface for ViewModel testing
- **Maintainability:** One handler class equals one responsibility
- **Scalability:** Easy to add new operations without modifying existing code
- **Developer Experience:** Clear patterns for implementing new features

<div class="page-break"></div>

## Common Pitfalls to Avoid

### Anti-Pattern 1: Direct DAO Injection

**Incorrect:** ViewModels directly inject Data Access Objects

**Correct:** ViewModels inject Mediator interface

### Anti-Pattern 2: God Handlers

**Incorrect:** Single handler performing multiple operations (CreateAndUpdateUser)

**Correct:** Separate handlers for each operation (CreateUser, UpdateUser)

### Anti-Pattern 3: Skipping Validation

**Incorrect:** Handlers that don't validate input before processing

**Correct:** All commands validated before execution via pipeline behavior

### Anti-Pattern 4: String Interpolation in Logging

**Incorrect:** Log messages with string concatenation or interpolation

**Correct:** Structured logging with semantic properties

<div class="page-break"></div>

## Risk Mitigation

### Performance Risk

**Risk:** MediatR adds overhead to every operation

**Mitigation:** Establish performance baseline before rebuild, measure after each phase, optimize hotspots

### Migration Risk

**Risk:** Breaking existing functionality during transition

**Mitigation:** Feature flags to enable gradual rollout, maintain old code paths until new paths proven

### Testing Risk

**Risk:** Insufficient test coverage leads to production defects

**Mitigation:** 80% coverage requirement, automated test suite, integration testing against real database

### Knowledge Transfer Risk

**Risk:** Team unfamiliar with new patterns

**Mitigation:** Comprehensive documentation, code examples, pair programming during implementation

<div class="page-break"></div>

## Pre-Implementation Checklist

**Before Starting Phase 1:**

- [ ] All critical questions in Clarification Questions document answered
- [ ] Team approval on library selections
- [ ] NuGet package approval process completed
- [ ] Test database environment available
- [ ] Development environment setup verified
- [ ] Constitutional compliance review completed

**During Implementation:**

- [ ] Follow phase order strictly (1 through 6)
- [ ] Update task tracking after each task completion
- [ ] Run automated tests after each phase
- [ ] Document architectural decisions
- [ ] Conduct code review after each phase

**Post-Implementation:**

- [ ] All tests passing with 80%+ coverage
- [ ] No architectural violations detected
- [ ] Documentation complete and accurate
- [ ] Performance benchmarks meet targets
- [ ] Code review approved by team
- [ ] Deployment plan reviewed and approved

<div class="page-break"></div>

## References

### Official Documentation

- MediatR: github.com/jbogard/MediatR/wiki
- Serilog: serilog.net
- FluentValidation: docs.fluentvalidation.net
- CsvHelper: joshclose.github.io/CsvHelper
- WinUI 3: learn.microsoft.com/windows/apps/winui

### Architecture Patterns

- Clean Architecture: blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- CQRS Pattern: martinfowler.com/bliki/CQRS.html
- Modular Monolith: github.com/kgrzybek/modular-monolith-with-ddd

### Project-Specific Documents

- Constitution: .specify/memory/constitution.md
- Copilot Instructions: .github/copilot-instructions.md
- MVVM Guide: .github/instructions/mvvm-pattern.instructions.md
- DAO Guide: .github/instructions/dao-pattern.instructions.md

<div class="page-break"></div>

## Appendix: Key Concepts

### CQRS (Command Query Responsibility Segregation)

Architectural pattern that separates read operations (queries) from write operations (commands). Queries retrieve data without side effects. Commands modify state and may trigger validation or business rules.

### Mediator Pattern

Behavioral design pattern that reduces coupling between components by having them communicate through a mediator object instead of directly with each other. In this context, ViewModels send requests to Mediator, which routes them to appropriate handlers.

### Pipeline Behavior

Middleware-like functionality that wraps handler execution, allowing cross-cutting concerns (logging, validation, transactions) to be applied consistently without duplicating code in each handler.

### Structured Logging

Logging approach that treats log events as data structures with semantic properties rather than plain text strings. Enables better filtering, searching, and analysis of log data.

### Declarative Validation

Validation rules defined as data/configuration rather than imperative code. Rules are composable, reusable, and can be tested independently of business logic.

---

**End of Implementation Guide**


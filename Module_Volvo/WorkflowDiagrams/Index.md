# Workflow Diagrams - Index

This directory contains end-to-end Mermaid sequence diagrams for validating the Module_Volvo CQRS modernization implementation.

## 📋 Diagram Files

| File | Workflow | User Story | Status |
|------|----------|------------|--------|
| [01-complete-shipment-entry.md](01-complete-shipment-entry.md) | Complete Shipment Entry (New) | US1 | ✅ Phase 3 |
| [02-pending-shipment-resume.md](02-pending-shipment-resume.md) | Save Pending & Resume Later | US1 | ✅ Phase 3 |
| [03-part-search-add.md](03-part-search-add.md) | Part Search & Add (Real-time) | US1 | ✅ Phase 3 |
| [04-label-generation-csv.md](04-label-generation-csv.md) | Label Generation (CSV Export) | US1 | ✅ Phase 3 |
| [05-cqrs-pipeline-behaviors.md](05-cqrs-pipeline-behaviors.md) | MediatR Pipeline Architecture | Foundation | ✅ Phase 2 |

## 🎯 Purpose

These diagrams help you:

1. **Validate Correctness**: Visual verification of CQRS flow (View → ViewModel → IMediator → Handler → DAO → DB)
2. **Understand Dependencies**: See how queries and commands interact with database and services
3. **Verify Constitutional Compliance**: Ensure no ViewModel→DAO violations
4. **Review Validation Rules**: See where FluentValidation intercepts in pipeline
5. **Plan Testing**: Identify integration test scenarios from end-to-end flows

## 🔍 How to Use These Diagrams

### For Validation

- **Check each sequence**: Does the flow match your implementation?
- **Verify method signatures**: Do DAO methods match what's shown?
- **Confirm validation rules**: Are validators implemented as shown?
- **Review error paths**: Are exceptions handled correctly?

### For Implementation

- **Use as blueprint**: Implement handlers following the sequence shown
- **Reference for testing**: Create integration tests matching the full flow
- **Debug issues**: Compare actual flow vs diagram to find discrepancies

### For Documentation

- **Onboarding**: New developers can understand workflows visually
- **Architecture reviews**: Show stakeholders the CQRS pattern in action
- **Compliance audits**: Demonstrate constitutional adherence

## 🏗️ CQRS Pattern Summary

All workflows follow this pattern:

```
User → View (XAML) → ViewModel → IMediator → [Pipeline Behaviors] → Handler → DAO → Database
                                      ↓
                                ValidationBehavior (FluentValidation)
                                LoggingBehavior (Serilog)
                                PerformanceBehavior (Monitoring)
                                AuditBehavior (Audit Trail)
```

**Constitutional Compliance**:

- ✅ **Principle I (MVVM)**: Views use x:Bind, zero code-behind logic
- ✅ **Principle II (Data Access)**: Instance DAOs, Model_Dao_Result pattern
- ✅ **Principle III (CQRS)**: All operations through IMediator, no ViewModel→DAO calls
- ✅ **Principle IV (DI)**: Constructor injection throughout
- ✅ **Principle V (Validation)**: FluentValidation via pipeline behavior

## 📊 Workflow Coverage

### Phase 3 (User Story 1) - ✅ Documented

- [x] Complete shipment entry (new shipment)
- [x] Save pending & resume workflow
- [x] Part search with autocomplete
- [x] Label CSV generation
- [x] MediatR pipeline behaviors

### Phase 4 (User Story 2) - ⏳ Pending Implementation

- [ ] View shipment history with filtering
- [ ] Edit existing shipment
- [ ] Export shipments to CSV

### Phase 5 (User Story 3) - ⏳ Pending Implementation

- [ ] Add/edit/deactivate parts
- [ ] Manage part components
- [ ] Import/export part master data CSV

### Phase 6 (User Story 4) - ⏳ Pending Implementation

- [ ] Email notification preview
- [ ] Email format generation (HTML + text)

## 🔗 Related Documentation

- **Feature Specification**: `../spec.md`
- **Implementation Plan**: `../plan.md`
- **Task Breakdown**: `../tasks.md`
- **Data Model**: `../data-model.md`
- **API Contracts**: `../contracts/README.md`
- **Quickstart Guide**: `../quickstart.md`

## 🛠️ Viewing Mermaid Diagrams

### In VS Code

- Install: **Mermaid Preview** extension
- Right-click file → "Open Preview to the Side"

### In GitHub

- Push to repository
- View files directly (GitHub renders Mermaid automatically)

### In Markdown Editors

- **Obsidian**: Native Mermaid support
- **Typora**: Enable Mermaid in settings
- **Markdown Preview Enhanced**: VS Code extension

### Online

- Copy diagram code to [Mermaid Live Editor](https://mermaid.live)

## ✅ Validation Checklist

Use this checklist when reviewing each diagram:

**Data Flow**:

- [ ] User interaction triggers ViewModel command
- [ ] ViewModel calls `IMediator.Send(request)`
- [ ] MediatR pipeline behaviors intercept (validation, logging, etc.)
- [ ] Handler receives request after validation passes
- [ ] Handler calls DAO methods (never bypasses DAO layer)
- [ ] DAO calls stored procedures (MySQL only)
- [ ] Results flow back through pipeline to ViewModel
- [ ] ViewModel updates UI via observable properties

**Error Handling**:

- [ ] Validation failures throw `ValidationException` (caught by ViewModel)
- [ ] DAO failures return `Model_Dao_Result.Failure` (not exceptions)
- [ ] Handler catches unexpected exceptions, returns failure result
- [ ] ViewModel checks `result.IsSuccess` before updating UI
- [ ] User sees meaningful error messages (not stack traces)

**Constitutional Compliance**:

- [ ] No direct ViewModel→DAO calls
- [ ] No direct ViewModel→Database calls
- [ ] All queries/commands go through IMediator
- [ ] FluentValidation enforces business rules
- [ ] Logging/audit trails automatic via pipeline

---

**Last Updated**: January 16, 2026  
**Diagrams Created For**: Phase 3 (User Story 1) Implementation & Validation  
**Next Update**: After Phase 4 implementation begins

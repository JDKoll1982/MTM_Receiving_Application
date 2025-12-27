# Implementation Plan: Dunnage Stored Procedures

**Branch**: `005-dunnage-stored-procedures` | **Date**: 2025-12-26 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/CompletedSpecs/005-dunnage-stored-procedures/spec.md`

## Summary

Create 33 stored procedures for CRUD operations on the dunnage database tables (dunnage_types, dunnage_specs, dunnage_parts, dunnage_loads, inventoried_dunnage). This data access layer provides a consistent, secure, and maintainable interface between the WinUI 3 application and MySQL database, enforcing business rules at the database level.

**Architecture Principle**: ALL database operations MUST go through stored procedures - no direct SQL in C# code.

## Technical Context

**Language/Version**: SQL for MySQL 5.7.24, C# 12 / .NET 8.0 for DAO wrappers  
**Primary Dependencies**: MySQL.Data (8.0.x), Helper_Database_StoredProcedure utility class  
**Storage**: MySQL 5.7.24 (mtm_receiving_application database)  
**Testing**: Direct SQL testing (MySQL Workbench), xUnit for DAO integration tests  
**Target Platform**: Windows 10/11 (WinUI 3 application backend)  
**Project Type**: Database layer component (stored procedures + DAO static classes)  
**Performance Goals**: <500ms per procedure with 10K records, <200ms for search operations  
**Constraints**: MySQL 5.7.24 limitations (no CTEs, window functions, limited JSON), referential integrity enforced  
**Scale/Scope**: 5 tables, 33 stored procedures, 5 DAO static classes, comprehensive error handling

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify alignment with [MTM Receiving Application Constitution](../../.specify/memory/constitution.md) v1.1.0.

### Core Principles Alignment

- [x] **I. MVVM Architecture**: N/A - Database layer only, DAOs will be called from service layer
- [x] **II. Database Layer**: ALL operations use stored procedures, return Model_Dao_Result, async operations
- [x] **III. Dependency Injection**: DAOs are static classes (pattern), services using them will be in DI
- [x] **IV. Error Handling & Logging**: Procedures return error states, DAOs use Model_Dao_Result pattern
- [x] **V. Security & Authentication**: User tracking in all modification procedures (CreatedBy, ModifiedBy)
- [x] **VI. WinUI 3 Modern Practices**: N/A - Database layer, async-ready for UI consumption
- [x] **VII. Specification-Driven**: Following Speckit workflow, spec-driven design

### Technology Constraints

- [x] **Platform**: Windows 10/11, .NET 8.0 for DAO layer, MySQL 5.7.24 for procedures
- [x] **Database**: MySQL only (mtm_receiving_application), no Infor Visual dependency
- [x] **MySQL 5.7.24 Compatible**: Verified - using basic CRUD, no CTEs/window functions, JSON type supported
- [x] **Required Packages**: MySQL.Data (existing), Helper_Database_StoredProcedure (existing)

### Critical Constraints

- [x] **Infor Visual READ ONLY**: N/A - This feature uses MySQL only, no Infor Visual integration
- [x] **Forbidden Practices**: 
  - [x] No direct SQL in C# (using stored procedures only)
  - [x] No DAO exceptions (returning Model_Dao_Result.Failure instead)
  - [x] No service locator (DAOs are static, called from injected services)

### Justification for Violations
**No violations identified.** Feature fully aligns with all constitutional principles and constraints.

## Project Structure

### Documentation (this feature)

```text
specs/CompletedSpecs/005-dunnage-stored-procedures/
├── spec.md              # Feature specification (complete)
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0 - Technology decisions and patterns
├── data-model.md        # Phase 1 - Database schema and relationships
├── quickstart.md        # Phase 1 - Developer onboarding guide
├── contracts/           # Phase 1 - Stored procedure signatures
│   ├── dunnage_types.md
│   ├── dunnage_specs.md
│   ├── dunnage_parts.md
│   ├── dunnage_loads.md
│   └── inventoried_dunnage.md
└── checklists/
    └── requirements.md  # Spec quality validation (complete)
```

### Source Code (repository root)

```text
MTM_Receiving_Application/
├── Database/
│   └── StoredProcedures/
│       ├── Dunnage/                    # NEW - This feature
│       │   ├── sp_dunnage_types_*.sql          # 7 procedures
│       │   ├── sp_dunnage_specs_*.sql          # 7 procedures  
│       │   ├── sp_dunnage_parts_*.sql          # 8 procedures
│       │   ├── sp_dunnage_loads_*.sql          # 7 procedures
│       │   └── sp_inventoried_dunnage_*.sql    # 6 procedures
│       └── Deploy-StoredProcedures.ps1 # MODIFY - Add dunnage procedures
├── Data/
│   └── Dunnage/                        # NEW - DAO layer
│       ├── Dao_DunnageType.cs          # 7 methods (wraps type SPs)
│       ├── Dao_DunnageSpec.cs          # 7 methods (wraps spec SPs)
│       ├── Dao_DunnagePart.cs          # 8 methods (wraps part SPs)
│       ├── Dao_DunnageLoad.cs          # 7 methods (wraps load SPs)
│       └── Dao_InventoriedDunnage.cs   # 6 methods (wraps inventory SPs)
├── Models/
│   └── Dunnage/                        # NEW - Data models
│       ├── Model_DunnageType.cs
│       ├── Model_DunnageSpec.cs
│       ├── Model_DunnagePart.cs
│       ├── Model_DunnageLoad.cs
│       └── Model_InventoriedDunnage.cs
└── MTM_Receiving_Application.Tests/
    └── Integration/
        └── Dunnage/                    # NEW - Integration tests
            ├── Dao_DunnageType_Tests.cs
            ├── Dao_DunnageSpec_Tests.cs
            ├── Dao_DunnagePart_Tests.cs
            ├── Dao_DunnageLoad_Tests.cs
            └── Dao_InventoriedDunnage_Tests.cs
```

**Structure Decision**: Database-first approach with stored procedures as the primary deliverable. DAOs provide thin async wrappers using `Helper_Database_StoredProcedure` utility. Models match database table schemas. No UI components in this feature - pure data access layer.

## Complexity Tracking

**No complexity violations.** This feature implements standard DAO pattern with stored procedures, fully aligned with constitutional principles. Static DAO classes are the established pattern in this codebase (see existing `Dao_User.cs`, `Dao_ReceivingLine.cs` examples).

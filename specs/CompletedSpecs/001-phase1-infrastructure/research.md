# Phase 0: Research & Technology Decisions

**Feature**: Phase 1 Infrastructure Setup  
**Date**: December 15, 2025  
**Status**: Complete

## Overview

This document captures research findings and technology decisions for establishing the Phase 1 infrastructure. Since this infrastructure adapts proven patterns from the existing MTM_WIP_Application_WinForms project, most technology choices are predetermined to maintain consistency.

## Research Questions & Findings

### 1. Database Connectivity Approach

**Question**: What is the best approach for MySQL connectivity in .NET 8.0 with WinUI 3?

**Decision**: Use MySql.Data (Oracle's official connector)

**Rationale**:
- Official MySQL connector with long-term support
- Proven compatibility with .NET 8.0
- Already used successfully in MTM_WIP_Application_WinForms
- Full support for async/await patterns
- Works well with stored procedures

**Alternatives Considered**:
- **MySqlConnector (async-first)**: More modern, better async performance, but switching would require retraining team and rewriting WIP application patterns
- **Entity Framework Core with MySQL provider**: Adds ORM complexity unnecessary for stored procedure-based architecture
- **Dapper with MySql.Data**: Lightweight ORM but conflicts with requirement to use stored procedures exclusively

**Implementation Notes**:
- NuGet package: MySql.Data version 9.0+
- Connection string format: `Server=172.16.1.104;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;`
- Use MySqlConnection, MySqlCommand for stored procedure execution
- Implement connection pooling through connection string parameters

### 2. Error Handling Pattern

**Question**: How should errors be handled in WinUI 3 vs WinForms?

**Decision**: Adapt existing Service_ErrorHandler to use ContentDialog instead of MessageBox

**Rationale**:
- Preserves proven error handling architecture from WIP application
- Service pattern allows dependency injection (future-ready)
- ContentDialog is WinUI 3's native dialog component
- Maintains separation of concerns (UI display logic separate from error logging)

**Implementation Notes**:
- Replace `MessageBox.Show()` calls with `ContentDialog` instances
- ContentDialog requires XamlRoot reference (from Window or Page)
- Keep identical error logging to file system (%APPDATA%\MTM_Receiving_Application\Logs\)
- Preserve error severity levels (Info, Warning, Error, Critical, Fatal)

### 3. Async/Await Best Practices

**Question**: How should async/await patterns be implemented throughout the infrastructure?

**Decision**: All database operations return `Task<Model_Dao_Result>`, all service methods are async

**Rationale**:
- WinUI 3 UI thread must not block on I/O operations
- Async patterns prevent UI freezing during database calls
- Industry standard for modern .NET applications
- Future-proofs for potential web service integration

**Best Practices**:
- DAO methods: `public static async Task<Model_Dao_Result> InsertReceivingLineAsync(...)`
- Use `await` for all MySqlCommand.ExecuteNonQueryAsync() calls
- Use `ConfigureAwait(false)` in library code to avoid context capture
- ViewModels (Phase 2) will use async commands from CommunityToolkit.Mvvm

### 4. Stored Procedure Design

**Question**: What is the optimal stored procedure parameter and return structure?

**Decision**: Use OUT parameters for status codes and error messages, follow WIP application conventions

**Rationale**:
- Consistent with existing MTM_WIP_Application_WinForms patterns
- Status code (INT): 0 = success, 1 = error
- Error message (VARCHAR(500)): Empty on success, descriptive message on failure
- Allows transaction rollback with error reporting
- No result sets needed for INSERT/UPDATE operations

**Standard Procedure Template**:
```sql
CREATE PROCEDURE procedure_name(
    IN p_Parameter1 TYPE,
    IN p_Parameter2 TYPE,
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = 1;
        ROLLBACK;
    END;

    START TRANSACTION;
    
    -- Validation logic
    -- Insert/Update logic
    
    SET p_Status = 0;
    SET p_ErrorMsg = '';
    COMMIT;
END
```

### 5. Model Structure Design

**Question**: How should models be organized to match both database schema and Google Sheets?

**Decision**: Feature-based model organization (Models/Receiving, Models/Labels, Models/Lookup)

**Rationale**:
- Aligns with database table groupings
- Matches Google Sheets workbook structure
- Prevents namespace collisions (Model_ReceivingLine vs Model_DunnageLine)
- Makes future feature additions easier (add Models/NewFeature/)
- Team already familiar with this structure from WIP application

**Model Design Principles**:
- Properties match database column names exactly (case-insensitive MySQL)
- Use C# naming conventions: PascalCase for properties, camelCase for private fields
- Default values match database defaults: `public DateTime Date { get; set; } = DateTime.Now;`
- Calculated properties (LabelText) computed from other properties
- No database logic in models (keep them as pure data containers)

### 6. DAO Pattern Implementation

**Question**: How should the DAO pattern be structured for maintainability?

**Decision**: Static async methods returning Model_Dao_Result, one DAO class per entity

**Rationale**:
- Static methods simplify calling: `await Dao_ReceivingLine.InsertAsync(line)`
- No need for dependency injection at data layer (services handle that)
- Model_Dao_Result provides consistent response structure
- One DAO per entity prevents bloated classes
- Clear separation: Dao_ReceivingLine handles label_table_receiving table only

**DAO Method Naming**:
- Insert operations: `InsertReceivingLineAsync(Model_ReceivingLine line)`
- Batch insert: `InsertReceivingLinesAsync(List<Model_ReceivingLine> lines)`
- Query operations: `GetAllReceivingLinesAsync()`, `GetReceivingLineByIdAsync(int id)`
- Update operations: `UpdateReceivingLineAsync(Model_ReceivingLine line)`
- Delete operations: `DeleteReceivingLineAsync(int id)`

### 7. Helper_Database_StoredProcedure Capabilities

**Question**: What advanced features should the stored procedure helper provide?

**Decision**: Retry logic, performance monitoring, progress tracking, parameter validation

**Rationale**:
- Retry logic handles transient MySQL connection failures
- Performance monitoring identifies slow queries early
- Progress tracking useful for bulk operations (future Excel imports)
- Parameter validation prevents bad data from reaching database

**Implementation Features**:
- Automatic retry: 3 attempts with exponential backoff (100ms, 200ms, 400ms)
- Execution time tracking: Captured in Model_Dao_Result.ExecutionTimeMs
- Row count tracking: Captured in Model_Dao_Result.AffectedRows
- Parameter validation: Check for required parameters before execution

### 8. Template Migration Strategy

**Question**: How should template files be migrated from WIP application?

**Decision**: Copy to _TEMPLATE_*.txt, update namespaces, convert to .cs

**Rationale**:
- .txt extension prevents accidental compilation during migration
- Preserves original files in WIP application (no breaking changes)
- Namespace find-replace can be automated via PowerShell
- Allows verification before committing .cs files

**Migration Steps**:
1. Copy WIP file to `_TEMPLATE_Name.txt`
2. Bulk replace: `MTM_WIP_Application_Winforms` → `MTM_Receiving_Application`
3. Review for WinForms-specific code (MessageBox, Form references)
4. Convert to .cs and verify compilation
5. Delete .txt template after verification

### 9. Database Schema Design

**Question**: What indexes are needed for optimal query performance?

**Decision**: Index all foreign keys and frequently-queried columns

**Indexed Columns**:
- **label_table_receiving**: part_id, po_number, employee_number, transaction_date
- **label_table_dunnage**: po_number, transaction_date
- **routing_labels**: deliver_to, department, transaction_date

**Rationale**:
- PO number used for grouping labels
- Part ID used for lookups from Infor Visual
- Employee number used for audit trails
- Transaction date used for historical queries
- Indexes improve query performance with minimal insert overhead

### 10. Logging Strategy

**Question**: Where should logs be stored and what should be logged?

**Decision**: %APPDATA%\MTM_Receiving_Application\Logs\ with daily rotation

**Rationale**:
- %APPDATA% is user-specific, no admin rights required
- Daily rotation prevents log files from growing too large
- Separate log files per day: `app_2025-12-15.log`
- Full exception details including stack traces
- Context information: timestamp, severity, operation name, user

**Log Format**:
```
[2025-12-15 10:30:45.123] [ERROR] [Dao_ReceivingLine.InsertReceivingLineAsync]
Message: Failed to insert receiving line for PO 12345
Exception: MySql.Data.MySqlClient.MySqlException
Stack Trace: ...
User: DOMAIN\username
```

## Technology Stack Summary

| Component | Technology | Version | Rationale |
|-----------|-----------|---------|-----------|
| Language | C# | 12 | Latest with .NET 8.0 |
| Framework | .NET | 8.0 | LTS release, WinUI 3 requirement |
| UI Framework | WinUI 3 | Windows App SDK 1.5+ | Modern Windows UI |
| Database | MySQL | 5.7.24 | Existing infrastructure |
| DB Connector | MySql.Data | 9.0+ | Official, proven |
| MVVM Toolkit | CommunityToolkit.Mvvm | 8.2+ | Microsoft-recommended |
| Testing | xUnit or MSTest | Latest | .NET standard |

## Implementation Readiness

✅ **All research complete** - No NEEDS CLARIFICATION items remain

All technology decisions are finalized and ready for Phase 1 implementation. The infrastructure will follow proven patterns from MTM_WIP_Application_WinForms with necessary adaptations for WinUI 3.

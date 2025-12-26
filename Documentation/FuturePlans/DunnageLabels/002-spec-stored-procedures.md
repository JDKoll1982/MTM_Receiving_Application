# Feature Specification: Dunnage Stored Procedures

**Feature Branch**:  `002-stored-procedures`  
**Created**: 2025-12-26  
**Status**: Ready for Implementation  
**Parent Feature**: Dunnage Receiving System V2  
**Depends On**: 004-database-foundation

## Overview

Create all stored procedures required for CRUD operations on the dunnage database tables.  This layer provides a consistent, secure, and maintainable interface between the application and the database, enforcing business rules at the data access level.

**Architecture Principle**: ALL database operations MUST go through stored procedures - no direct SQL in C# code.

## User Scenarios & Testing

### User Story 1 - Dunnage Type Stored Procedures (Priority: P1)

As a **data access layer**, I need stored procedures for dunnage type operations (get all, get by ID, insert, update, delete, count parts, count transactions) so that the application can manage dunnage types safely.

**Why this priority**: Types are the foundation of the dunnage hierarchy. All parts and specs depend on types existing first.

**Independent Test**: Can be tested by executing each stored procedure directly in MySQL with test data and verifying correct results, error handling, and referential integrity enforcement.

**Acceptance Scenarios**: 

1. **Given** a populated `dunnage_types` table, **When** `sp_dunnage_types_get_all` is called, **Then** all types are returned with correct columns
2. **Given** a valid type ID, **When** `sp_dunnage_types_get_by_id` is called, **Then** the specific type record is returned
3. **Given** a new type name and user, **When** `sp_dunnage_types_insert` is called, **Then** a new type is created with auto-generated ID and current timestamp
4. **Given** a type ID with associated parts, **When** `sp_dunnage_types_delete` is called, **Then** deletion fails due to foreign key constraint
5. **Given** a type ID, **When** `sp_dunnage_types_count_parts` is called, **Then** the count of associated parts is returned for impact analysis

---

### User Story 2 - Dunnage Spec Stored Procedures (Priority: P1)

As a **data access layer**, I need stored procedures for specification operations (get by type, get all, insert, update, delete by ID, delete by type, count parts using spec) so that the application can manage dynamic spec schemas per type.

**Why this priority**:  Specs define the data structure for parts.  Without spec management, parts cannot have validated attributes.

**Independent Test**: Can be tested by creating a type, inserting spec JSON, modifying spec JSON, and verifying JSON is correctly stored/retrieved with proper validation.

**Acceptance Scenarios**:

1. **Given** a type ID, **When** `sp_dunnage_specs_get_by_type` is called, **Then** the JSON spec schema for that type is returned
2. **Given** no parameters, **When** `sp_dunnage_specs_get_all` is called, **Then** all spec records across all types are returned (for CSV column union)
3. **Given** a valid type ID and spec JSON, **When** `sp_dunnage_specs_insert` is called, **Then** the spec is created with current timestamp
4. **Given** a type ID and a spec key, **When** `sp_dunnage_specs_count_parts_using_spec` is called, **Then** the count of parts affected by removing that spec is returned
5. **Given** a type ID, **When** `sp_dunnage_specs_delete_by_type` is called, **Then** all specs for that type are removed (CASCADE behavior)

---

### User Story 3 - Dunnage Part Stored Procedures (Priority: P1)

As a **data access layer**, I need stored procedures for part operations (get all, get by type, get by ID, insert, update, delete, count transactions, search) so that the application can manage the master data for dunnage parts.

**Why this priority**: Parts represent the actual physical items being received. This is the core master data that transaction records reference.

**Independent Test**:  Can be tested by creating parts with various spec values (JSON), verifying unique PartID constraint, and testing search functionality with filters.

**Acceptance Scenarios**:

1. **Given** populated parts table, **When** `sp_dunnage_parts_get_all` is called, **Then** all parts are returned with their type information
2. **Given** a type ID, **When** `sp_dunnage_parts_get_by_type` is called, **Then** only parts of that type are returned
3. **Given** a unique PartID, **When** `sp_dunnage_parts_insert` is called, **Then** the part is created with JSON spec values
4. **Given** a duplicate PartID, **When** `sp_dunnage_parts_insert` is called, **Then** insert fails with unique constraint error
5. **Given** a PartID, **When** `sp_dunnage_parts_count_transactions` is called, **Then** the count of transaction records using this part is returned
6. **Given** a search text and optional type filter, **When** `sp_dunnage_parts_search` is called, **Then** matching parts are returned

---

### User Story 4 - Dunnage Load Stored Procedures (Priority: P1)

As a **data access layer**, I need stored procedures for load/transaction operations (get all, get by date range, insert single, insert batch, update, delete) so that the application can record received dunnage transactions.

**Why this priority**:  Loads are the transactional data representing actual receiving events. This is the end goal of the entire workflow.

**Independent Test**: Can be tested by inserting loads with UUID primary keys, verifying quantity constraints, testing date range filtering, and validating batch insert performance.

**Acceptance Scenarios**:

1. **Given** populated loads table, **When** `sp_dunnage_loads_get_all` is called, **Then** all transaction records are returned with part and type details
2. **Given** a start and end date, **When** `sp_dunnage_loads_get_by_date_range` is called, **Then** only loads within that range are returned
3. **Given** a load with quantity = 0, **When** `sp_dunnage_loads_insert` is called, **Then** insert fails due to CHECK constraint
4. **Given** an array of loads, **When** `sp_dunnage_loads_insert_batch` is called, **Then** all loads are inserted in a single transaction
5. **Given** a load UUID, **When** `sp_dunnage_loads_delete` is called, **Then** the record is permanently removed (hard delete)

---

### User Story 5 - Inventoried Dunnage Stored Procedures (Priority: P2)

As a **data access layer**, I need stored procedures for inventoried parts list operations (get all, check if part inventoried, get by part, insert, update, delete) so that the application can manage Visual ERP inventory notifications.

**Why this priority**:  Inventory notifications are important for data quality but not blocking for basic receiving workflow. Can be implemented after core CRUD is working.

**Independent Test**: Can be tested by adding parts to the inventoried list, checking inventory status, and verifying notification data is correctly stored/retrieved.

**Acceptance Scenarios**:

1. **Given** a PartID, **When** `sp_inventoried_dunnage_check` is called, **Then** a boolean indicating inventory requirement is returned
2. **Given** a PartID in the inventoried list, **When** `sp_inventoried_dunnage_get_by_part` is called, **Then** inventory method and notes are returned
3. **Given** a new PartID, **When** `sp_inventoried_dunnage_insert` is called, **Then** the part is added to the inventoried list with method and notes
4. **Given** no parameters, **When** `sp_inventoried_dunnage_get_all` is called, **Then** all inventoried parts are returned for admin management
5. **Given** a PartID, **When** `sp_inventoried_dunnage_delete` is called, **Then** the part is removed from the inventoried list

---

### Edge Cases

- What happens when calling `sp_dunnage_types_delete` on a type with parts?  (Foreign key RESTRICT should fail - verified by count_parts SP first)
- What happens when inserting a part with malformed JSON spec values? (MySQL JSON validation should reject)
- What happens when batch inserting loads where one fails validation? (Transaction rollback - all or nothing)
- What happens when searching parts with NULL type filter? (Should return all parts, filter is optional)
- What happens when getting specs for a type that has no specs defined? (Return empty result set, not error)

## Requirements

### Functional Requirements

#### Dunnage Type Stored Procedures (7 SPs)
- **FR-001**: System MUST provide `sp_dunnage_types_get_all()` to retrieve all types
- **FR-002**: System MUST provide `sp_dunnage_types_get_by_id(p_id)` to retrieve a specific type
- **FR-003**: System MUST provide `sp_dunnage_types_insert(p_type_name, p_user)` to create new types
- **FR-004**:  System MUST provide `sp_dunnage_types_update(p_id, p_type_name, p_user)` to modify types
- **FR-005**:  System MUST provide `sp_dunnage_types_delete(p_id)` to remove types (with FK checks)
- **FR-006**: System MUST provide `sp_dunnage_types_count_parts(p_type_id)` to count dependent parts
- **FR-007**:  System MUST provide `sp_dunnage_types_count_transactions(p_type_id)` to count dependent transactions

#### Dunnage Spec Stored Procedures (7 SPs)
- **FR-008**: System MUST provide `sp_dunnage_specs_get_by_type(p_type_id)` to retrieve spec schema for a type
- **FR-009**: System MUST provide `sp_dunnage_specs_get_all()` to retrieve all specs (for CSV column union)
- **FR-010**: System MUST provide `sp_dunnage_specs_insert(p_type_id, p_specs_json, p_user)` to create spec schemas
- **FR-011**: System MUST provide `sp_dunnage_specs_update(p_spec_id, p_specs_json, p_user)` to modify schemas
- **FR-012**: System MUST provide `sp_dunnage_specs_delete(p_spec_id)` to remove a spec record
- **FR-013**: System MUST provide `sp_dunnage_specs_delete_by_type(p_type_id)` to remove all specs for a type
- **FR-014**: System MUST provide `sp_dunnage_specs_count_parts_using_spec(p_type_id, p_spec_key)` for impact analysis

#### Dunnage Part Stored Procedures (8 SPs)
- **FR-015**: System MUST provide `sp_dunnage_parts_get_all()` to retrieve all parts with type info
- **FR-016**: System MUST provide `sp_dunnage_parts_get_by_type(p_type_id)` to filter parts by type
- **FR-017**: System MUST provide `sp_dunnage_parts_get_by_id(p_part_id)` to retrieve a specific part
- **FR-018**:  System MUST provide `sp_dunnage_parts_insert(p_part_id, p_type_id, p_spec_values_json, p_user)` to create parts
- **FR-019**: System MUST provide `sp_dunnage_parts_update(p_part_id, p_spec_values_json, p_user)` to modify parts
- **FR-020**:  System MUST provide `sp_dunnage_parts_delete(p_part_id)` to remove parts (with FK checks)
- **FR-021**: System MUST provide `sp_dunnage_parts_count_transactions(p_part_id)` for impact analysis
- **FR-022**: System MUST provide `sp_dunnage_parts_search(p_search_text, p_type_id)` for filtered searches

#### Dunnage Load Stored Procedures (6 SPs)
- **FR-023**: System MUST provide `sp_dunnage_loads_get_all()` to retrieve all transaction records
- **FR-024**: System MUST provide `sp_dunnage_loads_get_by_date_range(p_start_date, p_end_date)` for filtering
- **FR-025**: System MUST provide `sp_dunnage_loads_insert(p_id, p_part_id, p_type_id, p_qty, p_po, p_user, p_loc, p_label)` to create loads
- **FR-026**:  System MUST provide `sp_dunnage_loads_insert_batch(p_loads_json)` for batch inserts
- **FR-027**:  System MUST provide `sp_dunnage_loads_update(p_id, p_qty, p_po, p_loc)` to modify loads
- **FR-028**:  System MUST provide `sp_dunnage_loads_delete(p_id)` for hard deletes

#### Inventoried Dunnage Stored Procedures (6 SPs)
- **FR-029**: System MUST provide `sp_inventoried_dunnage_get_all()` to retrieve all inventoried parts
- **FR-030**: System MUST provide `sp_inventoried_dunnage_check(p_part_id)` to return boolean inventory status
- **FR-031**: System MUST provide `sp_inventoried_dunnage_get_by_part(p_part_id)` to get inventory details
- **FR-032**: System MUST provide `sp_inventoried_dunnage_insert(p_part_id, p_method, p_notes, p_user)` to add to list
- **FR-033**: System MUST provide `sp_inventoried_dunnage_update(p_part_id, p_method, p_notes)` to modify entries
- **FR-034**: System MUST provide `sp_inventoried_dunnage_delete(p_part_id)` to remove from list

### Non-Functional Requirements

- **NFR-001**: All stored procedures MUST use consistent parameter naming (`p_` prefix)
- **NFR-002**: All stored procedures MUST include header comments documenting purpose, parameters, and return values
- **NFR-003**: All INSERT procedures MUST return the newly created ID or UUID
- **NFR-004**:  All batch operations MUST use transactions (all-or-nothing)
- **NFR-005**:  All DELETE procedures MUST check for dependent records before deleting (or rely on FK constraints)
- **NFR-006**: All procedures MUST handle NULL parameters appropriately (e.g., optional filters)
- **NFR-007**: Search procedures MUST use LIKE queries with wildcard support for partial matching
- **NFR-008**:  Date range queries MUST support inclusive start and exclusive end dates

## Success Criteria

### Measurable Outcomes

- **SC-001**:  All 34 stored procedures execute successfully without syntax errors
- **SC-002**:  All procedures return expected result sets with correct column names and data types
- **SC-003**: Insert procedures correctly populate timestamp fields (EntryDate, CreatedAt, etc.)
- **SC-004**: Update procedures correctly populate alter timestamp fields (AlterDate, SpecAlterDate)
- **SC-005**: Delete procedures correctly respect foreign key constraints (RESTRICT vs CASCADE)
- **SC-006**:  Batch insert procedures complete 100 records in under 1 second
- **SC-007**: Search procedures return results in under 500ms for datasets up to 10,000 records
- **SC-008**: Impact analysis procedures (count_parts, count_transactions) return accurate counts

## Out of Scope

- ❌ Complex reporting stored procedures (e.g., aggregations, analytics)
- ❌ Database triggers (handled by stored procedure logic)
- ❌ Soft delete procedures (using hard deletes only)
- ❌ Audit trail procedures (future feature)
- ❌ Bulk import procedures (handled by batch insert SPs)

## Dependencies

- 004-database-foundation (all tables must exist)
- MySQL 8.0+ with stored procedure support
- Database user with CREATE PROCEDURE privilege

## Files to be Created

### Dunnage Type SPs
- `Database/StoredProcedures/Receiving/sp_dunnage_types_get_all.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_types_get_by_id.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_types_insert.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_types_update.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_types_delete.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_types_count_parts.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_types_count_transactions.sql`

### Dunnage Spec SPs
- `Database/StoredProcedures/Receiving/sp_dunnage_specs_get_by_type.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_specs_get_all.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_specs_insert.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_specs_update.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_specs_delete.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_specs_delete_by_type.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_specs_count_parts_using_spec.sql`

### Dunnage Part SPs
- `Database/StoredProcedures/Receiving/sp_dunnage_parts_get_all.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_parts_get_by_type.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_parts_get_by_id.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_parts_insert.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_parts_update.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_parts_delete.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_parts_count_transactions.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_parts_search.sql`

### Dunnage Load SPs
- `Database/StoredProcedures/Receiving/sp_dunnage_loads_get_all. sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_loads_get_by_date_range.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_loads_insert.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_loads_insert_batch.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_loads_update.sql`
- `Database/StoredProcedures/Receiving/sp_dunnage_loads_delete.sql`

### Inventoried Dunnage SPs
- `Database/StoredProcedures/Receiving/sp_inventoried_dunnage_get_all.sql`
- `Database/StoredProcedures/Receiving/sp_inventoried_dunnage_check.sql`
- `Database/StoredProcedures/Receiving/sp_inventoried_dunnage_get_by_part. sql`
- `Database/StoredProcedures/Receiving/sp_inventoried_dunnage_insert.sql`
- `Database/StoredProcedures/Receiving/sp_inventoried_dunnage_update.sql`
- `Database/StoredProcedures/Receiving/sp_inventoried_dunnage_delete.sql`

## Review & Acceptance Checklist

### Requirement Completeness
- [x] All 34 stored procedures are defined with clear signatures
- [x] Parameter naming convention is consistent (`p_` prefix)
- [x] Return value specifications are clear (result sets, IDs, booleans)
- [x] Impact analysis procedures are specified for admin delete operations

### Clarity & Unambiguity
- [x] Each procedure's purpose is clearly stated
- [x] Parameter data types match database column types
- [x] Optional vs required parameters are explicitly documented
- [x] Batch operations use JSON or table-valued parameters

### Testability
- [x] Each user story can be tested by executing SPs directly in MySQL
- [x] Success criteria define performance benchmarks (batch insert < 1s, search < 500ms)
- [x] Edge cases define expected error scenarios (FK violations, constraint failures)

### Security & Performance
- [x] All procedures use parameterized inputs (SQL injection prevention)
- [x] Batch operations use transactions for atomicity
- [x] Search queries will use indexed columns for performance
- [x] No dynamic SQL generation in procedures

## Clarifications

### Resolved Questions

**Q1**: Should batch insert use JSON array or table-valued parameters?  
**A1**: Use JSON array parameter.  More flexible, easier to call from C#, MySQL has native JSON parsing.

**Q2**: Should search support regex patterns?  
**A2**:  No.  Use simple LIKE with `%wildcard%` syntax. Regex adds complexity and performance overhead.

**Q3**: Should procedures log to audit tables?  
**A3**: No.  Logging is handled by application layer (ILoggingService). Procedures only manage core data.

**Q4**: Should delete procedures soft delete or hard delete?  
**A4**: Hard delete only. `sp_dunnage_loads_delete` permanently removes records. No soft delete columns exist. 
# Feature Specification: Dunnage Stored Procedures

**Feature Branch**: `005-dunnage-stored-procedures`  
**Created**: 2025-12-26  
**Status**: Draft  
**Input**: User description: "Create all stored procedures required for CRUD operations on the dunnage database tables"

## User Scenarios & Testing *(mandatory)*

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

**Why this priority**: Specs define the data structure for parts. Without spec management, parts cannot have validated attributes.

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

**Independent Test**: Can be tested by creating parts with various spec values (JSON), verifying unique PartID constraint, and testing search functionality with filters.

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

**Why this priority**: Loads are the transactional data representing actual receiving events. This is the end goal of the entire workflow.

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

**Why this priority**: Inventory notifications are important for data quality but not blocking for basic receiving workflow. Can be implemented after core CRUD is working.

**Independent Test**: Can be tested by adding parts to the inventoried list, checking inventory status, and verifying notification data is correctly stored/retrieved.

**Acceptance Scenarios**:

1. **Given** a PartID, **When** `sp_inventoried_dunnage_check` is called, **Then** a boolean indicating inventory requirement is returned
2. **Given** a PartID in the inventoried list, **When** `sp_inventoried_dunnage_get_by_part` is called, **Then** inventory method and notes are returned
3. **Given** a new PartID, **When** `sp_inventoried_dunnage_insert` is called, **Then** the part is added to the inventoried list with method and notes
4. **Given** no parameters, **When** `sp_inventoried_dunnage_get_all` is called, **Then** all inventoried parts are returned for admin management
5. **Given** a PartID, **When** `sp_inventoried_dunnage_delete` is called, **Then** the part is removed from the inventoried list

---

### Edge Cases

- What happens when calling `sp_dunnage_types_delete` on a type with parts? (Foreign key RESTRICT should fail - verified by count_parts SP first)
- What happens when inserting a part with malformed JSON spec values? (MySQL JSON validation should reject)
- What happens when batch inserting loads where one fails validation? (Transaction rollback - all or nothing)
- What happens when searching parts with NULL type filter? (Should return all parts, filter is optional)
- What happens when getting specs for a type that has no specs defined? (Return empty result set, not error)

## Requirements *(mandatory)*

### Functional Requirements

#### Dunnage Type Stored Procedures (7 SPs)
- **FR-001**: System MUST provide `sp_dunnage_types_get_all()` to retrieve all types
- **FR-002**: System MUST provide `sp_dunnage_types_get_by_id(p_id)` to retrieve a specific type
- **FR-003**: System MUST provide `sp_dunnage_types_insert(p_type_name, p_user)` to create new types
- **FR-004**: System MUST provide `sp_dunnage_types_update(p_id, p_type_name, p_user)` to modify types
- **FR-005**: System MUST provide `sp_dunnage_types_delete(p_id)` to remove types (with FK checks)
- **FR-006**: System MUST provide `sp_dunnage_types_count_parts(p_type_id)` to count dependent parts
- **FR-007**: System MUST provide `sp_dunnage_types_count_transactions(p_type_id)` to count dependent transactions

#### Dunnage Spec Stored Procedures (6 SPs)
- **FR-008**: System MUST provide `sp_dunnage_specs_get_by_type(p_type_id)` to retrieve specs for a type
- **FR-009**: System MUST provide `sp_dunnage_specs_get_all()` to retrieve all spec records
- **FR-010**: System MUST provide `sp_dunnage_specs_insert(p_type_id, p_spec_key, p_spec_value, p_user)` to create specs
- **FR-011**: System MUST provide `sp_dunnage_specs_update(p_id, p_spec_value, p_user)` to modify specs
- **FR-012**: System MUST provide `sp_dunnage_specs_delete_by_id(p_id)` to remove individual specs
- **FR-013**: System MUST provide `sp_dunnage_specs_delete_by_type(p_type_id)` to remove all specs for a type
- **FR-014**: System MUST provide `sp_dunnage_specs_count_parts_using_spec(p_type_id, p_spec_key)` for impact analysis

#### Dunnage Part Stored Procedures (8 SPs)
- **FR-015**: System MUST provide `sp_dunnage_parts_get_all()` to retrieve all parts
- **FR-016**: System MUST provide `sp_dunnage_parts_get_by_type(p_type_id)` to retrieve parts by type
- **FR-017**: System MUST provide `sp_dunnage_parts_get_by_id(p_part_id)` to retrieve a specific part
- **FR-018**: System MUST provide `sp_dunnage_parts_insert(p_part_id, p_type_id, p_spec_values, p_user)` to create parts
- **FR-019**: System MUST provide `sp_dunnage_parts_update(p_id, p_spec_values, p_user)` to modify parts
- **FR-020**: System MUST provide `sp_dunnage_parts_delete(p_id)` to remove parts
- **FR-021**: System MUST provide `sp_dunnage_parts_count_transactions(p_part_id)` for impact analysis
- **FR-022**: System MUST provide `sp_dunnage_parts_search(p_search_text, p_type_id)` to search parts with optional type filter

#### Dunnage Load Stored Procedures (6 SPs)
- **FR-023**: System MUST provide `sp_dunnage_loads_get_all()` to retrieve all transaction records
- **FR-024**: System MUST provide `sp_dunnage_loads_get_by_date_range(p_start_date, p_end_date)` to filter by date
- **FR-025**: System MUST provide `sp_dunnage_loads_get_by_id(p_load_uuid)` to retrieve specific load
- **FR-026**: System MUST provide `sp_dunnage_loads_insert(p_part_id, p_quantity, p_user)` to create single loads
- **FR-027**: System MUST provide `sp_dunnage_loads_insert_batch(p_load_data)` to create multiple loads in transaction
- **FR-028**: System MUST provide `sp_dunnage_loads_update(p_load_uuid, p_quantity, p_user)` to modify loads
- **FR-029**: System MUST provide `sp_dunnage_loads_delete(p_load_uuid)` to remove loads (hard delete)

#### Inventoried Dunnage Stored Procedures (6 SPs)
- **FR-030**: System MUST provide `sp_inventoried_dunnage_get_all()` to retrieve all inventoried parts
- **FR-031**: System MUST provide `sp_inventoried_dunnage_check(p_part_id)` to check if part requires inventory notification
- **FR-032**: System MUST provide `sp_inventoried_dunnage_get_by_part(p_part_id)` to retrieve inventory details
- **FR-033**: System MUST provide `sp_inventoried_dunnage_insert(p_part_id, p_method, p_notes, p_user)` to add inventoried parts
- **FR-034**: System MUST provide `sp_inventoried_dunnage_update(p_id, p_method, p_notes, p_user)` to modify inventory settings
- **FR-035**: System MUST provide `sp_inventoried_dunnage_delete(p_part_id)` to remove parts from inventory list

### Key Entities

- **DunnageType**: Type categorization (ID, TypeName, CreatedBy, CreatedDate)
- **DunnageSpec**: Dynamic schema definition per type (ID, TypeID, SpecKey, SpecValue, metadata)
- **DunnagePart**: Master data for physical items (ID, PartID, TypeID, SpecValues JSON, metadata)
- **DunnageLoad**: Transaction records of received items (LoadUUID, PartID, Quantity, ReceivedDate, metadata)
- **InventoriedDunnage**: Parts requiring Visual ERP inventory notification (ID, PartID, InventoryMethod, Notes)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All 33 stored procedures execute successfully without syntax errors on MySQL 5.7.24
- **SC-002**: Each stored procedure completes in under 500ms with test datasets of up to 10,000 records
- **SC-003**: Referential integrity constraints are properly enforced (FK violations fail gracefully)
- **SC-004**: JSON validation works correctly for spec values (malformed JSON rejected)
- **SC-005**: Batch insert operations maintain transactional consistency (all-or-nothing behavior)
- **SC-006**: Search functionality returns relevant results in under 200ms for text queries
- **SC-007**: Impact analysis procedures (count_parts, count_transactions) provide accurate dependency counts

## Assumptions

- MySQL 5.7.24 JSON support is available for SpecValues storage and validation
- Stored procedure naming convention follows `sp_{table_name}_{operation}` pattern
- All user parameters include auditing fields (CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
- Foreign key constraints are enforced at database level (RESTRICT behavior for deletions)
- Batch insert procedures accept JSON arrays as parameters for multiple record operations
- Search functionality uses LIKE matching on PartID and spec values (no full-text search required)
- Hard delete policy for transaction records (no soft delete/archiving)

## Dependencies

- **Database Schema**: Requires dunnage database tables to be created (dunnage_types, dunnage_specs, dunnage_parts, dunnage_loads, inventoried_dunnage)
- **MySQL Connection**: Database connection and proper permissions for CREATE PROCEDURE
- **JSON Support**: MySQL 5.7.24+ for JSON data type validation and manipulation

## Out of Scope

- Advanced search functionality (full-text indexing, complex queries)
- Soft delete implementation (all deletes are permanent)
- Audit trail tables (using simple CreatedBy/ModifiedBy columns)
- Performance optimization beyond basic indexing
- Data migration procedures from existing systems
- Backup and recovery procedures for stored procedures

---

## Constitution Compliance Check *(mandatory)*

### Principle Alignment

- [x] **MVVM Architecture**: Data access layer requirements clearly separate database operations from business logic
- [x] **Database Layer**: All database operations go through stored procedures (no direct SQL in C# code)
- [x] **Dependency Injection**: DAOs will be registered as services in DI container
- [x] **Error Handling**: Error scenarios documented for constraint violations and validation failures
- [x] **Security & Authentication**: User tracking built into all modification procedures
- [x] **WinUI 3 Practices**: Technology-agnostic database specification suitable for any UI framework
- [x] **Specification-Driven**: Requirements focus on data operations without implementation details

### Special Constraints

- [x] **Infor Visual Integration**: No Infor Visual dependency (pure MySQL stored procedures)
- [x] **MySQL 5.7.24 Compatibility**: Using supported JSON functions and features only
- [x] **Async Operations**: All procedures designed for async execution from C# DAOs

### Notes
Stored procedures provide the foundation for the DAO layer pattern required by the constitution. All procedures include proper error handling and user auditing as specified in the architectural standards.

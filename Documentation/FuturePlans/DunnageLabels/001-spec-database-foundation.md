# Feature Specification:  Dunnage Database Foundation

**Feature Branch**:  `004-database-foundation`  
**Created**: 2025-12-26  
**Status**: Ready for Implementation  
**Parent Feature**:  Dunnage Receiving System V2

## Overview

Establish the foundational database schema for the Dunnage receiving application, replacing the legacy `label_table_dunnage` Google Sheets structure with a modern Type/Part/Specs architecture.  This foundation will support all future dunnage workflows. 

**Critical Constraint**: This is a **CLEAN SLATE** implementation. NO data migration, NO legacy support, NO dual-table logic.

## User Scenarios & Testing

### User Story 1 - Database Schema Creation (Priority: P1)

As a **database administrator**, I need to create the complete dunnage database schema so that the application can store dunnage types, specifications, parts, transactions, and inventory tracking data.

**Why this priority**: This is the foundation that all other features depend on. Without the database schema, no other functionality can be implemented.

**Independent Test**: Can be fully tested by running the migration script against a MySQL database and verifying all tables, indexes, and foreign keys are created correctly with the expected columns and data types.

**Acceptance Scenarios**: 

1. **Given** an empty MySQL database, **When** the migration script executes, **Then** all 5 tables are created with correct schemas
2. **Given** the migration script has run, **When** checking foreign key constraints, **Then** all relationships between tables are properly enforced
3. **Given** the new tables exist, **When** attempting to insert test data, **Then** all constraints (NOT NULL, UNIQUE, CHECK) are enforced

---

### User Story 2 - Seed Default Data (Priority: P1)

As a **system administrator**, I need the database to be pre-populated with standard dunnage types and default specifications so that users can immediately begin receiving common dunnage items without manual setup.

**Why this priority**:  Seeding default data makes the system immediately usable.  Without it, admins would need to manually create every dunnage type before the system can be used.

**Independent Test**: Can be tested by running the seed data script and verifying that 11 dunnage types exist with their default specification schemas (Width, Height, Depth, IsInventoriedToVisual).

**Acceptance Scenarios**: 

1. **Given** the schema migration is complete, **When** the seed data script executes, **Then** 11 dunnage types are created (Pallet, Crate, Box, Skid, Foam, Shrink Wrap, Bubble Wrap, Gaylord, Foldable Crate, Wooden Crate, Plastic Totes)
2. **Given** seed types are created, **When** querying the dunnage_specs table, **Then** each type has a default spec schema with Width, Height, Depth, and IsInventoriedToVisual fields
3. **Given** default data exists, **When** the application launches, **Then** users can immediately select from 11 standard dunnage types

---

### User Story 3 - Legacy Table Removal (Priority: P1)

As a **database administrator**, I need to cleanly remove the legacy `label_table_dunnage` table and all references so that there is no confusion between old and new systems.

**Why this priority**: This prevents data corruption, confusion, and ensures the new system is the single source of truth. 

**Independent Test**: Can be tested by verifying the `label_table_dunnage` table no longer exists in the database schema and that all SQL schema files no longer reference it.

**Acceptance Scenarios**:

1. **Given** the migration script includes a DROP TABLE statement, **When** the script executes, **Then** `label_table_dunnage` table is removed from the database
2. **Given** the table definition file `01_create_receiving_tables.sql`, **When** reviewing the file, **Then** no reference to `label_table_dunnage` exists
3. **Given** a backup of legacy data is needed, **When** an admin requests it, **Then** manual CSV export instructions are provided (optional)

---

### Edge Cases

- What happens when attempting to insert a dunnage type with a duplicate name?  (UNIQUE constraint should fail)
- What happens when attempting to delete a dunnage type that has associated parts? (Foreign key constraint should prevent deletion)
- What happens when attempting to insert a load record with an invalid PartID? (Foreign key constraint should fail)
- What happens when inserting spec values JSON with incorrect data types? (Application layer validation required - not database constraint)
- What happens when attempting to delete a part that has transaction history? (Foreign key with RESTRICT should prevent deletion)

## Requirements

### Functional Requirements

- **FR-001**: Database MUST create 5 tables:  `dunnage_types`, `dunnage_specs`, `dunnage_part_numbers`, `dunnage_loads`, `inventoried_dunnage_list`
- **FR-002**: All tables MUST use InnoDB engine with utf8mb4_unicode_ci character set for proper Unicode support
- **FR-003**: System MUST enforce referential integrity with foreign key constraints between related tables
- **FR-004**:  System MUST support JSON data type for flexible specification storage (`DunnageSpecs` and `DunnageSpecValues` columns)
- **FR-005**: System MUST completely remove `label_table_dunnage` table and all references from schema files
- **FR-006**:  System MUST seed 11 default dunnage types with standard specification schemas
- **FR-007**:  System MUST use UUID (CHAR(36)) for primary keys in transaction tables to support distributed systems
- **FR-008**: System MUST use AUTO_INCREMENT integers for master data tables (types, specs, parts, inventoried list)
- **FR-009**: System MUST enforce quantity > 0 constraint on `dunnage_loads` table
- **FR-010**: System MUST support hard delete operations (no soft delete columns) for transaction records

### Key Entities

- **DunnageType**: Metadata table storing type classifications (e.g., Pallet, Crate). Attributes: ID, TypeName, EntryDate, EntryUser, AlterDate, AlterUser
- **DunnageSpec**: Stores JSON specification schemas per type. Attributes: ID, DunnageTypeID, DunnageSpecs (JSON), SpecAlterDate, SpecAlterUser
- **DunnagePart**: Master data table storing actual part IDs with their spec values. Attributes: ID, PartID (unique), DunnageTypeID, DunnageSpecValues (JSON), EntryDate, EntryUser
- **DunnageLoad**: Transaction table for received dunnage loads. Attributes: ID (UUID), PartID, DunnageTypeID, Quantity, PONumber, ReceivedDate, UserId, Location, LabelNumber, CreatedAt
- **InventoriedDunnage**: Reference table for parts requiring Visual ERP inventory tracking. Attributes: ID, PartID, RequiresInventory, InventoryMethod, Notes, DateAdded, AddedBy

### Database Constraints

- **UC-001**: `dunnage_types. DunnageType` MUST be UNIQUE
- **UC-002**: `dunnage_part_numbers.PartID` MUST be UNIQUE
- **UC-003**: `inventoried_dunnage_list.PartID` MUST be UNIQUE
- **FK-001**: `dunnage_specs.DunnageTypeID` references `dunnage_types.ID` with CASCADE delete
- **FK-002**: `dunnage_part_numbers.DunnageTypeID` references `dunnage_types.ID` with RESTRICT delete
- **FK-003**: `dunnage_loads.PartID` references `dunnage_part_numbers. PartID` with RESTRICT delete
- **FK-004**: `dunnage_loads.DunnageTypeID` references `dunnage_types.ID` with RESTRICT delete
- **FK-005**:  `inventoried_dunnage_list.PartID` references `dunnage_part_numbers. PartID` with CASCADE delete
- **CK-001**: `dunnage_loads.Quantity` MUST be > 0

### Indexing Requirements

- **IDX-001**: Index on `dunnage_types.DunnageType` for fast type lookups
- **IDX-002**: Index on `dunnage_specs.DunnageTypeID` for fast spec retrieval
- **IDX-003**: Index on `dunnage_part_numbers.PartID` for fast part lookups
- **IDX-004**: Index on `dunnage_part_numbers.DunnageTypeID` for filtered queries
- **IDX-005**:  Index on `dunnage_loads.PartID` for transaction history queries
- **IDX-006**: Index on `dunnage_loads.ReceivedDate` for date range filtering
- **IDX-007**:  Index on `dunnage_loads.PONumber` for PO-based searches
- **IDX-008**:  Index on `dunnage_loads.UserId` for user-specific queries
- **IDX-009**: Index on `inventoried_dunnage_list.PartID` for fast inventory checks

## Success Criteria

### Measurable Outcomes

- **SC-001**: Migration script executes successfully on dev, test, and production databases without errors
- **SC-002**:  All 5 tables exist with correct schemas, indexes, and constraints
- **SC-003**: All 11 default dunnage types are seeded with default specification schemas
- **SC-004**: Legacy `label_table_dunnage` table is completely removed from database and schema files
- **SC-005**:  Foreign key constraints prevent orphaned records (verified through insert/delete tests)
- **SC-006**: JSON columns accept valid JSON and reject invalid JSON (MySQL validates)
- **SC-007**: Database supports at least 100,000 transaction records with sub-second query performance on indexed columns
- **SC-008**: All constraints (UNIQUE, CHECK, NOT NULL) are enforced and reject invalid data

## Non-Functional Requirements

- **NFR-001**: Migration scripts MUST be idempotent (can be run multiple times safely)
- **NFR-002**: All SQL scripts MUST include comments explaining table purpose and relationships
- **NFR-003**:  Backup of legacy data (if requested) MUST be manual CSV export - no automated migration
- **NFR-004**:  Migration completion time MUST be under 5 seconds for empty database
- **NFR-005**: Database schema MUST support concurrent read/write operations from multiple users

## Out of Scope

- ❌ Data migration from `label_table_dunnage` to new tables (manual process if needed)
- ❌ Archive table for legacy data (no historical preservation)
- ❌ Soft delete functionality (`IsDeleted` column) - using hard deletes only
- ❌ Audit trail tables for tracking changes (future feature)
- ❌ Database replication configuration (infrastructure concern)
- ❌ Stored procedures for CRUD operations (covered in Phase 2)

## Dependencies

- MySQL 8.0+ database server
- Database user with CREATE, DROP, ALTER, INSERT privileges
- Access to `mtm_receiving_application` database

## Files to be Created/Modified

### New Files
- `Database/Schemas/06_create_dunnage_tables.sql` - Main migration script with DROP and CREATE statements
- `Database/Schemas/06_seed_dunnage_data.sql` - Seed script for 11 default types and specs

### Modified Files
- `Database/Schemas/01_create_receiving_tables.sql` - Remove `label_table_dunnage` table definition

## Review & Acceptance Checklist

### Requirement Completeness
- [x] All 5 table schemas are fully defined with columns, data types, constraints
- [x] Foreign key relationships are clearly documented
- [x] Index requirements are specified
- [x] Seed data requirements are explicit (11 types with default specs)
- [x] Legacy removal requirements are clear

### Clarity & Unambiguity
- [x] Table names and column names follow project naming conventions
- [x] JSON schema format is documented (spec definitions vs spec values)
- [x] UUID vs AUTO_INCREMENT usage is clearly explained
- [x] CASCADE vs RESTRICT delete behavior is specified per foreign key

### Testability
- [x] Each user story can be independently verified
- [x] Success criteria are measurable (table existence, row counts, constraint enforcement)
- [x] Edge cases define expected failure scenarios
- [x] Independent test scenarios are provided for each user story

### Risk Assessment
- [x] No data migration eliminates risk of data corruption
- [x] Legacy table removal is explicit and documented
- [x] Foreign key constraints prevent orphaned records
- [x] Idempotent scripts reduce deployment risk

## Clarifications

### Resolved Questions

**Q1**: Should we archive legacy data before deletion?  
**A1**: No automated archival.  If needed, manual CSV export can be performed before migration.  Legacy data is considered obsolete. 

**Q2**: Should we use soft deletes (IsDeleted column)?  
**A2**: No.  Use hard deletes for all operations. Simpler implementation, no legacy technical debt.

**Q3**: Should migration script be reversible?  
**A3**:  No. This is a one-way migration. Once executed, the legacy system is permanently replaced.  Backups are the recovery mechanism.

**Q4**: Should we validate JSON schema at database level?  
**A4**: No. MySQL validates JSON syntax only.  Schema validation (data types, required fields) is handled by application layer. 
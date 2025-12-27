# Feature Specification:  Dunnage Models and Data Access Objects

**Feature Branch**: `003-models-and-daos`  
**Created**: 2025-12-26  
**Status**: Ready for Implementation  
**Parent Feature**:  Dunnage Receiving System V2  
**Depends On**: 004-database-foundation, 002-stored-procedures

## Overview

Create C# model classes and Data Access Object (DAO) static classes that provide type-safe access to the dunnage database through stored procedures.  This layer bridges the database and business logic, following the application's established DAO pattern.

**Architecture Principle**: Models use CommunityToolkit.Mvvm `ObservableObject` for MVVM binding.  DAOs are static classes returning `Model_Dao_Result<T>` for consistent error handling. 

## User Scenarios & Testing

### User Story 1 - Observable Model Classes (Priority: P1)

As a **ViewModel developer**, I need observable model classes for all dunnage entities so that UI controls can bind to properties with automatic change notification.

**Why this priority**: Models are the foundation of MVVM architecture. ViewModels cannot function without properly structured models.

**Independent Test**: Can be tested by creating model instances, modifying properties, and verifying `PropertyChanged` events fire correctly using unit tests.

**Acceptance Scenarios**: 

1. **Given** a `Model_DunnageType` instance, **When** setting the `DunnageType` property, **Then** `PropertyChanged` event fires with correct property name
2. **Given** a `Model_DunnagePart` with JSON spec values, **When** accessing `SpecValues` dictionary, **Then** deserialized values match the JSON string
3. **Given** a `Model_DunnageLoad`, **When** setting `Quantity` to 10, **Then** the property value updates and change notification triggers
4. **Given** a `Model_DunnageSession`, **When** adding loads to the collection, **Then** `HasLoads` computed property returns true
5. **Given** all model classes, **When** checking inheritance, **Then** all inherit from `ObservableObject` and use `[ObservableProperty]` attributes

**Status**: Partially Implemented. `Model_DunnageType`, `Model_DunnageSpec`, `Model_DunnagePart`, `Model_DunnageLoad`, `Model_InventoriedDunnage` created. `Model_DunnageSession` and Dictionary properties pending.

---

### User Story 2 - Type-Safe DAO Layer for Dunnage Types (Priority: P1)

**Status**: Implemented. `Dao_DunnageType` created with all methods.

---

### User Story 3 - DAO Layer for Specs, Parts, Loads, and Inventory (Priority: P1)

**Status**: Implemented. `Dao_DunnageSpec`, `Dao_DunnagePart`, `Dao_DunnageLoad`, `Dao_InventoriedDunnage` created with all methods.

---

### Edge Cases

- What happens when deserializing malformed JSON from `DunnageSpecValues` column?  (JSON parse exception caught, returned in error result)
- What happens when calling `GetPartsByTypeIdAsync()` with non-existent type ID? (Empty list returned, success=true)
- What happens when `InsertLoadsAsync()` batch has one invalid load (quantity=0)? (Entire batch fails, rollback, success=false)
- What happens when concurrent updates modify the same part? (Last write wins, no optimistic concurrency in this phase)
- What happens when navigating to related entity that doesn't exist (orphaned FK)? (Should be prevented by database constraints, DAO returns null for navigation properties)

## Requirements

### Functional Requirements - Models

#### Core Model Classes (6 Models)
- **FR-001**: System MUST provide `Model_DunnageType` inheriting from `ObservableObject` with properties:  Id, DunnageType, EntryDate, EntryUser, AlterDate, AlterUser
- **FR-002**: System MUST provide `Model_DunnageSpec` with properties: Id, DunnageTypeID, DunnageSpecs (JSON string), SpecAlterDate, SpecAlterUser, SpecsDefinition (Dictionary)
- **FR-003**: System MUST provide `Model_DunnagePart` with properties: Id, PartID, DunnageTypeID, DunnageSpecValues (JSON string), EntryDate, EntryUser, SpecValues (Dictionary), DunnageTypeName
- **FR-004**:  System MUST provide `Model_DunnageLoad` with properties: Id (Guid), PartID, DunnageTypeID, Quantity, PONumber, ReceivedDate, UserId, Location, LabelNumber, IsDeleted (removed per Q&A), CreatedAt, DunnageTypeName, IsSelected
- **FR-005**: System MUST provide `Model_InventoriedDunnage` with properties: Id, PartID, RequiresInventory, InventoryMethod, Notes, DateAdded, AddedBy
- **FR-006**: System MUST provide `Model_DunnageSession` for workflow state management with properties: SelectedTypeID, SelectedTypeName, SelectedPart, Quantity, PONumber, Location, Loads (ObservableCollection), HasLoads (computed)

**Status**: Partially Implemented. `Model_DunnageType`, `Model_DunnageSpec`, `Model_DunnagePart`, `Model_DunnageLoad`, `Model_InventoriedDunnage` created. `Model_DunnageSession` and Dictionary properties pending.

#### Model Behaviors
- **FR-007**: All models MUST use `[ObservableProperty]` attributes for automatic PropertyChanged implementation
- **FR-008**: Models with JSON columns MUST provide both string property (for database) and deserialized Dictionary property (for UI binding)
- **FR-009**: `Model_DunnageSession. HasLoads` MUST return `Loads. Count > 0` as a computed property
- **FR-010**: All date properties MUST default to `DateTime.Now` for new instances

**Status**: Partially Implemented. `[ObservableProperty]` used. Dictionary properties and `Model_DunnageSession` pending. Default dates pending.

### Functional Requirements - DAOs

#### Dao_DunnageType (8 Methods)
**Status**: Implemented.

#### Dao_DunnageSpec (7 Methods)
**Status**: Implemented.

#### Dao_DunnagePart (8 Methods)
**Status**: Implemented.

#### Dao_DunnageLoad (6 Methods)
**Status**: Implemented.

#### Dao_InventoriedDunnage (6 Methods)
**Status**: Implemented.

### DAO Implementation Standards

- **FR-045**: All DAO classes MUST be static classes in `Data/Receiving/` namespace
- **FR-046**: All DAO methods MUST be async and return Task-wrapped results
- **FR-047**:  All DAO methods MUST use stored procedures (no inline SQL)
- **FR-048**: All DAO methods MUST catch exceptions and wrap in `Model_Dao_Result` with success=false
- **FR-049**: All DAO methods MUST use `MySqlConnection` from connection string configuration
- **FR-050**: All DAO methods MUST properly dispose database connections and commands (using statements)
- **FR-051**: JSON deserialization in models MUST handle null/empty strings gracefully (return empty dictionary)
- **FR-052**: Navigation properties (e.g., DunnageTypeName) MUST be populated via JOIN queries in stored procedures

## Success Criteria

### Measurable Outcomes - Models

- **SC-001**: All 6 model classes compile without errors and inherit from `ObservableObject`
- **SC-002**: All observable properties trigger `PropertyChanged` events when modified (verified by unit tests)
- **SC-003**: JSON deserialization properties correctly parse valid JSON and handle invalid JSON gracefully
- **SC-004**: `Model_DunnageSession. HasLoads` property correctly reflects collection state

### Measurable Outcomes - DAOs

- **SC-005**:  All 35 DAO methods compile and execute successfully against test database
- **SC-006**: All DAO methods return `Model_Dao_Result` with correct success/failure states
- **SC-007**: All DAO exception handling catches and wraps errors without throwing unhandled exceptions
- **SC-008**:  Batch insert operations complete 100 records in under 2 seconds
- **SC-009**: Search methods return filtered results matching search criteria
- **SC-010**: Impact analysis methods (count_parts, count_transactions) return accurate counts
- **SC-011**:  All database connections are properly disposed (no connection leaks)
- **SC-012**: All DAO methods work correctly with null optional parameters (filters, search text)

## Non-Functional Requirements

- **NFR-001**: Models MUST be partial classes to support future code-behind methods
- **NFR-002**:  DAO classes MUST log all database exceptions using `ILoggingService`
- **NFR-003**: DAO methods MUST complete within 500ms for single record operations
- **NFR-004**: DAO methods MUST complete within 2 seconds for batch operations (100 records)
- **NFR-005**: All code MUST follow project C# style guidelines (naming, formatting)
- **NFR-006**:  All public methods MUST have XML documentation comments

## Out of Scope

- ❌ Repository pattern (using static DAOs per project standards)
- ❌ Entity Framework or ORM (using stored procedures directly)
- ❌ Caching layer (service layer responsibility)
- ❌ Optimistic concurrency handling (future feature)
- ❌ Database transaction management in DAOs (stored procedures handle atomicity)
- ❌ Audit trail logging of changes (future feature)

## Dependencies

- 004-database-foundation (tables must exist)
- 002-stored-procedures (all SPs must be created)
- NuGet:  `CommunityToolkit.Mvvm` (for ObservableObject)
- NuGet: `MySql.Data` or `MySqlConnector` (for database access)
- NuGet: `System.Text.Json` (for JSON serialization)
- Project:  `Model_Dao_Result. cs` (existing result wrapper class)
- Project: `ILoggingService` (for error logging)

## Files to be Created

### Models (`Models/Receiving/`)
- `Model_DunnageType.cs` (6 properties)
- `Model_DunnageSpec.cs` (7 properties including Dictionary)
- `Model_DunnagePart.cs` (9 properties including Dictionary and navigation)
- `Model_DunnageLoad.cs` (13 properties including navigation and selection)
- `Model_InventoriedDunnage.cs` (7 properties)
- `Model_DunnageSession.cs` (8 properties including ObservableCollection and computed)

### DAOs (`Data/Receiving/`)
- `Dao_DunnageType.cs` (8 methods)
- `Dao_DunnageSpec.cs` (7 methods)
- `Dao_DunnagePart.cs` (8 methods)
- `Dao_DunnageLoad.cs` (6 methods)
- `Dao_InventoriedDunnage.cs` (6 methods)

## Review & Acceptance Checklist

### Requirement Completeness
- [x] All 6 model classes are defined with complete property sets
- [x] All 5 DAO classes are defined with all required methods (35 total)
- [x] JSON handling strategy is clearly specified (string + Dictionary properties)
- [x] Navigation property population strategy is defined (JOIN in SPs)
- [x] Error handling strategy is explicit (wrap exceptions in Model_Dao_Result)

### Clarity & Unambiguity
- [x] Model inheritance hierarchy is clear (all inherit ObservableObject)
- [x] DAO return types are consistent (Model_Dao_Result wrapping)
- [x] Async/await pattern is specified for all DAO methods
- [x] Stored procedure calling convention is defined
- [x] Connection management pattern is specified (using statements)

### Testability
- [x] Model testing approach is defined (PropertyChanged events)
- [x] DAO testing approach is defined (direct execution against test DB)
- [x] Success criteria are measurable (compilation, execution time, accuracy)
- [x] Edge cases define expected error handling behavior

### Architecture Alignment
- [x] Follows project DAO pattern (static classes, Model_Dao_Result)
- [x] Follows project MVVM pattern (ObservableObject, ObservableProperty)
- [x] Follows project error handling pattern (ILoggingService)
- [x] No deviation from established patterns

## Clarifications

### Resolved Questions

**Q1**: Should models implement INotifyPropertyChanged manually or use ObservableObject?   
**A1**: Use CommunityToolkit.Mvvm's `ObservableObject` base class with `[ObservableProperty]` attributes.  Matches project standards.

**Q2**: Should DAOs use instance methods or static methods?  
**A2**: Static methods.  Matches existing project DAO pattern (`Dao_Material`, `Dao_Receiving`, etc.).

**Q3**: Should we implement repository pattern?   
**A3**: No.  Project uses static DAOs directly. No abstraction layer needed.

**Q4**: How should JSON deserialization be handled?  
**A4**:  Provide both string property (database) and Dictionary property (UI). Deserialize on-demand with null/error handling.

**Q5**: Should Model_DunnageLoad have IsDeleted column?  
**A5**: No. Per resolved Q&A, using hard deletes only.  No IsDeleted property needed.

**Q6**: Should DAOs manage database transactions?  
**A6**: No.  Stored procedures handle atomicity. DAOs only execute single SP calls.  Batch SPs use transactions internally. 
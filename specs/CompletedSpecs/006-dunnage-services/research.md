# Research: Dunnage Services Layer

**Feature**: Dunnage Services Layer  
**Date**: 2025-12-26  
**Phase**: Phase 0 - Research & Decision Log

## Technology Decisions

### Decision 1: CSV Library Selection
**Context**: Need to export dunnage loads to CSV format with RFC 4180 compliance (proper escaping of quotes, commas, newlines).

**Decision**: Use `CsvHelper` library  
**Rationale**:
- Already used successfully in project (`Service_CSVWriter` for receiving loads)
- Robust RFC 4180 compliance with automatic escaping
- High performance with low memory footprint
- Supports both dynamic and strongly-typed writing
- Well-maintained with extensive documentation

**Alternatives Considered**:
- **Manual StreamWriter**: Risk of escaping errors, requires custom RFC 4180 implementation
- **StringBuilder**: Efficient but still requires manual escaping logic
- **System.Text.Json + conversion**: Overkill for CSV format, JSON != CSV

**Impact**: Low - Library already referenced in project

---

### Decision 2: JSON Validation Library
**Context**: Need to validate that spec schema strings stored in database are valid JSON before insert/update operations.

**Decision**: Use `System.Text.Json`  
**Rationale**:
- Built-in .NET 8 library (no additional dependencies)
- High performance serializer/deserializer
- Simple validation via `JsonDocument.Parse()` try-catch
- Future-proof for JSON manipulation if needed

**Alternatives Considered**:
- **Newtonsoft.Json**: Extra dependency, slower than System.Text.Json in .NET 8
- **Custom regex validation**: Fragile, won't catch all JSON syntax errors
- **No validation**: Risk of invalid JSON in database

**Impact**: None - Already referenced in project

---

### Decision 3: Workflow State Management Pattern
**Context**: Wizard workflow requires maintaining state across multiple ViewModel navigations.

**Decision**: Singleton Service (`Service_DunnageWorkflow`)  
**Rationale**:
- Persists state across page navigations
- Consistent with existing `Service_ReceivingWorkflow` pattern
- Simplifies ViewModel code (no manual state passing)
- Enables centralized validation logic for step transitions

**Alternatives Considered**:
- **Transient with manual state passing**: Complex, error-prone, requires ViewModels to manage state
- **Static class**: Anti-pattern for DI, not testable
- **ViewModel-based state**: Couples state to UI lifecycle, lost on navigation

**Impact**: Medium - Singleton lifecycle requires careful thread safety consideration (though WinUI is single-threaded UI)

---

### Decision 4: DI Registration Strategy
**Context**: Three services with different lifecycle needs.

**Decision**:
- `IService_DunnageWorkflow` → **Singleton** (maintains wizard state)
- `IService_MySQL_Dunnage` → **Transient** (stateless DAO wrapper)
- `IService_DunnageCSVWriter` → **Transient** (stateless export)

**Rationale**:
- Workflow must persist state → Singleton
- Database and CSV services are stateless → Transient (new instance per use)
- Follows Microsoft DI best practices
- Consistent with existing project patterns

**Alternatives Considered**:
- All singletons: Wastes memory for stateless services
- All transient: Loses workflow state across navigations

**Impact**: Low - Standard DI pattern

---

### Decision 5: Model Naming Convention
**Context**: Need result models for service operations (WorkflowStepResult, SaveResult, CSVWriteResult).

**Decision**: Prefix with `Model_` (e.g., `Model_WorkflowStepResult`)  
**Rationale**:
- Consistent with project naming convention (Model_DunnageType, Model_DunnageLoad, etc.)
- Clear namespace separation from service classes
- IDE autocomplete groups all models together

**Alternatives Considered**:
- Suffix with `Model`: Breaks existing convention
- No prefix: Risk of naming conflicts

**Impact**: None - Follows existing pattern

---

### Decision 6: Workflow Step Enum Naming
**Context**: Need enum for wizard steps (ModeSelection, TypeSelection, etc.).

**Decision**: `Enum_DunnageWorkflowStep` (placed in `Models/Enums/`)  
**Rationale**:
- Consistent with project enum naming (`Enum_ErrorSeverity`, `Enum_LabelType`, etc.)
- Prefix makes type clear in usage
- Centralized enums folder for discoverability

**Alternatives Considered**:
- `DunnageWorkflowStep` (no prefix): Breaks convention
- Nested in service class: Reduces reusability

**Impact**: None - Follows existing pattern

---

## Unknowns & Clarifications

### Resolved Questions

**Q1**: Should `Service_DunnageCSVWriter` reuse `Service_CSVWriter` or be a separate implementation?  
**A1**: Separate implementation. Dunnage CSV has unique requirements:
- Dynamic columns based on spec keys (requires `GetAllSpecKeysAsync()` from MySQL service)
- Different header structure (DunnageType, PartID vs. LoadID, PackageID)
- File naming convention (`DunnageData.csv` vs. `ReceivingData.csv`)

**Q2**: How should GetAllSpecKeysAsync() aggregate spec keys from multiple types?  
**A2**: Union operation - return all unique spec keys across all types, sorted alphabetically. DAOs will query all specs, service will extract JSON keys and deduplicate.

**Q3**: Should CSV service validate loads before writing?  
**A3**: Yes - Fail fast if loads list is null or empty. Return error result instead of creating empty CSV.

**Q4**: Should workflow service auto-advance steps or wait for ViewModel to call Advance?  
**A4**: Wait for ViewModel. Service provides navigation methods, ViewModel decides when to call them based on user actions (button clicks, etc.).

**Q5**: Should services cache data (e.g., types, parts)?  
**A5**: No caching in service layer. Services are thin wrappers around DAOs. If caching is needed later, it can be added as a separate caching service.

---

## Dependencies Verified

- [x] `CsvHelper` - Already in project (Service_CSVWriter)
- [x] `System.Text.Json` - Built-in .NET 8
- [x] `ILoggingService` - Existing in project
- [x] `IService_ErrorHandler` - Existing in project
- [x] `IService_UserSessionManager` - Existing in project (for user auditing)
- [x] All Dunnage DAOs from spec 005 - Depends on completion

---

## Architecture Patterns

### Service Layer Responsibilities
1. **Validation**: Business rule validation before DAO calls
2. **Orchestration**: Coordinating multiple DAO calls (e.g., SaveSession → SaveLoads + ExportCSV)
3. **Error Handling**: Wrapping DAO errors with user-friendly messages
4. **Logging**: Audit trail for all operations
5. **State Management**: Workflow state only (no data caching)

### NOT Service Layer Responsibilities
- Database constraint enforcement (DAOs handle this)
- Complex business logic (keep services thin)
- UI concerns (ViewModels handle presentation logic)
- Transaction management (DAOs manage MySQL transactions)

---

## Performance Considerations

- CSV write to network share may take >1 second → Local write MUST succeed even if network fails
- GetAllSpecKeysAsync() could be expensive with many types → Acceptable, called infrequently
- Workflow state in memory → Minimal footprint (one session object)

---

## Risk Mitigation

**Risk**: CSV network write failure crashes application  
**Mitigation**: Try-catch with logging, local write always succeeds

**Risk**: JSON validation allows malformed schemas  
**Mitigation**: System.Text.Json parse throws on invalid JSON

**Risk**: Workflow state lost on app crash  
**Mitigation**: Acceptable - wizard sessions are ephemeral, user can restart

---

## Next Steps (Phase 1)

1. Create data-model.md with all result models
2. Create interface contracts in contracts/ folder
3. Create quickstart.md with usage examples
4. Update agent context with new technology choices

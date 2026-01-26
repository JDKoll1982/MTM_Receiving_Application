# Phase 3: CQRS Handlers & Validators - Task List

**Phase:** 3 of 8  
**Status:** ✅ COMPLETE  
**Priority:** HIGH - Blocks ViewModels (Phase 4-5)  
**Dependencies:** Phase 2 (DAOs) must be complete

---

## 📊 **Phase 3 Overview**

**Goal:** Implement all CQRS Command Handlers, Query Handlers, and FluentValidation Validators

**Status:**
- ✅ Commands: 7/7 defined ✅ COMPLETE
- ✅ Queries: 7/7 defined ✅ COMPLETE
- ✅ Validators: 6/6 complete ✅ COMPLETE
- ✅ Command Handlers: 7/7 implemented ✅ COMPLETE
- ✅ Query Handlers: 7/7 implemented ✅ COMPLETE
- ✅ DI Registration: COMPLETE

**Completion:** 34/34 tasks (100%) ✅

**Completed:** 2026-01-25

---

## ✅ **ALL TASKS COMPLETE**

### DI Registration ✅ NEW 2026-01-25
- [x] **Critical:** Module_Receiving DAOs registered in `ModuleServicesExtensions.cs`
- [x] All 6 DAOs injected with IService_LoggingUtility dependency
- [x] MediatR and FluentValidation already globally registered via `CqrsInfrastructureExtensions.cs`

### All CQRS Artifacts Renamed to 5-Part Convention ✅ 2026-01-25
- [x] Commands: `CommandRequest_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- [x] Queries: `QueryRequest_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- [x] Handlers: `CommandHandler_` / `QueryHandler_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`
- [x] Validators: `Validator_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`

### Commands & Handlers (7/7) ✅
1. ✅ CommandRequest_Receiving_Shared_Save_Transaction + Handler
2. ✅ CommandRequest_Receiving_Shared_Save_WorkflowSession + Handler
3. ✅ CommandRequest_Receiving_Shared_Update_ReceivingLine + Handler
4. ✅ CommandRequest_Receiving_Shared_Delete_ReceivingLine + Handler
5. ✅ CommandRequest_Receiving_Wizard_Copy_FieldsToEmptyLoads + Handler
6. ✅ CommandRequest_Receiving_Wizard_Clear_AutoFilledFields + Handler
7. ✅ CommandRequest_Receiving_Shared_Complete_Workflow + Handler

### Queries & Handlers (7/7) ✅
1. ✅ QueryRequest_Receiving_Shared_Get_WorkflowSession + Handler
2. ✅ QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO + Handler
3. ✅ QueryRequest_Receiving_Shared_Get_ReceivingTransactionById + Handler
4. ✅ QueryRequest_Receiving_Shared_Get_PartDetails + Handler
5. ✅ QueryRequest_Receiving_Shared_Validate_PONumber + Handler
6. ✅ QueryRequest_Receiving_Shared_Get_ReferenceData + Handler
7. ✅ QueryRequest_Receiving_Shared_Search_Transactions + Handler

### Validators (6/6) ✅
1. ✅ Validator_Receiving_Shared_ValidateOn_SaveTransaction
2. ✅ Validator_Receiving_Shared_ValidateOn_SaveWorkflow
3. ✅ Validator_Receiving_Shared_ValidateOn_UpdateReceivingLine
4. ✅ Validator_Receiving_Shared_ValidateIf_ValidPOFormat
5. ✅ Validator_Receiving_Shared_ValidateIf_POExists
6. ✅ Validator_Receiving_Wizard_ValidateOn_CopyFieldsToEmptyLoads

---

## 🎯 **CQRS Architecture Pattern**

**ViewModels use MediatR, NOT Services directly:**

```csharp
// ❌ WRONG - Direct service call
public class ViewModel_Example
{
    private readonly IService_Receiving _service;
    public async Task LoadAsync()
    {
        var result = await _service.GetDataAsync();
    }
}

// ✅ CORRECT - CQRS with MediatR
public class ViewModel_Example
{
    private readonly IMediator _mediator;
    public async Task LoadAsync()
    {
        var query = new GetReceivingLinesByPOQuery { PONumber = "PO-123" };
        var result = await _mediator.Send(query);
    }
}
```

**Benefits:**
- ✅ Single Responsibility Principle
- ✅ Automatic validation via pipeline behaviors
- ✅ Automatic logging via pipeline behaviors
- ✅ Easy to test (mock IMediator)
- ✅ Decoupled from DAOs

---

## ✅ **Completed Tasks (12/34) - Updated 2026-01-25**

### Commands Defined & Implemented (5/7)
- [x] **Task 3.1:** `SaveReceivingTransactionCommand` - Save complete transaction ✅ Pre-existing
- [x] **Task 3.2:** `SaveWorkflowSessionCommand` - Persist session state ✅ Pre-existing
- [x] **Task 3.6:** `UpdateReceivingLineCommand` - Update existing line ✅ NEW 2026-01-25
- [x] **Task 3.7:** `DeleteReceivingLineCommand` - Soft delete line ✅ NEW 2026-01-25
- [x] **Task 3.10:** `CompleteWorkflowCommand` - Finalize and archive ✅ NEW 2026-01-25

### Command Handlers Implemented (3/7)
- [x] **Task 3.1 Handler:** `UpdateReceivingLineCommandHandler` ✅ NEW 2026-01-25
- [x] **Task 3.2 Handler:** `DeleteReceivingLineCommandHandler` ✅ NEW 2026-01-25
- [x] **Task 3.3 Handler:** `CompleteWorkflowCommandHandler` ✅ NEW 2026-01-25

### Queries Defined & Implemented (3/7)
- [x] **Task 3.3:** `GetWorkflowSessionQuery` - Retrieve session state ✅ Pre-existing
- [x] **Task 3.11:** `GetReceivingLinesByPOQuery` - Get lines by PO ✅ NEW 2026-01-25
- [x] **Task 3.12:** `GetReceivingTransactionByIdQuery` - Get transaction ✅ NEW 2026-01-25

### Query Handlers Implemented (2/7)
- [x] **Task 3.4 Handler:** `GetReceivingLinesByPOQueryHandler` ✅ NEW 2026-01-25
- [x] **Task 3.5 Handler:** `GetReceivingTransactionByIdQueryHandler` ✅ NEW 2026-01-25

### Validators (4/10)
- [x] **Task 3.4:** `SaveReceivingTransactionCommandValidator` ✅ Pre-existing
- [x] **Task 3.5:** `SaveWorkflowSessionCommandValidator` ✅ Pre-existing
- [x] **Task 3.6 Validator:** `UpdateReceivingLineCommandValidator` ✅ NEW 2026-01-25
- [x] **Task 3.7 Validator:** `GetReceivingLinesByPOQueryValidator` ✅ NEW 2026-01-25

---

## ⏳ **Command Implementation Tasks (5 remaining)**

**Location:** `Module_Receiving/Requests/Commands/` & `Module_Receiving/Handlers/Commands/`

---

### Task 3.6: UpdateReceivingLineCommand & Handler

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Requests/Commands/UpdateReceivingLineCommand.cs`
- `Module_Receiving/Handlers/Commands/UpdateReceivingLineCommandHandler.cs`
- `Module_Receiving/Validators/UpdateReceivingLineCommandValidator.cs`

**Estimated Time:** 2 hours

**Command Definition:**
```csharp
public class UpdateReceivingLineCommand : IRequest<Model_Dao_Result>
{
    public string LineId { get; set; } = string.Empty;
    public string? PONumber { get; set; }
    public string? PartNumber { get; set; }
    public int? LoadNumber { get; set; }
    public int? Quantity { get; set; }
    public decimal? Weight { get; set; }
    public string? HeatLot { get; set; }
    public string? PackageType { get; set; }
    public int? PackagesPerLoad { get; set; }
    public decimal? WeightPerPackage { get; set; }
    public string? ReceivingLocation { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
}
```

**Handler Logic:**
```csharp
public class UpdateReceivingLineCommandHandler : IRequestHandler<UpdateReceivingLineCommand, Model_Dao_Result>
{
    private readonly Dao_Receiving_Repository_Line _dao;
    private readonly IService_LoggingUtility _logger;
    
    public async Task<Model_Dao_Result> Handle(UpdateReceivingLineCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInfo($"Updating line {request.LineId}");
        
        var line = MapCommanDataTransferObjectsEntity(request);
        var result = await _dao.UpdateLineAsync(line);
        
        if (result.IsSuccess)
            _logger.LogInfo($"Line {request.LineId} updated successfully");
        else
            _logger.LogError($"Failed to update line {request.LineId}: {result.ErrorMessage}");
        
        return result;
    }
}
```

**Validator Rules:**
```csharp
public class UpdateReceivingLineCommandValidator : AbstractValidator<UpdateReceivingLineCommand>
{
    public UpdateReceivingLineCommandValidator()
    {
        RuleFor(x => x.LineId).NotEmpty().WithMessage("LineId is required");
        RuleFor(x => x.ModifiedBy).NotEmpty().WithMessage("ModifiedBy is required");
        
        When(x => x.Quantity.HasValue, () =>
        {
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be positive");
        });
        
        When(x => x.Weight.HasValue, () =>
        {
            RuleFor(x => x.Weight).GreaterThan(0).WithMessage("Weight must be positive");
        });
    }
}
```

**Acceptance Criteria:**
- [ ] Command class created with all properties
- [ ] Handler implemented calling DAO
- [ ] Validator created with business rules
- [ ] Logging for success/failure
- [ ] Returns `Model_Dao_Result`

---

### Task 3.7: DeleteReceivingLineCommand & Handler

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Requests/Commands/DeleteReceivingLineCommand.cs`
- `Module_Receiving/Handlers/Commands/DeleteReceivingLineCommandHandler.cs`

**Estimated Time:** 1 hour

**Command Definition:**
```csharp
public class DeleteReceivingLineCommand : IRequest<Model_Dao_Result>
{
    public string LineId { get; set; } = string.Empty;
    public string ModifiedBy { get; set; } = string.Empty;
    public string DeletionReason { get; set; } = string.Empty;
}
```

**Handler:** Calls `Dao_Receiving_Repository_Line.DeleteAsync` (soft delete)

**Acceptance Criteria:**
- [ ] Command class created
- [ ] Handler implemented (soft delete via DAO)
- [ ] Audit log entry created
- [ ] Returns `Model_Dao_Result`

---

### Task 3.8: BulkCopyFieldsCommand & Handler ✅ COMPLETE 2026-01-25

**Priority:** P1 - HIGH  
**Files:**
- ✅ `Module_Receiving/Requests/Commands/BulkCopyFieldsCommand.cs`
- ✅ `Module_Receiving/Handlers/Commands/BulkCopyFieldsCommandHandler.cs`
- ✅ `Module_Receiving/Validators/BulkCopyFieldsCommandValidator.cs`

**Completed:** 2026-01-25 Batch 2

**Acceptance Criteria:**
- [x] Command supports multiple field types (via Enum_Receiving_Type_CopyFieldSelection)
- [x] Handler implements "copy to empty only" logic
- [x] Returns count of updated loads
- [x] Validator enforces business rules

**Implementation Notes:**
- Supports: HeatLotOnly, PackageTypeOnly, PackagesPerLoadOnly, WeightQuantityOnly, AllFields
- Iterates through all loads except source, only updates empty fields
- Returns count of successfully updated loads

---

### Task 3.9: ClearAutoFilledFieldsCommand & Handler ✅ COMPLETE 2026-01-25

**Priority:** P1 - HIGH  
**Files:**
- ✅ `Module_Receiving/Requests/Commands/ClearAutoFilledFieldsCommand.cs`
- ✅ `Module_Receiving/Handlers/Commands/ClearAutoFilledFieldsCommandHandler.cs`

**Completed:** 2026-01-25 Batch 2

**Acceptance Criteria:**
- [x] Clears specific load or all loads (via optional TargetLoadNumber)
- [x] Clears: HeatLot, PackageType, PackagesPerLoad, ReceivingLocation
- [x] Resets to null/defaults
- [x] Returns count of cleared loads

**Implementation Notes:**
- Supports targeted clear (single load) or full clear (all loads)
- Resets PackagesPerLoad to 1 (default) instead of null

---

### Task 3.10: CompleteWorkflowCommand & Handler

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Requests/Commands/CompleteWorkflowCommand.cs`
- `Module_Receiving/Handlers/Commands/CompleteWorkflowCommandHandler.cs`

**Estimated Time:** 2 hours

**Command Definition:**
```csharp
public class CompleteWorkflowCommand : IRequest<Model_Dao_Result>
{
    public string TransactionId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string CSVFilePath { get; set; } = string.Empty;
    public string CompletedBy { get; set; } = string.Empty;
}
```

**Handler Logic:**
1. Complete transaction via `Dao_Receiving_Repository_Transaction.CompleteAsync`
2. Update session status to "Completed" via `Dao_Receiving_Repository_WorkflowSession.UpdateSessionAsync`
3. Archive transaction to `tbl_Receiving_CompletedTransaction`
4. Create audit log entry
5. Return success/failure

**Acceptance Criteria:**
- [ ] Transaction marked as completed
- [ ] Session marked as completed
- [ ] Data archived to completed transactions table
- [ ] Audit trail created
- [ ] CSV file path saved

---

## ⏳ **Query Implementation Tasks (6 remaining)**

**Location:** `Module_Receiving/Requests/Queries/` & `Module_Receiving/Handlers/Queries/`

---

### Task 3.11: GetReceivingLinesByPOQuery & Handler

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Requests/Queries/GetReceivingLinesByPOQuery.cs`
- `Module_Receiving/Handlers/Queries/GetReceivingLinesByPOQueryHandler.cs`
- `Module_Receiving/Validators/GetReceivingLinesByPOQueryValidator.cs`

**Estimated Time:** 1.5 hours

**Query Definition:**
```csharp
public class GetReceivingLinesByPOQuery : IRequest<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLoad>>>
{
    public string PONumber { get; set; } = string.Empty;
}
```

**Handler:** Calls `Dao_Receiving_Repository_Line.SelectByPOAsync`

**Validator:**
```csharp
RuleFor(x => x.PONumber)
    .NotEmpty()
    .Matches(@"^[A-Z0-9-]+$").WithMessage("Invalid PO Number format");
```

**Acceptance Criteria:**
- [ ] Query class created
- [ ] Handler calls DAO
- [ ] Validator enforces PO format
- [ ] Returns list of lines

---

### Task 3.12: GetReceivingTransactionByIdQuery & Handler

**Priority:** P0 - CRITICAL  
**Files:**
- `Module_Receiving/Requests/Queries/GetReceivingTransactionByIdQuery.cs`
- `Module_Receiving/Handlers/Queries/GetReceivingTransactionByIdQueryHandler.cs`

**Estimated Time:** 1 hour

**Query Definition:**
```csharp
public class GetReceivingTransactionByIdQuery : IRequest<Model_Dao_Result<Model_Receiving_Entity_ReceivingTransaction>>
{
    public string TransactionId { get; set; } = string.Empty;
}
```

**Handler:** Calls `Dao_Receiving_Repository_Transaction.SelectByIdAsync`

**Acceptance Criteria:**
- [ ] Query class created
- [ ] Handler calls DAO
- [ ] Returns single transaction or not found

---

### Task 3.13: GetPartDetailsQuery & Handler

**Priority:** P1 - HIGH  
**Files:**
- `Module_Receiving/Requests/Queries/GetPartDetailsQuery.cs`
- `Module_Receiving/Handlers/Queries/GetPartDetailsQueryHandler.cs`

**Estimated Time:** 2 hours

**Query Definition:**
```csharp
public class GetPartDetailsQuery : IRequest<Model_Dao_Result<Model_Receiving_DataTransferObjects_PartDetails>>
{
    public string PartNumber { get; set; } = string.Empty;
    public string Scope { get; set; } = "System";
    public string? ScopeUserId { get; set; }
}
```

**Handler Logic:**
1. Get part preferences from `Dao_Receiving_Repository_PartPreference`
2. Get part type details from `Dao_Receiving_Repository_Reference`
3. Combine into DataTransferObjects with defaults
4. Return enriched part details

**Acceptance Criteria:**
- [ ] Retrieves part preferences (User scope first, then System)
- [ ] Includes part type details
- [ ] Returns default values if no preferences found
- [ ] DataTransferObjects includes all fields needed for UI

---

### Task 3.14: SearchTransactionsQuery & Handler

**Priority:** P2 - MEDIUM  
**Files:**
- `Module_Receiving/Requests/Queries/SearchTransactionsQuery.cs`
- `Module_Receiving/Handlers/Queries/SearchTransactionsQueryHandler.cs`
- `Module_Receiving/Validators/SearchTransactionsQueryValidator.cs`

**Estimated Time:** 2 hours

**Query Definition:**
```csharp
public class SearchTransactionsQuery : IRequest<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingTransaction>>>
{
    public string? PONumber { get; set; }
    public string? PartNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public bool IncludeDeleted { get; set; } = false;
}
```

**Handler Logic:**
1. Build dynamic query based on provided filters
2. Call appropriate DAO methods (SelectByPO, SelectByDateRange)
3. Filter results in-memory if needed
4. Return matching transactions

**Acceptance Criteria:**
- [ ] Supports multiple search criteria
- [ ] Date range filtering
- [ ] Status filtering
- [ ] Optional include deleted records

---

### Task 3.15: GetAuditLogQuery & Handler

**Priority:** P2 - MEDIUM  
**Files:**
- `Module_Receiving/Requests/Queries/GetAuditLogQuery.cs`
- `Module_Receiving/Handlers/Queries/GetAuditLogQueryHandler.cs`

**Estimated Time:** 1 hour

**Query Definition:**
```csharp
public class GetAuditLogQuery : IRequest<Model_Dao_Result<List<Model_Receiving_Entity_AuditLog>>>
{
    public string TransactionId { get; set; } = string.Empty;
}
```

**Handler:** Calls `Dao_Receiving_Repository_Audit.SelectByTransactionAsync` (if DAO exists)

**Acceptance Criteria:**
- [ ] Retrieves complete audit trail
- [ ] Ordered by date descending
- [ ] Includes all change details

---

### Task 3.16: ValidatePONumberQuery & Handler

**Priority:** P1 - HIGH  
**Files:**
- `Module_Receiving/Requests/Queries/ValidatePONumberQuery.cs`
- `Module_Receiving/Handlers/Queries/ValidatePONumberQueryHandler.cs`
- `Module_Receiving/Validators/ValidatePONumberQueryValidator.cs`

**Estimated Time:** 2 hours

**Query Definition:**
```csharp
public class ValidatePONumberQuery : IRequest<Model_Dao_Result<Model_Receiving_DataTransferObjects_POValidationResult>>
{
    public string PONumber { get; set; } = string.Empty;
}
```

**DataTransferObjects:**
```csharp
public class Model_Receiving_DataTransferObjects_POValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ExistsInERP { get; set; }
    public bool ExistsInLocalDatabase { get; set; }
    public string? NormalizedPONumber { get; set; } // After auto-standardization
}
```

**Handler Logic:**
1. Validate PO format (regex: `^[A-Z0-9-]+$`)
2. Auto-standardize (uppercase, trim)
3. Check if PO exists in local database
4. Optionally check ERP (if integration enabled)
5. Return validation result

**Acceptance Criteria:**
- [ ] Format validation
- [ ] Auto-standardization
- [ ] Local database check
- [ ] ERP check (if enabled)
- [ ] Clear error messages

---

## ⏳ **Validator Implementation Tasks (8 remaining)**

All validators use **FluentValidation** library.

**Pattern:**
```csharp
public class MyCommandValidator : AbstractValidator<MyCommand>
{
    public MyCommandValidator()
    {
        RuleFor(x => x.PropertyName)
            .NotEmpty().WithMessage("PropertyName is required")
            .MaximumLength(50).WithMessage("PropertyName must be 50 characters or less");
    }
}
```

### Validators Needed:
- [ ] **Task 3.17:** `UpdateReceivingLineCommandValidator` (see Task 3.6)
- [ ] **Task 3.18:** `BulkCopyFieldsCommandValidator` (see Task 3.8)
- [ ] **Task 3.19:** `GetReceivingLinesByPOQueryValidator` (see Task 3.11)
- [ ] **Task 3.20:** `SearchTransactionsQueryValidator` (see Task 3.14)
- [ ] **Task 3.21:** `ValidatePONumberQueryValidator` (see Task 3.16)

---

## 🔌 **DI Registration Task**

### Task 3.22: Register Handlers & Validators

**Priority:** P0 - CRITICAL  
**File:** `App.xaml.cs`  
**Dependencies:** All handlers and validators complete  
**Estimated Time:** 30 minutes

**Registration Pattern:**
```csharp
// In ConfigureServices method
services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(App).Assembly);
});

services.AddValidatorsFromAssembly(typeof(App).Assembly);

// Add pipeline behaviors
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

**Acceptance Criteria:**
- [ ] MediatR registered with assembly scanning
- [ ] FluentValidation registered
- [ ] Pipeline behaviors configured
- [ ] All handlers auto-discovered
- [ ] All validators auto-discovered

---

## ✅ **Phase 3 Completion Criteria**

### Commands
- [ ] All 7 Command classes created
- [ ] All 7 Command Handlers implemented
- [ ] All handlers call appropriate DAOs
- [ ] All handlers return `Model_Dao_Result`

### Queries
- [ ] All 7 Query classes created
- [ ] All 7 Query Handlers implemented
- [ ] All handlers call appropriate DAOs
- [ ] All handlers return `Model_Dao_Result<T>`

### Validators
- [ ] All 10 Validators created
- [ ] All use FluentValidation library
- [ ] Business rules enforced
- [ ] Clear error messages

### Integration
- [ ] MediatR registered in DI
- [ ] Validators registered in DI
- [ ] Pipeline behaviors configured
- [ ] All tests passing

---

## 📝 **Notes & Guidelines**

### Handler Best Practices
1. **Thin Handlers:** Minimal logic, delegate to DAOs
2. **Logging:** Log start, success, and failure
3. **No Exceptions:** Return failure results, don't throw
4. **Async:** All methods async with `CancellationToken`
5. **Mapping:** Use helper methods to map between entities

### Validator Best Practices
1. **Early Validation:** Validate in pipeline before handler executes
2. **Clear Messages:** User-friendly error messages
3. **Conditional Rules:** Use `When()` for conditional validation
4. **Reusable Rules:** Extract common rules to shared class

### Pipeline Behaviors
```csharp
// ValidationBehavior - Runs before handler
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, ...)
    {
        // Validate request using FluentValidation
        // Return failure if validation fails
        // Continue to handler if valid
    }
}

// LoggingBehavior - Runs around handler
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, ...)
    {
        // Log request details
        // Execute handler
        // Log execution time
        // Log result
    }
}
```

---

## 🚀 **Next Steps After Phase 3**

Once all handlers and validators are complete:
1. **Phase 4:** Implement Wizard Mode ViewModels (12 files)
2. Update IMPLEMENTATION_PROGRESS.md
3. Begin ViewModel implementation using IMediator

---

**Total Phase 3 Tasks:** 34  
**Completed:** 5  
**Remaining:** 29  
**Estimated Time to Complete:** 20-24 hours

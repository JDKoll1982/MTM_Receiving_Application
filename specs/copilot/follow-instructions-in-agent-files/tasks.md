# Implementation Tasks: Receiving Workflow Consolidation

**Feature**: Receiving Workflow Consolidation  
**Branch**: `copilot/follow-instructions-in-agent-files`  
**Date**: 2026-01-24  
**Status**: Ready for Implementation

## Task Organization

Tasks are organized by implementation phase. Each task includes:
- **ID**: Unique identifier
- **Title**: Brief description
- **Phase**: Implementation phase
- **Dependencies**: Tasks that must be completed first
- **Acceptance Criteria**: How to verify completion
- **Files**: Files to create or modify

---

## Phase 0: Project Setup (Foundation)

### TASK-001: Create Database Tables and Stored Procedures

**Dependencies**: None  
**Estimated Effort**: 2 hours  
**Priority**: P0 (Critical - blocks all other work)

**Objective**: Create MySQL database schema for receiving workflow session management

**Files to Create**:
- `Database/Migrations/002-receiving-workflow-consolidation.sql`
- `Database/StoredProcedures/sp_Create_Receiving_Session.sql`
- `Database/StoredProcedures/sp_Update_Load_Detail.sql`
- `Database/StoredProcedures/sp_Get_Session_With_Loads.sql`
- `Database/StoredProcedures/sp_Save_Completed_Transaction.sql`
- `Database/StoredProcedures/sp_Copy_To_Loads.sql`
- `Database/StoredProcedures/sp_Clear_AutoFilled_Data.sql`

**Acceptance Criteria**:
- [X] Tables created: `receiving_workflow_sessions`, `receiving_load_details`, `receiving_completed_transactions`
- [X] All 6 stored procedures created and tested
- [X] Foreign key constraints in place
- [X] Indexes created on key columns
- [ ] Test data can be inserted and retrieved
- [ ] Migration script executes without errors

**SQL Template**:
```sql
-- See data-model.md for complete schema
CREATE TABLE receiving_workflow_sessions (
    session_id CHAR(36) PRIMARY KEY,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    -- ... (see data-model.md for complete definition)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Validation**:
```bash
mysql -u root -p mtm_receiving_application < Database/Migrations/002-receiving-workflow-consolidation.sql
mysql -u root -p mtm_receiving_application -e "SHOW TABLES;"
```

---

### TASK-002: Register MediatR and Pipeline Behaviors in DI

**Dependencies**: None  
**Estimated Effort**: 1 hour  
**Priority**: P0 (Critical)

**Objective**: Configure MediatR with FluentValidation pipeline behaviors in App.xaml.cs

**Files to Modify**:
- `App.xaml.cs`

**Files to Create**:
- `Module_Receiving/Behaviors/ValidationBehavior.cs`
- `Module_Receiving/Behaviors/LoggingBehavior.cs`
- `Module_Receiving/Behaviors/TransactionBehavior.cs`

**Acceptance Criteria**:
- [X] MediatR registered in DI container (already exists via AddCqrsInfrastructure)
- [X] ValidationBehavior registered (Module_Core/Behaviors/ValidationBehavior.cs)
- [X] LoggingBehavior registered (Module_Core/Behaviors/LoggingBehavior.cs)
- [X] AuditBehavior registered (Module_Core/Behaviors/AuditBehavior.cs)
- [X] Assembly scanning configured for handlers and validators
- [X] Application starts without DI errors

**Note**: CQRS infrastructure already configured via CqrsInfrastructureExtensions.cs. No changes needed.

**Implementation**:
```csharp
// In App.xaml.cs ConfigureServices()
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(App).Assembly);
});

services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

services.AddValidatorsFromAssembly(typeof(App).Assembly);
```

---

## Phase 1: Domain Models and CQRS Infrastructure (Foundation)

### TASK-003: Create Domain Models

**Dependencies**: TASK-001  
**Estimated Effort**: 2 hours  
**Priority**: P0 (Critical)

**Objective**: Implement all domain models from data-model.md

**Files to Create**:
- `Module_Receiving/Models/ReceivingWorkflowSession.cs`
- `Module_Receiving/Models/LoadDetail.cs`
- `Module_Receiving/Models/ValidationError.cs`
- `Module_Receiving/Models/CopyOperationResult.cs`
- `Module_Receiving/Models/CopyPreview.cs`
- `Module_Receiving/Models/SaveResult.cs`
- `Module_Receiving/Models/Enums/CopyFields.cs`
- `Module_Receiving/Models/Enums/ErrorSeverity.cs`

**Acceptance Criteria**:
- [X] All models created with properties matching data-model.md
- [X] XML documentation comments on public properties
- [X] Records used for immutable data (SaveResult)
- [X] Classes used for mutable entities (ReceivingWorkflowSession, LoadDetail)
- [X] Enums defined for CopyFields and ErrorSeverity
- [X] Compiles without errors

**Example**:
```csharp
// Module_Receiving/Models/LoadDetail.cs
public class LoadDetail
{
    public int LoadNumber { get; init; }
    public decimal? WeightOrQuantity { get; set; }
    public string HeatLot { get; set; }
    public string PackageType { get; set; }
    public int? PackagesPerLoad { get; set; }
    
    // Auto-fill tracking
    public bool IsWeightAutoFilled { get; set; }
    public bool IsHeatLotAutoFilled { get; set; }
    public bool IsPackageTypeAutoFilled { get; set; }
    public bool IsPackagesPerLoadAutoFilled { get; set; }
    
    // Validation
    public List<string> ValidationErrors { get; init; } = new();
    public bool IsValid => ValidationErrors.Count == 0;
}
```

---

### TASK-004: Create Commands and Queries (CQRS)

**Dependencies**: TASK-003  
**Estimated Effort**: 3 hours  
**Priority**: P0 (Critical)

**Objective**: Implement all MediatR commands and queries from contracts/mediatr-contracts.md

**Files to Create**:
- `Module_Receiving/Commands/StartWorkflowCommand.cs`
- `Module_Receiving/Commands/UpdateStep1Command.cs`
- `Module_Receiving/Commands/UpdateLoadDetailCommand.cs`
- `Module_Receiving/Commands/CopyToLoadsCommand.cs`
- `Module_Receiving/Commands/ClearAutoFilledDataCommand.cs`
- `Module_Receiving/Commands/ForceOverwriteCommand.cs`
- `Module_Receiving/Commands/ChangeCopySourceCommand.cs`
- `Module_Receiving/Commands/NavigateToStepCommand.cs`
- `Module_Receiving/Commands/SaveWorkflowCommand.cs`
- `Module_Receiving/Queries/GetSessionQuery.cs`
- `Module_Receiving/Queries/GetLoadDetailsQuery.cs`
- `Module_Receiving/Queries/GetValidationStatusQuery.cs`
- `Module_Receiving/Queries/GetCopyPreviewQuery.cs`
- `Module_Receiving/Queries/GetPartLookupQuery.cs`

**Acceptance Criteria**:
- [ ] All commands implement `IRequest<Result>` or `IRequest<Result<T>>`
- [ ] All queries implement `IRequest<Result<T>>`
- [ ] Records used for immutability
- [ ] XML documentation on each command/query
- [ ] Compiles without errors

**Example**:
```csharp
// Module_Receiving/Commands/UpdateStep1Command.cs
/// <summary>
/// Updates Step 1 data (PO Number, Part, Load Count)
/// </summary>
public record UpdateStep1Command(
    Guid SessionId,
    string PONumber,
    int PartId,
    int LoadCount
) : IRequest<Result>;
```

---

### TASK-005: Create FluentValidation Validators

**Dependencies**: TASK-004  
**Estimated Effort**: 3 hours  
**Priority**: P0 (Critical)

**Objective**: Implement validators for all commands

**Files to Create**:
- `Module_Receiving/Validators/StartWorkflowCommandValidator.cs`
- `Module_Receiving/Validators/UpdateStep1CommandValidator.cs`
- `Module_Receiving/Validators/UpdateLoadDetailCommandValidator.cs`
- `Module_Receiving/Validators/CopyToLoadsCommandValidator.cs`
- `Module_Receiving/Validators/ClearAutoFilledDataCommandValidator.cs`
- `Module_Receiving/Validators/SaveWorkflowCommandValidator.cs`

**Acceptance Criteria**:
- [ ] All validators inherit from `AbstractValidator<TCommand>`
- [ ] Validation rules match contracts/mediatr-contracts.md
- [ ] Error messages are clear and user-friendly
- [ ] Validators are discovered by assembly scanning
- [ ] Unit tests created for all validators

**Example**:
```csharp
// Module_Receiving/Validators/UpdateStep1CommandValidator.cs
public class UpdateStep1CommandValidator : AbstractValidator<UpdateStep1Command>
{
    public UpdateStep1CommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required");
            
        RuleFor(x => x.PONumber)
            .NotEmpty()
            .MaximumLength(50)
            .Matches(@"^[A-Za-z0-9\-]+$")
            .WithMessage("PO Number must be alphanumeric with hyphens only");
            
        RuleFor(x => x.LoadCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Load count must be between 1 and 100");
    }
}
```

---

## Phase 2: Data Access Layer (DAOs)

### TASK-006: Create DAO for Receiving Workflow Sessions

**Dependencies**: TASK-001, TASK-003  
**Estimated Effort**: 3 hours  
**Priority**: P1

**Objective**: Implement instance-based DAO for session CRUD operations

**Files to Create**:
- `Module_Receiving/Data/Dao_ReceivingWorkflowSession.cs`

**Acceptance Criteria**:
- [ ] Instance-based class (NOT static)
- [ ] Constructor accepts connection string
- [ ] All methods async with `Async` suffix
- [ ] Returns `Model_Dao_Result` or `Model_Dao_Result<T>`
- [ ] Uses stored procedures via `Helper_Database_StoredProcedure`
- [ ] NEVER throws exceptions (returns failure results)
- [ ] Registered as Singleton in App.xaml.cs
- [ ] Integration tests created

**Methods to Implement**:
```csharp
public class Dao_ReceivingWorkflowSession
{
    private readonly string _connectionString;
    
    public Dao_ReceivingWorkflowSession(string connectionString) { }
    
    public async Task<Model_Dao_Result<Guid>> CreateSessionAsync(ReceivingWorkflowSession session);
    public async Task<Model_Dao_Result<ReceivingWorkflowSession>> GetSessionAsync(Guid sessionId);
    public async Task<Model_Dao_Result> UpdateSessionAsync(ReceivingWorkflowSession session);
    public async Task<Model_Dao_Result> DeleteSessionAsync(Guid sessionId);
}
```

---

### TASK-007: Create DAO for Load Details

**Dependencies**: TASK-001, TASK-003  
**Estimated Effort**: 3 hours  
**Priority**: P1

**Objective**: Implement instance-based DAO for load detail CRUD operations

**Files to Create**:
- `Module_Receiving/Data/Dao_ReceivingLoadDetail.cs`

**Acceptance Criteria**:
- [ ] Instance-based class
- [ ] Uses stored procedures
- [ ] Returns `Model_Dao_Result` types
- [ ] Supports bulk operations (copy, clear auto-fill)
- [ ] Registered as Singleton
- [ ] Integration tests created

**Methods to Implement**:
```csharp
public class Dao_ReceivingLoadDetail
{
    public async Task<Model_Dao_Result> UpsertLoadDetailAsync(Guid sessionId, LoadDetail load);
    public async Task<Model_Dao_Result<List<LoadDetail>>> GetLoadsBySessionAsync(Guid sessionId);
    public async Task<Model_Dao_Result> CopyToLoadsAsync(Guid sessionId, int sourceLoad, List<int> targetLoads, CopyFields fields);
    public async Task<Model_Dao_Result> ClearAutoFilledAsync(Guid sessionId, List<int> loads, CopyFields fields);
    public async Task<Model_Dao_Result> SaveCompletedTransactionAsync(Guid sessionId, string csvPath, string user);
}
```

---

## Phase 3: Command/Query Handlers

### TASK-008: Implement Session Management Handlers

**Dependencies**: TASK-004, TASK-006  
**Estimated Effort**: 4 hours  
**Priority**: P1

**Objective**: Implement handlers for session lifecycle commands/queries

**Files to Create**:
- `Module_Receiving/Handlers/StartWorkflowCommandHandler.cs`
- `Module_Receiving/Handlers/NavigateToStepCommandHandler.cs`
- `Module_Receiving/Handlers/GetSessionQueryHandler.cs`

**Acceptance Criteria**:
- [ ] Handlers implement `IRequestHandler<TRequest, TResponse>`
- [ ] Inject DAOs via constructor
- [ ] Use async/await properly
- [ ] Return `Result` or `Result<T>`
- [ ] Log operations via Serilog
- [ ] Unit tests created with mocked DAOs

**Example**:
```csharp
public class StartWorkflowCommandHandler : IRequestHandler<StartWorkflowCommand, Result<Guid>>
{
    private readonly Dao_ReceivingWorkflowSession _dao;
    private readonly ILogger _logger;
    
    public async Task<Result<Guid>> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        _logger.Information("Starting workflow in {Mode} mode", request.Mode);
        
        var sessionId = Guid.NewGuid();
        var session = new ReceivingWorkflowSession 
        { 
            SessionId = sessionId,
            CurrentStep = 1
        };
        
        var result = await _dao.CreateSessionAsync(session);
        
        if (!result.IsSuccess)
            return Result<Guid>.Failure(result.ErrorMessage);
        
        return Result<Guid>.Success(sessionId);
    }
}
```

---

### TASK-009: Implement Step 1 Handlers

**Dependencies**: TASK-004, TASK-006  
**Estimated Effort**: 3 hours  
**Priority**: P1

**Objective**: Implement handlers for Step 1 commands/queries

**Files to Create**:
- `Module_Receiving/Handlers/UpdateStep1CommandHandler.cs`
- `Module_Receiving/Handlers/GetPartLookupQueryHandler.cs`

**Acceptance Criteria**:
- [ ] UpdateStep1CommandHandler updates session with PO/Part/LoadCount
- [ ] UpdateStep1CommandHandler initializes empty LoadDetail records
- [ ] GetPartLookupQueryHandler queries Infor Visual (read-only)
- [ ] Proper error handling and logging
- [ ] Unit tests created

---

### TASK-010: Implement Step 2 Load Detail Handlers

**Dependencies**: TASK-004, TASK-007  
**Estimated Effort**: 4 hours  
**Priority**: P1

**Objective**: Implement handlers for load detail operations

**Files to Create**:
- `Module_Receiving/Handlers/UpdateLoadDetailCommandHandler.cs`
- `Module_Receiving/Handlers/GetLoadDetailsQueryHandler.cs`
- `Module_Receiving/Handlers/GetValidationStatusQueryHandler.cs`

**Acceptance Criteria**:
- [ ] UpdateLoadDetailCommandHandler updates single load
- [ ] UpdateLoadDetailCommandHandler clears auto-fill flags on manual edit
- [ ] GetLoadDetailsQueryHandler returns all loads for session
- [ ] GetValidationStatusQueryHandler aggregates validation errors
- [ ] Unit tests created

---

### TASK-011: Implement Bulk Copy Handlers

**Dependencies**: TASK-004, TASK-007  
**Estimated Effort**: 5 hours  
**Priority**: P2

**Objective**: Implement handlers for bulk copy operations

**Files to Create**:
- `Module_Receiving/Handlers/CopyToLoadsCommandHandler.cs`
- `Module_Receiving/Handlers/GetCopyPreviewQueryHandler.cs`
- `Module_Receiving/Handlers/ChangeCopySourceCommandHandler.cs`

**Acceptance Criteria**:
- [ ] CopyToLoadsCommandHandler copies to empty cells only
- [ ] CopyToLoadsCommandHandler sets auto-fill flags
- [ ] CopyToLoadsCommandHandler returns CopyOperationResult with stats
- [ ] GetCopyPreviewQueryHandler shows what will be copied
- [ ] ChangeCopySourceCommandHandler updates session copy source
- [ ] Unit tests created with various scenarios

---

### TASK-012: Implement Clear Auto-Fill and Force Overwrite Handlers

**Dependencies**: TASK-004, TASK-007  
**Estimated Effort**: 4 hours  
**Priority**: P2

**Objective**: Implement handlers for clearing auto-fill and force overwrite

**Files to Create**:
- `Module_Receiving/Handlers/ClearAutoFilledDataCommandHandler.cs`
- `Module_Receiving/Handlers/ForceOverwriteCommandHandler.cs`

**Acceptance Criteria**:
- [ ] ClearAutoFilledDataCommandHandler clears only auto-filled cells
- [ ] ClearAutoFilledDataCommandHandler clears auto-fill flags
- [ ] ForceOverwriteCommandHandler requires confirmation
- [ ] ForceOverwriteCommandHandler logs overwrite operations
- [ ] Unit tests created

---

### TASK-013: Implement Save Workflow Handler

**Dependencies**: TASK-004, TASK-007  
**Estimated Effort**: 4 hours  
**Priority**: P1

**Objective**: Implement handler for saving completed workflow

**Files to Create**:
- `Module_Receiving/Handlers/SaveWorkflowCommandHandler.cs`

**Acceptance Criteria**:
- [ ] SaveWorkflowCommandHandler validates all loads before save
- [ ] SaveWorkflowCommandHandler generates CSV file using CsvHelper
- [ ] SaveWorkflowCommandHandler saves to database via stored procedure
- [ ] SaveWorkflowCommandHandler updates session as saved
- [ ] SaveWorkflowCommandHandler returns SaveResult with paths
- [ ] Unit tests created

**CSV Format**:
```csv
PONumber,PartNumber,LoadNumber,Weight,HeatLot,PackageType,PackagesPerLoad
PO-2024-001,PART-12345,1,250.5,HL-2024-001,Pallet,2
PO-2024-001,PART-12345,2,250.5,HL-2024-001,Pallet,2
```

---

## Phase 4: ViewModels (MVVM Layer)

### TASK-014: Create Main Workflow ViewModel

**Dependencies**: TASK-003, TASK-004  
**Estimated Effort**: 6 hours  
**Priority**: P1

**Objective**: Implement main ViewModel managing the 3-step workflow

**Files to Create**:
- `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs` (partial class)

**Acceptance Criteria**:
- [ ] Partial class inheriting from `ViewModel_Shared_Base`
- [ ] Injects `IMediator` (NOT DAOs or Services)
- [ ] Observable properties for all UI state
- [ ] RelayCommands for all user actions
- [ ] Manages current step navigation
- [ ] Manages edit mode flag
- [ ] Handles validation errors from mediator responses
- [ ] Registered as Transient in App.xaml.cs
- [ ] Unit tests with mocked IMediator

**Properties**:
```csharp
[ObservableProperty] private int _currentStep = 1;
[ObservableProperty] private bool _isEditMode;
[ObservableProperty] private string _poNumber;
[ObservableProperty] private int? _selectedPartId;
[ObservableProperty] private int _loadCount = 1;
[ObservableProperty] private ObservableCollection<LoadDetailViewModel> _loads;
[ObservableProperty] private int _copySourceLoadNumber = 1;
```

**Commands**:
```csharp
[RelayCommand] private async Task NavigateNextAsync();
[RelayCommand] private async Task NavigatePreviousAsync();
[RelayCommand] private async Task SaveAsync();
[RelayCommand] private async Task CopyToAllLoadsAsync(CopyFields fields);
```

---

### TASK-015: Create Load Detail ViewModel

**Dependencies**: TASK-003  
**Estimated Effort**: 3 hours  
**Priority**: P1

**Objective**: Implement ViewModel for individual load data binding

**Files to Create**:
- `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow_LoadDetail.cs`

**Acceptance Criteria**:
- [ ] Inherits from `ObservableObject`
- [ ] Observable properties for all load fields
- [ ] Observable properties for auto-fill flags
- [ ] Observable properties for validation errors
- [ ] Property changed notifications for real-time validation
- [ ] Compiles without errors

---

## Phase 5: Views (XAML UI)

### TASK-016: Create Step 1 View (Order & Part Selection)

**Dependencies**: TASK-014  
**Estimated Effort**: 4 hours  
**Priority**: P1

**Objective**: Implement Step 1 XAML view with x:Bind

**Files to Create**:
- `Module_Receiving/Views/View_Receiving_Workflow_Step1.xaml`
- `Module_Receiving/Views/View_Receiving_Workflow_Step1.xaml.cs`

**Acceptance Criteria**:
- [ ] Uses x:Bind (NOT Binding) for all bindings
- [ ] Explicit Mode specified (OneWay/TwoWay/OneTime)
- [ ] PO Number entry TextBox
- [ ] Part selection ComboBox (populated from GetPartLookupQuery)
- [ ] Load count NumberBox
- [ ] Next button (enabled when valid)
- [ ] Real-time validation error display
- [ ] No business logic in code-behind
- [ ] Registered as Transient in App.xaml.cs

**XAML Example**:
```xml
<TextBox 
    Header="PO Number"
    Text="{x:Bind ViewModel.PONumber, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
    PlaceholderText="Enter PO Number" />
    
<ComboBox
    Header="Part"
    ItemsSource="{x:Bind ViewModel.Parts, Mode=OneWay}"
    SelectedItem="{x:Bind ViewModel.SelectedPart, Mode=TwoWay}" />
```

---

### TASK-017: Create Step 2 View (Load Details Grid)

**Dependencies**: TASK-014, TASK-015  
**Estimated Effort**: 8 hours  
**Priority**: P1 (Most complex view)

**Objective**: Implement Step 2 XAML view with DataGrid for multi-load entry

**Files to Create**:
- `Module_Receiving/Views/View_Receiving_Workflow_Step2.xaml`
- `Module_Receiving/Views/View_Receiving_Workflow_Step2.xaml.cs`

**Acceptance Criteria**:
- [ ] Uses WinUI 3 DataGrid control
- [ ] Uses x:Bind for all bindings
- [ ] Columns: Load #, Weight, Heat Lot, Package Type, Packages Per Load
- [ ] Cell validation indicators (red border on error)
- [ ] Auto-fill indicators (icon or highlight)
- [ ] Copy to All Loads dropdown button
- [ ] Copy Source dropdown selector
- [ ] Clear Auto-Filled Data dropdown button
- [ ] Multi-select support (Shift+Click, Ctrl+Click)
- [ ] Progress indicator for large operations
- [ ] Toast notifications for copy results
- [ ] No business logic in code-behind

**DataGrid Example**:
```xml
<DataGrid ItemsSource="{x:Bind ViewModel.Loads, Mode=OneWay}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Load #" Binding="{Binding LoadNumber}" IsReadOnly="True" />
        <DataGridTextColumn Header="Weight" Binding="{Binding WeightOrQuantity, Mode=TwoWay}" />
        <DataGridTextColumn Header="Heat Lot" Binding="{Binding HeatLot, Mode=TwoWay}" />
        <!-- ... more columns ... -->
    </DataGrid.Columns>
</DataGrid>
```

---

### TASK-018: Create Step 3 View (Review & Save)

**Dependencies**: TASK-014  
**Estimated Effort**: 4 hours  
**Priority**: P1

**Objective**: Implement Step 3 XAML view for review and save

**Files to Create**:
- `Module_Receiving/Views/View_Receiving_Workflow_Step3.xaml`
- `Module_Receiving/Views/View_Receiving_Workflow_Step3.xaml.cs`

**Acceptance Criteria**:
- [ ] Read-only DataGrid showing all load data
- [ ] Auto-fill indicators visible
- [ ] Summary statistics (total loads, total weight, etc.)
- [ ] Edit Details button (navigate to Step 2 in edit mode)
- [ ] Save button (enabled when valid)
- [ ] Cancel button
- [ ] Success message on save
- [ ] No business logic in code-behind

---

## Phase 6: Testing

### TASK-019: Unit Tests for Validators

**Dependencies**: TASK-005  
**Estimated Effort**: 3 hours  
**Priority**: P1

**Objective**: Create comprehensive unit tests for all validators

**Files to Create**:
- `MTM_Receiving_Application.Tests/Module_Receiving/Validators/UpdateStep1CommandValidatorTests.cs`
- `MTM_Receiving_Application.Tests/Module_Receiving/Validators/CopyToLoadsCommandValidatorTests.cs`
- (etc. for all validators)

**Acceptance Criteria**:
- [ ] All validation rules tested
- [ ] Tests use FluentAssertions for assertions
- [ ] Test naming: `MethodName_Condition_ExpectedResult`
- [ ] All tests pass

**Example**:
```csharp
public class UpdateStep1CommandValidatorTests
{
    [Fact]
    public async Task Validate_ValidCommand_ReturnsSuccess()
    {
        var validator = new UpdateStep1CommandValidator();
        var command = new UpdateStep1Command(Guid.NewGuid(), "PO-001", 123, 5);
        
        var result = await validator.ValidateAsync(command);
        
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public async Task Validate_InvalidLoadCount_ReturnsFailure()
    {
        var validator = new UpdateStep1CommandValidator();
        var command = new UpdateStep1Command(Guid.NewGuid(), "PO-001", 123, 0);
        
        var result = await validator.ValidateAsync(command);
        
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateStep1Command.LoadCount));
    }
}
```

---

### TASK-020: Unit Tests for Handlers

**Dependencies**: TASK-008 through TASK-013  
**Estimated Effort**: 8 hours  
**Priority**: P1

**Objective**: Create unit tests for all command/query handlers

**Files to Create**:
- `MTM_Receiving_Application.Tests/Module_Receiving/Handlers/UpdateStep1CommandHandlerTests.cs`
- `MTM_Receiving_Application.Tests/Module_Receiving/Handlers/CopyToLoadsCommandHandlerTests.cs`
- (etc. for all handlers)

**Acceptance Criteria**:
- [ ] Handlers tested with mocked DAOs
- [ ] Success and failure paths tested
- [ ] Edge cases tested
- [ ] Uses FluentAssertions
- [ ] All tests pass

---

### TASK-021: Unit Tests for ViewModels

**Dependencies**: TASK-014, TASK-015  
**Estimated Effort**: 6 hours  
**Priority**: P1

**Objective**: Create unit tests for ViewModels with mocked IMediator

**Files to Create**:
- `MTM_Receiving_Application.Tests/Module_Receiving/ViewModels/ViewModel_Receiving_WorkflowTests.cs`

**Acceptance Criteria**:
- [ ] ViewModel tested with mocked IMediator
- [ ] Commands tested
- [ ] Property change notifications tested
- [ ] Navigation logic tested
- [ ] Validation error handling tested
- [ ] All tests pass

**Example**:
```csharp
public class ViewModel_Receiving_WorkflowTests
{
    [Fact]
    public async Task NavigateNextAsync_Step1Valid_AdvancesToStep2()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<GetValidationStatusQuery>(), default))
                .ReturnsAsync(Result<ValidationStatus>.Success(new ValidationStatus(true, [], 0, 0)));
        
        var viewModel = new ViewModel_Receiving_Workflow(mediator.Object, Mock.Of<IService_ErrorHandler>(), Mock.Of<ILogger>());
        viewModel.CurrentStep = 1;
        
        await viewModel.NavigateNextCommand.ExecuteAsync(null);
        
        viewModel.CurrentStep.Should().Be(2);
    }
}
```

---

### TASK-022: Integration Tests for DAOs

**Dependencies**: TASK-006, TASK-007  
**Estimated Effort**: 6 hours  
**Priority**: P2

**Objective**: Create integration tests for DAOs against real test database

**Files to Create**:
- `MTM_Receiving_Application.Tests/Module_Receiving/Integration/Dao_ReceivingWorkflowSessionIntegrationTests.cs`
- `MTM_Receiving_Application.Tests/Module_Receiving/Integration/Dao_ReceivingLoadDetailIntegrationTests.cs`

**Acceptance Criteria**:
- [ ] Tests use real MySQL test database
- [ ] `IAsyncLifetime` used for setup/teardown
- [ ] Test data prefixed with "TEST-"
- [ ] All CRUD operations tested
- [ ] All tests pass

---

### TASK-023: End-to-End Integration Tests

**Dependencies**: All handler tasks  
**Estimated Effort**: 8 hours  
**Priority**: P2

**Objective**: Create end-to-end tests for complete workflows

**Files to Create**:
- `MTM_Receiving_Application.Tests/Module_Receiving/Integration/ReceivingWorkflow_EndToEnd_Tests.cs`

**Acceptance Criteria**:
- [ ] Test complete 3-step workflow
- [ ] Test bulk copy scenarios
- [ ] Test edit from review
- [ ] Test save to CSV and database
- [ ] Verify data integrity after save
- [ ] All tests pass

---

## Phase 7: User Story Implementation (Features)

### TASK-024: Implement User Story 1 (Workflow 1.1 - Foundation)

**Dependencies**: TASK-014 through TASK-018  
**Estimated Effort**: 4 hours  
**Priority**: P1 (MVP - blocks all other stories)

**Objective**: Verify complete 3-step workflow works end-to-end

**Files to Test**:
- All Step 1, 2, 3 views
- Main workflow ViewModel
- All foundational handlers

**Acceptance Criteria**:
- [ ] User can complete all 3 steps
- [ ] Data validated at each step
- [ ] Navigation works (forward and backward)
- [ ] Save creates CSV and database records
- [ ] All acceptance scenarios from spec.md pass
- [ ] Follows quickstart.md Scenario 1

---

### TASK-025: Implement User Story 2.1 (Bulk Copy to All Loads)

**Dependencies**: TASK-011, TASK-024  
**Estimated Effort**: 4 hours  
**Priority**: P2

**Objective**: Implement bulk copy dropdown with all options

**Acceptance Criteria**:
- [ ] Copy to All Loads dropdown button visible
- [ ] Copy only fills empty cells
- [ ] Auto-fill flags set on copied cells
- [ ] Visual highlight for auto-filled cells (10 sec fade)
- [ ] Notification shows copy result
- [ ] All acceptance scenarios from spec.md pass
- [ ] Follows quickstart.md Scenario 2 and 3

---

### TASK-026: Implement User Story 2.2 (Change Copy Source)

**Dependencies**: TASK-011, TASK-025  
**Estimated Effort**: 2 hours  
**Priority**: P2

**Objective**: Allow user to select any load as copy source

**Acceptance Criteria**:
- [ ] Copy Source dropdown selector visible
- [ ] Selected load highlighted as source
- [ ] Copy operations use selected source
- [ ] Notification confirms source change
- [ ] Follows quickstart.md Scenario 5

---

### TASK-027: Implement User Story 2.3 (Clear Auto-Filled Data)

**Dependencies**: TASK-012, TASK-025  
**Estimated Effort**: 3 hours  
**Priority**: P2

**Objective**: Allow user to clear only auto-filled data

**Acceptance Criteria**:
- [ ] Clear Auto-Filled Data dropdown visible
- [ ] Confirmation dialog shown before clear
- [ ] Only auto-filled cells cleared
- [ ] Manually entered data preserved
- [ ] Auto-fill flags cleared after operation
- [ ] Follows quickstart.md Scenario 6

---

### TASK-028: Implement User Story 2.4 (Force Overwrite)

**Dependencies**: TASK-012, TASK-025  
**Estimated Effort**: 3 hours  
**Priority**: P3 (Power user feature)

**Objective**: Allow force overwrite of occupied cells

**Acceptance Criteria**:
- [ ] Force Overwrite option in context menu or dropdown
- [ ] Requires explicit confirmation
- [ ] Overwrites existing data
- [ ] Logs overwrite operations
- [ ] Sets auto-fill flags on all overwritten cells

---

### TASK-029: Implement User Story 3 (Manual Entry Mode AutoFill)

**Dependencies**: TASK-024  
**Estimated Effort**: 6 hours  
**Priority**: P3

**Objective**: Implement Manual Entry Mode with grid auto-fill

**Acceptance Criteria**:
- [ ] Manual Entry Mode selectable from mode screen
- [ ] Grid allows free-form data entry
- [ ] Auto-fill works similar to Guided Mode
- [ ] All acceptance scenarios from spec.md pass

---

### TASK-030: Implement User Story 4 (Edit from Review Screen)

**Dependencies**: TASK-024  
**Estimated Effort**: 3 hours  
**Priority**: P2

**Objective**: Allow editing from Step 3 review screen

**Acceptance Criteria**:
- [ ] Edit Details button on Step 3
- [ ] Navigates directly to Step 2 (skips Step 1)
- [ ] Edit Mode indicator visible
- [ ] Return to Review button visible (replaces Next)
- [ ] Returns directly to Step 3 (skips Step 1)
- [ ] Corrected fields highlighted for 3-5 seconds
- [ ] Follows quickstart.md Scenario 7

---

### TASK-031: Implement User Story 5 (Non-PO Receiving)

**Dependencies**: TASK-024  
**Estimated Effort**: 4 hours  
**Priority**: P3

**Objective**: Support receiving without purchase order

**Acceptance Criteria**:
- [ ] Non-PO mode selectable
- [ ] PO Number field hidden
- [ ] Vendor Name field visible
- [ ] Part selection works without PO
- [ ] Save works with NULL PO Number
- [ ] Follows quickstart.md Scenario 8

---

### TASK-032: Implement User Story 6 (Real-Time Validation)

**Dependencies**: TASK-024  
**Estimated Effort**: 3 hours  
**Priority**: P2

**Objective**: Show validation errors immediately on field blur

**Acceptance Criteria**:
- [ ] Validation runs on field blur (LostFocus event)
- [ ] Red border on invalid fields
- [ ] Error icon next to field
- [ ] Error message displayed
- [ ] Error count summary updated
- [ ] Follows quickstart.md Scenario 9

---

## Phase 8: Performance & Polish

### TASK-033: Performance Testing with 100 Loads

**Dependencies**: TASK-025  
**Estimated Effort**: 4 hours  
**Priority**: P2

**Objective**: Verify performance meets goals with maximum load count

**Acceptance Criteria**:
- [ ] Copy operation with 100 loads completes in < 1 second
- [ ] No UI freezing or lag
- [ ] Progress indicator visible for > 50 loads
- [ ] Memory usage acceptable
- [ ] Follows quickstart.md Performance Testing section

---

### TASK-034: Accessibility and UX Polish

**Dependencies**: All view tasks  
**Estimated Effort**: 4 hours  
**Priority**: P3

**Objective**: Ensure accessible and polished user experience

**Acceptance Criteria**:
- [ ] Keyboard navigation works throughout
- [ ] Screen reader announces state changes
- [ ] Focus indicators visible
- [ ] Color contrast meets WCAG standards
- [ ] Tooltips on all icons and buttons
- [ ] Loading states for async operations

---

## Summary

**Total Tasks**: 34  
**Estimated Effort**: ~130 hours  
**Critical Path**: TASK-001 → TASK-003 → TASK-004 → TASK-006 → TASK-008 → TASK-014 → TASK-016 → TASK-024

**Implementation Order**:
1. **Phase 0-1**: Foundation (TASK-001 through TASK-005) - ~11 hours
2. **Phase 2**: Data Access (TASK-006 through TASK-007) - ~6 hours
3. **Phase 3**: Handlers (TASK-008 through TASK-013) - ~24 hours
4. **Phase 4**: ViewModels (TASK-014 through TASK-015) - ~9 hours
5. **Phase 5**: Views (TASK-016 through TASK-18) - ~16 hours
6. **Phase 6**: Testing (TASK-019 through TASK-023) - ~31 hours
7. **Phase 7**: User Stories (TASK-024 through TASK-032) - ~32 hours
8. **Phase 8**: Performance & Polish (TASK-033 through TASK-034) - ~8 hours

**MVP Scope** (Deliver immediate value):
- TASK-001 through TASK-018: Core infrastructure and UI
- TASK-024: User Story 1 (3-step workflow)
- TASK-025: User Story 2.1 (Bulk copy)

**Post-MVP Enhancements**:
- TASK-026 through TASK-032: Additional features
- TASK-033 through TASK-034: Performance and polish

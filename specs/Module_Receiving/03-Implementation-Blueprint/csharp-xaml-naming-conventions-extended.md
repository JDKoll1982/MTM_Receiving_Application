# Naming Conventions Extended (5-Part Standard for ALL Artifacts)

**Version:** 1.0  
**Last Updated:** 2026-01-25  
**Purpose:** Complete naming standard for all Module_Receiving artifacts including methods

## Core Principle

**EVERY artifact** follows the **5-Part Naming Standard**:

```
{Type}_{Module}_{Mode}_{CategoryType}_{DescriptiveName}
```

**Applies To:**
- Classes (ViewModels, Views, Services, Models, DAOs, Helpers, Enums)
- **Methods** ← NEW
- Files (match class names)
- Interfaces (prefix with I)
- Properties (when complex)
- Database Tables (prefix: tbl_)
- Stored Procedures (prefix: sp_)

## Part Definitions

### Part 1: Type/Action

**For Classes:**
- `ViewModel` - Presentation logic
- `View` - XAML UI
- `Model` - Data structures
- `Dao` - Data access objects
- `Helper` - Utility functions
- `Enum` - Enumerations
- **CQRS (MediatR) Patterns:**
  - `Command` - Write operation request (in Requests/Commands/)
  - `Query` - Read operation request (in Requests/Queries/)
  - `CommandHandler` - Handler for command (in Handlers/Commands/)
  - `QueryHandler` - Handler for query (in Handlers/Queries/)
  - `Validator` - FluentValidation validator (in Validators/)

**For Methods:**
- `Load` - Retrieve/fetch data
- `Save` - Persist data
- `Validate` - Check data validity
- `Calculate` - Perform calculations
- `Copy` - Duplicate/transfer data
- `Clear` - Remove/reset data
- `Get` - Retrieve (synchronous or simple)
- `Set` - Assign value
- `Insert` - Add new record
- `Update` - Modify existing record
- `Delete` - Remove record
- `Select` - Query records
- `Execute` - Run operation
- `Initialize` - Set up initial state
- `Navigate` - Move between screens
- `Apply` - Execute operation
- `Build` - Construct object
- `Parse` - Extract/transform data
- `Format` - Structure data
- `Convert` - Transform data type

### Part 2: Module

**Values:**
- `Receiving` - Receiving module
- `Shared` - Cross-module shared components
- `Database` - Database-specific (for generic helpers)
- `Validation` - Validation-specific (for generic helpers)
- `FileIO` - File operations (for generic helpers)

### Part 3: Mode

**Values:**
- `Wizard` - Guided Mode (3-step wizard)
- `Manual` - Manual Entry Mode (grid-based)
- `Edit` - Edit Mode (historical transactions)
- `Hub` - Hub Orchestration (mode selection)
- `Shared` - Used across multiple modes
- `Consolidated` - Legacy/alternative workflow variant

### Part 4: CategoryType

**For ViewModels:**
- `Orchestration` - Main workflow coordinator
- `Display` - Data display/presentation
- `Dialog` - Modal dialogs
- `Panel` - Side panels or sections
- `Interaction` - User interaction handlers

**For Services:**
- `Business` - Business logic
- `Infrastructure` - Infrastructure services
- `Utility` - Utility services
- `Orchestration` - Workflow coordination

**For Models:**
- `TableEntitys` - Database entity models
- `DataTransferObjects` - Data transfer objects (DTOs)
- `Result` - Operation results

**For DAOs:**
- `Repository` - Data repository pattern
- `DataAccess` - Direct data access
- `Query` - Query operations

**For Helpers:**
- `Database` - Database utilities
- `FileIO` - File I/O utilities
- `Validation` - Validation utilities
- `Transformation` - Data transformation
- `Infrastructure` - Infrastructure utilities

**For Methods:**
- `Data` - Data-related operations
- `Input` - Input validation/processing
- `Transaction` - Database transactions
- `BulkOperation` - Bulk/batch operations
- `Business` - Business logic execution
- `Query` - Database queries
- `Record` - Single record operations
- `Records` - Multiple record operations
- `Session` - Session state management
- `Navigation` - Navigation operations
- `UI` - UI updates
- `Calculation` - Calculation operations

### Part 5: DescriptiveName

**Must be specific and clear:**
- ✅ GOOD: `PONumberFromSession`, `ReceivingLinesByPO`, `AllFieldsToEmptyCells`
- ❌ BAD: `Data`, `Value`, `Info`, `Thing`

**Guidelines:**
- Use full words, avoid abbreviations (except PO, CSV, SQL)
- Concatenate multiple words without separators
- Be specific about what/which entity
- Include prepositions when needed (To, From, By, For)

## Method Naming

### Format

```csharp
{Action}_{Module}_{Mode}_{CategoryType}_{DescriptiveName}Async
```

**Async Suffix:** Add `Async` if method is async (always at the very end)

### Examples by Action Type

#### Load Methods

```csharp
// In ViewModel_Receiving_Wizard_Display_PONumberEntry
public async Task Load_Receiving_Wizard_Data_PONumberFromSessionAsync()
public async Task Load_Receiving_Wizard_Data_PartDetailsFromERPAsync()
public async Task Load_Receiving_Wizard_Data_LoadCountFromSessionAsync()

// In Service_Receiving_Business_MySQL_ReceivingLine
public async Task Load_Receiving_Database_Query_ReceivingLinesByPOAsync(string poNumber)
```

#### Save Methods

```csharp
// In ViewModel_Receiving_Wizard_Display_PONumberEntry
public async Task Save_Receiving_Wizard_Data_PONumberToSessionAsync()
public async Task Save_Receiving_Wizard_Transaction_CompleteWorkflowAsync()

// In Service_Receiving_Business_MySQL_ReceivingLine
public async Task Save_Receiving_Database_Transaction_ReceivingLineAsync(Model_Receiving_Entity_ReceivingLine line)
public async Task Save_Receiving_FileIO_Transaction_ReceivingLineToCSVAsync(Model_Receiving_Entity_ReceivingLine line)
```

#### Validate Methods

```csharp
// In ViewModel_Receiving_Wizard_Display_PONumberEntry
public async Task<bool> Validate_Receiving_Wizard_Input_PONumberFormatAsync()
public async Task<bool> Validate_Receiving_Wizard_Input_AllFieldsCompleteAsync()

// In Helper_Validation_PONumber
public static bool Validate_Shared_Input_PONumberFormatAndStandardize(string input, out string standardized, out string error)
```

#### Calculate Methods

```csharp
// In Service_Receiving_Business_LoadCalculation
public async Task<int> Calculate_Receiving_Shared_Business_LoadCountFromPiecesAsync(int totalPieces, int piecesPerLoad)
public decimal Calculate_Receiving_Shared_Business_TotalWeightFromLoadsAsync(List<Model_Receiving_Entity_Load> loads)
```

#### Copy Methods (Bulk Operations)

```csharp
// In ViewModel_Receiving_Wizard_Display_LoadDetailsGrid
public async Task Copy_Receiving_Wizard_BulkOperation_AllFieldsToEmptyCellsAsync()
public async Task Copy_Receiving_Wizard_BulkOperation_HeatLotOnlyToEmptyCellsAsync()
public async Task Copy_Receiving_Wizard_BulkOperation_FieldsToSelectedLoadsAsync(List<int> targetLoadNumbers)
```

#### Clear Methods

```csharp
// In ViewModel_Receiving_Wizard_Display_LoadDetailsGrid
public async Task Clear_Receiving_Wizard_Data_AllAutoFilledFieldsAsync()
public async Task Clear_Receiving_Wizard_Data_AutoFilledHeatLotOnlyAsync()
public async Task Clear_Receiving_Wizard_Data_AllFieldsAsync()
```

#### Database DAO Methods

```csharp
// In Dao_Receiving_Repository_ReceivingLine
public async Task<Model_Dao_Result> Insert_Receiving_Database_Record_ReceivingLineAsync(Model_Receiving_Entity_ReceivingLine line)
public async Task<Model_Dao_Result> Update_Receiving_Database_Record_ReceivingLineAsync(Model_Receiving_Entity_ReceivingLine line)
public async Task<Model_Dao_Result> Delete_Receiving_Database_Record_ReceivingLineAsync(Guid lineId)
public async Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLine>>> Select_Receiving_Database_Records_ReceivingLinesByPOAsync(string poNumber)
public async Task<Model_Dao_Result<Model_Receiving_Entity_ReceivingLine>> Select_Receiving_Database_Record_ReceivingLineByIdAsync(Guid lineId)
```

#### Navigation Methods

```csharp
// In ViewModel_Receiving_Hub_Orchestration_MainWorkflow
public async Task Navigate_Receiving_Hub_Navigation_ToGuidedModeAsync()
public async Task Navigate_Receiving_Hub_Navigation_ToManualModeAsync()
public async Task Navigate_Receiving_Wizard_Navigation_ToNextStepAsync()
public async Task Navigate_Receiving_Wizard_Navigation_ToPreviousStepAsync()
public async Task Navigate_Receiving_Wizard_Navigation_ToReviewFromEditModeAsync()
```

#### Initialize Methods

```csharp
// In ViewModel_Receiving_Wizard_Orchestration_MainWorkflow
public async Task Initialize_Receiving_Wizard_Session_NewWorkflowAsync()
public async Task Initialize_Receiving_Wizard_Data_EmptyLoadsAsync(int loadCount)
public async Task Initialize_Receiving_Manual_Data_GridRowsAsync(int rowCount)
```

### Method Naming by Class Type

#### ViewModel Methods

**Pattern:** `{Action}_Receiving_{Mode}_{CategoryType}_{DescriptiveName}Async`

```csharp
public partial class ViewModel_Receiving_Wizard_Display_PONumberEntry : ViewModel_Shared_Base
{
    // Data Loading
    public async Task Load_Receiving_Wizard_Data_PONumberFromSessionAsync() { }
    public async Task Load_Receiving_Wizard_Data_PartDetailsFromERPAsync() { }
    
    // Data Saving
    public async Task Save_Receiving_Wizard_Data_PONumberToSessionAsync() { }
    public async Task Save_Receiving_Wizard_Session_CurrentStateAsync() { }
    
    // Validation
    public async Task<bool> Validate_Receiving_Wizard_Input_PONumberFormatAsync() { }
    public async Task<bool> Validate_Receiving_Wizard_Input_AllFieldsCompleteAsync() { }
    
    // UI Updates
    public async Task Update_Receiving_Wizard_UI_ValidationErrorsAsync() { }
    public async Task Update_Receiving_Wizard_UI_EnableNextButtonAsync() { }
    
    // Navigation
    public async Task Navigate_Receiving_Wizard_Navigation_ToNextStepAsync() { }
    
    // Clearing
    public async Task Clear_Receiving_Wizard_Data_AllFieldsAsync() { }
}
```

#### Service Methods

**Pattern:** `{Action}_Receiving_{Scope}_{CategoryType}_{DescriptiveName}Async`

```csharp
public class Service_Receiving_Business_MySQL_ReceivingLine : IService_Receiving_Business_MySQL_ReceivingLine
{
    // Save operations
    public async Task<Model_Dao_Result> Save_Receiving_Database_Transaction_ReceivingLineAsync(Model_Receiving_Entity_ReceivingLine line) { }
    
    // Query operations
    public async Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLine>>> Get_Receiving_Database_Query_ReceivingLinesByPOAsync(string poNumber) { }
    public async Task<Model_Dao_Result<Model_Receiving_Entity_ReceivingLine>> Get_Receiving_Database_Query_ReceivingLineByIdAsync(Guid lineId) { }
    
    // Business logic
    public async Task<Model_Dao_Result> Validate_Receiving_Business_Input_ReceivingLineDataAsync(Model_Receiving_Entity_ReceivingLine line) { }
}
```

#### DAO Methods

**Pattern:** `{DatabaseAction}_Receiving_Database_{RecordType}_{DescriptiveName}Async`

```csharp
public class Dao_Receiving_Repository_ReceivingLine
{
    // Insert
    public async Task<Model_Dao_Result> Insert_Receiving_Database_Record_ReceivingLineAsync(Model_Receiving_Entity_ReceivingLine line) { }
    public async Task<Model_Dao_Result> Insert_Receiving_Database_Records_BulkReceivingLinesAsync(List<Model_Receiving_Entity_ReceivingLine> lines) { }
    
    // Update
    public async Task<Model_Dao_Result> Update_Receiving_Database_Record_ReceivingLineAsync(Model_Receiving_Entity_ReceivingLine line) { }
    
    // Delete
    public async Task<Model_Dao_Result> Delete_Receiving_Database_Record_ReceivingLineAsync(Guid lineId) { }
    public async Task<Model_Dao_Result> Delete_Receiving_Database_Records_ReceivingLinesByTransactionAsync(Guid transactionId) { }
    
    // Select
    public async Task<Model_Dao_Result<List<Model_Receiving_Entity_ReceivingLine>>> Select_Receiving_Database_Records_ReceivingLinesByPOAsync(string poNumber) { }
    public async Task<Model_Dao_Result<Model_Receiving_Entity_ReceivingLine>> Select_Receiving_Database_Record_ReceivingLineByIdAsync(Guid lineId) { }
}
```

#### Helper Methods

**Pattern:** `{Action}_Shared_{CategoryType}_{DescriptiveName}`

```csharp
public static class Helper_Validation_PONumber
{
    // Validation
    public static bool Validate_Shared_Input_PONumberFormatAndStandardize(string input, out string standardized, out string errorMessage) { }
    
    // Formatting
    public static string Format_Shared_Input_PONumberWithPadding(string input) { }
    
    // Parsing
    public static bool Parse_Shared_Input_PONumberFromUserInput(string input, out string poNumber) { }
}

public static class Helper_Database_StoredProcedure
{
    // Execute
    public static async Task<Model_Dao_Result> Execute_Shared_Database_StoredProcedureAsync(string spName, MySqlParameter[] parameters, string connectionString) { }
    
    // Build
    public static MySqlParameter[] Build_Shared_Database_ParametersFromEntity(object entity) { }
}
```

## Property Naming

**Standard Properties:** Use PascalCase (not 5-part)
```csharp
public string PONumber { get; set; }
public int LoadCount { get; set; }
public bool IsNonPO { get; set; }
```

**Observable Properties:** Use `[ObservableProperty]` attribute on private field
```csharp
[ObservableProperty]
private string _poNumber = string.Empty;

[ObservableProperty]
private int _loadCount;

[ObservableProperty]
private bool _isNonPO;
```

**Complex Domain Properties (when needed):**
```csharp
public Model_Receiving_Entity_WorkflowSession CurrentSession { get; set; }
public List<Model_Receiving_Entity_LoadDetail> LoadDetails { get; set; }
```

## Variable Naming

**Local Variables:** Use camelCase
```csharp
var poNumber = "PO-123456";
var loadCount = 5;
var isNonPO = false;
var receivingLines = new List<Model_Receiving_Entity_ReceivingLine>();
```

**Private Fields:** Use _camelCase (underscore prefix)
```csharp
private readonly IService_Receiving_Business_MySQL_ReceivingLine _receivingLineService;
private readonly string _connectionString;
private bool _isInitialized;
```

**Parameters:** Use camelCase
```csharp
public async Task Load_Receiving_Wizard_Data_PONumberAsync(string poNumber, bool validateFormat)
{
    // poNumber and validateFormat are parameters
}
```

## Database Naming

### Table Names

**Format:** `tbl_{Module}_{EntityName}`

```sql
tbl_Receiving_Transaction
tbl_Receiving_Line
tbl_Receiving_Load
tbl_Receiving_CompletedTransaction
tbl_Receiving_AuditLog
```

### Stored Procedure Names

**Format:** `sp_{Module}_{EntityName}_{Action}`

```sql
-- Insert
sp_Receiving_Line_Insert
sp_Receiving_Transaction_Insert

-- Update
sp_Receiving_Line_Update
sp_Receiving_Transaction_Update

-- Delete
sp_Receiving_Line_Delete
sp_Receiving_Line_DeleteByTransaction

-- Select
sp_Receiving_Line_SelectByPO
sp_Receiving_Line_SelectById
sp_Receiving_Transaction_SelectByDateRange
sp_Receiving_Transaction_SelectByUser
```

### Column Names

**Use PascalCase:**
```sql
CREATE TABLE tbl_Receiving_Line (
    LineId CHAR(36) PRIMARY KEY,
    TransactionId CHAR(36) NOT NULL,
    PONumber VARCHAR(50),
    PartNumber VARCHAR(50) NOT NULL,
    LoadNumber INT NOT NULL,
    Weight DECIMAL(18, 2),
    HeatLot VARCHAR(100),
    PackageType VARCHAR(50),
    PackagesPerLoad INT,
    CreatedBy VARCHAR(100),
    CreatedDate DATETIME,
    ModifiedBy VARCHAR(100),
    ModifiedDate DATETIME
);
```

## File Naming

**Files MUST match class names exactly:**

```
ViewModel_Receiving_Wizard_Display_PONumberEntry.cs
View_Receiving_Wizard_Display_PONumberEntry.xaml
View_Receiving_Wizard_Display_PONumberEntry.xaml.cs
Service_Receiving_Business_MySQL_ReceivingLine.cs
IService_Receiving_Business_MySQL_ReceivingLine.cs
Model_Receiving_TableEntitys_ReceivingLine.cs
Model_Receiving_DataTransferObjects_PartDetails.cs
Dao_Receiving_Repository_ReceivingLine.cs
Helper_Validation_PONumber.cs
Enum_Receiving_Mode_WorkflowMode.cs
CommandRequest_Receiving_Shared_Save_Transaction.cs
CommandHandler_Receiving_Shared_Save_Transaction.cs
QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO.cs
QueryHandler_Receiving_Shared_Get_ReceivingLinesByPO.cs
Validator_Receiving_Shared_ValidateOn_SaveTransaction.cs
```

## Namespace Naming

**Format:** `MTM_Receiving_Application.Module_{Module}.{Layer}`

```csharp
namespace MTM_Receiving_Application.Module_Receiving.ViewModels
namespace MTM_Receiving_Application.Module_Receiving.Views
namespace MTM_Receiving_Application.Module_Receiving.Services
namespace MTM_Receiving_Application.Module_Receiving.Models
namespace MTM_Receiving_Application.Module_Receiving.DAOs
namespace MTM_Receiving_Application.Module_Shared.Helpers
namespace MTM_Receiving_Application.Module_Shared.Enums
```

### CQRS (MediatR) Naming Conventions

**Module_Receiving uses CQRS pattern with MediatR for all business operations.**

#### Commands (Write Operations)

**Location:** `Module_Receiving/Requests/Commands/`

**Format:** `CommandRequest_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`

**Examples:**
```csharp
// Module_Receiving/Requests/Commands/CommandRequest_Receiving_Shared_Save_Transaction.cs
namespace MTM_Receiving_Application.Module_Receiving.Requests.Commands;

public record CommandRequest_Receiving_Shared_Save_Transaction : IRequest<Model_Dao_Result>
{
    public string TransactionId { get; init; } = string.Empty;
    public string PONumber { get; init; } = string.Empty;
    public List<LoadDetail> Loads { get; init; } = new();
}
```

**Naming Pattern (ACTUAL):**
- `CommandRequest_Receiving_Shared_Save_Transaction`
- `CommandRequest_Receiving_Shared_Update_ReceivingLine`
- `CommandRequest_Receiving_Shared_Delete_ReceivingLine`
- `CommandRequest_Receiving_Wizard_Copy_FieldsToEmptyLoads`
- `CommandRequest_Receiving_Wizard_Clear_AutoFilledFields`
- `CommandRequest_Receiving_Shared_Complete_Workflow`

#### Queries (Read Operations)

**Location:** `Module_Receiving/Requests/Queries/`

**Format:** `QueryRequest_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`

**Examples:**
```csharp
// Module_Receiving/Requests/Queries/QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO.cs
namespace MTM_Receiving_Application.Module_Receiving.Requests.Queries;

public record QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO : IRequest<Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingLoad>>>
{
    public string PONumber { get; init; } = string.Empty;
}
```

**Naming Pattern (ACTUAL):**
- `QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO`
- `QueryRequest_Receiving_Shared_Get_ReceivingTransactionById`
- `QueryRequest_Receiving_Shared_Get_PartDetails`
- `QueryRequest_Receiving_Shared_Get_WorkflowSession`
- `QueryRequest_Receiving_Shared_Get_ReferenceData`
- `QueryRequest_Receiving_Shared_Search_Transactions`
- `QueryRequest_Receiving_Shared_Validate_PONumber`

#### Command Handlers

**Location:** `Module_Receiving/Handlers/Commands/`

**Format:** `CommandHandler_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`

**Examples:**
```csharp
// Module_Receiving/Handlers/Commands/CommandHandler_Receiving_Shared_Save_Transaction.cs
namespace MTM_Receiving_Application.Module_Receiving.Handlers.Commands;

public class CommandHandler_Receiving_Shared_Save_Transaction : 
    IRequestHandler<CommandRequest_Receiving_Shared_Save_Transaction, Model_Dao_Result>
{
    private readonly Dao_Receiving_Repository_Transaction _dao;
    private readonly IService_LoggingUtility _logger;
    
    public CommandHandler_Receiving_Shared_Save_Transaction(
        Dao_Receiving_Repository_Transaction dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }
    
    public async Task<Model_Dao_Result> Handle(
        CommandRequest_Receiving_Shared_Save_Transaction request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInfo("Saving receiving transaction");
        return await _dao.InsertAsync(request);
    }
}
```

**Naming Pattern (ACTUAL):**
- `CommandHandler_Receiving_Shared_Save_Transaction`
- `CommandHandler_Receiving_Shared_Update_ReceivingLine`
- `CommandHandler_Receiving_Shared_Delete_ReceivingLine`
- `CommandHandler_Receiving_Wizard_Copy_FieldsToEmptyLoads`
- `CommandHandler_Receiving_Wizard_Clear_AutoFilledFields`
- `CommandHandler_Receiving_Shared_Complete_Workflow`

#### Query Handlers

**Location:** `Module_Receiving/Handlers/Queries/`

**Format:** `QueryHandler_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`

**Examples:**
```csharp
// Module_Receiving/Handlers/Queries/QueryHandler_Receiving_Shared_Get_ReceivingLinesByPO.cs
namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries;

public class QueryHandler_Receiving_Shared_Get_ReceivingLinesByPO :
    IRequestHandler<QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO, Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingLoad>>>
{
    private readonly Dao_Receiving_Repository_Line _dao;
    private readonly IService_LoggingUtility _logger;
    
    public QueryHandler_Receiving_Shared_Get_ReceivingLinesByPO(
        Dao_Receiving_Repository_Line dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }
    
    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingLoad>>> Handle(
        QueryRequest_Receiving_Shared_Get_ReceivingLinesByPO request,
        CancellationToken cancellationToken)
    {
        _logger.LogInfo($"Getting receiving lines for PO: {request.PONumber}");
        return await _dao.SelectByPOAsync(request.PONumber);
    }
}
```

**Naming Pattern (ACTUAL):**
- `QueryHandler_Receiving_Shared_Get_ReceivingLinesByPO`
- `QueryHandler_Receiving_Shared_Get_ReceivingTransactionById`
- `QueryHandler_Receiving_Shared_Get_PartDetails`
- `QueryHandler_Receiving_Shared_Get_WorkflowSession`
- `QueryHandler_Receiving_Shared_Get_ReferenceData`
- `QueryHandler_Receiving_Shared_Search_Transactions`
- `QueryHandler_Receiving_Shared_Validate_PONumber`

#### Validators (FluentValidation)

**Location:** `Module_Receiving/Validators/`

**Format:** `Validator_{Module}_{Mode}_{CategoryType}_{DescriptiveName}`

**Examples:**
```csharp
// Module_Receiving/Validators/Validator_Receiving_Shared_ValidateOn_SaveTransaction.cs
namespace MTM_Receiving_Application.Module_Receiving.Validators;

public class Validator_Receiving_Shared_ValidateOn_SaveTransaction : AbstractValidator<CommandRequest_Receiving_Shared_Save_Transaction>
{
    public Validator_Receiving_Shared_ValidateOn_SaveTransaction()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("Transaction ID is required");
        
        RuleFor(x => x.PONumber)
            .NotEmpty()
            .WithMessage("PO Number is required");
        
        RuleFor(x => x.Loads)
            .NotEmpty()
            .WithMessage("At least one load is required");
    }
}
```

**Naming Pattern (ACTUAL):**
- `Validator_Receiving_Shared_ValidateOn_SaveTransaction`
- `Validator_Receiving_Shared_ValidateOn_UpdateReceivingLine`
- `Validator_Receiving_Shared_ValidateIf_ValidPOFormat`
- `Validator_Receiving_Shared_ValidateIf_POExists`
- `Validator_Receiving_Wizard_ValidateOn_CopyFieldsToEmptyLoads`

#### ViewModel Integration with MediatR

**ViewModels use IMediator instead of Services:**

```csharp
// File: ViewModel_Receiving_Wizard_Display_PONumberEntry.cs
namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_Wizard_Display_PONumberEntry : ViewModel_Shared_Base
    {
        private readonly IMediator _mediator;  // ← Inject IMediator, not services
        
        [ObservableProperty]
        private string _poNumber = string.Empty;
        
        public ViewModel_Receiving_Wizard_Display_PONumberEntry(
            IMediator mediator,
            IService_Shared_Infrastructure_ErrorHandler errorHandler,
            ILogger<ViewModel_Receiving_Wizard_Display_PONumberEntry> logger) : base(errorHandler, logger)
        {
            _mediator = mediator;
        }
        
        [RelayCommand]
        public async Task Load_Receiving_Wizard_Data_PONumberFromSessionAsync()
        {
            if (IsBusy) return;
            
            try
            {
                IsBusy = true;
                
                var query = new GetWorkflowSessionQuery();
                
                // Send query through MediatR
                // ValidationBehavior runs automatically
                // LoggingBehavior logs automatically
                // AuditBehavior logs user context automatically
                var result = await _mediator.Send(query);
                
                if (result.IsSuccess && result.Data != null)
                {
                    PONumber = result.Data.PONumber ?? string.Empty;
                }
            }
            catch (FluentValidation.ValidationException ex)
            {
                var errors = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage));
                _errorHandler.ShowUserError(errors, "Validation Error", nameof(Load_Receiving_Wizard_Data_PONumberFromSessionAsync));
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(Load_Receiving_Wizard_Data_PONumberFromSessionAsync), nameof(ViewModel_Receiving_Wizard_Display_PONumberEntry));
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        [RelayCommand]
        public async Task Save_Receiving_Wizard_Data_PONumberToSessionAsync()
        {
            if (IsBusy) return;
            
            try
            {
                IsBusy = true;
                
                var command = new SaveWorkflowSessionCommand
                {
                    PONumber = PONumber
                };
                
                var result = await _mediator.Send(command);
                
                if (!result.IsSuccess)
                {
                    _errorHandler.ShowUserError(result.ErrorMessage, "Save Error", nameof(Save_Receiving_Wizard_Data_PONumberToSessionAsync));
                }
            }
            catch (FluentValidation.ValidationException ex)
            {
                var errors = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage));
                _errorHandler.ShowUserError(errors, "Validation Error", nameof(Save_Receiving_Wizard_Data_PONumberToSessionAsync));
            }
            catch (Exception ex)
            {
                _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, nameof(Save_Receiving_Wizard_Data_PONumberToSessionAsync), nameof(ViewModel_Receiving_Wizard_Display_PONumberEntry));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
```

#### Pipeline Behaviors (Automatic)

**Location:** `Module_Core/Behaviors/` (shared across all modules)

All handlers automatically benefit from these behaviors:

1. **ValidationBehavior** - Runs FluentValidation validators before handler
2. **LoggingBehavior** - Logs request start/end and execution time
3. **AuditBehavior** - Logs user context for compliance

**No additional code needed in handlers - behaviors run automatically!**

## Complete Example

```csharp
// File: ViewModel_Receiving_Wizard_Display_PONumberEntry.cs
namespace MTM_Receiving_Application.Module_Receiving.ViewModels
{
    public partial class ViewModel_Receiving_Wizard_Display_PONumberEntry : ViewModel_Shared_Base
    {
        // Dependencies
        private readonly IService_Receiving_Business_MySQL_ReceivingLine _receivingLineService;
        private readonly IService_Receiving_Business_MySQL_ReceivingTransaction _receivingTransactionService;
        private readonly IService_ErrorHandler _errorHandler;
        private readonly IService_LoggingUtility _logger;
        
        // Observable Properties
        [ObservableProperty]
        private string _poNumber = string.Empty;
        
        [ObservableProperty]
        private string _partNumber = string.Empty;
        
        [ObservableProperty]
        private int _loadCount;
        
        [ObservableProperty]
        private bool _isNonPO;
        
        [ObservableProperty]
        private ObservableCollection<Model_Receiving_Entity_ReceivingLine> _receivingLines = new();
        
        // Constructor
        public ViewModel_Receiving_Wizard_Display_PONumberEntry(
            IService_Receiving_Business_MySQL_ReceivingLine receivingLineService,
            IService_Receiving_Business_MySQL_ReceivingTransaction receivingTransactionService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger) : base(errorHandler, logger)
        {
            _receivingLineService = receivingLineService;
            _receivingTransactionService = receivingTransactionService;
            _errorHandler = errorHandler;
            _logger = logger;
        }
        
        // Load Methods
        public async Task Load_Receiving_Wizard_Data_PONumberFromSessionAsync()
        {
            var session = await Get_Receiving_Wizard_Session_CurrentWorkflowAsync();
            PONumber = session?.PONumber ?? string.Empty;
        }
        
        public async Task Load_Receiving_Wizard_Data_PartDetailsFromERPAsync(string partNumber)
        {
            try
            {
                var result = await _receivingLineService.Get_Receiving_Database_Query_ReceivingLinesByPartAsync(partNumber);
                if (result.IsSuccess)
                {
                    ReceivingLines.Clear();
                    foreach (var line in result.Data)
                    {
                        ReceivingLines.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                await Handle_Receiving_Wizard_Error_ExceptionAsync(ex, nameof(Load_Receiving_Wizard_Data_PartDetailsFromERPAsync));
            }
        }
        
        // Save Methods
        public async Task Save_Receiving_Wizard_Data_PONumberToSessionAsync()
        {
            var session = await Get_Receiving_Wizard_Session_CurrentWorkflowAsync();
            session.PONumber = PONumber;
            await Update_Receiving_Wizard_Session_StateAsync(session);
        }
        
        // Validate Methods
        public async Task<bool> Validate_Receiving_Wizard_Input_PONumberFormatAsync()
        {
            if (IsNonPO)
            {
                return true; // PO Number not required for Non-PO
            }
            
            if (string.IsNullOrWhiteSpace(PONumber))
            {
                await Show_Receiving_Wizard_UI_ValidationErrorAsync("PO Number is required");
                return false;
            }
            
            if (!Helper_Validation_PONumber.Validate_Shared_Input_PONumberFormatAndStandardize(PONumber, out var standardized, out var error))
            {
                await Show_Receiving_Wizard_UI_ValidationErrorAsync(error);
                return false;
            }
            
            PONumber = standardized;
            return true;
        }
        
        public async Task<bool> Validate_Receiving_Wizard_Input_AllFieldsCompleteAsync()
        {
            var isPOValid = await Validate_Receiving_Wizard_Input_PONumberFormatAsync();
            var isPartValid = await Validate_Receiving_Wizard_Input_PartNumberFormatAsync();
            var isLoadCountValid = await Validate_Receiving_Wizard_Input_LoadCountRangeAsync();
            
            return isPOValid && isPartValid && isLoadCountValid;
        }
        
        // Clear Methods
        public async Task Clear_Receiving_Wizard_Data_AllFieldsAsync()
        {
            PONumber = string.Empty;
            PartNumber = string.Empty;
            LoadCount = 0;
            ReceivingLines.Clear();
            await Clear_Receiving_Wizard_UI_ValidationErrorsAsync();
        }
        
        // Navigation Methods
        [RelayCommand]
        public async Task Navigate_Receiving_Wizard_Navigation_ToNextStepAsync()
        {
            if (await Validate_Receiving_Wizard_Input_AllFieldsCompleteAsync())
            {
                await Save_Receiving_Wizard_Data_PONumberToSessionAsync();
                await Navigate_Receiving_Wizard_Navigation_ToStepTwoAsync();
            }
        }
        
        // UI Methods
        private async Task Show_Receiving_Wizard_UI_ValidationErrorAsync(string errorMessage)
        {
            _errorHandler.ShowUserError(errorMessage, "Validation Error", nameof(ViewModel_Receiving_Wizard_Display_PONumberEntry));
        }
        
        private async Task Clear_Receiving_Wizard_UI_ValidationErrorsAsync()
        {
            // Clear any displayed validation errors
        }
        
        // Helper Methods (private, can be shorter names if preferred)
        private async Task<Model_Receiving_Entity_WorkflowSession> Get_Receiving_Wizard_Session_CurrentWorkflowAsync()
        {
            // Get current workflow session
            return null; // placeholder
        }
        
        private async Task Update_Receiving_Wizard_Session_StateAsync(Model_Receiving_Entity_WorkflowSession session)
        {
            // Update session state
        }
        
        private async Task Handle_Receiving_Wizard_Error_ExceptionAsync(Exception ex, string methodName)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.Medium, methodName, nameof(ViewModel_Receiving_Wizard_Display_PONumberEntry));
        }
    }
}
```

## Compliance Rules

**MUST:**
- ✅ All classes use 5-part naming
- ✅ All methods use 5-part naming
- ✅ All files match class names exactly
- ✅ All async methods end with `Async`
- ✅ All database tables prefixed with `tbl_`
- ✅ All stored procedures prefixed with `sp_`
- ✅ All private fields prefixed with underscore
- ✅ All local variables use camelCase
- ✅ All properties use PascalCase

**MUST NOT:**
- ❌ Use vague names (Data, Info, Value, Thing)
- ❌ Use abbreviations except PO, CSV, SQL, ERP, UI
- ❌ Mix naming conventions
- ❌ Omit async suffix on async methods
- ❌ Use runtime binding in XAML (use x:Bind only)

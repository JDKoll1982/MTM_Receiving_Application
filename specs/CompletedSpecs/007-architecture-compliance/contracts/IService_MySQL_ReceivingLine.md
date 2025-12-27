# IService_MySQL_ReceivingLine Contract

**Type**: Service Interface  
**Namespace**: MTM_Receiving_Application.Contracts.Services  
**Purpose**: Abstraction layer providing ViewModels with access to receiving line data operations

---

## Interface Definition

```csharp
namespace MTM_Receiving_Application.Contracts.Services;

public interface IService_MySQL_ReceivingLine
{
    /// <summary>
    /// Inserts a new receiving line into the database
    /// </summary>
    /// <param name="line">Receiving line model with all required fields populated</param>
    /// <returns>Model_Dao_Result containing new line ID or failure</returns>
    Task<Model_Dao_Result<int>> InsertReceivingLineAsync(Model_ReceivingLine line);
    
    /// <summary>
    /// Retrieves all receiving lines for a specific receiving load
    /// </summary>
    /// <param name="loadId">Receiving load ID (foreign key)</param>
    /// <returns>Model_Dao_Result containing list of receiving lines or failure</returns>
    Task<Model_Dao_Result<List<Model_ReceivingLine>>> GetLinesByLoadIdAsync(int loadId);
    
    /// <summary>
    /// Updates an existing receiving line in the database
    /// </summary>
    /// <param name="line">Receiving line model with updated fields and original line_id</param>
    /// <returns>Model_Dao_Result indicating success or failure</returns>
    Task<Model_Dao_Result> UpdateReceivingLineAsync(Model_ReceivingLine line);
}
```

---

## Methods

### InsertReceivingLineAsync

**Purpose**: Create a new receiving line record in MySQL database.

**Parameters**:
- `line` (Model_ReceivingLine): Populated model with:
  - LoadId (required) - Foreign key to receiving_loads table
  - PoNumber (required) - Purchase order number
  - PoLine (required) - PO line number
  - PartNumber (required) - Part being received
  - Quantity (required) - Quantity received
  - PackageType (required) - "Package" or "Pallet"
  - LabelPrinted (optional) - Default false
  - ReceivedDate (optional) - Default NOW()

**Returns**: `Task<Model_Dao_Result<int>>`
- **Success**: Result.Data contains new auto-increment line_id
- **Failure**: Result.ErrorMessage describes validation error or database error

**Business Rules** (implemented in Service_MySQL_ReceivingLine):
- Validates LoadId > 0
- Validates PoNumber not empty
- Validates PartNumber not empty
- Validates Quantity > 0
- Validates PackageType is "Package" or "Pallet"
- Logs insertion for audit trail
- Transforms database errors to user-friendly messages

**Example Usage** (from ReceivingLabelViewModel):
```csharp
var newLine = new Model_ReceivingLine
{
    LoadId = CurrentLoadId,
    PoNumber = PoNumber,
    PoLine = PoLine,
    PartNumber = PartNumber,
    Quantity = ReceivedQuantity,
    PackageType = SelectedPackageType,
    ReceivedDate = DateTime.Now
};

var result = await _receivingLineService.InsertReceivingLineAsync(newLine);
if (result.IsSuccess)
{
    int newLineId = result.Data;
    StatusMessage = $"Line {newLineId} created successfully";
}
else
{
    _errorHandler.ShowUserError(result.ErrorMessage, "Insert Error", nameof(SaveLine));
}
```

---

### GetLinesByLoadIdAsync

**Purpose**: Retrieve all receiving lines associated with a specific receiving load.

**Parameters**:
- `loadId` (int): Receiving load ID (foreign key)

**Returns**: `Task<Model_Dao_Result<List<Model_ReceivingLine>>>`
- **Success**: Result.Data contains list of receiving lines (may be empty list if no lines)
- **Failure**: Result.ErrorMessage describes database error

**Business Rules**:
- Validates loadId > 0
- Returns empty list if no lines found (not a failure condition)
- Orders lines by line_id ascending
- Logs retrieval for audit trail

**Example Usage**:
```csharp
var result = await _receivingLineService.GetLinesByLoadIdAsync(selectedLoadId);
if (result.IsSuccess)
{
    ReceivingLines.Clear();
    foreach (var line in result.Data)
        ReceivingLines.Add(line);
    
    StatusMessage = $"Loaded {result.Data.Count} lines";
}
```

---

### UpdateReceivingLineAsync

**Purpose**: Update an existing receiving line record.

**Parameters**:
- `line` (Model_ReceivingLine): Model with original line_id and updated fields

**Returns**: `Task<Model_Dao_Result>`
- **Success**: Result.IsSuccess = true, line updated
- **Failure**: Result.ErrorMessage describes validation error or database error

**Business Rules**:
- Validates line.LineId > 0
- Validates updated fields (same rules as Insert)
- Logs update for audit trail
- Returns failure if line_id doesn't exist

**Example Usage**:
```csharp
existingLine.Quantity = updatedQuantity;
existingLine.PackageType = "Pallet";

var result = await _receivingLineService.UpdateReceivingLineAsync(existingLine);
if (result.IsSuccess)
{
    StatusMessage = "Line updated successfully";
}
```

---

## Implementation Class: Service_MySQL_ReceivingLine

**Location**: `Services/Receiving/Service_MySQL_ReceivingLine.cs`

**Dependencies** (constructor injection):
- `Dao_ReceivingLine` - Data access for receiving_lines table
- `ILoggingService` - Audit logging
- `IService_ErrorHandler` - Exception handling and user error display

**Pattern**:
```csharp
public class Service_MySQL_ReceivingLine : IService_MySQL_ReceivingLine
{
    private readonly Dao_ReceivingLine _receivingLineDao;
    private readonly ILoggingService _logger;
    private readonly IService_ErrorHandler _errorHandler;
    
    public Service_MySQL_ReceivingLine(
        Dao_ReceivingLine receivingLineDao,
        ILoggingService logger,
        IService_ErrorHandler errorHandler)
    {
        _receivingLineDao = receivingLineDao;
        _logger = logger;
        _errorHandler = errorHandler;
    }
    
    public async Task<Model_Dao_Result<int>> InsertReceivingLineAsync(Model_ReceivingLine line)
    {
        try
        {
            // BUSINESS VALIDATION
            if (line == null)
                return DaoResultFactory.Failure<int>("Receiving line cannot be null");
            
            if (line.LoadId <= 0)
                return DaoResultFactory.Failure<int>("Invalid load ID");
            
            if (string.IsNullOrWhiteSpace(line.PoNumber))
                return DaoResultFactory.Failure<int>("PO Number is required");
            
            if (line.Quantity <= 0)
                return DaoResultFactory.Failure<int>("Quantity must be greater than zero");
            
            if (line.PackageType != "Package" && line.PackageType != "Pallet")
                return DaoResultFactory.Failure<int>("Package type must be 'Package' or 'Pallet'");
            
            // DELEGATE TO DAO
            var result = await _receivingLineDao.InsertAsync(line);
            
            // LOGGING
            if (result.IsSuccess)
            {
                await _logger.LogInfoAsync("ReceivingLine", 
                    $"Inserted line {result.Data}: PO={line.PoNumber}, Part={line.PartNumber}, Qty={line.Quantity}");
            }
            else
            {
                await _logger.LogErrorAsync("ReceivingLine", 
                    $"Failed to insert line: {result.ErrorMessage}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Enum_ErrorSeverity.High, 
                nameof(InsertReceivingLineAsync), nameof(Service_MySQL_ReceivingLine));
            
            return DaoResultFactory.Failure<int>(
                "An error occurred while inserting the receiving line.", ex);
        }
    }
    
    // ... other method implementations
}
```

**DI Registration** (App.xaml.cs):
```csharp
services.AddTransient<IService_MySQL_ReceivingLine, Service_MySQL_ReceivingLine>();
```

---

## Consumers

### ReceivingLabelViewModel

**Current Violation**: Directly calls `Dao_ReceivingLine.InsertReceivingLineAsync()`

**Fixed Pattern**:
```csharp
public partial class ReceivingLabelViewModel : BaseViewModel
{
    private readonly IService_MySQL_ReceivingLine _receivingLineService;
    
    public ReceivingLabelViewModel(
        IService_MySQL_ReceivingLine receivingLineService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) : base(errorHandler, logger)
    {
        _receivingLineService = receivingLineService;
    }
    
    [RelayCommand]
    private async Task SaveReceivingLineAsync()
    {
        var line = new Model_ReceivingLine
        {
            LoadId = CurrentLoadId,
            PoNumber = PoNumber,
            PoLine = PoLine,
            PartNumber = PartNumber,
            Quantity = ReceivedQuantity,
            PackageType = SelectedPackageType
        };
        
        var result = await _receivingLineService.InsertReceivingLineAsync(line);
        
        if (result.IsSuccess)
        {
            StatusMessage = $"Line {result.Data} saved successfully";
            // Trigger label printing, etc.
        }
        else
        {
            _errorHandler.ShowUserError(
                result.ErrorMessage, 
                "Save Line Error", 
                nameof(SaveReceivingLineAsync));
        }
    }
}
```

---

## Testing Approach

### Unit Tests
- Mock `Dao_ReceivingLine` to return test data
- Verify validation logic (null line, invalid loadId, invalid quantity, invalid packageType)
- Verify logging calls for successful inserts
- Verify error transformation for database failures

### Integration Tests
- Test with real Dao_ReceivingLine instance against test database
- Verify INSERT creates new record with auto-increment ID
- Verify GetLinesByLoadId returns correct lines for load
- Verify UPDATE modifies existing line
- Verify foreign key constraint (LoadId must exist in receiving_loads)

---

## Related Documentation

- **Constitution**: Principle I (MVVM Architecture - ViewModel→Service→DAO pattern)
- **Service-DAO Pattern**: `.github/instructions/service-dao-pattern.instructions.md`
- **DAO Contract**: See `Dao_ReceivingLine.md` for data access layer contract

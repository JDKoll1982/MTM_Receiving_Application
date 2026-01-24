# MediatR Commands and Queries Contract

**Feature**: Receiving Workflow Consolidation  
**Date**: 2026-01-24  
**Type**: CQRS Command/Query Definitions

## Commands

All commands implement `IRequest<Result>` or `IRequest<Result<T>>` from MediatR.

### Workflow Management Commands

#### StartWorkflowCommand

**Purpose**: Initialize a new receiving workflow session

**Contract**:
```csharp
public record StartWorkflowCommand(
    string Mode  // "Guided" or "Manual"
) : IRequest<Result<Guid>>;
```

**Response**:
```csharp
Result<Guid>  // SessionId
```

**Validation**:
- Mode must be "Guided" or "Manual"

**Handler**: `StartWorkflowCommandHandler`

---

#### NavigateToStepCommand

**Purpose**: Navigate to a specific step in the workflow

**Contract**:
```csharp
public record NavigateToStepCommand(
    Guid SessionId,
    int TargetStep,      // 1, 2, or 3
    bool IsEditMode = false
) : IRequest<Result>;
```

**Response**:
```csharp
Result  // Success or error
```

**Validation**:
- SessionId must exist
- TargetStep must be 1, 2, or 3
- Current step must pass validation before advancing
- Cannot navigate to Step 3 without completing Step 2

**Handler**: `NavigateToStepCommandHandler`

---

### Step 1 Commands

#### UpdateStep1Command

**Purpose**: Update Step 1 data (PO, Part, Load Count)

**Contract**:
```csharp
public record UpdateStep1Command(
    Guid SessionId,
    string PONumber,
    int PartId,
    int LoadCount
) : IRequest<Result>;
```

**Response**:
```csharp
Result  // Success or error
```

**Validation**:
- SessionId must exist
- PONumber max length: 50 characters
- PONumber format: alphanumeric with hyphens allowed
- PartId must exist in database
- LoadCount must be between 1 and 100

**Side Effects**:
- Initializes LoadDetail records for LoadCount loads
- If LoadCount increases: adds new loads
- If LoadCount decreases: removes excess loads (with confirmation)

**Handler**: `UpdateStep1CommandHandler`

---

### Step 2 Commands

#### UpdateLoadDetailCommand

**Purpose**: Update data for a single load

**Contract**:
```csharp
public record UpdateLoadDetailCommand(
    Guid SessionId,
    int LoadNumber,
    decimal? WeightOrQuantity,
    string HeatLot,
    string PackageType,
    int? PackagesPerLoad
) : IRequest<Result>;
```

**Response**:
```csharp
Result  // Success or error
```

**Validation**:
- SessionId must exist
- LoadNumber must be between 1 and session.LoadCount
- WeightOrQuantity must be > 0 if provided
- HeatLot max length: 50 characters
- PackageType must be from predefined list if provided
- PackagesPerLoad must be > 0 if provided

**Side Effects**:
- Clears auto-fill flags for updated fields
- Triggers real-time validation

**Handler**: `UpdateLoadDetailCommandHandler`

---

#### CopyToLoadsCommand

**Purpose**: Copy data from source load to target loads (empty cells only)

**Contract**:
```csharp
public record CopyToLoadsCommand(
    Guid SessionId,
    int SourceLoadNumber,
    List<int> TargetLoadNumbers,  // Empty list = all loads
    CopyFields FieldsToCopy
) : IRequest<Result<CopyOperationResult>>;

public enum CopyFields
{
    AllFields,
    WeightOnly,
    HeatLotOnly,
    PackageTypeOnly,
    PackagesPerLoadOnly
}

public record CopyOperationResult(
    int SourceLoadNumber,
    int TotalTargetLoads,
    int CellsCopied,
    int CellsPreserved,
    List<int> LoadsWithPreservedData,
    bool Success,
    string Message
);
```

**Response**:
```csharp
Result<CopyOperationResult>
```

**Validation**:
- SessionId must exist
- SourceLoadNumber must be valid
- SourceLoadNumber must not have validation errors
- TargetLoadNumbers must be valid (if specified)
- FieldsToCopy must be a valid enum value

**Business Rules**:
- Only copy to **empty cells** (null or empty string)
- Never overwrite existing data
- Set auto-fill flags for copied cells
- Skip source load itself from targets

**Side Effects**:
- Updates LoadDetail records for target loads
- Sets auto-fill flags
- Updates LastCopyOperationAt timestamp

**Handler**: `CopyToLoadsCommandHandler`

---

#### ClearAutoFilledDataCommand

**Purpose**: Clear auto-filled data from loads

**Contract**:
```csharp
public record ClearAutoFilledDataCommand(
    Guid SessionId,
    List<int> TargetLoadNumbers,  // Empty list = all loads
    CopyFields FieldsToClear
) : IRequest<Result>;
```

**Response**:
```csharp
Result  // Success or error
```

**Validation**:
- SessionId must exist
- TargetLoadNumbers must be valid (if specified)
- FieldsToClear must be a valid enum value

**Business Rules**:
- Only clear fields where auto-fill flag is true
- Never clear manually entered data
- Clear auto-fill flags after clearing values

**Side Effects**:
- Updates LoadDetail records
- Clears auto-fill flags
- Triggers validation re-check

**Handler**: `ClearAutoFilledDataCommandHandler`

---

#### ForceOverwriteCommand

**Purpose**: Force overwrite occupied cells (power user feature)

**Contract**:
```csharp
public record ForceOverwriteCommand(
    Guid SessionId,
    int SourceLoadNumber,
    List<int> TargetLoadNumbers,
    CopyFields FieldsToOverwrite,
    bool Confirmed  // Must be true to execute
) : IRequest<Result<CopyOperationResult>>;
```

**Response**:
```csharp
Result<CopyOperationResult>
```

**Validation**:
- SessionId must exist
- SourceLoadNumber must be valid
- TargetLoadNumbers must be specified (cannot overwrite all)
- Confirmed must be true
- FieldsToOverwrite must be valid

**Business Rules**:
- Requires explicit user confirmation
- Can overwrite occupied cells
- Sets auto-fill flags for all copied cells
- Logs overwrite operations for audit

**Side Effects**:
- Updates LoadDetail records (overwrites existing data)
- Sets auto-fill flags
- Creates audit log entries

**Handler**: `ForceOverwriteCommandHandler`

---

#### ChangeCopySourceCommand

**Purpose**: Change which load is the copy source

**Contract**:
```csharp
public record ChangeCopySourceCommand(
    Guid SessionId,
    int NewSourceLoadNumber
) : IRequest<Result>;
```

**Response**:
```csharp
Result  // Success or error
```

**Validation**:
- SessionId must exist
- NewSourceLoadNumber must be between 1 and session.LoadCount

**Side Effects**:
- Updates session.CopySourceLoadNumber

**Handler**: `ChangeCopySourceCommandHandler`

---

### Step 3 Commands

#### SaveWorkflowCommand

**Purpose**: Save workflow data to database and CSV file

**Contract**:
```csharp
public record SaveWorkflowCommand(
    Guid SessionId,
    string CsvOutputPath,
    bool SaveToDatabase = true
) : IRequest<Result<SaveResult>>;

public record SaveResult(
    string CsvPath,
    int DatabaseRecordId,
    DateTime SavedAt
);
```

**Response**:
```csharp
Result<SaveResult>
```

**Validation**:
- SessionId must exist
- All loads must pass validation
- CsvOutputPath must be a valid file path
- Directory must be writable

**Business Rules**:
- Save to CSV (always)
- Save to MySQL database (if SaveToDatabase = true)
- Create transaction record
- Mark session as saved

**Side Effects**:
- Creates CSV file
- Inserts records into receiving_completed_transactions
- Updates session.IsSaved and session.SavedAt
- Clears HasUnsavedChanges flag

**Handler**: `SaveWorkflowCommandHandler`

---

## Queries

All queries implement `IRequest<Result<T>>` from MediatR.

### GetSessionQuery

**Purpose**: Retrieve current workflow session state

**Contract**:
```csharp
public record GetSessionQuery(
    Guid SessionId
) : IRequest<Result<ReceivingWorkflowSession>>;
```

**Response**:
```csharp
Result<ReceivingWorkflowSession>
```

**Handler**: `GetSessionQueryHandler`

---

### GetLoadDetailsQuery

**Purpose**: Retrieve all load details for a session

**Contract**:
```csharp
public record GetLoadDetailsQuery(
    Guid SessionId
) : IRequest<Result<List<LoadDetail>>>;
```

**Response**:
```csharp
Result<List<LoadDetail>>  // Ordered by LoadNumber
```

**Handler**: `GetLoadDetailsQueryHandler`

---

### GetValidationStatusQuery

**Purpose**: Get validation status for current or target step

**Contract**:
```csharp
public record GetValidationStatusQuery(
    Guid SessionId,
    int Step  // 1, 2, or 3
) : IRequest<Result<ValidationStatus>>;

public record ValidationStatus(
    bool IsValid,
    List<ValidationError> Errors,
    int ErrorCount,
    int WarningCount
);

public record ValidationError(
    string FieldName,
    int? LoadNumber,
    string ErrorMessage,
    ErrorSeverity Severity
);

public enum ErrorSeverity
{
    Info,
    Warning,
    Error
}
```

**Response**:
```csharp
Result<ValidationStatus>
```

**Handler**: `GetValidationStatusQueryHandler`

---

### GetCopyPreviewQuery

**Purpose**: Preview what a copy operation will do

**Contract**:
```csharp
public record GetCopyPreviewQuery(
    Guid SessionId,
    int SourceLoadNumber,
    List<int> TargetLoadNumbers,
    CopyFields FieldsToCopy
) : IRequest<Result<CopyPreview>>;

public record CopyPreview(
    int CellsToBeCopied,
    int CellsToBePreserved,
    Dictionary<int, List<string>> LoadsWithConflicts  // LoadNumber -> FieldNames
);
```

**Response**:
```csharp
Result<CopyPreview>
```

**Handler**: `GetCopyPreviewQueryHandler`

---

### GetPartLookupQuery

**Purpose**: Search for parts by PO number or part number

**Contract**:
```csharp
public record GetPartLookupQuery(
    string SearchTerm,
    string SearchType  // "PO" or "PartNumber"
) : IRequest<Result<List<PartInfo>>>;

public record PartInfo(
    int PartId,
    string PartNumber,
    string Description,
    string PONumber,
    string VendorName
);
```

**Response**:
```csharp
Result<List<PartInfo>>
```

**Data Source**: Infor Visual SQL Server (READ ONLY)

**Handler**: `GetPartLookupQueryHandler`

---

## Validation Rules

All commands use FluentValidation validators. Example:

### StartWorkflowCommandValidator

```csharp
public class StartWorkflowCommandValidator : AbstractValidator<StartWorkflowCommand>
{
    public StartWorkflowCommandValidator()
    {
        RuleFor(x => x.Mode)
            .NotEmpty()
            .Must(m => m == "Guided" || m == "Manual")
            .WithMessage("Mode must be either 'Guided' or 'Manual'");
    }
}
```

### UpdateStep1CommandValidator

```csharp
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
            
        RuleFor(x => x.PartId)
            .GreaterThan(0)
            .WithMessage("PartId must be valid");
            
        RuleFor(x => x.LoadCount)
            .InclusiveBetween(1, 100)
            .WithMessage("Load count must be between 1 and 100");
    }
}
```

## Error Handling

All handlers follow consistent error handling:

```csharp
public class SomeCommandHandler : IRequestHandler<SomeCommand, Result>
{
    public async Task<Result> Handle(SomeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validation
            if (!await ValidateAsync(request))
                return Result.Failure("Validation failed");
            
            // Business logic
            await ExecuteBusinessLogicAsync(request);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error handling {Command}", nameof(SomeCommand));
            return Result.Failure($"Error: {ex.Message}");
        }
    }
}
```

## Testing Contracts

Each command/query handler must have:

1. **Unit tests**: Test logic with mocked dependencies
2. **Validation tests**: Test all validation rules
3. **Integration tests**: Test database operations (for DAOs)

Example test structure:

```csharp
public class UpdateStep1CommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_UpdatesSession()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new UpdateStep1Command(...);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task Handle_InvalidLoadCount_ReturnsFailure()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new UpdateStep1Command(loadCount: 0);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Load count");
    }
}
```

## Pipeline Behaviors

MediatR pipeline behaviors for cross-cutting concerns:

### ValidationBehavior

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Automatically runs FluentValidation validators before command execution
}
```

### LoggingBehavior

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Logs command/query execution with timing
}
```

### TransactionBehavior

```csharp
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // Wraps database operations in transaction
}
```

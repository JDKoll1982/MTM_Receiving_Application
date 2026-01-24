# Data Model: Receiving Workflow Consolidation

**Feature**: Receiving Workflow Consolidation  
**Date**: 2026-01-24  
**Phase**: 1 - Design

## Entity Definitions

### Receiving Workflow Session

Represents a single receiving transaction workflow session.

```csharp
public class ReceivingWorkflowSession
{
    // Identity
    public Guid SessionId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastModifiedAt { get; set; }
    
    // Workflow State
    public int CurrentStep { get; set; }  // 1, 2, or 3
    public bool IsEditMode { get; set; }
    public bool HasUnsavedChanges { get; set; }
    
    // Step 1 Data
    public string PONumber { get; set; }
    public int? PartId { get; set; }
    public string PartNumber { get; set; }
    public string PartDescription { get; set; }
    public int LoadCount { get; set; }
    
    // Step 2 Data Collection
    public List<LoadDetail> Loads { get; init; } = new();
    
    // Copy Operation State
    public int CopySourceLoadNumber { get; set; } = 1;
    public DateTime? LastCopyOperationAt { get; set; }
    
    // Validation State
    public List<ValidationError> ValidationErrors { get; init; } = new();
    
    // Save State
    public bool IsSaved { get; set; }
    public DateTime? SavedAt { get; set; }
    public string SavedCsvPath { get; set; }
}
```

**Validation Rules**:
- SessionId must be unique
- CurrentStep must be 1, 2, or 3
- LoadCount must be > 0 and <= 100
- PONumber required if not in non-PO mode
- PartId required for PO mode
- Loads.Count must equal LoadCount

**State Transitions**:
- Created → Step 1 (default)
- Step 1 → Step 2 (when validated)
- Step 2 → Step 3 (when all loads validated)
- Step 3 → Step 2 (edit mode)
- Step 3 → Saved (final state)

### Load Detail

Represents data for a single load in a receiving transaction.

```csharp
public class LoadDetail
{
    // Identity
    public int LoadNumber { get; init; }  // 1-based sequence
    
    // Data Fields
    public decimal? WeightOrQuantity { get; set; }
    public string HeatLot { get; set; }
    public string PackageType { get; set; }
    public int? PackagesPerLoad { get; set; }
    
    // Metadata
    public bool IsWeightAutoFilled { get; set; }
    public bool IsHeatLotAutoFilled { get; set; }
    public bool IsPackageTypeAutoFilled { get; set; }
    public bool IsPackagesPerLoadAutoFilled { get; set; }
    
    // Validation
    public List<string> ValidationErrors { get; init; } = new();
    public bool IsValid => ValidationErrors.Count == 0;
}
```

**Validation Rules**:
- LoadNumber must be unique within session
- WeightOrQuantity must be > 0 if provided
- HeatLot max length: 50 characters
- PackageType must be from predefined list if provided
- PackagesPerLoad must be > 0 if provided

**Field Dependencies**:
- None - all fields are independent

### Validation Error

Represents a validation error in the workflow.

```csharp
public class ValidationError
{
    public string FieldName { get; init; }
    public int? LoadNumber { get; init; }  // Null for session-level errors
    public string ErrorMessage { get; init; }
    public ErrorSeverity Severity { get; init; }
}

public enum ErrorSeverity
{
    Info,
    Warning,
    Error
}
```

### Copy Operation Result

Represents the result of a bulk copy operation.

```csharp
public class CopyOperationResult
{
    public int SourceLoadNumber { get; init; }
    public int TotalTargetLoads { get; init; }
    public int CellsCopied { get; init; }
    public int CellsPreserved { get; init; }
    public List<int> LoadsWithPreservedData { get; init; } = new();
    public bool Success { get; init; }
    public string Message { get; init; }
}
```

## Command & Query Models (CQRS)

### Commands

#### StartWorkflowCommand

```csharp
public record StartWorkflowCommand(
    string Mode  // "Guided" or "Manual"
) : IRequest<Result<Guid>>;  // Returns SessionId
```

#### UpdateStep1Command

```csharp
public record UpdateStep1Command(
    Guid SessionId,
    string PONumber,
    int PartId,
    int LoadCount
) : IRequest<Result>;
```

#### UpdateLoadDetailCommand

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

#### CopyToLoadsCommand

```csharp
public record CopyToLoadsCommand(
    Guid SessionId,
    int SourceLoadNumber,
    List<int> TargetLoadNumbers,  // Empty = all loads
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
```

#### ClearAutoFilledDataCommand

```csharp
public record ClearAutoFilledDataCommand(
    Guid SessionId,
    List<int> TargetLoadNumbers,  // Empty = all loads
    CopyFields FieldsToClear
) : IRequest<Result>;
```

#### NavigateToStepCommand

```csharp
public record NavigateToStepCommand(
    Guid SessionId,
    int TargetStep,
    bool IsEditMode = false
) : IRequest<Result>;
```

#### SaveWorkflowCommand

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

### Queries

#### GetSessionQuery

```csharp
public record GetSessionQuery(
    Guid SessionId
) : IRequest<Result<ReceivingWorkflowSession>>;
```

#### GetLoadDetailsQuery

```csharp
public record GetLoadDetailsQuery(
    Guid SessionId
) : IRequest<Result<List<LoadDetail>>>;
```

#### GetValidationStatusQuery

```csharp
public record GetValidationStatusQuery(
    Guid SessionId,
    int Step
) : IRequest<Result<ValidationStatus>>;

public record ValidationStatus(
    bool IsValid,
    List<ValidationError> Errors,
    int ErrorCount,
    int WarningCount
);
```

#### GetCopyPreviewQuery

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

## Database Schema

### MySQL Tables

#### receiving_workflow_sessions

```sql
CREATE TABLE receiving_workflow_sessions (
    session_id CHAR(36) PRIMARY KEY,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_modified_at DATETIME,
    current_step TINYINT NOT NULL,
    is_edit_mode BOOLEAN NOT NULL DEFAULT FALSE,
    has_unsaved_changes BOOLEAN NOT NULL DEFAULT TRUE,
    po_number VARCHAR(50),
    part_id INT,
    part_number VARCHAR(50),
    part_description VARCHAR(200),
    load_count INT NOT NULL,
    copy_source_load_number INT NOT NULL DEFAULT 1,
    last_copy_operation_at DATETIME,
    is_saved BOOLEAN NOT NULL DEFAULT FALSE,
    saved_at DATETIME,
    saved_csv_path VARCHAR(500),
    INDEX idx_created_at (created_at),
    INDEX idx_po_number (po_number),
    INDEX idx_saved_at (saved_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

#### receiving_load_details

```sql
CREATE TABLE receiving_load_details (
    id INT AUTO_INCREMENT PRIMARY KEY,
    session_id CHAR(36) NOT NULL,
    load_number INT NOT NULL,
    weight_or_quantity DECIMAL(10,3),
    heat_lot VARCHAR(50),
    package_type VARCHAR(50),
    packages_per_load INT,
    is_weight_auto_filled BOOLEAN NOT NULL DEFAULT FALSE,
    is_heat_lot_auto_filled BOOLEAN NOT NULL DEFAULT FALSE,
    is_package_type_auto_filled BOOLEAN NOT NULL DEFAULT FALSE,
    is_packages_per_load_auto_filled BOOLEAN NOT NULL DEFAULT FALSE,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (session_id) REFERENCES receiving_workflow_sessions(session_id) ON DELETE CASCADE,
    UNIQUE KEY unique_session_load (session_id, load_number),
    INDEX idx_session_id (session_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

#### receiving_completed_transactions

Final saved transactions (persisted after workflow completion).

```sql
CREATE TABLE receiving_completed_transactions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    session_id CHAR(36) NOT NULL,
    po_number VARCHAR(50),
    part_id INT,
    part_number VARCHAR(50),
    load_number INT NOT NULL,
    weight_or_quantity DECIMAL(10,3),
    heat_lot VARCHAR(50),
    package_type VARCHAR(50),
    packages_per_load INT,
    csv_file_path VARCHAR(500),
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(50),
    INDEX idx_po_number (po_number),
    INDEX idx_part_number (part_number),
    INDEX idx_created_at (created_at),
    INDEX idx_session_id (session_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

## Stored Procedures

### sp_Create_Receiving_Session

```sql
CREATE PROCEDURE sp_Create_Receiving_Session(
    IN p_session_id CHAR(36),
    IN p_po_number VARCHAR(50),
    IN p_part_id INT,
    IN p_load_count INT
)
BEGIN
    INSERT INTO receiving_workflow_sessions (
        session_id, po_number, part_id, load_count, current_step
    ) VALUES (
        p_session_id, p_po_number, p_part_id, p_load_count, 1
    );
    
    SELECT session_id, created_at FROM receiving_workflow_sessions 
    WHERE session_id = p_session_id;
END;
```

### sp_Update_Load_Detail

```sql
CREATE PROCEDURE sp_Update_Load_Detail(
    IN p_session_id CHAR(36),
    IN p_load_number INT,
    IN p_weight_or_quantity DECIMAL(10,3),
    IN p_heat_lot VARCHAR(50),
    IN p_package_type VARCHAR(50),
    IN p_packages_per_load INT
)
BEGIN
    INSERT INTO receiving_load_details (
        session_id, load_number, weight_or_quantity, heat_lot, 
        package_type, packages_per_load
    ) VALUES (
        p_session_id, p_load_number, p_weight_or_quantity, p_heat_lot,
        p_package_type, p_packages_per_load
    )
    ON DUPLICATE KEY UPDATE
        weight_or_quantity = VALUES(weight_or_quantity),
        heat_lot = VALUES(heat_lot),
        package_type = VALUES(package_type),
        packages_per_load = VALUES(packages_per_load),
        updated_at = CURRENT_TIMESTAMP;
END;
```

### sp_Get_Session_With_Loads

```sql
CREATE PROCEDURE sp_Get_Session_With_Loads(
    IN p_session_id CHAR(36)
)
BEGIN
    -- Get session data
    SELECT * FROM receiving_workflow_sessions WHERE session_id = p_session_id;
    
    -- Get all load details
    SELECT * FROM receiving_load_details 
    WHERE session_id = p_session_id 
    ORDER BY load_number;
END;
```

### sp_Save_Completed_Transaction

```sql
CREATE PROCEDURE sp_Save_Completed_Transaction(
    IN p_session_id CHAR(36),
    IN p_csv_file_path VARCHAR(500),
    IN p_created_by VARCHAR(50)
)
BEGIN
    -- Insert all loads from session into completed transactions
    INSERT INTO receiving_completed_transactions (
        session_id, po_number, part_id, part_number, load_number,
        weight_or_quantity, heat_lot, package_type, packages_per_load,
        csv_file_path, created_by
    )
    SELECT 
        s.session_id, s.po_number, s.part_id, s.part_number, l.load_number,
        l.weight_or_quantity, l.heat_lot, l.package_type, l.packages_per_load,
        p_csv_file_path, p_created_by
    FROM receiving_workflow_sessions s
    INNER JOIN receiving_load_details l ON s.session_id = l.session_id
    WHERE s.session_id = p_session_id;
    
    -- Update session as saved
    UPDATE receiving_workflow_sessions
    SET is_saved = TRUE, saved_at = CURRENT_TIMESTAMP, saved_csv_path = p_csv_file_path
    WHERE session_id = p_session_id;
    
    SELECT ROW_COUNT() as rows_saved;
END;
```

## ViewModel State Models

### ReceivingWorkflowViewModel

Main ViewModel managing the entire workflow.

```csharp
public partial class ReceivingWorkflowViewModel : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    
    // Observable Properties
    [ObservableProperty] private int _currentStep = 1;
    [ObservableProperty] private bool _isEditMode;
    [ObservableProperty] private bool _hasUnsavedChanges;
    
    // Step 1 Properties
    [ObservableProperty] private string _poNumber;
    [ObservableProperty] private int? _selectedPartId;
    [ObservableProperty] private string _partDescription;
    [ObservableProperty] private int _loadCount = 1;
    
    // Step 2 Properties
    [ObservableProperty] private ObservableCollection<LoadDetailViewModel> _loads;
    [ObservableProperty] private int _copySourceLoadNumber = 1;
    [ObservableProperty] private LoadDetailViewModel _selectedLoad;
    
    // Validation
    [ObservableProperty] private ObservableCollection<string> _validationErrors;
    [ObservableProperty] private bool _canNavigateNext;
    
    // Commands
    [RelayCommand] private async Task NavigateNextAsync();
    [RelayCommand] private async Task NavigatePreviousAsync();
    [RelayCommand] private async Task SaveAsync();
    [RelayCommand] private async Task CopyToAllLoadsAsync(CopyFields fields);
    [RelayCommand] private async Task ClearAutoFilledAsync(CopyFields fields);
}
```

### LoadDetailViewModel

ViewModel for individual load data.

```csharp
public partial class LoadDetailViewModel : ObservableObject
{
    [ObservableProperty] private int _loadNumber;
    [ObservableProperty] private decimal? _weightOrQuantity;
    [ObservableProperty] private string _heatLot;
    [ObservableProperty] private string _packageType;
    [ObservableProperty] private int? _packagesPerLoad;
    
    // Auto-fill tracking
    [ObservableProperty] private bool _isWeightAutoFilled;
    [ObservableProperty] private bool _isHeatLotAutoFilled;
    [ObservableProperty] private bool _isPackageTypeAutoFilled;
    [ObservableProperty] private bool _isPackagesPerLoadAutoFilled;
    
    // Validation
    [ObservableProperty] private bool _hasErrors;
    [ObservableProperty] private ObservableCollection<string> _errorMessages;
    [ObservableProperty] private bool _isSelected;
}
```

## Relationships and Cardinality

```
ReceivingWorkflowSession 1 ──< * LoadDetail
ReceivingWorkflowSession 1 ──< * ValidationError
```

- One session has many load details (1 to LoadCount)
- One session has many validation errors (0 to many)
- Load details belong to exactly one session
- Validation errors belong to exactly one session

## Data Flow Summary

1. **Session Creation**: StartWorkflowCommand → ReceivingWorkflowSession created
2. **Step 1 Entry**: UpdateStep1Command → Session updated with PO/Part/LoadCount → LoadDetails initialized
3. **Step 2 Entry**: UpdateLoadDetailCommand (per load) → LoadDetail records created/updated
4. **Bulk Copy**: CopyToLoadsCommand → Multiple LoadDetails updated, auto-fill flags set
5. **Validation**: GetValidationStatusQuery → ValidationErrors collected
6. **Save**: SaveWorkflowCommand → Data persisted to receiving_completed_transactions + CSV export

## Migration Strategy

No existing data migration required - this is a new workflow implementation that coexists with the legacy 12-step wizard.

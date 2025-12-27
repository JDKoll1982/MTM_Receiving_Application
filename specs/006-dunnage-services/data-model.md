# Data Model: Dunnage Services Layer

**Feature**: Dunnage Services Layer  
**Date**: 2025-12-26  
**Phase**: Phase 1 - Design

## Overview

This document defines the enums and result models introduced by the dunnage services layer. These models support service operations and enable clean communication between services and ViewModels.

## Enums

### Enum_DunnageWorkflowStep
**Purpose**: Represents the current step in the dunnage receiving wizard workflow.  
**Location**: `Models/Enums/Enum_DunnageWorkflowStep.cs`

```csharp
public enum Enum_DunnageWorkflowStep
{
    ModeSelection = 0,    // Initial step: choose Receiving or Inventory mode
    TypeSelection = 1,    // Select dunnage type (Pallet, Divider, etc.)
    PartSelection = 2,    // Select specific part/spec
    QuantityEntry = 3,    // Enter quantity received
    DetailsEntry = 4,     // Enter additional details (PO number, notes, etc.)
    Review = 5            // Review session before saving
}
```

**Usage**: Workflow service tracks current step, ViewModels use for conditional rendering.

---

## Service Result Models

### Model_WorkflowStepResult
**Purpose**: Result of a workflow step transition attempt.  
**Location**: `Models/Receiving/Model_WorkflowStepResult.cs`

```csharp
public class Model_WorkflowStepResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public Enum_DunnageWorkflowStep? TargetStep { get; set; }
}
```

**Fields**:
- `IsSuccess`: Whether the transition was successful
- `ErrorMessage`: Reason for failure (e.g., "Type must be selected before advancing to Part selection")
- `TargetStep`: The step transitioned to (if successful)

**Example Usage**:
```csharp
var result = await _workflow.AdvanceToNextStepAsync();
if (result.IsSuccess) {
    // Navigate to next page corresponding to result.TargetStep
} else {
    // Show result.ErrorMessage
}
```

---

### Model_SaveResult
**Purpose**: Result of a session save operation (persisting loads + CSV export).  
**Location**: `Models/Receiving/Model_SaveResult.cs`

```csharp
public class Model_SaveResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int RecordsSaved { get; set; }
    public Model_CSVWriteResult? CSVExportResult { get; set; }
}
```

**Fields**:
- `IsSuccess`: Whether the save operation succeeded
- `ErrorMessage`: Combined error message if any failures
- `RecordsSaved`: Number of load records persisted to database
- `CSVExportResult`: Nested result object for CSV export portion

**Example Usage**:
```csharp
var result = await _workflow.SaveSessionAsync();
if (result.IsSuccess) {
    StatusMessage = $"Saved {result.RecordsSaved} loads. CSV: {result.CSVExportResult.LocalFilePath}";
} else {
    ErrorHandler.ShowError(result.ErrorMessage);
}
```

---

### Model_CSVWriteResult
**Purpose**: Result of a CSV export operation (local + network write).  
**Location**: `Models/Receiving/Model_CSVWriteResult.cs`

```csharp
public class Model_CSVWriteResult
{
    public bool LocalSuccess { get; set; }
    public bool NetworkSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string LocalFilePath { get; set; } = string.Empty;
    public string NetworkFilePath { get; set; } = string.Empty;
}
```

**Fields**:
- `LocalSuccess`: Whether write to `%APPDATA%\MTM_Receiving_Application\DunnageData.csv` succeeded
- `NetworkSuccess`: Whether write to network share succeeded (best-effort)
- `ErrorMessage`: Combined error message (network failures logged but don't fail operation)
- `LocalFilePath`: Full path to local CSV file
- `NetworkFilePath`: Full path to network CSV file (if successful)

**Example Usage**:
```csharp
var result = await _csvWriter.WriteToCSVAsync(loads);
if (result.LocalSuccess) {
    StatusMessage = "CSV created locally";
    if (!result.NetworkSuccess) {
        _logger.LogWarning("Network CSV write failed: " + result.ErrorMessage);
    }
} else {
    throw new Exception("Critical: Local CSV write failed"); // Should never happen
}
```

---

## Existing Models (Reference)

These models are defined in `specs/CompletedSpecs/005-dunnage-stored-procedures` and used heavily by the services.

### Model_DunnageType
- `TypeID` (int, PK)
- `TypeName` (string)
- `Description` (string)
- `RequiresPO` (bool)
- `DefaultSpecs` (JSON string)

### Model_DunnageSpec
- `SpecID` (int, PK)
- `TypeID` (int, FK)
- `SpecSchema` (JSON string)
- `SpecVersion` (int)

### Model_DunnagePart
- `PartID` (string, PK)
- `TypeID` (int, FK)
- `Description` (string)
- `DefaultQty` (decimal)
- `SpecValues` (JSON string)

### Model_DunnageLoad
- `LoadUUID` (string, PK)
- `PartID` (string, FK)
- `Quantity` (decimal)
- `PONumber` (string, nullable)
- `ReceivedDate` (DateTime)
- `Employee` (int)
- `SpecValues` (JSON string)

### Model_InventoriedDunnage
- `PartID` (string, PK)
- `Description` (string)
- `Location` (string)
- `ReorderPoint` (decimal)
- `ReorderQty` (decimal)

### Model_DunnageSession
**Purpose**: Workflow state object maintained by `Service_DunnageWorkflow`.  
**Location**: `Models/Receiving/Model_DunnageSession.cs` (already exists)

```csharp
public class Model_DunnageSession
{
    public Enum_DunnageWorkflowStep CurrentStep { get; set; }
    public Model_DunnageType? SelectedType { get; set; }
    public Model_DunnagePart? SelectedPart { get; set; }
    public decimal Quantity { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public Dictionary<string, string> SpecValues { get; set; } = new();
    public List<Model_DunnageLoad> PendingLoads { get; set; } = new();
}
```

---

## Data Flow Diagram

```
User Input (ViewModel)
    ↓
Service_DunnageWorkflow.AdvanceToNextStepAsync()
    → Validate CurrentSession data
    → Update CurrentStep
    → Return Model_WorkflowStepResult
    ↓
ViewModel checks result.IsSuccess
    → If success: Navigate to result.TargetStep page
    → If failure: Display result.ErrorMessage

---

User clicks Save (ViewModel)
    ↓
Service_DunnageWorkflow.SaveSessionAsync()
    → Service_MySQL_Dunnage.SaveLoadsAsync(session.PendingLoads)
        → DAO persists to database
        → Return Model_Dao_Result
    → Service_DunnageCSVWriter.WriteToCSVAsync(session.PendingLoads)
        → Write to local %APPDATA%
        → Best-effort write to network share
        → Return Model_CSVWriteResult
    → Return Model_SaveResult (combines both results)
    ↓
ViewModel checks result.IsSuccess
    → If success: Show "{result.RecordsSaved} loads saved"
    → If failure: Show result.ErrorMessage
```

---

## Validation Rules

### Model_WorkflowStepResult
- `IsSuccess = false` if `ErrorMessage` is not empty
- `TargetStep` is null when `IsSuccess = false`

### Model_SaveResult
- `IsSuccess = false` if database save fails OR local CSV write fails
- Network CSV write failure does NOT set `IsSuccess = false`
- `CSVExportResult` is always populated (even if database save failed)

### Model_CSVWriteResult
- `LocalSuccess` MUST be true for operation to be considered successful
- `NetworkSuccess` can be false without failing the operation
- `ErrorMessage` contains details of network failure (if any)

---

## Relationships

```
Enum_DunnageWorkflowStep
    ↓ (used by)
Model_WorkflowStepResult.TargetStep
    ↓ (used by)
Service_DunnageWorkflow

Model_CSVWriteResult
    ↓ (nested in)
Model_SaveResult.CSVExportResult
    ↓ (returned by)
Service_DunnageWorkflow.SaveSessionAsync()
```

---

## Testing Considerations

- **Model_WorkflowStepResult**: Unit test validation logic (IsSuccess vs ErrorMessage presence)
- **Model_SaveResult**: Integration test combining database + CSV operations
- **Model_CSVWriteResult**: Unit test local vs network failure scenarios
- **Enum_DunnageWorkflowStep**: Test step sequencing logic in workflow service

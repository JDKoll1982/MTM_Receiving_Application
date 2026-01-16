# API Contracts: Module_Volvo CQRS Modernization

**Feature**: Module_Volvo CQRS Modernization  
**Branch**: `001-volvo-modernization`  
**Date**: January 16, 2026

## Overview

This document defines the command and query contracts (request/response DTOs) for Module_Volvo CQRS handlers. All handlers use MediatR `IRequest<TResponse>` pattern with `Model_Dao_Result<T>` as the standard response type.

---

## Command Contracts

### 1. AddPartToShipmentCommand

**Purpose**: Adds a part to the current shipment (in-memory, not persisted until save/complete).

**Request**:
```csharp
public record AddPartToShipmentCommand : IRequest<Model_Dao_Result>
{
    public string PartNumber { get; init; }
    public int ReceivedSkidCount { get; init; }
    public int? ExpectedSkidCount { get; init; }
    public bool HasDiscrepancy { get; init; }
    public string DiscrepancyNote { get; init; }
    public int? PendingShipmentId { get; init; }
}
```

**Response**: `Model_Dao_Result` (success/failure with error message)

**Validation**:
- PartNumber: Required, must exist in volvo_master_data
- ReceivedSkidCount: > 0
- If HasDiscrepancy = true: ExpectedSkidCount and DiscrepancyNote required

---

### 2. RemovePartFromShipmentCommand

**Purpose**: Removes a part from the current shipment (in-memory operation).

**Request**:
```csharp
public record RemovePartFromShipmentCommand : IRequest<Model_Dao_Result>
{
    public string PartNumber { get; init; }
}
```

**Response**: `Model_Dao_Result`

**Validation**:
- PartNumber: Required, must exist in current shipment part list

---

### 3. SavePendingShipmentCommand

**Purpose**: Saves shipment as pending (allows resuming later).

**Request**:
```csharp
public record SavePendingShipmentCommand : IRequest<Model_Dao_Result<int>>
{
    public int? ShipmentId { get; init; }
    public DateTimeOffset ShipmentDate { get; init; }
    public int ShipmentNumber { get; init; }
    public string Notes { get; init; }
    public List<ShipmentLineDto> Parts { get; init; }
}
```

**Response**: `Model_Dao_Result<int>` (ShipmentId)

**Validation**:
- ShipmentDate: Required, <= DateTimeOffset.Now
- Parts: At least one part required

---

### 4. CompleteShipmentCommand

**Purpose**: Finalizes shipment, generates labels, sends email.

**Request**:
```csharp
public record CompleteShipmentCommand : IRequest<Model_Dao_Result<int>>
{
    public DateTimeOffset ShipmentDate { get; init; }
    public int ShipmentNumber { get; init; }
    public string Notes { get; init; }
    public List<ShipmentLineDto> Parts { get; init; }
    public string PONumber { get; init; }
    public string ReceiverNumber { get; init; }
}
```

**Response**: `Model_Dao_Result<int>` (ShipmentId)

**Validation**:
- ShipmentDate: Required, <= DateTimeOffset.Now
- Parts: At least one part required
- All parts validated per AddPartToShipmentCommand rules

---

### 5. UpdateShipmentCommand

**Purpose**: Updates existing shipment (edit from history view).

**Request**:
```csharp
public record UpdateShipmentCommand : IRequest<Model_Dao_Result>
{
    public int ShipmentId { get; init; }
    public DateTimeOffset ShipmentDate { get; init; }
    public string Notes { get; init; }
    public string PONumber { get; init; }
    public string ReceiverNumber { get; init; }
    public List<ShipmentLineDto> Parts { get; init; }
}
```

**Response**: `Model_Dao_Result`

**Validation**:
- ShipmentId: > 0
- Parts: At least one part required

---

### 6. AddVolvoPartCommand

**Purpose**: Adds new part to master data.

**Request**:
```csharp
public record AddVolvoPartCommand : IRequest<Model_Dao_Result<int>>
{
    public string PartNumber { get; init; }
    public double QuantityPerSkid { get; init; }
}
```

**Response**: `Model_Dao_Result<int>` (PartId)

**Validation**:
- PartNumber: Required, unique
- QuantityPerSkid: > 0

---

### 7. UpdateVolvoPartCommand

**Purpose**: Updates existing part in master data.

**Request**:
```csharp
public record UpdateVolvoPartCommand : IRequest<Model_Dao_Result>
{
    public int PartId { get; init; }
    public string PartNumber { get; init; }
    public double QuantityPerSkid { get; init; }
}
```

**Response**: `Model_Dao_Result`

**Validation**:
- PartId: > 0
- PartNumber: Required
- QuantityPerSkid: > 0

---

### 8. DeactivateVolvoPartCommand

**Purpose**: Deactivates part in master data.

**Request**:
```csharp
public record DeactivateVolvoPartCommand : IRequest<Model_Dao_Result>
{
    public int PartId { get; init; }
}
```

**Response**: `Model_Dao_Result`

**Validation**:
- PartId: > 0
- Cannot deactivate if referenced by pending shipments

---

### 9. ImportPartsCsvCommand

**Purpose**: Bulk imports parts from CSV file.

**Request**:
```csharp
public record ImportPartsCsvCommand : IRequest<Model_Dao_Result<ImportPartsCsvResult>>
{
    public string CsvFilePath { get; init; }
}

public record ImportPartsCsvResult
{
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<string> Errors { get; init; }
}
```

**Response**: `Model_Dao_Result<ImportPartsCsvResult>`

**Validation**:
- CsvFilePath: Required, must exist
- CSV format: PartNumber, QuantityPerSkid (validated per-row)

---

## Query Contracts

### 1. GetInitialShipmentDataQuery

**Purpose**: Gets initial data for new shipment entry (current date + next shipment number).

**Request**:
```csharp
public record GetInitialShipmentDataQuery : IRequest<Model_Dao_Result<InitialShipmentData>>
{
}

public record InitialShipmentData
{
    public DateTimeOffset CurrentDate { get; init; }
    public int NextShipmentNumber { get; init; }
}
```

**Response**: `Model_Dao_Result<InitialShipmentData>`

---

### 2. GetPendingShipmentQuery

**Purpose**: Retrieves pending shipment for current user (to resume entry).

**Request**:
```csharp
public record GetPendingShipmentQuery : IRequest<Model_Dao_Result<Model_VolvoShipment>>
{
    public string UserName { get; init; }
}
```

**Response**: `Model_Dao_Result<Model_VolvoShipment>` (null if no pending shipment)

---

### 3. SearchVolvoPartsQuery

**Purpose**: Autocomplete search for part numbers.

**Request**:
```csharp
public record SearchVolvoPartsQuery : IRequest<Model_Dao_Result<List<Model_VolvoPart>>>
{
    public string SearchText { get; init; }
    public int MaxResults { get; init; } = 10;
}
```

**Response**: `Model_Dao_Result<List<Model_VolvoPart>>`

---

### 4. GenerateLabelCsvQuery

**Purpose**: Generates CSV label file for shipment.

**Request**:
```csharp
public record GenerateLabelCsvQuery : IRequest<Model_Dao_Result<string>>
{
    public int ShipmentId { get; init; }
}
```

**Response**: `Model_Dao_Result<string>` (CSV content as string)

---

### 5. FormatEmailDataQuery

**Purpose**: Formats email notification data (HTML + plain text).

**Request**:
```csharp
public record FormatEmailDataQuery : IRequest<Model_Dao_Result<Model_VolvoEmailData>>
{
    public int ShipmentId { get; init; }
}
```

**Response**: `Model_Dao_Result<Model_VolvoEmailData>`

---

### 6. GetRecentShipmentsQuery

**Purpose**: Gets recent shipments for history view.

**Request**:
```csharp
public record GetRecentShipmentsQuery : IRequest<Model_Dao_Result<List<Model_VolvoShipment>>>
{
    public int Days { get; init; } = 30;
}
```

**Response**: `Model_Dao_Result<List<Model_VolvoShipment>>`

---

### 7. GetShipmentHistoryQuery

**Purpose**: Gets filtered shipment history.

**Request**:
```csharp
public record GetShipmentHistoryQuery : IRequest<Model_Dao_Result<List<Model_VolvoShipment>>>
{
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public VolvoShipmentStatus? StatusFilter { get; init; }
}
```

**Response**: `Model_Dao_Result<List<Model_VolvoShipment>>`

---

### 8. GetShipmentDetailQuery

**Purpose**: Gets shipment with line items for detail/edit view.

**Request**:
```csharp
public record GetShipmentDetailQuery : IRequest<Model_Dao_Result<ShipmentDetail>>
{
    public int ShipmentId { get; init; }
}

public record ShipmentDetail
{
    public Model_VolvoShipment Shipment { get; init; }
    public List<Model_VolvoShipmentLine> Lines { get; init; }
}
```

**Response**: `Model_Dao_Result<ShipmentDetail>`

---

### 9. GetAllVolvoPartsQuery

**Purpose**: Gets all parts for settings grid.

**Request**:
```csharp
public record GetAllVolvoPartsQuery : IRequest<Model_Dao_Result<List<Model_VolvoPart>>>
{
    public bool IncludeInactive { get; init; } = false;
}
```

**Response**: `Model_Dao_Result<List<Model_VolvoPart>>`

---

### 10. GetPartComponentsQuery

**Purpose**: Gets BOM components for a part.

**Request**:
```csharp
public record GetPartComponentsQuery : IRequest<Model_Dao_Result<List<Model_VolvoPartComponent>>>
{
    public int PartId { get; init; }
}
```

**Response**: `Model_Dao_Result<List<Model_VolvoPartComponent>>`

---

### 11. ExportPartsCsvQuery

**Purpose**: Exports parts master data to CSV.

**Request**:
```csharp
public record ExportPartsCsvQuery : IRequest<Model_Dao_Result<string>>
{
    public bool IncludeInactive { get; init; } = false;
}
```

**Response**: `Model_Dao_Result<string>` (CSV content)

---

### 12. ExportShipmentsQuery

**Purpose**: Exports shipments to CSV.

**Request**:
```csharp
public record ExportShipmentsQuery : IRequest<Model_Dao_Result<string>>
{
    public List<int> ShipmentIds { get; init; }
}
```

**Response**: `Model_Dao_Result<string>` (CSV content)

---

## Shared DTOs

### ShipmentLineDto

Used by multiple commands to represent shipment line items.

```csharp
public record ShipmentLineDto
{
    public string PartNumber { get; init; }
    public int ReceivedSkidCount { get; init; }
    public int? ExpectedSkidCount { get; init; }
    public bool HasDiscrepancy { get; init; }
    public string DiscrepancyNote { get; init; }
}
```

---

## Contract Versioning

**Version**: 1.0.0 (Initial CQRS Migration)  
**Breaking Changes**: None (initial implementation)  
**Deprecated**: Service_Volvo, Service_VolvoMasterData (will be marked [Obsolete] after migration)

---

## Implementation Checklist

- [ ] Create `Module_Volvo/Requests/Commands/` folder
- [ ] Create `Module_Volvo/Requests/Queries/` folder
- [ ] Implement all 9 command record classes
- [ ] Implement all 12 query record classes
- [ ] Implement SharedDto (ShipmentLineDto, etc.)
- [ ] Unit test DTO serialization (if needed for persistence)
- [ ] Register handlers via MediatR assembly scanning (App.xaml.cs)

---

**Contracts Status**: ✅ COMPLETE - Ready for handler implementation

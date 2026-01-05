# Service_Volvo - Implementation Guide

**Service Name:** Service_Volvo  
**Interface:** IService_Volvo  
**Module:** Module_Volvo  
**Purpose:** Business logic for Volvo dunnage requisition workflow

---

## Overview

Service_Volvo handles the core business logic for receiving, processing, and managing Volvo dunnage shipments. It orchestrates component explosion calculations, CSV label generation, email formatting, and shipment lifecycle management.

---

## Architecture

### Dependencies (Constructor Injection)
```csharp
private readonly Dao_VolvoShipment _shipmentDao;
private readonly Dao_VolvoShipmentLine _lineDao;
private readonly Dao_VolvoPart _partDao;
private readonly Dao_VolvoPartComponent _componentDao;
private readonly IService_LoggingUtility _logger;
```

**Registration** (App.xaml.cs):
```csharp
services.AddSingleton<IService_Volvo>(sp =>
{
    var shipmentDao = sp.GetRequiredService<Dao_VolvoShipment>();
    var lineDao = sp.GetRequiredService<Dao_VolvoShipmentLine>();
    var partDao = sp.GetRequiredService<Dao_VolvoPart>();
    var componentDao = sp.GetRequiredService<Dao_VolvoPartComponent>();
    var logger = sp.GetRequiredService<IService_LoggingUtility>();
    return new Service_Volvo(shipmentDao, lineDao, partDao, componentDao, logger);
});
```

---

## Core Methods

### 1. CalculateComponentExplosionAsync
**Purpose:** Calculate total piece counts for all parts and components in a shipment

**Algorithm:**
1. For each shipment line:
   - Get parent part details (QuantityPerSkid)
   - Calculate parent pieces: `skidCount × qtyPerSkid`
   - Get components for parent part
   - For each component: `skidCount × componentQty × componentQtyPerSkid`
2. Aggregate duplicate parts across all lines
3. Return dictionary of `partNumber → totalPieces`

**Example:**
```
Input:
  Line 1: V-EMB-500, 3 skids
    - V-EMB-500: 88 pcs/skid → 264 pcs
    - Component V-EMB-2: 1 qty × 1 pcs/skid → 3 pcs
    - Component V-EMB-92: 1 qty × 1 pcs/skid → 3 pcs

Output:
  {
    "V-EMB-500": 264,
    "V-EMB-2": 3,
    "V-EMB-92": 3
  }
```

**Validation:**
- ✅ All parts exist in master data
- ✅ QuantityPerSkid > 0 for parent parts
- ✅ Component quantities > 0 (skip invalid, log warning)
- ✅ ReceivedSkidCount between 1-99

**Error Handling:**
- Return `Model_Dao_Result<Dictionary<string, int>>` with `Success=false` on errors
- Log all operations and errors
- Do NOT throw exceptions

---

### 2. GenerateLabelCsvAsync
**Purpose:** Generate CSV file for LabelView 2022 label printing

**CSV Format:**
```
Material,Quantity,Employee,Date,Time,Receiver,Notes
V-EMB-500,264,E1234,01/05/2026,14:30,12345,Shipment #1
V-EMB-2,3,E1234,01/05/2026,14:30,12345,Shipment #1
```

**File Path:**
```
%APPDATA%\MTM_Receiving_Application\Volvo\Labels\Shipment_{shipmentId}_{yyyyMMdd}.csv
```

**Workflow:**
1. Validate shipmentId > 0 (prevent path injection)
2. Get shipment and lines from database
3. Calculate component explosion
4. Validate row count ≤ 10,000 (prevent huge files)
5. Create directory if missing
6. Validate filename has no invalid characters
7. Write CSV file
8. Return file path

**Validation:**
- ✅ shipmentId > 0
- ✅ Max 10,000 CSV lines
- ✅ Valid filename characters

---

### 3. FormatEmailTextAsync
**Purpose:** Generate formatted email text for PO requisition

**Template:**
```
Subject: PO Requisition - Volvo Dunnage - {MM/dd/yyyy} Shipment #{number}

Good morning,

Please create a PO for the following Volvo dunnage received on {MM/dd/yyyy}:

**DISCREPANCIES NOTED** (if applicable)
Part Number	Packlist Qty	Received Qty	Difference	Note
V-EMB-500	5		3		-2		Short 2 skids

Requested Lines:
Part Number	Quantity (pcs)
V-EMB-500	264
V-EMB-2		3

Additional Notes:
{shipment notes}

Thank you,
Employee #{employeeNumber}
```

**Parameters:**
- `shipment` - Shipment header (date, number, employee, notes)
- `lines` - Shipment lines (for discrepancy table)
- `requestedLines` - Dictionary from component explosion (part → pieces)

**Null Safety:**
- `requestedLines` parameter is nullable with default empty dictionary
- `shipment.Notes` checked for null/whitespace before including

---

### 4. ValidateShipmentAsync (NEW - Issue #20)
**Purpose:** Centralized validation logic for shipment data

**Validation Rules:**
```csharp
✅ At least 1 line required
✅ All lines have valid part numbers (non-empty)
✅ ReceivedSkidCount between 1-99 per line
✅ If HasDiscrepancy=true, ExpectedSkidCount must have value
✅ Shipment date not more than 1 day in future
```

**Returns:** `Model_Dao_Result` with `Success=false` and error message on validation failure

**Usage:**
```csharp
var validationResult = await _volvoService.ValidateShipmentAsync(shipment, lines);
if (!validationResult.Success)
{
    return Model_Dao_Result_Factory.Failure<T>(validationResult.ErrorMessage);
}
```

---

### 5. SaveShipmentAsync
**Purpose:** Save shipment and lines with transaction management

**Business Rules:**
- ✅ Only ONE pending shipment allowed at a time
- ✅ Validates shipment data before save
- ✅ Calculates CalculatedPieceCount for each line
- ✅ Uses MySQL transaction for atomicity

**Transaction Flow:**
```csharp
1. Check for existing pending shipment → return error if exists
2. Validate lines count > 0
3. For each line:
   - Get part details
   - Calculate CalculatedPieceCount = skidCount × qtyPerSkid
4. Insert shipment (gets ShipmentId and ShipmentNumber)
5. BEGIN TRANSACTION
   6. For each line:
      - Insert line via stored procedure
      - If any insert fails → ROLLBACK
   7. COMMIT TRANSACTION
8. Return (ShipmentId, ShipmentNumber)
```

**Critical Path:** Transaction ensures data integrity - either ALL lines save or NONE save

---

### 6. GetPendingShipmentAsync
**Purpose:** Retrieve pending shipment (status='pending_po')

**Returns:** `Model_Dao_Result<Model_VolvoShipment?>` where Data is null if no pending shipment exists

**Use Case:** Check if user can create new shipment (only 1 pending allowed)

---

### 7. GetPendingShipmentWithLinesAsync
**Purpose:** Retrieve pending shipment WITH all line items

**Returns:** `Model_Dao_Result<(Model_VolvoShipment? Shipment, List<Model_VolvoShipmentLine> Lines)>`

**Use Case:** Load pending shipment for editing or completion

---

### 8. CompleteShipmentAsync
**Purpose:** Mark shipment as completed with PO and Receiver numbers

**Workflow:**
1. Update shipment:
   - Set `po_number` = provided PO
   - Set `receiver_number` = provided Receiver
   - Set `status` = 'completed'
   - Set `is_archived` = 1
   - Set `modified_date` = CURRENT_TIMESTAMP
2. Log completion
3. Return success/failure

**Validation:**
- ✅ shipmentId exists
- ✅ poNumber and receiverNumber not empty
- ✅ Shipment is in 'pending_po' status (optional)

---

## Common Patterns

### Error Handling Template
```csharp
public async Task<Model_Dao_Result<T>> MethodAsync(...)
{
    try
    {
        await _logger.LogInfoAsync("Starting operation...");

        // Validation
        if (invalidCondition)
        {
            return new Model_Dao_Result<T>
            {
                Success = false,
                ErrorMessage = "Validation failed",
                Severity = Enum_ErrorSeverity.Warning
            };
        }

        // Business logic
        var result = await _dao.OperationAsync();
        
        if (!result.Success)
        {
            return new Model_Dao_Result<T>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                Severity = Enum_ErrorSeverity.Error
            };
        }

        await _logger.LogInfoAsync("Operation completed successfully");

        return new Model_Dao_Result<T>
        {
            Success = true,
            Data = result.Data
        };
    }
    catch (Exception ex)
    {
        await _logger.LogErrorAsync($"Error in operation: {ex.Message}", ex);
        return new Model_Dao_Result<T>
        {
            Success = false,
            ErrorMessage = $"Error: {ex.Message}",
            Severity = Enum_ErrorSeverity.Error,
            Exception = ex
        };
    }
}
```

### Transaction Pattern
```csharp
await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
await connection.OpenAsync();
await using var transaction = await connection.BeginTransactionAsync();

try
{
    foreach (var item in items)
    {
        var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
            connection,
            (MySqlTransaction)transaction,
            "sp_procedure_name",
            parameters);

        if (!result.Success)
        {
            await transaction.RollbackAsync();
            return failure;
        }
    }

    await transaction.CommitAsync();
    return success;
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    await _logger.LogErrorAsync($"Transaction failed: {ex.Message}", ex);
    return failure;
}
```

---

## Configuration Points

The following values are currently hardcoded and should be moved to settings (see VolvoSettings.md):

| Hardcoded Value | Location | Setting Name |
|----------------|----------|--------------|
| `10000` | GenerateLabelCsvAsync | MaxCsvLines |
| `%APPDATA%\MTM_Receiving_Application\Volvo\Labels` | GenerateLabelCsvAsync | CsvOutputDirectory |
| `"Good morning,"` | FormatEmailTextAsync | EmailGreeting |
| `"Thank you,"` | FormatEmailTextAsync | EmailSignature |
| Status check for existing pending | SaveShipmentAsync | AllowMultiplePendingShipments |

---

## Testing Checklist

When modifying Service_Volvo:

- [ ] All methods return `Model_Dao_Result` or `Model_Dao_Result<T>`
- [ ] No exceptions thrown (all caught and returned as failures)
- [ ] All operations logged (Info for success, Error for failures)
- [ ] Validation happens before database operations
- [ ] Null parameters checked at method entry
- [ ] Transaction rollback on any failure in multi-step operations
- [ ] Empty/null collections handled gracefully
- [ ] Integration tests pass for full workflow (save → generate CSV → format email → complete)

---

## Common Issues & Solutions

### Issue: "Part not found" error during component explosion
**Cause:** Part exists in shipment line but not in master data  
**Solution:** Ensure all parts are properly seeded in `volvo_parts` table

### Issue: Transaction timeout
**Cause:** Too many lines or network latency  
**Solution:** Reduce batch size or increase MySQL `wait_timeout`

### Issue: CSV file locked
**Cause:** LabelView or Excel has file open  
**Solution:** Close file before regenerating or implement retry logic

### Issue: "Only one pending shipment allowed" blocking new saves
**Cause:** Previous shipment not completed  
**Solution:** Complete or delete pending shipment first, OR implement setting to allow multiple

---

## Related Documentation

- [IService_Volvo Interface](../../Module_Core/Contracts/Services/IService_Volvo.cs)
- [Volvo Settings Guide](../../Documentation/FutureEnhancements/Module_Settings/VolvoSettings.md)
- [DAO Pattern Guide](.github/instructions/dao-pattern.instructions.md)
- [MVVM Pattern Guide](.github/instructions/mvvm-pattern.instructions.md)

---

**Version:** 1.0  
**Last Updated:** January 5, 2026  
**Maintained By:** Development Team

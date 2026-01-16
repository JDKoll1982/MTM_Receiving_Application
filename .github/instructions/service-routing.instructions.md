# Service Documentation: IRoutingService

**Module:** Module_Routing
**Service:** RoutingService
**Interface:** IRoutingService
**Implementation:** `Services/RoutingService.cs`
**Version:** 1.0
**Date:** 2026-01-06

---

## Overview

Core service for routing label operations. Handles the complete lifecycle of routing labels: creation, validation, CSV export, editing, and duplicate detection.

**Dependencies:**

- `Dao_RoutingLabel` - Label database operations
- `Dao_RoutingLabelHistory` - Edit history tracking
- `IRoutingInforVisualService` - PO validation via Infor Visual
- `IRoutingUsageTrackingService` - Recipient usage statistics
- `IService_LoggingUtility` - Application logging
- `IConfiguration` - Settings access (CSV paths, retry policy)

---

## Key Methods

### CreateLabelAsync

```csharp
Task<Model_Dao_Result<int>> CreateLabelAsync(Model_RoutingLabel label)
```

**Purpose:** Creates new routing label, validates, checks duplicates, exports to CSV, tracks usage

**Workflow:**

1. Validates label data (ValidateLabel)
2. Checks for duplicates within 24-hour window
3. Inserts label into database
4. Exports to CSV (network path with local fallback)
5. Increments recipient usage tracking
6. Returns new label ID

**Important Notes:**

- **No transaction** - CSV export failure doesn't rollback database insert (Issue #2)
- **Race condition risk** - Concurrent CSV writes may corrupt file (Issue #3)
- Continues even if CSV export or usage tracking fails (warns but doesn't fail)

**Common Issues:**

- Duplicate detection hardcoded to 24 hours (should be configurable)
- CSV paths must be writable or method fails

---

### UpdateLabelAsync

```csharp
Task<Model_Dao_Result> UpdateLabelAsync(Model_RoutingLabel label, int editedByEmployeeNumber)
```

**Purpose:** Updates existing label and logs all field changes to history table

**Workflow:**

1. Retrieves original label from database
2. Validates updated label data
3. Updates label in database
4. Compares old vs new, logs each changed field to routing_history
5. Returns success/failure result

**Important Notes:**

- **N+1 query** - Inserts history records one-by-one (Issue #18)
- Edit history is critical for audit trail
- Must provide editedByEmployeeNumber for audit tracking

**Common Issues:**

- Slow updates when many fields change (no batch insert)
- Original label not found = update fails

---

### ValidateLabel

```csharp
Model_Dao_Result ValidateLabel(Model_RoutingLabel label)
```

**Purpose:** Business rule validation before database operations

**Validations:**

- PONumber not empty
- LineNumber not empty
- Quantity > 0
- RecipientId > 0
- CreatedBy > 0
- If PONumber = "OTHER", OtherReasonId must be set

**Important Notes:**

- **Synchronous method** (no database calls)
- **Missing validation**: String length limits (Issue #6)
- **Missing validation**: RecipientId exists (Issue #9)

---

### CheckDuplicateLabelAsync

```csharp
Task<Model_Dao_Result<(bool Exists, int? ExistingLabelId)>> CheckDuplicateLabelAsync(
    string poNumber,
    string lineNumber,
    int recipientId,
    DateTime createdDate)
```

**Purpose:** Prevents accidental duplicate labels (same PO/Line/Recipient within time window)

**Returns:** Tuple with:

- `Exists`: true if duplicate found
- `ExistingLabelId`: ID of existing label (null if no duplicate)

**Important Notes:**

- DateTime parameter ignored - always checks 24-hour window (Issue #8)
- Only checks active labels (is_active = 1)

---

### ExportLabelToCsvAsync

```csharp
Task<Model_Dao_Result> ExportLabelToCsvAsync(Model_RoutingLabel label)
```

**Purpose:** Appends label data to CSV file for external system import

**CSV Format:**

```
PO,Line,Part,Quantity,Recipient,Location,Date
12345,001,WIDGET-A,5,John Doe,Building 2,2026-01-06 14:30:00
```

**Workflow:**

1. Tries network path with retry (default 3 attempts, 500ms delay)
2. Falls back to local path if network fails
3. Marks label as csv_exported in database
4. Returns success/failure result

**Important Notes:**

- **Path security risk** - No path traversal validation (Issue #5)
- **Race condition** - No file mutex (Issue #3)
- Configuration keys: `RoutingModule:CsvExportPath:Network` and `:Local`

---

## Configuration Requirements

### appsettings.json Structure

```json
{
  "RoutingModule": {
    "CsvExportPath": {
      "Network": "\\\\server\\share\\routing_label_data.csv",
      "Local": "C:\\RoutingLabels\\routing_label_data.csv"
    },
    "CsvRetry": {
      "MaxAttempts": 3,
      "DelayMs": 500
    }
  }
}
```

---

## Usage Examples

### Creating a Label (Wizard Workflow)

```csharp
var label = new Model_RoutingLabel
{
    PONumber = "12345",
    LineNumber = "001",
    Description = "Widget Assembly",
    RecipientId = 42,
    Quantity = 10,
    CreatedBy = employeeNumber,
    CreatedDate = DateTime.Now
};

var result = await _routingService.CreateLabelAsync(label);

if (result.IsSuccess)
{
    int newLabelId = result.Data;
    // Navigate to confirmation screen
}
else
{
    // Show error: result.ErrorMessage
}
```

### Creating OTHER Label (Manual Entry)

```csharp
var label = new Model_RoutingLabel
{
    PONumber = "OTHER",
    LineNumber = "0",
    Description = "Returned Materials",
    RecipientId = 42,
    Quantity = 3,
    CreatedBy = employeeNumber,
    OtherReasonId = 1, // Required for OTHER
    CreatedDate = DateTime.Now
};

var result = await _routingService.CreateLabelAsync(label);
```

### Editing Existing Label

```csharp
// Get label
var getResult = await _routingService.GetLabelByIdAsync(labelId);
if (!getResult.IsSuccess) return;

var label = getResult.Data;

// Modify
label.Quantity = 15;
label.RecipientId = 99;

// Update
var updateResult = await _routingService.UpdateLabelAsync(label, currentEmployeeNumber);

if (updateResult.IsSuccess)
{
    // Reload grid
}
```

---

## Error Handling Patterns

### Service Layer (RoutingService)

- Catches all exceptions
- Logs via IService_LoggingUtility
- Returns Model_Dao_Result.Failure with user-friendly message
- Never throws exceptions to ViewModels

### ViewModel Consumption

```csharp
try
{
    var result = await _routingService.CreateLabelAsync(label);

    if (result.IsSuccess)
    {
        // Success path
    }
    else
    {
        _errorHandler.ShowUserError(result.ErrorMessage, "Create Failed", nameof(CreateLabel));
    }
}
catch (Exception ex)
{
    // Should never reach here - service catches all exceptions
    _errorHandler.HandleException(ex, Enum_ErrorSeverity.High, nameof(CreateLabel), nameof(MyViewModel));
}
```

---

## Testing Guidance

### Unit Test Mocking

- Mock `IRoutingInforVisualService.ValidatePoNumberAsync`
- Mock `Dao_RoutingLabel.InsertLabelAsync` to return known ID
- Mock `IConfiguration` to return test CSV paths
- Mock `IService_LoggingUtility` to verify log calls

### Integration Test Scenarios

1. **Happy Path**: Create label with valid PO → verify database + CSV file
2. **Duplicate Detection**: Create label twice → second fails with duplicate error
3. **CSV Fallback**: Network path unavailable → succeeds via local path
4. **Validation Failure**: Missing required fields → fails before database call
5. **Edit Workflow**: Update label → verify history table has change records

### Test Data Requirements

- Valid PO in Infor Visual (or mock service)
- Active recipient in routing_recipients table
- Writable CSV paths (use temp directory for tests)

---

## Known Issues (From CODE_REVIEW.md)

| Issue # | Severity | Description | Fix Priority |
|---------|----------|-------------|--------------|
| #2 | 🔴 CRITICAL | No transaction wrapping label + CSV + usage tracking | High |
| #3 | 🔴 CRITICAL | Race condition in CSV file writing (no mutex) | High |
| #5 | 🟡 SECURITY | Path traversal risk in CSV export | Medium |
| #6 | 🟡 SECURITY | Missing string length validation | Medium |
| #8 | 🟠 DATA | Duplicate check ignores DateTime parameter | Medium |
| #9 | 🟠 DATA | No FK validation for RecipientId | Medium |
| #10 | 🔵 QUALITY | Magic string "OTHER" scattered in code | Low |
| #11 | 🔵 QUALITY | Inconsistent error handling pattern | Low |
| #18 | 🟣 PERFORMANCE | N+1 query in history logging | Medium |
| #20 | 🔧 MAINTAIN | CreateLabelAsync too complex (90 lines) | Low |

**See:** `Module_Routing/CODE_REVIEW.md` for full details and proposed fixes

---

## Future Enhancements

1. **Batch CSV Export** - Export multiple labels at once for better performance
2. **Async Background CSV** - Queue CSV exports to avoid blocking label creation
3. **Label Preview** - Generate preview before committing to database
4. **Bulk Edit** - Update multiple labels simultaneously
5. **CSV Format Customization** - Allow configurable CSV columns/format
6. **Duplicate Override** - Allow admin to force duplicate label creation

---

## Related Documentation

- [IRoutingRecipientService](service-routing-recipient.instructions.md) - Recipient management
- [IRoutingInforVisualService](service-routing-inforvisual.instructions.md) - PO validation
- [IRoutingUsageTrackingService](service-routing-usagetracking.instructions.md) - Usage statistics
- [DAO Pattern Instructions](../.github/instructions/dao-pattern.instructions.md)
- [MVVM Service Pattern](../.github/instructions/mvvm-services.instructions.md)

---

**Document Version:** 1.0
**Last Updated:** 2026-01-06
**Next Review:** After implementing CODE_REVIEW fixes

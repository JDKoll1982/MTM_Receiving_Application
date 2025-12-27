# IService_DunnageCSVWriter Contract

**Interface**: `IService_DunnageCSVWriter`  
**Implementation**: `Service_DunnageCSVWriter`  
**Location**: `Contracts/Services/IService_DunnageCSVWriter.cs`  
**Registration**: Transient (stateless)

## Interface Definition

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for exporting dunnage loads to CSV format for LabelView label printing.
    /// Implements "Local First, Network Best-Effort" strategy for dual-write.
    /// </summary>
    public interface IService_DunnageCSVWriter
    {
        /// <summary>
        /// Writes dunnage loads to CSV files (local + network).
        /// Local write MUST succeed. Network write is best-effort with error logging.
        /// </summary>
        /// <param name="loads">List of dunnage loads to export</param>
        /// <returns>Result indicating local/network success and file paths</returns>
        Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads);
    }
}
```

## Service Responsibilities

1. **CSV Generation**: Create properly formatted CSV with RFC 4180 compliance
2. **Dynamic Headers**: Query spec keys from `IService_MySQL_Dunnage.GetAllSpecKeysAsync()` to build headers
3. **Dual-Write**: Write to both local (%APPDATA%) and network share
4. **Graceful Degradation**: Network failure does not fail operation
5. **Directory Creation**: Create user subdirectories if they don't exist
6. **Escaping**: Properly escape commas, quotes, newlines in data

## File Paths

### Local Path
```
%APPDATA%\MTM_Receiving_Application\DunnageData.csv
```
**Example**: `C:\Users\JohnDoe\AppData\Roaming\MTM_Receiving_Application\DunnageData.csv`

**Behavior**: Always overwrite (no append). Single file for all dunnage exports.

---

### Network Path
```
\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\{Username}\DunnageData.csv
```
**Example**: `\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files\JohnDoe\DunnageData.csv`

**Behavior**: 
- Create `{Username}` subdirectory if it doesn't exist
- Always overwrite (no append)
- Each user has separate folder to avoid collisions

**Username Source**: `IService_UserSessionManager.CurrentSession.User.WindowsUsername` or `Environment.UserName` if session is null.

---

## CSV Format

### Header Row (Fixed + Dynamic)
**Fixed Columns** (in order):
1. `DunnageType`
2. `PartID`
3. `Quantity`
4. `PONumber`
5. `ReceivedDate`
6. `Employee`

**Dynamic Columns**: All spec keys from `GetAllSpecKeysAsync()`, sorted alphabetically, appended after fixed columns.

**Example** (if spec keys are "Color", "Size", "Material"):
```csv
DunnageType,PartID,Quantity,PONumber,ReceivedDate,Employee,Color,Material,Size
Pallet,PALLET-001,10,PO12345,2025-12-26,1001,Blue,Plastic,48x40
Divider,DIV-002,5,,2025-12-26,1001,,Cardboard,
```

---

### Data Rows
- **DunnageType**: From `Model_DunnageLoad.PartID → Model_DunnagePart.TypeID → Model_DunnageType.TypeName`
- **PartID**: From `Model_DunnageLoad.PartID`
- **Quantity**: From `Model_DunnageLoad.Quantity` (formatted as integer if no decimal)
- **PONumber**: From `Model_DunnageLoad.PONumber` (empty string if null)
- **ReceivedDate**: From `Model_DunnageLoad.ReceivedDate` (format: `yyyy-MM-dd`)
- **Employee**: From `Model_DunnageLoad.Employee` (employee number)
- **Spec Values**: From `Model_DunnageLoad.SpecValues` (JSON), extract values matching header keys. Empty string if key not found.

---

## WriteToCSVAsync() Algorithm

```csharp
public async Task<Model_CSVWriteResult> WriteToCSVAsync(List<Model_DunnageLoad> loads)
{
    var result = new Model_CSVWriteResult();

    try
    {
        // 1. Validation
        if (loads == null || loads.Count == 0) {
            result.ErrorMessage = "No loads to export";
            return result;
        }

        // 2. Get spec keys for headers
        var specKeys = await _mysqlService.GetAllSpecKeysAsync();

        // 3. Build header row
        var headers = new List<string> 
        { 
            "DunnageType", "PartID", "Quantity", "PONumber", "ReceivedDate", "Employee" 
        };
        headers.AddRange(specKeys); // Already sorted alphabetically

        // 4. Build data rows
        var rows = new List<List<string>>();
        foreach (var load in loads)
        {
            var row = new List<string>();
            
            // Fixed columns
            row.Add(load.PartID); // TODO: lookup Type name
            row.Add(load.PartID);
            row.Add(load.Quantity.ToString());
            row.Add(load.PONumber ?? string.Empty);
            row.Add(load.ReceivedDate.ToString("yyyy-MM-dd"));
            row.Add(load.Employee.ToString());
            
            // Dynamic spec columns
            var specValues = JsonDocument.Parse(load.SpecValues ?? "{}");
            foreach (var key in specKeys)
            {
                if (specValues.RootElement.TryGetProperty(key, out var value)) {
                    row.Add(value.GetString() ?? string.Empty);
                } else {
                    row.Add(string.Empty); // Missing spec value
                }
            }
            
            rows.Add(row);
        }

        // 5. Write to local path
        result.LocalFilePath = GetLocalPath();
        EnsureDirectoryExists(result.LocalFilePath);
        await WriteCsvFileAsync(result.LocalFilePath, headers, rows);
        result.LocalSuccess = true;

        // 6. Write to network path (best-effort)
        try
        {
            result.NetworkFilePath = GetNetworkPath();
            EnsureDirectoryExists(result.NetworkFilePath);
            await WriteCsvFileAsync(result.NetworkFilePath, headers, rows);
            result.NetworkSuccess = true;
        }
        catch (Exception netEx)
        {
            result.NetworkSuccess = false;
            result.ErrorMessage = $"Network write failed: {netEx.Message}";
            _logger.LogWarning($"Network CSV write failed: {netEx.Message}");
        }

        return result;
    }
    catch (Exception ex)
    {
        result.LocalSuccess = false;
        result.ErrorMessage = $"CSV export failed: {ex.Message}";
        _logger.LogError($"CSV export failed: {ex.Message}", ex);
        _errorHandler.HandleException(ex, Enum_ErrorSeverity.High, nameof(WriteToCSVAsync), nameof(Service_DunnageCSVWriter));
        return result;
    }
}
```

---

## CSV Writing with CsvHelper

```csharp
private async Task WriteCsvFileAsync(string filePath, List<string> headers, List<List<string>> rows)
{
    using var writer = new StreamWriter(filePath, append: false);
    using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        // RFC 4180 compliance
        ShouldQuote = (args) => true // Always quote fields for safety
    });

    // Write header
    foreach (var header in headers) {
        csv.WriteField(header);
    }
    csv.NextRecord();

    // Write rows
    foreach (var row in rows)
    {
        foreach (var field in row) {
            csv.WriteField(field);
        }
        csv.NextRecord();
    }

    await writer.FlushAsync();
}
```

---

## Error Handling Strategy

### Local Write Failure
- **Severity**: Critical (should never happen)
- **Action**: Log error, return `Model_CSVWriteResult` with `LocalSuccess = false`
- **User Impact**: Session save fails, user sees error message

### Network Write Failure
- **Severity**: Warning (expected if network share is unavailable)
- **Action**: Log warning, continue operation
- **Result**: `LocalSuccess = true`, `NetworkSuccess = false`, `ErrorMessage` contains details
- **User Impact**: Session save succeeds, user sees warning: "Saved locally, network copy failed"

### Empty Loads List
- **Severity**: Info
- **Action**: Return early with error message
- **User Impact**: User sees "No data to export" message

---

## Dependencies

- `IService_MySQL_Dunnage`: For `GetAllSpecKeysAsync()` to build dynamic headers
- `IService_UserSessionManager`: For username in network path
- `ILoggingService`: For error/warning logging
- `CsvHelper`: For RFC 4180 compliant CSV writing

---

## Testing Strategy

### Unit Tests (Mock Dependencies)
- Test header generation (fixed + dynamic columns)
- Test row generation (spec value extraction)
- Test local/network path calculation
- Test error handling (network failure doesn't fail operation)
- Test empty loads validation

### Integration Tests (Real File System)
- Test local CSV file creation
- Test network CSV file creation (if share available)
- Test file overwrite behavior
- Test directory creation
- Test concurrent writes by multiple users (different folders)

### Manual Tests
- Verify LabelView can read generated CSV
- Test special characters in data (commas, quotes, newlines)
- Test large file (100+ loads)

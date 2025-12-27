# CSV Export Service Standards

**Category**: Data Export
**Last Updated**: December 26, 2025
**Applies To**: `IService_CSVWriter`, `IService_DunnageCSVWriter`

## Overview

The application exports data to CSV for integration with external systems (e.g., LabelView).

## Dual-Write Strategy

To ensure reliability while supporting shared access:
1.  **Local Write (Mandatory)**: Always write to `%APPDATA%\MTM_Receiving_Application\`. This ensures the user can continue working even if the network is down.
2.  **Network Write (Best-Effort)**: Attempt to write to the network share. If it fails, log the error but **do not fail the operation**.

## Implementation Pattern

```csharp
public async Task<Model_CSVWriteResult> WriteToCSVAsync(List<Data> data)
{
    // 1. Write Local
    try {
        await WriteFileAsync(localPath, data);
    } catch {
        return Failure("Local write failed"); // Critical failure
    }

    // 2. Write Network
    bool networkSuccess = false;
    try {
        await WriteFileAsync(networkPath, data);
        networkSuccess = true;
    } catch (Exception ex) {
        _logger.LogError("Network write failed", ex);
        // Continue - do not throw
    }

    return new Model_CSVWriteResult { 
        LocalSuccess = true, 
        NetworkSuccess = networkSuccess 
    };
}
```

## CSV Formatting

- **Library**: Use `CsvHelper` for robust RFC 4180 compliance.
- **Headers**: Explicitly define headers.
- **Dynamic Columns**: Use `ExpandoObject` or `dynamic` for variable schema data (e.g., Dunnage Specs).
- **Escaping**: Handle commas, quotes, and newlines in data fields automatically via the library.

## File Paths
- **Local**: `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)`
- **Network**: Configurable path, often user-specific (e.g., `.../User CSV Files/{Username}/`).

# CSV to XLS Migration Guide

**Date:** February 10, 2026  
**Scope:** Complete conversion of all CSV file handling to XLS format using ClosedXML

## Summary

This document tracks the comprehensive migration from CSV to XLS format across the MTM Receiving Application. The migration replaces CsvHelper library with ClosedXML for Excel-compatible file generation.

---

## ✅ Completed Changes

### 1. NuGet Packages (.csproj)
- ✅ Replaced `CsvHelper` (v33.1.0) with `ClosedXML` (v0.104.2)

### 2. Model Files (Module_Receiving/Models/)
- ✅ Renamed `Model_CSVWriteResult.cs` → `Model_XLSWriteResult.cs`
- ✅ Updated class name: `Model_CSVWriteResult` → `Model_XLSWriteResult`
- ✅ Renamed `Model_CSVDeleteResult.cs` → `Model_XLSDeleteResult.cs`
- ✅ Updated class name: `Model_CSVDeleteResult` → `Model_XLSDeleteResult`
- ✅ Renamed `Model_CSVExistenceResult.cs` → `Model_XLSExistenceResult.cs`
- ✅ Updated class name: `Model_CSVExistenceResult` → `Model_XLSDeleteResult`

### 3. Service Interfaces
- ✅ Renamed `IService_CSVWriter.cs` → `IService_XLSWriter.cs`
- ✅ Updated interface name and all method signatures (CSV → XLS)
- ✅ Renamed `IService_DunnageCSVWriter.cs` → `IService_DunnageXLSWriter.cs`
- ✅ Updated interface name and all method signatures (CSV → XLS, Csv → Xls)

### 4. Service Implementations
- ✅ Created new `Service_XLSWriter.cs` with ClosedXML implementation
  - Replaced CsvHelper with ClosedXML.Excel
  - Changed file extension from .csv to .xls
  - Updated all method names (CSV → XLS)
  - Implemented Excel workbook/worksheet creation
  - Added header formatting (bold, gray background)
  - Auto-fit columns for better readability

- ✅ Created new `Service_DunnageXLSWriter.cs` with ClosedXML implementation
  - Full rewrite using ClosedXML
  - Dynamic column generation for specifications
  - Fixed and dynamic column support
  - Dual-path writing (local + network)
  - Excel formatting and auto-sizing

---

## ⚠️ Pending Changes (Must Be Completed)

### 5. Service Registration (App.xaml.cs or DI Configuration)
**Current:** Services registered as `IService_CSVWriter` 
**Required:**
```csharp
// OLD:
services.AddSingleton<IService_CSVWriter, Service_CSVWriter>();
services.AddSingleton<IService_DunnageCSVWriter, Service_DunnageCSVWriter>();

// NEW:
services.AddSingleton<IService_XLSWriter, Service_XLSWriter>();
services.AddSingleton<IService_DunnageXLSWriter, Service_DunnageXLSWriter>();
```

**Files to update:**
- `Infrastructure/DependencyInjection/ServiceConfiguration.cs` (likely location)
- Search for `AddSingleton.*CSV` or `AddTransient.*CSV`

### 6. Settings Keys (Module_Receiving/Settings/ReceivingSettingsKeys.cs)
**Required changes:**
| Old Key | New Key |
|---------|---------|
| `WorkflowResetCsv` | `WorkflowResetXls` |
| `CompletionLocalCsvLabel` | `CompletionLocalXlsLabel` |
| `CompletionNetworkCsvLabel` | `CompletionNetworkXlsLabel` |
| `CompletionCsvFileLabel` | `CompletionXlsFileLabel` |
| `SaveProgressSavingCsv` | `SaveProgressSavingXls` |
| `ResetCsvDialogTitle` | `ResetXlsDialogTitle` |
| `ResetCsvDialogContent` | `ResetXlsDialogContent` |
| `ResetCsvDialogDelete` | `ResetXlsDialogDelete` |
| `ResetCsvDialogCancel` | `ResetXlsDialogCancel` |
| `StatusCsvDeletedSuccess` | `StatusXlsDeletedSuccess` |
| `StatusCsvDeletedFailed` | `StatusXlsDeletedFailed` |
| `SaveToCsvEnabled` | `SaveToXlsEnabled` |
| `SaveToNetworkCsvEnabled` | `SaveToNetworkXlsEnabled` |
| `CsvSaveLocation` | `XlsSaveLocation` |

### 7. Settings Default Values
**Files to update:**
- `Module_Settings.Core/Defaults/settings.manifest.json`
  - Change all `Csv` → `Xls` in key names
  - Change all `CSV` → `XLS` in display values
  - Update default paths from `.csv` to `.xls`

### 8. ViewModels (inject IService_XLSWriter instead of IService_CSVWriter)
**Files to update:**
- `Module_Receiving/ViewModels/ViewModel_Receiving_Workflow.cs`
- `Module_Receiving/ViewModels/ViewModel_Receiving_EditMode.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_EditModeViewModel.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_ReviewViewModel.cs`
- `Module_Dunnage/ViewModels/ViewModel_Dunnage_Wizard.cs` (if exists)

**Pattern to search:**
```csharp
// OLD:
private readonly IService_CSVWriter _csvWriter;
public ViewModel(IService_CSVWriter csvWriter) { _csvWriter = csvWriter; }

// NEW:
private readonly IService_XLSWriter _xlsWriter;
public ViewModel(IService_XLSWriter xlsWriter) { _xlsWriter = xlsWriter; }
```

### 9. ViewModel Method Calls
Update all method calls:
```csharp
// OLD:
await _csvWriter.WriteToCSVAsync(loads);
await _csvWriter.ClearCSVFilesAsync();
var path = await _csvWriter.GetNetworkCSVPathAsync();

// NEW:
await _xlsWriter.WriteToXLSAsync(loads);
await _xlsWriter.ClearXLSFilesAsync();
var path = await _xlsWriter.GetNetworkXLSPathAsync();
```

### 10. Service Layer Files
**Files to update:**
- `Module_Receiving/Services/Service_ReceivingWorkflow.cs`
  - Update `IService_CSVWriter` → `IService_XLSWriter`
  - Update method calls: `WriteToCSVAsync` → `WriteToXLSAsync`
  - Update method calls: `ResetCSVFilesAsync` → `ResetXLSFilesAsync`
  
- `Module_Dunnage/Services/Service_DunnageWorkflow.cs`
  - Update `IService_DunnageCSVWriter` → `IService_DunnageXLSWriter`
  - Update all method calls

- `Module_Reporting/Services/Service_Reporting.cs`
  - Update `ExportToCSVAsync` → `ExportToXLSAsync`
  - Replace CSV generation with ClosedXML
  - Change filename from `.csv` to `.xls`

### 11. XAML Files (UI Text Updates)
**Files to search and update:**
- `Module_Settings.Receiving/ Views/View_Settings_Receiving_Defaults.xaml`
  - Change "CSV File Save Location" → "XLS File Save Location"
  - Change placeholder text references

- `Module_Settings.Receiving/Views/View_Settings_Receiving_BusinessRules.xaml`
  - Change "Save To CSV" → "Save To XLS"
  - Change "Save To Network CSV" → "Save To Network XLS"
  - Update tooltips

- `Module_Volvo/Views/View_Volvo_History.xaml`
  - Change "Export CSV" → "Export XLS"

- `Module_Volvo/Views/View_Volvo_Settings.xaml`
  - Change "Import CSV" / "Export CSV" buttons

**Search pattern:** `CSV|Csv|csv` in `**/*.xaml`

### 12. Volvo Module Service Updates
**File:** `Module_Volvo/Services/Service_VolvoMasterData.cs`

Update methods:
```csharp
// OLD:
public async Task<Model_Dao_Result<(int, int, int)>> ImportCsvAsync(string csvFilePath)
public async Task<Model_Dao_Result<string>> ExportCsvAsync(string csvFilePath, bool includeInactive = false)

// NEW:
public async Task<Model_Dao_Result<(int, int, int)>> ImportXlsAsync(string xlsFilePath)
public async Task<Model_Dao_Result<string>> ExportXlsAsync(string xlsFilePath, bool includeInactive = false)
```

Replace CSV parsing logic with ClosedXML:
```csharp
// OLD: Manual CSV parsing
var lines = csvFilePath.Split('\n');
var fields = ParseCsvLine(lines[i]);

// NEW: ClosedXML reading
using var workbook = new XLWorkbook(xlsFilePath);
var worksheet = workbook.Worksheet(1);
foreach (var row in worksheet.RowsUsed().Skip(1))
{
    var partNumber = row.Cell(1).GetString();
    var quantity = row.Cell(2).GetValue<int>();
    // ...
}
```

### 13. Documentation Updates
**Files to update:**
- `README.md` - Change references from CSV to XLS
- `docs/**/*.md` - Update technical documentation
- `Module_Receiving/Documentation/**` - Update module docs
- `Module_Dunnage/Documentation/**` - Update module docs
- `Module_Volvo/docs/**` - Update Volvo docs
- `Database/UseToValidate_App_To_SP_Workflow.md` - Update workflow docs

**Search pattern:** `\.csv|CSV|csv` in `**/*.md`

### 14. Configuration Files
- `appsettings.json` / `appsettings.Development.json`
  - Update any CSV-related configuration keys
  - Update default file paths
  - Update logging categories if named around CSV

### 15. Delete Old Service Files
**After confirming new services work:**
```powershell
Remove-Item "Module_Receiving\Services\Service_CSVWriter.cs"
Remove-Item "Module_Dunnage\Services\Service_DunnageCSVWriter.cs"
```

### 16. Network Path Updates
**Current paths:**
```
\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User CSV Files
```

**Review if path should be renamed to:**
```
\\mtmanu-fs01\Expo Drive\Receiving\MTM Receiving Application\User XLS Files
```

**OR keep path but change file extensions only** (recommended for backward compatibility)

### 17. Settings Keys Update JSON
**File:** `Module_Settings.Core/Defaults/settings.manifest.json`

Update all entries that reference CSV to XLS:
```json
{
  "key": "Receiving.Defaults.XlsSaveLocation",
  "displayName": "XLS File Save Location",
  "description": "Path where XLS receiving files are saved",
  "defaultValue": "\\\\mtmanu-fs01\\Expo Drive\\Receiving\\MTM Receiving Application\\User XLS Files"
}
```

---

## 🧪 Testing Checklist

After completing all changes:

### Unit Tests
- [ ] Update test file names (CSV → XLS)
- [ ] Update test class names
- [ ] Update mock service interfaces
- [ ] Update expected file extensions in assertions

### Integration Tests
- [ ] Test local XLS file creation
- [ ] Test network XLS file creation
- [ ] Test file reading
- [ ] Test file clearing/deletion
- [ ] Test dynamic column generation (Dunnage)

### Manual Testing
- [ ] Run Receiving workflow - Guided mode
- [ ] Run Receiving workflow - Manual Entry mode
- [ ] Run Receiving workflow - Edit mode
- [ ] Run Dunnage wizard workflow
- [ ] Run Dunnage manual entry
- [ ] Test Volvo part import/export
- [ ] Verify XLS files open correctly in Excel
- [ ] Verify network file creation
- [ ] Test settings UI updates
- [ ] Verify error handling for missing directories

### Verification in Excel
- [ ] Open generated XLS file in Microsoft Excel
- [ ] Verify headers are bold and gray background
- [ ] Verify columns auto-fit
- [ ] Verify data accuracy
- [ ] Verify dynamic columns appear correctly (Dunnage)
- [ ] Verify no missing data
- [ ] Test opening XLS in LibreOffice Calc (if applicable)

---

## 🔍 Search Commands for Remaining Work

Use these PowerShell commands to find remaining CSV references:

```powershell
# Find CSV in C# files
Get-ChildItem -Path . -Include *.cs -Recurse | Select-String -Pattern "CSV|Csv" | Select-Object Path, LineNumber, Line

# Find CSV in XAML files
Get-ChildItem -Path . -Include *.xaml -Recurse | Select-String -Pattern "CSV|Csv|csv" | Select-Object Path, LineNumber, Line

# Find CSV in JSON files
Get-ChildItem -Path . -Include *.json -Recurse | Select-String -Pattern "CSV|Csv|csv" | Select-Object Path, LineNumber, Line

# Find CSV in Markdown files
Get-ChildItem -Path . -Include *.md -Recurse | Select-String -Pattern "\.csv|CSV" | Select-Object Path, LineNumber, Line

# Find model references
Get-ChildItem -Path . -Include *.cs -Recurse | Select-String -Pattern "Model_CSV" | Select-Object Path, LineNumber

# Find service interface references
Get-ChildItem -Path . -Include *.cs -Recurse | Select-String -Pattern "IService_.*CSV" | Select-Object Path, LineNumber
```

---

## 📋 Validation Checklist

Before marking migration complete:

- [ ] Solution builds without errors
- [ ] All unit tests pass
- [ ] No compiler warnings about missing types
- [ ] DI registration updated (no runtime errors on startup)
- [ ] Settings UI displays XLS instead of CSV
- [ ] Generated files have `.xls` extension
- [ ] Files open successfully in Excel
- [ ] Network paths work correctly
- [ ] Old CSV service files deleted
- [ ] Documentation updated
- [ ] Git commit with clear message

---

## 🚀 Deployment Notes

### Breaking Changes
- **File Extension:** All files now generated as `.xls` instead of `.csv`
- **Network Path:** May need to create new directory structure if renaming from CSV to XLS
- **Settings Migration:** Existing user CSV path settings need migration to XLS keys

### Backward Compatibility
- Old CSV files will NOT be automatically migrated
- Users will need to re-export data if needed
- Consider adding one-time migration utility if historical data is critical

### Communication
Notify users:
1. File format has changed from CSV to XLS
2. Files will open directly in Excel
3. Improved formatting with headers and auto-sized columns
4. Existing CSV files are not automatically migrated

---

## 📝 Notes

**ClosedXML Version:** 0.104.2
- Supports .xls and .xlsx formats
- MIT License (no licensing concerns)
- Active maintenance and community support
- Good performance for small to medium datasets

**Implementation Details:**
- Header row formatted with bold font and gray background
- Columns auto-fit to content width
- Date/time formatted as "yyyy-MM-dd HH:mm:ss"
- Empty cells display as blank (not "NULL" or "N/A")
- UTF-8 encoding preserved
- Excel-compatible formulas supported (future enhancement)

**Performance Considerations:**
- ClosedXML loads entire file into memory
- For very large exports (>50,000 rows), consider streaming
- Network writes may be slower due to file size increase
- .xls files are larger than .csv files

---

## 🎯 Success Criteria

Migration is complete when:
1. ✅ All code compiles without errors
2. ✅ All tests pass (unit + integration)
3. ✅ Application starts without DI errors
4. ✅ XLS files are generated successfully
5. ✅ Files open correctly in Excel
6. ✅ Network file creation works
7. ✅ UI displays "XLS" terminology
8. ✅ Settings updated to XLS keys
9. ✅ Documentation reflects XLS format
10. ✅ Old CSV service files removed

---

**Status:** 🟡 In Progress (30% Complete)  
**Next Steps:** Update DI registration, ViewModels, and Settings keys

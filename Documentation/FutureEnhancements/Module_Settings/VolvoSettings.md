# Volvo Module - Configurable Settings

**Module:** Module_Volvo  
**Purpose:** Centralized configuration for Volvo dunnage requisition workflow  
**Target Implementation:** Settings Page in Application

---

## 📋 Settings Categories

### 1. File System Paths

| Setting Name | Current Value | Type | Description | Validation |
|--------------|---------------|------|-------------|------------|
| **CSV Output Directory** | `%APPDATA%\MTM_Receiving_Application\Volvo\Labels` | Directory Path | Location where label CSV files are generated | Must be writable, auto-create if missing |
| **CSV Filename Pattern** | `Shipment_{ShipmentId}_{Date}.csv` | String Template | Pattern for generated CSV filenames | Variables: `{ShipmentId}`, `{Date}`, `{ShipmentNumber}` |

**Implementation Notes:**
- Allow user to browse for custom directory
- Provide "Reset to Default" button
- Validate path exists and is writable on save
- Display current resolved path (with environment variables expanded)

---

### 2. CSV Generation Limits

| Setting Name | Current Value | Type | Description | Validation |
|--------------|---------------|------|-------------|------------|
| **Max CSV Lines** | `10000` | Integer | Maximum number of part lines allowed in single CSV file | Range: 100-100,000 |
| **CSV Delimiter** | `,` (comma) | Char | Character used to separate CSV columns | Common: `,` `;` `\t` `\|` |
| **Include Header Row** | `true` | Boolean | Whether to include column headers in CSV | N/A |

**Implementation Notes:**
- Max CSV Lines prevents memory issues and label printer overload
- Delimiter allows compatibility with different label software
- Warn user if changing delimiter from default

---

### 3. Validation Rules

| Setting Name | Current Value | Type | Description | Validation |
|--------------|---------------|------|-------------|------------|
| **Min Skid Count** | `1` | Integer | Minimum skids allowed per shipment line | Range: 1-99 |
| **Max Skid Count** | `99` | Integer | Maximum skids allowed per shipment line | Range: 1-999 |
| **Allow Duplicate Parts** | `false` | Boolean | Whether to allow same part multiple times in one shipment | N/A |
| **Require Shipment Notes** | `false` | Boolean | Whether notes field is mandatory | N/A |

**Implementation Notes:**
- Min/Max Skid Count currently hardcoded in AddPart() validation
- Duplicate parts check exists but could be made optional
- Notes requirement currently not enforced

**Current Code Locations:**
```csharp
// ViewModels/ViewModel_Volvo_ShipmentEntry.cs - AddPart()
if (ReceivedSkidsToAdd < 1 || ReceivedSkidsToAdd > 99)  // Lines 217-218
{
    await _errorHandler.HandleErrorAsync(
        "Received skid count must be between 1 and 99",
        Enum_ErrorSeverity.Low,
        null,
        true);
    return;
}

// Duplicate check - Line 226
if (Parts.Any(p => p.PartNumber.Equals(SelectedPartToAdd.PartNumber, StringComparison.OrdinalIgnoreCase)))
```

---

### 4. Email Formatting

| Setting Name | Current Value | Type | Description | Validation |
|--------------|---------------|------|-------------|------------|
| **Email Subject Template** | `PO Requisition - Volvo Dunnage - {Date} Shipment #{Number}` | String Template | Email subject line format | Variables: `{Date}`, `{Number}`, `{EmployeeNumber}` |
| **Email Greeting** | `Good morning,` | String | Opening greeting for email body | Max 100 chars |
| **Email Signature** | `Thank you,\nEmployee #{EmployeeNumber}` | String | Closing signature for email | Variables: `{EmployeeNumber}`, `{UserName}` |
| **Include Discrepancy Table** | `true` | Boolean | Show discrepancy section if applicable | N/A |

**Implementation Notes:**
- Templates use `{Variable}` syntax for replacement
- Support multiline for greeting/signature with `\n` or rich text editor
- Preview button to show example email

**Current Code Location:**
```csharp
// Services/Service_Volvo.cs - FormatEmailTextAsync() Lines 295-355
```

---

### 5. AutoSuggest Behavior

| Setting Name | Current Value | Type | Description | Validation |
|--------------|---------------|------|-------------|------------|
| **Max Suggestions** | `20` | Integer | Maximum parts shown in AutoSuggestBox dropdown | Range: 5-100 |
| **Min Search Length** | `1` | Integer | Minimum characters before showing suggestions | Range: 1-5 |
| **Search Case Sensitive** | `false` | Boolean | Whether part number search is case-sensitive | N/A |

**Implementation Notes:**
- Max Suggestions currently hardcoded as `.Take(20)` in UpdatePartSuggestions()
- Affects UI responsiveness with large part catalogs
- Lower values improve performance

**Current Code Location:**
```csharp
// ViewModels/ViewModel_Volvo_ShipmentEntry.cs - UpdatePartSuggestions() Line 185
var suggestions = _allParts
    .Where(p => p.PartNumber.Contains(queryText, StringComparison.OrdinalIgnoreCase))
    .Take(20)  // Currently hardcoded
    .ToList();
```

---

### 6. Data Retention

| Setting Name | Current Value | Type | Description | Validation |
|--------------|---------------|------|-------------|------------|
| **Archive Completed Shipments** | `true` | Boolean | Set is_archived=1 after completion | N/A |
| **Keep Pending Shipment Days** | `30` | Integer | Days to keep pending shipment before warning | Range: 1-365 |
| **Auto-Delete Archived After Days** | `0` (disabled) | Integer | Days before auto-deleting archived shipments (0=never) | Range: 0-3650 |

**Implementation Notes:**
- Archive flag prevents clutter in active views
- Pending shipment age check could trigger warnings
- Auto-delete requires careful implementation with audit logging

---

### 7. Workflow Behavior

| Setting Name | Current Value | Type | Description | Validation |
|--------------|---------------|------|-------------|------------|
| **Allow Multiple Pending Shipments** | `false` | Boolean | Whether to allow >1 pending shipment at once | N/A |
| **Auto-Generate Labels on Save** | `false` | Boolean | Generate CSV labels immediately when saving shipment | N/A |
| **Auto-Format Email on Save** | `false` | Boolean | Open email preview automatically after save | N/A |
| **Confirm Before Complete** | `true` | Boolean | Show confirmation dialog when completing shipment | N/A |

**Implementation Notes:**
- Multiple pending shipments currently blocked by business rule
- Auto-generation could streamline workflow for power users
- Confirmation prevents accidental completion

**Current Code Locations:**
```csharp
// Services/Service_Volvo.cs - SaveShipmentAsync() Lines 374-384
// Check for existing pending shipment
var existingPendingResult = await _shipmentDao.GetPendingAsync();
if (existingPendingResult.Success && existingPendingResult.Data != null)
{
    return new Model_Dao_Result<(int, int)>
    {
        Success = false,
        ErrorMessage = $"A pending shipment already exists...",
        Severity = Enum_ErrorSeverity.Warning
    };
}
```

---

### 8. Logging & Diagnostics

| Setting Name | Current Value | Type | Description | Validation |
|--------------|---------------|------|-------------|------------|
| **Log User Actions** | `true` | Boolean | Log when users add/remove parts | N/A |
| **Log Level** | `Information` | Enum | Minimum severity to log (Trace/Debug/Info/Warning/Error) | N/A |
| **Include Exception Details** | `true` | Boolean | Log full stack traces on errors | N/A |

**Implementation Notes:**
- User action logging already implemented (Issue #23)
- Log level control requires integration with IService_LoggingUtility
- Exception details help with debugging

---

## 🔧 Implementation Strategy

### Phase 1: Core Settings Infrastructure
1. Create `Model_VolvoSettings.cs` with all properties
2. Create `Dao_VolvoSettings` for database persistence
3. Create settings table in MySQL database
4. Add settings singleton to DI container

### Phase 2: Settings Service
1. Create `IService_VolvoSettings` interface
2. Implement `Service_VolvoSettings` with validation
3. Add default settings initialization on first run
4. Implement import/export for backup

### Phase 3: Settings UI
1. Create `SettingsPage.xaml` in Module_Settings
2. Group settings by category with ExpanderControls
3. Add validation with real-time feedback
4. Implement "Reset to Defaults" per category

### Phase 4: Integration
1. Refactor hardcoded values to use settings service
2. Add settings change notifications (INotifyPropertyChanged)
3. Update documentation with setting descriptions
4. Add tooltips in UI explaining each setting

---

## 📊 Database Schema

```sql
CREATE TABLE volvo_settings (
    setting_key VARCHAR(100) PRIMARY KEY,
    setting_value TEXT NOT NULL,
    setting_type ENUM('String','Integer','Boolean','Path','Enum') NOT NULL,
    category VARCHAR(50) NOT NULL,
    description TEXT,
    default_value TEXT NOT NULL,
    min_value INT NULL,
    max_value INT NULL,
    modified_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    modified_by VARCHAR(50),
    INDEX idx_category (category)
);
```

---

## 🎯 Priority Settings (Quick Wins)

These settings provide immediate value with minimal effort:

1. **Max CSV Lines** - Already has validation, just needs to be configurable
2. **Max Suggestions** - One-line change in UpdatePartSuggestions()
3. **CSV Output Directory** - Already uses Path.Combine, easy to parameterize
4. **Min/Max Skid Count** - Simple range validation replacement

---

## ⚠️ Settings Requiring Careful Implementation

1. **Allow Multiple Pending Shipments** - Requires removing business rule validation
2. **Auto-Delete Archived After Days** - Needs scheduled job and audit logging
3. **Email Templates** - Requires robust template parser with variable replacement
4. **CSV Delimiter** - May break label printer compatibility if changed

---

## 📖 Related Documents

- [Module Settings Constitution](.specify/memory/constitution.md)
- [Service Infrastructure Guide](../../README.md)
- [Volvo Workflow Specification](../../specs/002-volvo-module/)

---

**Document Version:** 1.0  
**Created:** January 5, 2026  
**Last Updated:** January 5, 2026  
**Status:** Proposed for Implementation

# Module_Volvo - Settable Objects Inventory

**Version:** 1.0.0 | **Generated:** 2026-01-17

---

## 📋 Overview

This document catalogs all configuration objects, settings, and environment-dependent values used by Module_Volvo. These objects are settable/configurable at runtime and should be externalized from code.

**Purpose:**  
- Document all configuration reads for Copilot awareness
- Identify hardcoded values that should be moved to configuration
- Track database-backed settings vs. app settings

---

## 🗄️ Database Settings (MySQL `settings_module_volvo` table)

Module_Volvo uses a dedicated database table for runtime-configurable settings accessed via `Dao_VolvoSettings`.

### Setting: email_to_recipients
**Key:** `email_to_recipients`  
**Type:** JSON Array of `Model_EmailRecipient`  
**Purpose:** TO recipients for PO requisition emails  
**Default Value:**  
```json
[
  {"Name": "Jose Rosas", "Email": "jrosas@mantoolmfg.com"},
  {"Name": "Sandy Miller", "Email": "smiller@mantoolmfg.com"},
  {"Name": "Steph Wittmus", "Email": "swittmus@mantoolmfg.com"}
]
```

**Fallback Value (if setting not found):**  
```
"Jose Rosas" <jrosas@mantoolmfg.com>; "Sandy Miller" <smiller@mantoolmfg.com>; "Steph Wittmus" <swittmus@mantoolmfg.com>
```

**Read Location:**  
- `ViewModel_Volvo_ShipmentEntry.ShowEmailPreviewDialogAsync()` (Line 679-681)

**Stored Procedure:**  
- `sp_Volvo_Settings_Get`

**DAO Method:**  
- `Dao_VolvoSettings.GetSettingAsync("email_to_recipients")`

**Usage Example:**
```csharp
var settingsDao = new Dao_VolvoSettings(connectionString);
var toResult = await settingsDao.GetSettingAsync("email_to_recipients");
string toRecipients = FormatRecipientsFromJson(
    toResult.IsSuccess && toResult.Data != null ? toResult.Data.SettingValue : null,
    "\"Jose Rosas\" <jrosas@mantoolmfg.com>; ..."
);
```

**Modification:**  
Via UI in Volvo Settings screen or direct database update:
```sql
CALL sp_Volvo_Settings_Upsert(
    'email_to_recipients', 
    '[{"Name":"John Doe","Email":"jdoe@mantoolmfg.com"}]',
    'admin'
);
```

---

### Setting: email_cc_recipients
**Key:** `email_cc_recipients`  
**Type:** JSON Array of `Model_EmailRecipient`  
**Purpose:** CC recipients for PO requisition emails  
**Default Value:**  
```json
[
  {"Name": "Debra Alexander", "Email": "dalexander@mantoolmfg.com"},
  {"Name": "Michelle Laurin", "Email": "mlaurin@mantoolmfg.com"}
]
```

**Fallback Value (if setting not found):**  
```
"Debra Alexander" <dalexander@mantoolmfg.com>; "Michelle Laurin" <mlaurin@mantoolmfg.com>
```

**Read Location:**  
- `ViewModel_Volvo_ShipmentEntry.ShowEmailPreviewDialogAsync()` (Line 681)

**Stored Procedure:**  
- `sp_Volvo_Settings_Get`

**DAO Method:**  
- `Dao_VolvoSettings.GetSettingAsync("email_cc_recipients")`

**Usage Example:**
```csharp
var settingsDao = new Dao_VolvoSettings(connectionString);
var ccResult = await settingsDao.GetSettingAsync("email_cc_recipients");
string ccRecipients = FormatRecipientsFromJson(
    ccResult.IsSuccess && ccResult.Data != null ? ccResult.Data.SettingValue : null,
    "\"Debra Alexander\" <dalexander@mantoolmfg.com>; ..."
);
```

**Modification:**  
Via UI in Volvo Settings screen or direct database update:
```sql
CALL sp_Volvo_Settings_Upsert(
    'email_cc_recipients', 
    '[{"Name":"Jane Smith","Email":"jsmith@mantoolmfg.com"}]',
    'admin'
);
```

---

## 🔌 Connection Strings

### MySQL Connection String
**Source:** `Helper_Database_Variables.GetConnectionString()`  
**Purpose:** Primary database for Volvo module (parts, shipments, settings)  
**Read Locations:**
- `ViewModel_Volvo_ShipmentEntry.ShowEmailPreviewDialogAsync()` (Line 679)
- `Service_Volvo` (Line 651)
- All DAOs (constructor injection)

**Usage Pattern:**
```csharp
// Instance-based DAO
var dao = new Dao_VolvoPart(Helper_Database_Variables.GetConnectionString());

// Direct usage (rare, prefer DAOs)
await using var connection = new MySqlConnection(Helper_Database_Variables.GetConnectionString());
```

**Configuration File:** `appsettings.json` (or environment variable)
```json
{
  "ConnectionStrings": {
    "MySQL": "Server=localhost;Database=mtm_receiving;User=root;Password=..."
  }
}
```

---

## 📁 File Paths

### CSV Export Directory
**Hardcoded Location:** `%TEMP%` or user-selected via `FileOpenPicker`  
**Purpose:** Export destination for parts catalog CSV and shipment history CSV  
**Read Locations:**
- `ExportPartsCsvQueryHandler` - Uses `FileOpenPicker` for user-selected path
- `ExportShipmentsQueryHandler` - Uses `FileOpenPicker` for user-selected path

**Recommendation:** Consider adding a default export directory setting in `settings_module_volvo`.

---

### CSV Import Source
**Hardcoded Location:** User-selected via `FileOpenPicker`  
**Purpose:** Import source for parts catalog CSV  
**Read Locations:**
- `ViewModel_Volvo_Settings.ImportCsvAsync()` - Uses `FileOpenPicker` for user-selected file

**CSV Format:**
```csv
PartNumber,QuantityPerSkid
V-EMB-1,50
V-EMB-2,20
V-EMB-500,88
```

---

### Label CSV Output Directory
**Hardcoded Location:** Likely `%TEMP%` or predefined path  
**Purpose:** Barcode label CSV generation for Bartender label printer  
**Read Locations:**
- `GenerateLabelCsvQueryHandler`

**Recommendation:** Add `label_output_directory` setting to `settings_module_volvo` for deployment flexibility.

---

## 🎨 UI Configuration

### Window Size
**Source:** `WindowHelper_WindowSizeAndStartupLocation.SetWindowSize()`  
**Default:** Likely 1400x900 (inherited from project standard)  
**Purpose:** Default window size for Volvo module views  
**Read Locations:**
- `View_Volvo_History.xaml.cs`
- `View_Volvo_Settings.xaml.cs`
- `View_Volvo_ShipmentEntry.xaml.cs`

**Not directly settable** - Uses Module_Core helper.

---

### Default Shipment History Date Range
**Hardcoded Value:** 30 days  
**Location:** `ViewModel_Volvo_History` (Line 35)

```csharp
[ObservableProperty]
private DateTimeOffset? _startDate = DateTimeOffset.Now.AddDays(-30);

[ObservableProperty]
private DateTimeOffset? _endDate = DateTimeOffset.Now;
```

**Recommendation:** Consider adding `default_history_days` setting to `settings_module_volvo`.

---

### Maximum Search Results
**Hardcoded Value:** 10  
**Location:** `SearchVolvoPartsQuery` (default parameter)

```csharp
public record SearchVolvoPartsQuery : IRequest<Model_Dao_Result<List<Model_VolvoPart>>>
{
    public string SearchText { get; init; } = string.Empty;
    public int MaxResults { get; init; } = 10; // <-- Hardcoded default
}
```

**Recommendation:** Consider adding `part_search_max_results` setting to `settings_module_volvo`.

---

## 📊 Database Schema Objects

### Tables (MySQL)
- `volvo_masterdata` - Volvo parts catalog
- `volvo_shipments` - Shipment headers
- `volvo_shipment_lines` - Shipment line items
- `volvo_part_components` - Part component relationships
- `settings_module_volvo` - Module settings

### Stored Procedures (MySQL)
**Parts:**
- `sp_Volvo_Part_Get`
- `sp_Volvo_Part_GetAll`
- `sp_Volvo_Part_Search`
- `sp_Volvo_Part_Insert`
- `sp_Volvo_Part_Update`
- `sp_Volvo_Part_Deactivate`

**Shipments:**
- `sp_Volvo_Shipment_Get`
- `sp_Volvo_Shipment_GetByNumber`
- `sp_Volvo_Shipment_GetRecent`
- `sp_Volvo_Shipment_GetHistory`
- `sp_Volvo_Shipment_GetPending`
- `sp_Volvo_Shipment_GetNextNumber`
- `sp_Volvo_Shipment_Insert`
- `sp_Volvo_Shipment_Update`
- `sp_Volvo_Shipment_Complete`
- `sp_Volvo_Shipment_DeletePending`

**Shipment Lines:**
- `sp_Volvo_ShipmentLine_GetByShipmentId`
- `sp_Volvo_ShipmentLine_Insert`
- `sp_Volvo_ShipmentLine_Update`
- `sp_Volvo_ShipmentLine_Delete`
- `sp_Volvo_ShipmentLine_DeleteByShipmentId`

**Settings:**
- `sp_Volvo_Settings_Get`
- `sp_Volvo_Settings_GetAll`
- `sp_Volvo_Settings_Upsert`
- `sp_Volvo_Settings_Reset`

**Part Components:**
- `sp_Volvo_PartComponent_GetByPartNumber`
- `sp_Volvo_PartComponent_Insert`
- `sp_Volvo_PartComponent_Delete`

---

## 🚨 Hardcoded Values to Consider Externalizing

### 1. Email Fallback Recipients
**Current:** Hardcoded in `ViewModel_Volvo_ShipmentEntry.ShowEmailPreviewDialogAsync()`

```csharp
string toRecipients = FormatRecipientsFromJson(
    toResult.IsSuccess && toResult.Data != null ? toResult.Data.SettingValue : null,
    "\"Jose Rosas\" <jrosas@mantoolmfg.com>; \"Sandy Miller\" <smiller@mantoolmfg.com>; \"Steph Wittmus\" <swittmus@mantoolmfg.com>\""
);
```

**Recommendation:** Move fallback values to `appsettings.json` or make required in database.

---

### 2. Email Subject Format
**Current:** Likely hardcoded in `FormatEmailDataQueryHandler`

**Recommendation:** Add `email_subject_template` setting with placeholder support (e.g., `"Volvo Shipment #{ShipmentNumber} - PO Requisition"`).

---

### 3. Recent Shipments Lookback Period
**Current:** Hardcoded to 30 days in `GetRecentShipmentsQuery`

```csharp
public record GetRecentShipmentsQuery : IRequest<Model_Dao_Result<List<Model_VolvoShipment>>>
{
    public int Days { get; init; } = 30; // <-- Hardcoded
}
```

**Recommendation:** Add `recent_shipments_days` setting to `settings_module_volvo`.

---

### 4. Status Filter Options
**Current:** Hardcoded in `ViewModel_Volvo_History`

```csharp
[ObservableProperty]
private ObservableCollection<string> _statusOptions = new() { "All", "Pending PO", "Completed" };
```

**Recommendation:** Consider making status options dynamic if Volvo workflow changes.

---

## 📝 Configuration Best Practices

### Adding New Settings
1. **Add to Database:**
   ```sql
   CALL sp_Volvo_Settings_Upsert(
       'setting_key',
       'setting_value',
       'admin'
   );
   ```

2. **Document Here:** Add to this SETTABLE_OBJECTS.md file

3. **Read in Code:**
   ```csharp
   var dao = new Dao_VolvoSettings(connectionString);
   var result = await dao.GetSettingAsync("setting_key");
   if (result.IsSuccess && result.Data != null)
   {
       var value = result.Data.SettingValue;
       // Use value
   }
   ```

4. **Provide Fallback:** Always include a fallback value for critical settings

---

### Configuration Hierarchy
1. **Database Settings** (`settings_module_volvo`) - Highest priority (runtime-configurable via UI)
2. **appsettings.json** - Environment-specific (connection strings, file paths)
3. **Hardcoded Defaults** - Lowest priority (fallback values)

---

## 🔍 Configuration Audit Checklist

✅ **Email Recipients:** Externalized to database (`email_to_recipients`, `email_cc_recipients`)  
⚠️ **Email Fallback Values:** Hardcoded (should move to appsettings.json)  
⚠️ **Export/Import Paths:** User-selected (consider default directory setting)  
⚠️ **History Date Range:** Hardcoded 30 days (consider externalizing)  
⚠️ **Search Max Results:** Hardcoded 10 (consider externalizing)  
✅ **Connection Strings:** Externalized to appsettings.json  
✅ **Stored Procedures:** Database schema (version-controlled in `Database/Schemas/`)  

---

**For more details, see:**
- `Module_Volvo/QUICK_REF.md` - CQRS components inventory
- `Module_Volvo/PRIVILEGES.md` - Authorization requirements
- `.github/copilot-instructions.md` - Project-wide standards

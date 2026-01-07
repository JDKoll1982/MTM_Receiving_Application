# Quickstart Guide: Routing Module Development

**Feature**: 001-routing-module  
**Date**: 2026-01-04  
**Audience**: Developers implementing the Internal Routing Module

This guide provides step-by-step instructions for setting up the development environment, creating the database schema, and testing the Routing Module.

## Prerequisites

- ✅ Windows 10/11 (64-bit)
- ✅ .NET 8.0 SDK installed
- ✅ Visual Studio 2022 or VS Code with C# Dev Kit
- ✅ MySQL 8.x server accessible (connection string in appsettings.json)
- ✅ SQL Server (Infor Visual MTMFG) accessible for PO validation
- ✅ MTM Receiving Application repository cloned
- ✅ Existing authentication system (Module_Core) functional

---

## Step 1: Database Setup

### 1.1 Create Database Schema

Run the schema creation script to create all routing tables:

```bash
# From repository root
cd Database/Schemas
mysql -h <host> -u <user> -p<password> mtm_receiving_application < schema_routing.sql
```

**Expected Output**:
```
Query OK, 0 rows affected
Query OK, 0 rows affected
...
Tables created: routing_labels, routing_recipients, routing_other_reasons, routing_usage_tracking, routing_user_preferences, routing_label_history
```

**Verify Tables**:
```sql
USE mtm_receiving_application;
SHOW TABLES LIKE 'routing_%';
```

### 1.2 Create Stored Procedures

Run all routing stored procedures:

```bash
cd Database/StoredProcedures
for file in sp_routing_*.sql; do
    mysql -h <host> -u <user> -p<password> mtm_receiving_application < "$file"
done
```

**Verify Procedures**:
```sql
SHOW PROCEDURE STATUS WHERE Db = 'mtm_receiving_application' AND Name LIKE 'sp_routing%';
```

### 1.3 Load Seed Data

Load initial recipients and other reasons:

```bash
cd Database/TestData
mysql -h <host> -u <user> -p<password> mtm_receiving_application < routing_sample_data.sql
```

**Verify Seed Data**:
```sql
SELECT * FROM routing_recipients WHERE is_active = 1;
SELECT * FROM routing_other_reasons WHERE is_active = 1 ORDER BY display_order;
```

**Expected Results**:
- ~10-20 recipients (e.g., "Engineering", "Shipping", "Quality Control")
- 5 other reasons (Returned Item, Vendor Sample, Mislabeled, Customer Return, Internal Transfer)

---

## Step 2: Project Configuration

### 2.1 Update appsettings.json

Add routing module configuration:

```json
{
  "ConnectionStrings": {
    "MySQL": "Server=172.16.1.104;Port=3306;Database=mtm_receiving_application;User Id=root;Password=root;",
    "InforVisual": "Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;ApplicationIntent=ReadOnly;"
  },
  "RoutingModule": {
    "CsvExportPath": "\\\\network\\share\\routing_labels.csv",
    "EnableValidation": true,
    "DefaultMode": "WIZARD"
  }
}
```

**Configuration Fields**:
- `CsvExportPath`: Network path to CSV file for LabelView integration
- `EnableValidation`: Default validation toggle (true = query Infor Visual)
- `DefaultMode`: System-wide default mode if user has no preference

### 2.2 Register Services in App.xaml.cs

Add routing services to DI container:

```csharp
// In ConfigureServices method
private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    // ... existing services ...

    // Connection strings
    var mysqlConnectionString = configuration.GetConnectionString("MySQL");
    var inforConnectionString = configuration.GetConnectionString("InforVisual");

    // Routing DAOs
    services.AddSingleton(sp => new Dao_RoutingLabel(mysqlConnectionString));
    services.AddSingleton(sp => new Dao_RoutingRecipient(mysqlConnectionString));
    services.AddSingleton(sp => new Dao_RoutingOtherReason(mysqlConnectionString));
    services.AddSingleton(sp => new Dao_RoutingUsageTracking(mysqlConnectionString));
    services.AddSingleton(sp => new Dao_RoutingUserPreference(mysqlConnectionString));
    services.AddSingleton(sp => new Dao_RoutingLabelHistory(mysqlConnectionString));
    services.AddSingleton(sp => new Dao_InforVisualPO(inforConnectionString));

    // Routing Services
    services.AddSingleton<IRoutingService, RoutingService>();
    services.AddSingleton<IRoutingInforVisualService, RoutingInforVisualService>();
    services.AddSingleton<IRoutingRecipientService, RoutingRecipientService>();
    services.AddSingleton<IRoutingUsageTrackingService, RoutingUsageTrackingService>();
    services.AddSingleton<IRoutingUserPreferenceService, RoutingUserPreferenceService>();

    // Routing ViewModels
    services.AddTransient<RoutingModeSelectionViewModel>();
    services.AddTransient<RoutingWizardStep1ViewModel>();
    services.AddTransient<RoutingWizardStep2ViewModel>();
    services.AddTransient<RoutingWizardStep3ViewModel>();
    services.AddTransient<RoutingManualEntryViewModel>();
    services.AddTransient<RoutingEditModeViewModel>();

    // Routing Views
    services.AddTransient<RoutingModeSelectionView>();
    services.AddTransient<RoutingWizardStep1View>();
    services.AddTransient<RoutingWizardStep2View>();
    services.AddTransient<RoutingWizardStep3View>();
    services.AddTransient<RoutingManualEntryView>();
    services.AddTransient<RoutingEditModeView>();
}
```

---

## Step 3: Build and Run

### 3.1 Build Solution

```bash
# From repository root
dotnet build MTM_Receiving_Application.csproj --configuration Debug
```

**Expected Output**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### 3.2 Run Application

```bash
dotnet run --project MTM_Receiving_Application.csproj
```

**Verify Startup**:
- Application window opens
- Authentication completes (existing Module_Core auth)
- Main navigation displays "Routing" module

---

## Step 4: Manual Testing

### 4.1 Test Wizard Mode (User Story 1)

**Test Scenario**: Create a label with valid PO and Quick Add recipient

1. **Launch Module**: Click "Routing" in main navigation
2. **Mode Selection**: Verify Mode Selection screen displays with 3 options
3. **Start Wizard**: Click "Start Wizard" button
4. **Step 1 - PO Entry**:
   - Enter a valid PO number (e.g., "66754" from Infor Visual test data)
   - Press Tab or click "Next"
   - Verify line items display in list (part numbers, descriptions, quantities)
   - Select a line item
   - Click "Next"
5. **Step 2 - Recipient Selection**:
   - Verify Quick Add buttons display at top (5 buttons)
   - Verify recipient list displays sorted by usage count
   - Click a Quick Add button
   - Verify wizard immediately advances to Step 3
6. **Step 3 - Review**:
   - Verify all label details display (PO, Line, Description, Recipient, Qty)
   - Click "Create Label"
   - Verify success message with label ID
   - Verify wizard returns to Mode Selection (or Step 1 if default mode set)

**Expected Database Changes**:
```sql
-- Label created
SELECT * FROM routing_labels WHERE id = <new_label_id>;

-- Usage count incremented
SELECT * FROM routing_usage_tracking WHERE employee_number = <your_employee_number>;

-- CSV exported
SELECT csv_exported, csv_export_date FROM routing_labels WHERE id = <new_label_id>;
```

**Expected CSV File**:
```
PO,Line,Description,Recipient,Qty,Timestamp,CreatedBy,OtherReason
66754,001,Steel Brackets,Engineering,100,2026-01-04 14:30:00,6229,
```

### 4.2 Test "OTHER" PO Workflow

**Test Scenario**: Create a label without PO validation

1. **Step 1**: Enter "OTHER" as PO number
2. **Verify**: "Other Reason" dropdown displays inline
3. **Select Reason**: Choose "Vendor Sample" from dropdown
4. **Enter Details**: Manually type description and quantity
5. **Complete Wizard**: Continue through Steps 2-3 normally
6. **Verify**:
   - Label saved with po_number='OTHER'
   - other_reason_id set correctly
   - CSV shows reason code in OtherReason column

### 4.3 Test Infor Visual Connection Failure

**Test Scenario**: Graceful degradation when Visual is unreachable

1. **Simulate Outage**: Temporarily change Infor Visual connection string to invalid server
2. **Enter Valid PO**: Enter a real PO number (e.g., "66754")
3. **Verify Error**: Error message displays: "Unable to connect to ERP system. You can still create labels using 'OTHER' PO type."
4. **Verify Workflow**: User can still create labels using "OTHER" workflow
5. **Restore Connection**: Fix connection string, restart app
6. **Verify Recovery**: PO validation works again

### 4.4 Test Manual Entry Mode (User Story 2)

**Test Scenario**: Create multiple labels via grid

1. **Mode Selection**: Click "Manual Entry"
2. **Verify Grid**: Editable DataGrid displays with columns (PO, Line, Description, Recipient, Qty)
3. **Enter Row 1**:
   - Type PO number, press Tab
   - Verify description auto-fills from Infor Visual
   - Tab to Recipient cell, start typing
   - Verify autocomplete dropdown shows matching recipients
   - Select recipient, enter quantity
   - Press Enter
4. **Verify**: New blank row added
5. **Enter Rows 2-5**: Repeat for multiple labels
6. **Save All**: Click "Save All" button
7. **Verify**: All valid rows saved to database and CSV

### 4.5 Test Edit Mode (User Story 3)

**Test Scenario**: Correct a label error

1. **Mode Selection**: Click "Edit Mode"
2. **Verify Grid**: All created labels display, sorted by date DESC
3. **Search**: Type "Engineering" in search box
4. **Verify**: Grid filters in real-time
5. **Select Label**: Click a label row
6. **Edit**: Click "Edit" button (or double-click row)
7. **Modify**: Change recipient to "Shipping", change quantity to 50
8. **Save**: Click "Save Changes"
9. **Verify Database**:
   ```sql
   -- Label updated
   SELECT * FROM routing_labels WHERE id = <edited_label_id>;
   
   -- History logged
   SELECT * FROM routing_label_history WHERE label_id = <edited_label_id>;
   ```

### 4.6 Test User Preferences (User Story 4)

**Test Scenario**: Set default mode

1. **Mode Selection**: Check "Set as default mode" for "Wizard"
2. **Start Wizard**: Click "Start Wizard"
3. **Close App**: Exit application
4. **Reopen**: Launch app again
5. **Verify**: Routing module opens directly to Wizard Step 1 (skips Mode Selection)
6. **Change Default**: Click "Mode Selection" button in bottom bar
7. **Set New Default**: Check "Set as default mode" for "Manual Entry"
8. **Verify**: Reopening app launches directly to Manual Entry grid

---

## Step 5: Troubleshooting

### Issue: Stored Procedure Not Found

**Symptom**: Error message "PROCEDURE sp_routing_label_insert does not exist"

**Solution**:
```sql
-- Check if procedure exists
SHOW PROCEDURE STATUS WHERE Db = 'mtm_receiving_application' AND Name = 'sp_routing_label_insert';

-- If missing, re-run stored procedure script
mysql -h <host> -u <user> -p<password> mtm_receiving_application < Database/StoredProcedures/sp_routing_label_insert.sql
```

### Issue: Foreign Key Constraint Violation

**Symptom**: Error "Cannot add or update a child row: a foreign key constraint fails"

**Solution**:
- Verify recipient_id exists in routing_recipients table
- Verify employee_number exists in user table
- Check that recipient is active (is_active=1)

```sql
SELECT * FROM routing_recipients WHERE id = <recipient_id> AND is_active = 1;
SELECT * FROM user WHERE employee_number = <employee_number>;
```

### Issue: CSV File Locked

**Symptom**: Label saves to database but CSV export fails with "file in use" error

**Solution**:
- Close Excel or any program with CSV file open
- Check network share permissions (read/write access)
- Verify CsvExportPath in appsettings.json is correct

### Issue: Infor Visual Connection Timeout

**Symptom**: PO lookup hangs for 30+ seconds, then times out

**Solution**:
- Verify SQL Server connection string in appsettings.json
- Check network connectivity to VISUAL server
- Verify SHOP2 user has permissions on MTMFG database
- Use SQL Server Management Studio to test connection manually

### Issue: Quick Add Buttons Show Wrong Recipients

**Symptom**: Quick Add buttons don't match expected frequent recipients

**Solution**:
- Check usage_count in routing_usage_tracking table
- Verify employee has created 20+ labels (for personalization)
- If <20 labels, Quick Add shows system-wide top 5 (expected behavior)

```sql
-- Check employee label count
SELECT COUNT(*) FROM routing_labels WHERE created_by = <employee_number>;

-- Check personal usage counts
SELECT r.name, ut.usage_count
FROM routing_usage_tracking ut
INNER JOIN routing_recipients r ON ut.recipient_id = r.id
WHERE ut.employee_number = <employee_number>
ORDER BY ut.usage_count DESC
LIMIT 5;
```

---

## Step 6: Performance Validation

### Verify NFR-001: Infor Visual PO Lookup (<2 seconds)

```csharp
// Add logging in RoutingInforVisualService.GetPoLinesAsync()
var stopwatch = Stopwatch.StartNew();
var result = await _dao.GetPoLinesAsync(poNumber);
stopwatch.Stop();

if (stopwatch.ElapsedMilliseconds > 2000)
{
    await _logger.LogWarningAsync($"Slow PO lookup: {stopwatch.ElapsedMilliseconds}ms for PO {poNumber}");
}
```

**Expected**: 95% of queries complete in <2 seconds

### Verify NFR-002: Recipient Filtering (<100ms)

```csharp
// Test client-side filtering performance
var allRecipients = await _recipientService.GetActiveRecipientsSortedByUsageAsync(employeeNumber);
var stopwatch = Stopwatch.StartNew();
var filtered = _recipientService.FilterRecipients(allRecipients, "Eng");
stopwatch.Stop();

// Expected: <100ms even for 200 recipients
Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100);
```

### Verify NFR-003: CSV Write (<500ms)

```csharp
// Add logging in RoutingService.ExportLabelToCsvAsync()
var stopwatch = Stopwatch.StartNew();
await File.AppendAllTextAsync(_csvPath, FormatCsvLine(label));
stopwatch.Stop();

// Expected: <500ms for single label
Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500);
```

---

## Step 7: Integration Testing

### Test Concurrent Label Creation

**Scenario**: 10 users creating labels simultaneously (NFR-006)

1. Open 10 instances of the app (or use automated test)
2. Each user creates 1 label at the same time
3. Verify:
   - All 10 labels saved to database
   - No duplicate label IDs (AUTO_INCREMENT)
   - No deadlocks or transaction rollbacks
   - CSV file has all 10 entries (no corruption)

### Test CSV Export Retry Mechanism

**Scenario**: CSV file locked by another process

1. Open routing_labels.csv in Excel (locks file)
2. Create a label in the app
3. Verify:
   - Label saves to database (success message)
   - CSV export retries 3 times (check logs)
   - After 3 failed attempts, error logged but label still created
4. Close Excel
5. Click "Reprint" in Edit Mode
6. Verify: CSV entry now written successfully

---

## Development Workflow

### Adding a New Feature

1. **Update Spec**: Add user story to spec.md
2. **Update Data Model**: Add tables/fields to data-model.md + schema_routing.sql
3. **Create Stored Procedures**: Write sp_routing_*.sql scripts
4. **Update Models**: Add/modify C# models in Module_Routing/Models/
5. **Update DAOs**: Implement database operations
6. **Update Services**: Add business logic
7. **Update ViewModels**: Add UI logic
8. **Update Views**: Create XAML UI
9. **Test**: Follow quickstart test scenarios

### Debugging Tips

- **Use Breakpoints**: Set breakpoints in ViewModel commands to trace workflow
- **Check Logs**: Review application logs in %APPDATA%\MTM_Receiving_Application\Logs\
- **SQL Logging**: Enable MySQL query logging to debug stored procedure issues
- **Network Tracing**: Use Wireshark to debug Infor Visual connection issues
- **ViewModel State**: Use debugger to inspect ObservableProperties during wizard navigation

---

## Next Steps

After completing quickstart setup and testing:

1. ✅ **Phase 1 Complete**: All design artifacts created (plan, research, data-model, contracts, quickstart)
2. ➡️ **Phase 2**: Run `/speckit.tasks` to generate tasks.md (task breakdown by user story)
3. ➡️ **Phase 3**: Run `/speckit.implement` to execute tasks and build the module

---

**Status**: ✅ Quickstart Guide Complete - Ready for implementation

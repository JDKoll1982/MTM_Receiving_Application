# Research: Multi-Step Receiving Label Entry Workflow

**Feature**: 001-receiving-workflow  
**Date**: December 17, 2025  
**Status**: Complete

## Research Tasks

### 1. WinUI 3 Multi-Step Navigation Patterns

**Decision**: Use UserControl-based views with ViewModel-driven visibility within a single parent View

**Rationale**: 
- WinUI 3 NavigationView is best suited for top-level app navigation, not wizard-style workflows
- Using multiple UserControls within a single ReceivingWorkflowView allows:
  - Smoother transitions between steps
  - Shared ViewModel state across all steps
  - No navigation stack complexity
  - Easier progress tracking and step validation
- ViewModel controls which step's UserControl is visible via Visibility properties

**Alternatives Considered**:
- **Frame navigation**: Would require passing state between pages, complicates back/forward
- **TabView**: Not appropriate for sequential workflow (implies random access)
- **Separate windows**: Violates FR-036 (must be in NavigationView)

**Implementation**:
```xml
<UserControl x:Class="ReceivingWorkflowView">
    <StackPanel>
        <TextBlock Text="{x:Bind ViewModel.CurrentStepTitle}"/>
        
        <!-- Step 1: PO Entry -->
        <local:POEntryView Visibility="{x:Bind ViewModel.IsPOEntryVisible}"/>
        
        <!-- Step 2: Load Entry -->
        <local:LoadEntryView Visibility="{x:Bind ViewModel.IsLoadEntryVisible}"/>
        
        <!-- ... other steps ... -->
    </StackPanel>
</UserControl>
```

---

### 2. Editable DataGrid with Cascading Updates

**Decision**: Use WinUI 3 DataGrid (CommunityToolkit) with custom CellEditEnding event handler

**Rationale**:
- CommunityToolkit.WinUI.UI.Controls.DataGrid provides editable cells
- CellEditEnding event allows interception before commit
- Cascading logic in ViewModel: when Part# changes, update all loads with old Part#
- ObservableCollection with INotifyPropertyChanged ensures UI updates automatically

**Alternatives Considered**:
- **ListView with TextBoxes**: More control but significantly more XAML/code
- **Custom control**: Over-engineering for this requirement
- **Third-party grid**: Adds dependency without clear benefit

**Implementation**:
```csharp
private void DataGrid_CellEditEnding(object sender, CellEditEndingEventArgs e)
{
    if (e.Column.Tag?.ToString() == "PartID")
    {
        var editedLoad = e.Row.DataContext as ReceivingLoad;
        var newPartID = (e.EditingElement as TextBox)?.Text;
        
        if (editedLoad != null && newPartID != null)
        {
            var oldPartID = editedLoad.PartID;
            // Cascading update
            foreach (var load in AllLoads.Where(l => l.PartID == oldPartID))
            {
                load.PartID = newPartID;
            }
        }
    }
}
```

---

### 3. Session Persistence Strategy

**Decision**: Use System.Text.Json with async file I/O to %APPDATA%\MTM_Receiving_Application\session.json

**Rationale**:
- System.Text.Json is built-in, performant, and sufficient for our needs
- %APPDATA% is standard Windows location for user-specific app data
- Async file operations prevent UI blocking
- Auto-save on each step completion ensures minimal data loss
- Delete session.json after successful save to avoid stale data

**Alternatives Considered**:
- **Newtonsoft.Json**: Unnecessary dependency when System.Text.Json works
- **SQLite local DB**: Over-engineering for simple session state
- **In-memory only**: Would lose data on crash (violates FR-038)

**Implementation**:
```csharp
public class SessionManager : IService_SessionManager
{
    private readonly string _sessionPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MTM_Receiving_Application",
        "session.json");
    
    public async Task SaveSessionAsync(ReceivingSession session)
    {
        var json = JsonSerializer.Serialize(session, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_sessionPath, json);
    }
    
    public async Task<ReceivingSession?> LoadSessionAsync()
    {
        if (!File.Exists(_sessionPath)) return null;
        
        try
        {
            var json = await File.ReadAllTextAsync(_sessionPath);
            return JsonSerializer.Deserialize<ReceivingSession>(json);
        }
        catch
        {
            // Corrupted file - delete and return null
            File.Delete(_sessionPath);
            return null;
        }
    }
}
```

---

### 4. CSV File Writing with Network Path Fallback

**Decision**: Use CsvHelper library with try-catch for network path, continue on failure

**Rationale**:
- CsvHelper is industry-standard for CSV generation in .NET
- Network paths (\\mtmanu-fs01) may be unavailable due to connectivity issues
- Graceful degradation: attempt network save, warn user if it fails, but don't block
- Local CSV always succeeds (or fails entire operation if %APPDATA% is inaccessible)

**Alternatives Considered**:
- **Manual CSV construction**: Error-prone, doesn't handle escaping properly
- **Block on network failure**: Would prevent legitimate work from being saved
- **Queue for retry**: Over-engineering - user can manually sync later

**Implementation**:
```csharp
public async Task<CSVWriteResult> WriteToCSVAsync(List<ReceivingLoad> loads)
{
    var result = new CSVWriteResult();
    
    // Write to local CSV
    var localPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ReceivingData.csv");
    await WriteCSVFileAsync(localPath, loads);
    result.LocalSuccess = true;
    
    // Attempt network CSV
    try
    {
        var networkPath = @"\\mtmanu-fs01\...\JKOLL\ReceivingData.csv";
        await WriteCSVFileAsync(networkPath, loads);
        result.NetworkSuccess = true;
    }
    catch (Exception ex)
    {
        result.NetworkSuccess = false;
        result.NetworkError = ex.Message;
    }
    
    return result;
}
```

---

### 5. Infor Visual Database Query Patterns

**Decision**: Use Microsoft.Data.SqlClient with stored procedures for PO/Part queries

**⚠️ CRITICAL CONSTRAINT: Infor Visual is STRICTLY READ ONLY - NO WRITES ALLOWED**

**Connection Details**:
- **Server**: VISUAL
- **Database**: MTMFG  
- **Warehouse ID**: 002 (always)
- **Default Credentials**: Username=SHOP2, Password=SHOP
- **Access Level**: READ ONLY (SELECT queries only)

**Actual Schema** (from MTMFG database):
- `PURCHASE_ORDER` table: ID (nvarchar), VENDOR_ID, STATUS, ORDER_DATE
- `PURC_ORDER_LINE` table: PURC_ORDER_ID (FK), LINE_NO (smallint), PART_ID, ORDER_QTY, TOTAL_RECEIVED_QTY
- `PART` table: ID (nvarchar), DESCRIPTION, PRODUCT_CODE, STOCK_UM, INVENTORY_LOCKED
- `INVENTORY_TRANS` table: PURC_ORDER_ID, PART_ID, QTY, TRANSACTION_DATE, TYPE='R' (receipt), CLASS='1' (PO receipt)

**Rationale**:
- Infor Visual database is SQL Server (existing connection pattern in app)
- Stored procedures provide:
  - Better performance (query plan caching)
  - Abstraction from table structure changes
  - Security (no direct table access needed)
  - Enforced READ ONLY access
- `SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED` for performance
- `ApplicationIntent=ReadOnly` in connection string
- Async/await patterns keep UI responsive
- **All writes go to MySQL database (MTM_Receiving_Database)**, never to Infor Visual

**Alternatives Considered**:
- **Direct table queries**: More brittle, harder to maintain
- **Entity Framework**: Over-engineering for read-only queries
- **Dapper**: Possible alternative, but stored procs work with SqlClient

**Implementation**:
```csharp
public async Task<InforVisualPO> GetPOWithPartsAsync(string poNumber)
{
    using var connection = new SqlConnection(_inforVisualConnectionString);
    using var command = new SqlCommand("sp_GetPOWithParts", connection)
    {
        CommandType = CommandType.StoredProcedure
    };
    
    command.Parameters.AddWithValue("@PONumber", poNumber);
    
    await connection.OpenAsync();
    using var reader = await command.ExecuteReaderAsync();
    
    var po = new InforVisualPO { PONumber = poNumber, Parts = new List<InforVisualPart>() };
    
    while (await reader.ReadAsync())
    {
        po.Parts.Add(new InforVisualPart
        {
            PartID = reader.GetString("PartID"),
            POLineNumber = reader.GetString("POLineNumber"),
            PartType = reader.GetString("PartType"),
            QtyOrdered = reader.GetDecimal("QtyOrdered"),
            Description = reader.GetString("Description")
        });
    }
    
    return po;
}
```

---

### 6. MySQL Database Operations for Receiving Data

**Decision**: Use MySql.Data with parameterized queries for INSERT operations

**Rationale**:
- MySql.Data is official MySQL connector for .NET
- Parameterized queries prevent SQL injection
- Batch inserts for multiple loads improve performance
- Transaction support ensures atomicity (all loads saved or none)

**Alternatives Considered**:
- **Dapper**: Good option, but MySql.Data is sufficient
- **Entity Framework Core**: Over-engineering for simple INSERTs
- **Individual INSERTs**: Would be slower than batch operation

**Implementation**:
```csharp
public async Task SaveReceivingLoadsAsync(List<ReceivingLoad> loads)
{
    using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    
    using var transaction = await connection.BeginTransactionAsync();
    try
    {
        foreach (var load in loads)
        {
            var command = new MySqlCommand(
                @"INSERT INTO receiving_loads (PartID, PartType, PONumber, POLineNumber, 
                  LoadNumber, WeightQuantity, HeatLotNumber, PackagesPerLoad, 
                  PackageTypeName, WeightPerPackage, IsNonPOItem, ReceivedDate)
                  VALUES (@PartID, @PartType, @PONumber, @POLineNumber, @LoadNumber, 
                  @WeightQuantity, @HeatLotNumber, @PackagesPerLoad, @PackageTypeName, 
                  @WeightPerPackage, @IsNonPOItem, @ReceivedDate)",
                connection, transaction);
            
            // Add parameters...
            await command.ExecuteNonQueryAsync();
        }
        
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

### 7. Package Type Default Logic and Persistence

**Decision**: Implement smart defaults in ViewModel with database persistence on change

**Rationale**:
- Package type determination based on Part ID prefix is business logic → belongs in ViewModel/Service
- Default logic: `partID.StartsWith("MMC") ? "Coils" : partID.StartsWith("MMF") ? "Sheets" : ""`
- When user changes from default, persist to `package_type_preferences` table
- On subsequent entries, check preferences table first before applying default

**Alternatives Considered**:
- **Hardcode in database**: Less flexible for business rule changes
- **Configuration file**: Would require app restart to change
- **No persistence**: User would re-select every time (poor UX)

**Implementation**:
```csharp
public async Task<string> GetPackageTypeForPartAsync(string partID)
{
    // Check for saved preference first
    var preference = await _packagePreferenceService.GetPreferenceAsync(partID);
    if (preference != null)
        return preference.PackageTypeName;
    
    // Apply default logic
    if (partID.StartsWith("MMC")) return "Coils";
    if (partID.StartsWith("MMF")) return "Sheets";
    
    return ""; // No default - user must select
}

public async Task SavePackageTypePreferenceAsync(string partID, string packageType)
{
    await _packagePreferenceService.SavePreferenceAsync(new PackageTypePreference
    {
        PartID = partID,
        PackageTypeName = packageType,
        LastModified = DateTime.UtcNow
    });
}
```

---

### 8. Quick-Select Heat Number UI Pattern

**Decision**: Use CheckBox list with event handler to apply selected heat number to empty loads

**Rationale**:
- Simple UI: List of checkboxes generated from unique heat numbers entered
- User-friendly: Check a box to apply that heat number to all loads without heat numbers
- Performance: O(n) operation on checkbox check, acceptable for typical load counts (< 100)
- Fits WinUI 3 paradigm: data-driven UI with ItemsControl

**Alternatives Considered**:
- **Drag-and-drop**: More intuitive but significantly more complex
- **Copy/paste**: Less discoverable for users
- **Auto-fill first entry**: Too aggressive, might overwrite user intent

**Implementation**:
```xaml
<ItemsControl ItemsSource="{x:Bind ViewModel.UniqueHeatNumbers}">
    <ItemsControl.ItemTemplate>
        <DataTemplate x:DataType="local:HeatCheckboxItem">
            <CheckBox Content="{x:Bind HeatLotNumber}" 
                      IsChecked="{x:Bind IsChecked, Mode=TwoWay}"
                      Command="{x:Bind ApplyToEmptyLoadsCommand}"/>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

```csharp
[RelayCommand]
private void ApplyHeatToEmptyLoads(string heatNumber)
{
    foreach (var load in Loads.Where(l => string.IsNullOrEmpty(l.HeatLotNumber)))
    {
        load.HeatLotNumber = heatNumber;
    }
}
```

---

## Summary of Decisions

| Area | Technology/Pattern | Justification |
|------|-------------------|---------------|
| Navigation | UserControl-based steps in single View | Simpler than Frame navigation, shared ViewModel |
| Editable Grid | CommunityToolkit DataGrid | Built-in editing, event interception for cascading |
| Session Persistence | System.Text.Json → %APPDATA% | Standard, performant, no extra dependencies |
| CSV Writing | CsvHelper with fallback | Industry standard, graceful network failure |
| SQL Server Queries | SqlClient + Stored Procedures | Performance, security, maintainability |
| MySQL Writes | MySql.Data with transactions | Atomicity, official connector |
| Package Defaults | ViewModel logic + DB persistence | Flexible business rules, UX continuity |
| Heat Quick-Select | CheckBox list with command | Simple, discoverable, performant |

---

## Open Questions (None)

All technical clarifications have been resolved through research and decision-making above.

---

**Sign-off**: Research complete, ready for Phase 1 (Data Model & Contracts)

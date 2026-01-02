# Infor Visual Integration Architecture

## Overview

Refactored Infor Visual database integration using proper layered architecture with SQL files, DAOs, Models, and Services.

## File Structure

```
Database/InforVisualScripts/Queries/
├── 01_GetPOWithParts.sql              - Retrieve PO with all line items
├── 02_ValidatePONumber.sql            - Check if PO exists
├── 03_GetPartByNumber.sql             - Get part details
└── 04_SearchPartsByDescription.sql    - Search parts by description

Database/InforVisualTest/                - Testing sandbox for SQL queries
└── (Same files - tested versions before production)

Models/InforVisual/
├── Model_InforVisualConnection.cs     - Connection configuration model
├── Model_InforVisualPO.cs             - Purchase Order model (flat)
└── Model_InforVisualPart.cs           - Part model

Data/InforVisual/
└── Dao_InforVisualConnection.cs       - Database access layer using SQL files

Services/Database/
└── Service_InforVisualConnect.cs      - Business logic service layer

Helpers/Database/
└── Helper_SqlQueryLoader.cs           - Loads SQL files from embedded resources
```

## Architecture Layers

### 1. **SQL Query Files** (`Database/InforVisualScripts/Queries/`)
- **Purpose**: Store all SQL queries as separate `.sql` files
- **Benefits**:
  - Easy to test in SQL Server Management Studio
  - Version control friendly
  - No SQL string concatenation in C# code
  - Can be edited without recompiling

### 2. **Models** (`Models/InforVisual/`)
- **Model_InforVisualConnection**: Configuration for connection strings
- **Model_InforVisualPO**: Flat representation of PO line items (DAO layer)
- **Model_InforVisualPart**: Part information from Infor Visual

### 3. **Data Access Layer** (`Data/InforVisual/`)
- **Dao_InforVisualConnection**: 
  - Loads SQL queries using `Helper_SqlQueryLoader`
  - Executes queries against Infor Visual (SQL Server)
  - Returns `Model_Dao_Result` types
  - **READ-ONLY** - No write operations permitted

### 4. **Service Layer** (`Services/Database/`)
- **Service_InforVisualConnect**:
  - Implements `IService_InforVisual` interface
  - Converts flat DAO models to hierarchical service models
  - Handles mock data mode for testing without database
  - Business logic and validation

## Mock Data Mode

Toggle between live Infor Visual connection and mock data:

```csharp
// In App.xaml.cs ConfigureServices
var useMockData = false; // true = mock data, false = live connection
services.AddSingleton<IService_InforVisual>(sp =>
{
    var dao = sp.GetRequiredService<Dao_InforVisualConnection>();
    var logger = sp.GetService<IService_LoggingUtility>();
    return new Service_InforVisualConnect(dao, useMockData, logger);
});
```

**Benefits**:
- No `#if DEBUG` preprocessor directives
- Can be toggled at runtime via configuration
- Test application without Infor Visual server access

## SQL Query Testing Workflow

1. **Create/Edit Query** in `Database/InforVisualTest/`
2. **Test in SSMS**:
   ```sql
   -- Update DECLARE parameters with test values
   DECLARE @PoNumber VARCHAR(20) = 'PO-067101';
   
   -- Execute and verify results
   ```
3. **Document Results** in SQL file comments
4. **Copy to Production** `Database/InforVisualScripts/Queries/`
5. **Rebuild Application** - files embedded as resources
6. **Use in Code** via `Dao_InforVisualConnection`

## Connection String

```csharp
// Read-only connection with ApplicationIntent
Server=VISUAL;
Database=MTMFG;
User Id=SHOP2;
Password=SHOP;
TrustServerCertificate=True;
ApplicationIntent=ReadOnly;
```

**⚠️ CRITICAL**: `ApplicationIntent=ReadOnly` prevents accidental writes to Infor Visual.

## Usage Example

```csharp
// Inject service via DI
public class MyViewModel
{
    private readonly IService_InforVisual _inforVisual;
    
    public MyViewModel(IService_InforVisual inforVisual)
    {
        _inforVisual = inforVisual;
    }
    
    public async Task LoadPOAsync(string poNumber)
    {
        var result = await _inforVisual.GetPOWithPartsAsync(poNumber);
        
        if (result.IsSuccess && result.Data != null)
        {
            var po = result.Data;
            // Use PO data...
        }
    }
}
```

## Key Benefits

✅ **Separation of Concerns**: SQL, data access, and business logic are properly separated  
✅ **Testability**: Easy to test SQL queries independently  
✅ **Maintainability**: SQL changes don't require C# code changes  
✅ **Mock Data Support**: Can run application without Infor Visual connection  
✅ **Type Safety**: Strong typing throughout the layers  
✅ **Read-Only Safety**: `ApplicationIntent=ReadOnly` prevents accidental writes  

## Migration from Old Code

**Old** (`Service_InforVisual.cs`):
- Mixed SQL strings in C# code
- `#if DEBUG` preprocessor directives
- Hardcoded connection logic

**New** (`Service_InforVisualConnect.cs`):
- SQL in separate `.sql` files
- Runtime mock data toggle
- Proper DAO/Model/Service layers

## Configuration

To enable mock data mode, edit `App.xaml.cs`:

```csharp
var useMockData = true; // Change this to toggle mock data
```

Or load from `appsettings.json`:

```json
{
  "AppSettings": {
    "UseInforVisualMockData": false
  }
}
```

```csharp
// Load from config
var config = LoadAppSettings();
var useMockData = config.UseInforVisualMockData;
```

## Future Enhancements

- [ ] Load `useMockData` from appsettings.json
- [ ] Add connection pooling configuration
- [ ] Implement same-day receiving quantity query
- [ ] Add part type lookup (currently defaults to "FG")
- [ ] Cache frequently accessed PO/Part data

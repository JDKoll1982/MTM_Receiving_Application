# Data Model: Multi-Step Receiving Label Entry Workflow

**Feature**: 001-receiving-workflow  
**Date**: December 17, 2025

## Entity Relationship Diagram

```
┌─────────────────────────┐
│  ReceivingSession       │
│─────────────────────────│
│ + SessionID: Guid       │
│ + CreatedDate: DateTime │
│ + Loads: List<Load>     │
│ + IsNonPO: bool         │
└───────┬─────────────────┘
        │
        │ 1:N
        ▼
┌─────────────────────────────────┐
│  ReceivingLoad                  │
│─────────────────────────────────│
│ + LoadID: Guid                  │
│ + PartID: string                │
│ + PartType: string              │
│ + PONumber: string? (nullable)  │
│ + POLineNumber: string          │
│ + LoadNumber: int               │
│ + WeightQuantity: decimal       │
│ + HeatLotNumber: string         │
│ + PackagesPerLoad: int          │
│ + PackageTypeName: string       │
│ + WeightPerPackage: decimal     │
│ + IsNonPOItem: bool             │
│ + ReceivedDate: DateTime        │
└─────────────────────────────────┘

┌─────────────────────────────┐
│  InforVisualPO              │
│─────────────────────────────│
│ + PONumber: string          │
│ + Vendor: string            │
│ + Parts: List<Part>         │
└───────┬─────────────────────┘
        │
        │ 1:N
        ▼
┌─────────────────────────────┐
│  InforVisualPart            │
│─────────────────────────────│
│ + PartID: string            │
│ + POLineNumber: string      │
│ + PartType: string          │
│ + QtyOrdered: decimal       │
│ + Description: string       │
└─────────────────────────────┘

┌──────────────────────────────────┐
│  PackageTypePreference           │
│──────────────────────────────────│
│ + PreferenceID: int (PK)         │
│ + PartID: string (unique)        │
│ + PackageTypeName: string        │
│ + CustomTypeName: string?        │
│ + LastModified: DateTime         │
└──────────────────────────────────┘

┌──────────────────────────────┐
│  HeatCheckboxItem (UI Model) │
│──────────────────────────────│
│ + HeatLotNumber: string      │
│ + IsChecked: bool            │
│ + LoadNumber: int            │
└──────────────────────────────┘
```

## Entities

### ReceivingLoad (Primary Domain Entity)

Represents one load/skid of received material.

```csharp
public class Model_ReceivingLoad : ObservableObject
{
    private Guid _loadID;
    private string _partID;
    private string _partType;
    private string? _poNumber;
    private string _poLineNumber;
    private int _loadNumber;
    private decimal _weightQuantity;
    private string _heatLotNumber;
    private int _packagesPerLoad;
    private string _packageTypeName;
    private decimal _weightPerPackage;
    private bool _isNonPOItem;
    private DateTime _receivedDate;

    public Guid LoadID
    {
        get => _loadID;
        set => SetProperty(ref _loadID, value);
    }

    public string PartID
    {
        get => _partID;
        set => SetProperty(ref _partID, value);
    }

    public string PartType
    {
        get => _partType;
        set => SetProperty(ref _partType, value);
    }

    public string? PONumber
    {
        get => _poNumber;
        set => SetProperty(ref _poNumber, value);
    }

    public string POLineNumber
    {
        get => _poLineNumber;
        set => SetProperty(ref _poLineNumber, value);
    }

    public int LoadNumber
    {
        get => _loadNumber;
        set => SetProperty(ref _loadNumber, value);
    }

    public decimal WeightQuantity
    {
        get => _weightQuantity;
        set
        {
            if (SetProperty(ref _weightQuantity, value))
            {
                CalculateWeightPerPackage();
            }
        }
    }

    public string HeatLotNumber
    {
        get => _heatLotNumber;
        set => SetProperty(ref _heatLotNumber, value);
    }

    public int PackagesPerLoad
    {
        get => _packagesPerLoad;
        set
        {
            if (SetProperty(ref _packagesPerLoad, value))
            {
                CalculateWeightPerPackage();
            }
        }
    }

    public string PackageTypeName
    {
        get => _packageTypeName;
        set => SetProperty(ref _packageTypeName, value);
    }

    public decimal WeightPerPackage
    {
        get => _weightPerPackage;
        private set => SetProperty(ref _weightPerPackage, value);
    }

    public bool IsNonPOItem
    {
        get => _isNonPOItem;
        set => SetProperty(ref _isNonPOItem, value);
    }

    public DateTime ReceivedDate
    {
        get => _receivedDate;
        set => SetProperty(ref _receivedDate, value);
    }

    private void CalculateWeightPerPackage()
    {
        if (PackagesPerLoad > 0)
        {
            WeightPerPackage = Math.Round(WeightQuantity / PackagesPerLoad, 2);
        }
        else
        {
            WeightPerPackage = 0;
        }
    }

    // Display property for review grid
    public string WeightPerPackageDisplay => 
        $"{WeightPerPackage:F2} lbs per {PackageTypeName}";
}
```

**Validation Rules**:
- PartID: Required, max 50 characters
- WeightQuantity: Must be > 0
- PackagesPerLoad: Must be > 0
- HeatLotNumber: Required, max 50 characters
- PackageTypeName: Required, max 50 characters
- PONumber: Nullable (null for non-PO items), max 6 digits when present

---

### ReceivingSession

Represents the current data entry session with accumulated loads from potentially multiple parts.

```csharp
public class Model_ReceivingSession
{
    public Guid SessionID { get; set; } = Guid.NewGuid();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public List<Model_ReceivingLoad> Loads { get; set; } = new();
    public bool IsNonPO { get; set; }
    
    // Transient properties (not persisted)
    public int TotalLoadsCount => Loads.Count;
    public decimal TotalWeightQuantity => Loads.Sum(l => l.WeightQuantity);
    public List<string> UniqueParts => Loads.Select(l => l.PartID).Distinct().ToList();
}
```

**Persistence**:
- Serialized to JSON at %APPDATA%\MTM_Receiving_Application\session.json
- Auto-saved after each step completion
- Deleted after successful save to CSV/database

---

### InforVisualPO

Represents a purchase order retrieved from Infor Visual (SQL Server).

```csharp
public class Model_InforVisualPO
{
    public string PONumber { get; set; }
    public string Vendor { get; set; }
    public List<Model_InforVisualPart> Parts { get; set; } = new();
    
    public bool HasParts => Parts?.Count > 0;
}
```

**Source**: Infor Visual SQL Server database via stored procedure `sp_GetPOWithParts`

---

### InforVisualPart

Represents a part/line item on a purchase order from Infor Visual.

```csharp
public class Model_InforVisualPart
{
    public string PartID { get; set; }
    public string POLineNumber { get; set; }
    public string PartType { get; set; }
    public decimal QtyOrdered { get; set; }
    public string Description { get; set; }
    
    public string DisplayText => $"{PartID} - {Description} (Line {POLineNumber})";
}
```

**Source**: Infor Visual SQL Server database

---

### PackageTypePreference

Stores user's preferred package type for specific part IDs.

```csharp
public class Model_PackageTypePreference
{
    public int PreferenceID { get; set; }
    public string PartID { get; set; }
    public string PackageTypeName { get; set; }
    public string? CustomTypeName { get; set; }
    public DateTime LastModified { get; set; }
}
```

**Persistence**: MySQL `package_type_preferences` table

**Schema**:
```sql
CREATE TABLE package_type_preferences (
    PreferenceID INT PRIMARY KEY AUTO_INCREMENT,
    PartID VARCHAR(50) UNIQUE NOT NULL,
    PackageTypeName VARCHAR(50) NOT NULL,
    CustomTypeName VARCHAR(100),
    LastModified DATETIME NOT NULL,
    INDEX idx_partid (PartID)
);
```

---

### HeatCheckboxItem (UI Helper Model)

UI-specific model for the quick-select heat number feature.

```csharp
public class Model_HeatCheckboxItem : ObservableObject
{
    private string _heatLotNumber;
    private bool _isChecked;
    private int _firstLoadNumber;

    public string HeatLotNumber
    {
        get => _heatLotNumber;
        set => SetProperty(ref _heatLotNumber, value);
    }

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }

    public int FirstLoadNumber
    {
        get => _firstLoadNumber;
        set => SetProperty(ref _firstLoadNumber, value);
    }

    public string DisplayText => $"{HeatLotNumber} (from Load {FirstLoadNumber})";
}
```

---

## Database Schema

### MySQL Tables

#### receiving_loads

```sql
CREATE TABLE receiving_loads (
    LoadID CHAR(36) PRIMARY KEY,
    PartID VARCHAR(50) NOT NULL,
    PartType VARCHAR(50) NOT NULL,
    PONumber VARCHAR(6),
    POLineNumber VARCHAR(10) NOT NULL,
    LoadNumber INT NOT NULL,
    WeightQuantity DECIMAL(10,2) NOT NULL,
    HeatLotNumber VARCHAR(50) NOT NULL,
    PackagesPerLoad INT NOT NULL,
    PackageTypeName VARCHAR(50) NOT NULL,
    WeightPerPackage DECIMAL(10,2) NOT NULL,
    IsNonPOItem BOOLEAN NOT NULL DEFAULT FALSE,
    ReceivedDate DATETIME NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    INDEX idx_partid (PartID),
    INDEX idx_ponumber (PONumber),
    INDEX idx_receiveddate (ReceivedDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

#### package_type_preferences

```sql
CREATE TABLE package_type_preferences (
    PreferenceID INT PRIMARY KEY AUTO_INCREMENT,
    PartID VARCHAR(50) UNIQUE NOT NULL,
    PackageTypeName VARCHAR(50) NOT NULL,
    CustomTypeName VARCHAR(100),
    LastModified DATETIME NOT NULL,
    
    INDEX idx_partid (PartID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

---

### SQL Server Stored Procedures (Infor Visual - Read Only)

#### sp_GetPOWithParts

```sql
CREATE PROCEDURE sp_GetPOWithParts
    @PONumber VARCHAR(6)
AS
BEGIN
    SELECT 
        pl.PartID,
        pl.LineNumber AS POLineNumber,
        p.PartType,
        pl.QtyOrdered,
        p.Description
    FROM 
        PurchaseOrderLines pl
    INNER JOIN 
        Parts p ON pl.PartID = p.PartID
    WHERE 
        pl.PONumber = @PONumber
    ORDER BY 
        pl.LineNumber;
END
```

#### sp_GetPartByID

```sql
CREATE PROCEDURE sp_GetPartByID
    @PartID VARCHAR(50)
AS
BEGIN
    SELECT 
        PartID,
        PartType,
        Description
    FROM 
        Parts
    WHERE 
        PartID = @PartID;
END
```

#### sp_GetReceivingByPOPartDate

```sql
CREATE PROCEDURE sp_GetReceivingByPOPartDate
    @PONumber VARCHAR(6),
    @PartID VARCHAR(50),
    @Date DATE
AS
BEGIN
    SELECT 
        SUM(QtyReceived) AS TotalQtyReceived
    FROM 
        ReceivingTransactions
    WHERE 
        PONumber = @PONumber
        AND PartID = @PartID
        AND CAST(ReceivedDateTime AS DATE) = @Date;
END
```

---

## Validation Rules Summary

| Entity | Field | Rule |
|--------|-------|------|
| ReceivingLoad | PartID | Required, max 50 chars |
| ReceivingLoad | WeightQuantity | > 0, decimal(10,2) |
| ReceivingLoad | PackagesPerLoad | > 0, integer |
| ReceivingLoad | HeatLotNumber | Required, max 50 chars |
| ReceivingLoad | PackageTypeName | Required, max 50 chars |
| ReceivingLoad | PONumber | Nullable, 6 digits when present |
| InforVisualPO | PONumber | 6 digits, numeric |
| ReceivingSession | Loads | Must have at least 1 load |

---

## State Transitions

```
[New Session] 
    → PO Entry (or Non-PO Entry)
    → Part Selected
    → Loads Created (1-99)
    → Weights Entered (all > 0)
    → Heat Numbers Entered
    → Package Types Selected
    → Review Grid (editable)
    → Save in Progress
    → Save Complete
    → [Session Cleared]
```

**Alternative Path** (Multiple Parts):
```
Review Grid → "Add Another Part/PO" → Back to PO Entry
(accumulates loads in session)
Final Review → Save all loads together
```

---

## Calculated Fields

- **WeightPerPackage**: `WeightQuantity ÷ PackagesPerLoad` (rounded to 2 decimals)
- **TotalLoadsCount**: `Count(Loads)` in session
- **TotalWeightQuantity**: `Sum(Load.WeightQuantity)` in session

---

## Data Flow

1. **PO Entry**: Query Infor Visual → Return InforVisualPO with Parts
2. **Part Selection**: User selects → ReceivingLoad entities created (template)
3. **Data Entry Steps**: User fills WeightQuantity, HeatLotNumber, etc.
4. **Review**: All loads displayed in grid, editable with cascading updates
5. **Save**: 
   - Write to local CSV
   - Write to network CSV (with fallback)
   - Insert into MySQL receiving_loads
   - Update/Insert package_type_preferences if changed
   - Delete session.json

---

**Sign-off**: Data model complete, ready for contracts generation

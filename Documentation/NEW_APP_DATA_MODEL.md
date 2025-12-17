# Receiving Label Application - Data Model

## Data Model Structure

This document defines the data model for the Receiving Label Application, including C# models, database tables, and CSV format.

---

## C# Models

### Model_ReceivingLine.cs

```csharp
namespace ReceivingLabelApp.Models;

/// <summary>
/// Represents a single receiving line item (one skid of parts).
/// </summary>
public class Model_ReceivingLine
{
    #region Properties

    /// <summary>
    /// Date and time the receiving entry was created.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Delivery destination (ENUM).
    /// </summary>
    public string DeliverTo { get; set; } = "Unknown";

    /// <summary>
    /// Receiving department (ENUM).
    /// </summary>
    public string Department { get; set; } = "Unknown";

    /// <summary>
    /// Packaging/dunnage identifier.
    /// </summary>
    public string DunnageID { get; set; } = "Unknown";

    /// <summary>
    /// Type of packaging/dunnage (ENUM).
    /// </summary>
    public string DunnageType { get; set; } = "Unknown";

    /// <summary>
    /// Name of the receiving employee.
    /// </summary>
    public string EmployeeName { get; set; } = "Unknown";

    /// <summary>
    /// Employee number (4-digit ID).
    /// </summary>
    public int EmployeeNumber { get; set; }

    /// <summary>
    /// Heat/Lot number for material traceability.
    /// </summary>
    public string Heat { get; set; } = "Unknown";

    /// <summary>
    /// Auto-calculated label number for this receiving session.
    /// Starts at 1 and increments for each line added.
    /// </summary>
    public int LabelNumber { get; set; } = 1;

    /// <summary>
    /// Storage location where parts will be placed.
    /// </summary>
    public string LocatedTo { get; set; } = "Unknown";

    /// <summary>
    /// Description of the package contents.
    /// </summary>
    public string PackageDescription { get; set; } = "Unknown";

    /// <summary>
    /// Number of packages (boxes/coils/bundles) on this skid.
    /// Used to calculate weight per package: Quantity ÷ PackagesOnSkid
    /// </summary>
    public int PackagesOnSkid { get; set; }

    /// <summary>
    /// Part number/ID from Infor Visual.
    /// </summary>
    public string PartID { get; set; } = string.Empty;

    /// <summary>
    /// Part type/category (ENUM).
    /// </summary>
    public string PartType { get; set; } = "Unknown";

    /// <summary>
    /// Purchase order number (6-digit).
    /// </summary>
    public int PONumber { get; set; }

    /// <summary>
    /// Total quantity of parts on this skid.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Vendor/Supplier name from Infor Visual.
    /// </summary>
    public string VendorName { get; set; } = "Unknown";

    /// <summary>
    /// Part description from Infor Visual.
    /// </summary>
    public string PartDescription { get; set; } = string.Empty;

    /// <summary>
    /// Unit of measure (e.g., EA, LB, FT).
    /// </summary>
    public string UnitOfMeasure { get; set; } = "EA";

    /// <summary>
    /// Material weight (may equal Quantity if UOM is LB).
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Work order number (optional, for UPS/routing labels).
    /// </summary>
    public string WorkOrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Receiver tracking number (optional).
    /// </summary>
    public int ReceiverNumber { get; set; }

    #endregion

    #region Calculated Properties

    /// <summary>
    /// Calculated weight per package (for coil labels).
    /// Formula: Quantity ÷ PackagesOnSkid
    /// </summary>
    public decimal WeightPerPackage
    {
        get
        {
            if (PackagesOnSkid == 0) return 0;
            return (decimal)Quantity / PackagesOnSkid;
        }
    }

    /// <summary>
    /// Number of coil labels to generate.
    /// If PackagesOnSkid > 1, generates multiple labels.
    /// </summary>
    public int CoilLabelsToGenerate => PackagesOnSkid > 1 ? PackagesOnSkid : 1;

    /// <summary>
    /// Total weight for display (may differ from Quantity depending on UOM).
    /// </summary>
    public decimal DisplayWeight => Weight > 0 ? Weight : Quantity;

    #endregion
}
```

---

### Model_ReceivingSession.cs

```csharp
namespace ReceivingLabelApp.Models;

/// <summary>
/// Represents a receiving session (collection of lines for one or more POs).
/// </summary>
public class Model_ReceivingSession
{
    /// <summary>
    /// List of receiving line items in this session.
    /// </summary>
    public List<Model_ReceivingLine> Lines { get; set; } = new();

    /// <summary>
    /// Current employee working on this session.
    /// </summary>
    public int CurrentEmployeeNumber { get; set; }

    /// <summary>
    /// Current employee name.
    /// </summary>
    public string CurrentEmployeeName { get; set; } = string.Empty;

    /// <summary>
    /// Session start time.
    /// </summary>
    public DateTime SessionStartTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Total number of lines in session.
    /// </summary>
    public int TotalLines => Lines.Count;

    /// <summary>
    /// Auto-incrementing label number for next line.
    /// </summary>
    public int NextLabelNumber => Lines.Count > 0 ? Lines.Max(l => l.LabelNumber) + 1 : 1;

    /// <summary>
    /// Adds a line to the session with auto-calculated label number.
    /// </summary>
    public void AddLine(Model_ReceivingLine line)
    {
        line.LabelNumber = NextLabelNumber;
        line.Date = DateTime.Now;
        line.EmployeeNumber = CurrentEmployeeNumber;
        line.EmployeeName = CurrentEmployeeName;
        Lines.Add(line);
    }

    /// <summary>
    /// Clears all lines from the session (CSV reset).
    /// </summary>
    public void Reset()
    {
        Lines.Clear();
    }
}
```

---

### Model_InforVisualPO.cs

```csharp
namespace ReceivingLabelApp.Models;

/// <summary>
/// Represents PO data from Infor Visual database.
/// </summary>
public class Model_InforVisualPO
{
    /// <summary>
    /// Purchase order number.
    /// </summary>
    public int PONumber { get; set; }

    /// <summary>
    /// List of parts on this PO.
    /// </summary>
    public List<Model_InforVisualPart> Parts { get; set; } = new();
}

/// <summary>
/// Represents a part from Infor Visual database.
/// </summary>
public class Model_InforVisualPart
{
    /// <summary>
    /// Part ID/Number.
    /// </summary>
    public string PartID { get; set; } = string.Empty;

    /// <summary>
    /// Part type/category.
    /// </summary>
    public string PartType { get; set; } = string.Empty;

    /// <summary>
    /// Part description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Quantity on PO.
    /// </summary>
    public int QuantityOrdered { get; set; }

    /// <summary>
    /// Unit of measure (e.g., EA, LB, FT).
    /// </summary>
    public string UnitOfMeasure { get; set; } = string.Empty;
}
```

---

## ENUM Definitions

### Enum_DeliverTo.cs

```csharp
namespace ReceivingLabelApp.Models;

/// <summary>
/// Delivery destinations for received parts.
/// </summary>
public enum Enum_DeliverTo
{
    Unknown,
    Warehouse,
    ProductionFloor,
    QualityControl,
    Inspection,
    ShippingArea
}
```

### Enum_Department.cs

```csharp
namespace ReceivingLabelApp.Models;

/// <summary>
/// Departments handling receiving operations.
/// </summary>
public enum Enum_Department
{
    Unknown,
    Receiving,
    Shipping,
    QualityControl,
    Production,
    MaterialHandling
}
```

### Enum_DunnageType.cs

```csharp
namespace ReceivingLabelApp.Models;

/// <summary>
/// Types of packaging/dunnage.
/// </summary>
public enum Enum_DunnageType
{
    Unknown,
    Pallet,
    Crate,
    Box,
    Coil,
    Bundle,
    Skid,
    Container
}
```

### Enum_PartType.cs

```csharp
namespace ReceivingLabelApp.Models;

/// <summary>
/// Types/categories of parts.
/// </summary>
public enum Enum_PartType
{
    Unknown,
    RawMaterial,
    FinishedGoods,
    SubAssembly,
    Component,
    ToolingSupplies,
    OfficeSupplies
}
```

---

## CSV Format Specification

### File Structure

**Filename**: `ReceivingData.csv`

**Header Row**: None (data starts at row 1)

**Column Order** (must match exactly for LabelView compatibility):
```
Date,DeliverTo,Department,DunnageID,DunnageType,EmployeeName,EmployeeNumber,Heat,LabelNumber,LocatedTo,PackageDescription,PackagesOnSkid,PartID,PartType,PONumber,Quantity,VendorName,Weight,WorkOrderNumber,ReceiverNumber
```

### Example CSV Output (Full Format)

```csv
Date,DeliverTo,Department,DunnageID,DunnageType,EmployeeName,EmployeeNumber,Heat,LabelNumber,LocatedTo,PackageDescription,PackagesOnSkid,PartID,PartType,PONumber,Quantity,VendorName,Weight,WorkOrderNumber,ReceiverNumber
2025-12-15 10:30:45,Warehouse,Receiving,Unknown,Pallet,John Doe,6229,330212,1,RECV,Steel Coil 11Ga,2,MMC0000848,Raw Material,66754,3690,MST Steel Corporation,3690,,144987
2025-12-15 10:32:10,ProductionFloor,Receiving,Unknown,Skid,John Doe,6229,H123456,2,A-01-05,Aluminum Plate,1,PART-001,Raw Material,123456,500,Supplier ABC,500,,144988
2025-12-15 10:35:22,QualityControl,Receiving,DUN-001,Crate,John Doe,6229,H789012,3,B-02-10,Machined Component,1,PART-003,Finished Goods,123457,250,Vendor XYZ,250,,144989
```

### Minimal CSV Output (Coil Labels Only)

For generating only coil labels, a simplified format can be used:

```csv
Date,EmployeeNumber,Heat,PartID,PONumber,Quantity,PackagesOnSkid,LabelNumber,VendorName,Weight
2025-12-15 10:30:45,6229,330212,MMC0000848,66754,3690,2,1,MST Steel Corporation,3690
```

### Field Formats

| Field | Format | Example |
|-------|--------|---------|
| Date | `yyyy-MM-dd HH:mm:ss` | `2025-12-15 10:30:45` |
| DeliverTo | String | `Warehouse` |
| Department | String | `Receiving` |
| DunnageID | String | `DUN-001` or `Unknown` |
| DunnageType | String | `Pallet`, `Skid`, `Crate` |
| EmployeeName | String | `John Doe` |
| EmployeeNumber | Integer | `6229` |
| Heat | String | `330212`, `H123456` |
| LabelNumber | Integer | `1` (increments per line) |
| LocatedTo | String | `RECV`, `A-01-05` |
| PackageDescription | String | `Steel Coil 11Ga` |
| PackagesOnSkid | Integer | `2` (number of coils/packages) |
| PartID | String | `MMC0000848`, `PART-001` |
| PartType | String | `Raw Material`, `Finished Goods` |
| PONumber | Integer | `66754` |
| Quantity | Integer | `3690` |
| VendorName | String | `MST Steel Corporation` |
| Weight | Decimal | `3690.00` (may equal Quantity) |
| WorkOrderNumber | String | `WO-63444` (optional) |
| ReceiverNumber | Integer | `144987` (optional) |
## MySQL Database Schema

See [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md) for complete MySQL table definitions.

**Primary Table**: `label_table_receiving`

**Key Columns**:
- `id` (INT, AUTO_INCREMENT, PRIMARY KEY)
- `date` (DATETIME)
- `employee_number` (INT)
- `heat` (VARCHAR(50))
- `located_to` (VARCHAR(50))
- `packages_on_skid` (INT)
- `part_id` (VARCHAR(100))
- `part_type` (VARCHAR(100))
- `po_number` (INT)
- `quantity` (INT)
- `created_at` (TIMESTAMP, DEFAULT CURRENT_TIMESTAMP)

---

## Validation Rules

### Required Fields
- `PONumber` (must be > 0)
- `PartID` (cannot be empty)
- `Quantity` (must be > 0)
- `PackagesOnSkid` (must be > 0)
- `EmployeeNumber` (must be 4 digits)

### Optional Fields
- `Heat` (defaults to "Unknown")
- `LocatedTo` (defaults to "Unknown")
- `DeliverTo` (defaults to "Unknown")
- `Department` (defaults to "Unknown")
- `DunnageID` (defaults to "Unknown")
- `DunnageType` (defaults to "Unknown")

### Calculated Fields
- `LabelNumber` (auto-increments per session)
- `Date` (auto-populated with current DateTime)
- `WeightPerPackage` (Quantity ÷ PackagesOnSkid)

---

## Data Flow

```
Infor Visual DB (SQL Server)
         ↓
  [PO Number Entry]
         ↓
  [Part Selection]
         ↓
  Model_ReceivingLine (in-memory)
         ↓
  Model_ReceivingSession.AddLine()
         ↓
    [User Saves]
         ↓
    ┌─────────┴─────────┐
    ↓                   ↓
CSV File           MySQL Database
(LabelView)        (Reporting)
```

---

**Next**: See [USER_WORKFLOW.md](USER_WORKFLOW.md) for UI implementation guidance.

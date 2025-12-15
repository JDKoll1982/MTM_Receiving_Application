# Receiving Label Application - Overview

## Application Purpose

A WinUI 3 desktop application for managing receiving operations, generating labels, and tracking parts/packages as they arrive at the facility. The application integrates with Infor Visual database for PO data and saves receiving records to both CSV files and MySQL database.

---

## Key Features

1. **PO-Driven Data Entry**: Enter PO number and pull part information from Infor Visual
2. **Multi-Line Processing**: Handle multiple part numbers and skids per PO
3. **Heat/Lot Number Management**: Track heat numbers with smart ListView selection
4. **Package Tracking**: Record packages per skid with automatic weight calculations
5. **Dual Storage**: Save to CSV (for LabelView) and MySQL (for reporting)
6. **CSV Reset Workflow**: Optional reset with warning before starting new session

---

## Technology Stack

- **Platform**: WinUI 3 (Desktop)
- **Framework**: .NET 8.0
- **Architecture**: MVVM with CommunityToolkit.Mvvm
- **Databases**:
  - **SQL Server** (Infor Visual) - Read-only for PO/Part data
  - **MySQL** - Read/Write for receiving records
- **File Output**: CSV files for LabelView label printing system

---

## User Workflow

```
┌─────────────────────────────────────────────────────────────┐
│ Step 0: CSV Reset Prompt (Optional)                        │
│ "Do you want to reset the CSV file? This will make all     │
│  saved work unprintable."                                   │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 1: Enter PO Number                                    │
│ - User enters PO number                                    │
│ - System queries Infor Visual database                     │
│ - Retrieves all parts associated with PO                   │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 2: Select Part Number                                 │
│ - Display list of part numbers from PO                     │
│ - User selects the part they are receiving                 │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 3: Enter Skid Information                             │
│ - Enter number of lines (individual entries)               │
│ - Enter total skids for this part number                   │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 4: Enter Quantities                                   │
│ - For each line from Step 3                                │
│ - Enter individual quantity per skid                        │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 5: Enter Heat Numbers                                 │
│ - For each line from Step 3                                │
│ - Enter heat/lot number                                     │
│ - System generates ListView with checkboxes                 │
│ - Quick select for repeated heat numbers                    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 6: Enter Packages Per Skid                            │
│ - For each line from Step 3                                │
│ - Enter number of packages/boxes/coils on skid             │
│ - Used to calculate weight per package (Qty ÷ Packages)    │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 7: Save or Continue                                   │
│ - Option A: Return to Step 1 (process another PO/Part)     │
│ - Option B: Save to CSV and MySQL                          │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 8: Save to CSV                                        │
│ - Save to: %APPDATA%\ReceivingData.csv                     │
│ - Save to: \\mtmanu-fs01\Expo Drive\Receiving\             │
│            LabelViewQuickPrint\CSV Files\{Username}\       │
│                                                             │
│ CSV Format: One row per line entered                       │
│ Columns: Date, EmployeeNumber, Heat, LocatedTo,            │
│          PackagesOnSkid, PartID, PartType,                  │
│          PONumber, Quantity                                 │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│ Step 9: Save to MySQL Database                             │
│ - TBD: Save receiving records to MySQL                     │
│ - For reporting and historical tracking                     │
└─────────────────────────────────────────────────────────────┘
```

---

## Integration Points

### Infor Visual Database (SQL Server)
- **Purpose**: Read-only data source for PO information
- **Queries**:
  - Get PO details by PO number
  - Get part list for PO
  - Get part details (PartID, PartType, etc.)
- **Connection**: Reuse `Helper_SqlServer_StoredProcedure` from MTM WIP app

### MySQL Database
- **Purpose**: Store receiving records for reporting
- **Tables**: TBD (to be designed)
- **Connection**: Reuse `Helper_MySQL_StoredProcedure` from MTM WIP app

### CSV File Output
- **Purpose**: LabelView label printing system integration
- **Format**: Comma-separated, one row per line item
- **Locations**:
  - Local backup: `%APPDATA%\ReceivingData.csv`
  - Network production: `\\mtmanu-fs01\Expo Drive\Receiving\LabelViewQuickPrint\CSV Files\{Username}\ReceivingData.csv`

---

## Data Model Summary

| Field | Type | Size | Default | Notes |
|-------|------|------|---------|-------|
| Date | DateTime | - | Current DateTime | Auto-populated |
| DeliverTo | String (ENUM) | 100 | "Unknown" | Delivery destination |
| Department | String (ENUM) | 100 | "Unknown" | Receiving department |
| DunnageID | String | 100 | "Unknown" | Packaging identifier |
| DunnageType | String (ENUM) | 100 | "Unknown" | Type of packaging |
| EmployeeName | String | 100 | "Unknown" | Name of receiving employee |
| EmployeeNumber | Int | 4 digits | Unknown | Employee ID |
| Heat | String | 50 | "Unknown" | Heat/Lot number |
| LabelNumber | Int | 3 digits | 1 | Auto-calculated in app |
| LocatedTo | String | 50 | "Unknown" | Storage location |
| PackageDescription | String | 100 | "Unknown" | Description of package |
| PackagesOnSkid | Int | 6 digits | 0 | Count of packages per skid |
| PartID | String | 100 | Empty | Part number |
| PartType | String (ENUM) | 100 | "Unknown" | Type/category of part |
| PONumber | Int | 6 digits | 0 | Purchase order number |
| Quantity | Int | 7 digits | 0 | Total quantity on skid |

---

## ENUM Values (To Be Defined)

The following fields use ENUM constraints (fixed list of values):
- **DeliverTo**: (e.g., Warehouse, Production Floor, Inspection)
- **Department**: (e.g., Receiving, Shipping, Quality Control)
- **DunnageType**: (e.g., Pallet, Crate, Box, Coil)
- **PartType**: (e.g., Raw Material, Finished Goods, Sub-Assembly)

These values should be configurable in the application settings or database.

---

## Next Steps

1. Review [DATA_MODEL.md](DATA_MODEL.md) for detailed data structure
2. Review [USER_WORKFLOW.md](USER_WORKFLOW.md) for step-by-step UI mockups
3. Review [DATABASE_SCHEMA.md](DATABASE_SCHEMA.md) for MySQL table definitions
4. Review [INFOR_VISUAL_INTEGRATION.md](INFOR_VISUAL_INTEGRATION.md) for SQL Server queries
5. Review [FILE_OPERATIONS.md](FILE_OPERATIONS.md) for CSV handling logic

---

**Project Status**: Specification Phase  
**Target Platform**: WinUI 3 Desktop (.NET 8.0)  
**Created**: December 15, 2025

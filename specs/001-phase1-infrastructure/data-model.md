# Phase 1: Data Model

**Feature**: Phase 1 Infrastructure Setup  
**Date**: December 15, 2025  
**Status**: Complete

## Overview

This document defines the data model for the Phase 1 infrastructure, including entities, relationships, validation rules, and state transitions. The models are designed to match both the MySQL database schema and the existing Google Sheets structure used by the manufacturing floor.

## Core Entities

### Model_Dao_Result

**Purpose**: Standardized response object for all database operations

**Properties**:

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| Success | bool | Operation succeeded | false |
| ErrorMessage | string | Error description | string.Empty |
| Severity | Enum_ErrorSeverity | Error severity level | Info |
| AffectedRows | int | Rows inserted/updated/deleted | 0 |
| ExecutionTimeMs | long | Operation duration | 0 |
| ReturnValue | object? | Optional return data | null |

**Usage Pattern**:
```csharp
var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);
if (result.Success)
{
    // Handle success
    Console.WriteLine($"Inserted {result.AffectedRows} rows in {result.ExecutionTimeMs}ms");
}
else
{
    // Handle error
    await Service_ErrorHandler.HandleErrorAsync(result.ErrorMessage, result.Severity);
}
```

**Validation Rules**:
- None (this is a result object, not input data)

---

### Model_Application_Variables

**Purpose**: Application-wide configuration and constants

**Properties**:

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| ApplicationName | string | Display name | "MTM Receiving Label Application" |
| Version | string | Semantic version | "1.0.0" |
| ConnectionString | string | MySQL connection | From Helper_Database_Variables |
| LogDirectory | string | Log file path | %APPDATA%\MTM_Receiving_Application\Logs\ |
| EnvironmentType | Enum_Environment | Dev/Test/Prod | Dev |

**Validation Rules**:
- ConnectionString must not be empty
- Version must follow semantic versioning (MAJOR.MINOR.PATCH)
- LogDirectory must be writable

---

### Model_ReceivingLine

**Purpose**: Represents a receiving label entry from Google Sheets "Receiving Data"

**Properties**:

| Property | Type | Description | Database Column | Sheet Column |
|----------|------|-------------|----------------|--------------|
| Id | int | Auto-increment primary key | id | N/A |
| Quantity | int | Number of pieces | quantity | A |
| PartID | string | Part identifier | part_id | B |
| PONumber | int | Purchase order number | po_number | C |
| EmployeeNumber | int | Receiving employee | employee_number | D |
| Heat | string | Heat/lot number | heat | E |
| Date | DateTime | Transaction date | transaction_date | F |
| InitialLocation | string | Storage location | initial_location | G |
| CoilsOnSkid | int? | Optional coil count | coils_on_skid | H |
| LabelNumber | int | Current label number | label_number | Calculated |
| TotalLabels | int | Total labels for PO | N/A (calculated) | Calculated |
| VendorName | string | Supplier name | vendor_name | Lookup |
| PartDescription | string | Part description | part_description | Lookup |
| CreatedAt | DateTime | Record creation time | created_at | N/A |

**Calculated Properties**:
```csharp
public string LabelText => $"{LabelNumber} / {TotalLabels}";
```

**Validation Rules**:
- Quantity > 0
- PartID not empty, max length 50
- PONumber > 0
- EmployeeNumber > 0
- Heat max length 100 (can be empty)
- Date >= 1900-01-01 (reasonable historical date)
- InitialLocation max length 50
- CoilsOnSkid >= 1 if provided
- VendorName max length 255
- PartDescription max length 500

**Relationships**:
- Many receiving lines per PO number
- Many receiving lines per part ID
- Many receiving lines per employee number

**State Transitions**:
1. **New**: Instance created in memory, Id = 0
2. **Validated**: All validation rules passed
3. **Persisted**: Inserted to database, Id > 0, CreatedAt populated
4. **Queried**: Retrieved from database for display

---

### Model_DunnageLine

**Purpose**: Represents a dunnage label entry from Google Sheets "Dunnage Data"

**Properties**:

| Property | Type | Description | Database Column | Sheet Column |
|----------|------|-------------|----------------|--------------|
| Id | int | Auto-increment primary key | id | N/A |
| Line1 | string | First line of text | line1 | A |
| Line2 | string | Second line of text | line2 | B |
| PONumber | int | Purchase order number | po_number | C |
| Date | DateTime | Transaction date | transaction_date | D |
| EmployeeNumber | int | Creating employee | employee_number | E |
| VendorName | string | Supplier name | vendor_name | F (or lookup) |
| Location | string | Storage location | location | G |
| LabelNumber | int | Current label number | label_number | Calculated |
| CreatedAt | DateTime | Record creation time | created_at | N/A |

**Validation Rules**:
- Line1 not empty, max length 100
- Line2 max length 100 (can be empty)
- PONumber > 0
- Date >= 1900-01-01
- EmployeeNumber > 0
- VendorName max length 255 (default: "Unknown")
- Location max length 50

**Relationships**:
- Many dunnage lines per PO number
- Many dunnage lines per employee number

---

### Model_RoutingLabel

**Purpose**: Represents a routing label for internal package delivery

**Properties**:

| Property | Type | Description | Database Column | Sheet Column |
|----------|------|-------------|----------------|--------------|
| Id | int | Auto-increment primary key | id | N/A |
| DeliverTo | string | Recipient name/dept | deliver_to | A |
| Department | string | Target department | department | B |
| PackageDescription | string | Contents description | package_description | C |
| PONumber | int | Related PO (optional) | po_number | D |
| WorkOrderNumber | string | Related WO (optional) | work_order_number | E |
| EmployeeNumber | int | Creating employee | employee_number | F |
| LabelNumber | int | Label number | label_number | G |
| Date | DateTime | Transaction date | transaction_date | H |
| CreatedAt | DateTime | Record creation time | created_at | N/A |

**Validation Rules**:
- DeliverTo not empty, max length 100
- Department not empty, max length 100
- PackageDescription max length 255
- PONumber >= 0 (0 means no PO)
- WorkOrderNumber max length 50
- EmployeeNumber > 0
- LabelNumber >= 1
- Date >= 1900-01-01

**Relationships**:
- Many routing labels per department
- Many routing labels per employee number
- Optional link to PO number (may be 0)

---

## Enumerations

### Enum_ErrorSeverity

```csharp
public enum Enum_ErrorSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3,
    Fatal = 4
}
```

**Usage**: Categorize errors in Model_Dao_Result and Service_ErrorHandler

---

### Enum_DatabaseEnum_ErrorSeverity

```csharp
public enum Enum_DatabaseEnum_ErrorSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2,
    Critical = 3,
    Fatal = 4
}
```

**Usage**: Maps to database-stored error severity values for logging

---

### Enum_LabelType

```csharp
public enum Enum_LabelType
{
    Receiving = 1,
    Dunnage = 2,
    UPSFedEx = 3,
    MiniReceiving = 4,
    MiniCoil = 5
}
```

**Usage**: Identify label type for printing and display logic

---

## Entity Relationships

```
PO Number (PONumber)
├── Model_ReceivingLine (many)
├── Model_DunnageLine (many)
└── Model_RoutingLabel (many, optional)

Part ID (PartID)
└── Model_ReceivingLine (many)

Employee Number (EmployeeNumber)
├── Model_ReceivingLine (many)
├── Model_DunnageLine (many)
└── Model_RoutingLabel (many)

Department
└── Model_RoutingLabel (many)
```

**Key Relationships**:
- **One PO → Many Labels**: A single purchase order can generate multiple receiving labels, dunnage labels, and routing labels
- **One Part → Many Receiving Lines**: Parts are received multiple times over time
- **One Employee → Many Labels**: Employees create multiple labels during their shifts
- **One Department → Many Routing Labels**: Departments receive multiple packages

---

## Database Schema Mapping

### receiving_lines Table

```sql
CREATE TABLE receiving_lines (
  id INT AUTO_INCREMENT PRIMARY KEY,
  quantity INT NOT NULL,
  part_id VARCHAR(50) NOT NULL,
  po_number INT NOT NULL,
  employee_number INT NOT NULL,
  heat VARCHAR(100),
  transaction_date DATE NOT NULL,
  initial_location VARCHAR(50),
  coils_on_skid INT,
  label_number INT DEFAULT 1,
  vendor_name VARCHAR(255),
  part_description VARCHAR(500),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_part_id (part_id),
  INDEX idx_po_number (po_number),
  INDEX idx_date (transaction_date),
  INDEX idx_employee (employee_number)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Maps to**: Model_ReceivingLine

---

### dunnage_lines Table

```sql
CREATE TABLE dunnage_lines (
  id INT AUTO_INCREMENT PRIMARY KEY,
  line1 VARCHAR(100) NOT NULL,
  line2 VARCHAR(100),
  po_number INT NOT NULL,
  transaction_date DATE NOT NULL,
  employee_number INT NOT NULL,
  vendor_name VARCHAR(255) DEFAULT 'Unknown',
  location VARCHAR(50),
  label_number INT DEFAULT 1,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_po (po_number),
  INDEX idx_date (transaction_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Maps to**: Model_DunnageLine

---

### routing_labels Table

```sql
CREATE TABLE routing_labels (
  id INT AUTO_INCREMENT PRIMARY KEY,
  deliver_to VARCHAR(100) NOT NULL,
  department VARCHAR(100) NOT NULL,
  package_description VARCHAR(255),
  po_number INT NOT NULL,
  work_order_number VARCHAR(50),
  employee_number INT NOT NULL,
  label_number INT NOT NULL,
  transaction_date DATE NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_deliver_to (deliver_to),
  INDEX idx_department (department),
  INDEX idx_date (transaction_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

**Maps to**: Model_RoutingLabel

---

## Data Flow

### Creating a Receiving Label

```
1. User Input (future UI)
   ↓
2. Model_ReceivingLine instance created
   ↓
3. Validation rules checked
   ↓
4. Dao_ReceivingLine.InsertReceivingLineAsync()
   ↓
5. Helper_Database_StoredProcedure.Execute()
   ↓
6. Stored procedure: receiving_line_Insert
   ↓
7. Database insert with transaction
   ↓
8. Model_Dao_Result returned
   ↓
9. Service_ErrorHandler (if error)
   ↓
10. LoggingUtility (always)
```

---

## Validation Strategy

**Client-Side Validation** (in models/ViewModels - Phase 2):
- Required field checks
- String length limits
- Numeric range checks
- Format validation (dates, numbers)

**Database-Side Validation** (in stored procedures):
- Foreign key constraints (if referential integrity added)
- Unique constraints (if needed)
- Business rule validation (e.g., PO must exist)
- Transaction-level consistency checks

**Model_Dao_Result Integration**:
- Validation failures return: `Success = false`, `ErrorMessage = "Validation failed: {details}"`
- Database errors return: `Success = false`, `ErrorMessage = "Database error: {details}"`
- Success returns: `Success = true`, `AffectedRows = 1`, `ExecutionTimeMs = {time}`

---

## Future Extensions (Phase 2+)

- Add foreign key relationships to vendor/part master tables
- Add audit trail columns (modified_by, modified_at)
- Add soft delete support (is_deleted flag)
- Add version tracking for optimistic concurrency
- Add computed columns in database for frequently-used calculations
- Add full-text search indexes for part descriptions

---

## Implementation Readiness

✅ **Data model complete** - All entities, validations, and relationships defined

Ready for DAO implementation and stored procedure creation.

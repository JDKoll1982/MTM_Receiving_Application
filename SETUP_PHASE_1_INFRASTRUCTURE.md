# MTM Receiving Application - Phase 1: Infrastructure Setup

**Purpose**: Establish foundational infrastructure (database layer, services, helpers) before building MVVM features.

**Prerequisites**: 
- .NET 8.0 SDK installed
- MySQL 5.7.24 server accessible
- Visual Studio 2022 or VS Code with C# extension

**Estimated Time**: 2-3 hours

---

## Overview

This phase establishes the core infrastructure required for the receiving label application. Complete this **before** attempting to build any MVVM features.

**Phase 1 Components**:
1. Copy template files from MTM WIP Application
2. Create core models and enums
3. Set up database helpers
4. Configure services (error handling, logging)
5. Establish DAO pattern
6. Create GitHub instruction files for Copilot
7. Initialize project documentation

---

## Step 1: Copy Template Files

Run this PowerShell script from the MTM_WIP_Application_WinForms directory:

```powershell
$source = "C:\Users\johnk\source\repos\MTM_WIP_Application_WinForms"
$dest = "C:\Users\johnk\source\repos\MTM_Receiving_Application"

# Models - Core templates
Copy-Item "$source\Models\Core\Model_Dao_Result.cs" "$dest\Models\Receiving\_TEMPLATE_Model_Dao_Result.txt" -Force
Copy-Item "$source\Models\Core\Model_Application_Variables.cs" "$dest\Models\Receiving\_TEMPLATE_Model_Application_Variables.txt" -Force

# Enums
Copy-Item "$source\Models\Enums\Enum_ErrorSeverity.cs" "$dest\Models\Enums\_TEMPLATE_Enum_ErrorSeverity.txt" -Force
Copy-Item "$source\Models\Enums\Enum_DatabaseEnum_ErrorSeverity.cs" "$dest\Models\Enums\_TEMPLATE_Enum_DatabaseEnum_ErrorSeverity.txt" -Force

# Helpers - Database
Copy-Item "$source\Helpers\Helper_Database_StoredProcedure.cs" "$dest\Helpers\Database\_TEMPLATE_Helper_Database_StoredProcedure.txt" -Force
Copy-Item "$source\Helpers\Helper_Database_Variables.cs" "$dest\Helpers\Database\_TEMPLATE_Helper_Database_Variables.txt" -Force
Copy-Item "$source\Helpers\Helper_StoredProcedureProgress.cs" "$dest\Helpers\Database\_TEMPLATE_Helper_StoredProcedureProgress.txt" -Force

# Helpers - Validation & Formatting
Copy-Item "$source\Helpers\Helper_ValidatedTextBox.cs" "$dest\Helpers\Validation\_TEMPLATE_Helper_ValidatedTextBox.txt" -Force
Copy-Item "$source\Helpers\Helper_ExportManager.cs" "$dest\Helpers\Formatting\_TEMPLATE_Helper_ExportManager.txt" -Force

# Services
Copy-Item "$source\Services\ErrorHandling\Service_ErrorHandler.cs" "$dest\Services\Database\_TEMPLATE_Service_ErrorHandler.txt" -Force
Copy-Item "$source\Services\Logging\LoggingUtility.cs" "$dest\Services\Database\_TEMPLATE_LoggingUtility.txt" -Force
Copy-Item "$source\Services\Debugging\Service_DebugTracer.cs" "$dest\Services\Database\_TEMPLATE_Service_DebugTracer.txt" -Force

# Contracts
Copy-Item "$source\Services\ErrorHandling\IService_ErrorHandler.cs" "$dest\Contracts\Services\_TEMPLATE_IService_ErrorHandler.txt" -Force
Copy-Item "$source\Services\Logging\ILoggingService.cs" "$dest\Contracts\Services\_TEMPLATE_ILoggingService.txt" -Force

# Data Layer Examples
Copy-Item "$source\Data\Dao_User.cs" "$dest\Data\Receiving\_TEMPLATE_Dao_User.txt" -Force
Copy-Item "$source\Data\Dao_Inventory.cs" "$dest\Data\Receiving\_TEMPLATE_Dao_Inventory.txt" -Force

Write-Host "✓ All template files copied to proper subfolders" -ForegroundColor Green
```

**Verification**: Check that 16 `_TEMPLATE_*.txt` files exist in their respective subfolders.

---

## Step 2: Create Core Models

### 2.1 Model_Dao_Result (Copy from template, update namespace)

**File**: `Models/Receiving/Model_Dao_Result.cs`

```powershell
# Convert template to C# file
$template = Get-Content "Models\Receiving\_TEMPLATE_Model_Dao_Result.txt" -Raw
$updated = $template -replace 'MTM_WIP_Application_Winforms', 'MTM_Receiving_Application'
Set-Content "Models\Receiving\Model_Dao_Result.cs" $updated
```

### 2.2 Model_Application_Variables

**File**: `Models/Receiving/Model_Application_Variables.cs`

**Instructions**: Adapt from template, change:
- Namespace to `MTM_Receiving_Application.Models`
- Connection string to point to receiving database
- Application name to "MTM Receiving Label Application"
- Version to 1.0.0

### 2.3 Create Receiving-Specific Models

**File**: `Models/Receiving/Model_ReceivingLine.cs`

```csharp
namespace MTM_Receiving_Application.Models.Receiving;

/// <summary>
/// Represents a single receiving label line entry.
/// Matches Google Sheets "Receiving Data" structure.
/// </summary>
public class Model_ReceivingLine
{
    // Column A
    public int Quantity { get; set; }
    
    // Column B
    public string PartID { get; set; } = string.Empty;
    
    // Column C
    public int PONumber { get; set; }
    
    // Column D
    public int EmployeeNumber { get; set; }
    
    // Column E
    public string Heat { get; set; } = string.Empty;
    
    // Column F
    public DateTime Date { get; set; } = DateTime.Now;
    
    // Column G
    public string InitialLocation { get; set; } = string.Empty;
    
    // Column H (Optional)
    public int? CoilsOnSkid { get; set; }
    
    // Calculated
    public int LabelNumber { get; set; } = 1;
    public int TotalLabels { get; set; } = 1;
    public string LabelText => $"{LabelNumber} / {TotalLabels}";
    
    // Vendor lookup (from Infor Visual or history)
    public string VendorName { get; set; } = "Unknown";
    
    // Part description (from Infor Visual)
    public string PartDescription { get; set; } = string.Empty;
}
```

**Repeat for**:
- `Model_DunnageLine.cs`
- `Model_RoutingLabel.cs`

---

## Step 3: Create Enums

### 3.1 Copy Enum Templates

```powershell
# Error severity enums (reuse from WIP app)
Copy-Item "Models\Enums\_TEMPLATE_Enum_ErrorSeverity.txt" "Models\Enums\Enum_ErrorSeverity.cs"
Copy-Item "Models\Enums\_TEMPLATE_Enum_DatabaseEnum_ErrorSeverity.txt" "Models\Enums\Enum_DatabaseEnum_ErrorSeverity.cs"

# Update namespaces
Get-ChildItem "Models\Enums\*.cs" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content -replace 'MTM_WIP_Application_Winforms', 'MTM_Receiving_Application' | Set-Content $_.FullName
}
```

### 3.2 Create Receiving-Specific Enums

**File**: `Models/Enums/Enum_LabelType.cs`

```csharp
namespace MTM_Receiving_Application.Models.Enums;

public enum Enum_LabelType
{
    Receiving = 1,
    Dunnage = 2,
    UPSFedEx = 3,
    MiniReceiving = 4,
    MiniCoil = 5
}
```

---

## Step 4: Set Up Database Helpers

### 4.1 Helper_Database_Variables

**File**: `Helpers/Database/Helper_Database_Variables.cs`

**Instructions**:
1. Copy from `_TEMPLATE_Helper_Database_Variables.txt`
2. Update namespace to `MTM_Receiving_Application.Helpers.Database`
3. Change connection strings:
   ```csharp
   public static string ProductionConnectionString => 
       "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=root;";
   
   public static string TestConnectionString => 
       "Server=localhost;Port=3306;Database=mtm_receiving_application_test;Uid=root;Pwd=root;";
   ```

### 4.2 Helper_Database_StoredProcedure

**File**: `Helpers/Database/Helper_Database_StoredProcedure.cs`

**Instructions**:
1. Copy entire file from template
2. Update namespace
3. Remove WinForms-specific dependencies (replace with WinUI 3 equivalents)
4. Keep all retry logic, performance monitoring, Model_Dao_Result integration

---

## Step 5: Configure Services

### 5.1 Service_ErrorHandler

**File**: `Services/Database/Service_ErrorHandler.cs`

**Key Changes**:
- Replace `MessageBox.Show` with WinUI 3 `ContentDialog`
- Replace `Form` references with `Window` or `Page` references
- Update namespace to `MTM_Receiving_Application.Services.Database`

### 5.2 LoggingUtility

**File**: `Services/Database/LoggingUtility.cs`

**Instructions**:
- Copy from template
- Update log file path to `%APPDATA%\MTM_Receiving_Application\Logs\`
- Update namespace

### 5.3 Create Contracts

**Files**:
- `Contracts/Services/IService_ErrorHandler.cs`
- `Contracts/Services/ILoggingService.cs`

Copy from templates, update namespaces.

---

## Step 6: Establish DAO Pattern

### 6.1 Create Base DAO Structure

**File**: `Data/Receiving/Dao_ReceivingLine.cs`

```csharp
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models;
using MTM_Receiving_Application.Models.Receiving;

namespace MTM_Receiving_Application.Data.Receiving;

/// <summary>
/// Data Access Object for receiving line operations.
/// Implements Model_Dao_Result pattern with async/await.
/// </summary>
public class Dao_ReceivingLine
{
    /// <summary>
    /// Inserts a new receiving line entry.
    /// </summary>
    /// <param name="line">The receiving line data</param>
    /// <returns>Model_Dao_Result indicating success/failure</returns>
    public static async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                ["Quantity"] = line.Quantity,
                ["PartID"] = line.PartID,
                ["PONumber"] = line.PONumber,
                ["EmployeeNumber"] = line.EmployeeNumber,
                ["Heat"] = line.Heat,
                ["Date"] = line.Date,
                ["InitialLocation"] = line.InitialLocation,
                ["CoilsOnSkid"] = line.CoilsOnSkid ?? (object)DBNull.Value
            };

            var result = await Helper_Database_StoredProcedure.ExecuteNonQueryWithStatusAsync(
                Model_Application_Variables.ConnectionString,
                "receiving_line_Insert",
                parameters
            );

            if (!result.IsSuccess)
                return Model_Dao_Result.Failure(result.StatusMessage);

            return Model_Dao_Result.Success("Receiving line inserted successfully", result.RowsAffected);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result.Failure($"Failed to insert receiving line: {ex.Message}", ex);
        }
    }

    // TODO: Add GetAllAsync, GetByIdAsync, UpdateAsync, DeleteAsync, etc.
}
```

**Repeat for**:
- `Dao_DunnageLine.cs`
- `Dao_RoutingLabel.cs`

---

## Step 7: Create Database Schema

### 7.1 Create Tables

**File**: `Database/Schemas/01_create_receiving_tables.sql`

```sql
-- Receiving Lines Table
CREATE TABLE IF NOT EXISTS `receiving_lines` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `quantity` INT NOT NULL,
  `part_id` VARCHAR(50) NOT NULL,
  `po_number` INT NOT NULL,
  `employee_number` INT NOT NULL,
  `heat` VARCHAR(100),
  `transaction_date` DATE NOT NULL,
  `initial_location` VARCHAR(50),
  `coils_on_skid` INT,
  `label_number` INT DEFAULT 1,
  `vendor_name` VARCHAR(255),
  `part_description` VARCHAR(500),
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_part_id (`part_id`),
  INDEX idx_po_number (`po_number`),
  INDEX idx_date (`transaction_date`),
  INDEX idx_employee (`employee_number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Dunnage Lines Table
CREATE TABLE IF NOT EXISTS `dunnage_lines` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `line1` VARCHAR(100) NOT NULL,
  `line2` VARCHAR(100),
  `po_number` INT NOT NULL,
  `transaction_date` DATE NOT NULL,
  `employee_number` INT NOT NULL,
  `vendor_name` VARCHAR(255) DEFAULT 'Unknown',
  `location` VARCHAR(50),
  `label_number` INT DEFAULT 1,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_po (`po_number`),
  INDEX idx_date (`transaction_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Routing Labels Table
CREATE TABLE IF NOT EXISTS `routing_labels` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `deliver_to` VARCHAR(100) NOT NULL,
  `department` VARCHAR(100) NOT NULL,
  `package_description` VARCHAR(255),
  `po_number` INT NOT NULL,
  `work_order_number` VARCHAR(50),
  `employee_number` INT NOT NULL,
  `label_number` INT NOT NULL,
  `transaction_date` DATE NOT NULL,
  `created_at` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_deliver_to (`deliver_to`),
  INDEX idx_department (`department`),
  INDEX idx_date (`transaction_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### 7.2 Create Stored Procedures

**File**: `Database/StoredProcedures/Receiving/receiving_line_Insert.sql`

```sql
DELIMITER $$

DROP PROCEDURE IF EXISTS `receiving_line_Insert` $$

CREATE PROCEDURE `receiving_line_Insert`(
    IN p_Quantity INT,
    IN p_PartID VARCHAR(50),
    IN p_PONumber INT,
    IN p_EmployeeNumber INT,
    IN p_Heat VARCHAR(100),
    IN p_Date DATE,
    IN p_InitialLocation VARCHAR(50),
    IN p_CoilsOnSkid INT,
    OUT p_Status INT,
    OUT p_ErrorMsg VARCHAR(500)
)
BEGIN
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 p_ErrorMsg = MESSAGE_TEXT;
        SET p_Status = -1;
        ROLLBACK;
    END;

    START TRANSACTION;

    INSERT INTO receiving_lines (
        quantity, part_id, po_number, employee_number, heat,
        transaction_date, initial_location, coils_on_skid
    ) VALUES (
        p_Quantity, p_PartID, p_PONumber, p_EmployeeNumber, p_Heat,
        p_Date, p_InitialLocation, p_CoilsOnSkid
    );

    SET p_Status = 1;
    SET p_ErrorMsg = 'Receiving line inserted successfully';

    COMMIT;
END $$

DELIMITER ;
```

**Run SQL Scripts**:
```powershell
& "C:\MAMP\bin\mysql\bin\mysql.exe" -h localhost -P 3306 -u root -proot < Database/Schemas/01_create_receiving_tables.sql
& "C:\MAMP\bin\mysql\bin\mysql.exe" -h localhost -P 3306 -u root -proot mtm_receiving_application < Database/StoredProcedures/Receiving/receiving_line_Insert.sql
```

---

## Step 8: Create GitHub Instruction Files

Create these instruction files in `.github/instructions/`:

### 8.1 Database Layer Instructions

**File**: `.github/instructions/database-layer.instructions.md`

See Section 9 below for content.

### 8.2 Service Layer Instructions

**File**: `.github/instructions/service-layer.instructions.md`

See Section 9 below for content.

### 8.3 DAO Pattern Instructions

**File**: `.github/instructions/dao-pattern.instructions.md`

See Section 9 below for content.

### 8.4 Error Handling Instructions

**File**: `.github/instructions/error-handling.instructions.md`

See Section 9 below for content.

---

## Step 9: Verification Checklist

Before proceeding to Phase 2 (MVVM Features), verify:

- [ ] All 16 template files copied successfully
- [ ] Core models created (Model_Dao_Result, Model_Application_Variables, Model_ReceivingLine, etc.)
- [ ] Enums created (Enum_ErrorSeverity, Enum_LabelType)
- [ ] Database helpers configured (Helper_Database_StoredProcedure, Helper_Database_Variables)
- [ ] Services configured (Service_ErrorHandler, LoggingUtility)
- [ ] Contracts/interfaces created
- [ ] DAO pattern established (at least one DAO created)
- [ ] MySQL database created (`mtm_receiving_application`)
- [ ] Tables created (receiving_lines, dunnage_lines, routing_labels)
- [ ] At least one stored procedure created and tested
- [ ] GitHub instruction files created (4 minimum)
- [ ] Project builds without errors
- [ ] Can successfully connect to MySQL database
- [ ] Sample DAO method executes successfully

---

## Next Steps

**Once all verification items are checked**, proceed to:

**[SETUP_PHASE_2_MVVM_FEATURES.md](SETUP_PHASE_2_MVVM_FEATURES.md)** - Build MVVM application features

Do NOT proceed to Phase 2 until Phase 1 is 100% complete. The MVVM layer depends on this infrastructure.

---

**Phase 1 Complete!** You now have:
✅ Solid database foundation  
✅ Reusable services and helpers  
✅ DAO pattern established  
✅ Error handling configured  
✅ GitHub Copilot instructions ready  

Time to build features! 🚀

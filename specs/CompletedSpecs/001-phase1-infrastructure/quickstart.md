# Phase 1 Infrastructure Quickstart

**Feature**: Phase 1 Infrastructure Setup  
**Date**: December 15, 2025  
**Estimated Time**: 2-3 hours

## Prerequisites

Before starting Phase 1 infrastructure setup, ensure you have:

- ✅ .NET 8.0 SDK installed
- ✅ MySQL 5.7.24 running (via MAMP at localhost:3306)
- ✅ Visual Studio 2022 or VS Code with C# extension
- ✅ WinUI 3 project created (MTM_Receiving_Application.csproj)
- ✅ MTM_WIP_Application_WinForms accessible at C:\Users\johnk\source\repos\
- ✅ Git initialized in project directory

## Quick Setup (30 minutes)

### Step 1: Install NuGet Packages (2 minutes)

```powershell
# From project root
dotnet add package MySql.Data --version 9.0.0
dotnet add package CommunityToolkit.Mvvm --version 8.2.2
```

Verify packages in .csproj:
```xml
<PackageReference Include="MySql.Data" Version="9.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
```

### Step 2: Copy Template Files (5 minutes)

Run PowerShell script from **MTM_WIP_Application_WinForms** directory:

```powershell
$source = "C:\Users\johnk\source\repos\MTM_WIP_Application_WinForms"
$dest = "C:\Users\johnk\source\repos\MTM_Receiving_Application"

# Models
Copy-Item "$source\Models\Core\Model_Dao_Result.cs" "$dest\Models\Receiving\_TEMPLATE_Model_Dao_Result.txt"
Copy-Item "$source\Models\Core\Model_Application_Variables.cs" "$dest\Models\Receiving\_TEMPLATE_Model_Application_Variables.txt"

# Enums
Copy-Item "$source\Models\Enums\Enum_ErrorSeverity.cs" "$dest\Models\Enums\_TEMPLATE_Enum_ErrorSeverity.txt"

# Helpers
Copy-Item "$source\Helpers\Helper_Database_StoredProcedure.cs" "$dest\Helpers\Database\_TEMPLATE_Helper_Database_StoredProcedure.txt"
Copy-Item "$source\Helpers\Helper_Database_Variables.cs" "$dest\Helpers\Database\_TEMPLATE_Helper_Database_Variables.txt"

# Services
Copy-Item "$source\Services\ErrorHandling\Service_ErrorHandler.cs" "$dest\Services\Database\_TEMPLATE_Service_ErrorHandler.txt"
Copy-Item "$source\Services\Logging\LoggingUtility.cs" "$dest\Services\Database\_TEMPLATE_LoggingUtility.txt"

# DAOs
Copy-Item "$source\Data\Dao_User.cs" "$dest\Data\Receiving\_TEMPLATE_Dao_User.txt"

Write-Host "✓ Template files copied" -ForegroundColor Green
```

**Verify**: 8 _TEMPLATE_*.txt files exist

### Step 3: Create Database (3 minutes)

```powershell
# Create database
& "C:\MAMP\bin\mysql\bin\mysql.exe" -h localhost -P 3306 -u root -proot -e "CREATE DATABASE IF NOT EXISTS mtm_receiving_application CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

# Verify
& "C:\MAMP\bin\mysql\bin\mysql.exe" -h localhost -P 3306 -u root -proot -e "SHOW DATABASES LIKE 'mtm_receiving%';"
```

**Expected output**: `mtm_receiving_application`

### Step 4: Create Core Models (10 minutes)

Convert templates and update namespaces:

```powershell
# Model_Dao_Result
$content = Get-Content "Models\Receiving\_TEMPLATE_Model_Dao_Result.txt" -Raw
$content = $content -replace 'MTM_WIP_Application_Winforms','MTM_Receiving_Application'
$content = $content -replace 'using System.Windows.Forms;',''
Set-Content "Models\Receiving\Model_Dao_Result.cs" $content

# Enum_ErrorSeverity
$content = Get-Content "Models\Enums\_TEMPLATE_Enum_ErrorSeverity.txt" -Raw
$content = $content -replace 'MTM_WIP_Application_Winforms','MTM_Receiving_Application'
Set-Content "Models\Enums\Enum_ErrorSeverity.cs" $content
```

Create Model_ReceivingLine.cs manually (see data-model.md for full structure):

```csharp
namespace MTM_Receiving_Application.Models.Receiving;

public class Model_ReceivingLine
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public string PartID { get; set; } = string.Empty;
    public int PONumber { get; set; }
    public int EmployeeNumber { get; set; }
    public string Heat { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public string InitialLocation { get; set; } = string.Empty;
    public int? CoilsOnSkid { get; set; }
    public int LabelNumber { get; set; } = 1;
    public int TotalLabels { get; set; } = 1;
    public string VendorName { get; set; } = "Unknown";
    public string PartDescription { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public string LabelText => $"{LabelNumber} / {TotalLabels}";
}
```

### Step 5: Create Database Tables (5 minutes)

Create `Database/Schemas/01_create_receiving_tables.sql`:

```sql
CREATE TABLE IF NOT EXISTS label_table_receiving (
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

Execute:
```powershell
& "C:\MAMP\bin\mysql\bin\mysql.exe" -h localhost -P 3306 -u root -proot mtm_receiving_application < Database\Schemas\01_create_receiving_tables.sql
```

### Step 6: Create First Stored Procedure (5 minutes)

Create `Database/StoredProcedures/Receiving/receiving_line_Insert.sql`:

```sql
DELIMITER $$

DROP PROCEDURE IF EXISTS receiving_line_Insert $$

CREATE PROCEDURE receiving_line_Insert(
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
        SET p_Status = 1;
        ROLLBACK;
    END;

    START TRANSACTION;

    INSERT INTO label_table_receiving (
        quantity, part_id, po_number, employee_number, heat,
        transaction_date, initial_location, coils_on_skid
    ) VALUES (
        p_Quantity, p_PartID, p_PONumber, p_EmployeeNumber, p_Heat,
        p_Date, p_InitialLocation, p_CoilsOnSkid
    );

    SET p_Status = 0;
    SET p_ErrorMsg = '';
    COMMIT;
END $$

DELIMITER ;
```

Execute:
```powershell
& "C:\MAMP\bin\mysql\bin\mysql.exe" -h localhost -P 3306 -u root -proot mtm_receiving_application < Database\StoredProcedures\Receiving\receiving_line_Insert.sql
```

## Verification Checklist

After quick setup, verify:

- [ ] Project builds without errors (`dotnet build`)
- [ ] MySql.Data package installed and referenced
- [ ] Database `mtm_receiving_application` exists
- [ ] Table `label_table_receiving` exists with proper structure
- [ ] Stored procedure `receiving_line_Insert` exists
- [ ] Model_Dao_Result.cs exists in Models/Receiving/
- [ ] Model_ReceivingLine.cs exists in Models/Receiving/
- [ ] Enum_ErrorSeverity.cs exists in Models/Enums/
- [ ] No compilation errors in converted template files

## Next Steps: Full Implementation (1.5-2 hours)

### 1. Complete Helper Layer (30 minutes)

- Convert Helper_Database_Variables.txt → .cs
- Convert Helper_Database_StoredProcedure.txt → .cs
- Update connection strings for receiving application
- Remove WinForms dependencies

### 2. Complete Service Layer (30 minutes)

- Convert Service_ErrorHandler.txt → .cs
- Replace MessageBox with ContentDialog
- Convert LoggingUtility.txt → .cs
- Update log paths to MTM_Receiving_Application

### 3. Complete DAO Layer (30 minutes)

- Create Dao_ReceivingLine.cs from template
- Implement InsertReceivingLineAsync()
- Test with sample data
- Create Dao_DunnageLine.cs stub
- Create Dao_RoutingLabel.cs stub

### 4. Create GitHub Instructions (15 minutes)

Create in `.github/instructions/`:
- database-layer.instructions.md
- service-layer.instructions.md
- error-handling.instructions.md
- dao-pattern.instructions.md

## Testing the Infrastructure

### Unit Test Example (xUnit)

Create `Tests/Data/Dao_ReceivingLine_Tests.cs`:

```csharp
using Xunit;
using MTM_Receiving_Application.Data.Receiving;
using MTM_Receiving_Application.Models.Receiving;

public class Dao_ReceivingLine_Tests
{
    [Fact]
    public async Task InsertReceivingLine_ValidData_ReturnsSuccess()
    {
        // Arrange
        var line = new Model_ReceivingLine
        {
            Quantity = 100,
            PartID = "TEST-PART-001",
            PONumber = 12345,
            EmployeeNumber = 1001,
            Heat = "HEAT-ABC",
            Date = DateTime.Today,
            InitialLocation = "A-1-1",
            VendorName = "Test Vendor",
            PartDescription = "Test Part"
        };

        // Act
        var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.AffectedRows);
        Assert.Empty(result.ErrorMessage);
    }
}
```

Run tests:
```powershell
dotnet test
```

## Troubleshooting

### Build Errors

**Problem**: Namespace not found  
**Solution**: Check `using` statements and namespace declarations match folder structure

**Problem**: MySql.Data not found  
**Solution**: `dotnet restore` then `dotnet build`

### Database Errors

**Problem**: Can't connect to MySQL  
**Solution**: Verify MAMP is running, check port 3306

**Problem**: Stored procedure not found  
**Solution**: Re-run SQL script, check procedure name spelling

**Problem**: Access denied  
**Solution**: Verify root password in connection string

### Template Conversion Errors

**Problem**: WinForms references remain  
**Solution**: Search for "System.Windows.Forms" and replace with WinUI 3 equivalents

**Problem**: Form class not found  
**Solution**: Replace `Form` with `Window` or `Page`

## Success Criteria Met

After completing this quickstart, you should have:

✅ Database connectivity working  
✅ Model_Dao_Result pattern established  
✅ At least one model created  
✅ At least one table created  
✅ At least one stored procedure created  
✅ At least one DAO method working  
✅ Project building without errors  
✅ Foundation ready for Phase 2 MVVM

**Estimated Total Time**: 2-3 hours for experienced developer

---

**Need Help?** Refer to:
- [data-model.md](data-model.md) for complete entity definitions
- [research.md](research.md) for technology decisions
- [plan.md](plan.md) for full project structure
- SETUP_PHASE_1_INFRASTRUCTURE.md for detailed step-by-step guide

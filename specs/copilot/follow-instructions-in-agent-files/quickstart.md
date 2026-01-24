# Quick Start Guide: Receiving Workflow Consolidation

**Feature**: Receiving Workflow Consolidation  
**Date**: 2026-01-24  
**Audience**: Developers, Testers, Product Owners

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Setup Instructions](#setup-instructions)
3. [Running the Application](#running-the-application)
4. [Testing Scenarios](#testing-scenarios)
5. [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Software

- **Operating System**: Windows 10/11 (version 1809 or later)
- **.NET SDK**: .NET 8.0 or later
- **IDE**: Visual Studio 2022 (17.8+) or JetBrains Rider
- **Database**: MySQL 8.0 or later
- **Optional**: SQL Server (for Infor Visual read-only data)

### Database Setup

1. **Install MySQL** (if not already installed):
   ```bash
   # Download from: https://dev.mysql.com/downloads/mysql/
   # Follow installation wizard
   ```

2. **Create Database**:
   ```sql
   CREATE DATABASE mtm_receiving_application CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```

3. **Run Migrations**:
   ```bash
   cd Database/Migrations
   mysql -u root -p mtm_receiving_application < 001-create-receiving-workflow-tables.sql
   ```

4. **Verify Tables**:
   ```sql
   USE mtm_receiving_application;
   SHOW TABLES;
   -- Should show: receiving_workflow_sessions, receiving_load_details, receiving_completed_transactions
   ```

### Configuration

1. **Update Connection Strings** (appsettings.json):
   ```json
   {
     "ConnectionStrings": {
       "MTM_Receiving": "Server=localhost;Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=yourpassword;",
       "InforVisual": "Server=your-server;Database=VISUAL;ApplicationIntent=ReadOnly;Trusted_Connection=True;"
     }
   }
   ```

2. **Update CSV Output Path** (appsettings.json):
   ```json
   {
     "ReceivingWorkflow": {
       "CsvOutputDirectory": "C:\\MTM_Receiving\\Output",
       "MaxLoadsPerTransaction": 100
     }
   }
   ```

## Setup Instructions

### Clone and Build

```bash
# Clone repository
git clone https://github.com/JDKoll1982/MTM_Receiving_Application.git
cd MTM_Receiving_Application

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test
```

### Visual Studio Setup

1. Open `MTM_Receiving_Application.slnx`
2. Set startup project to `MTM_Receiving_Application`
3. Build solution (Ctrl+Shift+B)
4. Run (F5)

## Running the Application

### Launch Application

```bash
# From command line
dotnet run --project MTM_Receiving_Application.csproj

# Or use Visual Studio
F5 (Debug) or Ctrl+F5 (Run without debugging)
```

### Navigate to Receiving Module

1. **From Main Window**: Click "Receiving" in the navigation menu
2. **Select Mode**: 
   - **Guided Mode** (recommended): 3-step wizard with bulk operations
   - **Manual Mode**: Grid-based entry with auto-fill

## Testing Scenarios

### Scenario 1: Basic 3-Step Workflow (User Story 1)

**Objective**: Complete a standard receiving transaction with 5 loads

**Steps**:

1. **Launch Application** and select **Receiving > Guided Mode**

2. **Step 1: Order & Part Selection**
   - Enter PO Number: `PO-2024-001`
   - Select Part: `PART-12345 - Steering Column Assembly`
   - Enter Load Count: `5`
   - Click **Next**
   
   **Expected Result**: Advances to Step 2 with 5 empty load rows

3. **Step 2: Load Details Entry**
   - **Load 1**:
     - Weight/Quantity: `250.5`
     - Heat Lot: `HL-2024-001`
     - Package Type: `Pallet`
     - Packages Per Load: `2`
   - Click to next field (validation passes)
   
   **Expected Result**: No errors, can proceed to other loads

4. **Navigate to Step 3** (click Next)
   
   **Expected Result**: Review screen shows all 1 load with data, 4 loads empty

5. **Click Save**
   
   **Expected Result**:
   - Success message displayed
   - CSV file created in output directory
   - Database records created
   - Transaction complete

**Validation**:
- Check CSV file exists: `C:\MTM_Receiving\Output\PO-2024-001-[timestamp].csv`
- Verify database records:
  ```sql
  SELECT * FROM receiving_completed_transactions WHERE po_number = 'PO-2024-001';
  ```

---

### Scenario 2: Bulk Copy to All Loads (User Story 2.1)

**Objective**: Use bulk copy to fill all loads with identical data

**Steps**:

1. **Complete Step 1** as in Scenario 1, but with **10 loads**

2. **Step 2: Enter Load 1 Data**:
   - Weight/Quantity: `300.0`
   - Heat Lot: `HL-2024-002`
   - Package Type: `Box`
   - Packages Per Load: `5`

3. **Use Bulk Copy**:
   - Click **Copy to All Loads** dropdown
   - Select **Copy All Fields to All Loads**
   
   **Expected Result**:
   - All 10 loads now have the same data
   - Cells 2-10 have subtle highlight (fades after 10 seconds)
   - Auto-fill indicator visible on hover
   - Notification: "Data copied to 10 loads (empty cells only)"

4. **Verify Copy Result**:
   - Scroll through loads 2-10
   - All should have identical data to Load 1
   - Auto-fill icon visible on each copied cell

5. **Modify Load 5** (manually):
   - Change Weight: `350.0`
   - Auto-fill icon should disappear for that cell

6. **Save Workflow**
   
   **Expected Result**: All 10 loads saved, 9 with auto-filled data, 1 with partial manual override

---

### Scenario 3: Copy with Preservation (User Story 2.1, Acceptance 1-2)

**Objective**: Verify bulk copy only fills empty cells, preserves existing data

**Steps**:

1. **Setup**:
   - Complete Step 1 with **5 loads**
   - In Step 2:
     - **Load 1**: Fill all fields (Weight: 250, Heat: HL-001, Package: Pallet, Packages: 2)
     - **Load 2**: Fill only Heat Lot: `HL-002`
     - **Load 3**: Fill only Package Type: `Box`
     - **Loads 4-5**: Leave empty

2. **Execute Bulk Copy**:
   - Click **Copy to All Loads** > **Copy All Fields to All Loads**
   
   **Expected Result**:
   - **Load 2**: Weight, Package Type, and Packages copied from Load 1; Heat Lot preserved as `HL-002`
   - **Load 3**: Weight, Heat Lot, Packages copied from Load 1; Package Type preserved as `Box`
   - **Loads 4-5**: All fields copied from Load 1
   - Notification: "Data copied to 5 loads (empty cells only). Occupied cells in 2 loads preserved (Load 2, Load 3)"

3. **Verify Preservation**:
   - **Load 2**: Heat Lot should still be `HL-002` (NOT `HL-001`)
   - **Load 3**: Package Type should still be `Box` (NOT `Pallet`)
   - Preserved cells should have NO auto-fill indicator

---

### Scenario 4: Validation Error Blocks Copy (User Story 2.2, Acceptance 8)

**Objective**: Verify copy is disabled when source load has validation errors

**Steps**:

1. **Setup**:
   - Complete Step 1 with **3 loads**
   - In Step 2, **Load 1**:
     - Weight/Quantity: `-50` (invalid - negative)
     - Heat Lot: `HL-003`

2. **Attempt Bulk Copy**:
   - Hover over **Copy to All Loads** dropdown button
   
   **Expected Result**:
   - Button is **disabled** (grayed out)
   - Tooltip shows: "Cannot copy: Load 1 has validation errors. Fix errors before copying"

3. **Fix Validation Error**:
   - Change Weight to `250.0` (valid)
   
   **Expected Result**:
   - Button becomes **enabled**
   - Tooltip disappears

4. **Execute Copy**:
   - Click **Copy to All Loads** > **Copy All Fields to All Loads**
   
   **Expected Result**: Copy succeeds, data copied to Loads 2-3

---

### Scenario 5: Change Copy Source (User Story 2.2)

**Objective**: Use a different load as the copy source

**Steps**:

1. **Setup**:
   - Complete Step 1 with **5 loads**
   - In Step 2:
     - **Load 1**: Weight: 200, Heat: HL-001, Package: Pallet, Packages: 1
     - **Load 3**: Weight: 350, Heat: HL-003, Package: Box, Packages: 3

2. **Change Copy Source**:
   - Click **Copy Source** dropdown
   - Select **Load 3**
   
   **Expected Result**:
   - Load 3 row has highlight indicating it's the active source
   - Notification: "Load 3 is now copy source"

3. **Execute Copy**:
   - Click **Copy to All Loads** > **Copy All Fields to All Loads**
   
   **Expected Result**:
   - Loads 1, 2, 4, 5 receive data from **Load 3** (NOT Load 1)
   - Load 1 now has Weight: 350, Heat: HL-003, etc. (if cells were empty)

---

### Scenario 6: Clear Auto-Filled Data (User Story 2.3)

**Objective**: Clear only auto-filled cells, preserve manually entered data

**Steps**:

1. **Setup**:
   - Complete Scenario 2 (bulk copy to all loads)
   - Manually edit **Load 5**: Change Weight to `400.0`

2. **Clear Auto-Filled Data**:
   - Click **Clear Auto-Filled Data** dropdown
   - Select **Clear All Auto-Filled Fields**
   - Confirm dialog: "This will clear auto-filled data in X cells across Y loads. Manually entered data will be preserved. Continue?" → Click **Yes**
   
   **Expected Result**:
   - **Loads 2-4, 6-10**: All fields cleared (were auto-filled)
   - **Load 5**: Weight remains `400.0` (manually entered); other fields cleared (were auto-filled)
   - **Load 1**: No fields cleared (all manually entered)

---

### Scenario 7: Edit from Review Screen (User Story 4.1)

**Objective**: Return to Step 2 from Review to correct errors

**Steps**:

1. **Complete Basic Workflow** to Step 3

2. **Click Edit Details Button**
   
   **Expected Result**:
   - Navigates directly to **Step 2** (skips Step 1)
   - "Edit Mode" indicator visible at top
   - "Return to Review" button visible (replaces "Next")

3. **Make Corrections**:
   - Change **Load 1** Weight to `275.0`

4. **Click Return to Review**
   
   **Expected Result**:
   - Navigates directly to **Step 3** (skips Step 1)
   - Corrected field highlighted for 3-5 seconds
   - Can now save

---

### Scenario 8: Non-PO Receiving (User Story 5.1)

**Objective**: Receive items without a purchase order

**Steps**:

1. **From Receiving Menu**: Select **Guided Mode > Non-PO Receiving**

2. **Step 1 (Modified)**:
   - **No PO Number field** (hidden)
   - Part Selection: `MISC-ITEM-001 - Miscellaneous Parts`
   - Vendor Name: `Acme Supplies`
   - Load Count: `2`
   - Click **Next**

3. **Complete Steps 2-3** as normal

4. **Save**
   
   **Expected Result**:
   - CSV saved with `PONumber` column empty
   - Database record shows NULL for po_number

---

### Scenario 9: Real-Time Validation (User Story 6.1)

**Objective**: See validation errors immediately as data is entered

**Steps**:

1. **Setup**: Complete Step 1 with **3 loads**

2. **Step 2 - Enter Invalid Data**:
   - **Load 1** Weight: `-100` (negative)
   - Tab to next field
   
   **Expected Result**:
   - Red border appears immediately around Weight field
   - Error icon appears next to field
   - Error message: "Weight must be positive"
   - Summary error count updates: "1 error"

3. **Correct Error**:
   - Change Weight to `250.0`
   - Tab to next field
   
   **Expected Result**:
   - Red border disappears
   - Error icon disappears
   - Error message clears
   - Summary error count: "0 errors"

---

## Performance Testing

### Large Load Count (100 Loads)

**Objective**: Verify performance with maximum load count

**Steps**:
1. Step 1: Enter Load Count: `100`
2. Step 2: Enter data for Load 1
3. Execute **Copy All Fields to All Loads**
4. Measure time from click to completion

**Expected Performance**:
- Copy operation completes in **< 1 second**
- No UI freezing or lag
- Progress indicator visible for >50 loads

---

## Troubleshooting

### Issue: Database Connection Failed

**Symptoms**: Error on startup: "Unable to connect to MySQL database"

**Solutions**:
1. Verify MySQL service is running:
   ```bash
   # Windows
   services.msc → Find "MySQL80" → Ensure Status = "Running"
   
   # macOS/Linux
   sudo systemctl status mysql
   ```

2. Check connection string in appsettings.json
3. Test connection:
   ```bash
   mysql -u root -p mtm_receiving_application
   ```

---

### Issue: Copy Operation Not Working

**Symptoms**: Click "Copy to All Loads" but nothing happens

**Solutions**:
1. Check for validation errors on source load (red borders)
2. Verify target loads exist (load count > 1)
3. Check browser console for JavaScript errors (if web-based)
4. Restart application

---

### Issue: CSV File Not Created

**Symptoms**: Save succeeds but CSV file missing

**Solutions**:
1. Verify output directory exists:
   ```powershell
   Test-Path "C:\MTM_Receiving\Output"
   ```
2. Check write permissions on directory
3. Review application logs for file I/O errors:
   ```bash
   cat logs/mtm-receiving-[date].log | grep "CSV"
   ```

---

### Issue: Validation Always Fails

**Symptoms**: Cannot proceed past Step 1 or Step 2 due to validation

**Solutions**:
1. Read validation error messages carefully
2. Check data format requirements (e.g., PO Number alphanumeric only)
3. Verify database lookup data exists (Parts, Package Types)
4. Check application logs for detailed validation errors

---

## Additional Resources

- **Full Feature Specification**: See `spec.md` in this directory
- **Data Model Details**: See `data-model.md` in this directory
- **API Contracts**: See `contracts/mediatr-contracts.md` in this directory
- **Technical Architecture**: See `plan.md` in this directory

---

## Quick Reference: Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Tab` | Navigate to next field |
| `Shift+Tab` | Navigate to previous field |
| `Ctrl+S` | Save workflow |
| `Ctrl+N` | Start new workflow |
| `Esc` | Cancel current operation |
| `F5` | Refresh validation |

---

## Quick Reference: Workflow States

| State | Description | Can Navigate? |
|-------|-------------|---------------|
| Step 1 (Entry) | Entering PO/Part/Load Count | Yes (to Step 2 if valid) |
| Step 2 (Entry) | Entering load details | Yes (to Step 1 or Step 3) |
| Step 2 (Edit Mode) | Editing from Review | Yes (to Step 3 only) |
| Step 3 (Review) | Reviewing before save | Yes (to Step 2) |
| Saved | Transaction complete | No (read-only) |

---

**Last Updated**: 2026-01-24  
**Version**: 1.0  
**Authors**: Development Team

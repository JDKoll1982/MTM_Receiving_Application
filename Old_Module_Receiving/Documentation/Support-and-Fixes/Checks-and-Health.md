# Health Checks - Module_Receiving

**Last Updated: 2025-01-15**

## Quick Health Checklist

Run these checks to verify Module_Receiving is functioning normally. All checks should complete successfully under normal conditions.

---

## Check 1: Basic Module Access

**Test**: Can you open the Receiving module?

**Steps**:
1. Launch MTM Receiving Application
2. Click on "Receiving" from main menu
3. Mode Selection screen should appear within 5 seconds

**Expected result**:
- Screen loads without errors
- Three mode buttons are visible and clickable
- No red error messages

**If it fails**:
- Restart application and try again
- Check Windows Event Viewer for application errors
- Contact IT if repeated failures

---

## Check 2: ERP Connectivity

**Test**: Can the system validate PO numbers against ERP?

**Steps**:
1. Go to Guided Wizard mode
2. Enter a known valid PO number (ask supervisor for test PO)
3. Click Next

**Expected result**:
- System shows "Validating..." briefly
- Part information appears (description, UOM, expected qty)
- No "connection failed" or "timeout" errors

**If it fails**:
- Check network connectivity
- Try a different PO number
- Contact IT if all POs fail validation

**Note**: This validates SQL Server (Infor Visual) read-only connection

---

## Check 3: Session Save

**Test**: Does session state persist correctly?

**Steps**:
1. Start a wizard receive
2. Enter a PO and proceed to Load Entry
3. Enter one load's details
4. Close the application WITHOUT saving
5. Restart application
6. Return to Receiving module

**Expected result**:
- System offers "Restore Session" option
- Clicking Restore recovers PO and load data
- All previously entered data is intact

**If it fails**:
- Check `%APPDATA%\MTM_Receiving_Application\session.json` exists
- Verify file has recent timestamp and non-zero size
- Contact IT if session never saves

---

## Check 4: Local CSV Creation

**Test**: Can system write to local CSV path?

**Steps**:
1. Complete a test receive (or use current session)
2. Navigate to Review step
3. Click Save
4. Note the local CSV path from confirmation message
5. Navigate to that folder in Windows Explorer

**Expected result**:
- CSV file exists with current date/time
- File is not empty (open in Notepad to verify)
- File contains data matching your test receive

**If it fails**:
- Check disk space on C: drive
- Verify folder exists and you have write permissions
- Check Settings module for configured local path
- Contact IT if path is inaccessible

**Default path**: Usually in your Documents or AppData folder

---

## Check 5: Network CSV Creation (If Configured)

**Test**: Can system write to network share?

**Steps**:
1. Complete a test receive
2. Click Save
3. Note network CSV path from message
4. Navigate to network path (e.g., `\\server\share\receiving`)

**Expected result**:
- CSV file exists on network share
- Timestamp matches save time
- File content matches local CSV

**If it fails**:
- **If message says "Network CSV failed" but save succeeded**: This is acceptable (local is critical, network is optional)
- Verify you can access the network path at all
- Check with IT if share permissions changed
- Label printing may require manual intervention

**Note**: Network failures don't block receiving, but labels may not auto-print

---

## Check 6: Database Save

**Test**: Does data save to MySQL?

**Steps**:
1. Complete and save a test receive
2. Note the PO number and timestamp
3. Go to Edit Mode → Load from Database
4. Search for your test receive (use PO number and today's date)

**Expected result**:
- Test receive appears in search results
- All data matches what you entered
- Loads show correct quantities, weights, package types

**If it fails**:
- Check if save showed "Database error" message
- Verify other users can save to database
- Check if database connection settings changed
- Contact IT immediately if no one can save

**Note**: This validates MySQL connectivity and stored procedure execution

---

## Check 7: Validation Rules

**Test**: Do validation rules catch errors correctly?

**Steps**:
1. Start a wizard receive
2. Try to enter invalid data:
   - Blank quantity field
   - Negative weight
   - Zero for number of loads
3. Attempt to proceed to next step

**Expected result**:
- Red error messages appear for invalid fields
- Cannot proceed until errors are fixed
- Error messages are clear and helpful

**If it fails**:
- System allows obviously invalid data (e.g., blank quantity)
- Proceeds even with errors showing
- Contact IT—validation is broken

**Purpose**: Prevents bad data from reaching database

---

## Check 8: Edit Mode Functions

**Test**: Can you load and modify existing receives?

**Steps**:
1. Go to Edit Mode
2. Load from Database (or CSV if recent file available)
3. Search for a completed receive from yesterday or earlier
4. Load it and modify a quantity
5. Save changes

**Expected result**:
- Search returns expected results
- Data loads correctly into edit grid
- Changes save without errors
- Updated CSV is generated

**If it fails**:
- Search returns no results (check filters)
- Data loads but shows wrong information
- Save fails with database error
- Contact IT if unable to edit any receives

---

## Check 9: Manual Entry Mode

**Test**: Does bulk grid entry work?

**Steps**:
1. Go to Manual Entry mode
2. Enter a row with PO, Part, Quantity, Package Type
3. Click "Add Row" to create a second row
4. Fill the second row
5. Click "Save All"

**Expected result**:
- Grid accepts data without lag
- New rows add successfully
- Validation highlights errors (if any)
- All rows save as separate loads

**If it fails**:
- Grid is unresponsive or slow
- "Add Row" doesn't work
- Save doesn't validate before saving
- Contact IT if grid functionality is broken

---

## Check 10: Help Content Loads

**Test**: Is help documentation accessible?

**Steps**:
1. On any Receiving screen, click the Help icon (usually ? button)
2. Help dialog should appear

**Expected result**:
- Help window opens
- Content is relevant to current screen
- Text is readable and formatted

**If it fails**:
- Help button does nothing
- Help content is blank or shows error
- Wrong help content displays
- Report to IT (non-critical but should work)

---

## Performance Baselines

**Normal behavior**:
- PO validation: 1-3 seconds
- Save operation: 5-15 seconds
- Session restore: Instant (under 1 second)
- Database search: 2-10 seconds depending on date range
- Loading CSV for edit: 1-2 seconds

**Concerning slowness**:
- PO validation takes more than 10 seconds
- Save operation exceeds 30 seconds
- Database search times out
- App freezes during operations

**If consistently slow**:
- Note which operations are slow
- Test at different times of day
- Check with other users (local issue vs. system-wide)
- Report to IT with specifics

---

## Automated Health Dashboard (If Available)

Some installations include a built-in health dashboard:

**Where to find it**:
- Settings module → Developer Tools → Database Test
- Or direct database monitoring tools

**What it shows**:
- Database connection status
- CSV path accessibility
- Last successful receive timestamp
- Error count in last 24 hours

**Green indicators**: Everything nominal
**Yellow warnings**: Non-critical issues (e.g., network CSV path unreachable)
**Red errors**: Critical failures requiring immediate attention

---

## Monthly Health Review

**For IT or system administrators**:

Run complete health check suite monthly and log results:

| Check | Status | Last Run | Notes |
|-------|--------|----------|-------|
| Module Access | Pass/Fail | YYYY-MM-DD | |
| ERP Connection | Pass/Fail | YYYY-MM-DD | |
| Session Save | Pass/Fail | YYYY-MM-DD | |
| Local CSV | Pass/Fail | YYYY-MM-DD | |
| Network CSV | Pass/Fail | YYYY-MM-DD | |
| Database Save | Pass/Fail | YYYY-MM-DD | |
| Validation Rules | Pass/Fail | YYYY-MM-DD | |
| Edit Mode | Pass/Fail | YYYY-MM-DD | |
| Manual Entry | Pass/Fail | YYYY-MM-DD | |
| Help Content | Pass/Fail | YYYY-MM-DD | |

**Action on failures**: Document root cause and remediation

**Trend monitoring**: Watch for degrading performance over time

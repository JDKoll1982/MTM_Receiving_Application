# Daily Tasks - Module_Receiving

**Last Updated: 2025-01-15**

## Standard Receiving Workflow (Wizard Mode)

**When to use**: Most receives, especially when you have:
- Multiple loads
- Heat/lot numbers to track
- Different package types

**Steps**:

1. **Start a New Receive**
   - Open the Receiving module from the main menu
   - System shows Mode Selection screen
   - Click "Guided Wizard" button

2. **Enter Purchase Order**
   - Type the PO number in the text field
   - Press Enter or click Next
   - Wait for system to validate against ERP
   - Confirm the part description shown is correct

3. **Select Parts** (if PO has multiple parts)
   - Check the boxes for parts you're receiving now
   - Leave unchecked any parts not in this shipment
   - Click Next

4. **Enter Number of Loads**
   - Count your pallets, boxes, or containers
   - Enter that number in "Number of Loads"
   - Click Next to create load entries

5. **Fill in Load Details**
   - For each load, enter:
     - **Quantity**: Number of pieces
     - **Weight**: Total weight (if using scales)
     - **Heat/Lot**: Batch tracking number (if on packing slip)
     - **Package Type**: Select from dropdown (Pallet, Box, Crate, etc.)
   - Use Tab key to move quickly between fields
   - Click Next when all loads are complete

6. **Review Your Entries**
   - Check the summary screen carefully
   - Look for:
     - Correct PO number
     - Right part numbers
     - Accurate quantities and weights
     - Proper package types
   - Use Back button if you need to fix anything

7. **Save and Generate Labels**
   - Click "Save" button
   - Wait for confirmation message
   - Look for "Save successful" message
   - CSV files are created automatically
   - Labels should print automatically (if configured)

8. **Verify Labels Printed**
   - Check label printer or queue
   - Verify one label per load
   - Apply labels to corresponding loads

## Quick Bulk Entry (Manual Mode)

**When to use**: Simple receives where:
- All loads are identical
- No heat/lot tracking needed
- Experienced users only

**Steps**:

1. **Start Manual Entry**
   - Open Receiving module
   - Click "Manual Entry" on Mode Selection

2. **Use the Grid**
   - Each row = one load
   - Fill in columns:
     - PO Number
     - Part ID
     - Quantity
     - Weight (optional)
     - Package Type
   - Copy/paste works from Excel

3. **Add More Rows**
   - Click "Add Row" button at bottom
   - Or press Ctrl+Enter on last row
   - System creates a new empty row

4. **Delete Mistakes**
   - Click the row to select it
   - Click "Delete Row" button
   - Or press Delete key

5. **Save All Lines**
   - Click "Save All" button
   - System validates all rows
   - Errors show in red
   - Fix errors and save again

## Editing a Previous Receive

**When to use**: Need to correct a mistake or add missing information

**Steps**:

1. **Open Edit Mode**
   - From Mode Selection, click "Edit Mode"
   - Choose where to load from:
     - Current Session (today's unsaved work)
     - CSV File (recent receives)
     - Database History (any past receive)

2. **Find the Receive**
   - If loading from database:
     - Enter PO number, date range, or user
     - Click Search
     - Select the receive from the list
   - If loading from CSV:
     - Browse to the CSV file
     - Click Open

3. **Make Changes**
   - Edit any field that needs correction
   - Add or delete loads if needed
   - Change quantities, weights, package types, etc.

4. **Save Updates**
   - Click "Save Changes"
   - Original record is updated
   - New CSV is generated (overwrites old one)

## Resetting CSV Files

**When to use**: CSV got corrupted or label printer jammed

**Steps**:

1. **Click Reset CSV Button**
   - Found on main Receiving screen (top toolbar)
   - Confirms "Are you sure?" before proceeding

2. **System Behavior**
   - Deletes existing CSV files (local and network)
   - Recreates CSV from current session data
   - Shows confirmation when complete

3. **Reprint Labels**
   - Labels should re-queue automatically
   - Or manually send CSV to label system

## End of Day Checklist

Before closing:
- [ ] All physical receives entered into system
- [ ] Labels applied to all loads
- [ ] No error messages showing
- [ ] Session cleared (or saved for tomorrow if ongoing)
- [ ] Any CSV path errors reported to IT
- [ ] Quality hold items flagged in system

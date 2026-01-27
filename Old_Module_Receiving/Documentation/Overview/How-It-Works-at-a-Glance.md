# How Module_Receiving Works at a Glance

**Last Updated: 2025-01-15**

## The Big Picture

Module_Receiving takes information about incoming shipments and creates records that:
1. Track what arrived and when
2. Generate labels for warehouse tracking
3. Provide traceability for quality and compliance
4. Feed data to routing and reporting systems

## Main Workflow Steps

### Step 1: Choose Your Path
When you open receiving, you pick how to work:
- **Wizard Mode**: Step-by-step guidance (best for complex receives)
- **Manual Entry**: Quick grid entry (best for bulk simple receives)
- **Edit Mode**: Fix or update existing records

### Step 2: Enter Basic Information
- Type in the **Purchase Order (PO) number**
- System looks it up in the ERP system (Infor Visual)
- Validates the PO exists and shows part details

### Step 3: Specify What You're Receiving
- If the PO has multiple parts, pick which ones you're receiving now
- System shows description, unit of measure, and expected quantity
- You can receive everything or just some parts

### Step 4: Break It Into Loads
- Tell the system how many separate loads/containers arrived
- Each load becomes a separate label
- Example: 1 PO might arrive on 3 pallets = 3 loads

### Step 5: Add Details for Each Load
- **Weight**: How much each load weighs (if applicable)
- **Quantity**: Number of pieces in each load
- **Heat/Lot**: Batch tracking information (if applicable)
- **Package Type**: Pallet, box, crate, etc.

### Step 6: Review and Save
- See a summary of everything you entered
- Make final corrections if needed
- **Save** button does three things:
  1. Saves to session file (for recovery)
  2. Creates CSV files (for label printers)
  3. Saves to MySQL database (permanent record)

### Step 7: Print Labels
- CSV files are created in configured locations:
  - Local path (your computer)
  - Network path (shared drive for label systems)
- Label printing system picks up the CSV automatically
- You can also manually print from the CSV if needed

## What Happens Behind the Scenes

### Data Validation
Every entry is checked:
- PO number must exist in ERP
- Part numbers must match the PO
- Quantities must be positive numbers
- Required fields must be filled

### Session Management
- System saves your work automatically every time you move between steps
- If the app crashes, you can recover your session
- Session files are stored in your user folder

### CSV File Creation
Two CSV files are generated:
1. **Local CSV**: Saved to your computer (always works)
2. **Network CSV**: Saved to shared network location (may fail if network is down)
- Local CSV is critical—save operation fails if this doesn't work
- Network CSV is optional—save succeeds even if network is unavailable

### Database Saving
- Data is saved to MySQL using stored procedures
- Each load becomes a separate database record
- Records link to the PO, part, and user who entered them
- Timestamps track when each receive happened

## Error Handling

If something goes wrong:
- **PO Not Found**: Check the PO number with your supervisor
- **CSV Path Error**: Contact IT to verify network paths
- **Database Error**: System logs the error; data remains in session for retry
- **Validation Error**: Red message shows which field needs correction

## Recovery Options

### Session Recovery
If app crashes mid-receive:
1. Restart the application
2. Your session data is still saved
3. Continue from where you left off

### Edit Existing Receives
If you need to fix a mistake:
1. Use **Edit Mode**
2. Load from CSV or database
3. Make corrections
4. Save updates

# MTM Receiving Application - Mockup

This is a functional HTML/CSS/JavaScript mockup of the Multi-Step Receiving Label Entry Workflow feature as specified in `spec.md`.

## Files

- **index.html** - Main application structure with all workflow steps
- **styles.css** - Complete styling for the application
- **app.js** - Application logic and state management
- **mockData.json** - Mock database with PO data, parts, and receiving history
- **README.md** - This file

## How to Run

1. Open `index.html` in a modern web browser (Chrome, Firefox, Edge, or Safari)
2. The application will load mock data from `mockData.json`
3. You'll see a CSV Reset dialog on startup - choose either option to proceed

## Testing the Workflow

### Test Scenario 1: Complete Receiving Entry with PO

1. **CSV Reset Dialog**: Click "No, Continue" to proceed with existing data
2. **Step 1 - PO Entry**: Enter PO number `123456` and click "Load PO"
3. **Select Part**: Click "Select" on the first part (MMC-12345)
4. **Step 2 - Load Number**: Enter `3` for number of loads and click "Continue"
5. **Step 3 - Weight/Quantity**: Enter weights for each load (e.g., 1250, 1300, 1450) and click "Continue to Heat/Lot#"
6. **Step 4 - Heat/Lot#**: Enter heat number for Load 1 (e.g., "H12345A"), then check the quick-select checkbox to apply it to other loads
7. **Step 5 - Package Type**: Review default package types (Coils for MMC parts), enter packages per load (e.g., 25, 26, 29)
8. **Step 6 - Review**: Review the data grid, try editing fields to see cascading updates
9. **Save**: Click "Save to CSV & Database" to see progress indicators
10. **Complete**: View save summary and start a new entry

### Test Scenario 2: Non-PO Item (Customer Supplied)

1. **Step 1**: Click "Non-PO Item" button
2. Enter Part ID: `CUST-00001` and click "Look Up Part"
3. Review part details and click "Continue"
4. Follow steps 2-6 as above (note: no PO validation occurs)

### Test Scenario 3: Multiple Parts in One Session

1. Complete steps 1-6 for first part
2. At Step 6 (Review), click "Add Another Part/PO" instead of "Save"
3. Enter a different PO (e.g., `789012`) and select a part
4. Complete workflow for second part
5. At Review, you'll see all loads from both parts
6. Click "Save to CSV & Database" to save everything together

## Mock Database

The `mockData.json` file contains:

### Available PO Numbers
- `123456` - 3 parts (MMC-12345, MMF-67890, ABC-11111)
- `789012` - 1 part (MMC-54321)
- `345678` - 2 parts (XYZ-99999, MMF-22222)

### Available Parts for Non-PO Lookup
- `MMC-12345` - Steel Coil (defaults to "Coils")
- `MMF-67890` - Steel Sheet (defaults to "Sheets")
- `ABC-11111` - Component (no default, user must select)
- `CUST-00001` - Customer Supplied Material
- And more... (see mockData.json)

## Features Implemented

### All 9 Workflow Steps
1. ✅ CSV Reset Dialog (startup)
2. ✅ PO Entry with validation
3. ✅ Part/PO-Line# selection
4. ✅ Load Number/Skid Amount entry
5. ✅ Weight/Quantity input with validation
6. ✅ Heat/Lot# entry with quick-select
7. ✅ Package Type per Load with smart defaults
8. ✅ Review Grid with inline editing
9. ✅ Save progress and completion

### Key Features
- ✅ PO lookup from mock database
- ✅ Non-PO item support with part validation
- ✅ Smart package type defaults (MMC→Coils, MMF→Sheets)
- ✅ Quick-select heat/lot numbers
- ✅ Editable review grid with cascading updates
- ✅ Part number edit updates all matching loads
- ✅ PO number edit updates all loads of that part
- ✅ Weight per package calculation
- ✅ Multiple parts in one session
- ✅ Progress indicators during save
- ✅ Status messages and validation
- ✅ Modal dialogs for confirmations

### Validations
- ✅ PO number must be 6 digits
- ✅ Number of loads: 1-99
- ✅ All weights/quantities must be > 0
- ✅ Warning if total exceeds PO ordered quantity
- ✅ Part ID validation for non-PO items
- ✅ Package type required selection

## Testing Cascading Updates

The review grid implements intelligent cascading updates:

1. **Edit Part Number**: 
   - Click on any Part# cell in the review grid
   - Change "MMC-12345" to "MMC-99999"
   - All loads with "MMC-12345" will update to "MMC-99999"

2. **Edit PO Number**:
   - Click on any PO# cell
   - Change the PO number
   - All loads with the same Part# will get the new PO number

## Limitations (Mockup Only)

- No actual CSV file generation (console.log only)
- No actual database persistence (data lost on refresh)
- No session restoration from JSON file
- No label printing functionality
- Network delays simulated with setTimeout
- Limited error handling compared to production app

## Browser Requirements

- Modern browser with ES6+ support
- JavaScript enabled
- Local file access allowed (or run from a web server)

## Next Steps

Use this mockup to:
1. Validate the user workflow and step sequence
2. Test the UI/UX for data entry efficiency
3. Verify the review grid editing behavior
4. Confirm the cascading update logic
5. Identify any missing features or edge cases
6. Make corrections to the specification before implementation

## Making Corrections

After testing, update the specification (`spec.md`) with:
- Any workflow improvements
- Missing validation rules
- UI/UX adjustments
- Additional edge cases discovered
- Modified acceptance criteria

---

**Created**: December 16, 2025  
**Version**: 1.0  
**Purpose**: Interactive mockup for feature validation before WinUI implementation

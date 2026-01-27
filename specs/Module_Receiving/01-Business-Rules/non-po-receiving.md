# Non-PO Receiving

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [PO Number Dynamics](./po-number-dynamics.md), [Part Number Dynamics](./part-number-dynamics.md), [Hub Orchestration](../02-Workflow-Modes/004-hub-orchestration-specification.md), [Guided Mode](../02-Workflow-Modes/001-wizardmode-specification.md), [Manual Entry Mode](../02-Workflow-Modes/003-manual-mode-specification.md)

## Overview

Non-PO Receiving is a specialized receiving mode that allows users to receive items without an associated Purchase Order. This supports scenarios such as receiving miscellaneous items, customer returns, samples, warranty replacements, or other materials that don't originate from a formal PO.

**Key Design Decision:** Non-PO mode is selected at the **Hub Orchestration level** (Mode Selection screen) rather than within individual workflow modes. This creates a cleaner workflow separation and makes PO vs. Non-PO a first-class workflow choice.

## Concept

In normal receiving workflows, a PO Number is required and used to validate part selection against specific PO lines. Non-PO mode bypasses this requirement, allowing unrestricted part selection from the entire part master.

**Common Use Cases:**
- Miscellaneous supplies (office supplies, shop consumables)
- Customer returns (returned goods without PO)
- Free samples from vendors
- Warranty replacements
- Inter-facility transfers without PO
- Emergency/rush deliveries without formal PO
- Consignment inventory
- Loaner equipment returns

## Activation

### User Interface Element

**Location:** Mode Selection screen in Hub Orchestration (before entering Guided or Manual mode)

**Element:** Checkbox labeled "Non-PO Item (samples, returns, misc)" on mode selection cards

**Applicable Modes:**
- ✓ **Guided Mode** - Has Non-PO checkbox
- ✓ **Manual Entry Mode** - Has Non-PO checkbox
- ✗ **Edit Mode** - No checkbox (PO/Non-PO determined by loaded transaction)

**Default State:** Unchecked (PO mode is default)

### Enabling Non-PO Mode

**Action:** User checks the "Non-PO Item" checkbox on the Guided Mode or Manual Entry Mode card before clicking "Continue"

**Mode Selection UI:**
```
┌──────────────────────────────────────┐
│  📋 Guided Mode                      │
│                                      │
│  Step-by-step wizard workflow        │
│  for standard receiving              │
│                                      │
│  ─────────────────────────────────   │
│                                      │
│  ☐ Non-PO Item (samples, returns...) │ ← User checks this
│                                      │
│  [Continue]                          │
└──────────────────────────────────────┘
```

**Immediate Effects (upon checking):**
1. Visual indicator appears on card (e.g., "Guided Mode - Non-PO" badge)
2. Card background/border changes slightly (light blue tint)
3. Tooltip appears confirming Non-PO mode selection

**Session Initialization (when user clicks "Continue"):**
```csharp
var session = new ReceivingSession
{
    SessionId = Guid.NewGuid(),
    Mode = WorkflowMode.Guided, // or Manual
    IsNonPO = true, // ← Set from checkbox
    CurrentStep = "Step1",
    CreatedDate = DateTime.Now
};
```

### Confirmation Dialog (If PO Already Entered)

**NOTE:** This scenario **no longer applies** with Hub-level Non-PO selection.

**Rationale:** Since Non-PO mode is selected before entering the workflow (at Mode Selection), users cannot have PO data entered when they check the Non-PO checkbox. The checkbox state is determined before any workflow data exists.

**Historical Context (for reference):**  
Previously, the Non-PO checkbox was in Step 1 of Guided Mode, allowing users to toggle it after entering a PO Number. This required confirmation dialogs. The new Hub-level design eliminates this complexity.

## Part Selection in Non-PO Mode

### Unrestricted Part Search

**Behavior:** When Non-PO mode is active, part search shows ALL parts in the system without PO-based filtering.

**Search Scope:**
- All inventoried parts in Infor Visual
- All custom-defined parts
- All part types (MMC, MMF, MMCCS, MMFCS, MMCSR, MMFSR)

**Search Interface:**
```
┌─────────────────────────────────────────────┐
│ Part Number Search                          │
│ ─────────────────────────────────────────── │
│ [Search for part...]                    [🔍]│
│                                             │
│ Non-PO Mode: All parts available           │
│ ─────────────────────────────────────────── │
│ ○ MMC0001000 - Steel Coil, 0.100" Thick    │
│ ○ MMF0002000 - Flat Stock, 0.250" Thick    │
│ ○ MMCCS00123 - Customer Coil (Custom)      │
│ ○ MMFSR00511 - Special Request Flat Stock  │
│ ...                                         │
└─────────────────────────────────────────────┘
```

### Recent Non-PO Parts

**Feature:** System remembers recently used parts for Non-PO receiving to speed up selection for common items.

**Display:**
```
┌─────────────────────────────────────────────┐
│ Recent Non-PO Parts                         │
│ ─────────────────────────────────────────── │
│ ○ MMC0005000 - Sample Coil (last used 2h ago)
│ ○ MMF0010000 - Return Item (last used 1d ago)
│ ○ MISC0001 - Office Supplies (last used 3d ago)
└─────────────────────────────────────────────┘
```

### Part Validation in Non-PO Mode

**Required Validation:**
- Part Number must be valid format
- Part must exist in part master OR be manually entered (if allowed)
- Part Type must be assigned

**NOT Required:**
- PO Line validation (bypassed in Non-PO mode)
- PO-specific quantity constraints

## Disabling Non-PO Mode

### Reverting to PO Mode

**NOTE:** With Hub-level Non-PO selection, users cannot switch between PO and Non-PO modes mid-workflow.

**Rationale:** Non-PO mode is determined at Mode Selection before workflow data exists. Once the workflow begins, changing between PO and Non-PO modes would require restarting the workflow from Mode Selection.

**To Switch Modes:**
1. User clicks "Return to Mode Selection" button
2. System warns about unsaved changes (if applicable)
3. User confirms and returns to Mode Selection
4. User can select a different mode or toggle Non-PO checkbox
5. User clicks "Continue" to start fresh workflow

**Historical Context:**  
Previously, the Non-PO checkbox existed within Step 1, allowing mid-workflow toggling with confirmation dialogs. The Hub-level design eliminates this complexity and prevents confusing mode switches.

## Workflow with Non-PO Mode (Guided Mode)

### Mode Selection

**User selects Guided Mode with Non-PO checked:**
```
┌──────────────────────────────────────┐
│  📋 Guided Mode - Non-PO      ⬛     │
│                                      │
│  Step-by-step wizard workflow        │
│  for standard receiving              │
│                                      │
│  ─────────────────────────────────   │
│                                      │
│  ☑ Non-PO Item (samples, returns...) │ ← Checked
│                                      │
│  [Continue]                          │
└──────────────────────────────────────┘
```

**Session Created:**
```csharp
{
    SessionId: "...",
    Mode: WorkflowMode.Guided,
    IsNonPO: true,
    CurrentStep: "Step1"
}
```

### Step 1: Order & Part Selection (Non-PO)

**Field States:**
```
┌─────────────────────────────────────────────┐
│ Step 1 of 3: Order & Part Selection        │
│ ─────────────────────────────────────────── │
│                                             │
│ Mode: Non-PO Receiving ⬛                   │ ← Indicator shown
│                                             │
│ PO Number: [Not required for Non-PO items] │ ← Disabled/hidden
│                                             │
│ Part Number: [Search for part...] [🔍]     │
│   ○ Selected: MMC0005000 - Sample Coil     │
│                                             │
│ Load Count: [5]                             │
│                                             │
│           [Back]  [Next]                    │
└─────────────────────────────────────────────┘
```

**Validation:**
- ✓ Non-PO mode: Active (from session)
- ✓ PO Number: N/A (not required)
- ✓ Part Number: Selected (MMC0005000)
- ✓ Load Count: Valid (5)
- **Result:** Step 1 validation passes

### Step 2: Load Details Entry (Non-PO)

**Behavior:** Identical to PO-based receiving

- All load detail fields function normally
- Bulk copy operations work identically
- Validation rules apply as usual

**No Special Handling Required** - Non-PO mode only affects Step 1.

### Step 3: Review & Save (Non-PO)

**Review Display:**
```
┌─────────────────────────────────────────────┐
│ Review Receiving Transaction                │
│ ─────────────────────────────────────────── │
│                                             │
│ PO Number: Non-PO ⬛ [Edit Order & Part]    │
│ Part Number: MMC0005000                     │
│ Description: Sample Coil                    │
│ Load Count: 5 loads                         │
│                                             │
│ Load Details: [Edit Load Details]          │
│ • Load 1: 10,000 lbs, HL-001, SKID, 3 pkgs │
│ • Load 2: 10,000 lbs, HL-001, SKID, 3 pkgs │
│ ...                                         │
│                                             │
│           [Back]  [Save and Complete]       │
└─────────────────────────────────────────────┘
```

### Step 3: Review & Save (Non-PO - Guided Mode)

**Review Display:**
```
┌─────────────────────────────────────────────┐
│ Review Receiving Transaction                │
│ ─────────────────────────────────────────── │
│                                             │
│ PO Number: Non-PO ⬛ [Edit Order & Part]    │
│ Part Number: MMC0005000                     │
│ Description: Sample Coil                    │
│ Load Count: 5 loads                         │
│                                             │
│ Load Details: [Edit Load Details]          │
│ • Load 1: 10,000 lbs, HL-001, SKID, 3 pkgs │
│ • Load 2: 10,000 lbs, HL-001, SKID, 3 pkgs │
│ ...                                         │
│                                             │
│           [Back]  [Save and Complete]       │
└─────────────────────────────────────────────┘
```

**Visual Indicators:**
- PO Number shows: "Non-PO" with distinctive badge/icon ⬛
- Color-coded indicator (e.g., gray or blue) to distinguish from regular PO transactions

## Workflow with Non-PO Mode (Manual Entry Mode)

### Mode Selection

**User selects Manual Entry Mode with Non-PO checked:**
```
┌──────────────────────────────────────┐
│  📊 Manual Entry Mode - Non-PO  ⬛   │
│                                      │
│  Spreadsheet-style bulk entry        │
│  for high volume                     │
│                                      │
│  ─────────────────────────────────   │
│                                      │
│  ☑ Non-PO Item (samples, returns...) │ ← Checked
│                                      │
│  [Continue]                          │
└──────────────────────────────────────┘
```

**Session Created:**
```csharp
{
    SessionId: "...",
    Mode: WorkflowMode.Manual,
    IsNonPO: true
}
```

### Pre-Grid Configuration

**Before entering grid, user must configure:**
```
┌─────────────────────────────────────────────┐
│ Manual Entry Mode - Setup                   │
│ ─────────────────────────────────────────── │
│                                             │
│ Mode: Non-PO Receiving ⬛                   │
│                                             │
│ PO Number: [Not required for Non-PO items] │ ← Disabled/hidden
│                                             │
│ Part Number: [Search all parts...] [🔍]    │
│   ○ Selected: MMC0005000 - Sample Coil     │
│                                             │
│ Initial Row Count: [50]                     │
│                                             │
│           [Cancel]  [Start Entry]           │
└─────────────────────────────────────────────┘
```

**Validation:**
- ✓ Non-PO mode: Active (from session)
- ✓ PO Number: N/A (not required)
- ✓ Part Number: Selected (MMC0005000)
- ✓ Row Count: Valid (50)
- **Result:** Configuration passes, proceed to grid

### Grid Interface (Non-PO)

**Column Layout:**
```
┌──────────────────────────────────────────────────────────────────────┐
│ Manual Entry Mode - Non-PO Receiving: MMC0005000                     │
│ ──────────────────────────────────────────────────────────────────── │
│                                                                       │
│ [AutoFill ▼] [Set Copy Source] [Clear AutoFilled ▼] [Validate All]  │
│                                                                       │
│ Load# │ Weight │ Heat Lot │ Package Type │ Packages │ Receiving Loc  │
│ ───── │ ────── │ ──────── │ ──────────── │ ──────── │ ─────────────  │
│   1   │        │          │              │          │                │
│   2   │        │          │              │          │                │
│   3   │        │          │              │          │                │
│  ...  │        │          │              │          │                │
│  50   │        │          │              │          │                │
│                                                                       │
│                                        [Save and Finish]              │
└──────────────────────────────────────────────────────────────────────┘
```

**Key Differences from PO Mode:**
- **PO Number column:** Hidden (not present in grid)
- **Title:** Shows "Non-PO Receiving: [Part Number]"
- **Part Number:** Fixed (selected in pre-grid config, cannot change in grid)
- **All other functionality:** Identical to PO mode (bulk copy, keyboard navigation, validation)

### Validation

**Manual Entry Mode Non-PO Validation:**
```csharp
public class ManualEntryNonPOValidator
{
    public ValidationResult ValidateRow(ManualEntryRow row, bool isNonPO)
    {
        var errors = new List<string>();
        
        // PO Number NOT required in Non-PO mode
        if (!isNonPO && string.IsNullOrWhiteSpace(row.PONumber))
        {
            errors.Add("PO Number is required");
        }
        
        // Part Number required (set at session level, not per-row)
        if (string.IsNullOrWhiteSpace(row.PartNumber))
        {
            errors.Add("Part Number is required");
        }
        
        // Weight validation (same as PO mode)
        if (row.Weight <= 0)
        {
            errors.Add("Weight must be greater than zero");
        }
        
        // Heat Lot validation (same as PO mode)
        if (string.IsNullOrWhiteSpace(row.HeatLot))
        {
            errors.Add("Heat Lot is required");
        }
        
        // ... (other validations)
        
        return errors.Any() 
            ? ValidationResult.Errors(errors) 
            : ValidationResult.Success();
    }
}
```

### Save Operation (Manual Entry Mode Non-PO)

**CSV Export:**
```csv
PO_Number,Is_Non_PO,Part_Number,Load_Number,Weight,Heat_Lot,Package_Type,Packages_Per_Load,Created_By,Created_Date
,TRUE,MMC0005000,1,10000,HL-001,SKID,3,JDoe,2026-01-25 10:30:00
,TRUE,MMC0005000,2,10000,HL-001,SKID,3,JDoe,2026-01-25 10:30:00
,TRUE,MMC0005000,3,10000,HL-001,SKID,3,JDoe,2026-01-25 10:30:00
...
```

**Database Storage:**
```sql
INSERT INTO ReceivingTransactions
(TransactionId, PONumber, IsNonPO, PartNumber, LoadCount, CreatedBy, CreatedDate)
VALUES
('...', NULL, TRUE, 'MMC0005000', 50, 'JDoe', '2026-01-25 10:30:00');
```

## Save Operation (All Modes)
- PO Number shows: "Non-PO" with distinctive badge/icon ⬛
- Color-coded indicator (e.g., gray or blue) to distinguish from regular PO transactions

### Save Operation (Non-PO)

**Database Storage:**
```sql
INSERT INTO ReceivingTransactions
(
    TransactionId,
    PONumber, -- Stored as NULL or empty string
    IsNonPO, -- TRUE
    PartNumber,
    LoadCount,
    CreatedBy,
    CreatedDate
)
VALUES
(
    '...',
    NULL, -- or ''
    TRUE,
    'MMC0005000',
    5,
    'JDoe',
    '2026-01-25 10:30:00'
);
```

**CSV Export:**
```csv
PO_Number,Is_Non_PO,Part_Number,Load_Number,Weight,Heat_Lot,Package_Type,Packages_Per_Load,Created_By,Created_Date
,TRUE,MMC0005000,1,10000,HL-001,SKID,3,JDoe,2026-01-25 10:30:00
,TRUE,MMC0005000,2,10000,HL-001,SKID,3,JDoe,2026-01-25 10:30:00
```

Or with placeholder:
```csv
PO_Number,Part_Number,Load_Number,Weight,Heat_Lot,Package_Type,Packages_Per_Load,Created_By,Created_Date
NON-PO,MMC0005000,1,10000,HL-001,SKID,3,JDoe,2026-01-25 10:30:00
NON-PO,MMC0005000,2,10000,HL-001,SKID,3,JDoe,2026-01-25 10:30:00
```

## Non-PO Mode and Edit Mode

### Editing Historical Non-PO Transactions

**Scenario:** User opens Edit Mode and loads a historical Non-PO transaction

**Edit Mode Display:**
```
┌─────────────────────────────────────────────┐
│ Edit Transaction #T-2026-001               │
│ ─────────────────────────────────────────── │
│                                             │
│ [☑] Non-PO Item                             │
│ PO Number: [DISABLED]                       │
│ Part Number: MMC0005000                     │
│                                             │
│ [Load Details Grid...]                     │
│                                             │
│             [Cancel]  [Save Changes]        │
└─────────────────────────────────────────────┘
```

**Editing Restrictions:**
- Non-PO checkbox state matches original transaction
- Cannot switch between PO and Non-PO mode in Edit Mode
- Can modify part selection (within Non-PO scope)
- Can modify load details as usual

### Notification When Switching to Non-PO in Edit Mode

**Scenario:** User attempts to switch an existing PO transaction to Non-PO mode in Edit Mode

**Dialog:**
```
┌──────────────────────────────────────────────┐
│ ℹ Cannot Change PO Mode in Edit Mode        │
│ ────────────────────────────────────────────│
│                                              │
│ This transaction was originally created     │
│ with PO-123456.                              │
│                                              │
│ You cannot switch between PO and Non-PO     │
│ modes when editing historical transactions. │
│                                              │
│ To receive this part as Non-PO, create a    │
│ new receiving transaction.                   │
│                                              │
│                      [OK]                    │
└──────────────────────────────────────────────┘
```

## Validation Rules

### Non-PO Mode Validation

**Required Fields (Non-PO):**
- ✓ Non-PO Item checkbox: Checked
- ✓ Part Number: Selected or entered
- ✓ Load Count: ≥ 1

**NOT Required (Non-PO):**
- PO Number (field disabled)

**Step 1 Validation (Non-PO):**
```
if (IsNonPO)
{
    // PO Number NOT required
    if (PartNumber == null || PartNumber == "")
    {
        return ValidationError("Part selection is required for Non-PO items");
    }
    
    if (LoadCount < 1)
    {
        return ValidationError("Load count must be at least 1");
    }
    
    return ValidationSuccess();
}
```

### Switching Mode Validation

**Scenario:** User has data entered and switches mode

**Validation:**
- If switching from PO → Non-PO: Warn that PO Number will be cleared
- If switching from Non-PO → PO: Warn that Part selection will be cleared (if part was selected)

## Edge Cases

### Case 1: Accidentally Check Non-PO with PO Entered

**Scenario:** User enters PO-123456, then accidentally checks "Non-PO Item"

**Behavior:**
- Confirmation dialog shown
- User must confirm to proceed
- On confirm: PO Number cleared, Non-PO mode activated
- On cancel: Checkbox unchecked, no changes

### Case 2: Non-PO Transaction Without Part Selection

**Scenario:** User checks "Non-PO Item" but doesn't select a part, then clicks "Next"

**Behavior:**
- Validation error: "Part selection is required for Non-PO items"
- Step navigation blocked
- User must select part before proceeding

### Case 3: Load Count Change in Non-PO Mode

**Scenario:** User in Non-PO mode navigates back from Step 2 to Step 1 and changes load count from 5 to 8

**Behavior:**
- Existing loads 1-5: Data preserved
- New loads 6-8: Empty cells, ready for data entry or bulk copy
- Non-PO status preserved
- No additional validation required

### Case 4: Starting New Entry After Non-PO Transaction

**Scenario:** User completes Non-PO transaction, clicks "Start New Entry"

**Behavior:**
- Returns to Step 1
- "Non-PO Item" checkbox is **UNCHECKED** (defaults back to PO mode)
- User must explicitly check it again for another Non-PO transaction

**Rationale:** PO-based receiving is the most common workflow, so default back to it for user convenience.

### Case 5: Non-PO with Quality Hold Part

**Scenario:** User selects a part in Non-PO mode that requires Quality Hold

**Behavior:**
- Quality Hold procedures are displayed
- Same confirmation/acknowledgment required as PO mode
- Quality Hold flag is independent of PO/Non-PO status

### Case 6: Switching Between PO and Non-PO Multiple Times

**Scenario:** User toggles "Non-PO Item" checkbox multiple times

**Behavior:**
- Each toggle prompts confirmation (if data would be cleared)
- Data is cleared each time mode switches (after confirmation)
- Session tracks mode state changes for audit purposes

## User Interface Elements

### Non-PO Checkbox

**Visual Design:**
```
[☐] Non-PO Item
```

**States:**
- **Unchecked (Default):** PO mode active
  - PO Number field: Enabled (editable)
  - Part search: Restricted to PO
  
- **Checked:** Non-PO mode active
  - PO Number field: Disabled (grayed out)
  - Part search: Unrestricted (all parts)

**Tooltip (Unchecked):**
"Check this box to receive items without a Purchase Order (samples, returns, misc items)"

**Tooltip (Checked):**
"Non-PO mode active. Part search is unrestricted. Uncheck to require PO."

### PO Number Field (Non-PO Mode)

**Visual State:**
```
PO Number: [DISABLED - Not required for Non-PO items]
```

**Style:**
- Background: Light gray (disabled)
- Text: Gray or italic
- Placeholder: "Not required for Non-PO items"

### Visual Indicators on Review

**Non-PO Badge:**
```
PO Number: [Non-PO]  ⬛
```

**Color Scheme:**
- Badge background: Gray or blue
- Badge text: White or dark (high contrast)
- Icon: Distinctive symbol (e.g., square, tag, info icon)

## Reporting and Analytics

### Non-PO Transaction Tracking

**Database Query:**
```sql
SELECT
    TransactionId,
    PONumber,
    IsNonPO,
    PartNumber,
    LoadCount,
    CreatedBy,
    CreatedDate
FROM ReceivingTransactions
WHERE IsNonPO = TRUE
ORDER BY CreatedDate DESC;
```

**Analytics:**
- Non-PO transaction count by month
- Most common Non-PO parts
- Non-PO transaction volume by user
- Non-PO transactions by part type

### Audit Trail

**Non-PO Mode Changes:**
```
Event: Non-PO Mode Activated
User: JDoe
Timestamp: 2026-01-25 10:15:00
Previous PO Number: PO-123456 (cleared)
Action: User checked "Non-PO Item" checkbox
```

```
Event: Non-PO Transaction Saved
User: JDoe
Timestamp: 2026-01-25 10:30:00
Part Number: MMC0005000
Load Count: 5
PO Number: NULL (Non-PO)
```

## Testing Scenarios

### Test 1: Enable Non-PO Mode

**Given:** User on Step 1 with empty fields  
**When:** User checks "Non-PO Item" checkbox  
**Expected:**
- PO Number field disabled
- Part search unrestricted
- "Next" button enabled when part selected  
**Result:** ✓ Success

### Test 2: Enable Non-PO with Existing PO

**Given:** User has entered PO-123456  
**When:** User checks "Non-PO Item" checkbox  
**Expected:**
- Confirmation dialog shown
- On confirm: PO Number cleared, Non-PO activated
- On cancel: Checkbox unchecked, PO preserved  
**Result:** ✓ Success

### Test 3: Complete Non-PO Transaction

**Given:** User in Non-PO mode, part selected, load count entered  
**When:** User completes all steps and saves  
**Expected:**
- Step 3 shows "Non-PO" indicator
- Database stores IsNonPO = TRUE
- CSV export shows "Non-PO" or empty PO field  
**Result:** ✓ Success

### Test 4: Switch from Non-PO to PO Mode

**Given:** User in Non-PO mode with part selected  
**When:** User unchecks "Non-PO Item" checkbox  
**Expected:**
- Confirmation dialog shown
- On confirm: Part cleared, PO field enabled
- On cancel: Checkbox remains checked, part preserved  
**Result:** ✓ Success

### Test 5: Edit Historical Non-PO Transaction

**Given:** User opens Edit Mode for historical Non-PO transaction  
**When:** User views transaction details  
**Expected:**
- "Non-PO Item" checkbox checked and disabled (read-only)
- PO Number field shows "Non-PO" or empty
- Can modify load details as usual  
**Result:** ✓ Success

### Test 6: Non-PO with Quality Hold Part

**Given:** User in Non-PO mode selects part with Quality Hold flag  
**When:** System checks Quality Hold status  
**Expected:**
- Quality Hold procedures displayed
- User must acknowledge before proceeding
- Same workflow as PO mode  
**Result:** ✓ Success

## Related Documentation

- [PO Number Dynamics](./po-number-dynamics.md) - PO Number format and validation
- [Part Number Dynamics](./part-number-dynamics.md) - Part selection and validation
- [Quality Hold](./quality-hold.md) - Quality Hold procedures (applies to Non-PO)
- [Data Flow](../00-Core/data-flow.md) - Transaction save flow
- [Guided Mode](../02-Workflow-Modes/001-wizardmode-specification.md) - Step 1 workflow

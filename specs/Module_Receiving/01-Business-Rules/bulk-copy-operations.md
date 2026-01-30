# Bulk Copy Operations (Copy to Empty Cells Only)

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Data Flow](../00-Core/data-flow.md), [Guided Mode](../02-Workflow-Modes/001-wizardmode-specification.md), [Manual Entry Mode](../02-Workflow-Modes/003-manual-mode-specification.md)

## Overview

Bulk Copy Operations allow users to efficiently replicate data across multiple loads/rows by copying field values from a source load to target loads. The **fundamental rule** is that bulk copy operations **ONLY fill empty cells** and **NEVER overwrite occupied cells**, preserving manually entered data.

## Core Principle

**Empty-Cell-Only Rule:**  
All bulk copy operations fill ONLY empty cells in target loads. Occupied cells (cells with existing data) are ALWAYS preserved, regardless of whether they were manually entered or previously auto-filled.

This rule prevents accidental data loss and ensures user confidence when using bulk operations.

## Concept

In high-volume receiving scenarios (multiple identical loads), users enter data once for a source load and use bulk copy to apply that data to all other loads, dramatically reducing data entry time.

**Example Scenario:**
```
Receiving 50 loads of the same part with identical attributes:
- Source Load 1: Weight=10,000 lbs, Heat Lot=HL-001, Package Type=SKID, Packages=3
- Target Loads 2-50: All empty cells
- Bulk Copy Result: Loads 2-50 filled with Load 1's data (all cells empty, so all filled)
```

## Bulk Copy Modes

### 1. Copy All Fields to All Loads/Rows

**Operation:** Copies ALL fields from source load to empty cells in all other loads.

**Behavior:**
- Weight/Quantity → Copies to empty weight cells
- Heat Lot → Copies to empty heat lot cells
- Package Type → Copies to empty package type cells
- Packages Per Load → Copies to empty packages cells
- Receiving Location → Copies to empty location cells (if applicable)

**Example:**
```
Source: Load 1 (Weight=10,000, Heat Lot=HL-001, Package Type=SKID, Packages=3)

Before Copy:
- Load 1: Weight=10,000, Heat Lot=HL-001, Package Type=SKID, Packages=3
- Load 2: [empty], [empty], [empty], [empty]
- Load 3: Weight=12,000, [empty], [empty], [empty]
- Load 4: [empty], Heat Lot=HL-002, [empty], [empty]

After Copy (All Fields to All Loads):
- Load 1: Weight=10,000, Heat Lot=HL-001, Package Type=SKID, Packages=3 (source, unchanged)
- Load 2: Weight=10,000, Heat Lot=HL-001, Package Type=SKID, Packages=3 (all filled)
- Load 3: Weight=12,000, Heat Lot=HL-001, Package Type=SKID, Packages=3 (weight preserved, others filled)
- Load 4: [empty], Heat Lot=HL-002, Package Type=SKID, Packages=3 (heat lot preserved, others filled)
```

### 2. Copy Specific Field to All Loads/Rows

**Operation:** Copies ONLY the selected field from source load to empty cells in that field across all loads.

**Field Options:**
- Copy Weight/Quantity Only
- Copy Heat Lot Only
- Copy Package Type Only
- Copy Packages Per Load Only

**Example (Copy Heat Lot Only):**
```
Source: Load 1 (Heat Lot=HL-001)

Before Copy:
- Load 1: Weight=10,000, Heat Lot=HL-001, Package Type=SKID
- Load 2: Weight=12,000, [empty], [empty]
- Load 3: [empty], Heat Lot=HL-002, Package Type=PALLET

After Copy (Heat Lot Only):
- Load 1: Weight=10,000, Heat Lot=HL-001, Package Type=SKID (unchanged)
- Load 2: Weight=12,000, Heat Lot=HL-001, [empty] (heat lot filled, others unchanged)
- Load 3: [empty], Heat Lot=HL-002, Package Type=PALLET (HL-002 preserved, not overwritten)
```

### 3. Copy to Selected Loads/Rows Only

**Operation:** Copies fields from source load to empty cells in user-selected target loads only (not all loads).

**Selection Methods:**
- **Shift+Click:** Select range (e.g., Loads 5-10)
- **Ctrl+Click:** Select individual non-contiguous loads (e.g., Loads 3, 7, 15)

**Example:**
```
Source: Load 1 (Weight=10,000, Heat Lot=HL-001)
User selects Loads 5, 7, 9 (Ctrl+Click)

Before Copy:
- Load 1: Weight=10,000, Heat Lot=HL-001
- Load 2-4: [various data]
- Load 5: [empty], [empty]
- Load 6: [various data]
- Load 7: Weight=8,000, [empty]
- Load 8: [various data]
- Load 9: [empty], Heat Lot=HL-003

After Copy (All Fields to Selected):
- Load 1: Unchanged (source)
- Load 2-4: Unchanged (not selected)
- Load 5: Weight=10,000, Heat Lot=HL-001 (all empty, all filled)
- Load 6: Unchanged (not selected)
- Load 7: Weight=8,000, Heat Lot=HL-001 (weight preserved, heat lot filled)
- Load 8: Unchanged (not selected)
- Load 9: [empty], Heat Lot=HL-003 (heat lot preserved, weight remains empty)
```

## Copy Source Selection

### Default Copy Source

**Default:** Load 1 (or Row 1 in Manual Entry Mode) is the default copy source when the workflow begins.

### Changing Copy Source

**Operation:** User can designate any load as the copy source for subsequent bulk copy operations.

**UI:** "Copy Source" dropdown or "Set as Copy Source" button

**Example:**
```
User changes copy source from Load 1 to Load 5:
- Load 5 becomes the active source
- All subsequent "Copy to All Loads" operations use Load 5's data
- Visual indicator shows Load 5 is active source (e.g., highlight, icon)
```

**Use Case:**  
User discovers Load 1 had incorrect data but Load 5 is correct. Change source to Load 5 and re-apply bulk copy.

## Auto-Fill Metadata Tracking

### Metadata Purpose

The system tracks which cells were auto-filled vs. manually entered to support:
1. **Clear Auto-Filled Data** operations (selective clearing)
2. **Visual indicators** (showing which cells are auto-filled)
3. **Audit trail** (showing copy source and timestamp)

### Metadata Structure

```csharp
public class AutoFillMetadata
{
    public Guid LoadId { get; set; }
    public string FieldName { get; set; } // "Weight", "HeatLot", "PackageType", "PackagesPerLoad"
    public bool IsAutoFilled { get; set; }
    public Guid? CopySourceLoadId { get; set; } // Which load was the source
    public DateTime? AutoFilledAt { get; set; }
    public string AutoFilledBy { get; set; } // User who performed copy
}
```

### Visual Indicators

**Immediately After Copy (10 seconds):**
- Auto-filled cells display subtle highlight (e.g., light blue background)
- Indicates which cells were just filled

**On Hover:**
- Tooltip shows: "Auto-filled from Load 1 on [timestamp]"

**On Review Screen (Step 3):**
- Indicator shows: "Data auto-filled from Load 1 (occupied cells preserved)"

## Clear Auto-Filled Data

### Operation

Allows users to selectively clear ONLY auto-filled data while preserving manually entered data.

### Clear Options

**1. Clear All Auto-Filled Fields**
- Clears all cells that were populated by bulk copy operations
- Leaves manually entered data intact

**2. Clear Specific Auto-Filled Fields**
- Clear Auto-Filled Weight Only
- Clear Auto-Filled Heat Lot Only
- Clear Auto-Filled Package Type Only
- Clear Auto-Filled Packages Per Load Only

### Confirmation Dialog

```
Title: Clear Auto-Filled Data
Message: This will clear auto-filled data in [X] cells across [Y] loads.
         Manually entered data will be preserved. Continue?

Example: "This will clear auto-filled data in 45 cells across 15 loads.
          Manually entered data will be preserved. Continue?"

[Cancel] [Clear Auto-Filled Data]
```

### Example

```
Current State:
- Load 1: Weight=10,000 (manual), Heat Lot=HL-001 (manual), Package Type=SKID (manual)
- Load 2: Weight=10,000 (auto-filled), Heat Lot=HL-001 (auto-filled), Package Type=SKID (manual)
- Load 3: Weight=10,000 (auto-filled), Heat Lot=HL-002 (manual), Package Type=SKID (auto-filled)

User selects: "Clear All Auto-Filled Fields"

After Clear:
- Load 1: Weight=10,000, Heat Lot=HL-001, Package Type=SKID (all manual, unchanged)
- Load 2: [empty], [empty], Package Type=SKID (auto-filled cleared, manual preserved)
- Load 3: [empty], Heat Lot=HL-002, [empty] (auto-filled cleared, manual preserved)
```

## Validation Rules

### Pre-Copy Validation

**Source Load Validation:**
- Source load MUST NOT have validation errors
- If source has errors, bulk copy button is disabled
- Tooltip shows: "Cannot copy: Source load has validation errors. Fix errors before copying."

**Example:**
```
Load 1 has negative weight (-500)
User attempts to click "Copy All Fields to All Loads"
Result: Button disabled, tooltip shown
Fix: User corrects Load 1 weight to positive value (500)
Result: Button enabled, copy proceeds
```

### Post-Copy Validation

After bulk copy operation:
- All target loads undergo standard field validation
- Validation errors displayed per load
- "Jump to Next Error" button helps navigate to errors

## Performance Considerations

### Large Load Counts

**50+ Loads:**
- Display progress bar: "Copying to empty cells in 50 loads..."
- Complete operation within 1 second

**100+ Loads:**
- Use background processing
- Show progress: "Copying to empty cells in 100 loads... 45% complete"
- Final notification: "Filled X empty cells, preserved Y occupied cells"

### Virtual Scrolling

For grids with 100+ rows:
- Only render visible rows
- Bulk copy processes all rows in memory
- UI updates efficiently without freezing

## Preview Copy Operation

### Purpose

Allows users to see what will happen before executing bulk copy, reducing anxiety about accidental overwriting.

### Preview Dialog

**Display:**
- **Green Cells:** Will be filled (empty cells)
- **Yellow Cells:** Will be preserved (occupied cells)
- **Gray Cells:** Not affected (source load or unselected loads)

**Example Preview:**
```
┌─────────────────────────────────────────────┐
│ Preview Copy Operation                      │
│ ─────────────────────────────────────────── │
│ Source: Load 1                              │
│ Target: All Loads (50 loads)                │
│                                             │
│ ┌─ Load 2 ──────────────────────────────┐  │
│ │ Weight: [Green] Will fill             │  │
│ │ Heat Lot: [Green] Will fill           │  │
│ │ Package Type: [Yellow] Occupied       │  │
│ │ Packages: [Green] Will fill           │  │
│ └───────────────────────────────────────┘  │
│                                             │
│ Summary:                                    │
│ • Will fill 145 empty cells                │
│ • Will preserve 32 occupied cells          │
│                                             │
│           [Cancel]  [Copy to Empty Cells]  │
└─────────────────────────────────────────────┘
```

## Force Overwrite (Power User Feature)

### Purpose

Allows experienced users to intentionally overwrite occupied cells in specific situations.

**WARNING:** This is an advanced feature that bypasses the empty-cell-only rule. Use with caution.

### Operation

**Steps:**
1. User selects target loads (Shift+Click or Ctrl+Click)
2. User opens "Force Overwrite" submenu
3. User selects fields to overwrite
4. System shows confirmation warning
5. User confirms
6. System overwrites occupied cells in selected loads

### Confirmation Dialog

```
⚠ WARNING: Force Overwrite Occupied Cells

You are about to overwrite [X] occupied cells in [Y] selected loads.
This will REPLACE existing data and cannot be undone.

Fields to overwrite:
☑ Weight/Quantity
☑ Heat Lot
☐ Package Type
☐ Packages Per Load

Affected Loads: 3, 5, 7, 10

[Cancel] [Force Overwrite]
```

### Use Case

**Scenario:** User discovers all even-numbered loads have incorrect heat lot "HL-999" and need it changed to "HL-001" from Load 1.

**Solution:**
1. Select even-numbered loads (2, 4, 6, 8, 10) using Ctrl+Click
2. Open "Force Overwrite" submenu
3. Select "Heat Lot Only"
4. Confirm overwrite
5. Result: Heat Lot in loads 2, 4, 6, 8, 10 is overwritten with "HL-001"

## Integration with Load Count Changes

### Load Count Increase

**Scenario:** User increases load count from 5 to 8 in Step 1 (or adds rows in Manual Entry Mode).

**Behavior:**
- Existing loads 1-5: Data preserved
- New loads 6-8: Empty cells automatically receive copy source data
- Notification: "New loads initialized with Load 1 data (empty cells only)"

**Example:**
```
Original: 5 loads (Loads 1-5 with data)
User increases to 8 loads
Copy source: Load 1

Result:
- Loads 1-5: Unchanged (existing data preserved)
- Loads 6-8: Auto-filled from Load 1 (all cells empty, so all filled)
```

### Load Count Decrease

**Scenario:** User decreases load count from 8 to 5.

**Behavior:**
- Loads 6-8 are deleted
- Confirmation required: "This will discard data for loads 6, 7, and 8. Continue?"
- Auto-fill metadata for deleted loads is removed

## User Interface Elements

### Bulk Copy Toolbar/Dropdown

**Guided Mode (Step 2):**
```
┌─────────────────────────────────────────┐
│ ▼ Copy to All Loads                     │
│ ─────────────────────────────────────── │
│   Copy All Fields to All Loads          │
│   ─────────────────────────────────     │
│   Copy Weight Only                      │
│   Copy Heat Lot Only                    │
│   Copy Package Type Only                │
│   Copy Packages Per Load Only           │
│   ─────────────────────────────────     │
│   Copy All Fields to Selected Loads     │
│   Preview Copy Operation                │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ ▼ Copy Source                           │
│ ─────────────────────────────────────── │
│   ● Load 1 (Current)                    │
│   ○ Load 2                              │
│   ○ Load 3                              │
│   ...                                   │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ ▼ Clear Auto-Filled Data                │
│ ─────────────────────────────────────── │
│   Clear All Auto-Filled Fields          │
│   ─────────────────────────────────     │
│   Clear Auto-Filled Weight              │
│   Clear Auto-Filled Heat Lot            │
│   Clear Auto-Filled Package Type        │
│   Clear Auto-Filled Packages Per Load   │
└─────────────────────────────────────────┘
```

**Manual Entry Mode:**
- Same options via toolbar buttons with dropdown menus
- Consistent behavior across modes

## Success Messages

**After Bulk Copy:**
```
✓ Data copied to 50 loads (empty cells only)
  • Filled 145 empty cells
  • Preserved 32 occupied cells in 8 loads (Loads 3, 7, 9, 12, 15, 22, 31, 45)
```

**After Clear Auto-Filled:**
```
✓ Cleared 120 auto-filled cells in 40 loads
  • Manual data preserved
  • Loads ready for fresh data entry
```

## Error Handling

### Source Load Has Validation Errors

**Error:** "Cannot copy: Source load has validation errors. Fix errors before copying."  
**Action:** Button disabled until errors are corrected.

### No Empty Cells to Fill

**Warning:** "All target cells are occupied. No data copied."  
**Action:** Operation completes with no changes.

### Copy Operation Timeout (100+ loads)

**Error:** "Copy operation timed out. Please try again or contact support."  
**Action:** Retry button shown, or reduce load count.

## Testing Scenarios

### Test 1: Basic Copy All Fields

**Given:** Load 1 has all fields populated, Loads 2-5 are empty  
**When:** User clicks "Copy All Fields to All Loads"  
**Expected:** Loads 2-5 filled with Load 1 data  
**Result:** ✓ Success

### Test 2: Copy with Partial Data

**Given:** Load 1 populated, Load 2 has weight only, Load 3 has heat lot only  
**When:** User clicks "Copy All Fields to All Loads"  
**Expected:**
- Load 2: Weight preserved, other fields filled
- Load 3: Heat Lot preserved, other fields filled  
**Result:** ✓ Success

### Test 3: Copy Specific Field Only

**Given:** Load 1 populated, Loads 2-5 have mixed data  
**When:** User clicks "Copy Heat Lot Only"  
**Expected:** Only empty Heat Lot cells filled, all other fields unchanged  
**Result:** ✓ Success

### Test 4: Source with Validation Errors

**Given:** Load 1 has negative weight (-500)  
**When:** User attempts to click bulk copy  
**Expected:** Button disabled, tooltip shown  
**Result:** ✓ Success

### Test 5: Large Load Count Performance

**Given:** 100 loads, Load 1 populated, Loads 2-100 empty  
**When:** User clicks "Copy All Fields to All Loads"  
**Expected:** Operation completes in <1 second, progress shown  
**Result:** ✓ Success

### Test 6: Clear Auto-Filled Data

**Given:** 50 loads with auto-filled data from Load 1  
**When:** User clicks "Clear All Auto-Filled Fields"  
**Expected:** All auto-filled data cleared, manual data preserved  
**Result:** ✓ Success

## Related Documentation

- [Load Number Dynamics](./load-number-dynamics.md) - Load count changes affect bulk copy
- [Data Flow](../00-Core/data-flow.md) - Session state management
- [Guided Mode](../02-Workflow-Modes/001-wizardmode-specification.md) - Bulk copy in Step 2
- [Manual Entry Mode](../02-Workflow-Modes/003-manual-mode-specification.md) - Bulk copy in grid

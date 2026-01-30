# Load Number Dynamics

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Load Composition Rules](./load-composition-rules.md), [Data Flow](../00-Core/data-flow.md)

## Overview

Load Number defines how a single received line is split into multiple labels. This rule governs the automatic calculation of per-label quantities and the conditions under which calculations are triggered or suppressed.

## Concept

When receiving goods, a single PO line may need to be divided across multiple physical loads (e.g., multiple skids, pallets, or containers). The Load Number represents how many labels are required for this single PO line.

**Example:**
- PO Line: 45,000 lbs total
- Load Number: 3
- Result: 3 labels are generated

## Auto-Calculate Behavior

### Default Setting

**Auto-calculate is ON by default.**

When auto-calculate is enabled:
- System computes per-label quantity automatically
- Calculation formula: `Per-Label Quantity = Total Quantity ÷ Load Number`
- Result is rounded according to configured precision (default: nearest whole number)

### Calculation Trigger

Auto-calculation runs **ONCE** when:
1. User enters Load Number for the first time, OR
2. User changes Load Number value

**Important:** After the initial automatic calculation, user edits are **NEVER** overwritten.

### Recalculation Conditions

The system will re-run calculations ONLY if:

1. **Load Number Changes**
   - User modifies the Load Number field
   - System recalculates using new Load Number
   - Previous manual edits are overwritten (with warning)

2. **User Explicitly Requests Recalculation**
   - User clicks "Recalculate Quantities" button
   - System shows confirmation: "This will overwrite manual edits. Continue?"
   - User confirms
   - System recalculates all quantities

### Auto-Calculate Workflow Example (ON)

**Scenario:** Receiving 45,000 lbs across 3 loads

1. **User enters:**
   - Part Number: MMC0001000
   - PO Number: PO-123456
   - Total Quantity: 45,000 lbs
   - Load Number: 3

2. **System computes (automatic):**
   - Per-label quantity = 45,000 ÷ 3 = 15,000 lbs

3. **System generates:**
   - Load 1: 15,000 lbs
   - Load 2: 15,000 lbs
   - Load 3: 15,000 lbs

4. **User adjusts (manual override):**
   - User changes Load 1 to 16,000 lbs
   - User changes Load 2 to 14,500 lbs
   - User changes Load 3 to 14,500 lbs
   - Total remains 45,000 lbs

5. **User saves:**
   - Manual edits are preserved
   - No auto-recalculation occurs

### Manual Entry Mode (Auto-Calculate OFF)

When auto-calculate is disabled:
- System does NOT compute per-label quantities
- User must manually enter quantity for each label
- System validates that total matches expected quantity
- No automatic recalculation occurs

**Use Case:** Loads have intentionally unequal weights that don't follow a simple division pattern.

## Implementation Rules

### Rule 1: One-Time Calculation

**When:** Load Number is first entered  
**Action:** Calculate per-label quantity once  
**Formula:** `PerLabelQty = TotalQty ÷ LoadNumber`  
**Rounding:** Apply configured precision (default: whole number)  
**Storage:** Store calculated value as initial value

### Rule 2: Preserve User Edits

**When:** User manually modifies any label quantity  
**Action:** Mark field as "user-edited"  
**Protection:** Never overwrite user-edited values automatically  
**Visibility:** Show visual indicator (e.g., blue highlight) for edited fields

### Rule 3: Explicit Recalculation Only

**When:** User clicks "Recalculate Quantities"  
**Action:** Show confirmation dialog  
**Warning:** "This will overwrite all manual edits. Continue?"  
**On Confirm:** Recalculate all quantities  
**On Cancel:** Preserve existing values

### Rule 4: Load Number Change

**When:** Load Number is modified  
**Action:** Trigger recalculation  
**Warning:** "Changing Load Number will recalculate quantities. Continue?"  
**Effect:**
- Add rows if Load Number increases
- Remove rows if Load Number decreases (with confirmation)
- Recalculate all remaining quantities

## Validation Rules

### Load Number Validation

- **Minimum:** Load Number ≥ 1
- **Maximum:** Load Number ≤ 999 (practical limit)
- **Type:** Integer only (no decimals)
- **Required:** Yes (cannot be empty)

### Quantity Distribution Validation

- **Sum Requirement:** Sum of all per-label quantities MUST equal Total Quantity
- **Tolerance:** Allow ±1 unit tolerance for rounding issues
- **Warning:** If sum doesn't match total, show warning:
  - "Total quantity mismatch: Expected 45,000 lbs, actual 45,001 lbs. Adjust quantities."

### Per-Label Validation

Each label quantity must be:
- **Positive:** Quantity > 0
- **Reasonable:** Within expected range for part type
- **Numeric:** Valid decimal or integer value

## User Interface Requirements

### Load Number Entry

**Display Elements:**
- Input field for Load Number (integer)
- Label: "Number of Loads" or "Load Count"
- Tooltip: "Number of labels to generate for this PO line"
- Auto-calculate toggle: "Auto-calculate per-load quantities"

### Auto-Calculate Toggle

**When ON (default):**
- Show checkmark or "ON" indicator
- Display calculated quantities immediately
- Show "Recalculate Quantities" button

**When OFF:**
- Show empty checkbox or "OFF" indicator
- Display empty quantity fields for manual entry
- Hide "Recalculate Quantities" button

### Recalculate Button

**Visibility:** Shown only when auto-calculate is ON  
**State:** Enabled when manual edits exist  
**Action:** Trigger recalculation with confirmation  
**Confirmation Dialog:**
```
Title: Confirm Recalculation
Message: This will overwrite all manual edits and recalculate quantities based on current Load Number.
Total Quantity: 45,000 lbs
Load Number: 3
New Per-Load Quantity: 15,000 lbs

Continue?
[Cancel] [Recalculate]
```

### Visual Indicators

**Calculated Values:**
- Display in normal text
- No special highlighting

**User-Edited Values:**
- Display with blue background or border
- Show edit icon next to field
- Tooltip: "Manually edited (auto-calc disabled for this field)"

## Edge Cases

### Uneven Division

**Scenario:** Total Quantity doesn't divide evenly by Load Number

**Example:**
- Total Quantity: 45,500 lbs
- Load Number: 3
- Calculated: 45,500 ÷ 3 = 15,166.666... lbs

**Handling:**
1. Round each quantity to configured precision (default: 1 lb)
   - Load 1: 15,167 lbs
   - Load 2: 15,167 lbs
   - Load 3: 15,166 lbs
2. Reconcile total to match exactly:
   - Adjust last load to balance: 45,500 - (15,167 + 15,167) = 15,166 lbs
3. Show tooltip: "Quantities adjusted to total exactly 45,500 lbs"

**Advanced Handling (Uneven Division Toggle):**
- For complex scenarios with intentionally unequal loads
- See: [Load Composition Rules - Uneven Division Handling](./load-composition-rules.md#uneven-division-handling)

### Load Number Increase

**Scenario:** User increases Load Number from 3 to 5

**Actions:**
1. Show confirmation: "Add 2 new loads? Existing loads will be recalculated."
2. On confirm:
   - Add 2 new empty rows
   - Recalculate quantities for all 5 loads
   - Distribute total evenly: 45,000 ÷ 5 = 9,000 lbs each
3. User can then manually adjust as needed

### Load Number Decrease

**Scenario:** User decreases Load Number from 5 to 3

**Actions:**
1. Show confirmation: "Remove 2 loads? Data in removed loads will be lost."
2. On confirm:
   - Remove last 2 loads (Loads 4 and 5)
   - Recalculate quantities for remaining 3 loads
   - Distribute total evenly: 45,000 ÷ 3 = 15,000 lbs each
3. If removed loads had unique data (heat lots, etc.), show warning

### Zero or Negative Total

**Scenario:** User enters invalid Total Quantity

**Validation:**
- Block auto-calculation if Total Quantity ≤ 0
- Show error: "Total Quantity must be positive"
- Disable "Next" or "Save" button until corrected

### Load Number Zero or Negative

**Scenario:** User enters invalid Load Number

**Validation:**
- Reject Load Number < 1
- Show error: "Load Number must be at least 1"
- Prevent proceeding to next step

## Integration with Other Rules

### Load Composition Rules

When using pieces per load (coils, pieces):
- Auto-calculate applies to both total load weight AND per-piece weight
- See: [Load Composition Rules](./load-composition-rules.md)

### Uneven Division Handling

For advanced scenarios with measurement-based distribution:
- Auto-calculate is overridden by diameter-based calculations
- See: [Load Composition Rules - Overage Distribution](./load-composition-rules.md#overage-rounded-units--coils--accurate-distribution-rules)

### Quality Hold

Quality Hold status does not affect Load Number calculations:
- Calculations proceed normally
- Quality Hold is a separate validation step

## Testing Scenarios

### Test 1: Basic Auto-Calculate

**Given:**
- Total Quantity: 30,000 lbs
- Load Number: 3
- Auto-calculate: ON

**Expected:**
- Load 1: 10,000 lbs
- Load 2: 10,000 lbs
- Load 3: 10,000 lbs

### Test 2: Manual Override

**Given:**
- Initial calculation: 3 loads × 10,000 lbs = 30,000 lbs
- User changes Load 1 to 12,000 lbs
- User changes Load 2 to 9,000 lbs
- User changes Load 3 to 9,000 lbs

**Expected:**
- Load 1: 12,000 lbs (edited)
- Load 2: 9,000 lbs (edited)
- Load 3: 9,000 lbs (edited)
- No auto-recalculation
- Validation passes (sum = 30,000 lbs)

### Test 3: Explicit Recalculation

**Given:**
- User has manually edited quantities
- User clicks "Recalculate Quantities"
- User confirms

**Expected:**
- All loads reset to calculated values (10,000 lbs each)
- User edits are discarded
- Visual indicators removed

### Test 4: Uneven Division

**Given:**
- Total Quantity: 31,000 lbs
- Load Number: 3
- Auto-calculate: ON

**Expected:**
- Calculated: 31,000 ÷ 3 = 10,333.333...
- Rounded:
  - Load 1: 10,333 lbs
  - Load 2: 10,333 lbs
  - Load 3: 10,334 lbs (reconciled to match total)
- Sum verification: 10,333 + 10,333 + 10,334 = 31,000 ✓

### Test 5: Load Number Increase

**Given:**
- Original: 3 loads, 30,000 lbs total
- User changes Load Number to 5

**Expected:**
- Confirmation shown
- On confirm:
  - 5 loads created
  - Each load: 6,000 lbs
  - Previous manual edits overwritten

### Test 6: Auto-Calculate OFF

**Given:**
- Total Quantity: 30,000 lbs
- Load Number: 3
- Auto-calculate: OFF

**Expected:**
- All load quantity fields empty
- User must manually enter each quantity
- Validation requires sum = 30,000 lbs

## Related Documentation

- [Load Composition Rules](./load-composition-rules.md) - Pieces per load handling
- [Uneven Division Handling](./load-composition-rules.md#uneven-division-handling) - Advanced scenarios
- [Data Flow](../00-Core/data-flow.md) - Overall data flow architecture
- [Guided Mode Specification](../02-Workflow-Modes/001-workflow-consolidation-spec.md) - User workflow

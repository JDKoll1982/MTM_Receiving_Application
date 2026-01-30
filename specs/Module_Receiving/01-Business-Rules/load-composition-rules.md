# Load Composition Rules

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Load Number Dynamics](./load-number-dynamics.md), [Part Number Dynamics](./part-number-dynamics.md)

## Overview

Load Composition Rules define how to handle pieces per load (e.g., coils per skid, flat-stock pieces per pallet) and the calculations for distributing total load weight across individual pieces.

## Concept

Many manufactured parts are shipped in discrete units (coils, pieces, bars) that are grouped together on a single load (skid, pallet, container). The Load Composition Rules govern:

1. How many pieces constitute a load
2. How to distribute total load weight across pieces
3. How to handle uneven pieces (different sizes/weights)

## Basic Load Composition

### Standard Scenario: Equal Pieces

**Assumption:** All pieces on a load are identical in size and weight.

**Example 1: Coils on Skids**
```
Line 1: Part MMC0001000 (Coils), 2 Loads

Load 1:
- Number of coils: 2
- Total load weight: 20,000 lbs
- Per-coil weight: 20,000 ÷ 2 = 10,000 lbs each

Load 2:
- Number of coils: 3
- Total load weight: 30,000 lbs
- Per-coil weight: 30,000 ÷ 3 = 10,000 lbs each
```

**Example 2: Flat Stock on Pallets**
```
Line 2: Part MMF0002000 (Flat Stock), 3 Loads

Load 1:
- Number of pieces: 5
- Total load length: 500 ft
- Per-piece length: 500 ÷ 5 = 100 ft each

Load 2:
- Number of pieces: 10
- Total load length: 1,000 ft
- Per-piece length: 1,000 ÷ 10 = 100 ft each

Load 3:
- Number of pieces: 8
- Total load length: 800 ft
- Per-piece length: 800 ÷ 8 = 100 ft each
```

### Auto-Calculate (Pieces)

**Formula:** `Per-Piece Quantity = Load Total ÷ Pieces in Load`

**Behavior:**
- Runs **once** when pieces/load count is first entered
- User edits are **never** overwritten automatically
- Re-runs only if:
  - Pieces per load changes, OR
  - User clicks "Recalculate Quantities"

**Workflow:**
1. User enters total load weight (e.g., 20,000 lbs)
2. User enters number of pieces on load (e.g., 2 coils)
3. System calculates per-piece weight: 20,000 ÷ 2 = 10,000 lbs
4. User can manually adjust individual piece weights if needed
5. System validates that sum matches load total

## Uneven Division Handling

### Overview

Accessed via **"Uneven Division Handling" toggle**, which switches the current visual display to accommodate uneven division rules.

**Use Cases:**
- Coils with different diameters on same skid
- Mixed piece sizes in a single load
- Partial loads with odd-sized pieces
- Measurement-based weight distribution

### Overage (Rounded Units / Coils) — Accurate Distribution Rules

#### Scenario

One or more coils are larger than others on a skid (rounded unit variance). The skid weight entered is always **net material weight** (skid hardware excluded).

#### Required Inputs

1. **Total Load Weight** (net, e.g., lbs)
2. **Number of Coils** (pieces) on the skid
3. **Measured Diameters** for each coil:
   - **Minimum:** One "even" coil diameter AND one "overage" coil diameter
   - **Recommended:** All coil diameters (supports multiple overages, best accuracy)

#### Assumptions

- Width and thickness are consistent across coils of the same part
- Coil mass ∝ diameter² (area scaling approximation)
- Rounding precision is configurable (default: lbs to nearest 1.0)

#### Calculation Steps

**Step 1: Enter Data**
- User enters total load weight (net material only)
- User enters number of coils on the skid
- User measures and enters diameters:
  - Minimum case: One "even" coil diameter and one "overage" coil diameter
  - Preferred case: All coil diameters for best accuracy

**Step 2: Validate Inputs**
- TotalWeight > 0
- Pieces ≥ 1
- All diameters > 0 and within expected bounds for part type
- Units normalized (internal unit = lbs)

**Step 3: Baseline Equal Division (Reference Only)**
- Calculate: `BaselinePerCoil = TotalWeight ÷ Pieces`
- Store for reference
- **Not used if full diameter set is provided**

**Step 4: Weight Estimation Using Diameter² Ratios**

**Case A: All Diameters Provided**
```
For each coil i:
  RawWeight_i = (Diameter_i²) / Σ(Diameter_k² for all k) × TotalWeight
```

**Example:**
```
Coils: 3
Diameters: [66", 66", 32"]
Total Weight: W lbs

Diameter²:
- Coil A: 66² = 4,356
- Coil B: 66² = 4,356
- Coil C: 32² = 1,024
- Sum: 4,356 + 4,356 + 1,024 = 9,736

Raw Weights:
- Coil A: (4,356 / 9,736) × W = 0.4474W lbs
- Coil B: (4,356 / 9,736) × W = 0.4474W lbs
- Coil C: (1,024 / 9,736) × W = 0.1052W lbs
```

**Case B: Two Classes (Even + Overage)**
```
Given:
- OverageDiameter
- EvenDiameter
- Pieces (total)
- TotalWeight

Calculate:
  r = (OverageDiameter / EvenDiameter)²
  
  Let x = weight of each even coil
  Let y = weight of the overage coil
  
  Constraints:
    (Pieces - 1) × x + y = TotalWeight
    y = r × x
  
  Solve:
    x = TotalWeight ÷ ((Pieces - 1) + r)
    y = r × x
```

**Example:**
```
Coils: 3
Diameters: [62", 62", 77"] (two even, one overage)
Total Weight: W lbs

r = (77 / 62)² = 1.540

Even coil count = 2
x = W ÷ (2 + 1.540) = W ÷ 3.540 = 0.2825W
y = 1.540 × 0.2825W = 0.4350W

Result:
- 62" Coil 1: 0.2825W lbs
- 62" Coil 2: 0.2825W lbs
- 77" Coil 3: 0.4350W lbs
- Sum: 0.2825W + 0.2825W + 0.4350W = 1.0W ✓
```

**Step 5: Apply Rounding**

Round each RawWeight_i to configured precision (default: nearest 1 lb; ties use "round half up")

**Step 6: Reconcile Sum to Total**
```
Compute: Delta = TotalWeight - Σ(RoundedWeight_i)

If Delta ≠ 0:
  Apply Delta to the last coil deterministically
  
  OR (if last would violate bounds):
  Distribute proportionally and re-round
```

**Step 7: Bounds and Outlier Handling**

- If any single coil exceeds configured overage ratio threshold (default: r > 1.6):
  - Prompt user confirmation
  - Display: "Coil 3 is 60% larger than average. Confirm measurements?"

- If rounding or bounds cause violations (negative or implausible weights):
  - Revert to proportional (pre-round)
  - Re-round with proportional reconciliation

- If diameters are missing or invalid:
  - Fall back to equal division, OR
  - Require manual per-coil entry

**Step 8: Multiple Overage Support**

When multiple coils are oversized:
- Provide all diameters
- System distributes weights proportionally using diameter² scaling
- Performs same rounding and reconciliation

**Step 9: User Interaction**

- **"Uneven Division Handling" toggle** switches to measurement-based distribution UI
- **Show preview** of computed coil weights before applying
- **Allow per-coil "Manual Override"**:
  - Once edited, auto-calc will not overwrite
  - Unless user clicks "Recalculate Quantities" and confirms "Apply recalculated values"
- **Provide "Reset to Equal Distribution" option**

**Step 10: Recalculation Triggers**

Recalculation occurs when:
- Pieces change
- Any diameter value changes
- User clicks "Recalculate Quantities"

**Step 11: Persistence and Audit**

Save the following data:
- Inputs: TotalWeight, Pieces, diameters
- Computed weights
- Rounding precision
- Manual overrides

Record recalculation events:
- Timestamp
- Reason for recalculation
- User who triggered it

**Edit Mode Support:**
- Preserves audit trail
- Supports CSV re-export with modified values

### Worked Example 1: Standard Overage (3 Coils)

**Scenario:**
- Part: MMC0001000 (Coils)
- Skid: 3 coils
- Diameters: [77", 62", 62"] (the 77" coil is the overage)
- Total load weight (net): 30,000 lbs

**Computation:**
```
r = (77 / 62)² = (1.2419)² = 1.5424

Even coil count = 2
x = 30,000 ÷ (2 + 1.5424) = 30,000 ÷ 3.5424 = 8,467.74 lbs
y = 1.5424 × 8,467.74 = 13,064.52 lbs

Rounding (nearest 1 lb, round half up):
- 62" Coil 1: 8,468 lbs
- 62" Coil 2: 8,468 lbs
- 77" Coil 3: 13,065 lbs

Sum check: 8,468 + 8,468 + 13,065 = 30,001 lbs

Delta: 30,000 - 30,001 = -1 lb

Reconcile (adjust last coil):
- 77" Coil 3: 13,065 - 1 = 13,064 lbs

Final Result:
- 62" Coil 1: 8,468 lbs
- 62" Coil 2: 8,468 lbs
- 77" Coil 3: 13,064 lbs
- Total: 30,000 lbs ✓
```

### Worked Example 2: Partial on a Skid (3 Coils)

**Scenario:**
- Part: MMC0001000 (Coils)
- Skid: 3 coils
- Diameters: [66", 66", 32"] (two at 66", one smaller at 32")
- Total load weight (net): 25,000 lbs

**Computation Path (Full diameter set):**
```
Step 1: Compute squares
- 66² = 4,356
- 66² = 4,356
- 32² = 1,024

Step 2: Sum of squares
S = 4,356 + 4,356 + 1,024 = 9,736

Step 3: Raw per-coil weights (before rounding)
- Coil A (66"): (4,356 / 9,736) × 25,000 = 11,184.83 lbs
- Coil B (66"): (4,356 / 9,736) × 25,000 = 11,184.83 lbs
- Coil C (32"): (1,024 / 9,736) × 25,000 = 2,630.34 lbs

Step 4: Apply rounding (nearest 1 lb, round half up)
- Coil A: 11,185 lbs
- Coil B: 11,185 lbs
- Coil C: 2,630 lbs

Step 5: Reconcile to total
Sum: 11,185 + 11,185 + 2,630 = 25,000 lbs ✓ (no adjustment needed)

Final Result:
- 66" Coil A: 11,185 lbs
- 66" Coil B: 11,185 lbs
- 32" Coil C: 2,630 lbs
- Total: 25,000 lbs ✓
```

**Notes:**
- Baseline equal division reference: BaselinePerCoil = 25,000 ÷ 3 = 8,333 lbs (stored but not used)
- Outlier check: Evaluate ratios; prompt if any single coil exceeds threshold (default r > 1.6)
- Manual override allowed per coil; auto-calc won't overwrite unless user clicks "Recalculate Quantities" and confirms
- Persistence: Save diameters [66, 66, 32], computed weights [11,185, 11,185, 2,630], rounding precision, overrides, and recalculation events

## User Interface Requirements

### Standard Load Composition Entry

**Display Elements:**
- Input field: "Pieces per Load" (integer)
- Input field: "Total Load Weight/Length"
- Display: "Per-Piece Weight/Length" (calculated, read-only unless manually overridden)

### Uneven Division Toggle

**Toggle Control:**
- Checkbox or toggle switch: "Uneven Division Handling"
- When OFF: Standard equal-piece calculation
- When ON: Show measurement-based input fields

**Measurement-Based UI (When Toggle ON):**
```
┌─────────────────────────────────────────┐
│ Uneven Division Handling                │
│ ☑ Enable measurement-based distribution│
│                                         │
│ Total Load Weight: [30,000] lbs        │
│ Number of Coils:   [3]                  │
│                                         │
│ ┌─ Coil Diameters ──────────────────┐ │
│ │ Coil 1: [77] inches               │ │
│ │ Coil 2: [62] inches               │ │
│ │ Coil 3: [62] inches               │ │
│ └───────────────────────────────────┘ │
│                                         │
│ [Preview Calculated Weights]            │
│                                         │
│ ┌─ Calculated Weights ──────────────┐ │
│ │ Coil 1 (77"): 13,064 lbs         │ │
│ │ Coil 2 (62"):  8,468 lbs         │ │
│ │ Coil 3 (62"):  8,468 lbs         │ │
│ │ Total:        30,000 lbs ✓       │ │
│ └───────────────────────────────────┘ │
│                                         │
│ [Apply]  [Reset to Equal Distribution] │
└─────────────────────────────────────────┘
```

### Manual Override

**Per-Coil Override:**
- Click weight value to edit
- Show blue highlight when manually edited
- Tooltip: "Manually edited (auto-calc disabled)"
- Validation: Sum must equal total load weight

## Validation Rules

### Pieces Per Load Validation

- **Minimum:** Pieces ≥ 1
- **Maximum:** Pieces ≤ 999 (practical limit)
- **Type:** Integer only
- **Required:** Yes (when composition is enabled)

### Diameter Validation

- **Minimum:** Diameter > 0 inches
- **Maximum:** Diameter ≤ 120 inches (configurable per part type)
- **Type:** Numeric (decimal allowed)
- **Required:** Yes (when uneven division enabled)
- **Bounds Check:** Diameter within expected range for part type

### Weight Distribution Validation

- **Sum Requirement:** Sum of all piece weights MUST equal total load weight
- **Tolerance:** Allow ±1 unit tolerance for rounding
- **Warning:** If sum doesn't match:
  - "Total weight mismatch: Expected 30,000 lbs, actual 30,001 lbs. Adjust weights."

### Outlier Validation

- **Ratio Check:** If any piece exceeds threshold (default r > 1.6 relative to average):
  - Show confirmation: "Coil 1 (77\") is 60% larger than average. Confirm measurements?"
  - Allow user to confirm or re-measure

## Edge Cases

### Unequal Pieces with No Measurement Data

**Scenario:** User wants unequal pieces but doesn't have diameter measurements

**Handling:**
- Disable "Uneven Division Handling" toggle
- User must manually enter each piece weight
- System validates sum equals total

### Rounding Causes Sum Mismatch

**Scenario:** After rounding, sum doesn't equal total

**Handling:**
- Calculate Delta = TotalWeight - Sum(RoundedWeights)
- Apply Delta to last piece
- If last piece would go negative or exceed bounds:
  - Distribute Delta proportionally across all pieces
  - Re-round with proportional reconciliation

### Missing Diameter for One Coil

**Scenario:** User enters diameters for 2 of 3 coils

**Handling:**
- Show error: "All diameters required for uneven division"
- Prompt user to:
  - Enter missing diameter, OR
  - Switch to manual entry mode

### Zero or Negative Diameter

**Scenario:** User enters invalid diameter

**Handling:**
- Show error: "Diameter must be positive"
- Block calculation until corrected

## Integration with Other Rules

### Load Number Dynamics

- Load Composition applies within each load defined by Load Number
- Each load can have different piece counts
- Each load independently validates weight distribution

### Part Number Dynamics

- Part type determines expected ranges for measurements
- Coils (MMC*): Expect diameter measurements
- Flat Stock (MMF*): Expect length measurements
- Tubing: May use length or diameter

## Testing Scenarios

### Test 1: Equal Pieces (Standard)

**Given:**
- Total Load Weight: 30,000 lbs
- Pieces: 3 coils
- Uneven Division: OFF

**Expected:**
- Coil 1: 10,000 lbs
- Coil 2: 10,000 lbs
- Coil 3: 10,000 lbs

### Test 2: Uneven Division (All Diameters)

**Given:**
- Total Load Weight: 25,000 lbs
- Pieces: 3 coils
- Diameters: [66", 66", 32"]
- Uneven Division: ON

**Expected:**
- 66" Coil 1: 11,185 lbs
- 66" Coil 2: 11,185 lbs
- 32" Coil 3: 2,630 lbs
- Sum: 25,000 lbs ✓

### Test 3: Two-Class Overage

**Given:**
- Total Load Weight: 30,000 lbs
- Pieces: 3 coils
- Diameters: [62", 62", 77"] (two even, one overage)
- Uneven Division: ON

**Expected:**
- 62" Coil 1: 8,468 lbs
- 62" Coil 2: 8,468 lbs
- 77" Coil 3: 13,064 lbs
- Sum: 30,000 lbs ✓

### Test 4: Manual Override

**Given:**
- Initial calculation produces [10,000, 10,000, 10,000]
- User manually changes Coil 1 to 12,000 lbs
- User manually changes Coil 2 to 9,000 lbs
- User manually changes Coil 3 to 9,000 lbs

**Expected:**
- Coil 1: 12,000 lbs (edited)
- Coil 2: 9,000 lbs (edited)
- Coil 3: 9,000 lbs (edited)
- Sum validation: 30,000 lbs ✓
- No auto-recalculation

### Test 5: Recalculation After Manual Override

**Given:**
- User has manually edited weights
- User clicks "Recalculate Quantities"
- User confirms

**Expected:**
- All weights reset to calculated values
- Manual edits discarded
- Equal distribution restored: [10,000, 10,000, 10,000]

## Related Documentation

- [Load Number Dynamics](./load-number-dynamics.md) - Load count and auto-calculation
- [Part Number Dynamics](./part-number-dynamics.md) - Part type and measurement expectations
- [Data Flow](../00-Core/data-flow.md) - Overall data flow architecture
- [Guided Mode Specification](../02-Workflow-Modes/001-workflow-consolidation-spec.md)
- [Manual Entry Mode Specification](../02-Workflow-Modes/003-manual-mode-specification.md)

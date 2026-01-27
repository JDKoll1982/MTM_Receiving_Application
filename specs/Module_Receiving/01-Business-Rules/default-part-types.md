# Default Part Types

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Part Number Dynamics](./part-number-dynamics.md), [Load Composition Rules](./load-composition-rules.md)

## Overview

Part Types categorize manufactured parts based on their physical form and handling characteristics. This rule defines the available part types, default assignments by prefix, and Settings integration for custom type assignments.

## Available Part Types

### Core Part Types

**1. Coils**
- **Description:** Rolled metal sheets in coil form
- **Common Part Prefixes:** MMC*, MMCCS, MMCSR
- **Expected Measurements:** Diameter, Width, Thickness, Weight
- **Common Package Types:** Skid, Pallet
- **Handling Characteristics:** Round, heavy, requires coil rack

**2. Flat Stock**
- **Description:** Metal sheets or plates in flat form
- **Common Part Prefixes:** MMF*, MMFCS, MMFSR
- **Expected Measurements:** Length, Width, Thickness, Weight
- **Common Package Types:** Pallet, Bundle, Crate
- **Handling Characteristics:** Flat, stackable

**3. Tubing**
- **Description:** Hollow cylindrical metal tubes
- **Expected Measurements:** Outside Diameter, Wall Thickness, Length, Weight
- **Common Package Types:** Bundle, Rack
- **Handling Characteristics:** Cylindrical, long lengths

**4. Barstock**
- **Description:** Solid metal bars or rods
- **Expected Measurements:** Diameter (round) or Dimensions (square/hex), Length, Weight
- **Common Package Types:** Bundle, Rack
- **Handling Characteristics:** Straight, solid, various cross-sections

### Fasteners and Hardware

**5. Nuts**
- **Description:** Threaded fasteners
- **Expected Measurements:** Size, Thread Count, Quantity (count)
- **Common Package Types:** Box, Bag
- **Handling Characteristics:** Small, counted by piece

**6. Bolts**
- **Description:** Threaded fasteners with head
- **Expected Measurements:** Size, Length, Thread Count, Quantity (count)
- **Common Package Types:** Box, Bag
- **Handling Characteristics:** Small, counted by piece

**7. Washers**
- **Description:** Flat discs with center hole
- **Expected Measurements:** Inner Diameter, Outer Diameter, Thickness, Quantity (count)
- **Common Package Types:** Box, Bag
- **Handling Characteristics:** Small, counted by piece

**8. Bushings**
- **Description:** Cylindrical bearings or spacers
- **Expected Measurements:** Inner Diameter, Outer Diameter, Length, Quantity (count)
- **Common Package Types:** Box, Bag
- **Handling Characteristics:** Small, counted by piece

**9. Screws**
- **Description:** Threaded fasteners with point
- **Expected Measurements:** Size, Length, Thread Count, Quantity (count)
- **Common Package Types:** Box, Bag
- **Handling Characteristics:** Small, counted by piece

**10. Misc Hardware**
- **Description:** Other hardware and miscellaneous parts
- **Expected Measurements:** Varies by part
- **Common Package Types:** Box, Bag, Pallet
- **Handling Characteristics:** Varies

## Default Part Type Assignment

### Assignment by Prefix

**Automatic Assignment Rules:**

| Part Prefix | Default Part Type | Rationale |
|-------------|------------------|-----------|
| MMC* | Coils | Standard coil parts |
| MMCCS | Coils | Customer-supplied coils |
| MMCSR | Coils | Special request coils |
| MMF* | Flat Stock | Standard flat stock parts |
| MMFCS | Flat Stock | Customer-supplied flat stock |
| MMFSR | Flat Stock | Special request flat stock |
| Other | Misc Hardware | Unknown or unclassified parts |

**Assignment Workflow:**
1. User enters Part Number (e.g., MMC0001000)
2. System parses prefix (MMC)
3. System looks up default type for prefix (Coils)
4. System auto-assigns Part Type: Coils
5. If explicit override exists in Settings, use override instead

## Settings Integration

### Part Number Management - Part Type Configuration

**Access Path:**
```
Settings
  → Part Number Management
    → Select Part
      → Edit
        → Part Type (Dropdown)
```

**Dropdown Options:**
- Coils
- Flat Stock
- Tubing
- Barstock
- Nuts
- Bolts
- Washers
- Bushings
- Screws
- Misc Hardware

**Save Behavior:**
- Selected Part Type stored in application settings database
- Overrides automatic prefix-based assignment
- Applied automatically when part is entered in receiving workflow

### Settings Workflow Example

**Step 1: Navigate to Settings**
```
User: Settings → Part Number Management
```

**Step 2: Select Part**
```
User: Select MMC0005000 from dropdown or search
```

**Step 3: View Current Part Type**
```
Current Part Type: Coils (Auto-assigned by prefix)
```

**Step 4: Change Part Type (Optional)**
```
User: Changes dropdown to "Tubing"
Reason: MMC0005000 is actually tubing, not a coil (special case)
```

**Step 5: Save**
```
System: Saves MMC0005000 → Part Type: Tubing
Override: Active (will use "Tubing" instead of default "Coils")
```

**Step 6: Verify Override in Receiving**
```
Next receiving entry for MMC0005000:
  → Part Type displayed: Tubing (Settings override)
  → Expected measurements: Outside Diameter, Wall Thickness, Length, Weight
```

## Part Type Impact on UI and Behavior

### Measurement Field Display

**Part Type determines which measurement fields are shown:**

**Coils:**
```
┌───────────────────────────────────┐
│ Measurements                      │
│ ───────────────────────────────── │
│ Diameter:   [72] inches          │
│ Width:      [48] inches          │
│ Thickness:  [0.125] inches       │
│ Weight:     [10,000] lbs         │
└───────────────────────────────────┘
```

**Flat Stock:**
```
┌───────────────────────────────────┐
│ Measurements                      │
│ ───────────────────────────────── │
│ Length:     [120] inches         │
│ Width:      [48] inches          │
│ Thickness:  [0.25] inches        │
│ Weight:     [500] lbs            │
└───────────────────────────────────┘
```

**Tubing:**
```
┌───────────────────────────────────┐
│ Measurements                      │
│ ───────────────────────────────── │
│ Outside Diameter: [2.5] inches   │
│ Wall Thickness:   [0.125] inches │
│ Length:           [240] inches   │
│ Weight:           [50] lbs       │
└───────────────────────────────────┘
```

**Hardware (Nuts, Bolts, etc.):**
```
┌───────────────────────────────────┐
│ Measurements                      │
│ ───────────────────────────────── │
│ Size:      [1/2"-13]             │
│ Quantity:  [1000] pieces         │
│ Weight:    [25] lbs              │
└───────────────────────────────────┘
```

### Package Type Suggestions

**Part Type influences suggested package types:**

| Part Type | Suggested Package Types |
|-----------|------------------------|
| Coils | Skid, Pallet, Coil Rack |
| Flat Stock | Pallet, Bundle, Crate |
| Tubing | Bundle, Rack |
| Barstock | Bundle, Rack |
| Nuts/Bolts/Screws | Box, Bag |
| Washers/Bushings | Box, Bag |
| Misc Hardware | Box, Bag, Pallet |

**UI Implementation:**
- Auto-complete suggestions based on Part Type
- Most common package types appear first
- User can still enter custom package type

### Load Composition Behavior

**Part Type affects load composition rules:**

**Coils:**
- Support uneven division handling with diameter measurements
- Calculate per-coil weight using diameter² scaling
- See: [Load Composition Rules - Overage Distribution](./load-composition-rules.md#overage-rounded-units--coils--accurate-distribution-rules)

**Flat Stock:**
- Support pieces per pallet/bundle
- Calculate per-piece length or weight

**Hardware:**
- Typically counted by piece (quantity)
- Weight may be per-unit or total

## Validation and Business Rules

### Part Type Validation

**Required:** Yes (every part must have an assigned type)  
**Default:** Assigned automatically by prefix  
**Override:** Can be changed in Settings

**If No Type Assigned:**
- Fallback to "Misc Hardware"
- Show warning: "Part type not set. Defaulting to Misc Hardware."
- Suggest: "Configure in Settings → Part Number Management"

### Part Type Consistency

**Once Assigned:**
- Part Type should remain consistent for a given part number
- Changing Part Type in Settings affects all future entries
- Historical transactions retain their original Part Type

**Warning on Change:**
```
You are changing the Part Type for MMC0005000 from "Coils" to "Tubing".
This will affect all future receiving entries for this part.
Historical transactions will not be changed.
Continue?
[Cancel] [Change Part Type]
```

## User Interface Requirements

### Part Type Display (Receiving Workflow)

**Read-Only Display:**
- **Label:** "Part Type"
- **Display:** Show assigned Part Type (not editable in receiving workflow)
- **Tooltip:** "Configure Part Type in Settings → Part Number Management"

**Visual Indicator:**
- **Default (Auto-assigned):** Normal text, no icon
- **Override (Settings):** Blue info icon, tooltip "Custom Part Type from Settings"

### Part Type Selection (Settings)

**Dropdown Control:**
- **Label:** "Part Type"
- **Options:** All available part types (alphabetically sorted)
- **Default Selected:** Current assigned type (or auto-assigned type)
- **Change Confirmation:** If changing existing type, show warning

## Edge Cases

### Case 1: Unknown Prefix

**Scenario:** Part number has unrecognized prefix (e.g., XYZ0001000)

**Behavior:**
- Default Part Type: Misc Hardware
- Warning: "Part prefix not recognized. Using default type 'Misc Hardware'."
- User can change in Settings if needed

### Case 2: Override Doesn't Match Physical Form

**Scenario:** User sets MMC part (Coils prefix) to "Flat Stock" in Settings

**Behavior:**
- System uses Settings override: Flat Stock
- No validation error (user knows best)
- Measurement fields change to Flat Stock expectations
- Warning: "Part Type may not match part prefix. Verify configuration."

### Case 3: Part Type Changed After Historical Entries

**Scenario:** 100 historical transactions for MMC0005000 with Part Type "Coils", then type changed to "Tubing"

**Behavior:**
- Historical transactions: Retain "Coils" type
- New transactions: Use "Tubing" type
- Edit Mode: Show original Part Type for historical records
- No automatic backfill of changed type

## Implementation Notes

### Part Type Storage

**Application Settings Database:**
```sql
CREATE TABLE PartTypeOverrides (
    PartNumber VARCHAR(10) PRIMARY KEY,
    PartType VARCHAR(50) NOT NULL,
    SetBy VARCHAR(50),
    SetDate DATETIME,
    Notes TEXT
);
```

**Retrieval Logic:**
```csharp
public async Task<string> GetPartType(string partNumber)
{
    // 1. Check Settings override
    var override = await SettingsService.GetPartTypeOverride(partNumber);
    if (!string.IsNullOrEmpty(override))
    {
        return override;
    }
    
    // 2. Auto-assign by prefix
    if (partNumber.StartsWith("MMC"))
        return "Coils";
    if (partNumber.StartsWith("MMF"))
        return "Flat Stock";
    
    // 3. Fallback
    return "Misc Hardware";
}
```

## Testing Scenarios

### Test 1: Auto-Assign by Prefix (Coil)

**Given:**
- Part Number: MMC0001000
- No Settings override

**Expected:**
- Part Type: Coils (auto-assigned)
- Measurement Fields: Diameter, Width, Thickness, Weight
- Suggested Package Types: Skid, Pallet

### Test 2: Auto-Assign by Prefix (Flat Stock)

**Given:**
- Part Number: MMF0002000
- No Settings override

**Expected:**
- Part Type: Flat Stock (auto-assigned)
- Measurement Fields: Length, Width, Thickness, Weight
- Suggested Package Types: Pallet, Bundle, Crate

### Test 3: Settings Override

**Given:**
- Part Number: MMC0005000
- Settings Override: Tubing

**Expected:**
- Part Type: Tubing (Settings override)
- Measurement Fields: Outside Diameter, Wall Thickness, Length, Weight
- Source Indicator: Blue info icon "Custom Part Type from Settings"

### Test 4: Unknown Prefix Fallback

**Given:**
- Part Number: XYZ0001000
- No Settings override

**Expected:**
- Part Type: Misc Hardware (fallback)
- Warning: "Part prefix not recognized. Using default type 'Misc Hardware'."
- Measurement Fields: Generic Weight/Quantity

### Test 5: Change Part Type in Settings

**Given:**
- Part: MMC0005000
- Current Type: Coils
- User changes to: Tubing

**Expected:**
- Confirmation dialog: "Change Part Type from Coils to Tubing?"
- On confirm: Type saved as Tubing
- Future entries: Use Tubing type
- Historical entries: Retain Coils type

## Related Documentation

- [Part Number Dynamics](./part-number-dynamics.md) - Part number format and prefixes
- [Load Composition Rules](./load-composition-rules.md) - Part Type impact on load handling
- [Data Flow](../00-Core/data-flow.md) - Part Type in transaction flow
- [Guided Mode Specification](../02-Workflow-Modes/001-workflow-consolidation-spec.md)
- [Manual Entry Mode Specification](../02-Workflow-Modes/003-manual-mode-specification.md)

# Validation Rules

**Category**: Settings Category  
**Last Updated**: 2026-01-25  
**Related Documents**: [Business Rules](../../Module_Receiving/01-Business-Rules/), [CLARIFICATIONS.md](../../Module_Receiving/CLARIFICATIONS.md)

---

## Purpose

Validation Rules provides administrators with configuration options for data validation strictness, required field enforcement, and business logic validation thresholds. These settings determine what data is accepted during receiving workflows and when warnings or errors are shown to users.

---

## Access

**Location**: Main Menu → Settings → Validation Rules  
**Permission**: Administrator only (affects all users)  
**Scope**: System-wide (applies to all workflow modes)

---

## Settings Overview

This category contains **14 configurable validation rules** that control data quality and business logic enforcement.

---

## Validation Rules Settings

### 1. Required Field Enforcement

#### Require PO Number

**Setting Key**: `Receiving.Validation.RequirePoNumber`  
**Purpose**: Enforce PO number entry in PO-based receiving workflows.

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (required), `false` (optional)

**Behavior**:
```
When true:
- Wizard Mode: Cannot proceed from PO Entry step without valid PO
- Manual Mode: Row validation fails if PO Number is empty
- Error message: "PO Number is required"

When false:
- PO Number is optional
- User can proceed with empty PO (non-PO receiving)
```

**UI Component**: Checkbox  
☑ Require PO Number (recommended for PO-based workflows)

---

#### Require Part ID

**Setting Key**: `Receiving.Validation.RequirePartId`  
**Purpose**: Enforce Part ID entry in all receiving workflows.

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (required), `false` (optional)

**Behavior**:
```
When true:
- All modes: Cannot save transaction without valid Part ID
- Error message: "Part ID is required"

When false:
- Part ID optional (rarely used)
```

**UI Component**: Checkbox  
☑ Require Part ID (strongly recommended)

---

#### Require Quantity

**Setting Key**: `Receiving.Validation.RequireQuantity`  
**Purpose**: Enforce quantity entry (weight/count).

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (required), `false` (optional)

**Behavior**:
```
When true:
- Quantity field cannot be zero or empty
- Error message: "Quantity is required and must be greater than zero"

When false:
- Zero quantity allowed (edge case for tracking empty containers)
```

**UI Component**: Checkbox  
☑ Require Quantity

**Related**: See Edge Case 6 - Zero packages validation (resolved: not allowed)

---

#### Require Heat/Lot Number

**Setting Key**: `Receiving.Validation.RequireHeatLot`  
**Purpose**: Enforce Heat/Lot number entry for traceability.

**Data Type**: Boolean  
**Default Value**: `false`  
**Valid Values**: `true` (required), `false` (optional)

**Behavior**:
```
When true:
- Heat/Lot field cannot be empty
- User must enter value for each load
- Error message: "Heat/Lot number is required for traceability"

When false:
- Heat/Lot number is optional (default behavior)
```

**UI Component**: Checkbox  
☐ Require Heat/Lot Number (optional - enable for strict traceability)

**Note**: Quality Hold parts may have separate Heat/Lot requirements.

---

### 2. Quantity Validation

#### Allow Negative Quantity

**Setting Key**: `Receiving.Validation.AllowNegativeQuantity`  
**Purpose**: Control whether negative quantities are accepted (returns/adjustments).

**Data Type**: Boolean  
**Default Value**: `false`  
**Valid Values**: `true` (allow), `false` (block)

**Behavior**:
```
When false (default):
- Negative quantities blocked
- Error message: "Quantity must be positive"

When true:
- Negative quantities allowed (for returns/corrections)
- Warning message: "You are entering a negative quantity (return/adjustment)"
```

**UI Component**: Checkbox  
☐ Allow Negative Quantity (enable only if returns processed through receiving)

---

#### Minimum Quantity

**Setting Key**: `Receiving.Validation.MinQuantity`  
**Purpose**: Set minimum acceptable quantity per load.

**Data Type**: Integer  
**Default Value**: `1`  
**Valid Values**: `0` to `999999`

**Behavior**:
```
If user enters quantity below minimum:
- Error message: "Quantity must be at least {MinQuantity}"
- Row validation fails
```

**UI Component**: Numeric input  
Minimum quantity per load: [____1____] ▲▼

**Example**:
- MinQuantity = 1 → At least 1 unit per load
- MinQuantity = 10 → Minimum 10 units per load

---

#### Maximum Quantity

**Setting Key**: `Receiving.Validation.MaxQuantity`  
**Purpose**: Set maximum acceptable quantity per load (safety check).

**Data Type**: Integer  
**Default Value**: `999999`  
**Valid Values**: `1` to `999999`

**Behavior**:
```
If user enters quantity above maximum:
- Warning message: "Quantity {qty} exceeds maximum {MaxQuantity}. Please verify."
- User can proceed with confirmation (warning, not error)
```

**UI Component**: Numeric input  
Maximum quantity per load: [__999999__] ▲▼

**Example**:
- MaxQuantity = 10000 → Warn if load exceeds 10,000 lbs
- MaxQuantity = 50 → Warn if load exceeds 50 pieces

---

### 3. Load Count Validation

#### Minimum Load Count

**Setting Key**: `Receiving.Validation.MinLoadCount`  
**Purpose**: Set minimum number of loads per transaction.

**Data Type**: Integer  
**Default Value**: `1`  
**Valid Values**: `1` to `99`

**Behavior**:
```
If user enters load count below minimum:
- Error message: "Must enter at least {MinLoadCount} load(s)"
```

**UI Component**: Numeric input  
Minimum load count: [____1____] ▲▼

---

#### Maximum Load Count

**Setting Key**: `Receiving.Validation.MaxLoadCount`  
**Purpose**: Set maximum number of loads per transaction (UI performance).

**Data Type**: Integer  
**Default Value**: `99`  
**Valid Values**: `1` to `99`

**Behavior**:
```
If user enters load count above maximum:
- Error message: "Maximum {MaxLoadCount} loads per transaction. Split into multiple transactions."
```

**UI Component**: Numeric input  
Maximum load count: [___99____] ▲▼

**Note**: Related to Edge Case 17 (grid performance) - lower values improve UI performance.

---

### 4. ERP Integration Validation

#### Validate PO Exists in ERP

**Setting Key**: `Receiving.Validation.ValidatePoExists`  
**Purpose**: Check if PO number exists in Infor Visual before accepting.

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (validate), `false` (skip validation)

**Behavior**:
```
When true:
- On PO entry: Query Infor Visual to verify PO exists
- If PO not found: Error message "PO {po_number} not found in ERP"
- User cannot proceed

When false:
- Skip ERP validation
- Accept any PO number (useful for testing or offline mode)
```

**UI Component**: Checkbox  
☑ Validate PO exists in ERP (recommended for production)

**Related**: Edge Case 9 (PO format validation)

---

#### Validate Part Exists in ERP

**Setting Key**: `Receiving.Validation.ValidatePartExists`  
**Purpose**: Check if Part ID exists in Infor Visual before accepting.

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (validate), `false` (skip validation)

**Behavior**:
```
When true:
- On Part entry: Query Infor Visual to verify Part exists
- If Part not found: Warning message "Part {part_id} not found in ERP. Proceed?"
- User can override (warning, not error)

When false:
- Skip ERP validation
- Accept any Part ID
```

**UI Component**: Checkbox  
☑ Validate Part exists in ERP (recommended)

**Related**: Edge Case 7 (case-insensitive search), Edge Case 8 (non-standard parts)

---

### 5. Business Logic Warnings

#### Warn on Quantity Exceeds PO

**Setting Key**: `Receiving.Validation.WarnOnQuantityExceedsPo`  
**Purpose**: Show warning when received quantity exceeds PO quantity.

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (warn), `false` (no warning)

**Behavior**:
```
When true:
- If total received > PO ordered quantity:
  Warning message: "Total quantity {qty} exceeds PO ordered quantity {po_qty}. Proceed?"
- User can override (warning, not error)

When false:
- No warning for over-receiving
```

**UI Component**: Checkbox  
☑ Warn when quantity exceeds PO (recommended for inventory control)

---

#### Warn on Same-Day Receiving

**Setting Key**: `Receiving.Validation.WarnOnSameDayReceiving`  
**Purpose**: Show warning when receiving same PO/Part multiple times on same day.

**Data Type**: Boolean  
**Default Value**: `true`  
**Valid Values**: `true` (warn), `false` (no warning)

**Behavior**:
```
When true:
- Check if PO + Part already received today
- If yes: Warning message "PO {po} Part {part} already received today. Duplicate entry?"
- User can proceed (warning, not error)

When false:
- No duplicate check
```

**UI Component**: Checkbox  
☑ Warn on same-day duplicate receiving (recommended to catch data entry errors)

---

## User Interface Design

### Main Settings View

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ Validation Rules                                         [?] Help [✕] Close │
│ ═══════════════════════════════════════════════════════════════════════════│
│                                                                              │
│ Configure data validation rules for receiving workflows.                    │
│ Changes affect all users and workflow modes (Wizard, Manual, Edit).         │
│                                                                              │
│ REQUIRED FIELDS                                                             │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ☑ Require PO Number                                                         │
│ ☑ Require Part ID                                                           │
│ ☑ Require Quantity (weight/count)                                           │
│ ☐ Require Heat/Lot Number (optional - enable for strict traceability)      │
│                                                                              │
│ QUANTITY VALIDATION                                                         │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ☐ Allow Negative Quantity (for returns/adjustments)                        │
│                                                                              │
│ Minimum quantity per load                                                   │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ 1                                                                ▲▼  │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│ ℹ Minimum units/weight required per load (1-999999)                        │
│                                                                              │
│ Maximum quantity per load (warning threshold)                               │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ 999999                                                           ▲▼  │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│ ℹ Warn if quantity exceeds this value (safety check)                       │
│                                                                              │
│ LOAD COUNT VALIDATION                                                       │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ Minimum loads per transaction                                               │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ 1                                                                ▲▼  │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│ ℹ Must enter at least this many loads (1-99)                               │
│                                                                              │
│ Maximum loads per transaction                                               │
│ ┌──────────────────────────────────────────────────────────────────────┐   │
│ │ 99                                                               ▲▼  │   │
│ └──────────────────────────────────────────────────────────────────────┘   │
│ ℹ UI performance degrades beyond 50-100 loads. Split large transactions.   │
│                                                                              │
│ ERP INTEGRATION VALIDATION                                                  │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ☑ Validate PO exists in Infor Visual                                       │
│ ℹ Check PO number against ERP before accepting (recommended)               │
│                                                                              │
│ ☑ Validate Part exists in Infor Visual                                     │
│ ℹ Check Part ID against ERP (shows warning if not found)                   │
│                                                                              │
│ BUSINESS LOGIC WARNINGS                                                     │
│ ───────────────────────────────────────────────────────────────────────────│
│                                                                              │
│ ☑ Warn when received quantity exceeds PO quantity                          │
│ ℹ Alert user if over-receiving (can proceed with confirmation)             │
│                                                                              │
│ ☑ Warn on same-day duplicate receiving (same PO + Part)                    │
│ ℹ Catch potential data entry errors (can proceed with confirmation)        │
│                                                                              │
│                      [Reset to Defaults]  [Cancel]  [Save]                  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Integration with Receiving Workflows

### Wizard Mode
1. **PO Entry Step**: Validate RequirePoNumber, ValidatePoExists
2. **Part Entry**: Validate RequirePartId, ValidatePartExists
3. **Load Entry**: Validate MinLoadCount, MaxLoadCount
4. **Quantity Entry**: Validate RequireQuantity, MinQuantity, MaxQuantity, AllowNegativeQuantity
5. **Heat/Lot Entry**: Validate RequireHeatLot
6. **Review Step**: Check WarnOnQuantityExceedsPo, WarnOnSameDayReceiving

### Manual Entry Mode
- **Real-time validation**: Each field validates on blur/change
- **Row-level validation**: All rules apply per row
- **Batch validation**: Check all rules before save

### Edit Mode
- **Same rules apply**: Editing existing transactions uses same validation
- **Additional check**: Warn if modifying quantities after CSV export

---

## Validation Error Messages

### Error Messages (Block Action)
```
RequirePoNumber = false, empty PO:
→ "PO Number is required. Enter a valid PO or switch to Non-PO mode."

RequirePartId = false, empty Part:
→ "Part ID is required."

RequireQuantity = false, zero quantity:
→ "Quantity is required and must be greater than zero."

RequireHeatLot = false, empty Heat/Lot:
→ "Heat/Lot number is required for traceability."

AllowNegativeQuantity = false, negative value:
→ "Negative quantities are not allowed."

Quantity < MinQuantity:
→ "Quantity must be at least {MinQuantity}."

Load count < MinLoadCount:
→ "Must enter at least {MinLoadCount} load(s)."

Load count > MaxLoadCount:
→ "Maximum {MaxLoadCount} loads per transaction. Split into multiple transactions."

ValidatePoExists = true, PO not found:
→ "PO {po_number} not found in ERP system."
```

### Warning Messages (Allow Override)
```
Quantity > MaxQuantity:
→ "Quantity {qty} exceeds maximum {MaxQuantity}. Please verify. Proceed?"

ValidatePartExists = true, Part not found:
→ "Part {part_id} not found in ERP. Proceed with non-standard part?"

WarnOnQuantityExceedsPo = true:
→ "Total quantity {qty} exceeds PO ordered quantity {po_qty}. Over-receiving allowed. Proceed?"

WarnOnSameDayReceiving = true:
→ "PO {po} Part {part} already received today at {time}. Duplicate entry? Proceed?"
```

---

## Performance Considerations

### ERP Validation Performance
```
ValidatePoExists = true:
- Adds network latency (~100-500ms per PO lookup)
- Cache PO results for session to reduce queries
- Timeout after 5 seconds, show error

ValidatePartExists = true:
- Adds network latency (~100-500ms per Part lookup)
- Cache Part results for session
- Timeout after 5 seconds, show warning (not error)
```

### Recommendation:
- Enable ERP validation in production (data quality)
- Disable during testing or offline scenarios (performance)

---

## Database Schema

All validation rules stored in `system_settings` table:

```sql
CREATE TABLE system_settings (
    setting_id INT PRIMARY KEY IDENTITY(1,1),
    category NVARCHAR(50) NOT NULL, -- 'Receiving'
    key_name NVARCHAR(100) NOT NULL, -- 'Validation.RequirePoNumber'
    value NVARCHAR(MAX) NOT NULL, -- 'true' or numeric values
    data_type NVARCHAR(20) NOT NULL, -- 'Boolean', 'Integer'
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    updated_by NVARCHAR(100),
    UNIQUE (category, key_name)
);
```

---

## Related Edge Cases (from CLARIFICATIONS.md)

- **Edge Case 6**: Zero packages validation → RequireQuantity setting
- **Edge Case 7**: Part number case sensitivity → ValidatePartExists with case-insensitive lookup
- **Edge Case 8**: Non-standard parts → ValidatePartExists allows override
- **Edge Case 9**: PO format validation → ValidatePoExists with auto-standardization
- **Edge Case 17**: Grid performance → MaxLoadCount limits UI complexity

---

## Related Documentation

- [Business Rules - Load Number Dynamics](../../Module_Receiving/01-Business-Rules/load-number-dynamics.md)
- [Business Rules - PO Number Dynamics](../../Module_Receiving/01-Business-Rules/po-number-dynamics.md)
- [Business Rules - Part Number Dynamics](../../Module_Receiving/01-Business-Rules/part-number-dynamics.md)
- [Wizard Mode Specification](../../Module_Receiving/02-Workflow-Modes/001-wizard-mode-specification.md)
- [Manual Entry Mode Specification](../../Module_Receiving/02-Workflow-Modes/002-manual-entry-mode-specification.md)

---

**Last Updated:** 2026-01-25  
**Status:** Complete  
**Settings Count:** 14 validation rules

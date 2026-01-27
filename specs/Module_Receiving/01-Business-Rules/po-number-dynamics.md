# P.O. Number Dynamics

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Part Number Dynamics](./part-number-dynamics.md), [Data Flow](../00-Core/data-flow.md)

## Overview

Purchase Order (P.O.) Numbers follow a standardized format for consistent data entry and downstream system integration. This rule defines the format, validation logic, auto-standardization behavior, and reusable utility implementation.

## Standard Format

### Required Format

**Pattern:** `PO-` + 6 digits

**Examples:**
- `PO-123456`
- `PO-000001`
- `PO-999999`

**Components:**
- **Prefix:** `PO-` (uppercase, with hyphen)
- **Digits:** Exactly 6 numeric digits (0-9)
- **Total Length:** 9 characters

## Validation and Standardization Rules

### User Input Flexibility

Users may enter P.O. Numbers with or without the `PO-` prefix:

**Accepted Input Formats:**
- With prefix: `PO-123456`
- Without prefix: `123456`
- With lowercase prefix: `po-123456`
- With space: `PO 123456`
- Without hyphen: `PO123456`

### Auto-Standardization

The system automatically standardizes P.O. Numbers on save:

**Standardization Rules:**
1. If input is 6 digits only → Prepend `PO-`
   - Input: `123456` → Saved as: `PO-123456`

2. If input starts with `po-` (lowercase) → Convert to uppercase
   - Input: `po-123456` → Saved as: `PO-123456`

3. If input contains space instead of hyphen → Replace with hyphen
   - Input: `PO 123456` → Saved as: `PO-123456`

4. If input already matches standard format → No change
   - Input: `PO-123456` → Saved as: `PO-123456`
5. If input is missing the dash after `PO` → Insert dash
   - Input: `PO123456` → Saved as: `PO-123456`

### Validation Rules

**Format Validation:**
- Digits MUST be exactly 6 characters (after removing prefix)
- Digits MUST be numeric (0-9 only)
- Prefix (if provided) MUST be `PO-` or `po-` or `PO ` (with space) or `PO` (without hyphen)
- NO letters allowed in digit portion
- NO special characters except hyphen in prefix

**Error Conditions:**
- Less than 6 digits: "P.O. Number must have 6 digits"
- More than 6 digits: "P.O. Number must have exactly 6 digits"
- Contains letters (except prefix): "P.O. Number digits must be numeric only"
- Invalid prefix: "Invalid P.O. Number format. Use PO-XXXXXX or 6 digits"

## Implementation as Shared Utility

### Reusable Function Specification

**Purpose:** Provide consistent P.O. Number validation and standardization across all modules.

**Function Signature:**
```csharp
public static class Helper_Validation_PONumber
{
    /// <summary>
    /// Validates and standardizes a P.O. Number input.
    /// </summary>
    /// <param name="input">Raw P.O. Number input from user</param>
    /// <param name="standardized">Standardized P.O. Number in format PO-XXXXXX</param>
    /// <param name="errorMessage">Validation error message if validation fails</param>
    /// <returns>True if valid and standardized, False if validation fails</returns>
    public static bool ValidateAndStandardize(
        string input,
        out string standardized,
        out string errorMessage)
    {
        // Implementation here
    }
}
```

**Usage Example:**
```csharp
// In ViewModel or Service
string userInput = "123456";
if (Helper_Validation_PONumber.ValidateAndStandardize(
    userInput,
    out string standardizedPO,
    out string error))
{
    // Valid: Use standardizedPO (will be "PO-123456")
    PONumber = standardizedPO;
}
else
{
    // Invalid: Show error message
    ShowError(error);
}
```

### Standardization Algorithm

**Step 1: Clean Input**
```csharp
// Remove leading/trailing whitespace
input = input.Trim();

// Replace spaces with hyphens
input = input.Replace(" ", "-");

// Convert to uppercase
input = input.ToUpper();
```

**Step 2: Parse Prefix and Digits**
```csharp
string prefix = "";
string digits = "";

if (input.StartsWith("PO-"))
{
    prefix = "PO-";
    digits = input.Substring(3); // Everything after "PO-"
}
else
{
    // Assume entire input is digits
    digits = input;
}
```

**Step 3: Validate Digits**
```csharp
// Check digit count
if (digits.Length != 6)
{
    errorMessage = "P.O. Number must have exactly 6 digits";
    return false;
}

// Check all characters are digits
if (!digits.All(char.IsDigit))
{
    errorMessage = "P.O. Number digits must be numeric only";
    return false;
}
```

**Step 4: Standardize Output**
```csharp
standardized = $"PO-{digits}";
errorMessage = "";
return true;
```

## User Interface Requirements

### P.O. Number Entry Field

**Input Field Properties:**
- **Label:** "P.O. Number"
- **Placeholder:** "Enter P.O. Number (e.g., PO-123456 or 123456)"
- **Max Length:** 15 characters (allows for various input formats)
- **Auto-Format:** On blur, standardize to `PO-XXXXXX` format

**Real-Time Feedback:**
- **Valid:** Green checkmark icon
- **Invalid:** Red X icon with error message
- **Standardized:** Show tooltip "Standardized to PO-123456" after auto-format

### Validation Display

**On Blur (Field Exit):**
```
Input:  123456
Action: Auto-standardize
Display: PO-123456
Tooltip: "Standardized to PO-123456"
```

**On Invalid Input:**
```
Input:  12345 (only 5 digits)
Display: Error icon + message
Error:  "P.O. Number must have exactly 6 digits"
Field:  Highlighted in red
```

**On Save Attempt:**
- If P.O. Number is invalid, block save operation
- Display error: "Please correct P.O. Number before saving"
- Focus on P.O. Number field

## Edge Cases and Examples

### Example 1: Digits Only (Most Common)

**Input:** `123456`  
**Standardized:** `PO-123456`  
**Result:** Valid ✓

### Example 2: Already Standardized

**Input:** `PO-123456`  
**Standardized:** `PO-123456`  
**Result:** Valid ✓ (no change)

### Example 3: Lowercase Prefix

**Input:** `po-123456`  
**Standardized:** `PO-123456`  
**Result:** Valid ✓ (prefix converted to uppercase)

### Example 4: Space Instead of Hyphen

**Input:** `PO 123456`  
**Standardized:** `PO-123456`  
**Result:** Valid ✓ (space replaced with hyphen)

### Example 5: Too Few Digits

**Input:** `12345` (5 digits)  
**Error:** "P.O. Number must have exactly 6 digits"  
**Result:** Invalid ✗

### Example 6: Too Many Digits

**Input:** `1234567` (7 digits)  
**Error:** "P.O. Number must have exactly 6 digits"  
**Result:** Invalid ✗

### Example 7: Contains Letters

**Input:** `12345A`  
**Error:** "P.O. Number digits must be numeric only"  
**Result:** Invalid ✗

### Example 8: Invalid Prefix

**Input:** `ABC-123456`  
**Error:** "Invalid P.O. Number format. Use PO-XXXXXX or 6 digits"  
**Result:** Invalid ✗

### Example 9: Leading Zeros Preserved

**Input:** `000123`  
**Standardized:** `PO-000123`  
**Result:** Valid ✓ (leading zeros preserved)

### Example 10: Mixed Format

**Input:** `po 123456` (lowercase with space)  
**Standardized:** `PO-123456`  
**Result:** Valid ✓ (converted to uppercase, space replaced)

## Integration Points

### Module_Receiving

**All Modes:**
- Guided Mode (Wizard)
- Manual Entry Mode
- Edit Mode

**Validation Timing:**
- On field blur (exit)
- Before proceeding to next step
- Before save operation

### Module_Shipping (Future)

Same validation and standardization logic applies.

### Module_Inventory (Future)

P.O. Number lookups use standardized format.

### CSV Export

All exported P.O. Numbers are in standardized format:
```csv
PO_Number,Part_Number,Load_Number
PO-123456,MMC0001000,1
PO-123456,MMC0001000,2
```

### Database Storage

Store P.O. Numbers in standardized format:
```sql
INSERT INTO ReceivingTransactions (PONumber, ...)
VALUES ('PO-123456', ...);
```

## Testing Scenarios

### Test 1: Standard Input (Digits Only)

**Input:** `123456`  
**Expected Standardized:** `PO-123456`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, tooltip "Standardized to PO-123456"

### Test 2: Already Standardized

**Input:** `PO-123456`  
**Expected Standardized:** `PO-123456`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, no tooltip (no change)

### Test 3: Lowercase with Space

**Input:** `po 123456`  
**Expected Standardized:** `PO-123456`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, tooltip "Standardized to PO-123456"

### Test 4: Invalid - Too Short

**Input:** `12345`  
**Expected Standardized:** N/A  
**Expected Result:** Invalid ✗  
**Expected Error:** "P.O. Number must have exactly 6 digits"  
**Expected Display:** Red X icon, error message, red border

### Test 5: Invalid - Contains Letter

**Input:** `12345A`  
**Expected Standardized:** N/A  
**Expected Result:** Invalid ✗  
**Expected Error:** "P.O. Number digits must be numeric only"  
**Expected Display:** Red X icon, error message, red border

### Test 6: Leading Zeros

**Input:** `000001`  
**Expected Standardized:** `PO-000001`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, tooltip "Standardized to PO-000001"

### Test 7: Invalid Prefix

**Input:** `XYZ-123456`  
**Expected Standardized:** N/A  
**Expected Result:** Invalid ✗  
**Expected Error:** "Invalid P.O. Number format. Use PO-XXXXXX or 6 digits"  
**Expected Display:** Red X icon, error message, red border

### Test 8: Save Attempt with Invalid P.O.

**Given:** P.O. Number field contains invalid value `12345`  
**When:** User clicks "Save" or "Next"  
**Expected:**
- Save/Next blocked
- Error message: "Please correct P.O. Number before saving"
- Focus returns to P.O. Number field
- Field highlighted in red

## Error Messages Reference

| Input Condition | Error Message |
|----------------|---------------|
| Fewer than 6 digits | "P.O. Number must have exactly 6 digits" |
| More than 6 digits | "P.O. Number must have exactly 6 digits" |
| Contains non-numeric characters in digits | "P.O. Number digits must be numeric only" |
| Invalid prefix (not PO- or po-) | "Invalid P.O. Number format. Use PO-XXXXXX or 6 digits" |
| Empty/null | "P.O. Number is required" |

## Success Messages Reference

| Scenario | Tooltip/Message |
|----------|-----------------|
| Auto-standardized (changed) | "Standardized to PO-XXXXXX" |
| Already in standard format | (No message - already correct) |
| Valid on first entry | (Green checkmark only) |

## Related Documentation

- [Part Number Dynamics](./part-number-dynamics.md) - Related validation patterns
- [Data Flow](../00-Core/data-flow.md) - P.O. Number in transaction flow
- [Guided Mode Specification](../02-Workflow-Modes/001-workflow-consolidation-spec.md) - P.O. entry workflow
- [Manual Entry Mode Specification](../02-Workflow-Modes/003-manual-mode-specification.md) - Grid-based P.O. entry
- [Edit Mode Specification](../02-Workflow-Modes/002-editmode-specification.md) - P.O. search and modification

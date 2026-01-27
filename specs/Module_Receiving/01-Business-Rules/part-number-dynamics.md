# Part Number Dynamics

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Default Part Types](./default-part-types.md), [Load Composition Rules](./load-composition-rules.md)

## Overview

Part Numbers follow a standardized prefix-based naming convention that indicates the type of material and its source. This rule defines the valid prefixes, validation logic, and integration with other system components.

## Part Number Prefixes

### Standard Materials

**MMC - Standard Coil**
- Format: `MMC` + 7 digits (e.g., `MMC0001000`)
- Material Type: Coil (rolled metal)
- Source: Regular inventory
- Expected Measurements: Diameter, width, thickness, weight
- Common Package Types: Skid, Pallet

**MMF - Standard Flat Stock**
- Format: `MMF` + 7 digits (e.g., `MMF0002000`)
- Material Type: Flat stock (sheets, plates)
- Source: Regular inventory
- Expected Measurements: Length, width, thickness, weight
- Common Package Types: Pallet, Bundle

### Customer-Supplied Materials

**MMCCS - Coil Supplied by Customer**
- Format: `MMCCS` + 5 digits (e.g., `MMCCS01000`)
- Material Type: Coil
- Source: Customer-supplied material
- Expected Measurements: Same as MMC
- Special Handling: May require additional documentation

**MMFCS - Flat Stock Supplied by Customer**
- Format: `MMFCS` + 5 digits (e.g., `MMFCS02000`)
- Material Type: Flat stock
- Source: Customer-supplied material
- Expected Measurements: Same as MMF
- Special Handling: May require additional documentation

### Special Request Materials

**MMCSR - Special Request Coil**
- Format: `MMCSR` + 5 digits (e.g., `MMCSR99000`)
- Material Type: Coil
- Source: One-time or special order
- Expected Measurements: Same as MMC
- Special Handling: Custom specifications

**MMFSR - Special Request Flat Stock**
- Format: `MMFSR` + 5 digits (e.g., `MMFSR99001`)
- Material Type: Flat stock
- Source: One-time or special order
- Expected Measurements: Same as MMF
- Special Handling: Custom specifications

## Auto-Formatting Behavior

### User Input Flexibility

Users may enter part numbers with or without zero-padding in the numeric portion. The system automatically standardizes part numbers on save to ensure consistent formatting.

**Accepted Input Formats:**

**Standard Parts (MMC, MMF):**
- With padding: `MMC0001000`
- Without padding: `MMC1000`
- Partial padding: `MMC001000`
- With lowercase: `mmc1000`

**Customer-Supplied and Special Request (MMCCS, MMFCS, MMCSR, MMFSR):**
- With padding: `MMCCS00364`
- Without padding: `MMCCS364`
- Partial padding: `MMCCS0364`
- With lowercase: `mmccs364`

### Auto-Standardization

The system automatically standardizes part numbers on field blur (exit) or save:

**Standardization Rules:**

1. **Convert prefix to uppercase**
   - Input: `mmc1000` → Output: `MMC0001000`
   - Input: `mmfsr511` → Output: `MMFSR00511`

2. **Pad numeric portion with leading zeros**
   - Standard Parts (MMC/MMF): Pad to 7 digits
     - Input: `MMC1000` → Output: `MMC0001000`
     - Input: `MMF25` → Output: `MMF0000025`
   
   - Customer-Supplied/Special Request: Pad to 5 digits
     - Input: `MMCCS364` → Output: `MMCCS00364`
     - Input: `MMFSR511` → Output: `MMFSR00511`
     - Input: `MMCSR99` → Output: `MMCSR00099`

3. **Preserve existing padding**
   - Input: `MMC0001000` → Output: `MMC0001000` (no change)

### Auto-Standardization Examples

| User Input | Auto-Standardized Output | Notes |
|------------|-------------------------|-------|
| `MMC1000` | `MMC0001000` | 7-digit padding applied |
| `mmc1000` | `MMC0001000` | Uppercase + padding |
| `MMF25` | `MMF0000025` | Pad to 7 digits |
| `MMCCS364` | `MMCCS00364` | 5-digit padding applied |
| `mmccs364` | `MMCCS00364` | Uppercase + padding |
| `MMFSR511` | `MMFSR00511` | 5-digit padding applied |
| `MMCSR99` | `MMCSR00099` | Pad to 5 digits |
| `MMC0001000` | `MMC0001000` | Already standardized |

## Validation Rules

### Format Validation

**Standard Parts (MMC, MMF):**
- Prefix: Exactly 3 characters (MMC or MMF)
- Digits: 1 to 7 digits following prefix (will be padded to 7)
- Total Length After Standardization: 10 characters
- Valid Characters: Uppercase/lowercase letters (prefix) + digits only
- Examples:
  - Valid Input: `MMC1000`, `mmc1000`, `MMC0001000`
  - Standardized: `MMC0001000`
  - Invalid: `MMC` (no digits), `MMC000100A` (contains letter in digits)

**Customer-Supplied and Special Request (MMCCS, MMFCS, MMCSR, MMFSR):**
- Prefix: Exactly 5 characters
- Digits: 1 to 5 digits following prefix (will be padded to 5)
- Total Length After Standardization: 10 characters
- Valid Characters: Uppercase/lowercase letters (prefix) + digits only
- Examples:
  - Valid Input: `MMCCS364`, `mmccs364`, `MMCCS00364`
  - Standardized: `MMCCS00364`
  - Invalid: `MMCCS` (no digits), `MMCCS12345A` (contains letter in digits)

### Business Rule Validation

**Existence Validation:**
- Part must exist in Infor Visual (for standard parts)
- Customer-supplied and special request parts may not exist in ERP
- If part doesn't exist in ERP:
  - Show warning: "Part not found in Infor Visual. Continue with manual entry?"
  - Allow user to proceed or cancel

**Part Type Association:**
- Each part has an assigned Part Type (see [Default Part Types](./default-part-types.md))
- Part Type determines expected measurements and package types
- Part Type can be set/changed in Settings → Part Number Management

**Quality Hold Check:**
- Check if part requires Quality Hold (see [Quality Hold](./quality-hold.md))
- If Quality Hold required:
  - Display Quality Hold procedures
  - Require acknowledgment before proceeding

## Auto-Population Behavior

### Receiving Location Auto-Pull

When user enters a valid part number:
1. System queries Infor Visual for default receiving location
2. If found, auto-populate Receiving Location field
3. If not found, default to "RECV"
4. User can override during transaction (see [Receiving Location Dynamics](./receiving-location-dynamics.md))

### Part Type Assignment

1. System determines Part Type based on prefix:
   - MMC* → Default to "Coils"
   - MMF* → Default to "Flat Stock"
2. If Part Type explicitly set in Settings, use that value
3. Display Part Type in UI for user reference

### Expected Measurements

Based on Part Type, show appropriate input fields:
- **Coils:** Diameter, Width, Thickness, Weight
- **Flat Stock:** Length, Width, Thickness, Weight
- **Tubing:** Outside Diameter, Wall Thickness, Length, Weight
- **Other:** Generic Weight/Quantity field

## Integration Points

### Infor Visual (ERP)

**Part Master Data:**
- Part Number lookup
- Part description
- Default receiving location
- Unit of measure
- Standard package type

**Read-Only Access:**
- System ONLY reads from Infor Visual
- NO write operations to ERP
- Connection string includes `ApplicationIntent=ReadOnly`

### Settings Module

**Part Number Management:**
- Part Number → Part Type assignment
- Part Number → Default Receiving Location override
- Part Number → Quality Hold flag
- Part Number → Custom specifications

### Load Composition Rules

Part prefix influences load composition:
- **Coils (MMC*, MMCCS, MMCSR):** Support pieces per load with diameter measurements
- **Flat Stock (MMF*, MMFCS, MMFSR):** Support pieces per load with length measurements
- **Other:** Generic quantity handling

## User Interface Requirements

### Part Number Entry

**Input Field:**
- Label: "Part Number"
- Placeholder: "Enter part number (e.g., MMC1000 or MMC0001000)"
- Validation: Real-time format validation
- Auto-Format: On blur, standardize to proper format with zero-padding
- Auto-complete: Suggest matching parts from ERP (if available)

**Validation Feedback:**
- **Valid:** Green checkmark icon
- **Auto-Formatted:** Show tooltip "Standardized to MMC0001000" after formatting
- **Invalid Format:** Red X icon with error message
- **Not Found in ERP:** Yellow warning icon with message

**Error Messages:**
- "Invalid format. Expected MMC + up to 7 digits or MMF + up to 7 digits"
- "Invalid format. Expected MMCCS/MMFCS/MMCSR/MMFSR + up to 5 digits"
- "Part not found in Infor Visual. Verify part number or continue with manual entry."
- "This part requires Quality Hold. Review procedures before continuing."

**Auto-Format Display:**

**On Blur (Field Exit):**
```
Input:  MMC1000
Action: Auto-standardize with zero-padding
Display: MMC0001000
Tooltip: "Standardized to MMC0001000"
```

```
Input:  MMCCS364
Action: Auto-standardize with zero-padding
Display: MMCCS00364
Tooltip: "Standardized to MMCCS00364"
```

**On Invalid Input:**
```
Input:  MMC000100A (letter in digits)
Display: Error icon + message
Error:  "Invalid format. Part number digits must be numeric only"
Field:  Highlighted in red
```

### Part Information Display

After valid part entry, display:
```
┌─────────────────────────────────────────┐
│ Part Information                        │
│ ─────────────────────────────────────── │
│ Part Number:     MMC0001000             │
│ Description:     Steel Coil, 0.100" Thick│
│ Part Type:       Coils                  │
│ Default Location: V-C0-01               │
│ Unit of Measure: LBS                    │
│ Quality Hold:    No                     │
└─────────────────────────────────────────┘
```

## Implementation Notes

### Part Number Standardization Function

**Purpose:** Provide consistent part number validation and standardization across all modules.

**Function Signature:**
```csharp
public static class Helper_Validation_PartNumber
{
    /// <summary>
    /// Validates and standardizes a Part Number input with zero-padding.
    /// </summary>
    /// <param name="input">Raw Part Number input from user</param>
    /// <param name="standardized">Standardized Part Number with proper zero-padding</param>
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
string userInput = "MMC1000";
if (Helper_Validation_PartNumber.ValidateAndStandardize(
    userInput,
    out string standardizedPart,
    out string error))
{
    // Valid: Use standardizedPart (will be "MMC0001000")
    PartNumber = standardizedPart;
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

// Convert to uppercase
input = input.ToUpper();
```

**Step 2: Parse Prefix and Digits**
```csharp
string prefix = "";
string digits = "";
int requiredDigitLength = 0;

// Determine prefix and required digit length
if (input.StartsWith("MMCCS") || input.StartsWith("MMFCS") ||
    input.StartsWith("MMCSR") || input.StartsWith("MMFSR"))
{
    prefix = input.Substring(0, 5);
    digits = input.Substring(5);
    requiredDigitLength = 5;
}
else if (input.StartsWith("MMC") || input.StartsWith("MMF"))
{
    prefix = input.Substring(0, 3);
    digits = input.Substring(3);
    requiredDigitLength = 7;
}
else
{
    errorMessage = "Invalid part number prefix. Expected: MMC, MMF, MMCCS, MMFCS, MMCSR, or MMFSR";
    return false;
}
```

**Step 3: Validate Digits**
```csharp
// Check digit count (must be 1 to requiredDigitLength)
if (digits.Length == 0)
{
    errorMessage = "Part number must include numeric digits after prefix";
    return false;
}

if (digits.Length > requiredDigitLength)
{
    errorMessage = $"Part number has too many digits. Maximum {requiredDigitLength} digits allowed.";
    return false;
}

// Check all characters are digits
if (!digits.All(char.IsDigit))
{
    errorMessage = "Part number digits must be numeric only";
    return false;
}
```

**Step 4: Pad with Leading Zeros**
```csharp
// Pad digits to required length
string paddedDigits = digits.PadLeft(requiredDigitLength, '0');

// Construct standardized part number
standardized = $"{prefix}{paddedDigits}";
errorMessage = "";
return true;
```

**Complete Implementation:**
```csharp
public static class Helper_Validation_PartNumber
{
    public static bool ValidateAndStandardize(
        string input,
        out string standardized,
        out string errorMessage)
    {
        standardized = "";
        errorMessage = "";

        // Step 1: Clean Input
        if (string.IsNullOrWhiteSpace(input))
        {
            errorMessage = "Part number is required";
            return false;
        }

        input = input.Trim().ToUpper();

        // Step 2: Parse Prefix and Determine Required Length
        string prefix;
        string digits;
        int requiredDigitLength;

        if (input.StartsWith("MMCCS") || input.StartsWith("MMFCS") ||
            input.StartsWith("MMCSR") || input.StartsWith("MMFSR"))
        {
            prefix = input.Substring(0, Math.Min(5, input.Length));
            digits = input.Length > 5 ? input.Substring(5) : "";
            requiredDigitLength = 5;
        }
        else if (input.StartsWith("MMC") || input.StartsWith("MMF"))
        {
            prefix = input.Substring(0, Math.Min(3, input.Length));
            digits = input.Length > 3 ? input.Substring(3) : "";
            requiredDigitLength = 7;
        }
        else
        {
            errorMessage = "Invalid part number prefix. Expected: MMC, MMF, MMCCS, MMFCS, MMCSR, or MMFSR";
            return false;
        }

        // Step 3: Validate Digits
        if (digits.Length == 0)
        {
            errorMessage = "Part number must include numeric digits after prefix";
            return false;
        }

        if (digits.Length > requiredDigitLength)
        {
            errorMessage = $"Part number has too many digits. Maximum {requiredDigitLength} digits allowed";
            return false;
        }

        if (!digits.All(char.IsDigit))
        {
            errorMessage = "Part number digits must be numeric only";
            return false;
        }

        // Step 4: Pad with Leading Zeros and Standardize
        string paddedDigits = digits.PadLeft(requiredDigitLength, '0');
        standardized = $"{prefix}{paddedDigits}";

        return true;
    }
}
```

### Edge Case Handling

### Part Not Found in ERP

**Scenario:** User enters part that doesn't exist in Infor Visual

**Handling:**
- Show warning: "Part MMC0001000 not found in Infor Visual."
- Provide options:
  - "Continue with manual entry" → Proceed without ERP data
  - "Re-enter part number" → Clear field and allow correction
  - "Cancel" → Return to previous step

**Implications:**
- No auto-population of receiving location (use "RECV")
- No part description available
- Manual entry of all details required

### Invalid Prefix

**Scenario:** User enters part with unrecognized prefix (e.g., "XYZ0001000")

**Handling:**
- Show error: "Invalid part number prefix. Expected: MMC, MMF, MMCCS, MMFCS, MMCSR, or MMFSR"
- Block proceeding to next step
- Highlight Part Number field in red

### Discontinued Part

**Scenario:** Part exists in ERP but is marked as discontinued

**Handling:**
- Show warning: "Part MMC0001000 is discontinued. Contact Purchasing if you need to receive this material."
- Allow user to proceed or cancel
- Log warning to audit trail

### Mixed Case Entry

**Scenario:** User enters part number in lowercase or mixed case (e.g., "mmc1000")

**Handling:**
- Auto-convert to uppercase on blur: "mmc1000" → "MMC1000"
- Apply zero-padding: "MMC1000" → "MMC0001000"
- Display standardized value in field

**Example:**
```
Input:  mmccs364
Step 1: Convert to uppercase → MMCCS364
Step 2: Apply padding → MMCCS00364
Result: MMCCS00364
Tooltip: "Standardized to MMCCS00364"
```

### Short Format Entry

**Scenario:** User enters part without zero-padding (e.g., "MMC1000")

**Handling:**
- System auto-pads on field blur
- Show tooltip indicating standardization
- Store standardized format in database

**Example:**
```
Input:  MMC1000
Action: Auto-standardize with zero-padding
Output: MMC0001000
Tooltip: "Standardized to MMC0001000"
```

**Example:**
```
Input:  MMFSR511
Action: Auto-standardize with zero-padding
Output: MMFSR00511
Tooltip: "Standardized to MMFSR00511"
```

## Validation Error Examples

**Example 1: No Digits**
```
Input:  MMC
Error:  "Part number must include numeric digits after prefix"
```

**Example 2: Too Many Digits**
```
Input:  MMC12345678 (8 digits, max is 7)
Error:  "Part number has too many digits. Maximum 7 digits allowed"
```

**Example 3: Too Many Digits (Customer-Supplied)**
```
Input:  MMCCS123456 (6 digits, max is 5)
Error:  "Part number has too many digits. Maximum 5 digits allowed"
```

**Example 4: Contains Letters in Digit Portion**
```
Input:  MMC000100A
Error:  "Part number digits must be numeric only"
```

**Example 5: Lowercase with Short Format**
```
Input:  mmc1000
Step 1: Convert to uppercase → MMC1000
Step 2: Apply padding → MMC0001000
Result: Valid ✓
Display: MMC0001000
Tooltip: "Standardized to MMC0001000"
```

**Example 6: Invalid Prefix**
```
Input:  XYZ1000
Error:  "Invalid part number prefix. Expected: MMC, MMF, MMCCS, MMFCS, MMCSR, or MMFSR"
```
```
Input:  mmf0002000
Action: Auto-convert to MMF0002000
Result: Valid
```

**Example 4: Wrong Prefix Length**
```
Input:  MMCS01000 (missing one character in prefix)
Error:  "Invalid format. Expected MMCCS + 5 digits or MMCSR + 5 digits"
```

## Testing Scenarios

### Test 1: Valid Standard Part (Already Padded)

**Input:** `MMC0001000`  
**Expected Standardized:** `MMC0001000`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, no tooltip (already standardized)  
**ERP Lookup:** Success  
**Auto-populate:** Receiving location, Part Type, Description  
**Proceed:** Enabled

### Test 2: Valid Standard Part (Short Format - Auto-Pad)

**Input:** `MMC1000`  
**Expected Standardized:** `MMC0001000`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, tooltip "Standardized to MMC0001000"  
**ERP Lookup:** Success (using standardized value)  
**Auto-populate:** Receiving location, Part Type, Description  
**Proceed:** Enabled

### Test 3: Valid Customer-Supplied Part (Short Format)

**Input:** `MMCCS364`  
**Expected Standardized:** `MMCCS00364`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, tooltip "Standardized to MMCCS00364"  
**ERP Lookup:** May not exist (acceptable)  
**If not found:** Show warning, allow manual entry  
**Proceed:** Enabled

### Test 4: Valid Special Request Part (Short Format)

**Input:** `MMFSR511`  
**Expected Standardized:** `MMFSR00511`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, tooltip "Standardized to MMFSR00511"  
**ERP Lookup:** May not exist (acceptable)  
**Proceed:** Enabled

### Test 5: Lowercase with Short Format

**Input:** `mmc1000`  
**Expected Standardized:** `MMC0001000`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, tooltip "Standardized to MMC0001000"  
**Processing:**
  - Step 1: Convert to uppercase → `MMC1000`
  - Step 2: Apply zero-padding → `MMC0001000`  
**Proceed:** Enabled

### Test 6: Lowercase Customer-Supplied with Short Format

**Input:** `mmccs364`  
**Expected Standardized:** `MMCCS00364`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, tooltip "Standardized to MMCCS00364"  
**Proceed:** Enabled

### Test 7: Invalid - No Digits

**Input:** `MMC`  
**Expected Standardized:** N/A  
**Expected Result:** Invalid ✗  
**Expected Error:** "Part number must include numeric digits after prefix"  
**Expected Display:** Red X icon, error message, red border  
**Proceed:** Disabled

### Test 8: Invalid - Too Many Digits (Standard Part)

**Input:** `MMC12345678` (8 digits, max is 7)  
**Expected Standardized:** N/A  
**Expected Result:** Invalid ✗  
**Expected Error:** "Part number has too many digits. Maximum 7 digits allowed"  
**Expected Display:** Red X icon, error message, red border  
**Proceed:** Disabled

### Test 9: Invalid - Too Many Digits (Customer-Supplied)

**Input:** `MMCCS123456` (6 digits, max is 5)  
**Expected Standardized:** N/A  
**Expected Result:** Invalid ✗  
**Expected Error:** "Part number has too many digits. Maximum 5 digits allowed"  
**Expected Display:** Red X icon, error message, red border  
**Proceed:** Disabled

### Test 10: Invalid - Letter in Digits

**Input:** `MMC1000A`  
**Expected Standardized:** N/A  
**Expected Result:** Invalid ✗  
**Expected Error:** "Part number digits must be numeric only"  
**Expected Display:** Red X icon, error message, red border  
**Proceed:** Disabled

### Test 11: Part Not in ERP (Standardized Format)

**Input:** `MMC9999` (will be standardized to `MMC0009999`)  
**Expected Standardized:** `MMC0009999`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, tooltip "Standardized to MMC0009999"  
**ERP Lookup:** Not found  
**Warning:** "Part not found in Infor Visual. Continue?"  
**Options:** Continue, Re-enter, Cancel

### Test 12: Quality Hold Part (Short Format)

**Input:** `MMC5000` (flagged for Quality Hold)  
**Expected Standardized:** `MMC0005000`  
**Expected Result:** Valid ✓  
**Expected Display:** Green checkmark, tooltip "Standardized to MMC0005000"  
**ERP Lookup:** Success  
**Quality Hold check:** Required  
**Display:** Quality Hold procedures  
**Require:** User acknowledgment  
**Proceed:** Enabled after acknowledgment

### Test 13: Save with Standardized Part Number

**Given:** User enters `MMC1000`  
**When:** System standardizes to `MMC0001000`  
**Expected in Database:** `MMC0001000` (standardized format)  
**Expected in CSV Export:** `MMC0001000` (standardized format)  
**Expected in UI:** `MMC0001000` displayed after save

## Related Documentation

- [Default Part Types](./default-part-types.md) - Part type definitions and assignments
- [Quality Hold](./quality-hold.md) - Quality Hold procedures and requirements
- [Receiving Location Dynamics](./receiving-location-dynamics.md) - Location auto-population
- [Load Composition Rules](./load-composition-rules.md) - Measurement expectations by part type
- [Data Flow](../00-Core/data-flow.md) - Integration with ERP and Settings

# Quantity Validation Rules

**Category**: Business Rules  
**Last Updated**: 2026-01-25  
**Related Documents**: [Guided Mode](../02-Workflow-Modes/001-guided-mode-specification.md), [Manual Entry Mode](../02-Workflow-Modes/002-manual-entry-mode-specification.md)

---

## Rule Definition

Quantity Validation ensures all dunnage load quantities are valid, positive numbers that meet business requirements.

---

## Validation Rules

### Rule 1: Quantity Required

**Definition**: Quantity field cannot be empty or null.

**Validation**:
```
If Quantity is NULL OR empty:
    Error: "Quantity is required"
    Severity: Error (blocks save)
    Action: Highlight field, show error message
```

---

### Rule 2: Quantity Must Be Positive

**Definition**: Quantity must be greater than zero.

**Validation**:
```
If Quantity <= 0:
    Error: "Quantity must be greater than zero"
    Severity: Error (blocks save)
    Action: Highlight field, show error message
```

**Examples**:
- ✅ Valid: 1, 5, 100, 999
- ❌ Invalid: 0, -1, -10

---

### Rule 3: Quantity Must Be Integer

**Definition**: Quantity must be a whole number (no decimals).

**Validation**:
```
If Quantity contains decimal point:
    Error: "Quantity must be a whole number"
    Severity: Error (blocks save)
    Action: Round or reject based on user choice
```

**Examples**:
- ✅ Valid: 10, 25, 100
- ❌ Invalid: 10.5, 25.75, 100.1

**Auto-Correction Option**:
```
If user enters 10.5:
    Prompt: "Round to 11? [Yes] [No]"
    Yes: Set to 11
    No: Clear field, allow re-entry
```

---

### Rule 4: Maximum Quantity (Soft Limit)

**Definition**: Warn if quantity exceeds reasonable threshold (default: 100).

**Validation**:
```
If Quantity > 100:
    Warning: "Large quantity detected. Confirm: [Quantity] loads?"
    Severity: Warning (allows save with confirmation)
    Action: Show confirmation dialog
```

**Rationale**: Prevent accidental data entry errors (e.g., typing 1000 instead of 10).

**Configuration**: Maximum threshold configurable in Advanced Settings (default: 100).

---

### Rule 5: Quantity Format Validation

**Definition**: Quantity must parse as valid integer.

**Validation**:
```
If Quantity cannot parse as integer:
    Error: "Invalid quantity format. Enter a number."
    Severity: Error (blocks save)
```

**Examples**:
- ✅ Valid: "10", "25", "100"
- ❌ Invalid: "ABC", "10.5.5", "N/A", "TBD"

---

## UI Validation Behavior

### Real-Time Validation (Guided Mode)

**Step 3: Quantity Entry**
```
User types in NumberBox:
→ On focus loss (blur):
  → Run validation rules
  → If invalid: Show error tooltip, highlight field red
  → If valid: Remove error, enable "Next" button
```

---

### Grid Validation (Manual Entry Mode)

**Cell Edit:**
```
User edits quantity cell:
→ On cell exit:
  → Run validation rules
  → If invalid: Red border, status icon ✗
  → If valid: Remove border, status icon ✓
```

---

## Error Messages

**Required Field:**
```
❌ Quantity is required.
```

**Non-Positive:**
```
❌ Quantity must be greater than zero.
```

**Non-Integer:**
```
❌ Quantity must be a whole number (no decimals).
Would you like to round [10.5] to [11]? [Yes] [No]
```

**Large Quantity Warning:**
```
⚠ Large quantity detected: 150 loads

Is this correct? This is higher than typical receiving quantities.

[Cancel] [Confirm]
```

**Invalid Format:**
```
❌ Invalid quantity. Please enter a whole number greater than zero.
Example: 10, 25, 100
```

---

## Code Implementation

```csharp
public class Validator_Dunnage_Quantity
{
    private const int MAX_QUANTITY_SOFT_LIMIT = 100;
    
    public ValidationResult Validate(int? quantity)
    {
        // Rule 1: Required
        if (quantity == null)
            return ValidationResult.Error("Quantity is required");
        
        // Rule 2: Must be positive
        if (quantity <= 0)
            return ValidationResult.Error("Quantity must be greater than zero");
        
        // Rule 4: Soft limit warning
        if (quantity > MAX_QUANTITY_SOFT_LIMIT)
            return ValidationResult.Warning($"Large quantity: {quantity} loads. Confirm?");
        
        return ValidationResult.Success();
    }
    
    public ValidationResult ValidateString(string quantityText)
    {
        // Rule 5: Format validation
        if (!int.TryParse(quantityText, out int quantity))
            return ValidationResult.Error("Invalid quantity format");
        
        // Rule 3: Integer only
        if (quantityText.Contains("."))
        {
            int rounded = (int)Math.Round(decimal.Parse(quantityText));
            return ValidationResult.Warning(
                $"Round {quantityText} to {rounded}?", 
                roundedValue: rounded);
        }
        
        return Validate(quantity);
    }
}
```

---

## Related Documentation

- [Guided Mode](../02-Workflow-Modes/001-guided-mode-specification.md) - Step 3: Quantity Entry
- [Manual Entry Mode](../02-Workflow-Modes/002-manual-entry-mode-specification.md) - Grid validation

---

**Document Version:** 1.0  
**Last Updated:** 2026-01-25  
**Status:** Complete

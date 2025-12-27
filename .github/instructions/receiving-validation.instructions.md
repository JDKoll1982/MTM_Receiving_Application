# Receiving Validation Standards

**Category**: Business Logic
**Last Updated**: December 26, 2025
**Applies To**: `IService_ReceivingValidation`, `Service_ReceivingValidation`

## Overview

Validation logic is centralized in `IService_ReceivingValidation` to ensure consistency across the application. This service validates user input, business rules, and data integrity before persistence.

## Validation Types

1.  **Input Validation**: Checking for nulls, empty strings, valid formats (e.g., PO number format).
2.  **Business Rule Validation**: Checking logic (e.g., "Quantity Received cannot exceed Quantity Ordered").
3.  **Duplicate Checks**: Verifying uniqueness where required.

## Implementation Pattern

Methods should return `Model_ValidationResult` (or similar boolean/string tuple).

```csharp
public Model_ValidationResult ValidateReceivingLine(Model_ReceivingLine line)
{
    if (string.IsNullOrWhiteSpace(line.PartID))
        return Model_ValidationResult.Failure("Part ID is required.");

    if (line.Quantity <= 0)
        return Model_ValidationResult.Failure("Quantity must be positive.");

    return Model_ValidationResult.Success();
}
```

## Integration

- **ViewModels**: Call validation methods before enabling "Save" buttons or executing commands.
- **Workflow Services**: Call validation methods in `AdvanceToNextStepAsync`.

## Fail Fast

Validation should occur as early as possible:
1.  **UI**: Input constraints (MaxLength, InputScope).
2.  **ViewModel**: Property setters or command execution.
3.  **Service**: Business logic validation.
4.  **Database**: Constraints (Foreign Keys, Unique Indexes) as a final safety net.

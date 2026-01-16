---
description: Guidelines for working with Models in the MTM Receiving Application
applyTo: 'Models/**/*.cs'
---

# Model Development Guidelines

## Purpose

This file provides guidelines for creating and working with Model classes that represent data entities.

## Core Principles

1. **Models are simple data containers**
2. **No business logic in models**
3. **Models should be serializable**
4. **Use appropriate data types**

## Model Structure

### Basic Model Template

```csharp
namespace MTM_Receiving_Application.Models.FeatureName;

/// <summary>
/// Represents [description of what this model represents]
/// </summary>
public class Model_EntityName
{
    /// <summary>
    /// [Property description]
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// [Property description]
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// [Property description]
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Constructor
    /// </summary>
    public Model_EntityName()
    {
        // Initialize properties with default values
    }
}
```

## Property Guidelines

### Data Types

- **Strings**: Initialize with `string.Empty` (not null)
- **Numbers**: Use appropriate type (int, decimal, double)
- **Dates**: Use `DateTime`, initialize with `DateTime.Now` if applicable
- **Booleans**: Default to `false`
- **Nullable**: Use `?` suffix when value can be null (e.g., `int?`)

### Examples

```csharp
public string PartID { get; set; } = string.Empty;
public int Quantity { get; set; }
public decimal Weight { get; set; }
public DateTime ReceiveDate { get; set; } = DateTime.Now;
public bool IsActive { get; set; } = true;
public int? CoilsOnSkid { get; set; } // Nullable - optional field
```

## Existing Models

The application already has these models:

### Receiving

- `Model_ReceivingLine` - Receiving label data
- `Model_DunnageLine` - Dunnage label data
- `Model_RoutingLabel` - Internal routing label data
- `Model_Dao_Result<T>` - Database operation result wrapper
- `Model_Application_Variables` - Application-wide settings

### Enums

- `Enum_ErrorSeverity` - Error severity levels (Low, Medium, High, Critical)
- `Enum_LabelType` - Label type classifications

## DAO Result Pattern

All database operations return `Model_Dao_Result<T>`:

```csharp
public class Model_Dao_Result<T>
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public T? Data { get; set; }
}
```

### Usage in DAO

```csharp
public static async Task<Model_Dao_Result<Model_ReceivingLine>> InsertReceivingLineAsync(Model_ReceivingLine line)
{
    try
    {
        // Database operation
        return new Model_Dao_Result<Model_ReceivingLine>
        {
            IsSuccess = true,
            Data = line
        };
    }
    catch (Exception ex)
    {
        return new Model_Dao_Result<Model_ReceivingLine>
        {
            IsSuccess = false,
            ErrorMessage = ex.Message
        };
    }
}
```

### Usage in ViewModel

```csharp
var result = await Dao_ReceivingLine.InsertReceivingLineAsync(line);

if (result.IsSuccess)
{
    // Success handling
    var savedLine = result.Data;
}
else
{
    // Error handling
    _errorHandler.ShowUserError(result.ErrorMessage, "Error", nameof(Method));
}
```

## Naming Conventions

### Files

- Format: `Model_[EntityName].cs`
- Examples: `Model_ReceivingLine.cs`, `Model_DunnageLine.cs`

### Classes

- Format: `Model_[EntityName]`
- Pascal case for multi-word names
- Examples: `Model_ReceivingLine`, `Model_RoutingLabel`

### Properties

- Pascal case: `PartID`, `EmployeeNumber`, `InitialLocation`
- Be descriptive but concise

## Documentation

Always include XML documentation:

```csharp
/// <summary>
/// Represents a receiving label line item
/// </summary>
public class Model_ReceivingLine
{
    /// <summary>
    /// Gets or sets the material/part identifier from Infor Visual
    /// </summary>
    public string PartID { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of items received
    /// </summary>
    public int Quantity { get; set; }
}
```

## Validation

Models should be simple POCOs. Put validation in:

- **ViewModel**: For UI validation
- **DAO**: For database constraints
- **Service**: For business rule validation

## File Locations

- Domain models: `Models/[FeatureName]/Model_[EntityName].cs`
- Shared models: `Models/Shared/`
- Enums: `Models/Enums/`
- Example: `Models/Receiving/Model_ReceivingLine.cs`

## Common Patterns

### Database Entity

```csharp
public class Model_ReceivingLine
{
    public int ID { get; set; }
    public string PartID { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int EmployeeNumber { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
```

### Lookup/Reference Data

```csharp
public class Model_Location
{
    public string LocationCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
```

### Configuration/Settings

```csharp
public class Model_Application_Variables
{
    public string DatabaseConnectionString { get; set; } = string.Empty;
    public int DefaultEmployeeNumber { get; set; }
    public bool EnableLogging { get; set; } = true;
}
```

## Things to Avoid

❌ Don't add business logic to models
❌ Don't reference ViewModels or Views
❌ Don't add database access code
❌ Don't make properties with complex logic in getters/setters
❌ Don't use null for strings (use `string.Empty`)
❌ Don't forget XML documentation
❌ Don't create models with no properties

## Testing Models

Models should be simple enough that they don't need extensive unit tests, but verify:

- Default values are correct
- Properties can be set and retrieved
- Constructors initialize properly

## Serialization

Keep models serialization-friendly:

- Use simple types
- Avoid circular references
- Use public properties (not fields)
- Add `[JsonPropertyName]` if needed for API compatibility

```csharp
using System.Text.Json.Serialization;

public class Model_ApiData
{
    [JsonPropertyName("part_id")]
    public string PartID { get; set; } = string.Empty;
}
```

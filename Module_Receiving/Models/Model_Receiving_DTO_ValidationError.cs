using MTM_Receiving_Application.Module_Receiving.Models.Enums;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Represents a validation error in the receiving workflow.
/// Can be session-level (LoadNumber = null) or load-specific (LoadNumber set).
/// </summary>
public class Model_Receiving_DTO_ValidationError
{
    /// <summary>
    /// Name of the field that failed validation (e.g., "PONumber", "WeightOrQuantity").
    /// </summary>
    public string FieldName { get; init; } = string.Empty;
    
    /// <summary>
    /// Load number if this is a load-specific error, or null for session-level errors.
    /// </summary>
    public int? LoadNumber { get; init; }
    
    /// <summary>
    /// Human-readable error message describing the validation failure.
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;
    
    /// <summary>
    /// Severity of the validation error (Info, Warning, Error).
    /// </summary>
    public Enum_Shared_Severity_ErrorSeverity Severity { get; init; }
}

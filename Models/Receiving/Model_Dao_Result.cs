using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.Models.Receiving;

/// <summary>
/// Standardized response object for all database operations
/// Provides success/failure status, error messages, and performance metrics
/// </summary>
public class Model_Dao_Result
{
    /// <summary>
    /// Indicates whether the database operation succeeded
    /// </summary>
    public bool Success { get; set; } = false;

    /// <summary>
    /// Error message if operation failed, empty string if successful
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Severity level of any error that occurred
    /// </summary>
    public Enum_ErrorSeverity Severity { get; set; } = Enum_ErrorSeverity.Info;

    /// <summary>
    /// Number of rows affected by the database operation
    /// </summary>
    public int AffectedRows { get; set; } = 0;

    /// <summary>
    /// Execution time in milliseconds for performance monitoring
    /// </summary>
    public long ExecutionTimeMs { get; set; } = 0;

    /// <summary>
    /// Optional return value from the database operation (e.g., generated ID)
    /// </summary>
    public object? ReturnValue { get; set; } = null;
}

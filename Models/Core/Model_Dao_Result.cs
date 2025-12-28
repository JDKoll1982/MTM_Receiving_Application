using MTM_Receiving_Application.Models.Enums;
using System;

namespace MTM_Receiving_Application.Models.Core;

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
    /// Alias for Success to match some usage patterns
    /// </summary>
    public bool IsSuccess => Success;

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

    /// <summary>
    /// Exception that caused the failure, if any
    /// </summary>
    public Exception? Exception { get; set; }

    // Static factory methods moved to DaoResultFactory to avoid circular dependencies
}


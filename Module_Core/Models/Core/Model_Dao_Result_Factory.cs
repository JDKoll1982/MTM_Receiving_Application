using System;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Core.Models.Core;

/// <summary>
/// Factory for creating Model_Dao_Result instances to avoid self-referencing cycles in model classes.
/// </summary>
public static class Model_Dao_Result_Factory
{
    public static Model_Dao_Result Failure(string? message, Exception? ex = null)
    {
        return new Model_Dao_Result
        {
            Success = false,
            ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Operation failed." : message,
            Exception = ex,
            Severity = Enum_ErrorSeverity.Error
        };
    }

    public static Model_Dao_Result Success(int affectedRows = 0)
    {
        return new Model_Dao_Result
        {
            Success = true,
            AffectedRows = Math.Max(0, affectedRows),
            ErrorMessage = string.Empty
        };
    }

    public static Model_Dao_Result<T> Failure<T>(string? message, Exception? ex = null)
    {
        return new Model_Dao_Result<T>
        {
            Success = false,
            ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Operation failed." : message,
            Exception = ex,
            Severity = Enum_ErrorSeverity.Error
        };
    }

    public static Model_Dao_Result<T> Success<T>(T data, int affectedRows = 0)
    {
        return new Model_Dao_Result<T>
        {
            Success = true,
            Data = data,
            AffectedRows = Math.Max(0, affectedRows),
            ErrorMessage = string.Empty
        };
    }
}


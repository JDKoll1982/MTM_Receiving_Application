using System;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Models.Receiving;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.Helpers.Database;

/// <summary>
/// Helper class for executing stored procedures with retry logic, 
/// performance monitoring, and standardized error handling
/// </summary>
public static class Helper_Database_StoredProcedure
{
    private const int MaxRetries = 3;
    private static readonly int[] RetryDelaysMs = { 100, 200, 400 };

    /// <summary>
    /// Executes a stored procedure and returns a standardized result
    /// </summary>
    /// <param name="procedureName">Name of the stored procedure</param>
    /// <param name="parameters">MySqlParameter array for the procedure</param>
    /// <param name="connectionString">Database connection string</param>
    /// <returns>Model_Dao_Result with success status and performance metrics</returns>
    public static async Task<Model_Dao_Result> ExecuteAsync(
        string procedureName,
        MySqlParameter[] parameters,
        string connectionString)
    {
        var result = new Model_Dao_Result();
        var stopwatch = Stopwatch.StartNew();
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Add parameters
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                // Execute the stored procedure
                var affectedRows = await command.ExecuteNonQueryAsync();

                // Extract OUT parameters
                var statusParam = command.Parameters["@p_Status"];
                var errorMsgParam = command.Parameters["@p_ErrorMsg"];

                if (statusParam != null && errorMsgParam != null)
                {
                    int status = Convert.ToInt32(statusParam.Value);
                    string errorMsg = errorMsgParam.Value?.ToString() ?? string.Empty;

                    result.Success = status == 0;
                    result.ErrorMessage = errorMsg;
                    result.Severity = status == 0 ? Enum_ErrorSeverity.Info : Enum_ErrorSeverity.Error;
                    result.AffectedRows = status == 0 ? affectedRows : 0;
                }
                else
                {
                    // No OUT parameters, assume success if no exception
                    result.Success = true;
                    result.AffectedRows = affectedRows;
                }

                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

                return result; // Success, exit retry loop
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                // Transient error, wait and retry
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue; // Retry
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.ErrorMessage = $"Stored procedure '{procedureName}' failed: {ex.Message}";
                result.Severity = Enum_ErrorSeverity.Error;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
        }

        // All retries exhausted
        stopwatch.Stop();
        result.Success = false;
        result.ErrorMessage = $"Stored procedure '{procedureName}' failed after {MaxRetries} attempts";
        result.Severity = Enum_ErrorSeverity.Critical;
        result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
        return result;
    }

    /// <summary>
    /// Determines if a MySQL exception is transient (temporary) and should be retried
    /// </summary>
    private static bool IsTransientError(MySqlException ex)
    {
        // Common transient error codes:
        // 1205 = Lock wait timeout
        // 1213 = Deadlock
        // 2006 = Server has gone away
        // 2013 = Lost connection during query
        return ex.Number == 1205 || ex.Number == 1213 || 
               ex.Number == 2006 || ex.Number == 2013;
    }

    /// <summary>
    /// Validates that required parameters are not null or empty
    /// </summary>
    /// <param name="parameters">Parameters to validate</param>
    /// <returns>True if all parameters are valid</returns>
    public static bool ValidateParameters(MySqlParameter[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
        {
            return true; // No parameters to validate
        }

        foreach (var param in parameters)
        {
            if (param.Direction == ParameterDirection.Input || 
                param.Direction == ParameterDirection.InputOutput)
            {
                if (param.Value == null || param.Value == DBNull.Value)
                {
                    return false;
                }

                if (param.Value is string strValue && string.IsNullOrWhiteSpace(strValue))
                {
                    return false;
                }
            }
        }

        return true;
    }
}

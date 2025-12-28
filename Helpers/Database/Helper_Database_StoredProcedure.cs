using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Models.Core;
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
    /// Executes a stored procedure using MySqlParameter array (compatible with legacy/receiving DAOs)
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <param name="connectionString"></param>
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
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                var affectedRows = await command.ExecuteNonQueryAsync();

                // Handle output parameters if any
                foreach (var param in command.Parameters)
                {
                    if (param is MySqlParameter p && (p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.InputOutput))
                    {
                        // You might want to capture output values here if needed
                        // For now, we just ensure the command executes
                    }
                }

                stopwatch.Stop();
                result.Success = true;
                result.AffectedRows = affectedRows;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return Model_Dao_Result_Factory.Failure($"Stored procedure '{procedureName}' failed: {ex.Message}", ex);
            }
        }

        stopwatch.Stop();
        return Model_Dao_Result_Factory.Failure($"Stored procedure '{procedureName}' failed after {MaxRetries} attempts");
    }

    /// <summary>
    /// Validates that all required parameters are present and not null
    /// </summary>
    /// <param name="parameters"></param>
    public static bool ValidateParameters(MySqlParameter[] parameters)
    {
        if (parameters == null)
        {
            return true;
        }

        foreach (var param in parameters)
        {
            if (param.Value == null && param.Direction == ParameterDirection.Input)
            {
                // Allow DBNull.Value but not null
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Executes a stored procedure that returns no data (INSERT, UPDATE, DELETE)
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    public static async Task<Model_Dao_Result> ExecuteNonQueryAsync(
        string connectionString,
        string procedureName,
        Dictionary<string, object>? parameters = null)
    {
        var result = new Model_Dao_Result();
        var stopwatch = Stopwatch.StartNew();
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                AddParameters(command, parameters);

                var affectedRows = await command.ExecuteNonQueryAsync();

                stopwatch.Stop();
                result.Success = true;
                result.AffectedRows = affectedRows;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.ErrorMessage = $"Stored procedure '{procedureName}' failed: {ex.Message}";
                result.Severity = Enum_ErrorSeverity.Error;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                result.Exception = ex;
                return result;
            }
        }

        stopwatch.Stop();
        result.Success = false;
        result.ErrorMessage = $"Stored procedure '{procedureName}' failed after {MaxRetries} attempts";
        result.Severity = Enum_ErrorSeverity.Critical;
        result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
        return result;
    }

    /// <summary>
    /// Executes a stored procedure that returns a single record mapped to T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connectionString"></param>
    /// <param name="procedureName"></param>
    /// <param name="mapper"></param>
    /// <param name="parameters"></param>
    public static async Task<Model_Dao_Result<T>> ExecuteSingleAsync<T>(
        string connectionString,
        string procedureName,
        Func<IDataReader, T> mapper,
        Dictionary<string, object>? parameters = null)
    {
        var result = new Model_Dao_Result<T>();
        var stopwatch = Stopwatch.StartNew();
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                AddParameters(command, parameters);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    result.Data = mapper(reader);
                    result.Success = true;
                    result.AffectedRows = 1;
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = "No record found";
                    result.Severity = Enum_ErrorSeverity.Info;
                }

                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return Model_Dao_Result_Factory.Failure<T>($"Stored procedure '{procedureName}' failed: {ex.Message}", ex);
            }
        }

        stopwatch.Stop();
        return Model_Dao_Result_Factory.Failure<T>($"Stored procedure '{procedureName}' failed after {MaxRetries} attempts");
    }

    /// <summary>
    /// Executes a stored procedure that returns a list of records mapped to T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="connectionString"></param>
    /// <param name="procedureName"></param>
    /// <param name="mapper"></param>
    /// <param name="parameters"></param>
    public static async Task<Model_Dao_Result<List<T>>> ExecuteListAsync<T>(
        string connectionString,
        string procedureName,
        Func<IDataReader, T> mapper,
        Dictionary<string, object>? parameters = null)
    {
        var result = new Model_Dao_Result<List<T>>();
        var stopwatch = Stopwatch.StartNew();
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                AddParameters(command, parameters);

                await using var reader = await command.ExecuteReaderAsync();
                var list = new List<T>();

                while (await reader.ReadAsync())
                {
                    list.Add(mapper(reader));
                }

                result.Data = list;
                result.Success = true;
                result.AffectedRows = list.Count;

                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return Model_Dao_Result_Factory.Failure<List<T>>($"Stored procedure '{procedureName}' failed: {ex.Message}", ex);
            }
        }

        stopwatch.Stop();
        return Model_Dao_Result_Factory.Failure<List<T>>($"Stored procedure '{procedureName}' failed after {MaxRetries} attempts");
    }

    /// <summary>
    /// Executes a stored procedure that returns a DataTable (SELECT)
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    public static async Task<Model_Dao_Result<DataTable>> ExecuteDataTableAsync(
        string connectionString,
        string procedureName,
        Dictionary<string, object>? parameters = null)
    {
        var result = new Model_Dao_Result<DataTable>();
        var stopwatch = Stopwatch.StartNew();
        int attempt = 0;

        while (attempt < MaxRetries)
        {
            attempt++;

            try
            {
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using var command = new MySqlCommand(procedureName, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                AddParameters(command, parameters);

                await using var reader = await command.ExecuteReaderAsync();
                var dataTable = new DataTable();
                dataTable.Load(reader);

                stopwatch.Stop();
                result.Success = true;
                result.Data = dataTable;
                result.AffectedRows = dataTable.Rows.Count;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                return result;
            }
            catch (MySqlException ex) when (IsTransientError(ex) && attempt < MaxRetries)
            {
                await Task.Delay(RetryDelaysMs[attempt - 1]);
                continue;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.ErrorMessage = $"Stored procedure '{procedureName}' failed: {ex.Message}";
                result.Severity = Enum_ErrorSeverity.Error;
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                result.Exception = ex;
                return result;
            }
        }

        stopwatch.Stop();
        result.Success = false;
        result.ErrorMessage = $"Stored procedure '{procedureName}' failed after {MaxRetries} attempts";
        result.Severity = Enum_ErrorSeverity.Critical;
        result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
        return result;
    }

    /// <summary>
    /// Executes a stored procedure within an existing transaction
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    public static async Task<Model_Dao_Result> ExecuteInTransactionAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string procedureName,
        Dictionary<string, object>? parameters = null)
    {
        var result = new Model_Dao_Result();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await using var command = new MySqlCommand(procedureName, connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };

            AddParameters(command, parameters);

            var affectedRows = await command.ExecuteNonQueryAsync();

            stopwatch.Stop();
            result.Success = true;
            result.AffectedRows = affectedRows;
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ErrorMessage = $"Stored procedure '{procedureName}' failed in transaction: {ex.Message}";
            result.Severity = Enum_ErrorSeverity.Error;
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.Exception = ex;
            return result;
        }
    }

    private static void AddParameters(MySqlCommand command, Dictionary<string, object>? parameters)
    {
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                // Automatically add 'p_' prefix if missing, as per convention
                string paramName = param.Key.StartsWith("p_") ? "@" + param.Key : "@p_" + param.Key;
                // Or just use the key if the caller is expected to provide the correct name
                // The constitution says: "Parameter names in C# match stored procedure parameters (WITHOUT p_ prefix - added automatically)"
                // So we should add p_ prefix.

                // However, existing code might be using @p_ already.
                // Let's be safe: if it starts with @, use it. If not, check if it starts with p_.

                string cleanName = param.Key.TrimStart('@');
                string finalName = cleanName.StartsWith("p_") ? "@" + cleanName : "@p_" + cleanName;

                command.Parameters.AddWithValue(finalName, param.Value ?? DBNull.Value);
            }
        }
    }

    /// <summary>
    /// Determines if a MySQL exception is transient (temporary) and should be retried
    /// </summary>
    /// <param name="ex"></param>
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
}



using System.Data;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedures;

public abstract class StoredProcedureIntegrationTestBase
{
    protected static string? TryGetIntegrationConnectionString()
    {
        return Environment.GetEnvironmentVariable("MTM_TEST_MYSQL_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("MTM_MYSQL_CONNECTION_STRING");
    }

    protected static string CreateUniqueSuffix()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Adds parameters after guarding against the enum-overload trap. A literal <c>0</c> converts
    /// implicitly to any enum, so <c>new MySqlParameter("p_flag", 0)</c> binds the
    /// <c>MySqlDbType</c> overload and leaves <c>Value</c> null, which sends NULL to the procedure.
    /// </summary>
    private static void AddParameters(MySqlCommand command, MySqlParameter[] parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter.Direction == ParameterDirection.Input
                && parameter.Value is null
                && parameter.MySqlDbType == default(MySqlDbType))
            {
                throw new InvalidOperationException(
                    $"Parameter '{parameter.ParameterName}' has no value. Cast the literal to object "
                        + "when 0 is intended: new MySqlParameter(\"name\", (object)0)."
                );
            }

            command.Parameters.Add(parameter);
        }
    }

    protected static async Task<DataTable> ExecuteStoredProcedureQueryAsync(
        string connectionString,
        string procedureName,
        params MySqlParameter[] parameters
    )
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
        };

        if (parameters.Length > 0)
        {
            AddParameters(command, parameters);
        }

        await using var reader = await command.ExecuteReaderAsync();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    protected static async Task<StoredProcedureNonQueryResult> ExecuteStoredProcedureNonQueryAsync(
        string connectionString,
        string procedureName,
        params MySqlParameter[] parameters
    )
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
        };

        if (parameters.Length > 0)
        {
            AddParameters(command, parameters);
        }

        var affectedRows = await command.ExecuteNonQueryAsync();
        var outputValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (MySqlParameter parameter in command.Parameters)
        {
            if (
                parameter.Direction == ParameterDirection.Output
                || parameter.Direction == ParameterDirection.InputOutput
                || parameter.Direction == ParameterDirection.ReturnValue
            )
            {
                outputValues[parameter.ParameterName] =
                    parameter.Value == DBNull.Value ? null : parameter.Value;
            }
        }

        return new StoredProcedureNonQueryResult(affectedRows, outputValues);
    }

    protected static async Task<int> ExecuteSqlNonQueryAsync(
        string connectionString,
        string sql,
        params MySqlParameter[] parameters
    )
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        if (parameters.Length > 0)
        {
            AddParameters(command, parameters);
        }

        return await command.ExecuteNonQueryAsync();
    }

    protected static async Task<object?> ExecuteSqlScalarAsync(
        string connectionString,
        string sql,
        params MySqlParameter[] parameters
    )
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        if (parameters.Length > 0)
        {
            AddParameters(command, parameters);
        }

        var value = await command.ExecuteScalarAsync();
        return value == DBNull.Value ? null : value;
    }

    protected static int ConvertToInt(object? value)
    {
        return value switch
        {
            null => 0,
            int intValue => intValue,
            long longValue => Convert.ToInt32(longValue),
            decimal decimalValue => Convert.ToInt32(decimalValue),
            _ => Convert.ToInt32(value),
        };
    }
}

public sealed record StoredProcedureNonQueryResult(
    int AffectedRows,
    IReadOnlyDictionary<string, object?> OutputValues
);

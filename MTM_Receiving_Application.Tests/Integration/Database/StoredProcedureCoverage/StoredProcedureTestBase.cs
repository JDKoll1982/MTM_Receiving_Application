using System.Data;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Helpers for tests that drive the procedures deployed in the shared scratch schema.
/// </summary>
/// <remarks>
/// Rows created by these tests carry a <c>TEST-</c> prefix so a leftover row is obviously not real
/// data, and every class removes what it inserted.
/// </remarks>
public abstract class StoredProcedureTestBase
{
    protected StoredProcedureTestBase(StoredProcedureTestDatabase database)
    {
        Database = database;
    }

    protected StoredProcedureTestDatabase Database { get; }

    protected string ConnectionString => Database.ConnectionString!;

    /// <summary>
    /// Marks the test skipped when no MySQL server could be reached.
    /// </summary>
    protected void SkipWhenUnavailable()
    {
        Skip.If(
            Database.ConnectionString is null,
            Database.SkipReason ?? "No MySQL server is available for the stored procedure tests.");
    }

    protected static string UniqueToken(string prefix) => prefix + Guid.NewGuid().ToString("N")[..10];

    protected async Task<DataTable> QueryAsync(
        string procedureName,
        params MySqlParameter[] parameters)
    {
        await using var connection = await OpenAsync();

        await using var command = new MySqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 60,
        };
        command.Parameters.AddRange(parameters);

        await using var reader = await command.ExecuteReaderAsync();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    protected async Task<DataTable> QuerySqlAsync(string sql, params MySqlParameter[] parameters)
    {
        await using var connection = await OpenAsync();

        await using var command = new MySqlCommand(sql, connection) { CommandTimeout = 60 };
        command.Parameters.AddRange(parameters);

        await using var reader = await command.ExecuteReaderAsync();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    protected async Task<int> ExecuteProcedureAsync(
        string procedureName,
        params MySqlParameter[] parameters)
    {
        await using var connection = await OpenAsync();

        await using var command = new MySqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 60,
        };
        command.Parameters.AddRange(parameters);

        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Runs a procedure that reports through output parameters.
    /// </summary>
    protected async Task<Dictionary<string, object?>> ExecuteProcedureWithOutputsAsync(
        string procedureName,
        params MySqlParameter[] parameters)
    {
        await using var connection = await OpenAsync();

        await using var command = new MySqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 60,
        };
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync();

        var outputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (MySqlParameter parameter in command.Parameters)
        {
            if (parameter.Direction is ParameterDirection.Output
                or ParameterDirection.InputOutput
                or ParameterDirection.ReturnValue)
            {
                outputs[parameter.ParameterName] = parameter.Value == DBNull.Value
                    ? null
                    : parameter.Value;
            }
        }

        return outputs;
    }

    /// <summary>
    /// Calls a procedure with a hand-built <c>CALL</c>. Needed where MySql.Data cannot resolve the
    /// parameter metadata itself, which it fails to do for a <c>JSON</c> parameter on this server
    /// version even though the call is valid.
    /// </summary>
    protected async Task<int> ExecuteProcedureByPositionAsync(
        string procedureName,
        params MySqlParameter[] parameters)
    {
        await using var connection = await OpenAsync();

        var placeholders = string.Join(
            ", ",
            parameters.Select(parameter => parameter.ParameterName.StartsWith('@')
                ? parameter.ParameterName
                : "@" + parameter.ParameterName));
        await using var command = new MySqlCommand(
            $"CALL `{procedureName}`({placeholders});",
            connection)
        {
            CommandTimeout = 60,
        };
        command.Parameters.AddRange(parameters);

        return await command.ExecuteNonQueryAsync();
    }

    protected async Task<int> ExecuteSqlAsync(string sql, params MySqlParameter[] parameters)
    {
        await using var connection = await OpenAsync();

        await using var command = new MySqlCommand(sql, connection) { CommandTimeout = 60 };
        command.Parameters.AddRange(parameters);
        return await command.ExecuteNonQueryAsync();
    }

    protected async Task<object?> ExecuteScalarAsync(string sql, params MySqlParameter[] parameters)
    {
        await using var connection = await OpenAsync();

        await using var command = new MySqlCommand(sql, connection) { CommandTimeout = 60 };
        command.Parameters.AddRange(parameters);
        var value = await command.ExecuteScalarAsync();
        return value == DBNull.Value ? null : value;
    }

    protected static int ToInt(object? value)
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

    /// <summary>
    /// Reads a scalar as text without a null dereference, so an empty result fails the assertion
    /// instead of throwing.
    /// </summary>
    protected static string ToText(object? value) => value?.ToString() ?? string.Empty;

    protected static MySqlParameter Parameter(string name, object? value) =>
        new(name, value ?? DBNull.Value);

    /// <summary>
    /// Opens a connection whose connection collation matches the schema, because a procedure that
    /// filters with <c>LIKE</c> compares its parameter against a <c>utf8mb4_unicode_ci</c> column and
    /// MySQL refuses to mix the two collations.
    /// </summary>
    private async Task<MySqlConnection> OpenAsync()
    {
        var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(
            "SET collation_connection = 'utf8mb4_unicode_ci';",
            connection);
        await command.ExecuteNonQueryAsync();

        return connection;
    }
}

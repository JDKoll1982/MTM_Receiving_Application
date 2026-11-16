using System.Data;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Calls a stored procedure with sample arguments derived from its own parameter metadata.
/// </summary>
/// <remarks>
/// The goal is to prove the procedure compiles and runs against the deployed schema, not to reach a
/// business outcome. A procedure is allowed to reject the sample input with its own
/// <c>SIGNAL</c>, a duplicate-key error, a foreign-key error, or anything else it raises on purpose;
/// only errors that mean the SQL itself no longer matches the schema are reported as failures.
/// </remarks>
internal static class StoredProcedureInvoker
{
    /// <summary>
    /// Error numbers that mean the procedure body no longer matches the database: an unknown column
    /// or table, a syntax error, a missing routine, or an ambiguous reference.
    /// </summary>
    private static readonly HashSet<int> BrokenSqlErrors =
    [
        1051, // Unknown table
        1052, // Ambiguous column
        1054, // Unknown column
        1064, // Syntax error
        1109, // Unknown table in field list
        1146, // Table does not exist
        1305, // Procedure does not exist
        1630, // Function does not exist
    ];

    public static async Task<StoredProcedureInvocationResult> InvokeAsync(
        string connectionString,
        string procedureName)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 60,
        };

        try
        {
            MySqlCommandBuilder.DeriveParameters(command);
        }
        catch (MySqlException exception)
        {
            return StoredProcedureInvocationResult.FromFailure(
                exception.Number,
                exception.Message,
                BrokenSqlErrors.Contains(exception.Number));
        }

        foreach (MySqlParameter parameter in command.Parameters)
        {
            if (parameter.Direction is ParameterDirection.Input or ParameterDirection.InputOutput)
            {
                parameter.Value = SampleValue(parameter.MySqlDbType);
            }
        }

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            do
            {
                while (await reader.ReadAsync())
                {
                    // Every result set is drained so the procedure runs to completion.
                }
            }
            while (await reader.NextResultAsync());

            return StoredProcedureInvocationResult.FromSuccess(command.Parameters);
        }
        catch (MySqlException exception)
        {
            return StoredProcedureInvocationResult.FromFailure(
                exception.Number,
                exception.Message,
                BrokenSqlErrors.Contains(exception.Number));
        }
    }

    private static object SampleValue(MySqlDbType type) => type switch
    {
        MySqlDbType.Byte => (byte)1,
        MySqlDbType.UByte => (byte)1,
        MySqlDbType.Int16 or MySqlDbType.UInt16 => (short)1,
        MySqlDbType.Int24 or MySqlDbType.Int32 or MySqlDbType.UInt24
            or MySqlDbType.UInt32 => 1,
        MySqlDbType.Int64 or MySqlDbType.UInt64 => 1L,
        MySqlDbType.Decimal or MySqlDbType.NewDecimal => 1m,
        MySqlDbType.Float => 1f,
        MySqlDbType.Double => 1d,
        MySqlDbType.Date or MySqlDbType.DateTime
            or MySqlDbType.Timestamp => new DateTime(2020, 1, 1),
        MySqlDbType.Time => new TimeSpan(8, 0, 0),
        MySqlDbType.Year => 2020,
        MySqlDbType.Bit => 0UL,
        MySqlDbType.Blob or MySqlDbType.TinyBlob or MySqlDbType.MediumBlob
            or MySqlDbType.LongBlob or MySqlDbType.Binary or MySqlDbType.VarBinary
            or MySqlDbType.Geometry => Array.Empty<byte>(),
        _ => "TEST",
    };
}

/// <summary>
/// What happened when a stored procedure was called with sample arguments.
/// </summary>
internal sealed record StoredProcedureInvocationResult(
    bool Succeeded,
    int? ErrorNumber,
    string? ErrorMessage,
    bool LooksLikeBrokenSql,
    IReadOnlyDictionary<string, object?> OutputValues)
{
    public static StoredProcedureInvocationResult FromSuccess(
        MySqlParameterCollection parameters)
    {
        var outputs = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (MySqlParameter parameter in parameters)
        {
            if (parameter.Direction is ParameterDirection.Output
                or ParameterDirection.InputOutput
                or ParameterDirection.ReturnValue)
            {
                outputs[parameter.ParameterName] = parameter.Value;
            }
        }

        return new StoredProcedureInvocationResult(true, null, null, false, outputs);
    }

    public static StoredProcedureInvocationResult FromFailure(
        int errorNumber,
        string errorMessage,
        bool looksLikeBrokenSql) =>
        new(false, errorNumber, errorMessage, looksLikeBrokenSql,
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase));
}

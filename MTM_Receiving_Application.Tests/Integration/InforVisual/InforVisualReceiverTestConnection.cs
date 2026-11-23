using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Tests.Integration.InforVisual;

/// <summary>
/// One read of the recent receiver window, shared by the User Story 3 checks.
/// </summary>
/// <remarks>
/// The source is Infor Visual, which this feature must never write to, so the harness only ever
/// issues SELECT statements and forces <c>ApplicationIntent=ReadOnly</c> onto whatever connection
/// string it is given. When <c>MTM_INFORVISUAL_TEST_CONNECTION</c> is unset the harness reports that
/// it is not configured, and each test skips instead of pretending to pass.
/// </remarks>
public sealed class InforVisualReceiverTestConnection : IAsyncLifetime
{
    /// <summary>The environment variable that points the checks at a real MTMFG database.</summary>
    public const string ConnectionVariableName = "MTM_INFORVISUAL_TEST_CONNECTION";

    /// <summary>The window the list itself reads, so the checks inspect the same rows.</summary>
    public const int WindowSize = 1000;

    private string _connectionString = string.Empty;

    /// <summary>Gets a value indicating whether a read-only connection was configured and read.</summary>
    public bool IsConfigured { get; private set; }

    /// <summary>Gets the reason a caller should skip when nothing is configured.</summary>
    public string SkipReason =>
        $"{ConnectionVariableName} is not set, so the read-only Infor Visual receiver checks are skipped.";

    /// <summary>Gets the DAO under test, or null when nothing is configured.</summary>
    public Dao_InforVisualReceiver? Dao { get; private set; }

    /// <summary>Gets the read window, newest first.</summary>
    public IReadOnlyList<Model_InforVisualReceiverRow> Rows { get; private set; } =
        Array.Empty<Model_InforVisualReceiverRow>();

    public async Task InitializeAsync()
    {
        var configured = Environment.GetEnvironmentVariable(ConnectionVariableName);
        if (string.IsNullOrWhiteSpace(configured))
        {
            return;
        }

        // Whatever the caller supplies, the connection can only read.
        _connectionString = new SqlConnectionStringBuilder(configured)
        {
            ApplicationIntent = ApplicationIntent.ReadOnly,
        }.ConnectionString;

        Dao = new Dao_InforVisualReceiver(_connectionString);

        var result = await Dao.GetRecentReceiversAsync(WindowSize);
        if (!result.IsSuccess || result.Data is null)
        {
            throw new InvalidOperationException(
                "The recent receiver query failed against the configured Infor Visual connection: "
                    + result.ErrorMessage
            );
        }

        Rows = result.Data;
        IsConfigured = true;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>
    /// Recounts, straight from the source, every receipt quantity the receiver recorded for a part.
    /// This is the read-only cross-check for the summed quantity the list shows.
    /// </summary>
    /// <param name="receiverNumber">The receiver number the row came from.</param>
    /// <param name="partNumber">The part number the row came from.</param>
    /// <returns>The summed received quantity across that receiver's lines for the part.</returns>
    public async Task<decimal> GetReceivedQuantityForPartAsync(
        string receiverNumber,
        string partNumber
    )
    {
        const string sql = """
            SELECT ISNULL(SUM(rl.RECEIVED_QTY), 0)
            FROM dbo.RECEIVER r
            INNER JOIN dbo.RECEIVER_LINE rl ON rl.RECEIVER_ID = r.ID
            INNER JOIN dbo.PURC_ORDER_LINE pol
                ON rl.PURC_ORDER_ID = pol.PURC_ORDER_ID
                AND rl.PURC_ORDER_LINE_NO = pol.LINE_NO
            WHERE CAST(r.ID AS nvarchar(30)) = @ReceiverNumber
              AND pol.PART_ID = @PartNumber;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ReceiverNumber", receiverNumber);
        command.Parameters.AddWithValue("@PartNumber", partNumber);

        var scalar = await command.ExecuteScalarAsync();
        return scalar is null or DBNull
            ? 0m
            : Convert.ToDecimal(scalar, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Returns the receivers among <paramref name="receiverNumbers"/> that the source marks for
    /// removal or purge. The list must never show one.
    /// </summary>
    /// <param name="receiverNumbers">The receiver numbers on screen.</param>
    /// <returns>The subset the source has marked.</returns>
    public async Task<IReadOnlyList<string>> GetMarkedForPurgeReceiverNumbersAsync(
        IEnumerable<string> receiverNumbers
    )
    {
        ArgumentNullException.ThrowIfNull(receiverNumbers);

        var candidates = receiverNumbers
            .Where(number => string.IsNullOrWhiteSpace(number) is false)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (candidates.Count == 0)
        {
            return Array.Empty<string>();
        }

        var parameterNames = new List<string>(candidates.Count);
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand { Connection = connection };

        for (var index = 0; index < candidates.Count; index++)
        {
            var parameterName = $"@Receiver{index}";
            parameterNames.Add(parameterName);
            command.Parameters.AddWithValue(parameterName, candidates[index]);
        }

        command.CommandText =
            "SELECT CAST(ID AS nvarchar(30)) FROM dbo.RECEIVER "
            + "WHERE ISNULL(MARKED_FOR_PURGE, '') = 'Y' "
            + $"AND CAST(ID AS nvarchar(30)) IN ({string.Join(", ", parameterNames)});";

        var marked = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            marked.Add(reader.GetString(0));
        }

        return marked;
    }
}

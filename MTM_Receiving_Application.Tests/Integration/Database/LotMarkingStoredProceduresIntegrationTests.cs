using System.Data;
using FluentAssertions;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Tests.Integration.Database.StoredProcedures;

namespace MTM_Receiving_Application.Tests.Integration.Database;

/// <summary>
/// Proves a lot marking is copied verbatim through both hops a queued receiving row makes after
/// it has been saved: the promotion into <c>receiving_history</c>, and the re-queue back out of
/// it for a reprint. Neither hop may re-resolve, recompute, or drop the codes.
/// </summary>
/// <remarks>
/// The schema under test is whatever the connection string in
/// <c>MTM_TEST_MYSQL_CONNECTION_STRING</c> (or <c>MTM_MYSQL_CONNECTION_STRING</c>) names, so point
/// it at <c>mtm_receiving_application_test</c>. Every row written here is prefixed <c>TEST-</c>,
/// carries an employee number unique to this run, and is deleted on disposal. When no connection
/// string is set, or the target schema is not a test schema, the test reports success without
/// touching a database.
/// </remarks>
public sealed class LotMarkingStoredProceduresIntegrationTests
    : StoredProcedureIntegrationTestBase,
        IAsyncLifetime
{
    private const string ShapeCode = "\u2B50";
    private const string ColorCode = "TEAL";

    private readonly List<int> _createdHistoryIds = [];
    private readonly List<string> _createdLoadIds = [];

    private string? _connectionString;
    private int _employeeNumber;

    public Task InitializeAsync()
    {
        _connectionString = TryGetIntegrationConnectionString();

        // A run-unique employee number scopes the promotion, and every row this test later reads
        // back, to rows this test created. A shared test schema is never swept by another run.
        _employeeNumber = Random.Shared.Next(900_000, 999_999);

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        var connectionString = _connectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        foreach (var loadId in _createdLoadIds)
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_label_data WHERE load_id = @loadId;",
                new MySqlParameter("@loadId", loadId)
            );
        }

        foreach (var historyId in _createdHistoryIds)
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_history WHERE id = @historyId;",
                new MySqlParameter("@historyId", historyId)
            );
        }
    }

    [Fact]
    public async Task LotMarking_ShouldSurvivePromotionAndRequeueUnchanged_WhenQueueRowIsMarked()
    {
        var connectionString = _connectionString;
        if (string.IsNullOrWhiteSpace(connectionString) || !IsTestSchema(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix();
        var loadId = Guid.NewGuid().ToString();
        var receivedDate = DateTime.UtcNow;

        _createdLoadIds.Add(loadId);

        await InsertQueueRowAsync(connectionString, loadId, suffix, receivedDate);

        await ExecuteStoredProcedureNonQueryAsync(
            connectionString,
            "sp_Receiving_LabelData_SetMarking",
            new MySqlParameter("p_load_id", loadId),
            new MySqlParameter("p_heat_shape_code", ShapeCode),
            new MySqlParameter("p_heat_color_code", ColorCode)
        );

        var queuedShapeCode = await ReadQueueShapeCodeAsync(connectionString, loadId, 0);
        var queuedColorCode = await ReadQueueColorCodeAsync(connectionString, loadId, 0);
        queuedShapeCode.Should().Be(ShapeCode);
        queuedColorCode.Should().Be(ColorCode);

        var promotion = await ExecuteStoredProcedureNonQueryAsync(
            connectionString,
            "sp_Receiving_LabelData_ClearToHistory",
            new MySqlParameter("p_archived_by", $"TEST-{suffix}"),
            new MySqlParameter("p_employee_number", _employeeNumber),
            new MySqlParameter("p_clear_all", 0),
            new MySqlParameter("p_rows_moved", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output,
            },
            new MySqlParameter("p_archive_batch_id", MySqlDbType.VarChar, 36)
            {
                Direction = ParameterDirection.Output,
            },
            new MySqlParameter("p_status", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output,
            },
            new MySqlParameter("p_error_message", MySqlDbType.VarChar, 1000)
            {
                Direction = ParameterDirection.Output,
            }
        );

        ConvertToInt(promotion.OutputValues["p_status"]).Should().Be(0);
        ConvertToInt(promotion.OutputValues["p_rows_moved"]).Should().Be(1);

        var historyId = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT id FROM receiving_history WHERE load_guid = @loadId;",
                new MySqlParameter("@loadId", loadId)
            )
        );

        historyId.Should().BeGreaterThan(0);
        _createdHistoryIds.Add(historyId);

        var promotedShapeCode = (string?)
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT heat_shape_code FROM receiving_history WHERE id = @historyId;",
                new MySqlParameter("@historyId", historyId)
            );
        var promotedColorCode = (string?)
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT heat_color_code FROM receiving_history WHERE id = @historyId;",
                new MySqlParameter("@historyId", historyId)
            );

        promotedShapeCode.Should().Be(ShapeCode);
        promotedColorCode.Should().Be(ColorCode);

        var requeue = await ExecuteStoredProcedureQueryAsync(
            connectionString,
            "sp_Receiving_LabelData_InsertFromHistory",
            new MySqlParameter("p_history_id", historyId),
            new MySqlParameter("p_queued_by", $"TEST-{suffix}"),
            new MySqlParameter("p_employee_number", _employeeNumber)
        );

        requeue.Rows.Count.Should().Be(1);
        ConvertToInt(requeue.Rows[0]["rows_inserted"]).Should().Be(1);

        var requeuedShapeCode = await ReadQueueShapeCodeAsync(connectionString, loadId, 1);
        var requeuedColorCode = await ReadQueueColorCodeAsync(connectionString, loadId, 1);

        requeuedShapeCode.Should().Be(ShapeCode);
        requeuedColorCode.Should().Be(ColorCode);
    }

    private static bool IsTestSchema(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        var database = new MySqlConnectionStringBuilder(connectionString).Database;
        return database.Contains("test", StringComparison.OrdinalIgnoreCase);
    }

    private async Task InsertQueueRowAsync(
        string connectionString,
        string loadId,
        string suffix,
        DateTime receivedDate
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"
INSERT INTO receiving_label_data
(
    load_id,
    load_number,
    quantity,
    weight_quantity,
    part_id,
    part_description,
    employee_number,
    heat,
    received_date,
    transaction_date,
    label_number
)
VALUES
(
    @loadId,
    1,
    1,
    1.00,
    @partId,
    'TEST- queued row for the lot marking copy test',
    @employeeNumber,
    @heat,
    @receivedDate,
    @transactionDate,
    1
);",
            new MySqlParameter("@loadId", loadId),
            new MySqlParameter("@partId", $"TEST-PART-{suffix}"),
            new MySqlParameter("@employeeNumber", _employeeNumber),
            new MySqlParameter("@heat", $"TEST-HEAT-{suffix}"),
            new MySqlParameter("@receivedDate", receivedDate),
            new MySqlParameter("@transactionDate", receivedDate.Date)
        );
    }

    private async Task<string?> ReadQueueShapeCodeAsync(
        string connectionString,
        string loadId,
        int isReprint
    ) =>
        (string?)
            await ExecuteSqlScalarAsync(
                connectionString,
                @"SELECT heat_shape_code
FROM receiving_label_data
WHERE load_id = @loadId
  AND is_reprint = @isReprint;",
                new MySqlParameter("@loadId", loadId),
                new MySqlParameter("@isReprint", isReprint)
            );

    private async Task<string?> ReadQueueColorCodeAsync(
        string connectionString,
        string loadId,
        int isReprint
    ) =>
        (string?)
            await ExecuteSqlScalarAsync(
                connectionString,
                @"SELECT heat_color_code
FROM receiving_label_data
WHERE load_id = @loadId
  AND is_reprint = @isReprint;",
                new MySqlParameter("@loadId", loadId),
                new MySqlParameter("@isReprint", isReprint)
            );
}

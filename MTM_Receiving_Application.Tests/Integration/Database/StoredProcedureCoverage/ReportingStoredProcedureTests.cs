using FluentAssertions;
using MySql.Data.MySqlClient;
using System.Data;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Exercises the reporting procedures that had no coverage - the analytics coverage figures and the
/// received-part cohort - plus the Volvo generated-label reprint search.
/// </summary>
[Collection(StoredProcedureCoverageCollection.Name)]
public sealed class ReportingStoredProcedureTests : StoredProcedureTestBase, IAsyncLifetime
{
    private static readonly DateTime WindowStart = new(2099, 8, 3);
    private static readonly DateTime WindowEnd = new(2099, 8, 9);

    private string _partId = string.Empty;

    public ReportingStoredProcedureTests(StoredProcedureTestDatabase database)
        : base(database)
    {
    }

    public Task InitializeAsync()
    {
        _partId = UniqueToken("TEST-RPT-");
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Database.ConnectionString is null)
        {
            return;
        }

        await ExecuteSqlAsync(
            "DELETE FROM receiving_history WHERE part_id = @partId;",
            new MySqlParameter("@partId", _partId));
        await ExecuteSqlAsync(
            "DELETE FROM volvo_generated_label_history WHERE part_number = @partId;",
            new MySqlParameter("@partId", _partId));
    }

    [SkippableFact]
    public async Task ReceivedPartNumbers_ShouldIncludeThePartsTheAppReceivedInTheRange()
    {
        SkipWhenUnavailable();

        await SeedHistoryRowAsync(WindowStart.AddDays(1), quantity: 4);

        var parts = await QueryAsync(
            "sp_Reporting_ReceivingAnalyticsApp_GetReceivedPartNumbers",
            Parameter("p_start_date", WindowStart.Date),
            Parameter("p_end_date", WindowEnd.Date));

        parts.Rows.Cast<DataRow>()
            .Select(row => row[0].ToString())
            .Should().Contain(_partId);
    }

    [SkippableFact]
    public async Task CoverageLines_ShouldReportOneLinePerReceivedDateAndPart()
    {
        SkipWhenUnavailable();

        var receivedDate = WindowStart.AddDays(2);
        await SeedHistoryRowAsync(receivedDate, quantity: 6, poNumber: "TEST-PO-COV", poLine: "1");

        var lines = await QueryAsync(
            "sp_Reporting_ReceivingAnalyticsApp_GetCoverageLines",
            Parameter("p_start_date", WindowStart.Date),
            Parameter("p_end_date", WindowEnd.Date));

        var line = lines.Rows.Cast<DataRow>()
            .Should().ContainSingle(row => row["part_id"].ToString() == _partId).Subject;
        Convert.ToDateTime(line["last_received_date"]).Date.Should().Be(receivedDate.Date);
        ToInt(line["line_count"]).Should().Be(1);
    }

    [SkippableFact]
    public async Task GeneratedLabelHistory_GetForReprint_ShouldFlagRowsAlreadyQueued()
    {
        SkipWhenUnavailable();

        var originalId = 987654321;
        await ExecuteSqlAsync(
            "INSERT INTO volvo_generated_label_history "
            + "(original_id, shipment_id, shipment_number, shipment_date, part_number, quantity, "
            + "skid_number, total_skids, part_description, archived_by, archive_batch_id) "
            + "VALUES (@originalId, 1, 1000, @shipmentDate, @partId, 1, 1, 1, 'TEST DESCRIPTION', "
            + "'TEST-USER', @batchId);",
            new MySqlParameter("@originalId", originalId),
            new MySqlParameter("@shipmentDate", WindowStart.Date),
            new MySqlParameter("@partId", _partId),
            new MySqlParameter("@batchId", Guid.NewGuid().ToString()));

        var rows = await QueryAsync(
            "sp_Volvo_GeneratedLabelHistory_GetForReprint",
            Parameter("p_start_date", WindowStart.Date),
            Parameter("p_end_date", WindowEnd.Date),
            Parameter("p_search_by", "part"),
            Parameter("p_search_text", _partId));

        var row = rows.Rows.Cast<DataRow>().Should().ContainSingle().Subject;
        row["part_number"].ToString().Should().Be(_partId);
        ToInt(row["already_queued"]).Should().Be(0);
    }

    private async Task SeedHistoryRowAsync(
        DateTime transactionDate,
        int quantity,
        string? poNumber = null,
        string? poLine = null)
    {
        await ExecuteSqlAsync(
            "INSERT INTO receiving_history "
            + "(load_guid, quantity, part_id, po_number, po_line_number, employee_number, "
            + "transaction_date, initial_location) "
            + "VALUES (@loadGuid, @quantity, @partId, @poNumber, @poLine, 9001, @transactionDate, "
            + "'TEST-ORIGIN');",
            new MySqlParameter("@loadGuid", Guid.NewGuid().ToString()),
            new MySqlParameter("@quantity", quantity),
            new MySqlParameter("@partId", _partId),
            new MySqlParameter("@poNumber", (object?)poNumber ?? DBNull.Value),
            new MySqlParameter("@poLine", (object?)poLine ?? DBNull.Value),
            new MySqlParameter("@transactionDate", transactionDate.Date));
    }
}

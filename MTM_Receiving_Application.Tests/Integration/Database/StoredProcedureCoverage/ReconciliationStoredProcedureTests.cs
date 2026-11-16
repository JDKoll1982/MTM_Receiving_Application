using FluentAssertions;
using MySql.Data.MySqlClient;
using System.Data;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Exercises the reconciliation procedures: the activity calendar, the one-day row read, and the
/// single location correction that reconciliation owns.
/// </summary>
[Collection(StoredProcedureCoverageCollection.Name)]
public sealed class ReconciliationStoredProcedureTests : StoredProcedureTestBase, IAsyncLifetime
{
    private static readonly DateTime WindowStart = new(2099, 7, 10);
    private static readonly DateTime WindowEnd = new(2099, 7, 12);

    private string _partId = string.Empty;

    public ReconciliationStoredProcedureTests(StoredProcedureTestDatabase database)
        : base(database)
    {
    }

    public Task InitializeAsync()
    {
        _partId = UniqueToken("TEST-RECON-");
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Database.ConnectionString is null)
        {
            return;
        }

        await ExecuteSqlAsync(
            "DELETE FROM receiving_label_data WHERE part_id = @partId;",
            new MySqlParameter("@partId", _partId));
        await ExecuteSqlAsync(
            "DELETE FROM receiving_history WHERE part_id = @partId;",
            new MySqlParameter("@partId", _partId));
    }

    [SkippableFact]
    public async Task DayActivity_GetRange_ShouldListOnlyDaysHoldingAReceivingRow()
    {
        SkipWhenUnavailable();

        await SeedQueueRowAsync(WindowStart);
        await SeedHistoryRowAsync(WindowEnd);

        var days = await QueryAsync(
            "sp_Reconciliation_DayActivity_GetRange",
            Parameter("p_from", WindowStart.Date),
            Parameter("p_to", WindowEnd.Date));

        var dates = days.Rows.Cast<DataRow>()
            .Select(row => Convert.ToDateTime(row["activity_date"]).Date)
            .ToList();

        dates.Should().Contain(WindowStart.Date);
        dates.Should().Contain(WindowEnd.Date);
        dates.Should().NotContain(WindowStart.Date.AddDays(1));
    }

    [SkippableFact]
    public async Task ReceivingRows_GetDay_ShouldReturnBothRowSourcesForTheDay()
    {
        SkipWhenUnavailable();

        await SeedQueueRowAsync(WindowStart);
        await SeedHistoryRowAsync(WindowStart);

        var rows = await QueryAsync(
            "sp_Reconciliation_ReceivingRows_GetDay",
            Parameter("p_transaction_date", WindowStart.Date));

        var mine = rows.Rows.Cast<DataRow>()
            .Where(row => (string)row["part_id"] == _partId)
            .ToList();

        mine.Select(row => (string)row["row_source"])
            .Should().BeEquivalentTo(["CurrentLabel", "History"]);
    }

    [SkippableFact]
    public async Task ReceivingLocation_Update_ShouldCorrectTheSavedLocationOfOneRow()
    {
        SkipWhenUnavailable();

        await SeedQueueRowAsync(WindowStart);

        var rows = await QueryAsync(
            "sp_Reconciliation_ReceivingRows_GetDay",
            Parameter("p_transaction_date", WindowStart.Date));
        var recordId = ToInt(
            rows.Rows.Cast<DataRow>()
                .Single(row => (string)row["row_source"] == "CurrentLabel"
                    && (string)row["part_id"] == _partId)["record_id"]);

        var outputs = await ExecuteProcedureWithOutputsAsync(
            "sp_Reconciliation_ReceivingLocation_Update",
            Parameter("p_row_source", "CurrentLabel"),
            Parameter("p_record_id", recordId),
            Parameter("p_destination_location", "TEST-NEW-LOC"),
            Parameter("p_operator_user_id", "TEST-USER"),
            new MySqlParameter("p_affected_rows", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output,
            });

        ToInt(outputs["p_affected_rows"]).Should().Be(1);

        var location = await ExecuteScalarAsync(
            "SELECT initial_location FROM receiving_label_data WHERE id = @id;",
            new MySqlParameter("@id", recordId));
        ToText(location).Should().Be("TEST-NEW-LOC");
    }

    private async Task SeedQueueRowAsync(DateTime transactionDate)
    {
        await ExecuteSqlAsync(
            "INSERT INTO receiving_label_data "
            + "(part_id, quantity, transaction_date, employee_number, user_id, initial_location) "
            + "VALUES (@partId, 1, @transactionDate, 9001, 'TEST-USER', 'TEST-ORIGIN');",
            new MySqlParameter("@partId", _partId),
            new MySqlParameter("@transactionDate", transactionDate.Date));
    }

    private async Task SeedHistoryRowAsync(DateTime transactionDate)
    {
        await ExecuteSqlAsync(
            "INSERT INTO receiving_history "
            + "(load_guid, quantity, part_id, employee_number, transaction_date, initial_location) "
            + "VALUES (@loadGuid, 1, @partId, 9001, @transactionDate, 'TEST-ORIGIN');",
            new MySqlParameter("@loadGuid", Guid.NewGuid().ToString()),
            new MySqlParameter("@partId", _partId),
            new MySqlParameter("@transactionDate", transactionDate.Date));
    }
}

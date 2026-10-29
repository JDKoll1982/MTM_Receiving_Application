using System.Data;
using FluentAssertions;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedures;

/// <summary>
/// Exercises the four <c>sp_Reporting_ReceivingAnalyticsApp_*</c> procedures against the test
/// database: the range boundary, a row that carries both date columns, a row that carries only
/// <c>transaction_date</c>, a blank supplier, the missing-load count, and the on-time split.
/// </summary>
/// <remarks>
/// Every row is seeded inside a window no real receiving history can occupy (March 2099) and is
/// deleted again on disposal, so each count asserted here is this run's own. The schema is whatever
/// <c>MTM_TEST_MYSQL_CONNECTION_STRING</c> (or <c>MTM_MYSQL_CONNECTION_STRING</c>) names, and a
/// target that is not a test schema is left untouched.
/// </remarks>
public sealed class ReceivingAnalyticsAppStoredProcedureTests
    : StoredProcedureIntegrationTestBase,
        IAsyncLifetime
{
    private static readonly DateTime WindowStart = new(2099, 3, 10);
    private static readonly DateTime WindowEnd = new(2099, 3, 20);

    private string? _connectionString;
    private string _partIdPrefix = string.Empty;
    private int _seedSequence;

    public Task InitializeAsync()
    {
        _connectionString = TryGetIntegrationConnectionString();
        _partIdPrefix = $"ITEST-{CreateUniqueSuffix()[..12]}";
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        var connectionString = TestConnection();
        if (connectionString is null)
        {
            return;
        }

        await ExecuteSqlNonQueryAsync(
            connectionString,
            "DELETE FROM receiving_history WHERE part_id LIKE @prefix;",
            new MySqlParameter("@prefix", $"{_partIdPrefix}-%")
        );
    }

    [Fact]
    public async Task GetActivityByDay_ShouldIncludeTheStartDay_AndExcludeTheEndDay()
    {
        var connectionString = TestConnection();
        if (connectionString is null)
        {
            return;
        }

        await SeedAsync(connectionString, "ITEST-BOUNDARY-START", WindowStart, WindowStart, "ITEST VENDOR", "LB", 1, null);
        await SeedAsync(connectionString, "ITEST-BOUNDARY-END", WindowEnd, WindowEnd, "ITEST VENDOR", "LB", 1, null);

        var table = await ExecuteStoredProcedureQueryAsync(
            connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay",
            new MySqlParameter("p_start_date", WindowStart),
            new MySqlParameter("p_end_date", WindowEnd)
        );

        var buckets = BucketDates(table);
        buckets.Should().Contain(WindowStart.Date);
        buckets.Should().NotContain(WindowEnd.Date);
    }

    [Fact]
    public async Task GetActivityByDay_ShouldCountARowCarryingBothDateColumns_Once()
    {
        var connectionString = TestConnection();
        if (connectionString is null)
        {
            return;
        }

        var received = WindowStart.AddDays(2);
        var transacted = WindowStart.AddDays(5);
        await SeedAsync(connectionString, "ITEST-BOTH-DATES", received, transacted, "ITEST VENDOR", "LB", 7, null);

        var table = await ExecuteStoredProcedureQueryAsync(
            connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay",
            new MySqlParameter("p_start_date", WindowStart),
            new MySqlParameter("p_end_date", WindowEnd)
        );

        // A row that carries both columns belongs to one day, not two: the received date wins and
        // the transaction date adds no second bucket.
        var rows = RowsForPartType(table, "ITEST-BOTH-DATES");
        var row = rows.Should().ContainSingle().Subject;
        Convert.ToDateTime(row["bucket_date"]).Date.Should().Be(received.Date);
        ConvertToInt(row["label_count"]).Should().Be(1);
        ConvertToInt(row["receipt_count"]).Should().Be(1);
    }

    [Fact]
    public async Task GetActivityByDay_ShouldCountARowWithOnlyTheTransactionDate_Once()
    {
        var connectionString = TestConnection();
        if (connectionString is null)
        {
            return;
        }

        var transacted = WindowStart.AddDays(3);
        await SeedAsync(connectionString, "ITEST-TRANSACTION-ONLY", null, transacted, "ITEST VENDOR", "LB", 1, null);

        var table = await ExecuteStoredProcedureQueryAsync(
            connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetActivityByDay",
            new MySqlParameter("p_start_date", WindowStart),
            new MySqlParameter("p_end_date", WindowEnd)
        );

        var rows = RowsForPartType(table, "ITEST-TRANSACTION-ONLY");
        var row = rows.Should().ContainSingle().Subject;
        Convert.ToDateTime(row["bucket_date"]).Date.Should().Be(transacted.Date);
        ConvertToInt(row["label_count"]).Should().Be(1);
    }

    [Fact]
    public async Task GetSupplierTotals_ShouldGroupABlankSupplierAsUnknown()
    {
        var connectionString = TestConnection();
        if (connectionString is null)
        {
            return;
        }

        // Both spellings of "no supplier" land in the same group, so a reader sees one Unknown
        // rather than a blank row beside a null row.
        await SeedAsync(connectionString, "ITEST-BLANK-VENDOR", WindowStart, WindowStart, string.Empty, "LB", 1, null);
        await SeedAsync(connectionString, "ITEST-NULL-VENDOR", WindowStart, WindowStart, null, "LB", 1, null);

        var table = await ExecuteStoredProcedureQueryAsync(
            connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetSupplierTotals",
            new MySqlParameter("p_start_date", WindowStart),
            new MySqlParameter("p_end_date", WindowEnd)
        );

        var unknown = table
            .Rows.Cast<DataRow>()
            .Where(row =>
                Convert.ToString(row["vendor_name"]) == "Unknown"
                && Convert.ToString(row["unit_of_measure"]) == "LB"
            )
            .ToList();

        var row = unknown.Should().ContainSingle().Subject;
        ConvertToInt(row["label_count"]).Should().Be(2);
    }

    [Fact]
    public async Task GetMissingLoadNumberCount_ShouldMatchTheSeededRow()
    {
        var connectionString = TestConnection();
        if (connectionString is null)
        {
            return;
        }

        await SeedAsync(connectionString, "ITEST-NO-LOAD", WindowStart, WindowStart, "ITEST VENDOR", "LB", null, null);
        await SeedAsync(connectionString, "ITEST-WITH-LOAD", WindowStart, WindowStart, "ITEST VENDOR", "LB", 42, null);

        var table = await ExecuteStoredProcedureQueryAsync(
            connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetMissingLoadNumberCount",
            new MySqlParameter("p_start_date", WindowStart),
            new MySqlParameter("p_end_date", WindowEnd)
        );

        var row = table.Rows.Cast<DataRow>().Should().ContainSingle().Subject;
        ConvertToInt(row["missing_load_number_count"]).Should().Be(1);
    }

    [Fact]
    public async Task GetOnTimeByDay_ShouldClassifyOneSeededReceiptInEachStatus()
    {
        var connectionString = TestConnection();
        if (connectionString is null)
        {
            return;
        }

        var received = WindowStart.AddDays(2);
        await SeedAsync(connectionString, "ITEST-EARLY", received, received, "ITEST VENDOR", "LB", 1, received.AddDays(2));
        await SeedAsync(connectionString, "ITEST-ONTIME", received, received, "ITEST VENDOR", "LB", 1, received);
        await SeedAsync(connectionString, "ITEST-LATE", received, received, "ITEST VENDOR", "LB", 1, received.AddDays(-1));
        await SeedAsync(connectionString, "ITEST-NODUE", received, received, "ITEST VENDOR", "LB", 1, null);

        var table = await ExecuteStoredProcedureQueryAsync(
            connectionString,
            "sp_Reporting_ReceivingAnalyticsApp_GetOnTimeByDay",
            new MySqlParameter("p_start_date", WindowStart),
            new MySqlParameter("p_end_date", WindowEnd)
        );

        var byStatus = table
            .Rows.Cast<DataRow>()
            .Where(row => Convert.ToDateTime(row["bucket_date"]).Date == received.Date)
            .ToDictionary(
                row => Convert.ToString(row["status"]) ?? string.Empty,
                row => ConvertToInt(row["receipt_count"]),
                StringComparer.Ordinal
            );

        byStatus["Early"].Should().Be(1);
        byStatus["OnTime"].Should().Be(1);
        byStatus["Late"].Should().Be(1);
        byStatus["NoDueDate"].Should().Be(1);
    }

    private string? TestConnection()
    {
        var connectionString = _connectionString;
        return string.IsNullOrWhiteSpace(connectionString) || !IsTestSchema(connectionString)
            ? null
            : connectionString;
    }

    private async Task SeedAsync(
        string connectionString,
        string partType,
        DateTime? receivedDate,
        DateTime transactionDate,
        string? vendorName,
        string unit,
        int? loadNumber,
        DateTime? poDueDate
    )
    {
        _seedSequence++;

        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"
INSERT INTO receiving_history
(
    load_guid,
    quantity,
    part_id,
    part_type,
    transaction_date,
    received_date,
    vendor_name,
    unit_of_measure,
    load_number,
    po_due_date,
    employee_number,
    label_number
)
VALUES
(
    @loadGuid,
    1,
    @partId,
    @partType,
    @transactionDate,
    @receivedDate,
    @vendorName,
    @unit,
    @loadNumber,
    @poDueDate,
    9001,
    1
);",
            new MySqlParameter("@loadGuid", Guid.NewGuid().ToString()),
            new MySqlParameter("@partId", $"{_partIdPrefix}-{_seedSequence}"),
            new MySqlParameter("@partType", partType),
            new MySqlParameter("@transactionDate", transactionDate.Date),
            new MySqlParameter("@receivedDate", (object?)receivedDate ?? DBNull.Value),
            new MySqlParameter("@vendorName", (object?)vendorName ?? DBNull.Value),
            new MySqlParameter("@unit", unit),
            new MySqlParameter("@loadNumber", (object?)loadNumber ?? DBNull.Value),
            new MySqlParameter("@poDueDate", (object?)poDueDate ?? DBNull.Value)
        );
    }

    private static List<DateTime> BucketDates(DataTable table) =>
        table
            .Rows.Cast<DataRow>()
            .Select(row => Convert.ToDateTime(row["bucket_date"]).Date)
            .Distinct()
            .ToList();

    private static List<DataRow> RowsForPartType(DataTable table, string partType) =>
        table
            .Rows.Cast<DataRow>()
            .Where(row => Convert.ToString(row["part_type"]) == partType)
            .ToList();

    private static bool IsTestSchema(string connectionString)
    {
        var database = new MySqlConnectionStringBuilder(connectionString).Database;
        return database.Contains("test", StringComparison.OrdinalIgnoreCase);
    }
}

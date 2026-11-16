using FluentAssertions;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Exercises the two receiving procedures that had no coverage: the reprint search over receiving
/// history, and the vendor-specific variable mapping.
/// </summary>
[Collection(StoredProcedureCoverageCollection.Name)]
public sealed class ReceivingStoredProcedureTests : StoredProcedureTestBase, IAsyncLifetime
{
    private string _partId = string.Empty;
    private string _vendor = string.Empty;

    public ReceivingStoredProcedureTests(StoredProcedureTestDatabase database)
        : base(database)
    {
    }

    public Task InitializeAsync()
    {
        var token = UniqueToken("TEST-RCV-");
        _partId = token + "-PART";
        _vendor = token + " VENDOR";
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
            "DELETE FROM receiving_label_data WHERE part_id = @partId;",
            new MySqlParameter("@partId", _partId));
        await ExecuteSqlAsync(
            "DELETE FROM receiving_vendor_variables WHERE vendor_name = @vendor;",
            new MySqlParameter("@vendor", _vendor));
    }

    [SkippableFact]
    public async Task History_GetForReprint_ShouldFlagRowsAlreadyQueuedForReprint()
    {
        SkipWhenUnavailable();

        var loadGuid = Guid.NewGuid().ToString();
        var transactionDate = new DateTime(2099, 5, 4);

        await ExecuteSqlAsync(
            "INSERT INTO receiving_history "
            + "(load_guid, quantity, part_id, po_number, employee_number, heat, transaction_date, "
            + "initial_location, label_number, vendor_name, part_description) "
            + "VALUES (@loadGuid, 1, @partId, 'TEST-PO', 9001, 'TEST-HEAT', @transactionDate, "
            + "'RECV', 1, @vendor, 'TEST DESCRIPTION');",
            new MySqlParameter("@loadGuid", loadGuid),
            new MySqlParameter("@partId", _partId),
            new MySqlParameter("@transactionDate", transactionDate),
            new MySqlParameter("@vendor", _vendor));

        var beforeQueue = await QueryAsync(
            "sp_Receiving_History_GetForReprint",
            Parameter("p_start_date", transactionDate.Date),
            Parameter("p_end_date", transactionDate.Date),
            Parameter("p_search_by", "part"),
            Parameter("p_search_text", _partId));

        var historyRow = beforeQueue.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        ToInt(historyRow["already_queued"]).Should().Be(0);
        historyRow["part_id"].ToString().Should().Be(_partId);

        await ExecuteSqlAsync(
            "INSERT INTO receiving_label_data "
            + "(part_id, quantity, transaction_date, employee_number, user_id, is_reprint, load_id) "
            + "VALUES (@partId, 1, @transactionDate, 9001, 'TEST-USER', 1, @loadId);",
            new MySqlParameter("@partId", _partId),
            new MySqlParameter("@transactionDate", transactionDate),
            new MySqlParameter("@loadId", loadGuid));

        var afterQueue = await QueryAsync(
            "sp_Receiving_History_GetForReprint",
            Parameter("p_start_date", transactionDate.Date),
            Parameter("p_end_date", transactionDate.Date),
            Parameter("p_search_by", "part"),
            Parameter("p_search_text", _partId));

        ToInt(afterQueue.Rows[0]["already_queued"]).Should().Be(1,
            "a reprint row already sits in the queue for this history record");
    }

    [SkippableFact]
    public async Task VendorVariables_ShouldInsertReadUpdateAndDelete()
    {
        SkipWhenUnavailable();

        await ExecuteProcedureAsync(
            "sp_receiving_vendor_variables_insert",
            Parameter("p_vendor_name", _vendor),
            Parameter("p_variable_name", "TEST-DM"));

        var all = await QueryAsync("sp_receiving_vendor_variables_get_all");
        all.Rows.Cast<System.Data.DataRow>()
            .Should().Contain(row => (string)row["vendor_name"] == _vendor);

        var byVendor = await QueryAsync(
            "sp_receiving_vendor_variables_get_by_vendor",
            Parameter("p_vendor_name", _vendor));
        var row = byVendor.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        row["variable_name"].ToString().Should().Be("TEST-DM");

        await ExecuteProcedureAsync(
            "sp_receiving_vendor_variables_update",
            Parameter("p_vendor_name", _vendor),
            Parameter("p_variable_name", "TEST-HEAT"));

        var updated = await QueryAsync(
            "sp_receiving_vendor_variables_get_by_vendor",
            Parameter("p_vendor_name", _vendor));
        updated.Rows[0]["variable_name"].ToString().Should().Be("TEST-HEAT");

        await ExecuteProcedureAsync(
            "sp_receiving_vendor_variables_delete",
            Parameter("p_vendor_name", _vendor));

        var deleted = await QueryAsync(
            "sp_receiving_vendor_variables_get_by_vendor",
            Parameter("p_vendor_name", _vendor));
        deleted.Rows.Count.Should().Be(0);
    }
}

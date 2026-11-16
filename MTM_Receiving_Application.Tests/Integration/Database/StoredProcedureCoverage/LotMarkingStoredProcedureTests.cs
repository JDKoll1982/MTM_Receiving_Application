using FluentAssertions;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Exercises the heat/lot marking procedures: the enabled-part list, the marking ledger, the
/// grouped heat keys, and the two procedures that put a marking onto or take it off receiving rows.
/// </summary>
[Collection(StoredProcedureCoverageCollection.Name)]
public sealed class LotMarkingStoredProcedureTests : StoredProcedureTestBase, IAsyncLifetime
{
    private string _heat = string.Empty;
    private string _normalizedHeat = string.Empty;
    private string _partId = string.Empty;
    private string _supplier = string.Empty;

    public LotMarkingStoredProcedureTests(StoredProcedureTestDatabase database)
        : base(database)
    {
    }

    public Task InitializeAsync()
    {
        var token = UniqueToken("TEST-LOT-");
        _heat = token + "-HEAT";
        _normalizedHeat = token + "HEAT";
        _partId = token + "-PART";
        _supplier = token + " SUPPLIER";
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
        await ExecuteSqlAsync(
            "DELETE FROM lot_markings WHERE heat_number_normalized = @heat;",
            new MySqlParameter("@heat", _normalizedHeat));
        await ExecuteSqlAsync(
            "DELETE FROM lot_marking_part_scope WHERE part_id = @partId;",
            new MySqlParameter("@partId", _partId));
    }

    [SkippableFact]
    public async Task PartScope_ShouldUpsertSearchDisableAndDelete()
    {
        SkipWhenUnavailable();

        var upsert = await QueryAsync(
            "sp_LotMarkingPartScope_Upsert",
            Parameter("p_part_id", _partId),
            Parameter("p_added_by", "TEST-USER"));

        var scopeId = Convert.ToInt32(upsert.Rows[0]["part_scope_id"]);
        scopeId.Should().BeGreaterThan(0);

        var all = await QueryAsync("sp_LotMarkingPartScope_GetAll");
        all.Rows.Cast<System.Data.DataRow>()
            .Should().Contain(row => (string)row["part_id"] == _partId);

        var search = await QueryAsync(
            "sp_LotMarkingPartScope_Search",
            Parameter("p_search_text", _partId));
        search.Rows.Cast<System.Data.DataRow>()
            .Should().Contain(row => (string)row["part_id"] == _partId);

        await ExecuteProcedureAsync(
            "sp_LotMarkingPartScope_SetEnabled",
            Parameter("p_id", scopeId),
            Parameter("p_is_enabled", 0));

        var disabled = await ExecuteScalarAsync(
            "SELECT is_enabled FROM lot_marking_part_scope WHERE id = @id;",
            new MySqlParameter("@id", scopeId));
        ToInt(disabled).Should().Be(0);

        await ExecuteProcedureAsync(
            "sp_LotMarkingPartScope_Delete",
            Parameter("p_id", scopeId));

        var remaining = await ExecuteScalarAsync(
            "SELECT COUNT(*) FROM lot_marking_part_scope WHERE id = @id;",
            new MySqlParameter("@id", scopeId));
        ToInt(remaining).Should().Be(0);
    }

    [SkippableFact]
    public async Task LotMarkings_ShouldInsertResolveLockUpdateAndDelete()
    {
        SkipWhenUnavailable();

        await ExecuteProcedureAsync(
            "sp_LotMarkings_Insert",
            Parameter("p_heat_number", _heat),
            Parameter("p_heat_number_normalized", _normalizedHeat),
            Parameter("p_supplier_id", _supplier),
            Parameter("p_supplier_name", _supplier),
            Parameter("p_shape_code", "Circle"),
            Parameter("p_color_code", "Red"),
            Parameter("p_marking_source", "Auto"),
            Parameter("p_first_seen_part_number", _partId),
            Parameter("p_resolved_by", "TEST-USER"));

        var resolved = await QueryAsync(
            "sp_LotMarkings_Resolve",
            Parameter("p_heat_number_normalized", _normalizedHeat),
            Parameter("p_supplier_id", _supplier));

        var marking = resolved.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        marking["shape_code"].ToString().Should().Be("Circle");
        var markingId = ToInt(marking["id"]);

        var byHeat = await QueryAsync("sp_LotMarkings_GetByHeat", Parameter("p_search_text", _heat));
        byHeat.Rows.Cast<System.Data.DataRow>()
            .Should().Contain(row => ToInt(row["id"]) == markingId);

        var all = await QueryAsync("sp_LotMarkings_GetAll");
        all.Rows.Cast<System.Data.DataRow>()
            .Should().Contain(row => ToInt(row["id"]) == markingId);

        await ExecuteProcedureAsync(
            "sp_LotMarkings_SetLocked",
            Parameter("p_id", markingId),
            Parameter("p_is_locked", 1),
            Parameter("p_resolved_by", "TEST-USER"));

        var locked = await ExecuteScalarAsync(
            "SELECT is_locked FROM lot_markings WHERE id = @id;",
            new MySqlParameter("@id", markingId));
        ToInt(locked).Should().Be(1);

        await ExecuteProcedureAsync(
            "sp_LotMarkings_UpdateMarking",
            Parameter("p_id", markingId),
            Parameter("p_shape_code", "Square"),
            Parameter("p_color_code", "Blue"),
            Parameter("p_marking_source", "Manual"),
            Parameter("p_change_reason", "TEST-CHANGE"),
            Parameter("p_resolved_by", "TEST-USER"));

        var updated = await ExecuteScalarAsync(
            "SELECT shape_code FROM lot_markings WHERE id = @id;",
            new MySqlParameter("@id", markingId));
        ToText(updated).Should().Be("Square");

        var deleteWhileLocked = async () => await ExecuteProcedureAsync(
            "sp_LotMarkings_Delete",
            Parameter("p_id", markingId));
        await deleteWhileLocked.Should().ThrowAsync<MySqlException>(
            "a locked marking is protected from removal");

        await ExecuteProcedureAsync(
            "sp_LotMarkings_SetLocked",
            Parameter("p_id", markingId),
            Parameter("p_is_locked", 0),
            Parameter("p_resolved_by", "TEST-USER"));

        await ExecuteProcedureAsync("sp_LotMarkings_Delete", Parameter("p_id", markingId));

        var afterDelete = await QueryAsync(
            "sp_LotMarkings_Resolve",
            Parameter("p_heat_number_normalized", _normalizedHeat),
            Parameter("p_supplier_id", _supplier));
        afterDelete.Rows.Count.Should().Be(0);
    }

    [SkippableFact]
    public async Task HeatLotKeys_GetAll_ShouldGroupReceivingRowsByHeatAndSupplier()
    {
        SkipWhenUnavailable();

        await SeedQueueRowAsync(shapeCode: null);

        var keys = await QueryAsync(
            "sp_Receiving_HeatLotKeys_GetAll",
            Parameter("p_from_date", null),
            Parameter("p_to_date", null));

        var row = keys.Rows.Cast<System.Data.DataRow>()
            .Should().ContainSingle(candidate => (string)candidate["heat"] == _heat).Subject;
        row["source"].ToString().Should().Be("queue");
        row["supplier_name"].ToString().Should().Be(_supplier);
        row["part_id"].ToString().Should().Be(_partId);
        ToInt(row["row_count"]).Should().Be(1);
    }

    [SkippableFact]
    public async Task ReceivingLotMarking_RealignAndClear_ShouldWriteThenRemoveTheMarking()
    {
        SkipWhenUnavailable();

        await SeedQueueRowAsync(shapeCode: "Circle");

        var realign = await QueryAsync(
            "sp_Receiving_LotMarking_RealignRows",
            Parameter("p_heat", _heat),
            Parameter("p_supplier_name", _supplier),
            Parameter("p_heat_shape_code", "Square"),
            Parameter("p_heat_color_code", "Blue"));

        ToInt(realign.Rows[0]["queue_rows_updated"]).Should().Be(1);
        (await ReadShapeCodeAsync()).Should().Be("Square");

        var clear = await QueryAsync(
            "sp_Receiving_LotMarking_ClearRows",
            Parameter("p_heat", _heat),
            Parameter("p_supplier_name", _supplier));

        ToInt(clear.Rows[0]["queue_rows_cleared"]).Should().Be(1);
        (await ReadShapeCodeAsync()).Should().BeNull();
    }

    [SkippableFact]
    public async Task UnmarkedLotMarkings_ShouldPreviewThenFillInTheMarking()
    {
        SkipWhenUnavailable();

        await QueryAsync(
            "sp_LotMarkingPartScope_Upsert",
            Parameter("p_part_id", _partId),
            Parameter("p_added_by", "TEST-USER"));
        await SeedQueueRowAsync(shapeCode: null);

        var preview = await QueryAsync(
            "sp_Receiving_UnmarkedLotMarkings_Preview",
            Parameter("p_part_id", _partId));

        preview.Rows.Cast<System.Data.DataRow>()
            .Should().Contain(row => (string)row["heat"] == _heat && ToInt(row["row_count"]) == 1);

        await ExecuteProcedureAsync(
            "sp_Receiving_UnmarkedLotMarkings_Apply",
            Parameter("p_part_id", _partId),
            Parameter("p_heat", _heat),
            Parameter("p_supplier_name", _supplier),
            Parameter("p_heat_shape_code", "Triangle"),
            Parameter("p_heat_color_code", "Green"));

        (await ReadShapeCodeAsync()).Should().Be("Triangle");
    }

    private async Task SeedQueueRowAsync(string? shapeCode)
    {
        await ExecuteSqlAsync(
            "INSERT INTO receiving_label_data "
            + "(part_id, quantity, transaction_date, employee_number, user_id, heat, "
            + "heat_shape_code, heat_color_code, po_vendor) "
            + "VALUES (@partId, 1, CURDATE(), 9001, 'TEST-USER', @heat, @shapeCode, "
            + "CASE WHEN @shapeCode IS NULL THEN NULL ELSE 'Red' END, @supplier);",
            new MySqlParameter("@partId", _partId),
            new MySqlParameter("@heat", _heat),
            new MySqlParameter("@shapeCode", (object?)shapeCode ?? DBNull.Value),
            new MySqlParameter("@supplier", _supplier));
    }

    private async Task<object?> ReadShapeCodeAsync()
    {
        return await ExecuteScalarAsync(
            "SELECT heat_shape_code FROM receiving_label_data WHERE part_id = @partId;",
            new MySqlParameter("@partId", _partId));
    }
}

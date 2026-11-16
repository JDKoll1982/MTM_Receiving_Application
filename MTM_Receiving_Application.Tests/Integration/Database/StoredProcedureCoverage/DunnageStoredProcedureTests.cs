using FluentAssertions;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Exercises the dunnage procedures that had no coverage: custom-field choices, the
/// no-label history write, the no-label part flag, changing a part's type, and the two
/// delete-impact reports.
/// </summary>
[Collection(StoredProcedureCoverageCollection.Name)]
public sealed class DunnageStoredProcedureTests : StoredProcedureTestBase, IAsyncLifetime
{
    private readonly List<int> _typeIds = [];
    private string _token = string.Empty;

    public DunnageStoredProcedureTests(StoredProcedureTestDatabase database)
        : base(database)
    {
    }

    public Task InitializeAsync()
    {
        _token = UniqueToken("TEST-DUN-");
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Database.ConnectionString is null)
        {
            return;
        }

        foreach (var typeId in _typeIds)
        {
            await ExecuteSqlAsync(
                "DELETE FROM dunnage_requires_inventory WHERE part_id IN "
                + "(SELECT part_id FROM dunnage_parts WHERE type_id = @typeId);",
                new MySqlParameter("@typeId", typeId));
            await ExecuteSqlAsync(
                "DELETE FROM dunnage_history WHERE part_id IN "
                + "(SELECT part_id FROM dunnage_parts WHERE type_id = @typeId);",
                new MySqlParameter("@typeId", typeId));
            await ExecuteSqlAsync(
                "DELETE FROM dunnage_label_data WHERE part_id IN "
                + "(SELECT part_id FROM dunnage_parts WHERE type_id = @typeId);",
                new MySqlParameter("@typeId", typeId));
            await ExecuteSqlAsync(
                "DELETE FROM dunnage_no_label_parts WHERE part_id IN "
                + "(SELECT part_id FROM dunnage_parts WHERE type_id = @typeId);",
                new MySqlParameter("@typeId", typeId));
            await ExecuteSqlAsync(
                "DELETE FROM dunnage_parts WHERE type_id = @typeId;",
                new MySqlParameter("@typeId", typeId));
            await ExecuteSqlAsync(
                "DELETE FROM dunnage_types WHERE id = @typeId;",
                new MySqlParameter("@typeId", typeId));
        }
    }

    [SkippableFact]
    public async Task CustomFieldChoices_ShouldRoundTripThroughInsertGetAndDelete()
    {
        SkipWhenUnavailable();

        var fieldId = await CreateCustomFieldAsync();

        await ExecuteProcedureAsync(
            "sp_Dunnage_CustomFieldChoices_Insert",
            Parameter("p_custom_field_id", fieldId),
            Parameter("p_choice", "TEST-Alpha"),
            Parameter("p_sort_order", 2));
        await ExecuteProcedureAsync(
            "sp_Dunnage_CustomFieldChoices_Insert",
            Parameter("p_custom_field_id", fieldId),
            Parameter("p_choice", "TEST-Bravo"),
            Parameter("p_sort_order", 1));

        var choices = await QueryAsync(
            "sp_Dunnage_CustomFieldChoices_GetByField",
            Parameter("p_custom_field_id", fieldId));

        choices.Rows.Cast<System.Data.DataRow>()
            .Select(row => row["Choice"].ToString())
            .Should().Equal("TEST-Bravo", "TEST-Alpha");

        await ExecuteProcedureAsync(
            "sp_Dunnage_CustomFieldChoices_DeleteByField",
            Parameter("p_custom_field_id", fieldId));

        var remaining = await ExecuteScalarAsync(
            "SELECT COUNT(*) FROM dunnage_custom_field_choices WHERE CustomFieldID = @fieldId;",
            new MySqlParameter("@fieldId", fieldId));

        ToInt(remaining).Should().Be(0);
    }

    [SkippableFact]
    public async Task History_Insert_ShouldWriteHistoryRowWithoutQueuingALabel()
    {
        SkipWhenUnavailable();

        var loadUuid = Guid.NewGuid().ToString();
        var partId = await CreatePartAsync();

        await ExecuteProcedureAsync(
            "sp_Dunnage_History_Insert",
            Parameter("p_load_uuid", loadUuid),
            Parameter("p_part_id", partId),
            Parameter("p_dunnage_type_id", _typeIds[0]),
            Parameter("p_dunnage_type_name", "TEST-Type"),
            Parameter("p_dunnage_type_icon", "PackageVariantClosed"),
            Parameter("p_quantity", 4.5m),
            Parameter("p_quantity_type", "Quantity"),
            Parameter("p_po_number", "TEST-PO"),
            Parameter("p_received_date", new DateTime(2020, 1, 2, 3, 4, 5)),
            Parameter("p_user_id", "TEST-USER"),
            Parameter("p_employee_number", 9001),
            Parameter("p_location", "TEST-LOC"),
            Parameter("p_label_number", "1"),
            Parameter("p_part_skid_sequence", 1),
            Parameter("p_part_skid_total", 1),
            Parameter("p_udc1", "TEST-UDC"),
            Parameter("p_udc2", null),
            Parameter("p_udc3", null),
            Parameter("p_udc4", null),
            Parameter("p_udc5", null),
            Parameter("p_udc6", null),
            Parameter("p_udc7", null),
            Parameter("p_udc8", null),
            Parameter("p_udc9", null),
            Parameter("p_udc10", null));

        var history = await QuerySqlAsync("SELECT part_id, quantity, user_id, udc1, archived_at FROM dunnage_history "
            + "WHERE load_uuid = @loadUuid;",
            new MySqlParameter("@loadUuid", loadUuid));

        var row = history.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        row["part_id"].ToString().Should().Be(partId);
        Convert.ToDecimal(row["quantity"]).Should().Be(4.5m);
        row["udc1"].ToString().Should().Be("TEST-UDC");
        row["archived_at"].Should().Be(DBNull.Value, "the row was never in the queue");

        var queued = await ExecuteScalarAsync(
            "SELECT COUNT(*) FROM dunnage_label_data WHERE load_uuid = @loadUuid;",
            new MySqlParameter("@loadUuid", loadUuid));

        ToInt(queued).Should().Be(0, "a part flagged as needing no label must not queue a label");
    }

    [SkippableFact]
    public async Task NoLabelParts_Set_ShouldAddAndRemoveTheFlag()
    {
        SkipWhenUnavailable();

        var partId = await CreatePartAsync();

        await ExecuteProcedureAsync(
            "sp_Dunnage_NoLabelParts_Set",
            Parameter("p_part_id", partId),
            Parameter("p_needs_no_label", 1),
            Parameter("p_user", "TEST-USER"));

        var flagged = await QueryAsync("sp_Dunnage_NoLabelParts_GetAll");
        flagged.Rows.Cast<System.Data.DataRow>()
            .Select(row => row["part_id"].ToString())
            .Should().Contain(partId);

        await ExecuteProcedureAsync(
            "sp_Dunnage_NoLabelParts_Set",
            Parameter("p_part_id", partId),
            Parameter("p_needs_no_label", 0),
            Parameter("p_user", "TEST-USER"));

        var cleared = await QueryAsync("sp_Dunnage_NoLabelParts_GetAll");
        cleared.Rows.Cast<System.Data.DataRow>()
            .Select(row => row["part_id"].ToString())
            .Should().NotContain(partId);
    }

    [SkippableFact]
    public async Task Parts_ChangeType_ShouldMoveThePartAndRewriteItsUdcValues()
    {
        SkipWhenUnavailable();

        var partId = await CreatePartAsync();
        var targetTypeId = await CreateTypeAsync();

        await ExecuteProcedureAsync(
            "sp_Dunnage_Parts_ChangeType",
            Parameter("p_part_id", partId),
            Parameter("p_new_type_id", targetTypeId),
            Parameter("p_udc1", "TEST-NEW-UDC"),
            Parameter("p_udc2", null),
            Parameter("p_udc3", null),
            Parameter("p_udc4", null),
            Parameter("p_udc5", null),
            Parameter("p_udc6", null),
            Parameter("p_udc7", null),
            Parameter("p_udc8", null),
            Parameter("p_udc9", null),
            Parameter("p_udc10", null),
            Parameter("p_user", "TEST-USER"));

        var part = await QuerySqlAsync("SELECT type_id, udc1, modified_by FROM dunnage_parts WHERE part_id = @partId;",
            new MySqlParameter("@partId", partId));

        var row = part.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        ToInt(row["type_id"]).Should().Be(targetTypeId);
        row["udc1"].ToString().Should().Be("TEST-NEW-UDC");
        row["modified_by"].ToString().Should().Be("TEST-USER");
    }

    [SkippableFact]
    public async Task Parts_GetDeleteImpact_ShouldCountQueueHistoryAndInventoryRows()
    {
        SkipWhenUnavailable();

        var partId = await CreatePartAsync();
        await SeedQueueRowAsync(partId);
        await SeedHistoryRowAsync(partId);
        await ExecuteSqlAsync(
            "INSERT INTO dunnage_requires_inventory (part_id, inventory_method, created_by, created_date) "
            + "VALUES (@partId, 'Manual', 'TEST-USER', NOW());",
            new MySqlParameter("@partId", partId));

        var impact = await QueryAsync(
            "sp_Dunnage_Parts_GetDeleteImpact",
            Parameter("p_part_id", partId));

        var row = impact.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        ToInt(row["label_data_count"]).Should().Be(1);
        ToInt(row["history_count"]).Should().Be(1);
        ToInt(row["inventory_count"]).Should().Be(1);
    }

    [SkippableFact]
    public async Task Types_GetDeleteImpact_ShouldCountPartsQueueAndHistoryRows()
    {
        SkipWhenUnavailable();

        var partId = await CreatePartAsync();
        var typeId = _typeIds[0];
        await SeedQueueRowAsync(partId);
        await SeedHistoryRowAsync(partId);

        var impact = await QueryAsync(
            "sp_Dunnage_Types_GetDeleteImpact",
            Parameter("p_type_id", typeId));

        var row = impact.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        ToInt(row["part_count"]).Should().BeGreaterThanOrEqualTo(1);
        ToInt(row["label_data_count"]).Should().BeGreaterThanOrEqualTo(1);
        ToInt(row["history_count"]).Should().BeGreaterThanOrEqualTo(1);
    }

    [SkippableFact]
    public async Task LabelHistory_GetForReprint_ShouldFlagRowsAlreadyQueuedForReprint()
    {
        SkipWhenUnavailable();

        var loadUuid = Guid.NewGuid().ToString();
        var partId = await CreatePartAsync();
        await SeedHistoryRowAsync(partId, loadUuid);
        await ExecuteSqlAsync(
            "INSERT INTO dunnage_label_data "
            + "(load_uuid, part_id, quantity, quantity_type, received_date, user_id, is_reprint) "
            + "VALUES (@loadUuid, @partId, 1, 'Quantity', NOW(), 'TEST-USER', 1);",
            new MySqlParameter("@loadUuid", loadUuid),
            new MySqlParameter("@partId", partId));

        var rows = await QueryAsync(
            "sp_Dunnage_LabelHistory_GetForReprint",
            Parameter("p_start_date", null),
            Parameter("p_end_date", null),
            Parameter("p_search_by", "part"),
            Parameter("p_search_text", partId));

        var row = rows.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        ToInt(row["already_queued"]).Should().Be(1);
        row["load_uuid"].ToString().Should().Be(loadUuid);
    }

    private async Task<int> CreateTypeAsync()
    {
        var typeName = _token + Guid.NewGuid().ToString("N")[..6];
        await ExecuteSqlAsync(
            "INSERT INTO dunnage_types (type_name, icon, created_by, created_date) "
            + "VALUES (@typeName, 'PackageVariantClosed', 'TEST-USER', NOW());",
            new MySqlParameter("@typeName", typeName));

        var typeId = ToInt(
            await ExecuteScalarAsync(
                "SELECT id FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)));

        _typeIds.Add(typeId);
        return typeId;
    }

    private async Task<string> CreatePartAsync()
    {
        var typeId = _typeIds.Count == 0 ? await CreateTypeAsync() : _typeIds[0];
        var partId = UniqueToken("TEST-PART-");

        await ExecuteSqlAsync(
            "INSERT INTO dunnage_parts (part_id, type_id, quantity_type, created_by, created_date) "
            + "VALUES (@partId, @typeId, 'Quantity', 'TEST-USER', NOW());",
            new MySqlParameter("@partId", partId),
            new MySqlParameter("@typeId", typeId));

        return partId;
    }

    private async Task<int> CreateCustomFieldAsync()
    {
        var typeId = _typeIds.Count == 0 ? await CreateTypeAsync() : _typeIds[0];

        await ExecuteSqlAsync(
            "INSERT INTO dunnage_custom_fields "
            + "(DunnageTypeID, FieldName, FieldType, DisplayOrder, IsRequired, CreatedBy) "
            + "VALUES (@typeId, @fieldName, 'Choices', 1, 0, 'TEST-USER');",
            new MySqlParameter("@typeId", typeId),
            new MySqlParameter("@fieldName", UniqueToken("TEST-FIELD-")));

        return ToInt(
            await ExecuteScalarAsync(
                "SELECT ID FROM dunnage_custom_fields WHERE DunnageTypeID = @typeId "
                + "ORDER BY ID DESC LIMIT 1;",
                new MySqlParameter("@typeId", typeId)));
    }

    private async Task SeedQueueRowAsync(string partId)
    {
        await ExecuteSqlAsync(
            "INSERT INTO dunnage_label_data "
            + "(load_uuid, part_id, quantity, quantity_type, received_date, user_id) "
            + "VALUES (@loadUuid, @partId, 1, 'Quantity', NOW(), 'TEST-USER');",
            new MySqlParameter("@loadUuid", Guid.NewGuid().ToString()),
            new MySqlParameter("@partId", partId));
    }

    private async Task SeedHistoryRowAsync(string partId, string? loadUuid = null)
    {
        await ExecuteSqlAsync(
            "INSERT INTO dunnage_history "
            + "(load_uuid, part_id, quantity, quantity_type, received_date, created_by, "
            + "created_date, user_id) "
            + "VALUES (@loadUuid, @partId, 1, 'Quantity', NOW(), 'TEST-USER', NOW(), 'TEST-USER');",
            new MySqlParameter("@loadUuid", loadUuid ?? Guid.NewGuid().ToString()),
            new MySqlParameter("@partId", partId));
    }
}

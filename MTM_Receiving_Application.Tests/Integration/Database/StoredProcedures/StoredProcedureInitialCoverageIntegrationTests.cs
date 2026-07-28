using System.Data;
using FluentAssertions;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedures;

public sealed class StoredProcedureInitialCoverageIntegrationTests
    : StoredProcedureIntegrationTestBase
{
    [Fact]
    public async Task sp_Receiving_Load_Delete_ShouldRemoveSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix();
        var loadGuid = Guid.NewGuid().ToString();

        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"
INSERT INTO receiving_history
(
    load_guid,
    quantity,
    part_id,
    po_number,
    employee_number,
    heat,
    transaction_date,
    initial_location,
    coils_on_skid,
    label_number,
    vendor_name,
    part_description
)
VALUES
(
    @loadGuid,
    1,
    @partId,
    @poNumber,
    9001,
    'TEST-HEAT',
    @transactionDate,
    'RECV',
    1,
    1,
    'Integration Vendor',
    'Integration Test Part'
);",
            new MySqlParameter("@loadGuid", loadGuid),
            new MySqlParameter("@partId", $"TEST-PART-{suffix}"),
            new MySqlParameter("@poNumber", $"PO-{suffix[..12]}"),
            new MySqlParameter("@transactionDate", DateTime.UtcNow.Date)
        );

        var historyRecordId = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT id FROM receiving_history WHERE load_guid = @loadGuid;",
                new MySqlParameter("@loadGuid", loadGuid)
            )
        );

        historyRecordId.Should().BeGreaterThan(0);

        await ExecuteStoredProcedureNonQueryAsync(
            connectionString,
            "sp_Receiving_Load_Delete",
            new MySqlParameter("p_LoadID", loadGuid),
            new MySqlParameter("p_HistoryRecordID", historyRecordId)
        );

        var remainingRows = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COUNT(*) FROM receiving_history WHERE id = @historyRecordId;",
                new MySqlParameter("@historyRecordId", historyRecordId)
            )
        );

        remainingRows.Should().Be(0);
    }

    [Fact]
    public async Task sp_Receiving_Load_Insert_ShouldInsertHistoryRow_AndDeleteCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix();
        var loadGuid = Guid.NewGuid().ToString();
        int historyRecordId = 0;

        try
        {
            var insertResult = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_Load_Insert",
                new MySqlParameter("p_LoadGuid", loadGuid),
                new MySqlParameter("p_Quantity", 2),
                new MySqlParameter("p_PartID", $"TEST-PART-{suffix}"),
                new MySqlParameter("p_PONumber", $"PO-{suffix[..12]}"),
                new MySqlParameter("p_EmployeeNumber", 9002),
                new MySqlParameter("p_Heat", "TEST-HEAT"),
                new MySqlParameter("p_TransactionDate", DateTime.UtcNow.Date),
                new MySqlParameter("p_InitialLocation", "RECV"),
                new MySqlParameter("p_CoilsOnSkid", 1),
                new MySqlParameter("p_LabelNumber", 1),
                new MySqlParameter("p_VendorName", "Integration Vendor"),
                new MySqlParameter("p_PartDescription", "Inserted by integration test")
            );

            insertResult.Rows.Count.Should().Be(1);
            historyRecordId = ConvertToInt(insertResult.Rows[0]["new_id"]);
            historyRecordId.Should().BeGreaterThan(0);

            var deleteResult = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_Load_Delete",
                new MySqlParameter("p_LoadID", loadGuid),
                new MySqlParameter("p_HistoryRecordID", historyRecordId)
            );

            deleteResult.AffectedRows.Should().BeGreaterThan(0);

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_history WHERE id = @historyRecordId;",
                    new MySqlParameter("@historyRecordId", historyRecordId)
                )
            );

            remainingRows.Should().Be(0);
        }
        finally
        {
            if (historyRecordId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_history WHERE id = @historyRecordId;",
                    new MySqlParameter("@historyRecordId", historyRecordId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_LabelData_GetAll_ShouldReturnSeededQueueRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadId = Guid.NewGuid().ToString();

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_label_data
(
    load_id,
    load_number,
    quantity,
    part_id,
    employee_number,
    transaction_date,
    user_id,
    received_date
)
VALUES
(
    @loadId,
    501,
    3,
    'TEST-PART-QUEUE',
    9003,
    @transactionDate,
    'integration.user',
    @receivedDate
);",
                new MySqlParameter("@loadId", loadId),
                new MySqlParameter("@transactionDate", DateTime.UtcNow.Date),
                new MySqlParameter("@receivedDate", DateTime.UtcNow)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_LabelData_GetAll"
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["load_id"]?.ToString(),
                        loadId,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["part_id"].Should().Be("TEST-PART-QUEUE");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_label_data WHERE load_id = @loadId;",
                new MySqlParameter("@loadId", loadId)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_LabelData_Insert_ShouldInsertQueueRow_AndDeleteCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadId = Guid.NewGuid().ToString();
        int labelDataRecordId = 0;

        try
        {
            var insertResult = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_LabelData_Insert",
                new MySqlParameter("p_load_id", loadId),
                new MySqlParameter("p_load_number", 601),
                new MySqlParameter("p_quantity", 8),
                new MySqlParameter("p_weight_quantity", 8.5m),
                new MySqlParameter("p_part_id", "TEST-LABEL-PART"),
                new MySqlParameter("p_part_description", "Label data integration test"),
                new MySqlParameter("p_part_type", "Standard"),
                new MySqlParameter("p_po_number", "PO-LABEL-001"),
                new MySqlParameter("p_po_line_number", "1"),
                new MySqlParameter("p_po_vendor", "Integration Vendor"),
                new MySqlParameter("p_po_status", "Open"),
                new MySqlParameter("p_po_due_date", DateTime.UtcNow.Date),
                new MySqlParameter("p_qty_ordered", 8.0m),
                new MySqlParameter("p_unit_of_measure", "EA"),
                new MySqlParameter("p_remaining_quantity", 0),
                new MySqlParameter("p_employee_number", 9004),
                new MySqlParameter("p_user_id", "integration.user"),
                new MySqlParameter("p_heat", "TEST-HEAT"),
                new MySqlParameter("p_received_date", DateTime.UtcNow),
                new MySqlParameter("p_transaction_date", DateTime.UtcNow.Date),
                new MySqlParameter("p_initial_location", "RECV"),
                new MySqlParameter("p_packages_per_load", 1),
                new MySqlParameter("p_package_type_name", "Skid"),
                new MySqlParameter("p_weight_per_package", 8.5m),
                new MySqlParameter("p_coils_on_skid", 0),
                new MySqlParameter("p_label_number", 1),
                new MySqlParameter("p_vendor_name", "Integration Vendor"),
                new MySqlParameter("p_is_non_po_item", 0),
                new MySqlParameter("p_is_quality_hold_required", 0),
                new MySqlParameter("p_is_quality_hold_acknowledged", 0),
                new MySqlParameter("p_quality_hold_restriction_type", DBNull.Value),
                new MySqlParameter("p_part_skid_sequence", 1),
                new MySqlParameter("p_part_skid_total", 1)
            );

            insertResult.AffectedRows.Should().BeGreaterThanOrEqualTo(0);

            labelDataRecordId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM receiving_label_data WHERE load_id = @loadId;",
                    new MySqlParameter("@loadId", loadId)
                )
            );

            labelDataRecordId.Should().BeGreaterThan(0);

            var deleteResult = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_LabelData_Delete",
                new MySqlParameter("p_label_data_record_id", labelDataRecordId),
                new MySqlParameter("p_load_id", loadId)
            );

            deleteResult.AffectedRows.Should().BeGreaterThan(0);

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_label_data WHERE id = @labelDataRecordId;",
                    new MySqlParameter("@labelDataRecordId", labelDataRecordId)
                )
            );

            remainingRows.Should().Be(0);
        }
        finally
        {
            if (labelDataRecordId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_label_data WHERE id = @labelDataRecordId;",
                    new MySqlParameter("@labelDataRecordId", labelDataRecordId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_LabelData_Delete_ShouldRemoveSeededQueueRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadId = Guid.NewGuid().ToString();

        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"
INSERT INTO receiving_label_data
(
    load_id,
    load_number,
    quantity,
    part_id,
    employee_number,
    transaction_date,
    user_id,
    received_date
)
VALUES
(
    @loadId,
    701,
    2,
    'TEST-LABEL-DELETE',
    9005,
    @transactionDate,
    'integration.user',
    @receivedDate
);",
            new MySqlParameter("@loadId", loadId),
            new MySqlParameter("@transactionDate", DateTime.UtcNow.Date),
            new MySqlParameter("@receivedDate", DateTime.UtcNow)
        );

        var labelDataRecordId = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT id FROM receiving_label_data WHERE load_id = @loadId;",
                new MySqlParameter("@loadId", loadId)
            )
        );

        labelDataRecordId.Should().BeGreaterThan(0);

        await ExecuteStoredProcedureNonQueryAsync(
            connectionString,
            "sp_Receiving_LabelData_Delete",
            new MySqlParameter("p_label_data_record_id", labelDataRecordId),
            new MySqlParameter("p_load_id", loadId)
        );

        var remainingRows = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COUNT(*) FROM receiving_label_data WHERE id = @labelDataRecordId;",
                new MySqlParameter("@labelDataRecordId", labelDataRecordId)
            )
        );

        remainingRows.Should().Be(0);
    }

    [Fact]
    public async Task sp_Receiving_PackageTypeMappings_Insert_ShouldInsertMapping_AndDeleteCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..6].ToUpperInvariant();
        var partPrefix = $"T{suffix}";
        int mappingId = 0;

        try
        {
            var insertResult = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypeMappings_Insert",
                new MySqlParameter("p_part_prefix", partPrefix),
                new MySqlParameter("p_package_type", "IntegrationPackage"),
                new MySqlParameter("p_is_default", 0),
                new MySqlParameter("p_display_order", 501),
                new MySqlParameter("p_created_by", 9006)
            );

            insertResult.Rows.Count.Should().Be(1);
            mappingId = ConvertToInt(insertResult.Rows[0]["id"]);
            mappingId.Should().BeGreaterThan(0);

            var deleteResult = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypeMappings_Delete",
                new MySqlParameter("p_id", mappingId)
            );

            deleteResult.Rows.Count.Should().Be(1);
            ConvertToInt(deleteResult.Rows[0]["affected_rows"]).Should().Be(1);
            deleteResult.Rows[0]["status"].Should().Be("OK");

            var activeRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_package_type_mapping WHERE id = @mappingId AND is_active = 1;",
                    new MySqlParameter("@mappingId", mappingId)
                )
            );

            activeRows.Should().Be(0);
        }
        finally
        {
            if (mappingId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_package_type_mapping WHERE id = @mappingId;",
                    new MySqlParameter("@mappingId", mappingId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_PackageTypeMappings_Delete_ShouldSoftDeleteSeededMapping_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..6].ToUpperInvariant();
        var partPrefix = $"D{suffix}";
        int mappingId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_package_type_mapping
(
    part_prefix,
    package_type,
    is_default,
    display_order,
    is_active,
    created_by
)
VALUES
(
    @partPrefix,
    'DeleteIntegrationPackage',
    0,
    611,
    1,
    9007
);",
                new MySqlParameter("@partPrefix", partPrefix)
            );

            mappingId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM receiving_package_type_mapping WHERE part_prefix = @partPrefix;",
                    new MySqlParameter("@partPrefix", partPrefix)
                )
            );

            mappingId.Should().BeGreaterThan(0);

            var deleteResult = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypeMappings_Delete",
                new MySqlParameter("p_id", mappingId)
            );

            deleteResult.Rows.Count.Should().Be(1);
            ConvertToInt(deleteResult.Rows[0]["affected_rows"]).Should().Be(1);
            deleteResult.Rows[0]["status"].Should().Be("OK");

            var activeRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_package_type_mapping WHERE id = @mappingId AND is_active = 1;",
                    new MySqlParameter("@mappingId", mappingId)
                )
            );

            activeRows.Should().Be(0);
        }
        finally
        {
            if (mappingId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_package_type_mapping WHERE id = @mappingId;",
                    new MySqlParameter("@mappingId", mappingId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_PackageTypeMappings_GetAll_ShouldReturnSeededMapping_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..6].ToUpperInvariant();
        var partPrefix = $"G{suffix}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_package_type_mapping
(
    part_prefix,
    package_type,
    is_default,
    display_order,
    is_active,
    created_by
)
VALUES
(
    @partPrefix,
    'GetAllIntegrationPackage',
    0,
    621,
    1,
    9008
);",
                new MySqlParameter("@partPrefix", partPrefix)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypeMappings_GetAll",
                new MySqlParameter("p_includeInactive", 0)
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["part_prefix"]?.ToString(),
                        partPrefix,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["package_type"].Should().Be("GetAllIntegrationPackage");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_package_type_mapping WHERE part_prefix = @partPrefix;",
                new MySqlParameter("@partPrefix", partPrefix)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_PackageTypeMappings_GetByPrefix_ShouldReturnMatchingPackageType_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..5].ToUpperInvariant();
        var partPrefix = $"P{suffix}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_package_type_mapping
(
    part_prefix,
    package_type,
    is_default,
    display_order,
    is_active,
    created_by
)
VALUES
(
    @partPrefix,
    'PrefixIntegrationPackage',
    0,
    631,
    1,
    9009
);",
                new MySqlParameter("@partPrefix", partPrefix)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypeMappings_GetByPrefix",
                new MySqlParameter("p_part_identifier", $"{partPrefix}-12345")
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["package_type"].Should().Be("PrefixIntegrationPackage");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_package_type_mapping WHERE part_prefix = @partPrefix;",
                new MySqlParameter("@partPrefix", partPrefix)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_PackageTypePreference_Get_ShouldReturnSeededPreference_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partId = $"TEST-PREF-GET-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_package_types
(
    PartID,
    PackageTypeName,
    CustomTypeName,
    LastModified
)
VALUES
(
    @partId,
    'Custom',
    'IntegrationCustomType',
    @lastModified
);",
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@lastModified", DateTime.UtcNow)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypePreference_Get",
                new MySqlParameter("p_PartID", partId)
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["PartID"].Should().Be(partId);
            result.Rows[0]["CustomTypeName"].Should().Be("IntegrationCustomType");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_package_types WHERE PartID = @partId;",
                new MySqlParameter("@partId", partId)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_PackageTypePreference_Save_ShouldPersistPreference_AndDeleteCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partId = $"TEST-PREF-SAVE-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypePreference_Save",
                new MySqlParameter("p_PartID", partId),
                new MySqlParameter("p_PackageTypeName", "Custom"),
                new MySqlParameter("p_CustomTypeName", "SavedIntegrationType"),
                new MySqlParameter("p_LastModified", DateTime.UtcNow)
            );

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_package_types WHERE PartID = @partId;",
                    new MySqlParameter("@partId", partId)
                )
            );

            savedRows.Should().Be(1);

            var deleteResult = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypePreference_Delete",
                new MySqlParameter("p_PartID", partId)
            );

            deleteResult.AffectedRows.Should().BeGreaterThan(0);

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_package_types WHERE PartID = @partId;",
                    new MySqlParameter("@partId", partId)
                )
            );

            remainingRows.Should().Be(0);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_package_types WHERE PartID = @partId;",
                new MySqlParameter("@partId", partId)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_NonPO_GetAll_ShouldReturnSeededEntry_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var value = $"NONPO-GET-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_non_po_entries (value, created_by, use_count)
VALUES (@value, 'integration.user', 1);",
                new MySqlParameter("@value", value)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_NonPO_GetAll"
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["value"]?.ToString(),
                        value,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["created_by"].Should().Be("integration.user");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_non_po_entries WHERE value = @value;",
                new MySqlParameter("@value", value)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_NonPO_Upsert_ShouldPersistEntry_AndDeleteCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var value = $"NONPO-SAVE-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";
        int entryId = 0;

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_NonPO_Upsert",
                new MySqlParameter("p_value", value),
                new MySqlParameter("p_created_by", "integration.user")
            );

            entryId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM receiving_non_po_entries WHERE value = @value;",
                    new MySqlParameter("@value", value)
                )
            );

            entryId.Should().BeGreaterThan(0);

            var deleteResult = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_NonPO_Delete",
                new MySqlParameter("p_id", entryId)
            );

            deleteResult.AffectedRows.Should().BeGreaterThan(0);

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_non_po_entries WHERE id = @entryId;",
                    new MySqlParameter("@entryId", entryId)
                )
            );

            remainingRows.Should().Be(0);
        }
        finally
        {
            if (entryId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_non_po_entries WHERE id = @entryId;",
                    new MySqlParameter("@entryId", entryId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_NonPO_Delete_ShouldRemoveSeededEntry_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var value = $"NONPO-DELETE-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";

        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"
INSERT INTO receiving_non_po_entries (value, created_by, use_count)
VALUES (@value, 'integration.user', 1);",
            new MySqlParameter("@value", value)
        );

        var entryId = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT id FROM receiving_non_po_entries WHERE value = @value;",
                new MySqlParameter("@value", value)
            )
        );

        entryId.Should().BeGreaterThan(0);

        await ExecuteStoredProcedureNonQueryAsync(
            connectionString,
            "sp_Receiving_NonPO_Delete",
            new MySqlParameter("p_id", entryId)
        );

        var remainingRows = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COUNT(*) FROM receiving_non_po_entries WHERE id = @entryId;",
                new MySqlParameter("@entryId", entryId)
            )
        );

        remainingRows.Should().Be(0);
    }

    [Fact]
    public async Task sp_Receiving_NonPO_PartDefault_GetByPartId_ShouldReturnSeededDefault_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partId = $"PART-DEFAULT-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_non_po_part_defaults (part_id, value, updated_by)
VALUES (@partId, 'DEFAULT-REFERENCE', 'integration.user');",
                new MySqlParameter("@partId", partId)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_NonPO_PartDefault_GetByPartId",
                new MySqlParameter("p_part_id", partId)
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["value"].Should().Be("DEFAULT-REFERENCE");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_non_po_part_defaults WHERE part_id = @partId;",
                new MySqlParameter("@partId", partId)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_NonPO_PartDefault_Upsert_ShouldPersistDefault_AndDirectCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partId = $"PART-SAVE-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_NonPO_PartDefault_Upsert",
                new MySqlParameter("p_part_id", partId),
                new MySqlParameter("p_value", "SAVED-REFERENCE"),
                new MySqlParameter("p_updated_by", "integration.user")
            );

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_non_po_part_defaults WHERE part_id = @partId;",
                    new MySqlParameter("@partId", partId)
                )
            );

            savedRows.Should().Be(1);

            var savedValue = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT value FROM receiving_non_po_part_defaults WHERE part_id = @partId;",
                new MySqlParameter("@partId", partId)
            );

            savedValue.Should().Be("SAVED-REFERENCE");
        }
        finally
        {
            var deletedRows = await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_non_po_part_defaults WHERE part_id = @partId;",
                new MySqlParameter("@partId", partId)
            );

            deletedRows.Should().Be(1);
        }
    }

    [Fact]
    public async Task sp_Auth_Department_GetAll_ShouldReturnSeededDepartment_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var departmentName = $"IntegrationDept{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO departments (department_name, is_active, sort_order)
VALUES (@departmentName, 1, 998);",
                new MySqlParameter("@departmentName", departmentName)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_Department_GetAll"
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["department_name"]?.ToString(),
                        departmentName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            ConvertToInt(matchingRow!["sort_order"]).Should().Be(998);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM departments WHERE department_name = @departmentName;",
                new MySqlParameter("@departmentName", departmentName)
            );
        }
    }

    [Fact]
    public async Task sp_Auth_User_GetByWindowsUsername_ShouldReturnSeededUser_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.user.{suffix}";

        try
        {
            await InsertAuthUserAsync(
                connectionString,
                windowsUsername,
                $"Integration User {suffix}",
                "PIN-GET",
                "guided"
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_GetByWindowsUsername",
                new MySqlParameter("p_windows_username", windowsUsername)
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["windows_username"].Should().Be(windowsUsername);
            result.Rows[0]["default_receiving_mode"].Should().Be("guided");
        }
        finally
        {
            await DeleteAuthUserAsync(connectionString, windowsUsername);
        }
    }

    [Fact]
    public async Task sp_Auth_User_GetDefaultMode_ShouldReturnSeededMode_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.mode.{suffix}";

        try
        {
            var employeeNumber = await InsertAuthUserAsync(
                connectionString,
                windowsUsername,
                $"Integration Mode {suffix}",
                "PIN-MODE",
                "manual"
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_GetDefaultMode",
                new MySqlParameter("p_user_id", employeeNumber)
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["default_receiving_mode"].Should().Be("manual");
        }
        finally
        {
            await DeleteAuthUserAsync(connectionString, windowsUsername);
        }
    }

    [Fact]
    public async Task sp_Auth_User_IsWindowsUsernameUnique_ShouldReportExistingUser_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.unique.{suffix}";

        try
        {
            await InsertAuthUserAsync(
                connectionString,
                windowsUsername,
                $"Integration Unique {suffix}",
                "PIN-UNIQUE",
                "guided"
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_IsWindowsUsernameUnique",
                new MySqlParameter("p_windows_username", windowsUsername),
                new MySqlParameter("p_exclude_employee_number", DBNull.Value)
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["username_count"]).Should().Be(1);
        }
        finally
        {
            await DeleteAuthUserAsync(connectionString, windowsUsername);
        }
    }

    [Fact]
    public async Task sp_Auth_User_ValidatePin_ShouldReturnSeededUser_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.pin.{suffix}";

        try
        {
            await InsertAuthUserAsync(
                connectionString,
                windowsUsername,
                $"Integration Pin {suffix}",
                "PIN-VALID",
                "guided"
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_ValidatePin",
                new MySqlParameter("p_username", windowsUsername),
                new MySqlParameter("p_pin", "PIN-VALID")
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["windows_username"].Should().Be(windowsUsername);
            result.Rows[0]["pin"].Should().Be("PIN-VALID");
        }
        finally
        {
            await DeleteAuthUserAsync(connectionString, windowsUsername);
        }
    }

    [Fact]
    public async Task sp_Auth_Terminal_GetShared_ShouldReturnSeededSharedTerminal_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var workstationName = $"INT-SHARED-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO auth_workstation_config
(
    workstation_name,
    workstation_type,
    is_active,
    description
)
VALUES
(
    @workstationName,
    'shared_terminal',
    1,
    'Integration shared terminal'
);",
                new MySqlParameter("@workstationName", workstationName)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_Terminal_GetShared"
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["workstation_name"]?.ToString(),
                        workstationName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["workstation_type"].Should().Be("shared_terminal");
        }
        finally
        {
            await DeleteWorkstationAsync(connectionString, workstationName);
        }
    }

    [Fact]
    public async Task sp_Auth_User_Create_ShouldCreateUser_AndDirectCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.create.{suffix}";
        var employeeNumber = 700000 + Random.Shared.Next(10000, 99999);
        var errorParameter = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Auth_User_Create",
                new MySqlParameter("p_employee_number", employeeNumber),
                new MySqlParameter("p_windows_username", windowsUsername),
                new MySqlParameter("p_full_name", $"Integration Create {suffix}"),
                new MySqlParameter("p_pin", "PIN-CREATE"),
                new MySqlParameter("p_department", "Receiving"),
                new MySqlParameter("p_shift", "1st Shift"),
                new MySqlParameter("p_created_by", "integration.user"),
                new MySqlParameter("p_visual_username", DBNull.Value),
                new MySqlParameter("p_visual_password", DBNull.Value),
                errorParameter
            );

            result.OutputValues["p_error_message"].Should().BeNull();

            var createdRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM auth_users WHERE employee_number = @employeeNumber;",
                    new MySqlParameter("@employeeNumber", employeeNumber)
                )
            );

            createdRows.Should().Be(1);
        }
        finally
        {
            await DeleteAuthUserByEmployeeNumberAsync(connectionString, employeeNumber);
        }
    }

    [Fact]
    public async Task sp_Auth_User_Upsert_ShouldPersistUser_AndDirectCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.upsert.{suffix}";
        var employeeNumber = 800000 + Random.Shared.Next(10000, 99999);

        try
        {
            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_Upsert",
                new MySqlParameter("p_employee_number", employeeNumber),
                new MySqlParameter("p_windows_username", windowsUsername),
                new MySqlParameter("p_full_name", $"Integration Upsert {suffix}"),
                new MySqlParameter("p_pin", "PIN-UPSERT"),
                new MySqlParameter("p_department", "Receiving"),
                new MySqlParameter("p_shift", "1st Shift"),
                new MySqlParameter("p_is_active", 1),
                new MySqlParameter("p_visual_username", DBNull.Value),
                new MySqlParameter("p_visual_password", DBNull.Value),
                new MySqlParameter("p_created_by", "integration.user")
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["success"]).Should().Be(1);

            var createdRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM auth_users WHERE employee_number = @employeeNumber;",
                    new MySqlParameter("@employeeNumber", employeeNumber)
                )
            );

            createdRows.Should().Be(1);
        }
        finally
        {
            await DeleteAuthUserByEmployeeNumberAsync(connectionString, employeeNumber);
        }
    }

    [Fact]
    public async Task sp_Auth_Workstation_Upsert_ShouldPersistWorkstation_AndDirectCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var workstationName = $"INT-WS-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Auth_Workstation_Upsert",
                new MySqlParameter("p_workstation_name", workstationName),
                new MySqlParameter("p_workstation_type", "shared_terminal"),
                new MySqlParameter("p_is_active", 1),
                new MySqlParameter("p_description", "Persisted by integration test")
            );

            var persistedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM auth_workstation_config WHERE workstation_name = @workstationName;",
                    new MySqlParameter("@workstationName", workstationName)
                )
            );

            persistedRows.Should().Be(1);
        }
        finally
        {
            var deletedRows = await DeleteWorkstationAsync(connectionString, workstationName);
            deletedRows.Should().Be(1);
        }
    }

    [Fact]
    public async Task sp_Auth_User_Deactivate_ShouldDeactivateSeededUser_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.deactivate.{suffix}";
        var employeeNumber = await InsertAuthUserAsync(
            connectionString,
            windowsUsername,
            $"Integration Deactivate {suffix}",
            "PIN-DEACT",
            "guided"
        );
        var errorParameter = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Auth_User_Deactivate",
                new MySqlParameter("p_employee_number", employeeNumber),
                new MySqlParameter("p_updated_by", windowsUsername),
                errorParameter
            );

            result.OutputValues["p_error_message"].Should().BeNull();

            var isActive = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT is_active FROM auth_users WHERE employee_number = @employeeNumber;",
                    new MySqlParameter("@employeeNumber", employeeNumber)
                )
            );

            isActive.Should().Be(0);

            var auditCount = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    @"SELECT COUNT(*)
FROM settings_personal_activity_log
WHERE event_type = 'user_deactivated'
  AND username = @windowsUsername
  AND details = @details;",
                    new MySqlParameter("@windowsUsername", windowsUsername),
                    new MySqlParameter(
                        "@details",
                        $"User deactivated: employee_number={employeeNumber}"
                    )
                )
            );

            auditCount.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"DELETE FROM settings_personal_activity_log
WHERE event_type = 'user_deactivated'
  AND username = @windowsUsername
  AND details = @details;",
                new MySqlParameter("@windowsUsername", windowsUsername),
                new MySqlParameter(
                    "@details",
                    $"User deactivated: employee_number={employeeNumber}"
                )
            );

            await DeleteAuthUserByEmployeeNumberAsync(connectionString, employeeNumber);
        }
    }

    [Fact]
    public async Task sp_Settings_ReportingRecipients_GetAll_ShouldReturnSeededRecipient_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var email = $"reporting.get.{CreateUniqueSuffix()[..8]}@example.com";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO settings_reporting_recipients (first_name, last_name, recipient_type, email)
VALUES ('Reporting', 'GetAll', 'To', @email);",
                new MySqlParameter("@email", email)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ReportingRecipients_GetAll"
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["email"]?.ToString(),
                        email,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["first_name"].Should().Be("Reporting");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM settings_reporting_recipients WHERE email = @email;",
                new MySqlParameter("@email", email)
            );
        }
    }

    [Fact]
    public async Task sp_Settings_ReportingRecipients_Insert_ShouldInsertRecipient_AndDeleteCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var email = $"reporting.insert.{CreateUniqueSuffix()[..8]}@example.com";
        int recipientId = 0;

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Settings_ReportingRecipients_Insert",
                new MySqlParameter("p_first_name", "Reporting"),
                new MySqlParameter("p_last_name", "Insert"),
                new MySqlParameter("p_recipient_type", "To"),
                new MySqlParameter("p_email", email)
            );

            recipientId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM settings_reporting_recipients WHERE email = @email;",
                    new MySqlParameter("@email", email)
                )
            );

            recipientId.Should().BeGreaterThan(0);

            var deleteResult = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Settings_ReportingRecipients_Delete",
                new MySqlParameter("p_id", recipientId)
            );

            deleteResult.AffectedRows.Should().BeGreaterThan(0);

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM settings_reporting_recipients WHERE id = @recipientId;",
                    new MySqlParameter("@recipientId", recipientId)
                )
            );

            remainingRows.Should().Be(0);
        }
        finally
        {
            if (recipientId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM settings_reporting_recipients WHERE id = @recipientId;",
                    new MySqlParameter("@recipientId", recipientId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Settings_ReportingRecipients_Update_ShouldUpdateSeededRecipient_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var originalEmail = $"reporting.update.{CreateUniqueSuffix()[..8]}@example.com";
        var updatedEmail = $"reporting.updated.{CreateUniqueSuffix()[..8]}@example.com";
        int recipientId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO settings_reporting_recipients (first_name, last_name, recipient_type, email)
VALUES ('Reporting', 'Original', 'To', @email);",
                new MySqlParameter("@email", originalEmail)
            );

            recipientId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM settings_reporting_recipients WHERE email = @email;",
                    new MySqlParameter("@email", originalEmail)
                )
            );

            recipientId.Should().BeGreaterThan(0);

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Settings_ReportingRecipients_Update",
                new MySqlParameter("p_id", recipientId),
                new MySqlParameter("p_first_name", "Reporting"),
                new MySqlParameter("p_last_name", "Updated"),
                new MySqlParameter("p_recipient_type", "CC"),
                new MySqlParameter("p_email", updatedEmail)
            );

            var row = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ReportingRecipients_GetAll"
            );

            var matchingRow = row
                .Rows.Cast<DataRow>()
                .SingleOrDefault(dataRow => ConvertToInt(dataRow["id"]) == recipientId);

            matchingRow.Should().NotBeNull();
            matchingRow!["last_name"].Should().Be("Updated");
            matchingRow["recipient_type"].Should().Be("CC");
            matchingRow["email"].Should().Be(updatedEmail);
        }
        finally
        {
            if (recipientId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM settings_reporting_recipients WHERE id = @recipientId;",
                    new MySqlParameter("@recipientId", recipientId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Settings_ReportingRecipients_Delete_ShouldRemoveSeededRecipient_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var email = $"reporting.delete.{CreateUniqueSuffix()[..8]}@example.com";

        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"
INSERT INTO settings_reporting_recipients (first_name, last_name, recipient_type, email)
VALUES ('Reporting', 'Delete', 'To', @email);",
            new MySqlParameter("@email", email)
        );

        var recipientId = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT id FROM settings_reporting_recipients WHERE email = @email;",
                new MySqlParameter("@email", email)
            )
        );

        recipientId.Should().BeGreaterThan(0);

        await ExecuteStoredProcedureNonQueryAsync(
            connectionString,
            "sp_Settings_ReportingRecipients_Delete",
            new MySqlParameter("p_id", recipientId)
        );

        var remainingRows = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COUNT(*) FROM settings_reporting_recipients WHERE id = @recipientId;",
                new MySqlParameter("@recipientId", recipientId)
            )
        );

        remainingRows.Should().Be(0);
    }

    [Fact]
    public async Task sp_Settings_VolvoRecipients_GetAll_ShouldReturnSeededRecipient_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var email = $"volvo.get.{CreateUniqueSuffix()[..8]}@example.com";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO settings_volvo_recipents (first_name, last_name, recipient_type, email)
VALUES ('Volvo', 'GetAll', 'To', @email);",
                new MySqlParameter("@email", email)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_VolvoRecipients_GetAll"
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["email"]?.ToString(),
                        email,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["first_name"].Should().Be("Volvo");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM settings_volvo_recipents WHERE email = @email;",
                new MySqlParameter("@email", email)
            );
        }
    }

    [Fact]
    public async Task sp_Settings_VolvoRecipients_Insert_ShouldInsertRecipient_AndDeleteCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var email = $"volvo.insert.{CreateUniqueSuffix()[..8]}@example.com";
        int recipientId = 0;

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Settings_VolvoRecipients_Insert",
                new MySqlParameter("p_first_name", "Volvo"),
                new MySqlParameter("p_last_name", "Insert"),
                new MySqlParameter("p_recipient_type", "To"),
                new MySqlParameter("p_email", email)
            );

            recipientId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM settings_volvo_recipents WHERE email = @email;",
                    new MySqlParameter("@email", email)
                )
            );

            recipientId.Should().BeGreaterThan(0);

            var deleteResult = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Settings_VolvoRecipients_Delete",
                new MySqlParameter("p_id", recipientId)
            );

            deleteResult.AffectedRows.Should().BeGreaterThan(0);

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM settings_volvo_recipents WHERE id = @recipientId;",
                    new MySqlParameter("@recipientId", recipientId)
                )
            );

            remainingRows.Should().Be(0);
        }
        finally
        {
            if (recipientId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM settings_volvo_recipents WHERE id = @recipientId;",
                    new MySqlParameter("@recipientId", recipientId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Settings_VolvoRecipients_Update_ShouldUpdateSeededRecipient_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var originalEmail = $"volvo.update.{CreateUniqueSuffix()[..8]}@example.com";
        var updatedEmail = $"volvo.updated.{CreateUniqueSuffix()[..8]}@example.com";
        int recipientId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO settings_volvo_recipents (first_name, last_name, recipient_type, email)
VALUES ('Volvo', 'Original', 'To', @email);",
                new MySqlParameter("@email", originalEmail)
            );

            recipientId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM settings_volvo_recipents WHERE email = @email;",
                    new MySqlParameter("@email", originalEmail)
                )
            );

            recipientId.Should().BeGreaterThan(0);

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Settings_VolvoRecipients_Update",
                new MySqlParameter("p_id", recipientId),
                new MySqlParameter("p_first_name", "Volvo"),
                new MySqlParameter("p_last_name", "Updated"),
                new MySqlParameter("p_recipient_type", "CC"),
                new MySqlParameter("p_email", updatedEmail)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_VolvoRecipients_GetAll"
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row => ConvertToInt(row["id"]) == recipientId);

            matchingRow.Should().NotBeNull();
            matchingRow!["last_name"].Should().Be("Updated");
            matchingRow["recipient_type"].Should().Be("CC");
            matchingRow["email"].Should().Be(updatedEmail);
        }
        finally
        {
            if (recipientId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM settings_volvo_recipents WHERE id = @recipientId;",
                    new MySqlParameter("@recipientId", recipientId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Settings_VolvoRecipients_Delete_ShouldRemoveSeededRecipient_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var email = $"volvo.delete.{CreateUniqueSuffix()[..8]}@example.com";

        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"
INSERT INTO settings_volvo_recipents (first_name, last_name, recipient_type, email)
VALUES ('Volvo', 'Delete', 'To', @email);",
            new MySqlParameter("@email", email)
        );

        var recipientId = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT id FROM settings_volvo_recipents WHERE email = @email;",
                new MySqlParameter("@email", email)
            )
        );

        recipientId.Should().BeGreaterThan(0);

        await ExecuteStoredProcedureNonQueryAsync(
            connectionString,
            "sp_Settings_VolvoRecipients_Delete",
            new MySqlParameter("p_id", recipientId)
        );

        var remainingRows = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COUNT(*) FROM settings_volvo_recipents WHERE id = @recipientId;",
                new MySqlParameter("@recipientId", recipientId)
            )
        );

        remainingRows.Should().Be(0);
    }

    [Fact]
    public async Task sp_SoftwareVersion_GetCurrent_ShouldReturnSeededSingletonRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var originalVersion = await ExecuteSqlScalarAsync(
            connectionString,
            "SELECT required_version FROM software_version WHERE id = 1;"
        );
        var originalUpdatedBy = await ExecuteSqlScalarAsync(
            connectionString,
            "SELECT updated_by FROM software_version WHERE id = 1;"
        );
        var hadOriginalRow =
            ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM software_version WHERE id = 1;"
                )
            ) == 1;
        var testVersion = $"88.88.{Random.Shared.Next(1000, 9999)}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO software_version (id, required_version, updated_by)
VALUES (1, @requiredVersion, 'integration.user')
ON DUPLICATE KEY UPDATE required_version = @requiredVersion, updated_by = 'integration.user';",
                new MySqlParameter("@requiredVersion", testVersion)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_SoftwareVersion_GetCurrent"
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["required_version"].Should().Be(testVersion);
            result.Rows[0]["updated_by"].Should().Be("integration.user");
        }
        finally
        {
            if (hadOriginalRow)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "UPDATE software_version SET required_version = @requiredVersion, updated_by = @updatedBy WHERE id = 1;",
                    new MySqlParameter("@requiredVersion", originalVersion ?? DBNull.Value),
                    new MySqlParameter("@updatedBy", originalUpdatedBy ?? DBNull.Value)
                );
            }
            else
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM software_version WHERE id = 1;"
                );
            }
        }
    }

    [Fact]
    public async Task sp_SoftwareVersion_Upsert_ShouldPersistSingletonRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var originalVersion = await ExecuteSqlScalarAsync(
            connectionString,
            "SELECT required_version FROM software_version WHERE id = 1;"
        );
        var originalUpdatedBy = await ExecuteSqlScalarAsync(
            connectionString,
            "SELECT updated_by FROM software_version WHERE id = 1;"
        );
        var hadOriginalRow =
            ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM software_version WHERE id = 1;"
                )
            ) == 1;
        var testVersion = $"99.99.{Random.Shared.Next(1000, 9999)}";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_SoftwareVersion_Upsert",
                new MySqlParameter("p_required_version", testVersion),
                new MySqlParameter("p_updated_by", "integration.user")
            );

            var savedVersion = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT required_version FROM software_version WHERE id = 1;"
            );
            var savedUpdatedBy = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT updated_by FROM software_version WHERE id = 1;"
            );

            savedVersion.Should().Be(testVersion);
            savedUpdatedBy.Should().Be("integration.user");
        }
        finally
        {
            if (hadOriginalRow)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "UPDATE software_version SET required_version = @requiredVersion, updated_by = @updatedBy WHERE id = 1;",
                    new MySqlParameter("@requiredVersion", originalVersion ?? DBNull.Value),
                    new MySqlParameter("@updatedBy", originalUpdatedBy ?? DBNull.Value)
                );
            }
            else
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM software_version WHERE id = 1;"
                );
            }
        }
    }

    [Fact]
    public async Task sp_Auth_User_GetAll_ShouldReturnSeededUser_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.getall.{suffix}";

        try
        {
            await InsertAuthUserAsync(
                connectionString,
                windowsUsername,
                $"Integration GetAll {suffix}",
                "PIN-GETALL",
                "guided"
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_GetAll"
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["windows_username"]?.ToString(),
                        windowsUsername,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["full_name"].Should().Be($"Integration GetAll {suffix}");
        }
        finally
        {
            await DeleteAuthUserAsync(connectionString, windowsUsername);
        }
    }

    [Fact]
    public async Task sp_Auth_User_Update_ShouldUpdateSeededUserAndWriteAudit_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.update.{suffix}";
        var employeeNumber = await InsertAuthUserAsync(
            connectionString,
            windowsUsername,
            $"Integration Update {suffix}",
            "PIN-OLD",
            "guided"
        );
        var errorParameter = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Auth_User_Update",
                new MySqlParameter("p_employee_number", employeeNumber),
                new MySqlParameter("p_full_name", $"Integration Updated {suffix}"),
                new MySqlParameter("p_pin", "PIN-NEW"),
                new MySqlParameter("p_department", "Shipping"),
                new MySqlParameter("p_shift", "2nd Shift"),
                new MySqlParameter("p_is_active", 1),
                new MySqlParameter("p_visual_username", "visual-user"),
                new MySqlParameter("p_visual_password", "visual-pass"),
                new MySqlParameter("p_updated_by", windowsUsername),
                errorParameter
            );

            result.OutputValues["p_error_message"].Should().BeNull();

            var row = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_GetByWindowsUsername",
                new MySqlParameter("p_windows_username", windowsUsername)
            );

            row.Rows.Count.Should().Be(1);
            row.Rows[0]["full_name"].Should().Be($"Integration Updated {suffix}");
            row.Rows[0]["department"].Should().Be("Shipping");
            row.Rows[0]["shift"].Should().Be("2nd Shift");
            row.Rows[0]["visual_username"].Should().Be("visual-user");

            var auditCount = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    @"SELECT COUNT(*)
FROM settings_personal_activity_log
WHERE event_type = 'user_updated'
  AND username = @username
  AND details = @details;",
                    new MySqlParameter("@username", windowsUsername),
                    new MySqlParameter(
                        "@details",
                        $"User record updated for employee_number={employeeNumber}"
                    )
                )
            );

            auditCount.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"DELETE FROM settings_personal_activity_log
WHERE event_type = 'user_updated'
  AND username = @username
  AND details = @details;",
                new MySqlParameter("@username", windowsUsername),
                new MySqlParameter(
                    "@details",
                    $"User record updated for employee_number={employeeNumber}"
                )
            );

            await DeleteAuthUserByEmployeeNumberAsync(connectionString, employeeNumber);
        }
    }

    [Fact]
    public async Task sp_Auth_User_UpdateVisualCredentials_ShouldUpdateVisualFieldsAndWriteAudit_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.visual.{suffix}";
        var employeeNumber = await InsertAuthUserAsync(
            connectionString,
            windowsUsername,
            $"Integration Visual {suffix}",
            "PIN-VIS",
            "guided"
        );
        var errorParameter = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Auth_User_UpdateVisualCredentials",
                new MySqlParameter("p_employee_number", employeeNumber),
                new MySqlParameter("p_visual_username", "visual.updated"),
                new MySqlParameter("p_visual_password", "visual.secret"),
                new MySqlParameter("p_updated_by", windowsUsername),
                errorParameter
            );

            result.OutputValues["p_error_message"].Should().BeNull();

            var row = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_GetByWindowsUsername",
                new MySqlParameter("p_windows_username", windowsUsername)
            );

            row.Rows.Count.Should().Be(1);
            row.Rows[0]["visual_username"].Should().Be("visual.updated");
            row.Rows[0]["visual_password"].Should().Be("visual.secret");

            var auditCount = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    @"SELECT COUNT(*)
FROM settings_personal_activity_log
WHERE event_type = 'visual_credentials_updated'
  AND username = @username
  AND details = @details;",
                    new MySqlParameter("@username", windowsUsername),
                    new MySqlParameter(
                        "@details",
                        $"Visual credentials updated for employee_number={employeeNumber}"
                    )
                )
            );

            auditCount.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"DELETE FROM settings_personal_activity_log
WHERE event_type = 'visual_credentials_updated'
  AND username = @username
  AND details = @details;",
                new MySqlParameter("@username", windowsUsername),
                new MySqlParameter(
                    "@details",
                    $"Visual credentials updated for employee_number={employeeNumber}"
                )
            );

            await DeleteAuthUserByEmployeeNumberAsync(connectionString, employeeNumber);
        }
    }

    [Fact]
    public async Task sp_Auth_User_SeedDefaultModes_ShouldFillMissingModes_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.seed.{suffix}";
        var employeeNumber = 900000 + Random.Shared.Next(10000, 99999);

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO auth_users
(
    employee_number,
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active,
    default_receiving_mode,
    default_dunnage_mode,
    created_by
)
VALUES
(
    @employeeNumber,
    @windowsUsername,
    @fullName,
    'PIN-SEED',
    'Receiving',
    '1st Shift',
    1,
    NULL,
    NULL,
    'integration.user'
);",
                new MySqlParameter("@employeeNumber", employeeNumber),
                new MySqlParameter("@windowsUsername", windowsUsername),
                new MySqlParameter("@fullName", $"Integration Seed {suffix}")
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_SeedDefaultModes",
                new MySqlParameter("p_user_id", employeeNumber)
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["success"]).Should().Be(1);

            var row = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_GetByWindowsUsername",
                new MySqlParameter("p_windows_username", windowsUsername)
            );

            row.Rows.Count.Should().Be(1);
            row.Rows[0]["default_receiving_mode"].Should().Be("guided");
            row.Rows[0]["default_dunnage_mode"].Should().Be("guided");
        }
        finally
        {
            await DeleteAuthUserByEmployeeNumberAsync(connectionString, employeeNumber);
        }
    }

    [Fact]
    public async Task sp_Auth_User_UpdateDefaultMode_ShouldUpdateReceivingMode_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.defmode.{suffix}";
        var employeeNumber = await InsertAuthUserAsync(
            connectionString,
            windowsUsername,
            $"Integration DefaultMode {suffix}",
            "PIN-MODE2",
            "guided"
        );

        try
        {
            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_UpdateDefaultMode",
                new MySqlParameter("p_user_id", employeeNumber),
                new MySqlParameter("p_default_mode", "manual")
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["affected_rows"]).Should().Be(1);

            var updatedMode = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT default_receiving_mode FROM auth_users WHERE employee_number = @employeeNumber;",
                new MySqlParameter("@employeeNumber", employeeNumber)
            );

            updatedMode.Should().Be("manual");
        }
        finally
        {
            await DeleteAuthUserByEmployeeNumberAsync(connectionString, employeeNumber);
        }
    }

    [Fact]
    public async Task sp_Auth_User_UpdateDefaultReceivingMode_ShouldUpdateReceivingMode_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.defrecv.{suffix}";
        var employeeNumber = await InsertAuthUserAsync(
            connectionString,
            windowsUsername,
            $"Integration DefaultReceiving {suffix}",
            "PIN-RECV",
            "guided"
        );

        try
        {
            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_UpdateDefaultReceivingMode",
                new MySqlParameter("p_user_id", employeeNumber),
                new MySqlParameter("p_default_mode", "edit")
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["affected_rows"]).Should().Be(1);

            var updatedMode = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT default_receiving_mode FROM auth_users WHERE employee_number = @employeeNumber;",
                new MySqlParameter("@employeeNumber", employeeNumber)
            );

            updatedMode.Should().Be("edit");
        }
        finally
        {
            await DeleteAuthUserByEmployeeNumberAsync(connectionString, employeeNumber);
        }
    }

    [Fact]
    public async Task sp_Auth_User_UpdateDefaultDunnageMode_ShouldUpdateDunnageMode_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var windowsUsername = $"integration.defdun.{suffix}";
        var employeeNumber = await InsertAuthUserAsync(
            connectionString,
            windowsUsername,
            $"Integration DefaultDunnage {suffix}",
            "PIN-DUN",
            "guided"
        );

        try
        {
            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_User_UpdateDefaultDunnageMode",
                new MySqlParameter("p_user_id", employeeNumber),
                new MySqlParameter("p_default_mode", "manual")
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["affected_rows"]).Should().Be(1);

            var updatedMode = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT default_dunnage_mode FROM auth_users WHERE employee_number = @employeeNumber;",
                new MySqlParameter("@employeeNumber", employeeNumber)
            );

            updatedMode.Should().Be("manual");
        }
        finally
        {
            await DeleteAuthUserByEmployeeNumberAsync(connectionString, employeeNumber);
        }
    }

    [Fact]
    public async Task sp_Settings_ScheduledReport_GetAll_ShouldReturnSeededActiveReport_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var reportType = $"IntegrationReport-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO reporting_scheduled_reports
(
    report_type,
    schedule,
    email_recipients,
    is_active,
    next_run_date,
    created_by
)
VALUES
(
    @reportType,
    'Daily at 8:00 AM',
    'integration@example.com',
    1,
    @nextRunDate,
    1001
);",
                new MySqlParameter("@reportType", reportType),
                new MySqlParameter("@nextRunDate", DateTime.UtcNow.AddDays(1))
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_GetAll"
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["report_type"]?.ToString(),
                        reportType,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["email_recipients"].Should().Be("integration@example.com");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM reporting_scheduled_reports WHERE report_type = @reportType;",
                new MySqlParameter("@reportType", reportType)
            );
        }
    }

    [Fact]
    public async Task sp_Settings_ScheduledReport_Insert_ShouldInsertReport_AndDeleteCleanupShouldPass_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var reportType = $"IntegrationInsert-{CreateUniqueSuffix()[..8]}";
        int scheduledReportId = 0;

        try
        {
            var insertResult = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_Insert",
                new MySqlParameter("p_report_type", reportType),
                new MySqlParameter("p_schedule", "Weekly Monday 9:00 AM"),
                new MySqlParameter("p_email_recipients", "weekly@example.com"),
                new MySqlParameter("p_next_run_date", DateTime.UtcNow.AddDays(7)),
                new MySqlParameter("p_created_by", 1002)
            );

            insertResult.Rows.Count.Should().Be(1);
            scheduledReportId = ConvertToInt(insertResult.Rows[0]["id"]);
            scheduledReportId.Should().BeGreaterThan(0);

            var deleteResult = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_Delete",
                new MySqlParameter("p_id", scheduledReportId)
            );

            deleteResult.Rows.Count.Should().Be(1);
            ConvertToInt(deleteResult.Rows[0]["affected_rows"]).Should().Be(1);

            var activeRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM reporting_scheduled_reports WHERE id = @scheduledReportId AND is_active = 1;",
                    new MySqlParameter("@scheduledReportId", scheduledReportId)
                )
            );

            activeRows.Should().Be(0);
        }
        finally
        {
            if (scheduledReportId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM reporting_scheduled_reports WHERE id = @scheduledReportId;",
                    new MySqlParameter("@scheduledReportId", scheduledReportId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Settings_ScheduledReport_Delete_ShouldSoftDeleteSeededReport_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var reportType = $"IntegrationDelete-{CreateUniqueSuffix()[..8]}";

        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"
INSERT INTO reporting_scheduled_reports
(
    report_type,
    schedule,
    email_recipients,
    is_active,
    next_run_date,
    created_by
)
VALUES
(
    @reportType,
    'Monthly first day',
    'delete@example.com',
    1,
    @nextRunDate,
    1003
);",
            new MySqlParameter("@reportType", reportType),
            new MySqlParameter("@nextRunDate", DateTime.UtcNow.AddDays(30))
        );

        var scheduledReportId = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT id FROM reporting_scheduled_reports WHERE report_type = @reportType;",
                new MySqlParameter("@reportType", reportType)
            )
        );

        scheduledReportId.Should().BeGreaterThan(0);

        var deleteResult = await ExecuteStoredProcedureQueryAsync(
            connectionString,
            "sp_Settings_ScheduledReport_Delete",
            new MySqlParameter("p_id", scheduledReportId)
        );

        deleteResult.Rows.Count.Should().Be(1);
        ConvertToInt(deleteResult.Rows[0]["affected_rows"]).Should().Be(1);

        var activeRows = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COUNT(*) FROM reporting_scheduled_reports WHERE id = @scheduledReportId AND is_active = 1;",
                new MySqlParameter("@scheduledReportId", scheduledReportId)
            )
        );

        activeRows.Should().Be(0);

        await ExecuteSqlNonQueryAsync(
            connectionString,
            "DELETE FROM reporting_scheduled_reports WHERE id = @scheduledReportId;",
            new MySqlParameter("@scheduledReportId", scheduledReportId)
        );
    }

    [Fact]
    public async Task sp_Settings_ScheduledReport_GetById_ShouldReturnSeededReport_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var reportType = $"IntegrationById-{CreateUniqueSuffix()[..8]}";
        int scheduledReportId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO reporting_scheduled_reports
(
    report_type,
    schedule,
    email_recipients,
    is_active,
    next_run_date,
    created_by
)
VALUES
(
    @reportType,
    'Every hour',
    'byid@example.com',
    1,
    @nextRunDate,
    1004
);",
                new MySqlParameter("@reportType", reportType),
                new MySqlParameter("@nextRunDate", DateTime.UtcNow.AddHours(1))
            );

            scheduledReportId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM reporting_scheduled_reports WHERE report_type = @reportType;",
                    new MySqlParameter("@reportType", reportType)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_GetById",
                new MySqlParameter("p_id", scheduledReportId)
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["report_type"].Should().Be(reportType);
            result.Rows[0]["email_recipients"].Should().Be("byid@example.com");
        }
        finally
        {
            if (scheduledReportId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM reporting_scheduled_reports WHERE id = @scheduledReportId;",
                    new MySqlParameter("@scheduledReportId", scheduledReportId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Settings_ScheduledReport_GetActive_ShouldReturnOnlyActiveReportsWithNextRun_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var activeReportType = $"IntegrationActive-{CreateUniqueSuffix()[..8]}";
        var inactiveReportType = $"IntegrationInactive-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO reporting_scheduled_reports (report_type, schedule, email_recipients, is_active, next_run_date, created_by)
VALUES (@activeReportType, 'Daily', 'active@example.com', 1, @activeNextRun, 1005),
       (@inactiveReportType, 'Daily', 'inactive@example.com', 0, @inactiveNextRun, 1005);",
                new MySqlParameter("@activeReportType", activeReportType),
                new MySqlParameter("@inactiveReportType", inactiveReportType),
                new MySqlParameter("@activeNextRun", DateTime.UtcNow.AddHours(2)),
                new MySqlParameter("@inactiveNextRun", DateTime.UtcNow.AddHours(3))
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_GetActive"
            );

            var rows = result.Rows.Cast<DataRow>().ToList();
            rows.Any(row =>
                    string.Equals(
                        row["report_type"]?.ToString(),
                        activeReportType,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
            rows.Any(row =>
                    string.Equals(
                        row["report_type"]?.ToString(),
                        inactiveReportType,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeFalse();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM reporting_scheduled_reports WHERE report_type IN (@activeReportType, @inactiveReportType);",
                new MySqlParameter("@activeReportType", activeReportType),
                new MySqlParameter("@inactiveReportType", inactiveReportType)
            );
        }
    }

    [Fact]
    public async Task sp_Settings_ScheduledReport_GetDue_ShouldReturnOnlyDueActiveReports_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var dueReportType = $"IntegrationDue-{CreateUniqueSuffix()[..8]}";
        var futureReportType = $"IntegrationFuture-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO reporting_scheduled_reports (report_type, schedule, email_recipients, is_active, next_run_date, created_by)
VALUES (@dueReportType, 'Daily', 'due@example.com', 1, @dueNextRun, 1006),
       (@futureReportType, 'Daily', 'future@example.com', 1, @futureNextRun, 1006);",
                new MySqlParameter("@dueReportType", dueReportType),
                new MySqlParameter("@futureReportType", futureReportType),
                new MySqlParameter("@dueNextRun", DateTime.UtcNow.AddMinutes(-5)),
                new MySqlParameter("@futureNextRun", DateTime.UtcNow.AddHours(5))
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_GetDue"
            );

            var rows = result.Rows.Cast<DataRow>().ToList();
            rows.Any(row =>
                    string.Equals(
                        row["report_type"]?.ToString(),
                        dueReportType,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
            rows.Any(row =>
                    string.Equals(
                        row["report_type"]?.ToString(),
                        futureReportType,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeFalse();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM reporting_scheduled_reports WHERE report_type IN (@dueReportType, @futureReportType);",
                new MySqlParameter("@dueReportType", dueReportType),
                new MySqlParameter("@futureReportType", futureReportType)
            );
        }
    }

    [Fact]
    public async Task sp_Settings_ScheduledReport_ToggleActive_ShouldUpdateActiveFlag_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var reportType = $"IntegrationToggle-{CreateUniqueSuffix()[..8]}";
        int scheduledReportId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO reporting_scheduled_reports (report_type, schedule, email_recipients, is_active, next_run_date, created_by)
VALUES (@reportType, 'Weekly', 'toggle@example.com', 1, @nextRunDate, 1007);",
                new MySqlParameter("@reportType", reportType),
                new MySqlParameter("@nextRunDate", DateTime.UtcNow.AddDays(3))
            );

            scheduledReportId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM reporting_scheduled_reports WHERE report_type = @reportType;",
                    new MySqlParameter("@reportType", reportType)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_ToggleActive",
                new MySqlParameter("p_id", scheduledReportId),
                new MySqlParameter("p_is_active", 0)
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["affected_rows"]).Should().Be(1);

            var activeFlag = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT is_active FROM reporting_scheduled_reports WHERE id = @scheduledReportId;",
                    new MySqlParameter("@scheduledReportId", scheduledReportId)
                )
            );

            activeFlag.Should().Be(0);
        }
        finally
        {
            if (scheduledReportId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM reporting_scheduled_reports WHERE id = @scheduledReportId;",
                    new MySqlParameter("@scheduledReportId", scheduledReportId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Settings_ScheduledReport_UpdateLastRun_ShouldUpdateRunTimestamps_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var reportType = $"IntegrationLastRun-{CreateUniqueSuffix()[..8]}";
        int scheduledReportId = 0;
        var lastRunDate = DateTime.UtcNow.AddMinutes(-10);
        var nextRunDate = DateTime.UtcNow.AddHours(6);
        var affectedRowsParameter = new MySqlParameter("p_affected_rows", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO reporting_scheduled_reports (report_type, schedule, email_recipients, is_active, next_run_date, created_by)
VALUES (@reportType, 'Hourly', 'lastrun@example.com', 1, @seedNextRunDate, 1008);",
                new MySqlParameter("@reportType", reportType),
                new MySqlParameter("@seedNextRunDate", DateTime.UtcNow.AddHours(1))
            );

            scheduledReportId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM reporting_scheduled_reports WHERE report_type = @reportType;",
                    new MySqlParameter("@reportType", reportType)
                )
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_UpdateLastRun",
                new MySqlParameter("p_id", scheduledReportId),
                new MySqlParameter("p_last_run_date", lastRunDate),
                new MySqlParameter("p_next_run_date", nextRunDate),
                affectedRowsParameter
            );

            ConvertToInt(result.OutputValues["p_affected_rows"]).Should().Be(1);

            var row = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_GetById",
                new MySqlParameter("p_id", scheduledReportId)
            );

            row.Rows.Count.Should().Be(1);
            Convert
                .ToDateTime(row.Rows[0]["last_run_date"])
                .Should()
                .BeCloseTo(lastRunDate, TimeSpan.FromSeconds(1));
            Convert
                .ToDateTime(row.Rows[0]["next_run_date"])
                .Should()
                .BeCloseTo(nextRunDate, TimeSpan.FromSeconds(1));
        }
        finally
        {
            if (scheduledReportId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM reporting_scheduled_reports WHERE id = @scheduledReportId;",
                    new MySqlParameter("@scheduledReportId", scheduledReportId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Settings_ScheduledReport_Update_ShouldUpdateSeededReport_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var originalReportType = $"IntegrationUpdate-{CreateUniqueSuffix()[..8]}";
        var updatedReportType = $"IntegrationUpdated-{CreateUniqueSuffix()[..8]}";
        int scheduledReportId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO reporting_scheduled_reports (report_type, schedule, email_recipients, is_active, next_run_date, created_by)
VALUES (@reportType, 'Daily', 'update@example.com', 1, @nextRunDate, 1009);",
                new MySqlParameter("@reportType", originalReportType),
                new MySqlParameter("@nextRunDate", DateTime.UtcNow.AddHours(8))
            );

            scheduledReportId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM reporting_scheduled_reports WHERE report_type = @reportType;",
                    new MySqlParameter("@reportType", originalReportType)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_Update",
                new MySqlParameter("p_id", scheduledReportId),
                new MySqlParameter("p_report_type", updatedReportType),
                new MySqlParameter("p_schedule", "Twice Daily"),
                new MySqlParameter("p_email_recipients", "updated@example.com"),
                new MySqlParameter("p_next_run_date", DateTime.UtcNow.AddHours(12))
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["affected_rows"]).Should().Be(1);

            var row = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Settings_ScheduledReport_GetById",
                new MySqlParameter("p_id", scheduledReportId)
            );

            row.Rows.Count.Should().Be(1);
            row.Rows[0]["report_type"].Should().Be(updatedReportType);
            row.Rows[0]["schedule"].Should().Be("Twice Daily");
            row.Rows[0]["email_recipients"].Should().Be("updated@example.com");
        }
        finally
        {
            if (scheduledReportId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM reporting_scheduled_reports WHERE id = @scheduledReportId;",
                    new MySqlParameter("@scheduledReportId", scheduledReportId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Volvo_Settings_Get_ShouldReturnSeededSetting_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var settingKey = $"Integration.Volvo.Get.{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO settings_module_volvo
(
    setting_key,
    setting_value,
    setting_type,
    category,
    description,
    default_value,
    modified_by
)
VALUES
(
    @settingKey,
    'True',
    'Boolean',
    'Integration',
    'Integration test setting',
    'False',
    'integration.user'
);",
                new MySqlParameter("@settingKey", settingKey)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_Settings_Get",
                new MySqlParameter("p_setting_key", settingKey)
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["setting_key"].Should().Be(settingKey);
            result.Rows[0]["setting_value"].Should().Be("True");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM settings_module_volvo WHERE setting_key = @settingKey;",
                new MySqlParameter("@settingKey", settingKey)
            );
        }
    }

    [Fact]
    public async Task sp_Volvo_Settings_GetAll_ShouldReturnSeededCategorySetting_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var settingKey = $"Integration.Volvo.All.{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO settings_module_volvo (setting_key, setting_value, setting_type, category, description, default_value, modified_by)
VALUES (@settingKey, '42', 'Integer', 'Printing', 'GetAll integration setting', '10', 'integration.user');",
                new MySqlParameter("@settingKey", settingKey)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_Settings_GetAll",
                new MySqlParameter("p_category", "Printing")
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["setting_key"]?.ToString(),
                        settingKey,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["setting_value"].Should().Be("42");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM settings_module_volvo WHERE setting_key = @settingKey;",
                new MySqlParameter("@settingKey", settingKey)
            );
        }
    }

    [Fact]
    public async Task sp_Volvo_Settings_Reset_ShouldRestoreDefaultValue_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var settingKey = $"Integration.Volvo.Reset.{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO settings_module_volvo (setting_key, setting_value, setting_type, category, description, default_value, modified_by)
VALUES (@settingKey, 'Changed', 'String', 'Printing', 'Reset integration setting', 'Default', 'integration.user');",
                new MySqlParameter("@settingKey", settingKey)
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_Settings_Reset",
                new MySqlParameter("p_setting_key", settingKey),
                new MySqlParameter("p_modified_by", "integration.reset")
            );

            var row = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_Settings_Get",
                new MySqlParameter("p_setting_key", settingKey)
            );

            row.Rows.Count.Should().Be(1);
            row.Rows[0]["setting_value"].Should().Be("Default");
            row.Rows[0]["modified_by"].Should().Be("integration.reset");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM settings_module_volvo WHERE setting_key = @settingKey;",
                new MySqlParameter("@settingKey", settingKey)
            );
        }
    }

    [Fact]
    public async Task sp_Volvo_Settings_Upsert_ShouldUpdateExistingSetting_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var settingKey = $"Integration.Volvo.Upsert.{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO settings_module_volvo (setting_key, setting_value, setting_type, category, description, default_value, modified_by)
VALUES (@settingKey, 'OldValue', 'String', 'Integration', 'Upsert integration setting', 'DefaultValue', 'integration.user');",
                new MySqlParameter("@settingKey", settingKey)
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_Settings_Upsert",
                new MySqlParameter("p_setting_key", settingKey),
                new MySqlParameter("p_setting_value", "NewValue"),
                new MySqlParameter("p_modified_by", "integration.upsert")
            );

            var row = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_Settings_Get",
                new MySqlParameter("p_setting_key", settingKey)
            );

            row.Rows.Count.Should().Be(1);
            row.Rows[0]["setting_value"].Should().Be("NewValue");
            row.Rows[0]["modified_by"].Should().Be("integration.upsert");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM settings_module_volvo WHERE setting_key = @settingKey;",
                new MySqlParameter("@settingKey", settingKey)
            );
        }
    }

    [Fact]
    public async Task sp_Auth_Activity_Log_ShouldInsertAuditRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var username = $"integration.activity.{CreateUniqueSuffix()[..8]}";
        var details = $"integration audit details {CreateUniqueSuffix()[..8]}";

        try
        {
            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Auth_Activity_Log",
                new MySqlParameter("p_event_type", "integration_test"),
                new MySqlParameter("p_username", username),
                new MySqlParameter("p_workstation_name", "INTEGRATION-WS"),
                new MySqlParameter("p_details", details)
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["rows_affected"]).Should().Be(1);

            var insertedCount = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    @"SELECT COUNT(*) FROM settings_personal_activity_log
WHERE event_type = 'integration_test'
  AND username = @username
  AND workstation_name = 'INTEGRATION-WS'
  AND details = @details;",
                    new MySqlParameter("@username", username),
                    new MySqlParameter("@details", details)
                )
            );

            insertedCount.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"DELETE FROM settings_personal_activity_log
WHERE event_type = 'integration_test'
  AND username = @username
  AND workstation_name = 'INTEGRATION-WS'
  AND details = @details;",
                new MySqlParameter("@username", username),
                new MySqlParameter("@details", details)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_UserPreferences_GetRecentIcons_ShouldReturnMostRecentIcons_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var userId = $"integration.dunnage.icons.{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO settings_dunnage_personal (UserId, PreferenceKey, PreferenceValue, LastUpdated)
VALUES
(@userId, 'RecentIcon_Box', '1', @oldest),
(@userId, 'RecentIcon_Pallet', '1', @middle),
(@userId, 'RecentIcon_Crate', '1', @newest);",
                new MySqlParameter("@userId", userId),
                new MySqlParameter("@oldest", DateTime.UtcNow.AddMinutes(-30)),
                new MySqlParameter("@middle", DateTime.UtcNow.AddMinutes(-20)),
                new MySqlParameter("@newest", DateTime.UtcNow.AddMinutes(-10))
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_UserPreferences_GetRecentIcons",
                new MySqlParameter("p_user_id", userId),
                new MySqlParameter("p_count", 2)
            );

            result.Rows.Count.Should().Be(2);
            result.Rows[0]["icon_name"].Should().Be("Crate");
            result.Rows[1]["icon_name"].Should().Be("Pallet");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM settings_dunnage_personal WHERE UserId = @userId;",
                new MySqlParameter("@userId", userId)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_UserPreferences_Upsert_ShouldPersistPreference_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var userId = $"integration.dunnage.pref.{CreateUniqueSuffix()[..8]}";
        const string preferenceKey = "RecentIcon_Forklift";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_UserPreferences_Upsert",
                new MySqlParameter("p_user_id", userId),
                new MySqlParameter("p_pref_key", preferenceKey),
                new MySqlParameter("p_pref_value", "{\"count\":1}")
            );

            var savedValue = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT PreferenceValue FROM settings_dunnage_personal WHERE UserId = @userId AND PreferenceKey = @preferenceKey;",
                new MySqlParameter("@userId", userId),
                new MySqlParameter("@preferenceKey", preferenceKey)
            );

            savedValue.Should().Be("{\"count\":1}");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM settings_dunnage_personal WHERE UserId = @userId AND PreferenceKey = @preferenceKey;",
                new MySqlParameter("@userId", userId),
                new MySqlParameter("@preferenceKey", preferenceKey)
            );
        }
    }

    [Fact]
    public async Task sp_Reporting_DunnageHistory_GetByDateRange_ShouldReturnSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var typeName = $"IntegrationType{suffix}";
        var partId = $"DUN-{suffix}";
        var loadUuid = Guid.NewGuid().ToString();
        var createdBy = $"integration.dunnage.{suffix}";
        var historyDate = DateTime.UtcNow.AddHours(-1);

        try
        {
            await InsertAuthUserAsync(
                connectionString,
                createdBy,
                $"Integration Dunnage {suffix}",
                "PIN-DHIST",
                "guided"
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO dunnage_types (type_name, icon, created_by, created_date)
VALUES (@typeName, 'PackageVariantClosed', @createdBy, @createdDate);",
                new MySqlParameter("@typeName", typeName),
                new MySqlParameter("@createdBy", createdBy),
                new MySqlParameter("@createdDate", historyDate)
            );

            var typeId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_types WHERE type_name = @typeName;",
                    new MySqlParameter("@typeName", typeName)
                )
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO dunnage_parts (part_id, type_id, quantity_type, home_location, created_by, created_date)
VALUES (@partId, @typeId, 'Quantity', 'DUN-LOC', @createdBy, @createdDate);",
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@createdBy", createdBy),
                new MySqlParameter("@createdDate", historyDate)
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO dunnage_history
(
    load_uuid,
    part_id,
    quantity,
    quantity_type,
    received_date,
    created_by,
    employee_number,
    created_date,
    po_number,
    type_id,
    type_name,
    type_icon,
    location
)
VALUES
(
    @loadUuid,
    @partId,
    5,
    'Quantity',
    @receivedDate,
    @createdBy,
    1234,
    @createdDate,
    NULL,
    @typeId,
    @typeName,
    'PackageVariantClosed',
    'DUN-LOC'
);",
                new MySqlParameter("@loadUuid", loadUuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@receivedDate", historyDate),
                new MySqlParameter("@createdBy", createdBy),
                new MySqlParameter("@createdDate", historyDate),
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@typeName", typeName)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Reporting_DunnageHistory_GetByDateRange",
                new MySqlParameter("p_start_date", historyDate.AddDays(-1)),
                new MySqlParameter("p_end_date", historyDate.AddDays(1))
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["id"]?.ToString(),
                        loadUuid,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["part_number"].Should().Be(partId);
            matchingRow["dunnage_type"].Should().Be(typeName);
            matchingRow["source_module"].Should().Be("Dunnage");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_parts WHERE part_id = @partId;",
                new MySqlParameter("@partId", partId)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
            await DeleteAuthUserAsync(connectionString, createdBy);
        }
    }

    [Fact]
    public async Task sp_Reporting_ReceivingHistory_GetByDateRange_ShouldReturnSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadGuid = Guid.NewGuid().ToString();
        var partId = $"RCV-{CreateUniqueSuffix()[..8]}";
        var createdAt = DateTime.UtcNow.AddMinutes(-15);
        int historyRecordId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_history
(
    load_guid,
    quantity,
    part_id,
    po_number,
    po_line_number,
    employee_number,
    transaction_date,
    initial_location,
    label_number,
    vendor_name,
    part_description,
    created_at,
    user_id
)
VALUES
(
    @loadGuid,
    7,
    @partId,
    'PO-RPT-001',
    '1',
    4321,
    @transactionDate,
    'RCV-LOC',
    1,
    'Integration Vendor',
    'Reporting receiving test row',
    @createdAt,
    'integration.user'
);",
                new MySqlParameter("@loadGuid", loadGuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@transactionDate", createdAt.Date),
                new MySqlParameter("@createdAt", createdAt)
            );

            historyRecordId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM receiving_history WHERE load_guid = @loadGuid;",
                    new MySqlParameter("@loadGuid", loadGuid)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Reporting_ReceivingHistory_GetByDateRange",
                new MySqlParameter("p_start_date", createdAt.AddDays(-1)),
                new MySqlParameter("p_end_date", createdAt.AddDays(1))
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row => ConvertToInt(row["id"]) == historyRecordId);

            matchingRow.Should().NotBeNull();
            matchingRow!["part_id"].Should().Be(partId);
            matchingRow["source_module"].Should().Be("Receiving");
            matchingRow["initial_location"].Should().Be("RCV-LOC");
        }
        finally
        {
            if (historyRecordId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_history WHERE id = @historyRecordId;",
                    new MySqlParameter("@historyRecordId", historyRecordId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Reporting_Availability_GetByDateRange_ShouldReturnSeededCounts_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var typeName = $"AvailType{suffix}";
        var dunnagePartId = $"ADUN-{suffix}";
        var dunnageLoadUuid = Guid.NewGuid().ToString();
        var dunnageUser = $"integration.avail.{suffix}";
        var volvoPart = $"V{suffix[..Math.Min(7, suffix.Length)]}";
        var availabilityDate = DateTime.UtcNow.AddHours(-2);
        int receivingHistoryId = 0;
        int volvoShipmentId = 0;

        try
        {
            await InsertAuthUserAsync(
                connectionString,
                dunnageUser,
                $"Availability User {suffix}",
                "PIN-AVAIL",
                "guided"
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO dunnage_types (type_name, icon, created_by, created_date)
VALUES (@typeName, 'PackageVariantClosed', @createdBy, @createdDate);",
                new MySqlParameter("@typeName", typeName),
                new MySqlParameter("@createdBy", dunnageUser),
                new MySqlParameter("@createdDate", availabilityDate)
            );

            var dunnageTypeId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_types WHERE type_name = @typeName;",
                    new MySqlParameter("@typeName", typeName)
                )
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO dunnage_parts (part_id, type_id, quantity_type, home_location, created_by, created_date)
VALUES (@partId, @typeId, 'Quantity', 'AVAIL-DUN', @createdBy, @createdDate);",
                new MySqlParameter("@partId", dunnagePartId),
                new MySqlParameter("@typeId", dunnageTypeId),
                new MySqlParameter("@createdBy", dunnageUser),
                new MySqlParameter("@createdDate", availabilityDate)
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO dunnage_history
(
    load_uuid, part_id, quantity, quantity_type, received_date, created_by,
    employee_number, created_date, type_id, type_name, type_icon, location
)
VALUES
(
    @loadUuid, @partId, 4, 'Quantity', @receivedDate, @createdBy,
    2222, @createdDate, @typeId, @typeName, 'PackageVariantClosed', 'AVAIL-DUN'
);",
                new MySqlParameter("@loadUuid", dunnageLoadUuid),
                new MySqlParameter("@partId", dunnagePartId),
                new MySqlParameter("@receivedDate", availabilityDate),
                new MySqlParameter("@createdBy", dunnageUser),
                new MySqlParameter("@createdDate", availabilityDate),
                new MySqlParameter("@typeId", dunnageTypeId),
                new MySqlParameter("@typeName", typeName)
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_history
(
    load_guid, quantity, part_id, po_number, po_line_number, employee_number,
    transaction_date, initial_location, label_number, vendor_name, part_description,
    created_at, user_id
)
VALUES
(
    @loadGuid, 6, @partId, 'PO-AVAIL', '1', 3333,
    @transactionDate, 'AVAIL-RCV', 1, 'Availability Vendor', 'Availability receiving row',
    @createdAt, 'integration.user'
);",
                new MySqlParameter("@loadGuid", Guid.NewGuid().ToString()),
                new MySqlParameter("@partId", $"ARCV-{suffix}"),
                new MySqlParameter("@transactionDate", availabilityDate.Date),
                new MySqlParameter("@createdAt", availabilityDate)
            );

            receivingHistoryId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT MAX(id) FROM receiving_history WHERE part_id = @partId;",
                    new MySqlParameter("@partId", $"ARCV-{suffix}")
                )
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO volvo_masterdata (part_number, quantity_per_skid, is_active) VALUES (@partNumber, 12, 1);",
                new MySqlParameter("@partNumber", volvoPart)
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO volvo_label_data
(
    shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date, is_archived
)
VALUES
(
    @shipmentDate, @shipmentNumber, 'PO-VOLVO-AVAIL', 'RCV-VOLVO-AVAIL', '4444', 'Availability Volvo note', 'completed', @createdDate, 0
);",
                new MySqlParameter("@shipmentDate", availabilityDate.Date),
                new MySqlParameter("@shipmentNumber", Random.Shared.Next(1000, 9999)),
                new MySqlParameter("@createdDate", availabilityDate)
            );

            volvoShipmentId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT MAX(id) FROM volvo_label_data WHERE po_number = 'PO-VOLVO-AVAIL';"
                )
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO volvo_line_data
(
    shipment_id, part_number, po_status, location, quantity_per_skid, received_skid_count, calculated_piece_count
)
VALUES
(
    @shipmentId, @partNumber, 'Received', 'AVAIL-VOLVO', 12, 1, 12
);",
                new MySqlParameter("@shipmentId", volvoShipmentId),
                new MySqlParameter("@partNumber", volvoPart)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Reporting_Availability_GetByDateRange",
                new MySqlParameter("p_start_date", availabilityDate.AddDays(-1)),
                new MySqlParameter("p_end_date", availabilityDate.AddDays(1))
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["receiving_count"]).Should().BeGreaterThan(0);
            ConvertToInt(result.Rows[0]["dunnage_count"]).Should().BeGreaterThan(0);
            ConvertToInt(result.Rows[0]["volvo_count"]).Should().BeGreaterThan(0);
        }
        finally
        {
            if (volvoShipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_line_data WHERE shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", volvoShipmentId)
                );
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", volvoShipmentId)
                );
            }

            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_masterdata WHERE part_number = @partNumber;",
                new MySqlParameter("@partNumber", volvoPart)
            );

            if (receivingHistoryId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_history WHERE id = @historyId;",
                    new MySqlParameter("@historyId", receivingHistoryId)
                );
            }

            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", dunnageLoadUuid)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_parts WHERE part_id = @partId;",
                new MySqlParameter("@partId", dunnagePartId)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
            await DeleteAuthUserAsync(connectionString, dunnageUser);
        }
    }

    [Fact]
    public async Task sp_Reporting_VolvoHistory_GetByDateRange_ShouldReturnSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var suffix = CreateUniqueSuffix()[..8];
        var partNumber = $"VH{suffix[..Math.Min(6, suffix.Length)]}";
        var createdDate = DateTime.UtcNow.AddHours(-3);
        int shipmentId = 0;
        int lineId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO volvo_masterdata (part_number, quantity_per_skid, is_active) VALUES (@partNumber, 24, 1);",
                new MySqlParameter("@partNumber", partNumber)
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO volvo_label_data
(
    shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date, is_archived
)
VALUES
(
    @shipmentDate, @shipmentNumber, 'PO-VOLVO-HIST', 'RCV-VOLVO-HIST', '5555', 'Volvo history test row', 'completed', @createdDate, 0
);",
                new MySqlParameter("@shipmentDate", createdDate.Date),
                new MySqlParameter("@shipmentNumber", Random.Shared.Next(1000, 9999)),
                new MySqlParameter("@createdDate", createdDate)
            );

            shipmentId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT MAX(id) FROM volvo_label_data WHERE po_number = 'PO-VOLVO-HIST';"
                )
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO volvo_line_data
(
    shipment_id, part_number, po_status, location, quantity_per_skid, received_skid_count, calculated_piece_count, discrepancy_note
)
VALUES
(
    @shipmentId, @partNumber, 'Received', 'VOLVO-HIST-LOC', 24, 2, 48, 'Volvo integration note'
);",
                new MySqlParameter("@shipmentId", shipmentId),
                new MySqlParameter("@partNumber", partNumber)
            );

            lineId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT MAX(id) FROM volvo_line_data WHERE shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Reporting_VolvoHistory_GetByDateRange",
                new MySqlParameter("p_start_date", createdDate.AddDays(-1)),
                new MySqlParameter("p_end_date", createdDate.AddDays(1))
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["id"]?.ToString(),
                        lineId.ToString(),
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["part_number"].Should().Be(partNumber);
            matchingRow["source_module"].Should().Be("Volvo");
            matchingRow["location"].Should().Be("VOLVO-HIST-LOC");
        }
        finally
        {
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_line_data WHERE shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }

            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_masterdata WHERE part_number = @partNumber;",
                new MySqlParameter("@partNumber", partNumber)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_PackageTypeMappings_Update_ShouldUpdateSeededMapping_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partPrefix = $"U{CreateUniqueSuffix()[..5].ToUpperInvariant()}";
        int mappingId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_package_type_mapping (part_prefix, package_type, is_default, display_order, is_active, created_by)
VALUES (@partPrefix, 'Box', 0, 1, 1, 'integration.user');",
                new MySqlParameter("@partPrefix", partPrefix)
            );

            mappingId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM receiving_package_type_mapping WHERE part_prefix = @partPrefix;",
                    new MySqlParameter("@partPrefix", partPrefix)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypeMappings_Update",
                new MySqlParameter("p_id", mappingId),
                new MySqlParameter("p_part_prefix", partPrefix),
                new MySqlParameter("p_package_type", "Pallet"),
                new MySqlParameter("p_is_default", 1),
                new MySqlParameter("p_display_order", 9),
                new MySqlParameter("p_is_active", 1)
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["affected_rows"]).Should().Be(1);
            result.Rows[0]["error_message"].Should().Be(DBNull.Value);

            var row = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypeMappings_GetAll"
            );
            var matchingRow = row
                .Rows.Cast<DataRow>()
                .Single(r => ConvertToInt(r["id"]) == mappingId);
            matchingRow["package_type"].Should().Be("Pallet");
            ConvertToInt(matchingRow["display_order"]).Should().Be(9);
        }
        finally
        {
            if (mappingId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_package_type_mapping WHERE id = @mappingId;",
                    new MySqlParameter("@mappingId", mappingId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_PackageTypePreference_Delete_ShouldDeleteSeededPreference_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partId = $"PREF-DEL-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";

        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"
INSERT INTO receiving_package_types (PartID, PackageTypeName, CustomTypeName, LastModified)
VALUES (@partId, 'Custom', 'DeleteMe', NOW());",
            new MySqlParameter("@partId", partId)
        );

        await ExecuteStoredProcedureNonQueryAsync(
            connectionString,
            "sp_Receiving_PackageTypePreference_Delete",
            new MySqlParameter("p_PartID", partId)
        );

        var remainingRows = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COUNT(*) FROM receiving_package_types WHERE PartID = @partId;",
                new MySqlParameter("@partId", partId)
            )
        );

        remainingRows.Should().Be(0);
    }

    [Fact]
    public async Task sp_Receiving_Load_GetAll_ShouldReturnSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadGuid = Guid.NewGuid().ToString();
        var partId = $"LOAD-{CreateUniqueSuffix()[..8]}";
        var transactionDate = DateTime.UtcNow.Date;
        int historyRecordId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_history
(
    load_guid, quantity, part_id, po_number, po_line_number, employee_number,
    transaction_date, initial_location, label_number, load_number, vendor_name,
    part_description, created_at, user_id
)
VALUES
(
    @loadGuid, 10, @partId, 'PO-LOAD-GET', '1', 6789,
    @transactionDate, 'LOAD-LOC', 1, 2, 'Load Vendor',
    'Load get-all history row', @createdAt, 'integration.user'
);",
                new MySqlParameter("@loadGuid", loadGuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@transactionDate", transactionDate),
                new MySqlParameter("@createdAt", DateTime.UtcNow)
            );

            historyRecordId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM receiving_history WHERE load_guid = @loadGuid;",
                    new MySqlParameter("@loadGuid", loadGuid)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_Load_GetAll",
                new MySqlParameter("p_StartDate", transactionDate.AddDays(-1)),
                new MySqlParameter("p_EndDate", transactionDate.AddDays(1))
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row => ConvertToInt(row["id"]) == historyRecordId);

            matchingRow.Should().NotBeNull();
            matchingRow!["part_id"].Should().Be(partId);
            matchingRow["initial_location"].Should().Be("LOAD-LOC");
            ConvertToInt(matchingRow["load_number"]).Should().Be(2);
        }
        finally
        {
            if (historyRecordId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_history WHERE id = @historyRecordId;",
                    new MySqlParameter("@historyRecordId", historyRecordId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_Load_Update_ShouldUpdateSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadGuid = Guid.NewGuid().ToString();
        var partId = $"LUP-{CreateUniqueSuffix()[..8]}";
        int historyRecordId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_history
(
    load_guid, quantity, part_id, po_number, po_line_number, employee_number,
    transaction_date, initial_location, label_number, load_number, vendor_name,
    part_description, created_at, user_id, is_non_po_item
)
VALUES
(
    @loadGuid, 3, @partId, 'PO-LOAD-UPD', '1', 1111,
    @transactionDate, 'OLD-LOC', 1, 1, 'Update Vendor',
    'Load update history row', @createdAt, 'integration.user', 0
);",
                new MySqlParameter("@loadGuid", loadGuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@transactionDate", DateTime.UtcNow.Date),
                new MySqlParameter("@createdAt", DateTime.UtcNow)
            );

            historyRecordId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM receiving_history WHERE load_guid = @loadGuid;",
                    new MySqlParameter("@loadGuid", loadGuid)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_Load_Update",
                new MySqlParameter("p_LoadID", loadGuid),
                new MySqlParameter("p_HistoryRecordID", historyRecordId),
                new MySqlParameter("p_PartID", partId),
                new MySqlParameter("p_PartType", DBNull.Value),
                new MySqlParameter("p_PONumber", "PO-LOAD-UPD-NEW"),
                new MySqlParameter("p_POLineNumber", "2"),
                new MySqlParameter("p_LoadNumber", 7),
                new MySqlParameter("p_WeightQuantity", 12.0m),
                new MySqlParameter("p_HeatLotNumber", "HEAT-NEW"),
                new MySqlParameter("p_InitialLocation", "NEW-LOC"),
                new MySqlParameter("p_PackagesPerLoad", DBNull.Value),
                new MySqlParameter("p_PackageTypeName", DBNull.Value),
                new MySqlParameter("p_WeightPerPackage", DBNull.Value),
                new MySqlParameter("p_IsNonPOItem", 1),
                new MySqlParameter("p_ReceivedDate", DateTime.UtcNow)
            );

            var row = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_History_Get",
                new MySqlParameter("p_PartID", partId),
                new MySqlParameter("p_StartDate", DateTime.UtcNow.Date.AddDays(-1)),
                new MySqlParameter("p_EndDate", DateTime.UtcNow.Date.AddDays(1))
            );

            row.Rows.Count.Should().BeGreaterThan(0);
            var matchingRow = row
                .Rows.Cast<DataRow>()
                .Single(r => ConvertToInt(r["id"]) == historyRecordId);
            matchingRow["po_number"].Should().Be("PO-LOAD-UPD-NEW");
            matchingRow["initial_location"].Should().Be("NEW-LOC");
            matchingRow["heat"].Should().Be("HEAT-NEW");
            ConvertToInt(matchingRow["load_number"]).Should().Be(7);
        }
        finally
        {
            if (historyRecordId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_history WHERE id = @historyRecordId;",
                    new MySqlParameter("@historyRecordId", historyRecordId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_LabelData_Update_ShouldUpdateSeededQueueRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadId = Guid.NewGuid().ToString();
        var partId = $"LBL-{CreateUniqueSuffix()[..8]}";
        int labelDataRecordId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_label_data
(
    load_id, load_number, quantity, weight_quantity, part_id, part_description,
    po_number, po_line_number, employee_number, received_date, transaction_date,
    initial_location, label_number, vendor_name
)
VALUES
(
    @loadId, 1, 2, 2, @partId, 'Label data row',
    'PO-LABEL-UPD', '1', 2468, @receivedDate, @transactionDate,
    'OLD-LABEL-LOC', 1, 'Label Vendor'
);",
                new MySqlParameter("@loadId", loadId),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@receivedDate", DateTime.UtcNow),
                new MySqlParameter("@transactionDate", DateTime.UtcNow.Date)
            );

            labelDataRecordId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM receiving_label_data WHERE load_id = @loadId;",
                    new MySqlParameter("@loadId", loadId)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_LabelData_Update",
                new MySqlParameter("p_label_data_record_id", labelDataRecordId),
                new MySqlParameter("p_load_id", loadId),
                new MySqlParameter("p_load_number", 8),
                new MySqlParameter("p_quantity", 9),
                new MySqlParameter("p_weight_quantity", 9.5m),
                new MySqlParameter("p_part_id", partId),
                new MySqlParameter("p_part_description", "Updated label data row"),
                new MySqlParameter("p_part_type", "Standard"),
                new MySqlParameter("p_po_number", "PO-LABEL-UPD-NEW"),
                new MySqlParameter("p_po_line_number", "2"),
                new MySqlParameter("p_po_vendor", "Updated Vendor"),
                new MySqlParameter("p_po_status", "Open"),
                new MySqlParameter("p_po_due_date", DateTime.UtcNow.Date),
                new MySqlParameter("p_qty_ordered", 20m),
                new MySqlParameter("p_unit_of_measure", "EA"),
                new MySqlParameter("p_remaining_quantity", 11),
                new MySqlParameter("p_employee_number", 2468),
                new MySqlParameter("p_user_id", "integration.user"),
                new MySqlParameter("p_heat", "LBL-HEAT"),
                new MySqlParameter("p_received_date", DateTime.UtcNow),
                new MySqlParameter("p_transaction_date", DateTime.UtcNow.Date),
                new MySqlParameter("p_initial_location", "NEW-LABEL-LOC"),
                new MySqlParameter("p_packages_per_load", 3),
                new MySqlParameter("p_package_type_name", "Box"),
                new MySqlParameter("p_weight_per_package", 3.5m),
                new MySqlParameter("p_coils_on_skid", 0),
                new MySqlParameter("p_label_number", 4),
                new MySqlParameter("p_vendor_name", "Updated Vendor"),
                new MySqlParameter("p_is_non_po_item", 0),
                new MySqlParameter("p_is_quality_hold_required", 0),
                new MySqlParameter("p_is_quality_hold_acknowledged", 0),
                new MySqlParameter("p_quality_hold_restriction_type", DBNull.Value)
            );

            var rowCount = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_label_data WHERE id = @id AND initial_location = 'NEW-LABEL-LOC' AND po_number = 'PO-LABEL-UPD-NEW' AND load_number = 8;",
                    new MySqlParameter("@id", labelDataRecordId)
                )
            );

            rowCount.Should().Be(1);
        }
        finally
        {
            if (labelDataRecordId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_label_data WHERE id = @id;",
                    new MySqlParameter("@id", labelDataRecordId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_Line_Insert_ShouldReturnDeprecationStatus_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var statusParameter = new MySqlParameter("p_Status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorParameter = new MySqlParameter("p_ErrorMsg", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };

        var result = await ExecuteStoredProcedureNonQueryAsync(
            connectionString,
            "sp_Receiving_Line_Insert",
            new MySqlParameter("p_Quantity", 1),
            new MySqlParameter("p_PartID", "DEPRECATED-PART"),
            new MySqlParameter("p_PONumber", 12345),
            new MySqlParameter("p_EmployeeNumber", 9999),
            new MySqlParameter("p_Heat", "HEAT"),
            new MySqlParameter("p_Date", DateTime.UtcNow.Date),
            new MySqlParameter("p_InitialLocation", "DEP-LOC"),
            new MySqlParameter("p_CoilsOnSkid", 0),
            new MySqlParameter("p_VendorName", "Deprecated Vendor"),
            new MySqlParameter("p_PartDescription", "Deprecated procedure call"),
            statusParameter,
            errorParameter
        );

        ConvertToInt(result.OutputValues["p_Status"]).Should().Be(1);
        result
            .OutputValues["p_ErrorMsg"]
            .Should()
            .Be("sp_Receiving_Line_Insert is deprecated - use receiving_history table");
    }

    [Fact]
    public async Task sp_Receiving_History_Get_ShouldReturnSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadGuid = Guid.NewGuid().ToString();
        var partId = $"HIST-{CreateUniqueSuffix()[..8]}";
        int historyRecordId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_history
(
    load_guid, quantity, part_id, po_number, po_line_number, employee_number,
    transaction_date, initial_location, label_number, vendor_name, part_description,
    created_at, user_id
)
VALUES
(
    @loadGuid, 5, @partId, 'PO-HIST-GET', '1', 1357,
    @transactionDate, 'HIST-LOC', 2, 'History Vendor', 'History get row',
    @createdAt, 'integration.user'
);",
                new MySqlParameter("@loadGuid", loadGuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@transactionDate", DateTime.UtcNow.Date),
                new MySqlParameter("@createdAt", DateTime.UtcNow)
            );

            historyRecordId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM receiving_history WHERE load_guid = @loadGuid;",
                    new MySqlParameter("@loadGuid", loadGuid)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_History_Get",
                new MySqlParameter("p_PartID", partId),
                new MySqlParameter("p_StartDate", DateTime.UtcNow.Date.AddDays(-1)),
                new MySqlParameter("p_EndDate", DateTime.UtcNow.Date.AddDays(1))
            );

            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row => ConvertToInt(row["id"]) == historyRecordId);
            matchingRow.Should().NotBeNull();
            matchingRow!["part_id"].Should().Be(partId);
            matchingRow["initial_location"].Should().Be("HIST-LOC");
        }
        finally
        {
            if (historyRecordId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_history WHERE id = @historyRecordId;",
                    new MySqlParameter("@historyRecordId", historyRecordId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_PackageTypes_Delete_ShouldDeleteSeededPreferenceById_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partId = $"PTDEL-{CreateUniqueSuffix()[..8].ToUpperInvariant()}";
        int preferenceId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO receiving_package_types (PartID, PackageTypeName, CustomTypeName, LastModified)
VALUES (@partId, 'Custom', 'DeleteById', NOW());",
                new MySqlParameter("@partId", partId)
            );

            preferenceId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT PreferenceID FROM receiving_package_types WHERE PartID = @partId;",
                    new MySqlParameter("@partId", partId)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_PackageTypes_Delete",
                new MySqlParameter("p_id", preferenceId)
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["affected_rows"]).Should().Be(1);

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_package_types WHERE PreferenceID = @preferenceId;",
                    new MySqlParameter("@preferenceId", preferenceId)
                )
            );

            remainingRows.Should().Be(0);
        }
        finally
        {
            if (preferenceId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_package_types WHERE PreferenceID = @preferenceId;",
                    new MySqlParameter("@preferenceId", preferenceId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Dunnage_NonPO_GetAll_ShouldReturnSeededEntry_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var value = $"DUN-NONPO-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_non_po_entries (value, created_by, use_count) VALUES (@value, 'integration.user', 3);",
                new MySqlParameter("@value", value)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_NonPO_GetAll"
            );
            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["value"]?.ToString(),
                        value,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            ConvertToInt(matchingRow!["use_count"]).Should().Be(3);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_non_po_entries WHERE value = @value;",
                new MySqlParameter("@value", value)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_NonPO_Upsert_ShouldInsertOrIncrementEntry_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var value = $"DUN-UPSERT-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_NonPO_Upsert",
                new MySqlParameter("p_value", value),
                new MySqlParameter("p_created_by", "integration.user")
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_NonPO_Upsert",
                new MySqlParameter("p_value", value),
                new MySqlParameter("p_created_by", "integration.user")
            );

            var useCount = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT use_count FROM dunnage_non_po_entries WHERE value = @value;",
                    new MySqlParameter("@value", value)
                )
            );

            useCount.Should().Be(2);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_non_po_entries WHERE value = @value;",
                new MySqlParameter("@value", value)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_NonPO_Delete_ShouldDeleteSeededEntry_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var value = $"DUN-DEL-{CreateUniqueSuffix()[..8]}";
        int entryId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_non_po_entries (value, created_by) VALUES (@value, 'integration.user');",
                new MySqlParameter("@value", value)
            );

            entryId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_non_po_entries WHERE value = @value;",
                    new MySqlParameter("@value", value)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_NonPO_Delete",
                new MySqlParameter("p_id", entryId)
            );

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_non_po_entries WHERE id = @id;",
                    new MySqlParameter("@id", entryId)
                )
            );

            remainingRows.Should().Be(0);
        }
        finally
        {
            if (entryId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM dunnage_non_po_entries WHERE id = @id;",
                    new MySqlParameter("@id", entryId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Dunnage_NonPO_PartDefault_GetByPartId_ShouldReturnSeededDefault_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partId = $"DNPD-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_non_po_part_defaults (part_id, value, updated_by) VALUES (@partId, 'DefaultReason', 'integration.user');",
                new MySqlParameter("@partId", partId)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_NonPO_PartDefault_GetByPartId",
                new MySqlParameter("p_part_id", partId)
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["value"].Should().Be("DefaultReason");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_non_po_part_defaults WHERE part_id = @partId;",
                new MySqlParameter("@partId", partId)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_NonPO_PartDefault_Upsert_ShouldPersistDefault_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partId = $"DNPDU-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_NonPO_PartDefault_Upsert",
                new MySqlParameter("p_part_id", partId),
                new MySqlParameter("p_value", "UpsertedReason"),
                new MySqlParameter("p_updated_by", "integration.user")
            );

            var savedValue = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT value FROM dunnage_non_po_part_defaults WHERE part_id = @partId;",
                new MySqlParameter("@partId", partId)
            );

            savedValue.Should().Be("UpsertedReason");
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_non_po_part_defaults WHERE part_id = @partId;",
                new MySqlParameter("@partId", partId)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_QuantityTypes_GetAll_ShouldReturnSeededQuantityType_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var quantityType = $"Qty-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_quantity_types (quantity_type, created_by) VALUES (@quantityType, 'integration.user');",
                new MySqlParameter("@quantityType", quantityType)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_QuantityTypes_GetAll"
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["quantity_type"]?.ToString(),
                        quantityType,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_quantity_types WHERE quantity_type = @quantityType;",
                new MySqlParameter("@quantityType", quantityType)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_QuantityTypes_InsertIfMissing_ShouldInsertNewQuantityType_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var quantityType = $"InsertQty-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_QuantityTypes_InsertIfMissing",
                new MySqlParameter("p_quantity_type", quantityType),
                new MySqlParameter("p_user", "integration.user")
            );

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_quantity_types WHERE quantity_type = @quantityType;",
                    new MySqlParameter("@quantityType", quantityType)
                )
            );

            savedRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_quantity_types WHERE quantity_type = @quantityType;",
                new MySqlParameter("@quantityType", quantityType)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_GetAll_ShouldReturnSeededType_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DTGA-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_types (type_name, icon, created_by, created_date) VALUES (@typeName, 'PackageVariantClosed', 'integration.user', NOW());",
                new MySqlParameter("@typeName", typeName)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Types_GetAll"
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["type_name"]?.ToString(),
                        typeName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_GetById_ShouldReturnSeededType_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DTGI-{CreateUniqueSuffix()[..8]}";
        int typeId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_types (type_name, icon, created_by, created_date) VALUES (@typeName, 'PackageVariantClosed', 'integration.user', NOW());",
                new MySqlParameter("@typeName", typeName)
            );

            typeId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_types WHERE type_name = @typeName;",
                    new MySqlParameter("@typeName", typeName)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Types_GetById",
                new MySqlParameter("p_id", typeId)
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["type_name"].Should().Be(typeName);
        }
        finally
        {
            if (typeId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM dunnage_types WHERE id = @typeId;",
                    new MySqlParameter("@typeId", typeId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_CheckDuplicate_ShouldReportExistingType_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DTCD-{CreateUniqueSuffix()[..8]}";
        var existsParameter = new MySqlParameter("p_exists", MySqlDbType.Bit)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_types (type_name, icon, created_by, created_date) VALUES (@typeName, 'PackageVariantClosed', 'integration.user', NOW());",
                new MySqlParameter("@typeName", typeName)
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Types_CheckDuplicate",
                new MySqlParameter("p_type_name", typeName),
                new MySqlParameter("p_exclude_id", DBNull.Value),
                existsParameter
            );

            Convert.ToBoolean(result.OutputValues["p_exists"]).Should().BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_GetPartCount_ShouldReturnSeededPartCount_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DTGPC-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DTGPC-P-{CreateUniqueSuffix()[..6]}";
        var countParameter = new MySqlParameter("p_count", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "GETPARTCOUNT-LOC",
                "integration.user"
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Types_GetPartCount",
                new MySqlParameter("p_type_id", typeId),
                countParameter
            );

            ConvertToInt(result.OutputValues["p_count"]).Should().Be(1);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_GetTransactionCount_ShouldReturnSeededTransactionCount_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DTGTC-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DTGTC-P-{CreateUniqueSuffix()[..6]}";
        var loadUuid = Guid.NewGuid().ToString();
        var countParameter = new MySqlParameter("p_count", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "GETTRANSCOUNT-LOC",
                "integration.user"
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_history
(
    load_uuid, part_id, quantity, quantity_type, received_date, created_by,
    employee_number, created_date, type_id, type_name, type_icon, location
)
VALUES
(
    @loadUuid, @partId, 3, 'Quantity', @receivedDate, 'integration.user',
    8888, @createdDate, @typeId, @typeName, 'PackageVariantClosed', 'GETTRANSCOUNT-LOC'
);",
                new MySqlParameter("@loadUuid", loadUuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@receivedDate", DateTime.UtcNow),
                new MySqlParameter("@createdDate", DateTime.UtcNow),
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@typeName", typeName)
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Types_GetTransactionCount",
                new MySqlParameter("p_type_id", typeId),
                countParameter
            );

            ConvertToInt(result.OutputValues["p_count"]).Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_Insert_ShouldInsertType_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DTINS-{CreateUniqueSuffix()[..8]}";
        var newIdParameter = new MySqlParameter("p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var insertedTypeId = 0;

        try
        {
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Types_Insert",
                new MySqlParameter("p_type_name", typeName),
                new MySqlParameter("p_icon", "PackageVariantClosed"),
                new MySqlParameter("p_image_path", DBNull.Value),
                new MySqlParameter("p_user", "integration.user"),
                newIdParameter
            );

            insertedTypeId = ConvertToInt(result.OutputValues["p_new_id"]);
            insertedTypeId.Should().BeGreaterThan(0);

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_types WHERE id = @typeId AND type_name = @typeName;",
                    new MySqlParameter("@typeId", insertedTypeId),
                    new MySqlParameter("@typeName", typeName)
                )
            );

            savedRows.Should().Be(1);
        }
        finally
        {
            if (insertedTypeId > 0)
            {
                await DeleteDunnageTypeByIdAsync(connectionString, insertedTypeId);
            }
            else
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                    new MySqlParameter("@typeName", typeName)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_Update_ShouldUpdateSeededType_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var originalTypeName = $"DTUPD-O-{CreateUniqueSuffix()[..6]}";
        var updatedTypeName = $"DTUPD-N-{CreateUniqueSuffix()[..6]}";
        var typeId = 0;

        try
        {
            typeId = await InsertDunnageTypeAsync(connectionString, originalTypeName);

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Types_Update",
                new MySqlParameter("p_id", typeId),
                new MySqlParameter("p_type_name", updatedTypeName),
                new MySqlParameter("p_icon", "PackageVariantClosed"),
                new MySqlParameter("p_image_path", DBNull.Value),
                new MySqlParameter("p_modified_by", "integration.user")
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["affected_rows"]).Should().Be(1);

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_types WHERE id = @typeId AND type_name = @typeName;",
                    new MySqlParameter("@typeId", typeId),
                    new MySqlParameter("@typeName", updatedTypeName)
                )
            );

            savedRows.Should().Be(1);
        }
        finally
        {
            if (typeId > 0)
            {
                await DeleteDunnageTypeByIdAsync(connectionString, typeId);
            }
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_Delete_ShouldDeleteUnusedType_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DTDEL-{CreateUniqueSuffix()[..8]}";
        var statusParameter = new MySqlParameter("p_status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorParameter = new MySqlParameter("p_error_msg", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };
        var typeId = 0;

        try
        {
            typeId = await InsertDunnageTypeAsync(connectionString, typeName);

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Types_Delete",
                new MySqlParameter("p_id", typeId),
                new MySqlParameter("p_modified_by", "integration.user"),
                statusParameter,
                errorParameter
            );

            ConvertToInt(result.OutputValues["p_status"]).Should().Be(1);
            result.OutputValues["p_error_msg"].Should().Be("Dunnage type deleted successfully");

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_types WHERE id = @typeId;",
                    new MySqlParameter("@typeId", typeId)
                )
            );

            remainingRows.Should().Be(0);
        }
        finally
        {
            if (typeId > 0)
            {
                await DeleteDunnageTypeByIdAsync(connectionString, typeId);
            }
        }
    }

    private static async Task DeleteDunnageTypeByIdAsync(string connectionString, int typeId)
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            "DELETE FROM dunnage_types WHERE id = @typeId;",
            new MySqlParameter("@typeId", typeId)
        );
    }

    [Fact]
    public async Task sp_Dunnage_Parts_GetAll_ShouldReturnSeededPart_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPGA-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DPGA-P-{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "PARTS-ALL-LOC",
                "integration.user"
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_GetAll"
            );
            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["part_id"]?.ToString(),
                        partId,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["type_name"].Should().Be(typeName);
            matchingRow["home_location"].Should().Be("PARTS-ALL-LOC");
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Parts_GetById_ShouldReturnSeededPart_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPGI-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DPGI-P-{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "PARTS-ID-LOC",
                "integration.user"
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_GetById",
                new MySqlParameter("p_part_id", partId)
            );

            result.Rows.Count.Should().Be(1);
            result.Rows[0]["part_id"].Should().Be(partId);
            result.Rows[0]["type_name"].Should().Be(typeName);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Parts_GetByType_ShouldReturnSeededPart_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPGT-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DPGT-P-{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "PARTS-TYPE-LOC",
                "integration.user"
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_GetByType",
                new MySqlParameter("p_type_id", typeId)
            );
            var matchingRow = result
                .Rows.Cast<DataRow>()
                .SingleOrDefault(row =>
                    string.Equals(
                        row["part_id"]?.ToString(),
                        partId,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            matchingRow.Should().NotBeNull();
            matchingRow!["type_name"].Should().Be(typeName);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_CountParts_ShouldReturnSeededPartCount_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DTCP-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DTCP-P-{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "COUNT-PART-LOC",
                "integration.user"
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Types_CountParts",
                new MySqlParameter("p_type_id", typeId)
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["part_count"]).Should().Be(1);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_CountTransactions_ShouldReturnSeededTransactionCount_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DTCT-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DTCT-P-{CreateUniqueSuffix()[..6]}";
        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "COUNT-TRANS-LOC",
                "integration.user"
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"
INSERT INTO dunnage_history
(
    load_uuid, part_id, quantity, quantity_type, received_date, created_by,
    employee_number, created_date, type_id, type_name, type_icon, location
)
VALUES
(
    @loadUuid, @partId, 2, 'Quantity', @receivedDate, 'integration.user',
    7777, @createdDate, @typeId, @typeName, 'PackageVariantClosed', 'COUNT-TRANS-LOC'
);",
                new MySqlParameter("@loadUuid", loadUuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@receivedDate", DateTime.UtcNow),
                new MySqlParameter("@createdDate", DateTime.UtcNow),
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@typeName", typeName)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Types_CountTransactions",
                new MySqlParameter("p_type_id", typeId)
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["transaction_count"]).Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Parts_CountTransactions_ShouldReturnSeededTransactionCount_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPCT-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DPCT-P-{CreateUniqueSuffix()[..6]}";
        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "PARTS-COUNT-LOC",
                "integration.user"
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_history
(
    load_uuid, part_id, quantity, quantity_type, received_date, created_by,
    employee_number, created_date, type_id, type_name, type_icon, location
)
VALUES
(
    @loadUuid, @partId, 4, 'Quantity', @receivedDate, 'integration.user',
    9111, @createdDate, @typeId, @typeName, 'PackageVariantClosed', 'PARTS-COUNT-LOC'
);",
                new MySqlParameter("@loadUuid", loadUuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@receivedDate", DateTime.UtcNow),
                new MySqlParameter("@createdDate", DateTime.UtcNow),
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@typeName", typeName)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_CountTransactions",
                new MySqlParameter("p_part_id", partId)
            );

            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["transaction_count"]).Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Parts_GetTransactionCount_ShouldReturnSeededTransactionCount_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPGTC-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DPGTC-P-{CreateUniqueSuffix()[..6]}";
        var loadUuid = Guid.NewGuid().ToString();
        var countParameter = new MySqlParameter("p_count", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "PARTS-OUTCOUNT-LOC",
                "integration.user"
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_history
(
    load_uuid, part_id, quantity, quantity_type, received_date, created_by,
    employee_number, created_date, type_id, type_name, type_icon, location
)
VALUES
(
    @loadUuid, @partId, 5, 'Quantity', @receivedDate, 'integration.user',
    9222, @createdDate, @typeId, @typeName, 'PackageVariantClosed', 'PARTS-OUTCOUNT-LOC'
);",
                new MySqlParameter("@loadUuid", loadUuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@receivedDate", DateTime.UtcNow),
                new MySqlParameter("@createdDate", DateTime.UtcNow),
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@typeName", typeName)
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_GetTransactionCount",
                new MySqlParameter("p_part_id", partId),
                countParameter
            );

            ConvertToInt(result.OutputValues["p_count"]).Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_dunnage_parts_insert_ShouldInsertPart_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPINS-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DPINS-P-{CreateUniqueSuffix()[..6]}";
        var newIdParameter = new MySqlParameter("p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_Insert",
                new MySqlParameter("p_part_id", partId),
                new MySqlParameter("p_type_id", typeId),
                new MySqlParameter("p_spec_values", "{\"color\":\"red\"}"),
                new MySqlParameter("p_image_path", DBNull.Value),
                new MySqlParameter("p_quantity_type", "Boxes"),
                new MySqlParameter("p_home_location", "PART-INSERT-LOC"),
                new MySqlParameter("p_user", "integration.user"),
                newIdParameter
            );

            var newId = ConvertToInt(result.OutputValues["p_new_id"]);
            newId.Should().BeGreaterThan(0);

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_parts WHERE id = @id AND part_id = @partId;",
                    new MySqlParameter("@id", newId),
                    new MySqlParameter("@partId", partId)
                )
            );
            savedRows.Should().Be(1);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_dunnage_parts_update_ShouldUpdateSeededPart_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPUPD-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DPUPD-P-{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "PART-OLD-LOC",
                "integration.user"
            );
            var partRowId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_parts WHERE part_id = @partId;",
                    new MySqlParameter("@partId", partId)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_Update",
                new MySqlParameter("p_id", partRowId),
                new MySqlParameter("p_part_id", partId),
                new MySqlParameter("p_spec_values", "{\"width\":\"42\"}"),
                new MySqlParameter("p_image_path", DBNull.Value),
                new MySqlParameter("p_quantity_type", "Sheets"),
                new MySqlParameter("p_home_location", "PART-NEW-LOC"),
                new MySqlParameter("p_user", "integration.user")
            );

            var updatedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_parts WHERE id = @id AND home_location = 'PART-NEW-LOC' AND quantity_type = 'Sheets';",
                    new MySqlParameter("@id", partRowId)
                )
            );
            updatedRows.Should().Be(1);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_dunnage_parts_search_ShouldReturnMatchingPart_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPSEA-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"FindMe-{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_parts (part_id, type_id, spec_values, quantity_type, home_location, created_by, created_date)
VALUES (@partId, @typeId, '{""material"":""steel""}', 'Quantity', 'SEARCH-LOC', 'integration.user', NOW());",
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@typeId", typeId)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_Search",
                new MySqlParameter("p_search_text", "findme"),
                new MySqlParameter("p_type_id", 0)
            );

            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["part_id"]?.ToString(),
                        partId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Specs_GetAll_ShouldReturnSeededSpec_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DSGA-T-{CreateUniqueSuffix()[..6]}";
        var specKey = $"Spec{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_specs (type_id, spec_key, spec_value, created_by, created_date) VALUES (@typeId, @specKey, '{\"value\":\"A\"}', 'integration.user', NOW());",
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@specKey", specKey)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Specs_GetAll"
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["spec_key"]?.ToString(),
                        specKey,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_specs WHERE spec_key = @specKey;",
                new MySqlParameter("@specKey", specKey)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Specs_GetAllKeys_ShouldReturnKeysFromSeededSpecJson_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DSGAK-T-{CreateUniqueSuffix()[..6]}";
        var specKey = $"Parent{CreateUniqueSuffix()[..6]}";
        var jsonKey = $"Key{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                $"INSERT INTO dunnage_specs (type_id, spec_key, spec_value, created_by, created_date) VALUES (@typeId, @specKey, '{{\"{jsonKey}\":\"A\"}}', 'integration.user', NOW());",
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@specKey", specKey)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Specs_GetAllKeys"
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["SpecKey"]?.ToString(),
                        jsonKey,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_specs WHERE spec_key = @specKey;",
                new MySqlParameter("@specKey", specKey)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Specs_GetById_ShouldReturnSeededSpec_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DSGBI-T-{CreateUniqueSuffix()[..6]}";
        var specKey = $"Spec{CreateUniqueSuffix()[..6]}";
        var specId = 0;

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_specs (type_id, spec_key, spec_value, created_by, created_date) VALUES (@typeId, @specKey, '{\"value\":\"B\"}', 'integration.user', NOW());",
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@specKey", specKey)
            );
            specId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_specs WHERE type_id = @typeId AND spec_key = @specKey;",
                    new MySqlParameter("@typeId", typeId),
                    new MySqlParameter("@specKey", specKey)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Specs_GetById",
                new MySqlParameter("p_id", specId)
            );
            result.Rows.Count.Should().Be(1);
            result.Rows[0]["spec_key"].Should().Be(specKey);
        }
        finally
        {
            if (specId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM dunnage_specs WHERE id = @id;",
                    new MySqlParameter("@id", specId)
                );
            }
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Specs_GetByType_ShouldReturnSeededSpec_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DSGBT-T-{CreateUniqueSuffix()[..6]}";
        var specKey = $"Spec{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_specs (type_id, spec_key, spec_value, created_by, created_date) VALUES (@typeId, @specKey, '{\"value\":\"C\"}', 'integration.user', NOW());",
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@specKey", specKey)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Specs_GetByType",
                new MySqlParameter("p_type_id", typeId)
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["spec_key"]?.ToString(),
                        specKey,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_specs WHERE spec_key = @specKey;",
                new MySqlParameter("@specKey", specKey)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Specs_CountPartsUsingSpec_ShouldReturnSeededPartCount_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DSCPS-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DSCPS-P-{CreateUniqueSuffix()[..6]}";
        const string specKey = "color";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_parts (part_id, type_id, spec_values, quantity_type, home_location, created_by, created_date)
VALUES (@partId, @typeId, '{""color"":""blue""}', 'Quantity', 'SPEC-COUNT-LOC', 'integration.user', NOW());",
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@typeId", typeId)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Specs_CountPartsUsingSpec",
                new MySqlParameter("p_type_id", typeId),
                new MySqlParameter("p_spec_key", specKey)
            );
            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["part_count"]).Should().Be(1);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Specs_DeleteById_ShouldDeleteSeededSpec_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DSDBI-T-{CreateUniqueSuffix()[..6]}";
        var specKey = $"Spec{CreateUniqueSuffix()[..6]}";
        var specId = 0;

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_specs (type_id, spec_key, spec_value, created_by, created_date) VALUES (@typeId, @specKey, '{\"value\":\"D\"}', 'integration.user', NOW());",
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@specKey", specKey)
            );
            specId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_specs WHERE type_id = @typeId AND spec_key = @specKey;",
                    new MySqlParameter("@typeId", typeId),
                    new MySqlParameter("@specKey", specKey)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Specs_DeleteById",
                new MySqlParameter("p_id", specId)
            );

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_specs WHERE id = @id;",
                    new MySqlParameter("@id", specId)
                )
            );
            remainingRows.Should().Be(0);
        }
        finally
        {
            if (specId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM dunnage_specs WHERE id = @id;",
                    new MySqlParameter("@id", specId)
                );
            }
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Specs_DeleteByType_ShouldDeleteSeededSpecs_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DSDBT-T-{CreateUniqueSuffix()[..6]}";
        var specKey = $"Spec{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_specs (type_id, spec_key, spec_value, created_by, created_date) VALUES (@typeId, @specKey, '{\"value\":\"E\"}', 'integration.user', NOW());",
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@specKey", specKey)
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Specs_DeleteByType",
                new MySqlParameter("p_type_id", typeId)
            );

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_specs WHERE type_id = @typeId;",
                    new MySqlParameter("@typeId", typeId)
                )
            );
            remainingRows.Should().Be(0);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_specs WHERE spec_key = @specKey;",
                new MySqlParameter("@specKey", specKey)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_dunnage_specs_insert_ShouldInsertSpec_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DSINS-T-{CreateUniqueSuffix()[..6]}";
        var specKey = $"Spec{CreateUniqueSuffix()[..6]}";
        var newIdParameter = new MySqlParameter("p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Specs_Insert",
                new MySqlParameter("p_type_id", typeId),
                new MySqlParameter("p_spec_key", specKey),
                new MySqlParameter("p_spec_value", "{\"value\":\"inserted\"}"),
                new MySqlParameter("p_user", "integration.user"),
                newIdParameter
            );

            var newId = ConvertToInt(result.OutputValues["p_new_id"]);
            newId.Should().BeGreaterThan(0);

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_specs WHERE id = @id AND spec_key = @specKey;",
                    new MySqlParameter("@id", newId),
                    new MySqlParameter("@specKey", specKey)
                )
            );
            savedRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_specs WHERE spec_key = @specKey;",
                new MySqlParameter("@specKey", specKey)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_dunnage_specs_update_ShouldUpdateSeededSpec_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DSUPD-T-{CreateUniqueSuffix()[..6]}";
        var specKey = $"Spec{CreateUniqueSuffix()[..6]}";
        var specId = 0;

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_specs (type_id, spec_key, spec_value, created_by, created_date) VALUES (@typeId, @specKey, '{\"value\":\"old\"}', 'integration.user', NOW());",
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@specKey", specKey)
            );
            specId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_specs WHERE type_id = @typeId AND spec_key = @specKey;",
                    new MySqlParameter("@typeId", typeId),
                    new MySqlParameter("@specKey", specKey)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Specs_Update",
                new MySqlParameter("p_id", specId),
                new MySqlParameter("p_spec_value", "{\"value\":\"new\"}"),
                new MySqlParameter("p_user", "integration.user")
            );

            var savedValue = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT JSON_UNQUOTE(JSON_EXTRACT(spec_value, '$.value')) FROM dunnage_specs WHERE id = @id;",
                new MySqlParameter("@id", specId)
            );
            savedValue.Should().Be("new");
        }
        finally
        {
            if (specId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM dunnage_specs WHERE id = @id;",
                    new MySqlParameter("@id", specId)
                );
            }
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_dunnage_parts_delete_ShouldDeleteSeededPart_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPDEL-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DPDEL-P-{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "PART-DEL-LOC",
                "integration.user"
            );
            var partRowId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_parts WHERE part_id = @partId;",
                    new MySqlParameter("@partId", partId)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_Delete",
                new MySqlParameter("p_id", partRowId)
            );

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_parts WHERE id = @id;",
                    new MySqlParameter("@id", partRowId)
                )
            );
            remainingRows.Should().Be(0);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Parts_InsertWithInventory_ShouldInsertPartAndInventoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPINV-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DPINV-P-{CreateUniqueSuffix()[..6]}";
        var newIdParameter = new MySqlParameter("p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_InsertWithInventory",
                new MySqlParameter("p_part_id", partId),
                new MySqlParameter("p_type_id", typeId),
                new MySqlParameter("p_spec_values", "{\"value\":\"inv\"}"),
                new MySqlParameter("p_image_path", DBNull.Value),
                new MySqlParameter("p_quantity_type", "Quantity"),
                new MySqlParameter("p_home_location", "INV-LOC"),
                new MySqlParameter("p_inventory_method", "Manual"),
                new MySqlParameter("p_inventory_notes", "Needs Visual tracking"),
                new MySqlParameter("p_user", "integration.user"),
                newIdParameter
            );

            ConvertToInt(result.OutputValues["p_new_id"]).Should().BeGreaterThan(0);
            var inventoryRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_requires_inventory WHERE part_id = @partId;",
                    new MySqlParameter("@partId", partId)
                )
            );
            inventoryRows.Should().Be(1);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Parts_UpdateWithReferences_ShouldUpdatePartAndReferences_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DPREF-T-{CreateUniqueSuffix()[..6]}";
        var originalPartId = $"DPREF-O-{CreateUniqueSuffix()[..6]}";
        var newPartId = $"DPREF-N-{CreateUniqueSuffix()[..6]}";
        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                originalPartId,
                typeId,
                "REF-OLD-LOC",
                "integration.user"
            );
            var partRowId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_parts WHERE part_id = @partId;",
                    new MySqlParameter("@partId", originalPartId)
                )
            );

            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_requires_inventory (part_id, inventory_method, notes, created_by, created_date) VALUES (@partId, 'Manual', 'Old note', 'integration.user', NOW());",
                new MySqlParameter("@partId", originalPartId)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_history (load_uuid, part_id, quantity, quantity_type, received_date, created_by, employee_number, created_date, type_id, type_name, type_icon, location)
VALUES (@loadUuid, @partId, 1, 'Quantity', NOW(), 'integration.user', 9333, NOW(), @typeId, @typeName, 'PackageVariantClosed', 'REF-HIST-LOC');",
                new MySqlParameter("@loadUuid", loadUuid),
                new MySqlParameter("@partId", originalPartId),
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@typeName", typeName)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_label_data (load_uuid, part_id, dunnage_type_id, dunnage_type_name, dunnage_type_icon, quantity, quantity_type, received_date, user_id, employee_number, location)
VALUES (@loadUuid, @partId, @typeId, @typeName, 'PackageVariantClosed', 1, 'Quantity', NOW(), 'integration.user', 9333, 'REF-LABEL-LOC');",
                new MySqlParameter("@loadUuid", loadUuid),
                new MySqlParameter("@partId", originalPartId),
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@typeName", typeName)
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Parts_UpdateWithReferences",
                new MySqlParameter("p_id", partRowId),
                new MySqlParameter("p_original_part_id", originalPartId),
                new MySqlParameter("p_new_part_id", newPartId),
                new MySqlParameter("p_spec_values", "{\"value\":\"updated\"}"),
                new MySqlParameter("p_image_path", DBNull.Value),
                new MySqlParameter("p_quantity_type", "Quantity"),
                new MySqlParameter("p_home_location", "REF-NEW-LOC"),
                new MySqlParameter("p_inventory_method", "Automatic"),
                new MySqlParameter("p_inventory_notes", "Updated note"),
                new MySqlParameter("p_user", "integration.user")
            );

            var updatedPartRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_parts WHERE id = @id AND part_id = @newPartId AND home_location = 'REF-NEW-LOC';",
                    new MySqlParameter("@id", partRowId),
                    new MySqlParameter("@newPartId", newPartId)
                )
            );
            var updatedHistoryRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_history WHERE load_uuid = @loadUuid AND part_id = @newPartId;",
                    new MySqlParameter("@loadUuid", loadUuid),
                    new MySqlParameter("@newPartId", newPartId)
                )
            );
            var updatedLabelRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_label_data WHERE load_uuid = @loadUuid AND part_id = @newPartId;",
                    new MySqlParameter("@loadUuid", loadUuid),
                    new MySqlParameter("@newPartId", newPartId)
                )
            );
            var updatedInventoryRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_requires_inventory WHERE part_id = @newPartId AND inventory_method = 'Automatic';",
                    new MySqlParameter("@newPartId", newPartId)
                )
            );

            updatedPartRows.Should().Be(1);
            updatedHistoryRows.Should().Be(1);
            updatedLabelRows.Should().Be(1);
            updatedInventoryRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_label_data WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_requires_inventory WHERE part_id IN (@originalPartId, @newPartId);",
                new MySqlParameter("@originalPartId", originalPartId),
                new MySqlParameter("@newPartId", newPartId)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_parts WHERE part_id IN (@originalPartId, @newPartId);",
                new MySqlParameter("@originalPartId", originalPartId),
                new MySqlParameter("@newPartId", newPartId)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Types_GetUsageCount_ShouldReturnSeededUsageCount_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DTUSE-{CreateUniqueSuffix()[..8]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            var partPrefix = $"U{CreateUniqueSuffix()[..5].ToUpperInvariant()}";
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO receiving_package_type_mapping (part_prefix, package_type, is_default, display_order, is_active, created_by) VALUES (@partPrefix, @packageType, 0, 5, 1, NULL);",
                new MySqlParameter("@partPrefix", partPrefix),
                new MySqlParameter("@packageType", typeName)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Types_GetUsageCount",
                new MySqlParameter("p_id", typeId)
            );
            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["usage_count"]).Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_package_type_mapping WHERE package_type = @packageType;",
                new MySqlParameter("@packageType", typeName)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_CustomFields_GetByType_ShouldReturnSeededField_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DCFGT-T-{CreateUniqueSuffix()[..6]}";
        var fieldName = $"Field{CreateUniqueSuffix()[..6]}";
        var columnName = $"col_{CreateUniqueSuffix()[..6].ToLowerInvariant()}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_custom_fields (DunnageTypeID, FieldName, DatabaseColumnName, FieldType, DisplayOrder, IsRequired, ValidationRules, CreatedBy) VALUES (@typeId, @fieldName, @columnName, 'Text', 1, 0, NULL, 'integration.user');",
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@fieldName", fieldName),
                new MySqlParameter("@columnName", columnName)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_CustomFields_GetByType",
                new MySqlParameter("p_dunnage_type_id", typeId)
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["FieldName"]?.ToString(),
                        fieldName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_custom_fields WHERE DatabaseColumnName = @columnName;",
                new MySqlParameter("@columnName", columnName)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_CustomFields_Delete_ShouldDeleteSeededField_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DCFDEL-T-{CreateUniqueSuffix()[..6]}";
        var columnName = $"col_{CreateUniqueSuffix()[..6].ToLowerInvariant()}";
        var fieldId = 0;

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_custom_fields (DunnageTypeID, FieldName, DatabaseColumnName, FieldType, DisplayOrder, IsRequired, ValidationRules, CreatedBy) VALUES (@typeId, 'DeleteField', @columnName, 'Text', 1, 0, NULL, 'integration.user');",
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@columnName", columnName)
            );
            fieldId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT ID FROM dunnage_custom_fields WHERE DatabaseColumnName = @columnName;",
                    new MySqlParameter("@columnName", columnName)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_CustomFields_Delete",
                new MySqlParameter("p_field_id", fieldId)
            );

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_custom_fields WHERE ID = @id;",
                    new MySqlParameter("@id", fieldId)
                )
            );
            remainingRows.Should().Be(0);
        }
        finally
        {
            if (fieldId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM dunnage_custom_fields WHERE ID = @id;",
                    new MySqlParameter("@id", fieldId)
                );
            }
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_CustomFields_Insert_ShouldInsertField_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DCFINS-T-{CreateUniqueSuffix()[..6]}";
        var fieldName = $"Field{CreateUniqueSuffix()[..6]}";
        var columnName = $"col_{CreateUniqueSuffix()[..6].ToLowerInvariant()}";
        var newIdParameter = new MySqlParameter("p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var statusParameter = new MySqlParameter("p_status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorParameter = new MySqlParameter("p_error_msg", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_CustomFields_Insert",
                new MySqlParameter("p_dunnage_type_id", typeId),
                new MySqlParameter("p_field_name", fieldName),
                new MySqlParameter("p_database_column_name", columnName),
                new MySqlParameter("p_field_type", "Text"),
                new MySqlParameter("p_display_order", 1),
                new MySqlParameter("p_is_required", 0),
                new MySqlParameter("p_validation_rules", DBNull.Value),
                new MySqlParameter("p_user", "integration.user"),
                newIdParameter,
                statusParameter,
                errorParameter
            );

            ConvertToInt(result.OutputValues["p_status"]).Should().Be(1);
            result.OutputValues["p_error_msg"].Should().Be("Custom field created successfully");
            ConvertToInt(result.OutputValues["p_new_id"]).Should().BeGreaterThan(0);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_custom_fields WHERE DatabaseColumnName = @columnName;",
                new MySqlParameter("@columnName", columnName)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_CustomFields_Update_ShouldUpdateSeededField_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DCFUPD-T-{CreateUniqueSuffix()[..6]}";
        var originalColumnName = $"col_{CreateUniqueSuffix()[..6].ToLowerInvariant()}";
        var updatedColumnName = $"upd_{CreateUniqueSuffix()[..6].ToLowerInvariant()}";
        var fieldId = 0;

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_custom_fields (DunnageTypeID, FieldName, DatabaseColumnName, FieldType, DisplayOrder, IsRequired, ValidationRules, CreatedBy) VALUES (@typeId, 'OldField', @columnName, 'Text', 1, 0, NULL, 'integration.user');",
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@columnName", originalColumnName)
            );
            fieldId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT ID FROM dunnage_custom_fields WHERE DatabaseColumnName = @columnName;",
                    new MySqlParameter("@columnName", originalColumnName)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_CustomFields_Update",
                new MySqlParameter("p_field_id", fieldId),
                new MySqlParameter("p_field_name", "UpdatedField"),
                new MySqlParameter("p_database_column_name", updatedColumnName),
                new MySqlParameter("p_field_type", "Number"),
                new MySqlParameter("p_display_order", 2),
                new MySqlParameter("p_is_required", 1),
                new MySqlParameter("p_validation_rules", "{\"min\":1}")
            );

            var updatedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_custom_fields WHERE ID = @id AND FieldName = 'UpdatedField' AND DatabaseColumnName = @columnName AND FieldType = 'Number' AND DisplayOrder = 2 AND IsRequired = 1;",
                    new MySqlParameter("@id", fieldId),
                    new MySqlParameter("@columnName", updatedColumnName)
                )
            );
            updatedRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_custom_fields WHERE ID = @id OR DatabaseColumnName IN (@originalColumnName, @updatedColumnName);",
                new MySqlParameter("@id", fieldId),
                new MySqlParameter("@originalColumnName", originalColumnName),
                new MySqlParameter("@updatedColumnName", updatedColumnName)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_QualityHolds_GetByLoadID_ShouldReturnSeededHold_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadId = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COALESCE(MIN(load_id), 0) FROM receiving_quality_holds;"
            )
        );
        if (loadId <= 0)
        {
            return;
        }

        var partId = $"QHGET-{CreateUniqueSuffix()[..8]}";
        var statusParameter = new MySqlParameter("p_Status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorParameter = new MySqlParameter("p_ErrorMsg", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };
        var holdId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO receiving_quality_holds (load_id, part_id, restriction_type, quality_acknowledged_by, quality_acknowledged_at, created_at) VALUES (@loadId, @partId, 'MMFSR', 'quality.user', NOW(), NOW());",
                new MySqlParameter("@loadId", loadId),
                new MySqlParameter("@partId", partId)
            );
            holdId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT MAX(quality_hold_id) FROM receiving_quality_holds WHERE load_id = @loadId AND part_id = @partId;",
                    new MySqlParameter("@loadId", loadId),
                    new MySqlParameter("@partId", partId)
                )
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_QualityHolds_GetByLoadID",
                new MySqlParameter("p_LoadID", loadId),
                statusParameter,
                errorParameter
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row => ConvertToInt(row["quality_hold_id"]) == holdId)
                .Should()
                .BeTrue();
        }
        finally
        {
            if (holdId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_quality_holds WHERE quality_hold_id = @id;",
                    new MySqlParameter("@id", holdId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_QualityHolds_Insert_ShouldInsertHold_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadId = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COALESCE(MIN(load_id), 0) FROM receiving_quality_holds;"
            )
        );
        if (loadId <= 0)
        {
            return;
        }

        var partId = $"QHINS-{CreateUniqueSuffix()[..8]}";
        var holdIdParameter = new MySqlParameter("p_QualityHoldID", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var statusParameter = new MySqlParameter("p_Status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorParameter = new MySqlParameter("p_ErrorMsg", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };
        var insertedHoldId = 0;

        try
        {
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_QualityHolds_Insert",
                new MySqlParameter("p_LoadID", loadId),
                new MySqlParameter("p_PartID", partId),
                new MySqlParameter("p_RestrictionType", "MMCSR"),
                new MySqlParameter("p_QualityAcknowledgedBy", "quality.insert"),
                new MySqlParameter("p_QualityAcknowledgedAt", DateTime.UtcNow),
                holdIdParameter,
                statusParameter,
                errorParameter
            );

            ConvertToInt(result.OutputValues["p_Status"]).Should().Be(1);
            insertedHoldId = ConvertToInt(result.OutputValues["p_QualityHoldID"]);
            insertedHoldId.Should().BeGreaterThan(0);

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_quality_holds WHERE quality_hold_id = @id AND part_id = @partId;",
                    new MySqlParameter("@id", insertedHoldId),
                    new MySqlParameter("@partId", partId)
                )
            );
            savedRows.Should().Be(1);
        }
        finally
        {
            if (insertedHoldId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_quality_holds WHERE quality_hold_id = @id;",
                    new MySqlParameter("@id", insertedHoldId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Receiving_QualityHolds_Update_ShouldUpdateSeededHold_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadId = ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COALESCE(MIN(load_id), 0) FROM receiving_quality_holds;"
            )
        );
        if (loadId <= 0)
        {
            return;
        }

        var partId = $"QHUPD-{CreateUniqueSuffix()[..8]}";
        var statusParameter = new MySqlParameter("p_Status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorParameter = new MySqlParameter("p_ErrorMsg", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };
        var holdId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO receiving_quality_holds (load_id, part_id, restriction_type, quality_acknowledged_by, quality_acknowledged_at, created_at) VALUES (@loadId, @partId, 'MMFSR', NULL, NULL, NOW());",
                new MySqlParameter("@loadId", loadId),
                new MySqlParameter("@partId", partId)
            );
            holdId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT MAX(quality_hold_id) FROM receiving_quality_holds WHERE load_id = @loadId AND part_id = @partId;",
                    new MySqlParameter("@loadId", loadId),
                    new MySqlParameter("@partId", partId)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_QualityHolds_Update",
                new MySqlParameter("p_QualityHoldID", holdId),
                new MySqlParameter("p_QualityAcknowledgedBy", "quality.update"),
                new MySqlParameter("p_QualityAcknowledgedAt", DateTime.UtcNow),
                statusParameter,
                errorParameter
            );

            var updatedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_quality_holds WHERE quality_hold_id = @id AND quality_acknowledged_by = 'quality.update' AND quality_acknowledged_at IS NOT NULL;",
                    new MySqlParameter("@id", holdId)
                )
            );
            updatedRows.Should().Be(1);
        }
        finally
        {
            if (holdId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM receiving_quality_holds WHERE quality_hold_id = @id;",
                    new MySqlParameter("@id", holdId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Dunnage_Inventory_Check_ShouldReturnTrue_WhenSeededInventoryExists_AndIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DIC-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DIC-P-{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "INV-CHECK-LOC",
                "integration.user"
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_requires_inventory (part_id, inventory_method, notes, created_by, created_date) VALUES (@partId, 'Manual', 'check', 'integration.user', NOW());",
                new MySqlParameter("@partId", partId)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Inventory_Check",
                new MySqlParameter("p_part_id", partId)
            );
            result.Rows.Count.Should().Be(1);
            Convert.ToBoolean(result.Rows[0]["requires_inventory"]).Should().BeTrue();
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Inventory_Delete_ShouldDeleteSeededInventoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DID-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DID-P-{CreateUniqueSuffix()[..6]}";
        var inventoryId = 0;

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "INV-DEL-LOC",
                "integration.user"
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_requires_inventory (part_id, inventory_method, notes, created_by, created_date) VALUES (@partId, 'Manual', 'delete', 'integration.user', NOW());",
                new MySqlParameter("@partId", partId)
            );
            inventoryId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_requires_inventory WHERE part_id = @partId;",
                    new MySqlParameter("@partId", partId)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Inventory_Delete",
                new MySqlParameter("p_id", inventoryId)
            );

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_requires_inventory WHERE id = @id;",
                    new MySqlParameter("@id", inventoryId)
                )
            );
            remainingRows.Should().Be(0);
        }
        finally
        {
            if (inventoryId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM dunnage_requires_inventory WHERE id = @id;",
                    new MySqlParameter("@id", inventoryId)
                );
            }
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Inventory_GetAll_ShouldReturnSeededInventoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DIGA-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DIGA-P-{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "INV-GETALL-LOC",
                "integration.user"
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_requires_inventory (part_id, inventory_method, notes, created_by, created_date) VALUES (@partId, 'Manual', 'getall', 'integration.user', NOW());",
                new MySqlParameter("@partId", partId)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Inventory_GetAll"
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["part_id"]?.ToString(),
                        partId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Inventory_GetByPart_ShouldReturnSeededInventoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DIGP-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DIGP-P-{CreateUniqueSuffix()[..6]}";

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "INV-GETPART-LOC",
                "integration.user"
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_requires_inventory (part_id, inventory_method, notes, created_by, created_date) VALUES (@partId, 'Automatic', 'getbypart', 'integration.user', NOW());",
                new MySqlParameter("@partId", partId)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Inventory_GetByPart",
                new MySqlParameter("p_part_id", partId)
            );
            result.Rows.Count.Should().Be(1);
            result.Rows[0]["part_id"].Should().Be(partId);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Inventory_Insert_ShouldInsertInventoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DIINS-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DIINS-P-{CreateUniqueSuffix()[..6]}";
        var newIdParameter = new MySqlParameter("p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "INV-INSERT-LOC",
                "integration.user"
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Inventory_Insert",
                new MySqlParameter("p_part_id", partId),
                new MySqlParameter("p_inventory_method", "Manual"),
                new MySqlParameter("p_notes", "insert inventory"),
                new MySqlParameter("p_user", "integration.user"),
                newIdParameter
            );

            var newId = ConvertToInt(result.OutputValues["p_new_id"]);
            newId.Should().BeGreaterThan(0);
        }
        finally
        {
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_Inventory_Update_ShouldUpdateSeededInventoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DIUPD-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DIUPD-P-{CreateUniqueSuffix()[..6]}";
        var inventoryId = 0;

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await InsertDunnagePartAsync(
                connectionString,
                partId,
                typeId,
                "INV-UPD-LOC",
                "integration.user"
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO dunnage_requires_inventory (part_id, inventory_method, notes, created_by, created_date) VALUES (@partId, 'Manual', 'old', 'integration.user', NOW());",
                new MySqlParameter("@partId", partId)
            );
            inventoryId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM dunnage_requires_inventory WHERE part_id = @partId;",
                    new MySqlParameter("@partId", partId)
                )
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Inventory_Update",
                new MySqlParameter("p_id", inventoryId),
                new MySqlParameter("p_part_id", partId),
                new MySqlParameter("p_inventory_method", "Automatic"),
                new MySqlParameter("p_notes", "updated"),
                new MySqlParameter("p_user", "integration.user")
            );

            var updatedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_requires_inventory WHERE id = @id AND inventory_method = 'Automatic' AND notes = 'updated';",
                    new MySqlParameter("@id", inventoryId)
                )
            );
            updatedRows.Should().Be(1);
        }
        finally
        {
            if (inventoryId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM dunnage_requires_inventory WHERE id = @id;",
                    new MySqlParameter("@id", inventoryId)
                );
            }
            await DeleteDunnagePartAndTypeAsync(connectionString, partId, typeName);
        }
    }

    [Fact]
    public async Task sp_Dunnage_LabelData_GetAll_ShouldReturnSeededQueueRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DLGA-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DLGA-P-{CreateUniqueSuffix()[..6]}";
        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_label_data (load_uuid, part_id, dunnage_type_id, dunnage_type_name, dunnage_type_icon, quantity, quantity_type, received_date, user_id, employee_number, location)
VALUES (@loadUuid, @partId, @typeId, @typeName, 'PackageVariantClosed', 2, 'Quantity', NOW(), 'integration.user', 1001, 'LABEL-GETALL-LOC');",
                new MySqlParameter("@loadUuid", loadUuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@typeName", typeName)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_LabelData_GetAll"
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["load_uuid"]?.ToString(),
                        loadUuid,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_label_data WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_LabelData_Insert_ShouldInsertQueueRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DLINS-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DLINS-P-{CreateUniqueSuffix()[..6]}";
        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_LabelData_Insert",
                new MySqlParameter("p_load_uuid", loadUuid),
                new MySqlParameter("p_part_id", partId),
                new MySqlParameter("p_dunnage_type_id", typeId),
                new MySqlParameter("p_dunnage_type_name", typeName),
                new MySqlParameter("p_dunnage_type_icon", "PackageVariantClosed"),
                new MySqlParameter("p_quantity", 3m),
                new MySqlParameter("p_quantity_type", "Quantity"),
                new MySqlParameter("p_po_number", DBNull.Value),
                new MySqlParameter("p_received_date", DateTime.UtcNow),
                new MySqlParameter("p_user_id", "integration.user"),
                new MySqlParameter("p_employee_number", 1002),
                new MySqlParameter("p_location", "LABEL-INSERT-LOC"),
                new MySqlParameter("p_label_number", "LBL-1"),
                new MySqlParameter("p_part_skid_sequence", 1),
                new MySqlParameter("p_part_skid_total", 1),
                new MySqlParameter("p_specs_json", "{\"color\":\"green\"}")
            );

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_label_data WHERE load_uuid = @loadUuid AND part_id = @partId;",
                    new MySqlParameter("@loadUuid", loadUuid),
                    new MySqlParameter("@partId", partId)
                )
            );
            savedRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_label_data WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_LabelData_InsertBatch_ShouldInsertMultipleQueueRows_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadUuid1 = Guid.NewGuid().ToString();
        var loadUuid2 = Guid.NewGuid().ToString();
        const string payload =
            "[{\"load_uuid\":\"__LOAD1__\",\"part_id\":\"BATCH-1\",\"dunnage_type_id\":null,\"dunnage_type_name\":null,\"dunnage_type_icon\":null,\"quantity\":1,\"quantity_type\":\"Quantity\",\"po_number\":null,\"received_date\":\"2026-05-26 08:00:00\",\"location\":\"BATCH-LOC\",\"label_number\":\"1\",\"part_skid_sequence\":1,\"part_skid_total\":1,\"specs_json\":{\"a\":1}},{\"load_uuid\":\"__LOAD2__\",\"part_id\":\"BATCH-2\",\"dunnage_type_id\":null,\"dunnage_type_name\":null,\"dunnage_type_icon\":null,\"quantity\":2,\"quantity_type\":\"Quantity\",\"po_number\":null,\"received_date\":\"2026-05-26 08:01:00\",\"location\":\"BATCH-LOC\",\"label_number\":\"2\",\"part_skid_sequence\":1,\"part_skid_total\":1,\"specs_json\":{\"b\":2}}]";
        var batchJson = payload.Replace("__LOAD1__", loadUuid1).Replace("__LOAD2__", loadUuid2);

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_LabelData_InsertBatch",
                new MySqlParameter("p_load_data", batchJson),
                new MySqlParameter("p_user", "integration.user")
            );

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_label_data WHERE load_uuid IN (@loadUuid1, @loadUuid2);",
                    new MySqlParameter("@loadUuid1", loadUuid1),
                    new MySqlParameter("@loadUuid2", loadUuid2)
                )
            );
            savedRows.Should().Be(2);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_label_data WHERE load_uuid IN (@loadUuid1, @loadUuid2);",
                new MySqlParameter("@loadUuid1", loadUuid1),
                new MySqlParameter("@loadUuid2", loadUuid2)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_LabelData_Update_ShouldUpdateSeededQueueRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var typeName = $"DLUPD-T-{CreateUniqueSuffix()[..6]}";
        var partId = $"DLUPD-P-{CreateUniqueSuffix()[..6]}";
        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            var typeId = await InsertDunnageTypeAsync(connectionString, typeName);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_label_data (load_uuid, part_id, dunnage_type_id, dunnage_type_name, dunnage_type_icon, quantity, quantity_type, received_date, user_id, employee_number, location)
VALUES (@loadUuid, @partId, @typeId, @typeName, 'PackageVariantClosed', 1, 'Quantity', NOW(), 'integration.user', 1003, 'LABEL-OLD-LOC');",
                new MySqlParameter("@loadUuid", loadUuid),
                new MySqlParameter("@partId", partId),
                new MySqlParameter("@typeId", typeId),
                new MySqlParameter("@typeName", typeName)
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_LabelData_Update",
                new MySqlParameter("p_load_uuid", loadUuid),
                new MySqlParameter("p_part_id", partId),
                new MySqlParameter("p_dunnage_type_id", typeId),
                new MySqlParameter("p_dunnage_type_name", typeName),
                new MySqlParameter("p_dunnage_type_icon", "PackageVariantClosed"),
                new MySqlParameter("p_quantity", 9m),
                new MySqlParameter("p_quantity_type", "Boxes"),
                new MySqlParameter("p_po_number", DBNull.Value),
                new MySqlParameter("p_received_date", DateTime.UtcNow),
                new MySqlParameter("p_user_id", "integration.user"),
                new MySqlParameter("p_employee_number", 1003),
                new MySqlParameter("p_location", "LABEL-NEW-LOC"),
                new MySqlParameter("p_label_number", "L9"),
                new MySqlParameter("p_part_skid_sequence", 1),
                new MySqlParameter("p_part_skid_total", 2),
                new MySqlParameter("p_specs_json", "{\"size\":\"XL\"}")
            );

            var updatedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_label_data WHERE load_uuid = @loadUuid AND quantity = 9 AND location = 'LABEL-NEW-LOC' AND quantity_type = 'Boxes';",
                    new MySqlParameter("@loadUuid", loadUuid)
                )
            );
            updatedRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_label_data WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_LabelData_Delete_ShouldDeleteSeededQueueRows_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_label_data (load_uuid, part_id, quantity, quantity_type, received_date, user_id, employee_number)
VALUES (@loadUuid, 'DLDEL', 1, 'Quantity', NOW(), 'integration.user', 1004);",
                new MySqlParameter("@loadUuid", loadUuid)
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_LabelData_Delete",
                new MySqlParameter("p_load_uuid", loadUuid)
            );

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_label_data WHERE load_uuid = @loadUuid;",
                    new MySqlParameter("@loadUuid", loadUuid)
                )
            );
            remainingRows.Should().Be(0);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_label_data WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_LabelData_ClearToHistory_ShouldMoveSeededQueueRowsToHistory_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadUuid = Guid.NewGuid().ToString();
        var rowsMovedParameter = new MySqlParameter("p_rows_moved", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var archiveBatchIdParameter = new MySqlParameter(
            "p_archive_batch_id",
            MySqlDbType.VarChar,
            36
        )
        {
            Direction = ParameterDirection.Output,
        };
        var statusParameter = new MySqlParameter("p_status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorParameter = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 1000)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_label_data (load_uuid, part_id, quantity, quantity_type, received_date, user_id, employee_number, location, label_number)
VALUES (@loadUuid, 'DLCLEAR', 1, 'Quantity', NOW(), 'integration.user', 1005, 'CLEAR-LOC', 'CL-1');",
                new MySqlParameter("@loadUuid", loadUuid)
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_LabelData_ClearToHistory",
                new MySqlParameter("p_archived_by", "integration.user"),
                new MySqlParameter("p_employee_number", 1005),
                new MySqlParameter("p_clear_all", 0),
                rowsMovedParameter,
                archiveBatchIdParameter,
                statusParameter,
                errorParameter
            );

            ConvertToInt(result.OutputValues["p_rows_moved"]).Should().Be(1);
            ConvertToInt(result.OutputValues["p_status"]).Should().Be(0);

            var queueRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_label_data WHERE load_uuid = @loadUuid;",
                    new MySqlParameter("@loadUuid", loadUuid)
                )
            );
            var historyRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_history WHERE load_uuid = @loadUuid;",
                    new MySqlParameter("@loadUuid", loadUuid)
                )
            );
            queueRows.Should().Be(0);
            historyRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_label_data WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Loads_GetAll_ShouldReturnSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_history (load_uuid, part_id, quantity, quantity_type, received_date, created_by, employee_number, created_date, location)
VALUES (@loadUuid, 'DLGA-HIST', 1, 'Quantity', NOW(), 'integration.user', 1006, NOW(), 'LOAD-ALL-LOC');",
                new MySqlParameter("@loadUuid", loadUuid)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Loads_GetAll"
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["load_uuid"]?.ToString(),
                        loadUuid,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Loads_GetByDateRange_ShouldReturnSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadUuid = Guid.NewGuid().ToString();
        var receivedDate = DateTime.UtcNow;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_history (load_uuid, part_id, quantity, quantity_type, received_date, created_by, employee_number, created_date, location)
VALUES (@loadUuid, 'DLGBR-HIST', 2, 'Quantity', @receivedDate, 'integration.user', 1007, @receivedDate, 'LOAD-RANGE-LOC');",
                new MySqlParameter("@loadUuid", loadUuid),
                new MySqlParameter("@receivedDate", receivedDate)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Loads_GetByDateRange",
                new MySqlParameter("p_start_date", receivedDate.AddMinutes(-5)),
                new MySqlParameter("p_end_date", receivedDate.AddMinutes(5))
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["load_uuid"]?.ToString(),
                        loadUuid,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Loads_GetById_ShouldReturnSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_history (load_uuid, part_id, quantity, quantity_type, received_date, created_by, employee_number, created_date, location)
VALUES (@loadUuid, 'DLGBI-HIST', 3, 'Quantity', NOW(), 'integration.user', 1008, NOW(), 'LOAD-ID-LOC');",
                new MySqlParameter("@loadUuid", loadUuid)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Dunnage_Loads_GetById",
                new MySqlParameter("p_load_uuid", loadUuid)
            );
            result.Rows.Count.Should().Be(1);
            result.Rows[0]["load_uuid"].Should().Be(loadUuid);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Loads_Delete_ShouldDeleteSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_history (load_uuid, part_id, quantity, quantity_type, received_date, created_by, employee_number, created_date)
VALUES (@loadUuid, 'DLD-HIST', 4, 'Quantity', NOW(), 'integration.user', 1009, NOW());",
                new MySqlParameter("@loadUuid", loadUuid)
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Loads_Delete",
                new MySqlParameter("p_load_uuid", loadUuid)
            );

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_history WHERE load_uuid = @loadUuid;",
                    new MySqlParameter("@loadUuid", loadUuid)
                )
            );
            remainingRows.Should().Be(0);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
        }
    }

    [Fact]
    public async Task sp_Dunnage_Loads_Update_ShouldUpdateSeededHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadUuid = Guid.NewGuid().ToString();

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO dunnage_history (load_uuid, part_id, quantity, quantity_type, received_date, created_by, employee_number, created_date, location, label_number)
VALUES (@loadUuid, 'DLU-HIST', 5, 'Quantity', NOW(), 'integration.user', 1010, NOW(), 'LOAD-OLD-LOC', 'OLD');",
                new MySqlParameter("@loadUuid", loadUuid)
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Dunnage_Loads_Update",
                new MySqlParameter("p_load_uuid", loadUuid),
                new MySqlParameter("p_part_id", "DLU-HIST-NEW"),
                new MySqlParameter("p_quantity", 7m),
                new MySqlParameter("p_po_number", "PO-DUN-1"),
                new MySqlParameter("p_type_id", DBNull.Value),
                new MySqlParameter("p_type_name", "ManualType"),
                new MySqlParameter("p_type_icon", "PackageVariantClosed"),
                new MySqlParameter("p_location", "LOAD-NEW-LOC"),
                new MySqlParameter("p_label_number", "NEW"),
                new MySqlParameter("p_part_skid_sequence", 1),
                new MySqlParameter("p_part_skid_total", 1),
                new MySqlParameter("p_specs_json", "{\"batch\":1}"),
                new MySqlParameter("p_user", "integration.user")
            );

            var updatedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM dunnage_history WHERE load_uuid = @loadUuid AND part_id = 'DLU-HIST-NEW' AND quantity = 7 AND location = 'LOAD-NEW-LOC';",
                    new MySqlParameter("@loadUuid", loadUuid)
                )
            );
            updatedRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM dunnage_history WHERE load_uuid = @loadUuid;",
                new MySqlParameter("@loadUuid", loadUuid)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_History_Import_ShouldInsertHistoryRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partId = $"RHIMP-{CreateUniqueSuffix()[..8]}";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_History_Import",
                new MySqlParameter("p_Quantity", 2),
                new MySqlParameter("p_PartID", partId),
                new MySqlParameter("p_PONumber", "PO-IMP-1"),
                new MySqlParameter("p_EmployeeNumber", 1011),
                new MySqlParameter("p_Heat", "IMP-HEAT"),
                new MySqlParameter("p_TransactionDate", DateTime.UtcNow.Date),
                new MySqlParameter("p_InitialLocation", "IMP-LOC"),
                new MySqlParameter("p_CoilsOnSkid", 0),
                new MySqlParameter("p_LabelNumber", 1),
                new MySqlParameter("p_VendorName", "Import Vendor"),
                new MySqlParameter("p_PartDescription", "Imported row"),
                new MySqlParameter("p_IsNonPOItem", 0)
            );

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_history WHERE part_id = @partId AND po_number = 'PO-IMP-1';",
                    new MySqlParameter("@partId", partId)
                )
            );
            savedRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_history WHERE part_id = @partId;",
                new MySqlParameter("@partId", partId)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_LabelData_InsertFromHistory_ShouldQueueReprintRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadGuid = Guid.NewGuid().ToString();
        var partId = $"RLIFH-{CreateUniqueSuffix()[..8]}";
        var historyId = 0;

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO receiving_history (load_guid, quantity, part_id, po_number, po_line_number, employee_number, transaction_date, initial_location, label_number, vendor_name, part_description, created_at, user_id)
VALUES (@loadGuid, 1, @partId, 'PO-RPT', '1', 1012, CURDATE(), 'RPT-LOC', 1, 'Reprint Vendor', 'Reprint row', NOW(), 'integration.user');",
                new MySqlParameter("@loadGuid", loadGuid),
                new MySqlParameter("@partId", partId)
            );
            historyId = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT id FROM receiving_history WHERE load_guid = @loadGuid;",
                    new MySqlParameter("@loadGuid", loadGuid)
                )
            );

            await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Receiving_LabelData_InsertFromHistory",
                new MySqlParameter("p_history_id", historyId),
                new MySqlParameter("p_queued_by", "integration.user"),
                new MySqlParameter("p_employee_number", 1012)
            );

            var queuedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_label_data WHERE load_id = @loadGuid AND is_reprint = 1;",
                    new MySqlParameter("@loadGuid", loadGuid)
                )
            );
            queuedRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_label_data WHERE load_id = @loadGuid;",
                new MySqlParameter("@loadGuid", loadGuid)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_history WHERE load_guid = @loadGuid;",
                new MySqlParameter("@loadGuid", loadGuid)
            );
        }
    }

    [Fact]
    public async Task sp_Receiving_LabelData_ClearToHistory_ShouldMoveSeededQueueRowToHistory_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var loadId = Guid.NewGuid().ToString();
        var partId = $"RLCTH-{CreateUniqueSuffix()[..8]}";
        var rowsMovedParameter = new MySqlParameter("p_rows_moved", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var archiveBatchIdParameter = new MySqlParameter(
            "p_archive_batch_id",
            MySqlDbType.VarChar,
            36
        )
        {
            Direction = ParameterDirection.Output,
        };
        var statusParameter = new MySqlParameter("p_status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorParameter = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 1000)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                @"INSERT INTO receiving_label_data (load_id, load_number, quantity, weight_quantity, part_id, part_description, po_number, po_line_number, employee_number, received_date, transaction_date, initial_location, label_number, vendor_name, user_id)
VALUES (@loadId, 1, 1, 1, @partId, 'Clear row', 'PO-CLR', '1', 1013, NOW(), CURDATE(), 'RCV-CLEAR-LOC', 1, 'Clear Vendor', 'integration.user');",
                new MySqlParameter("@loadId", loadId),
                new MySqlParameter("@partId", partId)
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Receiving_LabelData_ClearToHistory",
                new MySqlParameter("p_archived_by", "integration.user"),
                new MySqlParameter("p_employee_number", 1013),
                new MySqlParameter("p_clear_all", 0),
                rowsMovedParameter,
                archiveBatchIdParameter,
                statusParameter,
                errorParameter
            );

            ConvertToInt(result.OutputValues["p_rows_moved"]).Should().Be(1);
            ConvertToInt(result.OutputValues["p_status"]).Should().Be(0);

            var queueRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_label_data WHERE load_id = @loadId;",
                    new MySqlParameter("@loadId", loadId)
                )
            );
            var historyRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM receiving_history WHERE load_guid = @loadId AND part_id = @partId;",
                    new MySqlParameter("@loadId", loadId),
                    new MySqlParameter("@partId", partId)
                )
            );
            queueRows.Should().Be(0);
            historyRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_label_data WHERE load_id = @loadId;",
                new MySqlParameter("@loadId", loadId)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM receiving_history WHERE load_guid = @loadId;",
                new MySqlParameter("@loadId", loadId)
            );
        }
    }

    [Fact]
    public async Task sp_SettingsCore_ShouldRoundTripSystemAndUserSettings_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var category = $"Integration.SettingsCore.{CreateUniqueSuffix()[..8]}";
        var systemKey = "SystemKey";
        var userKey = "UserKey";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_SettingsCore_System_Upsert",
                new MySqlParameter("p_category", category),
                new MySqlParameter("p_key", systemKey),
                new MySqlParameter("p_value", "system-value"),
                new MySqlParameter("p_data_type", "String"),
                new MySqlParameter("p_is_sensitive", 0),
                new MySqlParameter("p_updated_by", "integration.user")
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_SettingsCore_User_Upsert",
                new MySqlParameter("p_user_id", 1),
                new MySqlParameter("p_category", category),
                new MySqlParameter("p_key", userKey),
                new MySqlParameter("p_value", "user-value"),
                new MySqlParameter("p_data_type", "String"),
                new MySqlParameter("p_updated_by", "integration.user")
            );

            var systemByKey = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_SettingsCore_System_GetByKey",
                new MySqlParameter("p_category", category),
                new MySqlParameter("p_key", systemKey)
            );
            systemByKey.Rows.Count.Should().Be(1);
            systemByKey.Rows[0]["setting_value"].Should().Be("system-value");

            var systemByCategory = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_SettingsCore_System_GetByCategory",
                new MySqlParameter("p_category", category)
            );
            systemByCategory
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["setting_key"]?.ToString(),
                        systemKey,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();

            var userByKey = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_SettingsCore_User_GetByKey",
                new MySqlParameter("p_user_id", 1),
                new MySqlParameter("p_category", category),
                new MySqlParameter("p_key", userKey)
            );
            userByKey.Rows.Count.Should().Be(1);
            userByKey.Rows[0]["setting_value"].Should().Be("user-value");

            var userByCategory = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_SettingsCore_User_GetByCategory",
                new MySqlParameter("p_user_id", 1),
                new MySqlParameter("p_category", category)
            );
            userByCategory
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["setting_key"]?.ToString(),
                        userKey,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM settings_personal WHERE user_id = 1 AND category = @category;",
                new MySqlParameter("@category", category)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM settings_universal WHERE category = @category;",
                new MySqlParameter("@category", category)
            );
        }
    }

    [Fact]
    public async Task sp_Volvo_GeneratedLabelData_GetAll_ShouldReturnSeededRows_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var shipmentId = Random.Shared.Next(500000, 599999);
        var partNumber = $"VGLGA{CreateUniqueSuffix()[..6]}";

        try
        {
            await InsertVolvoGeneratedLabelAsync(
                connectionString,
                shipmentId,
                11,
                DateTime.UtcNow.Date,
                partNumber,
                10,
                1,
                1,
                "Generated label row",
                7701
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_GeneratedLabelData_GetAll"
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["part_number"]?.ToString(),
                        partNumber,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_generated_label_data WHERE shipment_id = @shipmentId;",
                new MySqlParameter("@shipmentId", shipmentId)
            );
        }
    }

    [Fact]
    public async Task sp_Volvo_GeneratedLabelData_Insert_ShouldInsertRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var shipmentId = Random.Shared.Next(600000, 699999);
        var partNumber = $"VGLIN{CreateUniqueSuffix()[..6]}";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_GeneratedLabelData_Insert",
                new MySqlParameter("p_shipment_id", shipmentId),
                new MySqlParameter("p_shipment_number", 22),
                new MySqlParameter("p_shipment_date", DateTime.UtcNow.Date),
                new MySqlParameter("p_part_number", partNumber),
                new MySqlParameter("p_quantity", 12),
                new MySqlParameter("p_skid_number", 1),
                new MySqlParameter("p_total_skids", 1),
                new MySqlParameter("p_part_description", "Inserted generated label"),
                new MySqlParameter("p_employee_number", 7702)
            );

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_generated_label_data WHERE shipment_id = @shipmentId AND part_number = @partNumber;",
                    new MySqlParameter("@shipmentId", shipmentId),
                    new MySqlParameter("@partNumber", partNumber)
                )
            );
            savedRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_generated_label_data WHERE shipment_id = @shipmentId;",
                new MySqlParameter("@shipmentId", shipmentId)
            );
        }
    }

    [Fact]
    public async Task sp_Volvo_GeneratedLabelData_DeleteByShipment_ShouldDeleteMatchingRows_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var shipmentId = Random.Shared.Next(700000, 799999);
        var otherShipmentId = shipmentId + 1;

        try
        {
            await InsertVolvoGeneratedLabelAsync(
                connectionString,
                shipmentId,
                31,
                DateTime.UtcNow.Date,
                $"VGLD1{CreateUniqueSuffix()[..4]}",
                10,
                1,
                2,
                "Delete shipment row 1",
                7703
            );
            await InsertVolvoGeneratedLabelAsync(
                connectionString,
                shipmentId,
                31,
                DateTime.UtcNow.Date,
                $"VGLD2{CreateUniqueSuffix()[..4]}",
                10,
                2,
                2,
                "Delete shipment row 2",
                7703
            );
            await InsertVolvoGeneratedLabelAsync(
                connectionString,
                otherShipmentId,
                32,
                DateTime.UtcNow.Date,
                $"VGLD3{CreateUniqueSuffix()[..4]}",
                10,
                1,
                1,
                "Other shipment row",
                7703
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_GeneratedLabelData_DeleteByShipment",
                new MySqlParameter("p_shipment_id", shipmentId)
            );

            var deletedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_generated_label_data WHERE shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_generated_label_data WHERE shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", otherShipmentId)
                )
            );
            deletedRows.Should().Be(0);
            remainingRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_generated_label_data WHERE shipment_id IN (@shipmentId, @otherShipmentId);",
                new MySqlParameter("@shipmentId", shipmentId),
                new MySqlParameter("@otherShipmentId", otherShipmentId)
            );
        }
    }

    [Fact]
    public async Task sp_Volvo_GeneratedLabelData_ClearToHistory_ShouldMoveEmployeeRowsToHistory_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var shipmentId = Random.Shared.Next(800000, 899999);
        var employeeNumber = 7704;
        var rowsMovedParameter = new MySqlParameter("p_rows_moved", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var archiveBatchIdParameter = new MySqlParameter(
            "p_archive_batch_id",
            MySqlDbType.VarChar,
            36
        )
        {
            Direction = ParameterDirection.Output,
        };
        var statusParameter = new MySqlParameter("p_status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorParameter = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 1000)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            await InsertVolvoGeneratedLabelAsync(
                connectionString,
                shipmentId,
                41,
                DateTime.UtcNow.Date,
                $"VGLC{CreateUniqueSuffix()[..5]}",
                8,
                1,
                1,
                "Clear generated label",
                employeeNumber
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_GeneratedLabelData_ClearToHistory",
                new MySqlParameter("p_archived_by", "integration.user"),
                new MySqlParameter("p_employee_number", employeeNumber),
                new MySqlParameter("p_clear_all", 0),
                rowsMovedParameter,
                archiveBatchIdParameter,
                statusParameter,
                errorParameter
            );

            ConvertToInt(result.OutputValues["p_rows_moved"]).Should().Be(1);
            ConvertToInt(result.OutputValues["p_status"]).Should().Be(0);

            var queueRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_generated_label_data WHERE shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            var historyRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_generated_label_history WHERE shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            queueRows.Should().Be(0);
            historyRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_generated_label_data WHERE shipment_id = @shipmentId;",
                new MySqlParameter("@shipmentId", shipmentId)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_generated_label_history WHERE shipment_id = @shipmentId;",
                new MySqlParameter("@shipmentId", shipmentId)
            );
        }
    }

    [Fact]
    public async Task sp_Volvo_PartComponent_Insert_ShouldInsertComponentRow_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var parentPart = $"VPAR{CreateUniqueSuffix()[..6]}";
        var componentPart = $"VCOM{CreateUniqueSuffix()[..6]}";

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, parentPart, 10, 1);
            await InsertVolvoMasterPartAsync(connectionString, componentPart, 4, 1);

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_PartComponent_Insert",
                new MySqlParameter("p_parent_part_number", parentPart),
                new MySqlParameter("p_component_part_number", componentPart),
                new MySqlParameter("p_quantity", 3)
            );

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_part_components WHERE parent_part_number = @parentPart AND component_part_number = @componentPart;",
                    new MySqlParameter("@parentPart", parentPart),
                    new MySqlParameter("@componentPart", componentPart)
                )
            );
            savedRows.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_part_components WHERE parent_part_number = @parentPart;",
                new MySqlParameter("@parentPart", parentPart)
            );
            await DeleteVolvoMasterPartAsync(connectionString, componentPart);
            await DeleteVolvoMasterPartAsync(connectionString, parentPart);
        }
    }

    [Fact]
    public async Task sp_Volvo_PartComponent_Get_ShouldReturnSeededComponents_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var parentPart = $"VPGT{CreateUniqueSuffix()[..6]}";
        var componentPart = $"VCGT{CreateUniqueSuffix()[..6]}";

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, parentPart, 10, 1);
            await InsertVolvoMasterPartAsync(connectionString, componentPart, 4, 1);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO volvo_part_components (parent_part_number, component_part_number, quantity) VALUES (@parentPart, @componentPart, 2);",
                new MySqlParameter("@parentPart", parentPart),
                new MySqlParameter("@componentPart", componentPart)
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_PartComponent_Get",
                new MySqlParameter("p_parent_part_number", parentPart)
            );
            result.Rows.Count.Should().Be(1);
            result.Rows[0]["component_part_number"].Should().Be(componentPart);
            ConvertToInt(result.Rows[0]["component_quantity_per_skid"]).Should().Be(4);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_part_components WHERE parent_part_number = @parentPart;",
                new MySqlParameter("@parentPart", parentPart)
            );
            await DeleteVolvoMasterPartAsync(connectionString, componentPart);
            await DeleteVolvoMasterPartAsync(connectionString, parentPart);
        }
    }

    [Fact]
    public async Task sp_Volvo_PartComponent_DeleteByParent_ShouldDeleteSeededComponents_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var parentPart = $"VPDP{CreateUniqueSuffix()[..6]}";
        var componentPart = $"VCDP{CreateUniqueSuffix()[..6]}";

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, parentPart, 10, 1);
            await InsertVolvoMasterPartAsync(connectionString, componentPart, 4, 1);
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "INSERT INTO volvo_part_components (parent_part_number, component_part_number, quantity) VALUES (@parentPart, @componentPart, 2);",
                new MySqlParameter("@parentPart", parentPart),
                new MySqlParameter("@componentPart", componentPart)
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_PartComponent_DeleteByParent",
                new MySqlParameter("p_parent_part_number", parentPart)
            );

            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_part_components WHERE parent_part_number = @parentPart;",
                    new MySqlParameter("@parentPart", parentPart)
                )
            );
            remainingRows.Should().Be(0);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_part_components WHERE parent_part_number = @parentPart;",
                new MySqlParameter("@parentPart", parentPart)
            );
            await DeleteVolvoMasterPartAsync(connectionString, componentPart);
            await DeleteVolvoMasterPartAsync(connectionString, parentPart);
        }
    }

    [Fact]
    public async Task sp_Volvo_PartMaster_Insert_ShouldInsertPart_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VPMI{CreateUniqueSuffix()[..6]}";

        try
        {
            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_PartMaster_Insert",
                new MySqlParameter("p_part_number", partNumber),
                new MySqlParameter("p_quantity_per_skid", 15),
                new MySqlParameter("p_is_active", 1)
            );

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_masterdata WHERE part_number = @partNumber AND quantity_per_skid = 15;",
                    new MySqlParameter("@partNumber", partNumber)
                )
            );
            savedRows.Should().Be(1);
        }
        finally
        {
            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_Volvo_PartMaster_GetById_ShouldReturnSeededPart_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VPMG{CreateUniqueSuffix()[..6]}";

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 21, 1);

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_PartMaster_GetById",
                new MySqlParameter("p_part_number", partNumber)
            );
            result.Rows.Count.Should().Be(1);
            result.Rows[0]["part_number"].Should().Be(partNumber);
            ConvertToInt(result.Rows[0]["quantity_per_skid"]).Should().Be(21);
        }
        finally
        {
            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_Volvo_PartMaster_GetAll_ShouldRespectIncludeInactiveFlag_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var activePart = $"VPGA{CreateUniqueSuffix()[..6]}";
        var inactivePart = $"VPGI{CreateUniqueSuffix()[..6]}";

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, activePart, 5, 1);
            await InsertVolvoMasterPartAsync(connectionString, inactivePart, 6, 0);

            var activeOnly = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_PartMaster_GetAll",
                new MySqlParameter("p_include_inactive", 0)
            );
            activeOnly
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["part_number"]?.ToString(),
                        activePart,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
            activeOnly
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["part_number"]?.ToString(),
                        inactivePart,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeFalse();

            var includingInactive = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_PartMaster_GetAll",
                new MySqlParameter("p_include_inactive", 1)
            );
            includingInactive
                .Rows.Cast<DataRow>()
                .Any(row =>
                    string.Equals(
                        row["part_number"]?.ToString(),
                        inactivePart,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            await DeleteVolvoMasterPartAsync(connectionString, inactivePart);
            await DeleteVolvoMasterPartAsync(connectionString, activePart);
        }
    }

    [Fact]
    public async Task sp_Volvo_PartMaster_SetActive_ShouldUpdateActiveFlag_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VPSA{CreateUniqueSuffix()[..6]}";

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 7, 1);

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_PartMaster_SetActive",
                new MySqlParameter("p_part_number", partNumber),
                new MySqlParameter("p_is_active", 0)
            );

            var updatedFlag = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT is_active FROM volvo_masterdata WHERE part_number = @partNumber;",
                    new MySqlParameter("@partNumber", partNumber)
                )
            );
            updatedFlag.Should().Be(0);
        }
        finally
        {
            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_Volvo_PartMaster_Update_ShouldUpdateQuantityPerSkid_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VPMU{CreateUniqueSuffix()[..6]}";

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 8, 1);

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_PartMaster_Update",
                new MySqlParameter("p_part_number", partNumber),
                new MySqlParameter("p_quantity_per_skid", 18)
            );

            var updatedQuantity = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT quantity_per_skid FROM volvo_masterdata WHERE part_number = @partNumber;",
                    new MySqlParameter("@partNumber", partNumber)
                )
            );
            updatedQuantity.Should().Be(18);
        }
        finally
        {
            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_volvo_part_check_references_ShouldReturnActiveReferenceCount_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VPCR{CreateUniqueSuffix()[..6]}";
        var referenceCountParameter = new MySqlParameter(
            "p_active_reference_count",
            MySqlDbType.Int32
        )
        {
            Direction = ParameterDirection.Output,
        };
        var shipmentId = 0;

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 9, 1);
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(2000, 2999),
                null,
                null,
                "7705",
                "reference shipment",
                "pending_po",
                0
            );
            await InsertVolvoShipmentLineAsync(
                connectionString,
                shipmentId,
                partNumber,
                "Pending",
                "REF-LOC",
                9,
                1,
                9,
                0,
                null,
                null
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_volvo_part_check_references",
                new MySqlParameter("p_part_number", partNumber),
                referenceCountParameter
            );
            ConvertToInt(result.OutputValues["p_active_reference_count"]).Should().Be(1);
        }
        finally
        {
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }

            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_volvo_shipment_insert_ShouldInsertPendingShipmentAndReturnOutputs_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var newIdParameter = new MySqlParameter("p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var shipmentNumberParameter = new MySqlParameter("p_shipment_number", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var insertedId = 0;

        try
        {
            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_Shipment_Insert",
                new MySqlParameter("p_shipment_date", DateTime.UtcNow.Date),
                new MySqlParameter("p_employee_number", "7706"),
                new MySqlParameter("p_notes", "inserted shipment"),
                newIdParameter,
                shipmentNumberParameter
            );

            insertedId = ConvertToInt(result.OutputValues["p_new_id"]);
            insertedId.Should().BeGreaterThan(0);
            ConvertToInt(result.OutputValues["p_shipment_number"]).Should().BeGreaterThan(0);

            var savedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_label_data WHERE id = @shipmentId AND status = 'pending_po';",
                    new MySqlParameter("@shipmentId", insertedId)
                )
            );
            savedRows.Should().Be(1);
        }
        finally
        {
            if (insertedId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", insertedId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_volvo_shipment_update_ShouldUpdateNotesAndReturnAffectedRows_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var shipmentId = 0;

        try
        {
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(3000, 3999),
                null,
                null,
                "7707",
                "old notes",
                "pending_po",
                0
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_Shipment_Update",
                new MySqlParameter("p_id", shipmentId),
                new MySqlParameter("p_notes", "new notes")
            );
            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["affected_rows"]).Should().Be(1);

            var savedNotes = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT notes FROM volvo_label_data WHERE id = @shipmentId;",
                new MySqlParameter("@shipmentId", shipmentId)
            );
            savedNotes?.ToString().Should().Be("new notes");
        }
        finally
        {
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Volvo_Shipment_GetNextShipmentNumber_ShouldReturnMaxPlusOne_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var shipmentId = 0;
        var shipmentNumber = Random.Shared.Next(4000, 4999);

        try
        {
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                shipmentNumber,
                null,
                null,
                "7708",
                "next number",
                "pending_po",
                0
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_Shipment_GetNextShipmentNumber"
            );
            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["next_shipment_number"])
                .Should()
                .BeGreaterThanOrEqualTo(shipmentNumber + 1);
        }
        finally
        {
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Volvo_Shipment_GetPending_ShouldReturnPendingShipment_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var pendingId = 0;
        var completedId = 0;

        try
        {
            completedId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(5000, 5499),
                null,
                null,
                "7709",
                "completed shipment",
                "completed",
                0
            );
            pendingId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(5500, 5999),
                null,
                null,
                "7709",
                "pending shipment",
                "pending_po",
                0
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_Shipment_GetPending"
            );
            result.Rows.Count.Should().BeGreaterThan(0);
            result
                .Rows.Cast<DataRow>()
                .Any(row => ConvertToInt(row["id"]) == pendingId)
                .Should()
                .BeTrue();
        }
        finally
        {
            if (pendingId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", pendingId)
                );
            }

            if (completedId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", completedId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Volvo_Shipment_GetById_ShouldReturnSeededShipment_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var shipmentId = 0;

        try
        {
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(6000, 6999),
                "PO-GETID",
                "RCV-GETID",
                "7710",
                "shipment get by id",
                "pending_po",
                0
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_Shipment_GetById",
                new MySqlParameter("p_id", shipmentId)
            );
            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["id"]).Should().Be(shipmentId);
            result.Rows[0]["po_number"].Should().Be("PO-GETID");
        }
        finally
        {
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Volvo_ShipmentLine_Insert_ShouldInsertLineAndNormalizeValues_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VSLI{CreateUniqueSuffix()[..6]}";
        var shipmentId = 0;

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 12, 1);
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(7000, 7999),
                null,
                null,
                "7711",
                "line insert",
                "pending_po",
                0
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_ShipmentLine_Insert",
                new MySqlParameter("p_shipment_id", shipmentId),
                new MySqlParameter("p_part_number", partNumber),
                new MySqlParameter("p_po_status", string.Empty),
                new MySqlParameter("p_location", string.Empty),
                new MySqlParameter("p_quantity_per_skid", 12),
                new MySqlParameter("p_received_skid_count", 2),
                new MySqlParameter("p_calculated_piece_count", 24),
                new MySqlParameter("p_has_discrepancy", 0),
                new MySqlParameter("p_expected_skid_count", DBNull.Value),
                new MySqlParameter("p_discrepancy_note", DBNull.Value)
            );

            var result = await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT COUNT(*) FROM volvo_line_data WHERE shipment_id = @shipmentId AND part_number = @partNumber AND po_status = 'Pending' AND location IS NULL;",
                new MySqlParameter("@shipmentId", shipmentId),
                new MySqlParameter("@partNumber", partNumber)
            );
            ConvertToInt(result).Should().Be(1);
        }
        finally
        {
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }

            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_Volvo_ShipmentLine_Update_ShouldUpdateLineValues_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var originalPart = $"VSLU{CreateUniqueSuffix()[..6]}";
        var updatedPart = $"VSLX{CreateUniqueSuffix()[..6]}";
        var shipmentId = 0;
        var lineId = 0;

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, originalPart, 10, 1);
            await InsertVolvoMasterPartAsync(connectionString, updatedPart, 14, 1);
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(8000, 8499),
                null,
                null,
                "7712",
                "line update",
                "pending_po",
                0
            );
            lineId = await InsertVolvoShipmentLineAsync(
                connectionString,
                shipmentId,
                originalPart,
                "Pending",
                "LINE-OLD",
                10,
                1,
                10,
                0,
                null,
                null
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_ShipmentLine_Update",
                new MySqlParameter("p_id", lineId),
                new MySqlParameter("p_part_number", updatedPart),
                new MySqlParameter("p_po_status", string.Empty),
                new MySqlParameter("p_location", string.Empty),
                new MySqlParameter("p_quantity_per_skid", 14),
                new MySqlParameter("p_received_skid_count", 2),
                new MySqlParameter("p_calculated_piece_count", 28),
                new MySqlParameter("p_has_discrepancy", 1),
                new MySqlParameter("p_expected_skid_count", 3),
                new MySqlParameter("p_discrepancy_note", "Updated discrepancy")
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_ShipmentLine_GetByShipment",
                new MySqlParameter("p_shipment_id", shipmentId)
            );
            var updatedRow = result
                .Rows.Cast<DataRow>()
                .Single(row => ConvertToInt(row["id"]) == lineId);
            updatedRow["part_number"].Should().Be(updatedPart);
            updatedRow["po_status"].Should().Be("Pending");
            updatedRow["location"].Should().Be(DBNull.Value);
            ConvertToInt(updatedRow["received_skid_count"]).Should().Be(2);
            ConvertToInt(updatedRow["has_discrepancy"]).Should().Be(1);
        }
        finally
        {
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }

            await DeleteVolvoMasterPartAsync(connectionString, updatedPart);
            await DeleteVolvoMasterPartAsync(connectionString, originalPart);
        }
    }

    [Fact]
    public async Task sp_Volvo_ShipmentLine_GetByShipment_ShouldReturnShipmentLines_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VSLG{CreateUniqueSuffix()[..6]}";
        var shipmentId = 0;

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 11, 1);
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(8500, 8999),
                null,
                null,
                "7713",
                "line get",
                "pending_po",
                0
            );
            await InsertVolvoShipmentLineAsync(
                connectionString,
                shipmentId,
                partNumber,
                "Pending",
                "LOC-A",
                11,
                1,
                11,
                0,
                null,
                null
            );
            await InsertVolvoShipmentLineAsync(
                connectionString,
                shipmentId,
                partNumber,
                "Received",
                "LOC-B",
                11,
                2,
                22,
                0,
                null,
                null
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_ShipmentLine_GetByShipment",
                new MySqlParameter("p_shipment_id", shipmentId)
            );
            result.Rows.Count.Should().Be(2);
            result
                .Rows.Cast<DataRow>()
                .Select(row => row["location"]?.ToString())
                .Should()
                .Contain(new[] { "LOC-A", "LOC-B" });
        }
        finally
        {
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }

            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_Volvo_ShipmentLine_Delete_ShouldDeleteSpecificLine_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VSLD{CreateUniqueSuffix()[..6]}";
        var shipmentId = 0;
        var lineId = 0;

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 13, 1);
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(9000, 9499),
                null,
                null,
                "7714",
                "line delete",
                "pending_po",
                0
            );
            lineId = await InsertVolvoShipmentLineAsync(
                connectionString,
                shipmentId,
                partNumber,
                "Pending",
                "DELETE-LOC",
                13,
                1,
                13,
                0,
                null,
                null
            );
            await InsertVolvoShipmentLineAsync(
                connectionString,
                shipmentId,
                partNumber,
                "Pending",
                "KEEP-LOC",
                13,
                1,
                13,
                0,
                null,
                null
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_ShipmentLine_Delete",
                new MySqlParameter("p_id", lineId)
            );

            var deletedRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_line_data WHERE id = @lineId;",
                    new MySqlParameter("@lineId", lineId)
                )
            );
            var remainingRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_line_data WHERE shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            deletedRows.Should().Be(0);
            remainingRows.Should().Be(1);
        }
        finally
        {
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }

            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_Volvo_LabelData_ClearToHistory_ShouldMoveActiveHeadersAndLinesToHistory_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VCLH{CreateUniqueSuffix()[..6]}";
        var shipmentId = 0;
        var headersMovedParameter = new MySqlParameter("p_headers_moved", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var linesMovedParameter = new MySqlParameter("p_lines_moved", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var archiveBatchIdParameter = new MySqlParameter(
            "p_archive_batch_id",
            MySqlDbType.VarChar,
            36
        )
        {
            Direction = ParameterDirection.Output,
        };
        var statusParameter = new MySqlParameter("p_status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var errorParameter = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 1000)
        {
            Direction = ParameterDirection.Output,
        };

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 15, 1);
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(9500, 9999),
                "PO-CLR",
                "RCV-CLR",
                "7715",
                "clear history",
                "pending_po",
                0
            );
            await InsertVolvoShipmentLineAsync(
                connectionString,
                shipmentId,
                partNumber,
                "Pending",
                "CLR-1",
                15,
                1,
                15,
                0,
                null,
                null
            );
            await InsertVolvoShipmentLineAsync(
                connectionString,
                shipmentId,
                partNumber,
                "Pending",
                "CLR-2",
                15,
                2,
                30,
                0,
                null,
                null
            );

            var result = await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_LabelData_ClearToHistory",
                new MySqlParameter("p_archived_by", "integration.user"),
                headersMovedParameter,
                linesMovedParameter,
                archiveBatchIdParameter,
                statusParameter,
                errorParameter
            );

            ConvertToInt(result.OutputValues["p_headers_moved"]).Should().BeGreaterThanOrEqualTo(1);
            ConvertToInt(result.OutputValues["p_lines_moved"]).Should().BeGreaterThanOrEqualTo(2);
            ConvertToInt(result.OutputValues["p_status"]).Should().Be(0);

            var activeHeaders = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            var historyHeaders = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_label_history WHERE original_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            var historyLines = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_line_history WHERE original_shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            activeHeaders.Should().Be(0);
            historyHeaders.Should().Be(1);
            historyLines.Should().Be(2);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_line_history WHERE original_shipment_id = @shipmentId;",
                new MySqlParameter("@shipmentId", shipmentId)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_label_history WHERE original_id = @shipmentId;",
                new MySqlParameter("@shipmentId", shipmentId)
            );
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }

            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_volvo_shipment_complete_ShouldMoveShipmentToHistory_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VSCP{CreateUniqueSuffix()[..6]}";
        var shipmentId = 0;

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 16, 1);
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(10000, 10499),
                null,
                null,
                "7716",
                "complete shipment",
                "pending_po",
                0
            );
            await InsertVolvoShipmentLineAsync(
                connectionString,
                shipmentId,
                partNumber,
                "Pending",
                "COMP-LOC",
                16,
                2,
                32,
                0,
                null,
                null
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_Shipment_Complete",
                new MySqlParameter("p_shipment_id", shipmentId),
                new MySqlParameter("p_po_number", "PO-COMPLETE"),
                new MySqlParameter("p_receiver_number", "RCV-COMPLETE"),
                new MySqlParameter("p_archived_by", "integration.user")
            );

            var activeHeaders = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            var historyHeaders = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_label_history WHERE original_id = @shipmentId AND status = 'completed';",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            var historyLines = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_line_history WHERE original_shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            activeHeaders.Should().Be(0);
            historyHeaders.Should().Be(1);
            historyLines.Should().Be(1);
        }
        finally
        {
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_line_history WHERE original_shipment_id = @shipmentId;",
                new MySqlParameter("@shipmentId", shipmentId)
            );
            await ExecuteSqlNonQueryAsync(
                connectionString,
                "DELETE FROM volvo_label_history WHERE original_id = @shipmentId;",
                new MySqlParameter("@shipmentId", shipmentId)
            );
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }

            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_volvo_shipment_delete_ShouldDeleteShipmentAndCascadeLines_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VSDL{CreateUniqueSuffix()[..6]}";
        var shipmentId = 0;

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 17, 1);
            shipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                DateTime.UtcNow.Date,
                Random.Shared.Next(10500, 10999),
                null,
                null,
                "7717",
                "delete shipment",
                "pending_po",
                0
            );
            await InsertVolvoShipmentLineAsync(
                connectionString,
                shipmentId,
                partNumber,
                "Pending",
                "DEL-LOC",
                17,
                1,
                17,
                0,
                null,
                null
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_Shipment_Delete",
                new MySqlParameter("p_shipment_id", shipmentId)
            );

            var headerRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            var lineRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_line_data WHERE shipment_id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                )
            );
            headerRows.Should().Be(0);
            lineRows.Should().Be(0);
        }
        finally
        {
            if (shipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", shipmentId)
                );
            }

            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_Volvo_Shipment_GetHistory_ShouldReturnActiveAndArchivedShipments_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var partNumber = $"VSGH{CreateUniqueSuffix()[..6]}";
        var activeShipmentId = 0;
        var historyShipmentId = 0;
        var archiveBatch = Guid.NewGuid().ToString();
        var today = DateTime.UtcNow.Date;

        try
        {
            await InsertVolvoMasterPartAsync(connectionString, partNumber, 18, 1);
            activeShipmentId = await InsertVolvoShipmentHeaderAsync(
                connectionString,
                today,
                Random.Shared.Next(11000, 11499),
                null,
                null,
                "7718",
                "active shipment",
                "pending_po",
                0
            );
            await InsertVolvoShipmentLineAsync(
                connectionString,
                activeShipmentId,
                partNumber,
                "Pending",
                "HIST-ACT",
                18,
                1,
                18,
                0,
                null,
                null
            );

            historyShipmentId = await InsertVolvoShipmentHistoryHeaderAsync(
                connectionString,
                activeShipmentId + 50000,
                today.AddDays(-1),
                Random.Shared.Next(11500, 11999),
                "PO-HIST",
                "RCV-HIST",
                "7718",
                "archived shipment",
                "completed",
                archiveBatch
            );
            await InsertVolvoShipmentHistoryLineAsync(
                connectionString,
                historyShipmentId,
                activeShipmentId + 50000,
                partNumber,
                "Received",
                "HIST-ARC",
                18,
                1,
                18,
                0,
                null,
                null,
                archiveBatch
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_Shipment_GetHistory",
                new MySqlParameter("p_start_date", today.AddDays(-2)),
                new MySqlParameter("p_end_date", today.AddDays(1)),
                new MySqlParameter("p_status", "all")
            );
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    ConvertToInt(row["id"]) == activeShipmentId
                    && ConvertToInt(row["is_archived"]) == 0
                )
                .Should()
                .BeTrue();
            result
                .Rows.Cast<DataRow>()
                .Any(row =>
                    ConvertToInt(row["id"]) == historyShipmentId
                    && ConvertToInt(row["is_archived"]) == 1
                )
                .Should()
                .BeTrue();
        }
        finally
        {
            if (activeShipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_data WHERE id = @shipmentId;",
                    new MySqlParameter("@shipmentId", activeShipmentId)
                );
            }

            if (historyShipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_history WHERE id = @historyId;",
                    new MySqlParameter("@historyId", historyShipmentId)
                );
            }

            await DeleteVolvoMasterPartAsync(connectionString, partNumber);
        }
    }

    [Fact]
    public async Task sp_Volvo_ShipmentHistory_GetById_ShouldReturnArchivedShipment_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var historyShipmentId = 0;
        var archiveBatch = Guid.NewGuid().ToString();

        try
        {
            historyShipmentId = await InsertVolvoShipmentHistoryHeaderAsync(
                connectionString,
                Random.Shared.Next(12000, 12999),
                DateTime.UtcNow.Date,
                120,
                "PO-HGET",
                "RCV-HGET",
                "7719",
                "history get",
                "completed",
                archiveBatch
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_ShipmentHistory_GetById",
                new MySqlParameter("p_id", historyShipmentId)
            );
            result.Rows.Count.Should().Be(1);
            ConvertToInt(result.Rows[0]["id"]).Should().Be(historyShipmentId);
            ConvertToInt(result.Rows[0]["is_archived"]).Should().Be(1);
        }
        finally
        {
            if (historyShipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_history WHERE id = @historyId;",
                    new MySqlParameter("@historyId", historyShipmentId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Volvo_ShipmentHistory_Delete_ShouldDeleteHistoryHeaderAndLines_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var historyShipmentId = 0;
        var archiveBatch = Guid.NewGuid().ToString();

        try
        {
            historyShipmentId = await InsertVolvoShipmentHistoryHeaderAsync(
                connectionString,
                Random.Shared.Next(13000, 13999),
                DateTime.UtcNow.Date,
                130,
                "PO-HDEL",
                "RCV-HDEL",
                "7720",
                "history delete",
                "completed",
                archiveBatch
            );
            await InsertVolvoShipmentHistoryLineAsync(
                connectionString,
                historyShipmentId,
                130,
                $"VHLD{CreateUniqueSuffix()[..5]}",
                "Received",
                "HDEL-LOC",
                10,
                1,
                10,
                0,
                null,
                null,
                archiveBatch
            );

            await ExecuteStoredProcedureNonQueryAsync(
                connectionString,
                "sp_Volvo_ShipmentHistory_Delete",
                new MySqlParameter("p_shipment_history_id", historyShipmentId)
            );

            var headerRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_label_history WHERE id = @historyId;",
                    new MySqlParameter("@historyId", historyShipmentId)
                )
            );
            var lineRows = ConvertToInt(
                await ExecuteSqlScalarAsync(
                    connectionString,
                    "SELECT COUNT(*) FROM volvo_line_history WHERE shipment_history_id = @historyId;",
                    new MySqlParameter("@historyId", historyShipmentId)
                )
            );
            headerRows.Should().Be(0);
            lineRows.Should().Be(0);
        }
        finally
        {
            if (historyShipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_history WHERE id = @historyId;",
                    new MySqlParameter("@historyId", historyShipmentId)
                );
            }
        }
    }

    [Fact]
    public async Task sp_Volvo_ShipmentLineHistory_GetByShipmentHistoryId_ShouldReturnArchivedLines_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var historyShipmentId = 0;
        var archiveBatch = Guid.NewGuid().ToString();
        var originalShipmentId = Random.Shared.Next(14000, 14999);
        var partNumber = $"VHLG{CreateUniqueSuffix()[..6]}";

        try
        {
            historyShipmentId = await InsertVolvoShipmentHistoryHeaderAsync(
                connectionString,
                originalShipmentId,
                DateTime.UtcNow.Date,
                140,
                "PO-HLIN",
                "RCV-HLIN",
                "7721",
                "history line get",
                "completed",
                archiveBatch
            );
            await InsertVolvoShipmentHistoryLineAsync(
                connectionString,
                historyShipmentId,
                originalShipmentId,
                partNumber,
                "Received",
                "HLIN-LOC",
                12,
                2,
                24,
                0,
                null,
                null,
                archiveBatch
            );
            await InsertVolvoShipmentHistoryLineAsync(
                connectionString,
                historyShipmentId,
                originalShipmentId,
                $"{partNumber}B",
                "Received",
                "HLIN-LOC-2",
                12,
                1,
                12,
                0,
                null,
                null,
                archiveBatch
            );

            var result = await ExecuteStoredProcedureQueryAsync(
                connectionString,
                "sp_Volvo_ShipmentLineHistory_GetByShipmentHistoryId",
                new MySqlParameter("p_shipment_history_id", historyShipmentId)
            );
            result.Rows.Count.Should().Be(2);
            result
                .Rows.Cast<DataRow>()
                .All(row => ConvertToInt(row["shipment_id"]) == originalShipmentId)
                .Should()
                .BeTrue();
        }
        finally
        {
            if (historyShipmentId > 0)
            {
                await ExecuteSqlNonQueryAsync(
                    connectionString,
                    "DELETE FROM volvo_label_history WHERE id = @historyId;",
                    new MySqlParameter("@historyId", historyShipmentId)
                );
            }
        }
    }

    private static async Task<int> InsertDunnageTypeAsync(string connectionString, string typeName)
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            "INSERT INTO dunnage_types (type_name, icon, created_by, created_date) VALUES (@typeName, 'PackageVariantClosed', 'integration.user', NOW());",
            new MySqlParameter("@typeName", typeName)
        );

        return ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT id FROM dunnage_types WHERE type_name = @typeName;",
                new MySqlParameter("@typeName", typeName)
            )
        );
    }

    private static async Task InsertDunnagePartAsync(
        string connectionString,
        string partId,
        int typeId,
        string homeLocation,
        string createdBy
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"INSERT INTO dunnage_parts (part_id, type_id, quantity_type, home_location, created_by, created_date)
VALUES (@partId, @typeId, 'Quantity', @homeLocation, @createdBy, NOW());",
            new MySqlParameter("@partId", partId),
            new MySqlParameter("@typeId", typeId),
            new MySqlParameter("@homeLocation", homeLocation),
            new MySqlParameter("@createdBy", createdBy)
        );
    }

    private static async Task DeleteDunnagePartAndTypeAsync(
        string connectionString,
        string partId,
        string typeName
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            "DELETE FROM dunnage_parts WHERE part_id = @partId;",
            new MySqlParameter("@partId", partId)
        );
        await ExecuteSqlNonQueryAsync(
            connectionString,
            "DELETE FROM dunnage_types WHERE type_name = @typeName;",
            new MySqlParameter("@typeName", typeName)
        );
    }

    private static async Task<int> InsertAuthUserAsync(
        string connectionString,
        string windowsUsername,
        string fullName,
        string pin,
        string defaultReceivingMode
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"
INSERT INTO auth_users
(
    windows_username,
    full_name,
    pin,
    department,
    shift,
    is_active,
    default_receiving_mode,
    default_dunnage_mode,
    created_by
)
VALUES
(
    @windowsUsername,
    @fullName,
    @pin,
    'Receiving',
    '1st Shift',
    1,
    @defaultReceivingMode,
    'guided',
    'integration.user'
);",
            new MySqlParameter("@windowsUsername", windowsUsername),
            new MySqlParameter("@fullName", fullName),
            new MySqlParameter("@pin", pin),
            new MySqlParameter("@defaultReceivingMode", defaultReceivingMode)
        );

        return ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT employee_number FROM auth_users WHERE windows_username = @windowsUsername;",
                new MySqlParameter("@windowsUsername", windowsUsername)
            )
        );
    }

    private static async Task DeleteAuthUserAsync(string connectionString, string windowsUsername)
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            "DELETE FROM auth_users WHERE windows_username = @windowsUsername;",
            new MySqlParameter("@windowsUsername", windowsUsername)
        );
    }

    private static async Task DeleteAuthUserByEmployeeNumberAsync(
        string connectionString,
        int employeeNumber
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            "DELETE FROM auth_users WHERE employee_number = @employeeNumber;",
            new MySqlParameter("@employeeNumber", employeeNumber)
        );
    }

    private static async Task InsertVolvoMasterPartAsync(
        string connectionString,
        string partNumber,
        int quantityPerSkid,
        int isActive
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            "INSERT INTO volvo_masterdata (part_number, quantity_per_skid, is_active) VALUES (@partNumber, @quantityPerSkid, @isActive);",
            new MySqlParameter("@partNumber", partNumber),
            new MySqlParameter("@quantityPerSkid", quantityPerSkid),
            new MySqlParameter("@isActive", isActive)
        );
    }

    private static async Task DeleteVolvoMasterPartAsync(string connectionString, string partNumber)
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            "DELETE FROM volvo_masterdata WHERE part_number = @partNumber;",
            new MySqlParameter("@partNumber", partNumber)
        );
    }

    private static async Task<int> InsertVolvoShipmentHeaderAsync(
        string connectionString,
        DateTime shipmentDate,
        int shipmentNumber,
        string? poNumber,
        string? receiverNumber,
        string employeeNumber,
        string? notes,
        string status,
        int isArchived
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"INSERT INTO volvo_label_data (shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date, is_archived)
VALUES (@shipmentDate, @shipmentNumber, @poNumber, @receiverNumber, @employeeNumber, @notes, @status, NOW(), @isArchived);",
            new MySqlParameter("@shipmentDate", shipmentDate.Date),
            new MySqlParameter("@shipmentNumber", shipmentNumber),
            new MySqlParameter("@poNumber", poNumber ?? (object)DBNull.Value),
            new MySqlParameter("@receiverNumber", receiverNumber ?? (object)DBNull.Value),
            new MySqlParameter("@employeeNumber", employeeNumber),
            new MySqlParameter("@notes", notes ?? (object)DBNull.Value),
            new MySqlParameter("@status", status),
            new MySqlParameter("@isArchived", isArchived)
        );

        return ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT MAX(id) FROM volvo_label_data WHERE shipment_date = @shipmentDate AND shipment_number = @shipmentNumber;",
                new MySqlParameter("@shipmentDate", shipmentDate.Date),
                new MySqlParameter("@shipmentNumber", shipmentNumber)
            )
        );
    }

    private static async Task<int> InsertVolvoShipmentLineAsync(
        string connectionString,
        int shipmentId,
        string partNumber,
        string poStatus,
        string? location,
        int quantityPerSkid,
        int receivedSkidCount,
        int calculatedPieceCount,
        int hasDiscrepancy,
        int? expectedSkidCount,
        string? discrepancyNote
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"INSERT INTO volvo_line_data (shipment_id, part_number, po_status, location, quantity_per_skid, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note)
VALUES (@shipmentId, @partNumber, @poStatus, @location, @quantityPerSkid, @receivedSkidCount, @calculatedPieceCount, @hasDiscrepancy, @expectedSkidCount, @discrepancyNote);",
            new MySqlParameter("@shipmentId", shipmentId),
            new MySqlParameter("@partNumber", partNumber),
            new MySqlParameter("@poStatus", poStatus),
            new MySqlParameter("@location", location ?? (object)DBNull.Value),
            new MySqlParameter("@quantityPerSkid", quantityPerSkid),
            new MySqlParameter("@receivedSkidCount", receivedSkidCount),
            new MySqlParameter("@calculatedPieceCount", calculatedPieceCount),
            new MySqlParameter("@hasDiscrepancy", hasDiscrepancy),
            new MySqlParameter("@expectedSkidCount", expectedSkidCount ?? (object)DBNull.Value),
            new MySqlParameter("@discrepancyNote", discrepancyNote ?? (object)DBNull.Value)
        );

        return ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT MAX(id) FROM volvo_line_data WHERE shipment_id = @shipmentId AND part_number = @partNumber;",
                new MySqlParameter("@shipmentId", shipmentId),
                new MySqlParameter("@partNumber", partNumber)
            )
        );
    }

    private static async Task InsertVolvoGeneratedLabelAsync(
        string connectionString,
        int shipmentId,
        int shipmentNumber,
        DateTime shipmentDate,
        string partNumber,
        int quantity,
        int skidNumber,
        int totalSkids,
        string partDescription,
        int employeeNumber
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"INSERT INTO volvo_generated_label_data (shipment_id, shipment_number, shipment_date, part_number, quantity, skid_number, total_skids, part_description, employee_number)
VALUES (@shipmentId, @shipmentNumber, @shipmentDate, @partNumber, @quantity, @skidNumber, @totalSkids, @partDescription, @employeeNumber);",
            new MySqlParameter("@shipmentId", shipmentId),
            new MySqlParameter("@shipmentNumber", shipmentNumber),
            new MySqlParameter("@shipmentDate", shipmentDate.Date),
            new MySqlParameter("@partNumber", partNumber),
            new MySqlParameter("@quantity", quantity),
            new MySqlParameter("@skidNumber", skidNumber),
            new MySqlParameter("@totalSkids", totalSkids),
            new MySqlParameter("@partDescription", partDescription),
            new MySqlParameter("@employeeNumber", employeeNumber)
        );
    }

    private static async Task<int> InsertVolvoShipmentHistoryHeaderAsync(
        string connectionString,
        int originalId,
        DateTime shipmentDate,
        int shipmentNumber,
        string? poNumber,
        string? receiverNumber,
        string employeeNumber,
        string? notes,
        string status,
        string archiveBatchId
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"INSERT INTO volvo_label_history (original_id, shipment_date, shipment_number, po_number, receiver_number, employee_number, notes, status, created_date, modified_date, archived_at, archived_by, archive_batch_id)
VALUES (@originalId, @shipmentDate, @shipmentNumber, @poNumber, @receiverNumber, @employeeNumber, @notes, @status, NOW(), NOW(), NOW(), 'integration.user', @archiveBatchId);",
            new MySqlParameter("@originalId", originalId),
            new MySqlParameter("@shipmentDate", shipmentDate.Date),
            new MySqlParameter("@shipmentNumber", shipmentNumber),
            new MySqlParameter("@poNumber", poNumber ?? (object)DBNull.Value),
            new MySqlParameter("@receiverNumber", receiverNumber ?? (object)DBNull.Value),
            new MySqlParameter("@employeeNumber", employeeNumber),
            new MySqlParameter("@notes", notes ?? (object)DBNull.Value),
            new MySqlParameter("@status", status),
            new MySqlParameter("@archiveBatchId", archiveBatchId)
        );

        return ConvertToInt(
            await ExecuteSqlScalarAsync(
                connectionString,
                "SELECT MAX(id) FROM volvo_label_history WHERE original_id = @originalId AND archive_batch_id = @archiveBatchId;",
                new MySqlParameter("@originalId", originalId),
                new MySqlParameter("@archiveBatchId", archiveBatchId)
            )
        );
    }

    private static async Task InsertVolvoShipmentHistoryLineAsync(
        string connectionString,
        int shipmentHistoryId,
        int originalShipmentId,
        string partNumber,
        string poStatus,
        string? location,
        int quantityPerSkid,
        int receivedSkidCount,
        int calculatedPieceCount,
        int hasDiscrepancy,
        int? expectedSkidCount,
        string? discrepancyNote,
        string archiveBatchId
    )
    {
        await ExecuteSqlNonQueryAsync(
            connectionString,
            @"INSERT INTO volvo_line_history (original_id, shipment_history_id, original_shipment_id, part_number, po_status, location, quantity_per_skid, received_skid_count, calculated_piece_count, has_discrepancy, expected_skid_count, discrepancy_note, archived_at, archived_by, archive_batch_id)
VALUES (@originalId, @shipmentHistoryId, @originalShipmentId, @partNumber, @poStatus, @location, @quantityPerSkid, @receivedSkidCount, @calculatedPieceCount, @hasDiscrepancy, @expectedSkidCount, @discrepancyNote, NOW(), 'integration.user', @archiveBatchId);",
            new MySqlParameter("@originalId", Random.Shared.Next(500000, 599999)),
            new MySqlParameter("@shipmentHistoryId", shipmentHistoryId),
            new MySqlParameter("@originalShipmentId", originalShipmentId),
            new MySqlParameter("@partNumber", partNumber),
            new MySqlParameter("@poStatus", poStatus),
            new MySqlParameter("@location", location ?? (object)DBNull.Value),
            new MySqlParameter("@quantityPerSkid", quantityPerSkid),
            new MySqlParameter("@receivedSkidCount", receivedSkidCount),
            new MySqlParameter("@calculatedPieceCount", calculatedPieceCount),
            new MySqlParameter("@hasDiscrepancy", hasDiscrepancy),
            new MySqlParameter("@expectedSkidCount", expectedSkidCount ?? (object)DBNull.Value),
            new MySqlParameter("@discrepancyNote", discrepancyNote ?? (object)DBNull.Value),
            new MySqlParameter("@archiveBatchId", archiveBatchId)
        );
    }

    private static async Task<int> DeleteWorkstationAsync(
        string connectionString,
        string workstationName
    )
    {
        return await ExecuteSqlNonQueryAsync(
            connectionString,
            "DELETE FROM auth_workstation_config WHERE workstation_name = @workstationName;",
            new MySqlParameter("@workstationName", workstationName)
        );
    }
}

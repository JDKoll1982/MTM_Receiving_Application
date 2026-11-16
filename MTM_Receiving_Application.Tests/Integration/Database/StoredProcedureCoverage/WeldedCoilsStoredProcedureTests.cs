using FluentAssertions;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Exercises the welded-coil settings procedures as one lifecycle: add, rename, deactivate, list,
/// remove.
/// </summary>
[Collection(StoredProcedureCoverageCollection.Name)]
public sealed class WeldedCoilsStoredProcedureTests : StoredProcedureTestBase, IAsyncLifetime
{
    private string _partId = string.Empty;

    public WeldedCoilsStoredProcedureTests(StoredProcedureTestDatabase database)
        : base(database)
    {
    }

    public Task InitializeAsync()
    {
        _partId = UniqueToken("TEST-WC-")[..11];
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Database.ConnectionString is null)
        {
            return;
        }

        await ExecuteSqlAsync(
            "DELETE FROM settings_weldedcoils WHERE partid LIKE 'TEST-WC-%';");
    }

    [SkippableFact]
    public async Task WeldedCoils_ShouldAddRenameDeactivateListAndDelete()
    {
        SkipWhenUnavailable();

        var outputs = await ExecuteProcedureWithOutputsAsync(
            "sp_Settings_WeldedCoils_Insert",
            Parameter("p_partid", _partId),
            new MySqlParameter("p_new_id", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output });

        var newId = ToInt(outputs["p_new_id"]);
        newId.Should().BeGreaterThan(0);

        var listed = await QueryAsync("sp_Settings_WeldedCoils_GetAll");
        var row = listed.Rows.Cast<System.Data.DataRow>()
            .Should().ContainSingle(candidate => (string)candidate["partid"] == _partId).Subject;
        ToInt(row["isActive"]).Should().Be(1, "a new welded-coil part starts active");

        var renamed = _partId[..8] + "REN";
        await ExecuteProcedureAsync(
            "sp_Settings_WeldedCoils_Update",
            Parameter("p_id", newId),
            Parameter("p_partid", renamed));

        var afterRename = await ExecuteScalarAsync(
            "SELECT partid FROM settings_weldedcoils WHERE id = @id;",
            new MySqlParameter("@id", newId));
        ToText(afterRename).Should().Be(renamed);

        await ExecuteProcedureAsync(
            "sp_Settings_WeldedCoils_SetActive",
            Parameter("p_id", newId),
            Parameter("p_is_active", 0));

        var afterDeactivate = await ExecuteScalarAsync(
            "SELECT isActive FROM settings_weldedcoils WHERE id = @id;",
            new MySqlParameter("@id", newId));
        ToInt(afterDeactivate).Should().Be(0);

        await ExecuteProcedureAsync(
            "sp_Settings_WeldedCoils_Delete",
            Parameter("p_id", newId));

        var afterDelete = await ExecuteScalarAsync(
            "SELECT COUNT(*) FROM settings_weldedcoils WHERE id = @id;",
            new MySqlParameter("@id", newId));
        ToInt(afterDelete).Should().Be(0);
    }

    [SkippableFact]
    public async Task WeldedCoils_Insert_ShouldRejectADuplicatePartNumber()
    {
        SkipWhenUnavailable();

        await ExecuteProcedureWithOutputsAsync(
            "sp_Settings_WeldedCoils_Insert",
            Parameter("p_partid", _partId),
            new MySqlParameter("p_new_id", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output });

        var insertAgain = async () => await ExecuteProcedureWithOutputsAsync(
            "sp_Settings_WeldedCoils_Insert",
            Parameter("p_partid", _partId),
            new MySqlParameter("p_new_id", MySqlDbType.Int32) { Direction = System.Data.ParameterDirection.Output });

        await insertAgain.Should().ThrowAsync<MySqlException>(
            "the procedure reports a duplicate part number before the unique key does");
    }
}

using FluentAssertions;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Integration.Database.StoredProcedureCoverage;

/// <summary>
/// Exercises the settings-core procedures: the metadata listings, the system and per-user value
/// tables, the audit trail, and role assignment.
/// </summary>
[Collection(StoredProcedureCoverageCollection.Name)]
public sealed class SettingsCoreStoredProcedureTests : StoredProcedureTestBase, IAsyncLifetime
{
    private const int TestUserId = 990991;

    private string _category = string.Empty;
    private string _settingKey = string.Empty;

    public SettingsCoreStoredProcedureTests(StoredProcedureTestDatabase database)
        : base(database)
    {
    }

    public Task InitializeAsync()
    {
        var token = UniqueToken("TEST-SC-");
        _category = token + "CATEGORY";
        _settingKey = token + "KEY";
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Database.ConnectionString is null)
        {
            return;
        }

        await ExecuteSqlAsync(
            "DELETE FROM settings_universal WHERE category = @category;",
            new MySqlParameter("@category", _category));
        await ExecuteSqlAsync(
            "DELETE FROM settings_personal WHERE category = @category;",
            new MySqlParameter("@category", _category));
        await ExecuteSqlAsync(
            "DELETE FROM settings_activity WHERE category = @category;",
            new MySqlParameter("@category", _category));
        await ExecuteSqlAsync(
            "DELETE FROM settings_user_roles WHERE user_id = @userId;",
            new MySqlParameter("@userId", TestUserId));
    }

    [SkippableFact]
    public async Task Meta_ShouldReportTheSettingsTablesAndProcedures()
    {
        SkipWhenUnavailable();

        var tables = await QueryAsync("sp_SettingsCore_Meta_GetTables");
        tables.Rows.Cast<System.Data.DataRow>()
            .Select(row => row["table_name"].ToString())
            .Should().Contain(["settings_universal", "settings_personal", "settings_activity"]);

        var procedures = await QueryAsync("sp_SettingsCore_Meta_GetStoredProcedures");
        var names = procedures.Rows.Cast<System.Data.DataRow>()
            .Select(row => row["procedure_name"].ToString())
            .ToList();

        names.Should().OnlyContain(name => name!.StartsWith("sp_SettingsCore_", StringComparison.Ordinal));
        names.Should().Contain("sp_SettingsCore_System_GetByKey");
    }

    [SkippableFact]
    public async Task System_ShouldUpsertReadAndResetAValue()
    {
        SkipWhenUnavailable();

        await ExecuteProcedureAsync(
            "sp_SettingsCore_System_Upsert",
            Parameter("p_category", _category),
            Parameter("p_key", _settingKey),
            Parameter("p_value", "TEST-VALUE"),
            Parameter("p_data_type", "string"),
            Parameter("p_is_sensitive", 0),
            Parameter("p_updated_by", "TEST-OWNER"));

        var byKey = await QueryAsync(
            "sp_SettingsCore_System_GetByKey",
            Parameter("p_category", _category),
            Parameter("p_key", _settingKey));
        var row = byKey.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        row["setting_value"].ToString().Should().Be("TEST-VALUE");

        var byCategory = await QueryAsync(
            "sp_SettingsCore_System_GetByCategory",
            Parameter("p_category", _category));
        byCategory.Rows.Cast<System.Data.DataRow>()
            .Should().Contain(candidate => (string)candidate["setting_key"] == _settingKey);

        await ExecuteProcedureAsync(
            "sp_SettingsCore_System_Reset",
            Parameter("p_category", _category),
            Parameter("p_key", _settingKey),
            Parameter("p_updated_by", "TEST-RESETTER"));

        var reset = await ExecuteScalarAsync(
            "SELECT updated_by FROM settings_universal WHERE category = @category AND setting_key = @key;",
            new MySqlParameter("@category", _category),
            new MySqlParameter("@key", _settingKey));
        ToText(reset).Should().Be("TEST-RESETTER");
    }

    [SkippableFact]
    public async Task User_ShouldUpsertReadAndResetAValue()
    {
        SkipWhenUnavailable();

        await ExecuteProcedureAsync(
            "sp_SettingsCore_User_Upsert",
            Parameter("p_user_id", TestUserId),
            Parameter("p_category", _category),
            Parameter("p_key", _settingKey),
            Parameter("p_value", "TEST-USER-VALUE"),
            Parameter("p_data_type", "string"),
            Parameter("p_updated_by", "TEST-OWNER"));

        var byKey = await QueryAsync(
            "sp_SettingsCore_User_GetByKey",
            Parameter("p_user_id", TestUserId),
            Parameter("p_category", _category),
            Parameter("p_key", _settingKey));
        var row = byKey.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        row["setting_value"].ToString().Should().Be("TEST-USER-VALUE");

        var byCategory = await QueryAsync(
            "sp_SettingsCore_User_GetByCategory",
            Parameter("p_user_id", TestUserId),
            Parameter("p_category", _category));
        byCategory.Rows.Cast<System.Data.DataRow>()
            .Should().Contain(candidate => (string)candidate["setting_key"] == _settingKey);

        await ExecuteProcedureAsync(
            "sp_SettingsCore_User_Reset",
            Parameter("p_user_id", TestUserId),
            Parameter("p_category", _category),
            Parameter("p_key", _settingKey),
            Parameter("p_updated_by", "TEST-RESETTER"));

        var reset = await ExecuteScalarAsync(
            "SELECT updated_by FROM settings_personal WHERE user_id = @userId "
            + "AND category = @category AND setting_key = @key;",
            new MySqlParameter("@userId", TestUserId),
            new MySqlParameter("@category", _category),
            new MySqlParameter("@key", _settingKey));
        ToText(reset).Should().Be("TEST-RESETTER");
    }

    [SkippableFact]
    public async Task Audit_ShouldInsertAndReadBackBySettingAndByUser()
    {
        SkipWhenUnavailable();

        await ExecuteProcedureAsync(
            "sp_SettingsCore_Audit_Insert",
            Parameter("p_scope", "System"),
            Parameter("p_category", _category),
            Parameter("p_setting_key", _settingKey),
            Parameter("p_old_value", "TEST-OLD"),
            Parameter("p_new_value", "TEST-NEW"),
            Parameter("p_change_type", "Update"),
            Parameter("p_user_id", TestUserId),
            Parameter("p_changed_by", "TEST-USER"),
            Parameter("p_ip_address", "127.0.0.1"),
            Parameter("p_workstation", "TEST-PC"));

        var bySetting = await QueryAsync(
            "sp_SettingsCore_Audit_GetBySetting",
            Parameter("p_category", _category),
            Parameter("p_setting_key", _settingKey));
        var settingRow = bySetting.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        settingRow["old_value"].ToString().Should().Be("TEST-OLD");
        settingRow["new_value"].ToString().Should().Be("TEST-NEW");

        var byUser = await QueryAsync(
            "sp_SettingsCore_Audit_GetByUser",
            Parameter("p_user_id", TestUserId));
        byUser.Rows.Cast<System.Data.DataRow>()
            .Should().Contain(row => (string)row["setting_key"] == _settingKey);
    }

    [SkippableFact]
    public async Task Roles_ShouldListFindByNameAndAssignToAUser()
    {
        SkipWhenUnavailable();

        var roles = await QueryAsync("sp_SettingsCore_Roles_GetAll");
        var roleRows = roles.Rows.Cast<System.Data.DataRow>().ToList();
        roleRows.Should().NotBeEmpty("the schema seeds its roles");

        var roleName = roleRows[0]["role_name"].ToString()!;
        var roleId = ToInt(roleRows[0]["id"]);

        var byName = await QueryAsync(
            "sp_SettingsCore_Roles_GetByName",
            Parameter("p_role_name", roleName));
        var named = byName.Rows.Cast<System.Data.DataRow>().Should().ContainSingle().Subject;
        ToInt(named["id"]).Should().Be(roleId);

        await ExecuteProcedureAsync(
            "sp_SettingsCore_UserRoles_Assign",
            Parameter("p_user_id", TestUserId),
            Parameter("p_role_id", roleId));

        var assigned = await QueryAsync(
            "sp_SettingsCore_UserRoles_GetByUser",
            Parameter("p_user_id", TestUserId));
        assigned.Rows.Cast<System.Data.DataRow>()
            .Should().Contain(row => ToInt(row["role_id"]) == roleId);

        // Assigning the same role twice must stay silent rather than fail.
        await ExecuteProcedureAsync(
            "sp_SettingsCore_UserRoles_Assign",
            Parameter("p_user_id", TestUserId),
            Parameter("p_role_id", roleId));

        var stillOne = await ExecuteScalarAsync(
            "SELECT COUNT(*) FROM settings_user_roles WHERE user_id = @userId AND role_id = @roleId;",
            new MySqlParameter("@userId", TestUserId),
            new MySqlParameter("@roleId", roleId));
        ToInt(stillOne).Should().Be(1);
    }
}

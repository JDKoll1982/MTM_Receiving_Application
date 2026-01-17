using System;
using FluentAssertions;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Tests.Helpers;
using Xunit.Sdk;

namespace MTM_Receiving_Application.Tests.Module_Settings.Core.Integration;

/// <summary>
/// Integration tests for core settings system DAO.
/// </summary>
[Collection("Database")]
public class SettingsCoreSystemDaoIntegrationTests
{
    [SkippableFact]
    public async Task Upsert_ShouldPersist_SystemSetting()
    {
        var connectionString = Helper_Database_Variables.GetConnectionString(useProduction: false);
        var (isReady, reason) = await DatabasePreflight.CheckAsync(connectionString, "sp_SettingsCore_System_Upsert");
        Skip.If(!isReady, reason ?? "Database not ready");

        var dao = new Dao_SettingsCoreSystem(connectionString);
        var category = "System";
        var key = $"TEST-{Guid.NewGuid():N}";

        var upsertResult = await dao.UpsertAsync(category, key, "1", "Int", false, "test");
        upsertResult.Success.Should().BeTrue();

        var getResult = await dao.GetByKeyAsync(category, key);
        getResult.Success.Should().BeTrue();
        getResult.Data.Should().NotBeNull();
        getResult.Data!.SettingKey.Should().Be(key);
    }
}

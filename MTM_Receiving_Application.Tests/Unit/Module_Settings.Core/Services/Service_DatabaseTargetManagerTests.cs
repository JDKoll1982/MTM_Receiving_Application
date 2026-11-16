using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Helpers;
using MTM_Receiving_Application.Module_Settings.Core.Services;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Core.Services;

/// <summary>
/// The Database Config page is the only place an operator can point a PC at the test database, and
/// the picker used to offer a "Test" entry that carried the live database name - choosing it saved
/// nothing. These tests keep the offered targets honest and prove the local override round-trips.
/// </summary>
public sealed class Service_DatabaseTargetManagerTests : IDisposable
{
    private const string LiveConnection =
        "Server=172.16.1.104;Port=3306;Database=mtm_receiving_application;User ID=app;Password=p;";

    private readonly bool _overrideExistedBefore;
    private readonly string? _overrideContentBefore;

    public Service_DatabaseTargetManagerTests()
    {
        _overrideExistedBefore = File.Exists(Helper_LocalAppConfigFile.OverrideFilePath);
        if (_overrideExistedBefore)
        {
            _overrideContentBefore = File.ReadAllText(Helper_LocalAppConfigFile.OverrideFilePath);
        }
    }

    public void Dispose()
    {
        // The manager writes next to the running app, so put the file back exactly as it was.
        if (_overrideExistedBefore)
        {
            File.WriteAllText(Helper_LocalAppConfigFile.OverrideFilePath, _overrideContentBefore!);
        }
        else if (File.Exists(Helper_LocalAppConfigFile.OverrideFilePath))
        {
            File.Delete(Helper_LocalAppConfigFile.OverrideFilePath);
        }
    }

    [Fact]
    public void Targets_ShouldOfferLiveAndTestWithDistinctDatabaseNames()
    {
        var manager = CreateManager();

        manager.Targets.Should().HaveCount(2);
        manager.Targets.Select(target => target.DatabaseName).Should().OnlyHaveUniqueItems();

        manager.Targets.Select(target => target.DatabaseName)
            .Should().BeEquivalentTo(
                "mtm_receiving_application",
                "mtm_receiving_application_test");

        var test = manager.Targets.Single(target => target.Id == "test");
        test.DatabaseName.Should().Be("mtm_receiving_application_test");
    }

    [Fact]
    public void BuildConnectionString_ShouldSwapOnlyTheDatabase_WhenTheTargetChanges()
    {
        var manager = CreateManager();
        var test = manager.Targets.Single(target => target.Id == "test");
        var live = manager.Targets.Single(target => target.Id == "live");

        var testBuilder = new MySqlConnectionStringBuilder(manager.BuildConnectionString(test));
        var liveBuilder = new MySqlConnectionStringBuilder(manager.BuildConnectionString(live));

        testBuilder.Database.Should().Be("mtm_receiving_application_test");
        liveBuilder.Database.Should().Be("mtm_receiving_application");

        // Everything else must survive the swap, or the app would reconnect with other credentials.
        testBuilder.Server.Should().Be("172.16.1.104");
        testBuilder.UserID.Should().Be("app");
        testBuilder.Password.Should().Be("p");
        testBuilder.Port.Should().Be(3306);
    }

    [Fact]
    public async Task SaveTargetAsync_ShouldMakeTheTestDatabaseThePendingOne()
    {
        var manager = CreateManager();
        var test = manager.Targets.Single(target => target.Id == "test");

        await manager.SaveTargetAsync(test);

        (await manager.GetPendingDatabaseNameAsync()).Should().Be("mtm_receiving_application_test");
        (await manager.GetIsRestartPendingAsync()).Should().BeTrue(
            "the running application is still connected to the live database");
        manager.CurrentDatabaseName.Should().Be("mtm_receiving_application");
    }

    [Fact]
    public async Task SaveTargetAsync_ShouldKeepTheRestOfTheOverrideFileIntact()
    {
        await File.WriteAllTextAsync(
            Helper_LocalAppConfigFile.OverrideFilePath,
            """{ "Custom": { "KeepMe": "yes" }, "ConnectionStrings": { "MySql": "old" } }""");

        var manager = CreateManager();
        var test = manager.Targets.Single(target => target.Id == "test");

        await manager.SaveTargetAsync(test);

        var saved = await File.ReadAllTextAsync(Helper_LocalAppConfigFile.OverrideFilePath);
        saved.Should().Contain("KeepMe");
        saved.Should().Contain("mtm_receiving_application_test");
        saved.Should().NotContain("\"MySql\": \"old\"");
    }

    [Fact]
    public async Task GetIsRestartPendingAsync_ShouldBeFalse_WhenTheSavedTargetAlreadyMatches()
    {
        var manager = CreateManager();
        var live = manager.Targets.Single(target => target.Id == "live");

        await manager.SaveTargetAsync(live);

        (await manager.GetIsRestartPendingAsync()).Should().BeFalse();
        (await manager.GetPendingDatabaseNameAsync()).Should().Be("mtm_receiving_application");
    }

    [Fact]
    public void FindByDatabaseName_ShouldResolveEachTarget()
    {
        var manager = CreateManager();

        manager.FindByDatabaseName("mtm_receiving_application_test")!.Id.Should().Be("test");
        manager.FindByDatabaseName("MTM_RECEIVING_APPLICATION")!.Id.Should().Be("live");
        manager.FindByDatabaseName("mtm_receiving_application_backup_test").Should().BeNull(
            "the disposable SyncTool copy is no longer a selectable target");
        manager.FindByDatabaseName(null).Should().BeNull();
    }

    private Service_DatabaseTargetManager CreateManager()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:MySql"] = LiveConnection,
                })
            .Build();

        return new Service_DatabaseTargetManager(
            configuration,
            new Mock<IService_LoggingUtility>().Object);
    }
}

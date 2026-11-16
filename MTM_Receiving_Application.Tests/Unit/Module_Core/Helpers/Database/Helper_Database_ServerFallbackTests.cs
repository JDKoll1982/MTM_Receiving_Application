using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Helpers.Database;

public sealed class Helper_Database_ServerFallbackTests
{
    private const string ShopServer = "172.16.1.104";

    [Fact]
    public void ResolveConnectionString_ShouldKeepTheConfiguredServer_WhenItAnswers()
    {
        var resolved = Helper_Database_ServerFallback.ResolveConnectionString(
            BuildConnectionString(ShopServer),
            canReach: (server, _) => server == ShopServer);

        resolved.Should().BeNull("the configured server answered, so nothing may be retargeted");
    }

    [Fact]
    public void ResolveConnectionString_ShouldUseLocalHost_WhenTheConfiguredServerDoesNotAnswer()
    {
        var resolved = Helper_Database_ServerFallback.ResolveConnectionString(
            BuildConnectionString(ShopServer),
            canReach: (server, _) => server == Helper_Database_ServerFallback.DefaultFallbackServer);

        resolved.Should().NotBeNull();

        var builder = new MySqlConnectionStringBuilder(resolved!);
        builder.Server.Should().Be(Helper_Database_ServerFallback.DefaultFallbackServer);
        builder.Database.Should().Be("mtm_receiving_application", "only the server may change");
        builder.UserID.Should().Be("root");
        builder.Password.Should().Be("secret");
        builder.Port.Should().Be(3306);
    }

    [Fact]
    public void ResolveConnectionString_ShouldUseTheConfiguredSpareServer_WhenOneIsGiven()
    {
        var resolved = Helper_Database_ServerFallback.ResolveConnectionString(
            BuildConnectionString(ShopServer),
            fallbackServer: "mtm-spare",
            canReach: (server, _) => server == "mtm-spare");

        new MySqlConnectionStringBuilder(resolved!).Server.Should().Be("mtm-spare");
    }

    [Fact]
    public void ResolveConnectionString_ShouldKeepTheConfiguredServer_WhenNoSpareAnswersEither()
    {
        var resolved = Helper_Database_ServerFallback.ResolveConnectionString(
            BuildConnectionString(ShopServer),
            canReach: (_, _) => false);

        resolved.Should().BeNull(
            "a dead spare is no better than a dead primary, and the operator needs the original "
            + "host name in the error message");
    }

    [Fact]
    public void ResolveConnectionString_ShouldDoNothing_WhenTheSpareIsTheConfiguredServer()
    {
        var probed = 0;

        var resolved = Helper_Database_ServerFallback.ResolveConnectionString(
            BuildConnectionString("127.0.0.1"),
            canReach: (_, _) =>
            {
                probed++;
                return false;
            });

        resolved.Should().BeNull();
        probed.Should().Be(1, "the primary is probed once and the identical spare is not");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("this is not a connection string")]
    public void ResolveConnectionString_ShouldReturnNull_WhenThereIsNothingUsable(string? connectionString)
    {
        var resolved = Helper_Database_ServerFallback.ResolveConnectionString(
            connectionString,
            canReach: (_, _) => true);

        resolved.Should().BeNull();
    }

    [Fact]
    public void BuildConfigurationOverride_ShouldOnlyCarryTheMySqlKey()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:MySql"] = BuildConnectionString(ShopServer),
                    ["AppSettings:MySqlFallbackServer"] = "127.0.0.1",
                }
            )
            .Build();

        var overrides = Helper_Database_ServerFallback.BuildConfigurationOverride(
            configuration,
            (server, _) => server == "127.0.0.1");

        overrides.Should().ContainSingle();
        overrides.Should().ContainKey("ConnectionStrings:MySql");
        new MySqlConnectionStringBuilder(overrides["ConnectionStrings:MySql"]!).Server
            .Should().Be("127.0.0.1");
    }

    [Fact]
    public void BuildConfigurationOverride_ShouldBeEmpty_WhenTheConfiguredServerAnswers()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:MySql"] = BuildConnectionString(ShopServer),
                }
            )
            .Build();

        var overrides = Helper_Database_ServerFallback.BuildConfigurationOverride(
            configuration,
            (_, _) => true);

        overrides.Should().BeEmpty();
    }

    [Fact]
    public void DescribeTargetServer_ShouldNameTheHostAndPort()
    {
        Helper_Database_ServerFallback.DescribeTargetServer(BuildConnectionString(ShopServer))
            .Should().Be($"{ShopServer}:3306");
        Helper_Database_ServerFallback.DescribeTargetServer(null).Should().BeNull();
        Helper_Database_ServerFallback.DescribeTargetServer("not a connection string").Should().BeNull();
    }

    [Fact]
    public void BuildConfigurationOverride_ShouldRetargetTheShippedSettings_WhenTheShopServerIsDown()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(ResolveAppSettingsPath(), optional: false)
            .Build();

        var overrides = Helper_Database_ServerFallback.BuildConfigurationOverride(
            configuration,
            (server, _) => server == Helper_Database_ServerFallback.DefaultFallbackServer);

        overrides.Should().ContainKey("ConnectionStrings:MySql");

        var builder = new MySqlConnectionStringBuilder(overrides["ConnectionStrings:MySql"]!);
        builder.Server.Should().Be(Helper_Database_ServerFallback.DefaultFallbackServer);
        builder.Database.Should().NotBeNullOrWhiteSpace(
            "only the server may change, the target database must survive the fallback");
        builder.UserID.Should().NotBeNullOrWhiteSpace();
    }

    private static string ResolveAppSettingsPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "appsettings.json");
            if (File.Exists(candidate)
                && File.Exists(Path.Combine(directory.FullName, "MTM_Receiving_Application.slnx")))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("appsettings.json could not be located from the test output.");
    }

    private static string BuildConnectionString(string server) =>
        $"Server={server};Port=3306;Database=mtm_receiving_application;Uid=root;Pwd=secret;";
}

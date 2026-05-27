using FluentAssertions;
using MTM_Receiving_Application.Module_ShipRec_Tools.Data.CustomerPullPack;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Tests.Integration.Module_ShipRec_Tools;

public sealed class Dao_CustomerPullPackUserDefaultsIntegrationTests
{
    [Fact]
    public async Task UpsertAsync_ThenGetByUserIdAsync_ShouldRoundTripDefaults_WhenIntegrationConnectionIsAvailable()
    {
        var connectionString = TryGetIntegrationConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var dao = new Dao_CustomerPullPackUserDefaults(connectionString);
        var suffix = Guid.NewGuid().ToString("N");
        var defaults = new Model_CustomerPullPack_UserDefaults
        {
            UserId = $"test.cpp.defaults.{suffix}",
            DefaultCustomerId = "VOLVO",
            FavoriteCustomerIds = ["VOLVO", "MACK"],
            LastGoodDateRangeType = "Custom",
            LastGoodDateFrom = new DateTime(2026, 5, 25),
            LastGoodDateTo = new DateTime(2026, 6, 1),
            DefaultSortMode = Enum_CustomerPullPackSortMode.Part,
            DefaultShortagesOnly = true,
            DefaultUnpulledOnly = false,
            DefaultLateOrdersOnly = true,
            DefaultWaitlistStatusSet =
            [
                Enum_CustomerPullPackWaitlistStatus.Requested,
                Enum_CustomerPullPackWaitlistStatus.Problem,
            ],
            DefaultPrintPreset = Enum_CustomerPullPackPrintMode.PullList,
        };

        var upsertResult = await dao.UpsertAsync(defaults, defaults.UserId);

        upsertResult.IsSuccess.Should().BeTrue();
        upsertResult.Data.Should().NotBeNull();

        var loadResult = await dao.GetByUserIdAsync(defaults.UserId);

        loadResult.IsSuccess.Should().BeTrue();
        loadResult.Data.Should().NotBeNull();
        loadResult.Data!.UserId.Should().Be(defaults.UserId);
        loadResult.Data.DefaultCustomerId.Should().Be(defaults.DefaultCustomerId);
        loadResult.Data.FavoriteCustomerIds.Should().BeEquivalentTo(defaults.FavoriteCustomerIds);
        loadResult.Data.LastGoodDateRangeType.Should().Be(defaults.LastGoodDateRangeType);
        loadResult.Data.LastGoodDateFrom.Should().Be(defaults.LastGoodDateFrom);
        loadResult.Data.LastGoodDateTo.Should().Be(defaults.LastGoodDateTo);
        loadResult.Data.DefaultSortMode.Should().Be(defaults.DefaultSortMode);
        loadResult.Data.DefaultShortagesOnly.Should().Be(defaults.DefaultShortagesOnly);
        loadResult.Data.DefaultUnpulledOnly.Should().Be(defaults.DefaultUnpulledOnly);
        loadResult.Data.DefaultLateOrdersOnly.Should().Be(defaults.DefaultLateOrdersOnly);
        loadResult
            .Data.DefaultWaitlistStatusSet.Should()
            .BeEquivalentTo(defaults.DefaultWaitlistStatusSet);
        loadResult.Data.DefaultPrintPreset.Should().Be(defaults.DefaultPrintPreset);
    }

    private static string? TryGetIntegrationConnectionString()
    {
        return Environment.GetEnvironmentVariable("MTM_TEST_MYSQL_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("MTM_MYSQL_CONNECTION_STRING");
    }
}

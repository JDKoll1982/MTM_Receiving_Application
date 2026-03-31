using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Services.Database;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Core.Services.Database;

public sealed class Service_InforVisualConnectTests
{
    [Fact]
    public async Task FuzzySearchLocationsAsync_ShouldReturnAllMockLocations_WhenTermIsEmpty()
    {
        var service = CreateService(useMockData: true);

        var result = await service.FuzzySearchLocationsAsync(string.Empty, "002");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result
            .Data!.Select(item => item.Label)
            .Should()
            .Equal("A-RECV-01", "B-RECV-02", "C-RECV-03", "DOCK-4", "QA-RECV", "RECV");
    }

    [Fact]
    public async Task FuzzySearchLocationsAsync_ShouldFilterStableMockLocations_WhenTermIsProvided()
    {
        var service = CreateService(useMockData: true);

        var result = await service.FuzzySearchLocationsAsync("C-RECV", "002");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainSingle();
        result.Data![0].Label.Should().Be("C-RECV-03");
        result.Data[0].Detail.Should().Be("Warehouse 002 — Mock location");
    }

    [Fact]
    public async Task FuzzySearchLocationsAsync_ShouldFail_WhenWarehouseCodeIsBlank()
    {
        var service = CreateService(useMockData: true);

        var result = await service.FuzzySearchLocationsAsync(string.Empty, string.Empty);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Warehouse code cannot be empty");
    }

    private static Service_InforVisualConnect CreateService(bool useMockData)
    {
        var dao = new Dao_InforVisualConnection(
            "Server=VISUAL;Database=MTMFG;ApplicationIntent=ReadOnly;Trusted_Connection=True;",
            new Mock<IService_LoggingUtility>().Object
        );

        return new Service_InforVisualConnect(
            dao,
            useMockData,
            new Mock<IService_LoggingUtility>().Object
        );
    }
}

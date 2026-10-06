using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;
using MTM_Receiving_Application.Module_Shared.Services.Lookup;

namespace MTM_Receiving_Application.Tests.Unit.Module_Shared.Services.Lookup;

/// <summary>
/// Validates the shared location-range expansion used by the Scanner Workbench and the ShipRec
/// Tools Material Availability Board: bound formatting, auto-swap, ordering, truncation, and the
/// failure paths an operator can actually hit.
/// </summary>
public sealed class Service_SharedLocationRangeTests
{
    private const string WarehouseCode = "002";

    private static Model_InforVisualLocationRow LocationRow(string locationId, string warehouse = WarehouseCode)
    {
        return new Model_InforVisualLocationRow
        {
            LocationId = locationId,
            WarehouseCode = warehouse,
        };
    }

    [Fact]
    public async Task ResolveRangeAsync_ShouldFormatBothBounds_AndReturnExistingLocations()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service =>
                service.GetLocationsInRangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationRow>
                    {
                        LocationRow("V-A0-01"),
                        LocationRow("V-A0-02"),
                        LocationRow("V-A0-03"),
                    }
                )
            );

        var rangeService = new Service_SharedLocationRange(inforVisual.Object);

        var result = await rangeService.ResolveRangeAsync("va0-01", "V-A0-3", WarehouseCode);

        result.IsSuccess.Should().BeTrue();
        result.Data!.StartLocation.Should().Be("V-A0-01");
        result.Data.StopLocation.Should().Be("V-A0-03");
        result.Data.Locations.Should().Equal("V-A0-01", "V-A0-02", "V-A0-03");
        result.Data.WasSwapped.Should().BeFalse();
        result.Data.WasTruncated.Should().BeFalse();

        // The formatted (canonical) bounds are what reach Infor Visual.
        inforVisual.Verify(
            service =>
                service.GetLocationsInRangeAsync("V-A0-01", "V-A0-03", WarehouseCode, It.IsAny<int>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ResolveRangeAsync_ShouldSwapBounds_WhenStartSortsAfterStop()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service =>
                service.GetLocationsInRangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationRow> { LocationRow("V-A0-01") }
                )
            );

        var rangeService = new Service_SharedLocationRange(inforVisual.Object);

        var result = await rangeService.ResolveRangeAsync("V-A0-05", "V-A0-01", WarehouseCode);

        result.IsSuccess.Should().BeTrue();
        result.Data!.WasSwapped.Should().BeTrue();
        result.Data.StartLocation.Should().Be("V-A0-01");
        result.Data.StopLocation.Should().Be("V-A0-05");

        inforVisual.Verify(
            service =>
                service.GetLocationsInRangeAsync("V-A0-01", "V-A0-05", WarehouseCode, It.IsAny<int>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ResolveRangeAsync_ShouldOrderAndDeduplicateLocations()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service =>
                service.GetLocationsInRangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualLocationRow>
                    {
                        LocationRow("V-A0-03"),
                        LocationRow("V-A0-01"),
                        LocationRow("v-a0-03"),
                        LocationRow("   "),
                    }
                )
            );

        var rangeService = new Service_SharedLocationRange(inforVisual.Object);

        var result = await rangeService.ResolveRangeAsync("V-A0-01", "V-A0-03", WarehouseCode);

        result.Data!.Locations.Should().Equal("V-A0-01", "V-A0-03");
    }

    [Fact]
    public async Task ResolveRangeAsync_ShouldTruncate_WhenRangeExceedsTheCap()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service =>
                service.GetLocationsInRangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync(
                (string _, string _, string _, int maxResults) =>
                    Model_Dao_Result_Factory.Success(
                        Enumerable
                            .Range(1, maxResults)
                            .Select(index => LocationRow($"V-A0-{index:00}"))
                            .ToList()
                    )
            );

        var rangeService = new Service_SharedLocationRange(inforVisual.Object);

        var result = await rangeService.ResolveRangeAsync("V-A0-01", "V-A9-99", WarehouseCode);

        result.IsSuccess.Should().BeTrue();
        result.Data!.WasTruncated.Should().BeTrue();
        result.Data.Locations.Should().HaveCount(rangeService.MaxLocationsInRange);
    }

    [Theory]
    [InlineData("RECV", "V-A0-05", "start")]
    [InlineData("V-A0-01", "RECV", "stop")]
    public async Task ResolveRangeAsync_ShouldFail_WhenABoundCannotBeFormatted(
        string startInput,
        string stopInput,
        string expectedWordInMessage
    )
    {
        var inforVisual = new Mock<IService_InforVisual>();
        var rangeService = new Service_SharedLocationRange(inforVisual.Object);

        var result = await rangeService.ResolveRangeAsync(startInput, stopInput, WarehouseCode);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain(expectedWordInMessage);
        inforVisual.Verify(
            service =>
                service.GetLocationsInRangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ResolveRangeAsync_ShouldPropagateFailure_WhenInforVisualFails()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service =>
                service.GetLocationsInRangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_InforVisualLocationRow>>(
                    "Infor Visual is unavailable."
                )
            );

        var rangeService = new Service_SharedLocationRange(inforVisual.Object);

        var result = await rangeService.ResolveRangeAsync("V-A0-01", "V-A0-05", WarehouseCode);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Infor Visual is unavailable.");
    }
}

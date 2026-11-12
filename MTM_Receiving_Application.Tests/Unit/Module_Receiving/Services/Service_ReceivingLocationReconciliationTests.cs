using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Services;
using Xunit;
using DaoResultFactory = MTM_Receiving_Application.Module_Core.Models.Core.Model_Dao_Result_Factory;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.Services;

/// <summary>
/// Only the entry-time recommendation stays in Receiving. The allocation cases this
/// file used to hold are rewritten in the Receiving Validator module (FR-066, TS-02).
/// </summary>
public sealed class Service_ReceivingLocationReconciliationTests
{
    [Fact]
    public async Task GetRecommendedLocationsAsync_ShouldIncludePreviouslyHardcodedLocations_WhenQuantityIsPositive()
    {
        var receivedDate = new DateTime(2026, 5, 7, 8, 0, 0);
        var inforVisualMock = new Mock<IService_InforVisual>();

        inforVisualMock
            .Setup(service =>
                service.GetReceivingLocationEvidenceAsync(
                    "PO-066868",
                    "MMC0000700",
                    "1",
                    receivedDate.Date
                )
            )
            .ReturnsAsync(
                DaoResultFactory.Success(
                    new List<Model_InforVisualLocationEvidence>
                    {
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "RECV",
                            CurrentQuantity = 4m,
                            LatestTransactionDate = receivedDate.AddHours(1),
                        },
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "FG",
                            CurrentQuantity = 3m,
                            LatestTransactionDate = receivedDate.AddHours(2),
                        },
                        new()
                        {
                            CurrentWarehouseId = "002",
                            CurrentLocationId = "WC",
                            CurrentQuantity = 0m,
                            LatestTransactionDate = receivedDate.AddHours(3),
                        },
                    }
                )
            );

        var service = new Service_ReceivingLocationReconciliation(inforVisualMock.Object);

        var result = await service.GetRecommendedLocationsAsync(
            "PO-066868",
            "MMC0000700",
            "1",
            receivedDate.Date
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Select(location => location.LocationId).Should().Equal("RECV", "FG");
        result.Data.Select(location => location.QuantityOnHand).Should().Equal(4m, 3m);
    }

    [Fact]
    public async Task GetRecommendedLocationsAsync_ShouldReturnNothingWhenThePurchaseOrderIsMissing()
    {
        var inforVisualMock = new Mock<IService_InforVisual>();
        var service = new Service_ReceivingLocationReconciliation(inforVisualMock.Object);

        var result = await service.GetRecommendedLocationsAsync(
            string.Empty,
            "MMC0000700",
            "1",
            null
        );

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}

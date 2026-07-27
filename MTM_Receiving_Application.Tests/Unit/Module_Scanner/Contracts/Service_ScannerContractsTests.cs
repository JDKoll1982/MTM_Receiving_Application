using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Contracts;

public sealed class Service_ScannerContractsTests
{
    [Fact]
    public async Task StartSessionAsync_ShouldFail_WhenOwnerUserIdMissing()
    {
        var service = CreateWorkflowService();

        var result = await service.StartSessionAsync(new Model_ScannerSessionStartRequest
        {
            OwnerUserId = string.Empty,
            OwnerDisplayName = "Operator A",
            ActiveProfileId = Guid.NewGuid(),
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("OwnerUserId is required.");
    }

    [Fact]
    public async Task StartSessionAsync_ShouldFail_WhenRequestIsNull()
    {
        var service = CreateWorkflowService();

        var result = await service.StartSessionAsync(null!);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Request is required.");
    }

    [Fact]
    public async Task ValidateNewItemAsync_ShouldReturnInvalidBoundary_WhenQuantityIsNotPositive()
    {
        var inforVisual = new Mock<IService_InforVisual>(MockBehavior.Strict);
        var service = new Service_ScannerValidation(inforVisual.Object);

        var result = await service.ValidateNewItemAsync(new Model_ScannerItemValidationRequest
        {
            SessionId = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            PartId = "MMCCS00740",
            FromWarehouse = "002",
            FromLocation = "A-01",
            ToWarehouse = "002",
            ToLocation = "B-01",
            Quantity = "0",
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.State.Should().Be(Enum_ScannerValidationState.Invalid);
        result.Data.Message.Should().Be("Quantity must be a positive number.");
        result.Data.Notes.Should().Be("Enter a quantity greater than zero.");
    }

    [Fact]
    public async Task ValidateNewItemAsync_ShouldReturnInvalidBoundary_WhenPartNotFoundInVisual()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service => service.PartExistsAsync("MMCCS00740"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(false));

        var service = new Service_ScannerValidation(inforVisual.Object);

        var result = await service.ValidateNewItemAsync(new Model_ScannerItemValidationRequest
        {
            SessionId = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            PartId = "mmccs00740",
            FromWarehouse = "002",
            FromLocation = "A-01",
            ToWarehouse = "002",
            ToLocation = "B-01",
            Quantity = "5",
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.State.Should().Be(Enum_ScannerValidationState.Invalid);
        result.Data.Message.Should().Be("Part ID was not found.");
    }

    [Fact]
    public async Task ValidateNewItemAsync_ShouldResolveLocationsWithoutDashes_WhenCanonicalVisualIdsExist()
    {
        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service => service.PartExistsAsync("MMCCS00740"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        inforVisual
            .Setup(service => service.LocationExistsAsync("A-01", "002"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(false));
        inforVisual
            .Setup(service => service.LocationExistsAsync("A01", "002"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        inforVisual
            .Setup(service => service.LocationExistsAsync("B-01", "002"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(false));
        inforVisual
            .Setup(service => service.LocationExistsAsync("B01", "002"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));
        inforVisual
            .Setup(service => service.GetMaterialAvailabilityCurrentStockAsync("A01", "MMCCS00740", "002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_InforVisualMaterialLocationRow>
                    {
                        new()
                        {
                            PartId = "MMCCS00740",
                            WarehouseCode = "002",
                            LocationId = "A01",
                            Quantity = 10m,
                        },
                    }
                )
            );

        var service = new Service_ScannerValidation(inforVisual.Object);

        var result = await service.ValidateNewItemAsync(new Model_ScannerItemValidationRequest
        {
            SessionId = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            PartId = "MMCCS00740",
            FromWarehouse = "002",
            FromLocation = "A-01",
            ToWarehouse = "002",
            ToLocation = "B-01",
            Quantity = "5",
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.State.Should().Be(Enum_ScannerValidationState.Valid);
        result.Data.CanonicalFromLocation.Should().Be("A01");
        result.Data.CanonicalToLocation.Should().Be("B01");
    }

    [Fact]
    public async Task SaveProfileAsync_ShouldFail_WhenProfileNameMissing()
    {
        var service = CreateWorkflowService();

        var result = await service.SaveProfileAsync(new Model_ScannerProfile
        {
            OwnerUserId = "u-123",
            ProfileName = string.Empty,
            AppWindowTitle = "Inventory Transfers",
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Profile name is required.");
    }

    [Fact]
    public async Task SetDefaultProfileAsync_ShouldFail_WhenOwnerUserIdMissing()
    {
        var service = CreateWorkflowService();

        var result = await service.SetDefaultProfileAsync(Guid.NewGuid(), string.Empty);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("OwnerUserId is required.");
    }

    private static Service_ScannerWorkflow CreateWorkflowService()
    {
        const string cs = "Server=localhost;Database=test;Uid=test;Pwd=test;";
        return new Service_ScannerWorkflow(
            new MTM_Receiving_Application.Module_Scanner.Data.Dao_ScannerBatchSession(cs),
            new MTM_Receiving_Application.Module_Scanner.Data.Dao_ScannerBatchItem(cs),
            new MTM_Receiving_Application.Module_Scanner.Data.Dao_ScannerRunHistory(cs),
            new MTM_Receiving_Application.Module_Scanner.Data.Dao_ScannerProfile(cs)
        );
    }
}

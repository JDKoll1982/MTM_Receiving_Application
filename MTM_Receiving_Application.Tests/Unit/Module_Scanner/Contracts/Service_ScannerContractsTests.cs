using FluentAssertions;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Contracts;

public sealed class Service_ScannerContractsTests
{
    [Fact]
    public async Task StartSessionAsync_ShouldFail_WhenOwnerUserIdMissing()
    {
        var service = new Service_ScannerWorkflow();

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
    public async Task StartSessionAsync_ShouldReturnResponseBoundary_WhenRequestIsValid()
    {
        var service = new Service_ScannerWorkflow();

        var result = await service.StartSessionAsync(new Model_ScannerSessionStartRequest
        {
            OwnerUserId = "u-123",
            OwnerDisplayName = "Operator A",
            ActiveProfileId = Guid.NewGuid(),
            AppWindowTitleSnapshot = "Inventory Transfers",
        });

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.SessionId.Should().NotBe(Guid.Empty);
        result.Data.Status.Should().Be(Enum_ScannerSessionStatus.Draft);
        result.Data.Message.Should().Be("Scanner session initialized.");
    }

    [Fact]
    public async Task ValidateNewItemAsync_ShouldReturnInvalidBoundary_WhenQuantityIsNotPositive()
    {
        var service = new Service_ScannerValidation();

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
}

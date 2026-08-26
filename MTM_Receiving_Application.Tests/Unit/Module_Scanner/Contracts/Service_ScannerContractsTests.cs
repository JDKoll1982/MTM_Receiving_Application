using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Scanner.Services;

namespace MTM_Receiving_Application.Tests.Unit.Module_Scanner.Contracts;

/// <summary>
/// Boundary-contract tests that run before any database call. DAOs are constructed with a
/// dummy connection string; only the validation guards are exercised.
/// </summary>
public sealed class Service_ScannerContractsTests
{
    private const string DummyConnectionString =
        "Server=localhost;Database=test;Uid=test;Pwd=test;Connection Timeout=1;Default Command Timeout=1;";

    [Fact]
    public async Task EnsureCurrentSessionAsync_ShouldFail_WhenOwnerUserIdMissing()
    {
        var service = CreateWorkflowService();

        var result = await service.EnsureCurrentSessionAsync(
            string.Empty,
            "Operator A",
            Guid.NewGuid()
        );

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("OwnerUserId is required.");
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldFail_WhenRequestIsNull()
    {
        var service = CreateWorkflowService();

        var result = await service.GetHistoryAsync(null!);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Request is required.");
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldFail_WhenOwnerUserIdMissing()
    {
        var service = CreateWorkflowService();

        var result = await service.GetHistoryAsync(new Model_ScannerHistoryQueryRequest
        {
            OwnerUserId = string.Empty,
        });

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("OwnerUserId is required.");
    }

    [Fact]
    public async Task GetProfilesAsync_ShouldFail_WhenOwnerUserIdMissing()
    {
        var service = CreateWorkflowService();

        var result = await service.GetProfilesAsync(string.Empty);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("OwnerUserId is required.");
    }

    [Fact]
    public async Task ValidateNewItemAsync_ShouldFail_WhenRequestIsNull()
    {
        var service = new Service_ScannerValidation(new Mock<IService_InforVisual>().Object);

        var result = await service.ValidateNewItemAsync(null!);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Validation request is required.");
    }

    [Fact]
    public async Task ValidateNewItemAsync_ShouldReturnInvalid_WhenQuantityIsNotPositive()
    {
        var service = new Service_ScannerValidation(new Mock<IService_InforVisual>().Object);

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
    }

    [Fact]
    public async Task ValidateNewItemAsync_ShouldReturnInvalid_WhenPartIdMissing()
    {
        var service = new Service_ScannerValidation(new Mock<IService_InforVisual>().Object);

        var result = await service.ValidateNewItemAsync(new Model_ScannerItemValidationRequest
        {
            SessionId = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            PartId = string.Empty,
            FromLocation = "A-01",
            ToLocation = "B-01",
            Quantity = "5",
        });

        result.Success.Should().BeTrue();
        result.Data!.State.Should().Be(Enum_ScannerValidationState.Invalid);
        result.Data.Message.Should().Be("Part ID is required.");
    }

    [Fact]
    public async Task PartExistsAsync_ShouldReturnFalse_WhenPartIdBlank()
    {
        var service = new Service_ScannerValidation(new Mock<IService_InforVisual>().Object);

        var result = await service.PartExistsAsync("   ");

        result.Success.Should().BeTrue();
        result.Data.Should().BeFalse();
    }

    private static Service_ScannerWorkflow CreateWorkflowService()
    {
        return new Service_ScannerWorkflow(
            new Dao_ScannerBatchSession(DummyConnectionString),
            new Dao_ScannerBatchItem(DummyConnectionString),
            new Dao_ScannerProfile(DummyConnectionString)
        );
    }
}

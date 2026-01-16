using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Commands;

/// <summary>
/// Unit tests for SavePendingShipmentCommandHandler
/// </summary>
public class SavePendingShipmentCommandHandlerTests
{
    private readonly Mock<IService_Volvo> _mockVolvoService;
    private readonly SavePendingShipmentCommandHandler _handler;

    public SavePendingShipmentCommandHandlerTests()
    {
        _mockVolvoService = new Mock<IService_Volvo>();
        _handler = new SavePendingShipmentCommandHandler(_mockVolvoService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnShipmentId_WhenSaveSuccessful()
    {
        // Arrange
        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            Notes = "Test shipment",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto
                {
                    PartNumber = "V-EMB-500",
                    ReceivedSkidCount = 5,
                    HasDiscrepancy = false
                }
            }
        };

        var expectedShipmentId = 123;
        var expectedShipmentNumber = 100;

        _mockVolvoService
            .Setup(x => x.SaveShipmentAsPendingAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<ShipmentLineDto>>()))
            .ReturnsAsync(new Model_Dao_Result<(int ShipmentId, int ShipmentNumber)>
            {
                Success = true,
                Data = (expectedShipmentId, expectedShipmentNumber)
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.ShipmentId.Should().Be(expectedShipmentId);
        result.Data.ShipmentNumber.Should().Be(expectedShipmentNumber);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenServiceFails()
    {
        // Arrange
        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            Notes = "",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto { PartNumber = "V-EMB-500", ReceivedSkidCount = 5 }
            }
        };

        var expectedError = "Database save failed";

        _mockVolvoService
            .Setup(x => x.SaveShipmentAsPendingAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<List<ShipmentLineDto>>()))
            .ReturnsAsync(new Model_Dao_Result<(int, int)>
            {
                Success = false,
                ErrorMessage = expectedError
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain(expectedError);
    }

    [Fact]
    public async Task Handle_ShouldPassParametersToService()
    {
        // Arrange
        var expectedDate = DateTimeOffset.Now.AddDays(-1);
        var expectedNumber = 42;
        var expectedNotes = "Test notes";
        var expectedParts = new List<ShipmentLineDto>
        {
            new ShipmentLineDto { PartNumber = "V-EMB-500", ReceivedSkidCount = 5 }
        };

        var command = new SavePendingShipmentCommand
        {
            ShipmentDate = expectedDate,
            ShipmentNumber = expectedNumber,
            Notes = expectedNotes,
            Parts = expectedParts
        };

        _mockVolvoService
            .Setup(x => x.SaveShipmentAsPendingAsync(
                expectedDate,
                expectedNumber,
                expectedNotes,
                expectedParts))
            .ReturnsAsync(new Model_Dao_Result<(int, int)>
            {
                Success = true,
                Data = (1, 42)
            });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockVolvoService.Verify(
            x => x.SaveShipmentAsPendingAsync(
                expectedDate,
                expectedNumber,
                expectedNotes,
                expectedParts),
            Times.Once);
    }
}

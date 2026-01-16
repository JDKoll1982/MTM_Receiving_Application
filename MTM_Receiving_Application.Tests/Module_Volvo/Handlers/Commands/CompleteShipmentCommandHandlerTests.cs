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
using MTM_Receiving_Application.Module_Volvo.Services;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Commands;

/// <summary>
/// Unit tests for CompleteShipmentCommandHandler
/// </summary>
public class CompleteShipmentCommandHandlerTests
{
    private readonly Mock<IService_Volvo> _mockVolvoService;
    private readonly Mock<IService_VolvoAuthorization> _mockAuthService;
    private readonly CompleteShipmentCommandHandler _handler;

    public CompleteShipmentCommandHandlerTests()
    {
        _mockVolvoService = new Mock<IService_Volvo>();
        _mockAuthService = new Mock<IService_VolvoAuthorization>();
        _handler = new CompleteShipmentCommandHandler(_mockVolvoService.Object, _mockAuthService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCompletionSuccessful()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
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

        _mockAuthService
            .Setup(x => x.CanCompleteShipmentAsync(It.IsAny<string>()))
            .ReturnsAsync(new Model_Dao_Result<bool>
            {
                Success = true,
                Data = true
            });

        _mockVolvoService
            .Setup(x => x.CompleteShipmentWithDetailsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<ShipmentLineDto>>()))
            .ReturnsAsync(new Model_Dao_Result
            {
                Success = true
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotAuthorized()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto { PartNumber = "V-EMB-500", ReceivedSkidCount = 5 }
            }
        };

        _mockAuthService
            .Setup(x => x.CanCompleteShipmentAsync(It.IsAny<string>()))
            .ReturnsAsync(new Model_Dao_Result<bool>
            {
                Success = true,
                Data = false
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not authorized");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAuthCheckFails()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto { PartNumber = "V-EMB-500", ReceivedSkidCount = 5 }
            }
        };

        var expectedError = "Authorization service unavailable";

        _mockAuthService
            .Setup(x => x.CanCompleteShipmentAsync(It.IsAny<string>()))
            .ReturnsAsync(new Model_Dao_Result<bool>
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
    public async Task Handle_ShouldReturnFailure_WhenCompletionFails()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto { PartNumber = "V-EMB-500", ReceivedSkidCount = 5 }
            }
        };

        var expectedError = "Database completion failed";

        _mockAuthService
            .Setup(x => x.CanCompleteShipmentAsync(It.IsAny<string>()))
            .ReturnsAsync(new Model_Dao_Result<bool>
            {
                Success = true,
                Data = true
            });

        _mockVolvoService
            .Setup(x => x.CompleteShipmentWithDetailsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<ShipmentLineDto>>()))
            .ReturnsAsync(new Model_Dao_Result
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
    public async Task Handle_ShouldCallAuthorizationCheck()
    {
        // Arrange
        var command = new CompleteShipmentCommand
        {
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            PONumber = "PO-12345",
            ReceiverNumber = "RCV-67890",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto { PartNumber = "V-EMB-500", ReceivedSkidCount = 5 }
            }
        };

        _mockAuthService
            .Setup(x => x.CanCompleteShipmentAsync(It.IsAny<string>()))
            .ReturnsAsync(new Model_Dao_Result<bool>
            {
                Success = true,
                Data = true
            });

        _mockVolvoService
            .Setup(x => x.CompleteShipmentWithDetailsAsync(
                It.IsAny<DateTimeOffset>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<List<ShipmentLineDto>>()))
            .ReturnsAsync(new Model_Dao_Result { Success = true });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockAuthService.Verify(
            x => x.CanCompleteShipmentAsync(It.IsAny<string>()),
            Times.Once);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Models;
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
    private readonly Mock<Dao_VolvoShipment> _mockShipmentDao;
    private readonly Mock<Dao_VolvoShipmentLine> _mockLineDao;
    private readonly Mock<IService_VolvoAuthorization> _mockAuthService;
    private readonly CompleteShipmentCommandHandler _handler;

    public CompleteShipmentCommandHandlerTests()
    {
        _mockShipmentDao = new Mock<Dao_VolvoShipment>("test_connection_string");
        _mockLineDao = new Mock<Dao_VolvoShipmentLine>("test_connection_string");
        _mockAuthService = new Mock<IService_VolvoAuthorization>();
        _handler = new CompleteShipmentCommandHandler(_mockShipmentDao.Object, _mockLineDao.Object, _mockAuthService.Object);
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
            .Setup(x => x.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));

        _mockShipmentDao
            .Setup(x => x.InsertAsync(It.IsAny<Model_VolvoShipment>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success((123, 100)));

        _mockLineDao
            .Setup(x => x.InsertAsync(It.IsAny<Model_VolvoShipmentLine>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(1));

        _mockShipmentDao
            .Setup(x => x.CompleteAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(123);

        _mockShipmentDao.Verify(x => x.InsertAsync(It.Is<Model_VolvoShipment>(s =>
            s.Status == "Completed" &&
            s.PONumber == "PO-12345" &&
            s.ReceiverNumber == "RCV-67890")), Times.Once);
        _mockLineDao.Verify(x => x.InsertAsync(It.IsAny<Model_VolvoShipmentLine>()), Times.Once);
        _mockShipmentDao.Verify(x => x.CompleteAsync(123, "PO-12345", "RCV-67890"), Times.Once);
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
            .Setup(x => x.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<bool>("Not authorized"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not authorized");

        _mockShipmentDao.Verify(x => x.InsertAsync(It.IsAny<Model_VolvoShipment>()), Times.Never);
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
            .Setup(x => x.CanCompleteShipmentsAsync())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain(expectedError);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenShipmentInsertFails()
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
            .Setup(x => x.CanCompleteShipmentsAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));

        _mockShipmentDao
            .Setup(x => x.InsertAsync(It.IsAny<Model_VolvoShipment>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<(int, int)>("Database insert failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Database insert failed");

        _mockLineDao.Verify(x => x.InsertAsync(It.IsAny<Model_VolvoShipmentLine>()), Times.Never);
        _mockShipmentDao.Verify(x => x.CompleteAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Constructor_ShouldThrowArgumentNullException_WhenShipmentDaoIsNull()
    {
        // Act
        Action act = () => new CompleteShipmentCommandHandler(null!, _mockLineDao.Object, _mockAuthService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("shipmentDao");
    }

    [Fact]
    public async Task Constructor_ShouldThrowArgumentNullException_WhenLineDaoIsNull()
    {
        // Act
        Action act = () => new CompleteShipmentCommandHandler(_mockShipmentDao.Object, null!, _mockAuthService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lineDao");
    }

    [Fact]
    public async Task Constructor_ShouldThrowArgumentNullException_WhenAuthServiceIsNull()
    {
        // Act
        Action act = () => new CompleteShipmentCommandHandler(_mockShipmentDao.Object, _mockLineDao.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("authService");
    }
}

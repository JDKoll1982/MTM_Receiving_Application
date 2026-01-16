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
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Commands;

/// <summary>
/// Unit tests for SavePendingShipmentCommandHandler
/// </summary>
public class SavePendingShipmentCommandHandlerTests
{
    private readonly Mock<Dao_VolvoShipment> _mockShipmentDao;
    private readonly Mock<Dao_VolvoShipmentLine> _mockLineDao;
    private readonly SavePendingShipmentCommandHandler _handler;

    public SavePendingShipmentCommandHandlerTests()
    {
        _mockShipmentDao = new Mock<Dao_VolvoShipment>("test_connection_string");
        _mockLineDao = new Mock<Dao_VolvoShipmentLine>("test_connection_string");
        _handler = new SavePendingShipmentCommandHandler(_mockShipmentDao.Object, _mockLineDao.Object);
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

        _mockShipmentDao
            .Setup(x => x.InsertAsync(It.IsAny<Model_VolvoShipment>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success((123, 100)));

        _mockLineDao
            .Setup(x => x.InsertAsync(It.IsAny<Model_VolvoShipmentLine>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(123);

        _mockShipmentDao.Verify(x => x.InsertAsync(It.Is<Model_VolvoShipment>(s =>
            s.Status == "Pending" && s.ShipmentNumber == 100)), Times.Once);
        _mockLineDao.Verify(x => x.InsertAsync(It.IsAny<Model_VolvoShipmentLine>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenShipmentInsertFails()
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

        _mockShipmentDao
            .Setup(x => x.InsertAsync(It.IsAny<Model_VolvoShipment>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<(int, int)>("Database save failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Database save failed");
        _mockLineDao.Verify(x => x.InsertAsync(It.IsAny<Model_VolvoShipmentLine>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateExistingShipment_WhenShipmentIdProvided()
    {
        // Arrange
        var expectedNumber = 42;
        var command = new SavePendingShipmentCommand
        {
            ShipmentId = 123,
            ShipmentDate = DateTimeOffset.Now,
            ShipmentNumber = 100,
            Notes = "Updated notes",
            Parts = new List<ShipmentLineDto>
            {
                new ShipmentLineDto { PartNumber = "V-EMB-500", ReceivedSkidCount = 5 }
            }
        };

        _mockShipmentDao
            .Setup(x => x.UpdateAsync(It.IsAny<Model_VolvoShipment>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(true));

        _mockLineDao
            .Setup(x => x.InsertAsync(It.IsAny<Model_VolvoShipmentLine>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(123);

        _mockShipmentDao.Verify(x => x.UpdateAsync(It.Is<Model_VolvoShipment>(s =>
            s.Id == 123 && s.Status == "Pending")), Times.Once);
        _mockShipmentDao.Verify(x => x.InsertAsync(It.IsAny<Model_VolvoShipment>()), Times.Never);
    }

    [Fact]
    public async Task Constructor_ShouldThrowArgumentNullException_WhenShipmentDaoIsNull()
    {
        // Act
        Action act = () => new SavePendingShipmentCommandHandler(null!, _mockLineDao.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("shipmentDao");
    }

    [Fact]
    public async Task Constructor_ShouldThrowArgumentNullException_WhenLineDaoIsNull()
    {
        // Act
        Action act = () => new SavePendingShipmentCommandHandler(_mockShipmentDao.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("lineDao");
    }
}

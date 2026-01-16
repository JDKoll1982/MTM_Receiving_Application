using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Unit tests for GetPendingShipmentQueryHandler
/// </summary>
public class GetPendingShipmentQueryHandlerTests
{
    private readonly Mock<Dao_VolvoShipment> _mockShipmentDao;
    private readonly GetPendingShipmentQueryHandler _handler;

    public GetPendingShipmentQueryHandlerTests()
    {
        _mockShipmentDao = new Mock<Dao_VolvoShipment>("test_connection_string");
        _handler = new GetPendingShipmentQueryHandler(_mockShipmentDao.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPendingShipment_WhenFound()
    {
        // Arrange
        var expectedShipment = new Model_VolvoShipment
        {
            Id = 123,
            ShipmentNumber = 100,
            ShipmentDate = DateTime.Now,
            Status = "Pending"
        };

        _mockShipmentDao
            .Setup(x => x.GetPendingAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(expectedShipment));

        var query = new GetPendingShipmentQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(123);
        result.Data.ShipmentNumber.Should().Be(100);
        result.Data.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNoPendingShipmentExists()
    {
        // Arrange
        _mockShipmentDao
            .Setup(x => x.GetPendingAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success<Model_VolvoShipment>(null));

        var query = new GetPendingShipmentQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDaoFails()
    {
        // Arrange
        var expectedError = "Database error";

        _mockShipmentDao
            .Setup(x => x.GetPendingAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<Model_VolvoShipment>(expectedError));

        var query = new GetPendingShipmentQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain(expectedError);
    }

    [Fact]
    public async Task Handle_ShouldCallDaoOnce()
    {
        // Arrange
        _mockShipmentDao
            .Setup(x => x.GetPendingAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success<Model_VolvoShipment>(null));

        var query = new GetPendingShipmentQuery();

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockShipmentDao.Verify(x => x.GetPendingAsync(), Times.Once);
    }
}

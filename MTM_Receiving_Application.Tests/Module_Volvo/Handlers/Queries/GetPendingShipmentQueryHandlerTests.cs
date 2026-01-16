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
        var userName = "testuser";
        var expectedShipment = new Model_VolvoShipment
        {
            Id = 123,
            ShipmentNumber = 100,
            ShipmentDate = DateTimeOffset.Now,
            Status = "Pending",
            CreatedBy = userName
        };

        _mockShipmentDao
            .Setup(x => x.GetPendingAsync(userName))
            .ReturnsAsync(new Model_Dao_Result<Model_VolvoShipment>
            {
                Success = true,
                Data = expectedShipment
            });

        var query = new GetPendingShipmentQuery { UserName = userName };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(123);
        result.Data.ShipmentNumber.Should().Be(100);
        result.Data.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNoPendingShipmentExists()
    {
        // Arrange
        var userName = "testuser";

        _mockShipmentDao
            .Setup(x => x.GetPendingAsync(userName))
            .ReturnsAsync(new Model_Dao_Result<Model_VolvoShipment>
            {
                Success = true,
                Data = null
            });

        var query = new GetPendingShipmentQuery { UserName = userName };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDaoFails()
    {
        // Arrange
        var userName = "testuser";
        var expectedError = "Database error";

        _mockShipmentDao
            .Setup(x => x.GetPendingAsync(userName))
            .ReturnsAsync(new Model_Dao_Result<Model_VolvoShipment>
            {
                Success = false,
                ErrorMessage = expectedError
            });

        var query = new GetPendingShipmentQuery { UserName = userName };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain(expectedError);
    }

    [Fact]
    public async Task Handle_ShouldCallDaoOnce()
    {
        // Arrange
        var userName = "testuser";

        _mockShipmentDao
            .Setup(x => x.GetPendingAsync(userName))
            .ReturnsAsync(new Model_Dao_Result<Model_VolvoShipment>
            {
                Success = true,
                Data = null
            });

        var query = new GetPendingShipmentQuery { UserName = userName };

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockShipmentDao.Verify(x => x.GetPendingAsync(userName), Times.Once);
    }
}

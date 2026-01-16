using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Unit tests for GetInitialShipmentDataQueryHandler
/// </summary>
public class GetInitialShipmentDataQueryHandlerTests
{
    private readonly Mock<Dao_VolvoShipment> _mockShipmentDao;
    private readonly GetInitialShipmentDataQueryHandler _handler;

    public GetInitialShipmentDataQueryHandlerTests()
    {
        _mockShipmentDao = new Mock<Dao_VolvoShipment>("test_connection_string");
        _handler = new GetInitialShipmentDataQueryHandler(_mockShipmentDao.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenNextShipmentNumberRetrievedSuccessfully()
    {
        // Arrange
        var query = new GetInitialShipmentDataQuery();
        var expectedNextNumber = 42;

        _mockShipmentDao
            .Setup(x => x.GetNextShipmentNumberAsync())
            .ReturnsAsync(new Model_Dao_Result<int>
            {
                Success = true,
                Data = expectedNextNumber
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.NextShipmentNumber.Should().Be(expectedNextNumber);
        result.Data.CurrentDate.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDaoFails()
    {
        // Arrange
        var query = new GetInitialShipmentDataQuery();
        var expectedError = "Database connection failed";

        _mockShipmentDao
            .Setup(x => x.GetNextShipmentNumberAsync())
            .ReturnsAsync(new Model_Dao_Result<int>
            {
                Success = false,
                ErrorMessage = expectedError
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain(expectedError);
    }

    [Fact]
    public async Task Handle_ShouldCallGetNextShipmentNumberOnce()
    {
        // Arrange
        var query = new GetInitialShipmentDataQuery();

        _mockShipmentDao
            .Setup(x => x.GetNextShipmentNumberAsync())
            .ReturnsAsync(new Model_Dao_Result<int>
            {
                Success = true,
                Data = 1
            });

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockShipmentDao.Verify(x => x.GetNextShipmentNumberAsync(), Times.Once);
    }
}

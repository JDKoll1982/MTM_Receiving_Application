using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Handlers.Queries;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Queries;

/// <summary>
/// Unit tests for GenerateLabelCsvQueryHandler
/// </summary>
public class GenerateLabelCsvQueryHandlerTests
{
    private readonly Mock<IService_Volvo> _mockVolvoService;
    private readonly GenerateLabelCsvQueryHandler _handler;

    public GenerateLabelCsvQueryHandlerTests()
    {
        _mockVolvoService = new Mock<IService_Volvo>();
        _handler = new GenerateLabelCsvQueryHandler(_mockVolvoService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCsvFilePath_WhenSuccessful()
    {
        // Arrange
        var shipmentId = 123;
        var expectedFilePath = "C:\\AppData\\MTM\\Volvo\\Labels\\Volvo_Labels.csv";

        _mockVolvoService
            .Setup(x => x.GenerateLabelCsvAsync(shipmentId))
            .ReturnsAsync(new Model_Dao_Result<string>
            {
                Success = true,
                Data = expectedFilePath
            });

        var query = new GenerateLabelCsvQuery
        {
            ShipmentId = shipmentId
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedFilePath);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenServiceFails()
    {
        // Arrange
        var shipmentId = 123;
        var expectedError = "Failed to generate CSV";

        _mockVolvoService
            .Setup(x => x.GenerateLabelCsvAsync(shipmentId))
            .ReturnsAsync(new Model_Dao_Result<string>
            {
                Success = false,
                ErrorMessage = expectedError
            });

        var query = new GenerateLabelCsvQuery
        {
            ShipmentId = shipmentId
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain(expectedError);
    }

    [Fact]
    public async Task Handle_ShouldDelegateToVolvoService()
    {
        // Arrange
        var shipmentId = 456;

        _mockVolvoService
            .Setup(x => x.GenerateLabelCsvAsync(shipmentId))
            .ReturnsAsync(new Model_Dao_Result<string>
            {
                Success = true,
                Data = "test.csv"
            });

        var query = new GenerateLabelCsvQuery
        {
            ShipmentId = shipmentId
        };

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockVolvoService.Verify(x => x.GenerateLabelCsvAsync(shipmentId), Times.Once);
    }
}

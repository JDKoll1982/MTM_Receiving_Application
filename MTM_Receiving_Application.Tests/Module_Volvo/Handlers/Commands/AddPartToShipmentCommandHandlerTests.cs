using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Handlers.Commands;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using Xunit;

namespace MTM_Receiving_Application.Tests.Module_Volvo.Handlers.Commands;

/// <summary>
/// Unit tests for AddPartToShipmentCommandHandler
/// </summary>
public class AddPartToShipmentCommandHandlerTests
{
    private readonly Mock<Dao_VolvoPart> _mockPartDao;
    private readonly AddPartToShipmentCommandHandler _handler;

    public AddPartToShipmentCommandHandlerTests()
    {
        _mockPartDao = new Mock<Dao_VolvoPart>("test_connection_string");
        _handler = new AddPartToShipmentCommandHandler(_mockPartDao.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPartExists()
    {
        // Arrange
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "V-EMB-500",
            ReceivedSkidCount = 5,
            HasDiscrepancy = false
        };

        _mockPartDao
            .Setup(x => x.GetByIdAsync(command.PartNumber))
            .ReturnsAsync(new Model_Dao_Result<Model_VolvoPart>
            {
                Success = true,
                Data = new Model_VolvoPart
                {
                    PartNumber = command.PartNumber,
                    QuantityPerSkid = 10,
                    IsActive = true
                }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPartDoesNotExist()
    {
        // Arrange
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "INVALID-PART",
            ReceivedSkidCount = 5,
            HasDiscrepancy = false
        };

        _mockPartDao
            .Setup(x => x.GetByIdAsync(command.PartNumber))
            .ReturnsAsync(new Model_Dao_Result<Model_VolvoPart>
            {
                Success = false,
                ErrorMessage = "Part not found",
                Severity = Enum_ErrorSeverity.Error
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ShouldCallDaoOnce()
    {
        // Arrange
        var command = new AddPartToShipmentCommand
        {
            PartNumber = "V-EMB-500",
            ReceivedSkidCount = 5,
            HasDiscrepancy = false
        };

        _mockPartDao
            .Setup(x => x.GetByIdAsync(command.PartNumber))
            .ReturnsAsync(new Model_Dao_Result<Model_VolvoPart>
            {
                Success = true,
                Data = new Model_VolvoPart()
            });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPartDao.Verify(x => x.GetByIdAsync(command.PartNumber), Times.Once);
    }
}

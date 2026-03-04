using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.Services;

public class Service_Tool_OutsideServiceHistory_Tests
{
    private readonly Mock<IService_InforVisual> _mockInforVisual;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly Service_Tool_OutsideServiceHistory _service;

    public Service_Tool_OutsideServiceHistory_Tests()
    {
        _mockInforVisual = new Mock<IService_InforVisual>();
        _mockLogger = new Mock<IService_LoggingUtility>();

        _service = new Service_Tool_OutsideServiceHistory(
            _mockInforVisual.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_NullInforVisual_ThrowsArgumentNullException()
    {
        var act = () => new Service_Tool_OutsideServiceHistory(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("inforVisual");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new Service_Tool_OutsideServiceHistory(_mockInforVisual.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task GetHistoryByPartAsync_DelegatesToInforVisual()
    {
        var expected = Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>
        {
            new() { PartNumber = "PART-001", VendorID = "V-1", DispatchID = "SD-1001" }
        });

        _mockInforVisual
            .Setup(s => s.GetOutsideServiceHistoryByPartAsync("PART-001"))
            .ReturnsAsync(expected);

        var result = await _service.GetHistoryByPartAsync("PART-001");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data![0].PartNumber.Should().Be("PART-001");

        _mockInforVisual.Verify(s => s.GetOutsideServiceHistoryByPartAsync("PART-001"), Times.Once);
    }

    [Fact]
    public async Task GetHistoryByPartAsync_WhenInforVisualReturnsFailure_PropagatesFailure()
    {
        var expected = Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>("SQL Server unavailable");

        _mockInforVisual
            .Setup(s => s.GetOutsideServiceHistoryByPartAsync(It.IsAny<string>()))
            .ReturnsAsync(expected);

        var result = await _service.GetHistoryByPartAsync("PART-999");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("SQL Server unavailable");
    }

    [Fact]
    public async Task GetHistoryByPartAsync_WhenResultIsEmpty_ReturnsEmptyList()
    {
        var expected = Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>());

        _mockInforVisual
            .Setup(s => s.GetOutsideServiceHistoryByPartAsync("PART-NONE"))
            .ReturnsAsync(expected);

        var result = await _service.GetHistoryByPartAsync("PART-NONE");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistoryByPartAsync_LogsPartNumber()
    {
        _mockInforVisual
            .Setup(s => s.GetOutsideServiceHistoryByPartAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>()));

        await _service.GetHistoryByPartAsync("LOG-PART");

        _mockLogger.Verify(l => l.LogInfo(It.Is<string>(msg => msg.Contains("LOG-PART"))), Times.Once);
    }
}

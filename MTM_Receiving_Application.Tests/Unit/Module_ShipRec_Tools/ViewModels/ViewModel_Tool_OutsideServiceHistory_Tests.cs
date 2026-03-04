using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public class ViewModel_Tool_OutsideServiceHistory_Tests
{
    private readonly Mock<IService_Tool_OutsideServiceHistory> _mockService;
    private readonly Mock<IService_ErrorHandler> _mockErrorHandler;
    private readonly Mock<IService_LoggingUtility> _mockLogger;
    private readonly Mock<IService_Notification> _mockNotification;
    private readonly ViewModel_Tool_OutsideServiceHistory _viewModel;

    public ViewModel_Tool_OutsideServiceHistory_Tests()
    {
        _mockService = new Mock<IService_Tool_OutsideServiceHistory>();
        _mockErrorHandler = new Mock<IService_ErrorHandler>();
        _mockLogger = new Mock<IService_LoggingUtility>();
        _mockNotification = new Mock<IService_Notification>();

        _viewModel = new ViewModel_Tool_OutsideServiceHistory(
            _mockService.Object,
            _mockErrorHandler.Object,
            _mockLogger.Object,
            _mockNotification.Object);
    }

    // ─── Initial state ───────────────────────────────────────────────────────

    [Fact]
    public void InitialState_PartNumberIsEmpty()
    {
        _viewModel.PartNumber.Should().BeEmpty();
    }

    [Fact]
    public void InitialState_ResultsIsEmpty()
    {
        _viewModel.Results.Should().BeEmpty();
    }

    [Fact]
    public void InitialState_IsBusyIsFalse()
    {
        _viewModel.IsBusy.Should().BeFalse();
    }

    // ─── SearchCommand: validation ────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchCommand_EmptyOrWhitespacePartNumber_ShowsUserError(string input)
    {
        _viewModel.PartNumber = input;

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _mockErrorHandler.Verify(
            e => e.ShowUserErrorAsync(It.IsAny<string>(), "Input Required", It.IsAny<string>()),
            Times.Once);
        _mockService.Verify(s => s.GetHistoryByPartAsync(It.IsAny<string>()), Times.Never);
    }

    // ─── SearchCommand: success path ──────────────────────────────────────────

    [Fact]
    public async Task SearchCommand_ValidPartWithResults_PopulatesResults()
    {
        _viewModel.PartNumber = "PART-001";

        var data = new List<Model_OutsideServiceHistory>
        {
            new() { PartNumber = "PART-001", VendorID = "V-1", DispatchID = "SD-1001" },
            new() { PartNumber = "PART-001", VendorID = "V-2", DispatchID = "SD-1002" }
        };

        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-001"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(data));

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().HaveCount(2);
        _viewModel.Results[0].VendorID.Should().Be("V-1");
    }

    [Fact]
    public async Task SearchCommand_ValidPartWithResults_StatusContainsCount()
    {
        _viewModel.PartNumber = "PART-002";

        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-002"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>
            {
                new() { PartNumber = "PART-002" }
            }));

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.StatusMessage.Should().Contain("1");
    }

    [Fact]
    public async Task SearchCommand_ValidPartWithNoResults_ReturnsEmptyResults()
    {
        _viewModel.PartNumber = "PART-NONE";

        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-NONE"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>()));

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().BeEmpty();
        _viewModel.StatusMessage.Should().Contain("No outside service history");
    }

    [Fact]
    public async Task SearchCommand_IsBusyIsFalseAfterCompletion()
    {
        _viewModel.PartNumber = "PART-001";

        _mockService
            .Setup(s => s.GetHistoryByPartAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>()));

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.IsBusy.Should().BeFalse();
    }

    // ─── SearchCommand: failure path ──────────────────────────────────────────

    [Fact]
    public async Task SearchCommand_ServiceReturnsFailure_ResultsRemainsEmpty()
    {
        _viewModel.PartNumber = "PART-ERR";

        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-ERR"))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>("Database error"));

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().BeEmpty();
        _viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task SearchCommand_ServiceReturnsFailure_StatusSeverityIsWarning()
    {
        _viewModel.PartNumber = "PART-ERR";

        _mockService
            .Setup(s => s.GetHistoryByPartAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>("Database error"));

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.StatusSeverity.Should().Be(InfoBarSeverity.Warning);
    }

    [Fact]
    public async Task SearchCommand_SecondSearch_ClearsOldResults()
    {
        _viewModel.PartNumber = "PART-001";

        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-001"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>
            {
                new() { PartNumber = "PART-001" },
                new() { PartNumber = "PART-001" }
            }));

        await _viewModel.SearchCommand.ExecuteAsync(null);
        _viewModel.Results.Should().HaveCount(2);

        _viewModel.PartNumber = "PART-002";

        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-002"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>
            {
                new() { PartNumber = "PART-002" }
            }));

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().HaveCount(1);
        _viewModel.Results[0].PartNumber.Should().Be("PART-002");
    }

    // ─── ClearCommand ─────────────────────────────────────────────────────────

    [Fact]
    public void ClearCommand_ResetsPartNumber()
    {
        _viewModel.PartNumber = "SOME-PART";

        _viewModel.ClearCommand.Execute(null);

        _viewModel.PartNumber.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearCommand_ClearsResults()
    {
        _viewModel.PartNumber = "PART-001";

        _mockService
            .Setup(s => s.GetHistoryByPartAsync(It.IsAny<string>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>
            {
                new() { PartNumber = "PART-001" }
            }));

        await _viewModel.SearchCommand.ExecuteAsync(null);
        _viewModel.Results.Should().HaveCount(1);

        _viewModel.ClearCommand.Execute(null);

        _viewModel.Results.Should().BeEmpty();
    }
}

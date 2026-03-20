using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
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
            _mockNotification.Object
        );
    }

    // ─── Initial state ───────────────────────────────────────────────────────

    [Fact]
    public void InitialState_SearchTermIsEmpty()
    {
        _viewModel.SearchTerm.Should().BeEmpty();
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

    [Fact]
    public void InitialState_IsSearchByPartMode()
    {
        _viewModel.IsSearchByPart.Should().BeTrue();
        _viewModel.IsSearchByVendor.Should().BeFalse();
    }

    // ─── Mode toggle ─────────────────────────────────────────────────────────

    [Fact]
    public void SetSearchByVendorCommand_SwitchesToVendorMode()
    {
        _viewModel.SetSearchByVendorCommand.Execute(null);

        _viewModel.IsSearchByVendor.Should().BeTrue();
        _viewModel.IsSearchByPart.Should().BeFalse();
    }

    [Fact]
    public void SetSearchByPartCommand_SwitchesBackToPartMode()
    {
        _viewModel.SetSearchByVendorCommand.Execute(null);
        _viewModel.SetSearchByPartCommand.Execute(null);

        _viewModel.IsSearchByPart.Should().BeTrue();
    }

    [Fact]
    public void SetSearchByVendorCommand_ClearsSearchTerm()
    {
        _viewModel.SearchTerm = "something";

        _viewModel.SetSearchByVendorCommand.Execute(null);

        _viewModel.SearchTerm.Should().BeEmpty();
    }

    // ─── SearchCommand: validation ────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchCommand_EmptyOrWhitespaceSearchTerm_ShowsUserError(string input)
    {
        _viewModel.SearchTerm = input;

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _mockErrorHandler.Verify(
            e => e.ShowUserErrorAsync(It.IsAny<string>(), "Input Required", It.IsAny<string>()),
            Times.Once
        );
        _mockService.Verify(s => s.FuzzySearchPartsAsync(It.IsAny<string>()), Times.Never);
    }

    // ─── SearchCommand: fuzzy → single match → history ───────────────────────

    [Fact]
    public async Task SearchCommand_SingleFuzzyMatch_PopulatesResultsFromHistory()
    {
        _viewModel.SearchTerm = "PART-001";

        _mockService
            .Setup(s => s.FuzzySearchPartsAsync("PART-001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new()
                        {
                            Key = "PART-001",
                            Label = "PART-001",
                            Detail = "Mock part",
                        },
                    }
                )
            );

        var data = new List<Model_OutsideServiceHistory>
        {
            new()
            {
                PartNumber = "PART-001",
                VendorID = "V-1",
                DispatchID = "SD-1001",
            },
            new()
            {
                PartNumber = "PART-001",
                VendorID = "V-2",
                DispatchID = "SD-1002",
            },
        };
        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-001"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(data));

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().HaveCount(2);
        _viewModel.Results[0].VendorID.Should().Be("V-1");
    }

    [Fact]
    public async Task SearchCommand_SingleFuzzyMatch_StatusContainsCount()
    {
        _viewModel.SearchTerm = "PART-002";

        _mockService
            .Setup(s => s.FuzzySearchPartsAsync("PART-002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-002", Label = "PART-002" },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceHistory> { new() { PartNumber = "PART-002" } }
                )
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.StatusMessage.Should().Contain("1");
    }

    [Fact]
    public async Task SearchCommand_NoFuzzyMatches_ResultsRemainsEmptyWithWarning()
    {
        _viewModel.SearchTerm = "PART-NONE";

        _mockService
            .Setup(s => s.FuzzySearchPartsAsync("PART-NONE"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>()));

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().BeEmpty();
        _viewModel.StatusSeverity.Should().Be(InfoBarSeverity.Warning);
    }

    [Fact]
    public async Task SearchCommand_IsBusyIsFalseAfterCompletion()
    {
        _viewModel.SearchTerm = "PART-001";

        _mockService
            .Setup(s => s.FuzzySearchPartsAsync(It.IsAny<string>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-001", Label = "PART-001" },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetHistoryByPartAsync(It.IsAny<string>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>())
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.IsBusy.Should().BeFalse();
    }

    // ─── SearchCommand: multiple fuzzy matches → picker fallback ─────────────

    [Fact]
    public async Task SearchCommand_MultipleFuzzyMatches_WithNoPickerDelegate_UsesFirstCandidate()
    {
        _viewModel.SearchTerm = "PART";

        _mockService
            .Setup(s => s.FuzzySearchPartsAsync("PART"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-001", Label = "PART-001" },
                        new() { Key = "PART-002", Label = "PART-002" },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceHistory> { new() { PartNumber = "PART-001" } }
                )
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().HaveCount(1);
        _viewModel.Results[0].PartNumber.Should().Be("PART-001");
    }

    // ─── SearchCommand: failure path ──────────────────────────────────────────

    [Fact]
    public async Task SearchCommand_FuzzySearchFailure_ResultsRemainsEmpty()
    {
        _viewModel.SearchTerm = "PART-ERR";

        _mockService
            .Setup(s => s.FuzzySearchPartsAsync("PART-ERR"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>("Database error")
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().BeEmpty();
        _viewModel.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task SearchCommand_HistoryFailure_StatusSeverityIsWarning()
    {
        _viewModel.SearchTerm = "PART-ERR";

        _mockService
            .Setup(s => s.FuzzySearchPartsAsync("PART-ERR"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-ERR", Label = "PART-ERR" },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-ERR"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_OutsideServiceHistory>>(
                    "Database error"
                )
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.StatusSeverity.Should().Be(InfoBarSeverity.Warning);
    }

    [Fact]
    public async Task SearchCommand_SecondSearch_ClearsOldResults()
    {
        _viewModel.SearchTerm = "PART-001";

        _mockService
            .Setup(s => s.FuzzySearchPartsAsync("PART-001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-001", Label = "PART-001" },
                    }
                )
            );
        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceHistory>
                    {
                        new() { PartNumber = "PART-001" },
                        new() { PartNumber = "PART-001" },
                    }
                )
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);
        _viewModel.Results.Should().HaveCount(2);

        _viewModel.SearchTerm = "PART-002";

        _mockService
            .Setup(s => s.FuzzySearchPartsAsync("PART-002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-002", Label = "PART-002" },
                    }
                )
            );
        _mockService
            .Setup(s => s.GetHistoryByPartAsync("PART-002"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceHistory> { new() { PartNumber = "PART-002" } }
                )
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().HaveCount(1);
        _viewModel.Results[0].PartNumber.Should().Be("PART-002");
    }

    // ─── Vendor mode: fuzzy → parts picker → dispatch history ────────────────

    [Fact]
    public async Task SearchCommand_VendorMode_SingleVendor_SinglePart_LoadsHistory()
    {
        _viewModel.SetSearchByVendorCommand.Execute(null);
        _viewModel.SearchTerm = "Acme";

        _mockService
            .Setup(s => s.FuzzySearchVendorsAsync("Acme"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "V-001", Label = "Acme Heat Treating Co." },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetPartsByVendorAsync("V-001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new()
                        {
                            Key = "PART-001",
                            Label = "PART-001",
                            Detail = "2 dispatch(es)",
                        },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetHistoryByVendorAndPartAsync("V-001", "PART-001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceHistory>
                    {
                        new()
                        {
                            VendorID = "V-001",
                            PartNumber = "PART-001",
                            DispatchID = "SD-100",
                        },
                        new()
                        {
                            VendorID = "V-001",
                            PartNumber = "PART-001",
                            DispatchID = "SD-101",
                        },
                    }
                )
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().HaveCount(2);
        _viewModel.Results[0].DispatchID.Should().Be("SD-100");
    }

    [Fact]
    public async Task SearchCommand_VendorMode_MultiplePartsNoPickerDelegate_UsesFirstPart()
    {
        _viewModel.SetSearchByVendorCommand.Execute(null);
        _viewModel.SearchTerm = "Acme";

        _mockService
            .Setup(s => s.FuzzySearchVendorsAsync("Acme"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "V-001", Label = "Acme Heat Treating Co." },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetPartsByVendorAsync("V-001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-A", Label = "PART-A" },
                        new() { Key = "PART-B", Label = "PART-B" },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetHistoryByVendorAndPartAsync("V-001", "PART-A"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceHistory>
                    {
                        new() { VendorID = "V-001", PartNumber = "PART-A" },
                    }
                )
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _mockService.Verify(s => s.GetHistoryByVendorAndPartAsync("V-001", "PART-A"), Times.Once);
        _viewModel.Results.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchCommand_VendorMode_NoParts_ShowsWarningAndNoHistory()
    {
        _viewModel.SetSearchByVendorCommand.Execute(null);
        _viewModel.SearchTerm = "Acme";

        _mockService
            .Setup(s => s.FuzzySearchVendorsAsync("Acme"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "V-001", Label = "Acme Heat Treating Co." },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetPartsByVendorAsync("V-001"))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<Model_FuzzySearchResult>()));

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().BeEmpty();
        _viewModel.StatusSeverity.Should().Be(InfoBarSeverity.Warning);
        _mockService.Verify(
            s => s.GetHistoryByVendorAndPartAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task SearchCommand_VendorMode_PartsQueryFails_ShowsWarning()
    {
        _viewModel.SetSearchByVendorCommand.Execute(null);
        _viewModel.SearchTerm = "Acme";

        _mockService
            .Setup(s => s.FuzzySearchVendorsAsync("Acme"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "V-001", Label = "Acme Heat Treating Co." },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetPartsByVendorAsync("V-001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_FuzzySearchResult>>("DB error")
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _viewModel.Results.Should().BeEmpty();
        _viewModel.StatusSeverity.Should().Be(InfoBarSeverity.Warning);
    }

    [Fact]
    public async Task SearchCommand_VendorMode_DoesNotCallGetHistoryByVendorAsync()
    {
        _viewModel.SetSearchByVendorCommand.Execute(null);
        _viewModel.SearchTerm = "Acme";

        _mockService
            .Setup(s => s.FuzzySearchVendorsAsync("Acme"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "V-001", Label = "Acme" },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetPartsByVendorAsync("V-001"))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-A", Label = "PART-A" },
                    }
                )
            );

        _mockService
            .Setup(s => s.GetHistoryByVendorAndPartAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(new List<Model_OutsideServiceHistory>())
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);

        _mockService.Verify(s => s.GetHistoryByVendorAsync(It.IsAny<string>()), Times.Never);
    }

    // ─── ClearCommand ─────────────────────────────────────────────────────────

    [Fact]
    public void ClearCommand_ResetsSearchTerm()
    {
        _viewModel.SearchTerm = "SOME-PART";

        _viewModel.ClearCommand.Execute(null);

        _viewModel.SearchTerm.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearCommand_ClearsResults()
    {
        _viewModel.SearchTerm = "PART-001";

        _mockService
            .Setup(s => s.FuzzySearchPartsAsync(It.IsAny<string>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_FuzzySearchResult>
                    {
                        new() { Key = "PART-001", Label = "PART-001" },
                    }
                )
            );
        _mockService
            .Setup(s => s.GetHistoryByPartAsync(It.IsAny<string>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_OutsideServiceHistory> { new() { PartNumber = "PART-001" } }
                )
            );

        await _viewModel.SearchCommand.ExecuteAsync(null);
        _viewModel.Results.Should().HaveCount(1);

        _viewModel.ClearCommand.Execute(null);

        _viewModel.Results.Should().BeEmpty();
    }
}

using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_ShipRec_Tools.ViewModels;

public sealed class ViewModel_Tool_DeliveryScheduleTests
{
    private static ViewModel_Tool_DeliverySchedule CreateViewModel(
        Mock<IService_Tool_DeliverySchedule> serviceMock
    ) =>
        new(
            serviceMock.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

    [Fact]
    public async Task SearchAsync_ShouldPopulateRows_OnSuccess()
    {
        var serviceMock = new Mock<IService_Tool_DeliverySchedule>();
        serviceMock
            .Setup(service => service.SearchAsync(It.IsAny<Model_InforVisualDeliveryScheduleFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_DeliveryScheduleLine>
                    {
                        new() { PoNumber = "PO-064008", VendorName = "Basic Metals" },
                    }
                )
            );
        var viewModel = CreateViewModel(serviceMock);

        await viewModel.SearchCommand.ExecuteAsync(null);

        viewModel.Rows.Should().ContainSingle().Which.PoNumber.Should().Be("PO-064008");
        viewModel.IsEmpty.Should().BeFalse();
        viewModel.ResultCountText.Should().Be("Found 1 receiving line.");
    }

    [Fact]
    public async Task SearchAsync_ShouldMapQuickSearchToAllSearchTerms()
    {
        var serviceMock = new Mock<IService_Tool_DeliverySchedule>();
        Model_InforVisualDeliveryScheduleFilter? captured = null;
        serviceMock
            .Setup(service => service.SearchAsync(It.IsAny<Model_InforVisualDeliveryScheduleFilter>()))
            .Callback<Model_InforVisualDeliveryScheduleFilter>(f => captured = f)
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_Tool_DeliveryScheduleLine>>([])
            );
        var viewModel = CreateViewModel(serviceMock);
        viewModel.QuickSearch = "MMC";

        await viewModel.SearchCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured!.PartSearch.Should().Be("MMC");
        captured.PoSearch.Should().Be("MMC");
        captured.SupplierSearch.Should().Be("MMC");
        captured.CarrierSearch.Should().Be("MMC");
        captured.SearchAll.Should().BeTrue();
        captured.ScopeUninventoried.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_ShouldMapScopeFlags_ToFilter()
    {
        var serviceMock = new Mock<IService_Tool_DeliverySchedule>();
        Model_InforVisualDeliveryScheduleFilter? captured = null;
        serviceMock
            .Setup(service => service.SearchAsync(It.IsAny<Model_InforVisualDeliveryScheduleFilter>()))
            .Callback<Model_InforVisualDeliveryScheduleFilter>(f => captured = f)
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_Tool_DeliveryScheduleLine>>([])
            );
        var viewModel = CreateViewModel(serviceMock);
        viewModel.ScopeParts = false;
        viewModel.ScopeCoils = true;
        viewModel.ScopeFlat = false;
        viewModel.ScopeOutside = false;
        viewModel.ScopeUninventoried = false;

        await viewModel.SearchCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured!.ScopeParts.Should().BeFalse();
        captured.ScopeCoils.Should().BeTrue();
        captured.ScopeFlat.Should().BeFalse();
        captured.ScopeOutside.Should().BeFalse();
        captured.ScopeUninventoried.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_ShouldMapNearFilledFilter()
    {
        var serviceMock = new Mock<IService_Tool_DeliverySchedule>();
        Model_InforVisualDeliveryScheduleFilter? captured = null;
        serviceMock
            .Setup(service => service.SearchAsync(It.IsAny<Model_InforVisualDeliveryScheduleFilter>()))
            .Callback<Model_InforVisualDeliveryScheduleFilter>(f => captured = f)
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_Tool_DeliveryScheduleLine>>([])
            );
        var viewModel = CreateViewModel(serviceMock);
        viewModel.ShowNearFilled = true;
        viewModel.NearFillPctText = "85";

        await viewModel.SearchCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured!.ShowNearFilled.Should().BeTrue();
        captured.NearFillPct.Should().Be(85);
    }

    [Fact]
    public void NearFillPctValue_ShouldClampToValidRange()
    {
        var serviceMock = new Mock<IService_Tool_DeliverySchedule>();
        var viewModel = CreateViewModel(serviceMock);

        viewModel.NearFillPctValue.Should().Be(90);

        viewModel.NearFillPctText = "0";
        viewModel.NearFillPctValue.Should().Be(1);

        viewModel.NearFillPctText = "150";
        viewModel.NearFillPctValue.Should().Be(99);

        viewModel.NearFillPctText = "abc";
        viewModel.NearFillPctValue.Should().Be(90);
    }

    [Fact]
    public async Task SearchAsync_ShouldUseIndividualSearchTerms_WhenQuickSearchEmpty()
    {
        var serviceMock = new Mock<IService_Tool_DeliverySchedule>();
        Model_InforVisualDeliveryScheduleFilter? captured = null;
        serviceMock
            .Setup(service => service.SearchAsync(It.IsAny<Model_InforVisualDeliveryScheduleFilter>()))
            .Callback<Model_InforVisualDeliveryScheduleFilter>(f => captured = f)
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success<List<Model_Tool_DeliveryScheduleLine>>([])
            );
        var viewModel = CreateViewModel(serviceMock);
        viewModel.PartSearch = "MMC0000";
        viewModel.PoSearch = "PO-07";
        viewModel.SearchAll = false;

        await viewModel.SearchCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured!.PartSearch.Should().Be("MMC0000");
        captured.PoSearch.Should().Be("PO-07");
        captured.SupplierSearch.Should().BeEmpty();
        captured.CarrierSearch.Should().BeEmpty();
        captured.SearchAll.Should().BeFalse();
    }

    [Fact]
    public async Task SearchAsync_ShouldClearRows_OnFailure()
    {
        var serviceMock = new Mock<IService_Tool_DeliverySchedule>();
        serviceMock
            .Setup(service => service.SearchAsync(It.IsAny<Model_InforVisualDeliveryScheduleFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_Tool_DeliveryScheduleLine>>("boom")
            );
        var viewModel = CreateViewModel(serviceMock);

        await viewModel.SearchCommand.ExecuteAsync(null);

        viewModel.Rows.Should().BeEmpty();
        viewModel.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task ExportAsync_ShouldBuildHtmlTable_WhenRowsExist()
    {
        var serviceMock = new Mock<IService_Tool_DeliverySchedule>();
        serviceMock
            .Setup(service => service.SearchAsync(It.IsAny<Model_InforVisualDeliveryScheduleFilter>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Success(
                    new List<Model_Tool_DeliveryScheduleLine>
                    {
                        new() { PoNumber = "PO-064008", VendorName = "Basic Metals", PartNumber = "MMC0000367" },
                    }
                )
            );
        var viewModel = CreateViewModel(serviceMock);
        await viewModel.SearchCommand.ExecuteAsync(null);

        Model_FormattedReportDocument? captured = null;
        viewModel.RequestExportAsync = doc =>
        {
            captured = doc;
            return Task.FromResult(Model_Dao_Result_Factory.Success(true));
        };

        await viewModel.ExportCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured!.HtmlFragment.Should().Contain("<table>");
        captured.HtmlFragment.Should().Contain("PO-064008");
        captured.HtmlFragment.Should().Contain("MMC0000367");
    }

    [Fact]
    public async Task ExportAsync_ShouldNotOpen_WhenNoRows()
    {
        var viewModel = CreateViewModel(new Mock<IService_Tool_DeliverySchedule>());
        var opened = false;
        viewModel.RequestExportAsync = _ =>
        {
            opened = true;
            return Task.FromResult(Model_Dao_Result_Factory.Success(true));
        };

        await viewModel.ExportCommand.ExecuteAsync(null);

        opened.Should().BeFalse();
    }
}
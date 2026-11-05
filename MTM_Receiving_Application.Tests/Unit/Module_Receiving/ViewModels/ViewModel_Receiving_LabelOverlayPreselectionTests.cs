using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Enums;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

/// <summary>
/// Covers the overlay opening on the labels the receiving results page just saved. A wrong match here
/// prints a symbol onto a tag nobody asked about, so the matching is by part and heat/lot together.
/// </summary>
public sealed class ViewModel_Receiving_LabelOverlayPreselectionTests
{
    private static Model_ReceivingLabelOverlayItem Item(
        string partId,
        string heatLot,
        int labelNumber
    ) =>
        new()
        {
            PartId = partId,
            HeatLotNumber = heatLot,
            LabelNumber = labelNumber,
            ShapeCode = "⭐",
            ColorCode = "BLUE",
            MarkingText = "Blue Star",
        };

    private static ViewModel_Receiving_LabelOverlay CreateViewModel(
        IReadOnlyList<Model_ReceivingLabelOverlayItem> queue
    )
    {
        var overlay = new Mock<IService_ReceivingLabelOverlay>();
        overlay.Setup(service => service.GetOverlayCandidatesAsync()).ReturnsAsync(
            Model_Dao_Result_Factory.Success(queue.ToList())
        );
        overlay
            .Setup(service => service.GetPrinterNamesAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<string>()));
        overlay
            .Setup(service => service.GetPaperSizesAsync(It.IsAny<string?>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_LabelOverlayPaperOption>>("none")
            );
        overlay
            .Setup(service => service.GetPaperSourcesAsync(It.IsAny<string?>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<List<Model_LabelOverlayPaperSourceOption>>("none")
            );

        var settings = new Mock<IService_ReceivingLabelOverlaySettings>();
        settings
            .Setup(service => service.GetPrintProfileAsync(It.IsAny<Enum_LabelOverlayPrintFormat>(), It.IsAny<int?>()))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<Model_LabelOverlayPrintSettings>("not saved")
            );
        settings
            .Setup(service => service.GetListFilterAsync(It.IsAny<Enum_LabelOverlayPrintFormat>(), It.IsAny<int?>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Failure<Model_LabelOverlayListFilter>("not saved"));
        settings
            .Setup(service => service.GetCardSheetPartIdAsync(It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        settings
            .Setup(service => service.SaveCardSheetPartIdAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success());

        return new ViewModel_Receiving_LabelOverlay(
            overlay.Object,
            settings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    [Fact]
    public async Task ActivateAsync_ShouldTickTheRequestedLot_AndNothingElse()
    {
        var wanted = Item("MMC0000848", "344934", 1);
        var otherLot = Item("MMC0000848", "345121", 2);
        var otherPart = Item("OTHER-PART", "344934", 3);

        var viewModel = CreateViewModel([wanted, otherLot, otherPart]);
        viewModel.RequestPreselection([("MMC0000848", "344934")]);

        await viewModel.ActivateAsync();

        wanted.IsSelected.Should().BeTrue();
        otherLot.IsSelected.Should().BeFalse("the lot is part of the match, not just the part number");
        otherPart.IsSelected.Should().BeFalse("the part is part of the match, not just the lot number");
    }

    [Fact]
    public async Task ActivateAsync_ShouldTickEveryLoadOfTheRequestedLot()
    {
        var first = Item("MMC0000848", "344934", 1);
        var second = Item("MMC0000848", "344934", 2);

        var viewModel = CreateViewModel([first, second]);
        viewModel.RequestPreselection([("MMC0000848", "344934")]);

        await viewModel.ActivateAsync();

        first.IsSelected.Should().BeTrue();
        second.IsSelected.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateAsync_ShouldMatchWhateverTheCaseOfTheRequestedValues()
    {
        var item = Item("MMC0000848", "344934", 1);

        var viewModel = CreateViewModel([item]);
        viewModel.RequestPreselection([("mmc0000848", " 344934 ")]);

        await viewModel.ActivateAsync();

        item.IsSelected.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateAsync_ShouldTickNothing_WhenNoRequestWasMade()
    {
        var item = Item("MMC0000848", "344934", 1);

        var viewModel = CreateViewModel([item]);

        await viewModel.ActivateAsync();

        item.IsSelected.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateAsync_ShouldLeaveTheQueueAlone_WhenTheRequestedLotIsNotInIt()
    {
        var item = Item("MMC0000848", "344934", 1);

        var viewModel = CreateViewModel([item]);
        viewModel.RequestPreselection([("MMC0000848", "999999")]);

        await viewModel.ActivateAsync();

        item.IsSelected.Should().BeFalse();
    }
}

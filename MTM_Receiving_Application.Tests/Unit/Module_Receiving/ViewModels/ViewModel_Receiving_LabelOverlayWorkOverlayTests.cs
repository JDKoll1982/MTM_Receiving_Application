using System;
using System.Collections.Generic;
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
/// Covers the overlay that says what the window is doing while it reads the printer's own lists. A scrim
/// left up after a failure would lock the operator out of the window, so every path has to take it down.
/// </summary>
public sealed class ViewModel_Receiving_LabelOverlayWorkOverlayTests
{
    private static ViewModel_Receiving_LabelOverlay CreateViewModel(
        Action<Mock<IService_ReceivingLabelOverlay>>? arrangeOverlay = null
    )
    {
        var overlay = new Mock<IService_ReceivingLabelOverlay>();
        overlay.Setup(service => service.GetOverlayCandidatesAsync()).ReturnsAsync(
            Model_Dao_Result_Factory.Success(new List<Model_ReceivingLabelOverlayItem>())
        );
        overlay
            .Setup(service => service.GetPrinterNamesAsync())
            .ReturnsAsync(Model_Dao_Result_Factory.Success(new List<string> { "TEST-PRINTER" }));
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

        arrangeOverlay?.Invoke(overlay);

        var settings = new Mock<IService_ReceivingLabelOverlaySettings>();
        settings
            .Setup(service =>
                service.GetPrintProfileAsync(It.IsAny<Enum_LabelOverlayPrintFormat>(), It.IsAny<int?>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<Model_LabelOverlayPrintSettings>("not saved")
            );
        settings
            .Setup(service =>
                service.GetListFilterAsync(It.IsAny<Enum_LabelOverlayPrintFormat>(), It.IsAny<int?>())
            )
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<Model_LabelOverlayListFilter>("not saved")
            );
        settings.Setup(service => service.GetCardSheetPartIdAsync(It.IsAny<int?>())).ReturnsAsync(string.Empty);

        return new ViewModel_Receiving_LabelOverlay(
            overlay.Object,
            settings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    [Fact]
    public async Task ActivateAsync_ShouldShowTheWorkOverlayWhileLoading_ThenTakeItDown()
    {
        var viewModel = CreateViewModel();

        var overlayStates = new List<bool>();
        var stepTexts = new List<string?>();
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ViewModel_Receiving_LabelOverlay.IsWorkOverlayVisible))
            {
                overlayStates.Add(viewModel.IsWorkOverlayVisible);
            }

            if (e.PropertyName == nameof(ViewModel_Receiving_LabelOverlay.WorkOverlayStepText))
            {
                stepTexts.Add(viewModel.WorkOverlayStepText);
            }
        };

        await viewModel.ActivateAsync();

        overlayStates.Should().Contain(true, "the window has to explain the wait while it reads the lists");
        overlayStates[^1].Should().BeFalse("the scrim cannot stay up once the load has finished");
        stepTexts.Should().Contain("Step 5 of 5", "the queue is the last of the five reads");
        viewModel.HasWorkOverlayStepText.Should().BeFalse();
        viewModel.IsWorkOverlayIndeterminate.Should().BeTrue();
    }

    [Fact]
    public async Task ReloadAsync_ShouldTakeTheWorkOverlayDown_WhenAReadThrows()
    {
        var viewModel = CreateViewModel(overlay =>
            overlay
                .Setup(service => service.GetPrinterNamesAsync())
                .ThrowsAsync(new InvalidOperationException("the print spooler is not running"))
        );

        await Assert.ThrowsAsync<InvalidOperationException>(viewModel.ReloadAsync);

        viewModel.IsWorkOverlayVisible.Should().BeFalse("a failed read must not leave the window blocked");
    }
}

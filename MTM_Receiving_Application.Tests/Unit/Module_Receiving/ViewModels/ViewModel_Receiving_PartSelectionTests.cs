using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Settings;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

public sealed class ViewModel_Receiving_PartSelectionTests
{
    [Fact]
    public void EmptyStateText_ShouldPointAtThePoStep_WhenNoPoHasBeenEntered()
    {
        var viewModel = CreateViewModel(out var workflow, out _);
        workflow.Object.CurrentPONumber = string.Empty;

        viewModel.ShowEmptyState.Should().BeTrue();
        viewModel
            .EmptyStateText.Should()
            .Be("Load the PO number first, then come back to choose the part.");
    }

    [Fact]
    public void EmptyStateText_ShouldSayThePartsAreNotLoaded_WhenAPoIsKnownButHasNoList()
    {
        var viewModel = CreateViewModel(out _, out _);

        viewModel
            .EmptyStateText.Should()
            .Contain("PO-066868")
            .And.Contain("have not been loaded", "the PO is in, only its part list is missing");
    }

    [Fact]
    public async Task Refresh_ShouldPickUpThePartList_WhenThePoPageWasReplacedAfterThisPageWasBuilt()
    {
        var liveEntries = new List<ViewModel_Receiving_POEntry>();
        var viewModel = CreateViewModel(out var workflow, out _, livePoEntries: liveEntries);
        await viewModel.ViewPreferencesLoaded;
        viewModel.Parts.Should().BeEmpty();

        liveEntries.Add(CreatePoEntry(workflow, [Part("TEST-A")]));
        viewModel.Refresh();

        viewModel.Parts.Select(part => part.PartID).Should().Equal("TEST-A");
    }

    [Fact]
    public void OnSelectedPartChanged_ShouldSetTheWorkflowPart_WhenNoPoPageIsRegistered()
    {
        var viewModel = CreateViewModel(out var workflow, out _);
        var part = new Model_InforVisualPart { PartID = "MMFTEST010" };

        viewModel.SelectedPart = part;

        workflow.Object.CurrentPart.Should().BeSameAs(part);
        viewModel.HasSelectedPart.Should().BeTrue();
    }

    [Fact]
    public void ResetToDefaults_ShouldClearTheSearchAndTheChosenPart()
    {
        var viewModel = CreateViewModel(out _, out _);
        viewModel.SearchText = "MMF";
        viewModel.SelectedPart = new Model_InforVisualPart { PartID = "MMFTEST011" };

        viewModel.ResetToDefaults();

        viewModel.SearchText.Should().BeEmpty();
        viewModel.SelectedPart.Should().BeNull();
        viewModel.HasSelectedPart.Should().BeFalse();
    }

    [Fact]
    public void HasQualityHold_ShouldFollowTheChosenPart()
    {
        var viewModel = CreateViewModel(out _, out _);

        viewModel.SelectedPart = new Model_InforVisualPart
        {
            PartID = "MMFSR0001",
            RequiresQualityHold = true,
            QualityHoldRestrictionType = "Sheet Material - Quality Hold Required",
        };

        viewModel.HasQualityHold.Should().BeTrue();
        viewModel.QualityHoldText.Should().Be("Sheet Material - Quality Hold Required");
    }

    [Fact]
    public void PoContextText_ShouldShowOnlyThePoNumber_WhenAPoIsLoaded()
    {
        var viewModel = CreateViewModel(out _, out _);

        viewModel
            .PoContextText.Should()
            .Be("PO-066868", "the stored number already carries its prefix, so no label belongs in front of it");
    }

    // ── sorting ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Parts_ShouldDefaultToMostRecentlyReceivedFirst()
    {
        var viewModel = CreateViewModel(
            out _,
            out _,
            parts:
            [
                Part("MMFTEST001", new DateTime(2026, 1, 5)),
                Part("MMFTEST002", new DateTime(2026, 3, 9)),
                Part("MMFTEST003", new DateTime(2025, 12, 1)),
            ]
        );
        await viewModel.ViewPreferencesLoaded;

        viewModel.SortColumn.Should().Be(ReceivingPartSortKeys.ReceiveDate);
        viewModel.SortAscending.Should().BeFalse();
        viewModel.Parts.Select(part => part.PartID).Should().Equal("MMFTEST002", "MMFTEST001", "MMFTEST003");
    }

    [Fact]
    public async Task Parts_ShouldKeepNeverReceivedPartsLast_WhenSomeLinesHaveNoReceipt()
    {
        var viewModel = CreateViewModel(
            out _,
            out _,
            parts:
            [
                Part("TEST-UNDATED"),
                Part("MMFTEST001", new DateTime(2026, 1, 5)),
                Part("MMFTEST002", new DateTime(2026, 2, 1)),
            ]
        );
        await viewModel.ViewPreferencesLoaded;

        viewModel
            .Parts.Select(part => part.PartID)
            .Should()
            .Equal("MMFTEST002", "MMFTEST001", "TEST-UNDATED");
    }

    [Fact]
    public async Task SortBy_ShouldReverseTheReceiveDateOrder_WhenTheHeaderIsClickedAgain()
    {
        var viewModel = CreateViewModel(
            out _,
            out _,
            parts:
            [
                Part("MMFTEST001", new DateTime(2026, 1, 5)),
                Part("MMFTEST002", new DateTime(2026, 3, 9)),
            ]
        );
        await viewModel.ViewPreferencesLoaded;

        viewModel.SortBy(ReceivingPartSortKeys.ReceiveDate, ascending: true);

        viewModel.Parts.Select(part => part.PartID).Should().Equal("MMFTEST001", "MMFTEST002");
    }

    [Fact]
    public async Task SortBy_ShouldOrderByPartId_WhenThePartIdHeaderIsClicked()
    {
        var viewModel = CreateViewModel(
            out _,
            out _,
            parts: [Part("TEST-C"), Part("TEST-A"), Part("TEST-B")]
        );
        await viewModel.ViewPreferencesLoaded;

        viewModel.SortBy(ReceivingPartSortKeys.PartId, ascending: true);

        viewModel.SortColumn.Should().Be(ReceivingPartSortKeys.PartId);
        viewModel.Parts.Select(part => part.PartID).Should().Equal("TEST-A", "TEST-B", "TEST-C");
    }

    // ── filters ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task OnHandOnly_ShouldHidePartsWithNoStock_WhenItIsSwitchedOn()
    {
        var viewModel = CreateViewModel(
            out _,
            out _,
            parts: [Part("TEST-STOCKED", onHand: 12.5m), Part("TEST-EMPTY")]
        );
        await viewModel.ViewPreferencesLoaded;

        viewModel.OnHandOnly = true;

        viewModel.Parts.Select(part => part.PartID).Should().Equal("TEST-STOCKED");
        viewModel.HasActiveFilters.Should().BeTrue();
        viewModel.ActiveFilterCount.Should().Be(1);
    }

    [Fact]
    public async Task QualityHoldOnly_ShouldKeepOnlyRestrictedParts()
    {
        var viewModel = CreateViewModel(
            out _,
            out _,
            parts:
            [
                Part("TEST-HOLD", qualityHold: true),
                Part("TEST-FREE"),
            ]
        );
        await viewModel.ViewPreferencesLoaded;

        viewModel.QualityHoldOnly = true;

        viewModel.Parts.Select(part => part.PartID).Should().Equal("TEST-HOLD");
    }

    [Fact]
    public async Task SelectedLocation_ShouldKeepOnlyThatLocation_AndCountAsOneFilter()
    {
        var viewModel = CreateViewModel(
            out _,
            out _,
            parts: [Part("TEST-A", location: "002-A"), Part("TEST-B", location: "002-B")]
        );
        await viewModel.ViewPreferencesLoaded;

        viewModel.SelectedLocation = "002-B";

        viewModel.Parts.Select(part => part.PartID).Should().Equal("TEST-B");
        viewModel.ActiveFilterCount.Should().Be(1);
        viewModel.AvailableLocations.Should().Contain(["All locations", "002-A", "002-B"]);
    }

    [Fact]
    public async Task ResetFilters_ShouldClearEveryFilter_AndKeepTheChosenSort()
    {
        var viewModel = CreateViewModel(out _, out _, parts: [Part("TEST-A", onHand: 5m)]);
        await viewModel.ViewPreferencesLoaded;
        viewModel.SortBy(ReceivingPartSortKeys.PartId, ascending: true);
        viewModel.OnHandOnly = true;
        viewModel.QualityHoldOnly = true;

        viewModel.ResetFiltersCommand.Execute(null);

        viewModel.HasActiveFilters.Should().BeFalse();
        viewModel.ActiveFilterCount.Should().Be(0);
        viewModel.SortColumn.Should().Be(ReceivingPartSortKeys.PartId);
        viewModel.Parts.Select(part => part.PartID).Should().Equal("TEST-A");
    }

    [Fact]
    public async Task EmptyStateText_ShouldBlameTheFilters_WhenOnlyFiltersHideEveryPart()
    {
        var viewModel = CreateViewModel(out _, out _, parts: [Part("TEST-A")]);
        await viewModel.ViewPreferencesLoaded;

        viewModel.OnHandOnly = true;

        viewModel.ShowEmptyState.Should().BeTrue();
        viewModel.EmptyStateText.Should().Contain("filters");
    }

    // ── per-user persistence ───────────────────────────────────────────────────

    [Fact]
    public async Task FilterChange_ShouldSaveTheFilterForTheUser()
    {
        var saved = new TaskCompletionSource<string>();
        var viewModel = CreateViewModel(out _, out var settings, parts: [Part("TEST-A", onHand: 3m)]);
        settings
            .Setup(service =>
                service.SaveStringAsync(
                    ReceivingSettingsKeys.UiText.PartSelectionOnHandOnly,
                    It.IsAny<string>(),
                    It.IsAny<int?>()
                )
            )
            .Callback<string, string, int?>((_, value, _) => saved.TrySetResult(value))
            .Returns(Task.CompletedTask);
        await viewModel.ViewPreferencesLoaded;

        viewModel.OnHandOnly = true;

        (await saved.Task.WaitAsync(TimeSpan.FromSeconds(5))).Should().Be("true");
    }

    [Fact]
    public async Task SavedSortAndFilters_ShouldBeRestored_WhenThePageIsBuiltForTheSameUser()
    {
        var viewModel = CreateViewModel(
            out _,
            out var settings,
            parts: [Part("TEST-A", location: "002-A"), Part("TEST-B", location: "002-B")],
            storedSettings: new Dictionary<string, string>
            {
                [ReceivingSettingsKeys.UiText.PartSelectionSortColumn] =
                    ReceivingPartSortKeys.PartId,
                [ReceivingSettingsKeys.UiText.PartSelectionSortAscending] = "true",
                [ReceivingSettingsKeys.UiText.PartSelectionLocation] = "002-A",
            }
        );

        await viewModel.ViewPreferencesLoaded;

        viewModel.SortColumn.Should().Be(ReceivingPartSortKeys.PartId);
        viewModel.SortAscending.Should().BeTrue();
        viewModel.SelectedLocation.Should().Be("002-A");
        viewModel.Parts.Select(part => part.PartID).Should().Equal("TEST-A");
        settings.Verify(
            service =>
                service.GetStringAsync(
                    ReceivingSettingsKeys.UiText.PartSelectionLocation,
                    It.IsAny<int?>()
                ),
            Times.Once
        );
    }

    // ── double-click advances ──────────────────────────────────────────────────

    [Fact]
    public async Task SelectPartAndAdvance_ShouldMoveTheWalkOn_WhenAPartIsChosen()
    {
        var viewModel = CreateViewModel(out var workflow, out _, parts: [Part("TEST-A")]);
        await viewModel.ViewPreferencesLoaded;
        workflow
            .Setup(service => service.AdvanceToNextStepAsync())
            .ReturnsAsync(Model_ReceivingWorkflowStepResult.SuccessResult(Enum_ReceivingWorkflowStep.LoadEntry));

        viewModel.SelectedPart = viewModel.Parts[0];
        await viewModel.SelectPartAndAdvanceCommand.ExecuteAsync(null);

        workflow.Verify(service => service.AdvanceToNextStepAsync(), Times.Once);
    }

    [Fact]
    public async Task SelectPartAndAdvance_ShouldDoNothing_WhenNoRowIsChosen()
    {
        var viewModel = CreateViewModel(out var workflow, out _, parts: [Part("TEST-A")]);
        await viewModel.ViewPreferencesLoaded;

        await viewModel.SelectPartAndAdvanceCommand.ExecuteAsync(null);

        workflow.Verify(service => service.AdvanceToNextStepAsync(), Times.Never);
    }

    [Fact]
    public async Task SelectPartAndAdvance_ShouldStayPut_WhenTheQualityHoldWasDeclined()
    {
        // The PO page drops a part the clerk will not acknowledge, so there is nothing to carry on
        // with; the double-click must not walk past that conversation.
        var viewModel = CreateViewModel(
            out var workflow,
            out _,
            parts: [Part("MMFSR0001", qualityHold: true)],
            declineQualityHold: true
        );
        await viewModel.ViewPreferencesLoaded;

        viewModel.SelectedPart = viewModel.Parts[0];
        await viewModel.SelectPartAndAdvanceCommand.ExecuteAsync(null);

        viewModel.SelectedPart.Should().BeNull("the declined part is cleared");
        workflow.Verify(service => service.AdvanceToNextStepAsync(), Times.Never);
    }

    private static Model_InforVisualPart Part(
        string partId,
        DateTime? lastReceivedDate = null,
        decimal onHand = 0m,
        string location = "",
        bool qualityHold = false
    ) =>
        new()
        {
            PartID = partId,
            Description = $"Description for {partId}",
            LastReceivedDate = lastReceivedDate,
            OnHandQty = onHand,
            Location = location,
            RequiresQualityHold = qualityHold,
        };

    private static ViewModel_Receiving_PartSelection CreateViewModel(
        out Mock<IService_ReceivingWorkflow> workflow,
        out Mock<IService_ReceivingSettings> settings,
        IEnumerable<Model_InforVisualPart>? parts = null,
        IReadOnlyDictionary<string, string>? storedSettings = null,
        List<ViewModel_Receiving_POEntry>? livePoEntries = null,
        bool declineQualityHold = false)
    {
        workflow = new Mock<IService_ReceivingWorkflow>();
        workflow.SetupProperty(service => service.CurrentPart);
        workflow.SetupProperty(service => service.CurrentPONumber, "PO-066868");

        settings = new Mock<IService_ReceivingSettings>();
        settings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(
                (string key, int? _) =>
                    storedSettings is not null && storedSettings.TryGetValue(key, out var stored)
                        ? stored
                        : string.Empty
            );
        settings
            .Setup(service =>
                service.SaveStringAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int?>()
                )
            )
            .Returns(Task.CompletedTask);

        // No PO page is registered, which is the state a part page would be in if it were reached
        // without the PO step having built one.
        var registry = new Mock<IService_ViewModelRegistry>();

        if (livePoEntries is not null)
        {
            registry
                .Setup(service => service.GetViewModels<ViewModel_Receiving_POEntry>())
                .Returns(() => livePoEntries.ToArray());
        }
        else
        {
            registry
                .Setup(service => service.GetViewModels<ViewModel_Receiving_POEntry>())
                .Returns(
                    parts is null
                        ? []
                        : [CreatePoEntry(workflow, parts, declineQualityHold)]
                );
        }

        return new ViewModel_Receiving_PartSelection(
            workflow.Object,
            registry.Object,
            settings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private static ViewModel_Receiving_POEntry CreatePoEntry(
        Mock<IService_ReceivingWorkflow> workflow,
        IEnumerable<Model_InforVisualPart> parts,
        bool declineQualityHold = false)
    {
        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);

        var qualityHoldWarning = new Mock<IService_QualityHoldWarning>();
        qualityHoldWarning
            .Setup(service => service.IsRestrictedPart(It.IsAny<string>()))
            .Returns(declineQualityHold);
        qualityHoldWarning
            .Setup(service => service.CheckAndWarnAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        var poEntry = new ViewModel_Receiving_POEntry(
            new Mock<IService_InforVisual>().Object,
            workflow.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Help>().Object,
            qualityHoldWarning.Object,
            new Mock<IService_InforVisualMockDataCatalog>().Object,
            new Mock<IService_ViewModelRegistry>().Object,
            new Mock<IService_Window>().Object,
            new Mock<IService_AppSettings>().Object,
            receivingSettings.Object,
            new Mock<IService_Notification>().Object
        );

        poEntry.Parts = new ObservableCollection<Model_InforVisualPart>(parts);
        return poEntry;
    }
}

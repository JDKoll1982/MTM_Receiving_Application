using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.ViewModels;

/// <summary>
/// The review cycle pre-fills the destination, writes on accept, counts a skip
/// without writing, and resumes where it stopped (FR-039, FR-040, FR-044, FR-045).
/// </summary>
public sealed class ViewModel_ReconciliationMainReviewTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private readonly Mock<IService_ReconciliationPreview> _preview = new();
    private readonly Mock<IService_ReconciliationWriteService> _writeService = new();

    public ViewModel_ReconciliationMainReviewTests()
    {
        _writeService
            .Setup(service =>
                service.AcceptAsync(
                    It.IsAny<Model_ReconciliationFinding>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(
                (Model_ReconciliationFinding finding, string _, string _) =>
                {
                    finding.State = Enum_ReconciliationFindingState.Written;

                    return Model_Dao_Result_Factory.Success(finding);
                }
            );
    }

    [Fact]
    public async Task LoadShouldPreFillTheDestinationFromTheProposal()
    {
        ViewModel_ReconciliationMain viewModel = await Loaded();

        viewModel.SelectedFinding.Should().NotBeNull();
        viewModel.DestinationField.Should().Be("SHIP");
    }

    [Fact]
    public async Task AcceptShouldWriteOnceAndClearTheSelectionWhenNoWritableLinesRemain()
    {
        ViewModel_ReconciliationMain viewModel = await Loaded();
        Model_ReconciliationFinding first = viewModel.SelectedFinding!;

        await viewModel.AcceptCommand.ExecuteAsync(null);

        _writeService.Verify(
            service => service.AcceptAsync(first, "SHIP", It.IsAny<string>()),
            Times.Once
        );
        viewModel.AcceptedCount.Should().Be(1);
        viewModel.SelectedFinding.Should().BeNull();
    }

    [Fact]
    public async Task SkipShouldCountWithoutWritingAnything()
    {
        ViewModel_ReconciliationMain viewModel = await Loaded();

        viewModel.SkipCommand.Execute(null);

        _writeService.Verify(
            service =>
                service.AcceptAsync(
                    It.IsAny<Model_ReconciliationFinding>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
            Times.Never
        );
        viewModel.SkippedCount.Should().Be(1);
        viewModel.SelectedFinding.Should().BeNull();
    }

    [Fact]
    public async Task UnresolvedLinesShouldNotEnterTheCycleButShouldStayReachableAsNeedsAttention()
    {
        ViewModel_ReconciliationMain viewModel = await Loaded();

        viewModel.SelectedFinding!.State.Should().Be(Enum_ReconciliationFindingState.Writable);
        viewModel.SelectedFinding!.Outcome.Should().NotBe(Enum_ReconciliationOutcome.NotEnough);

        await viewModel.AcceptCommand.ExecuteAsync(null);

        viewModel.SelectedFinding.Should().BeNull();
        viewModel.UnresolvedCount.Should().Be(1);
        viewModel.OpenNeedsAttentionCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task AcceptShouldResumeAtTheFirstReviewableLineWithinTheSession()
    {
        ViewModel_ReconciliationMain viewModel = await Loaded();
        Model_ReconciliationFinding first = viewModel.SelectedFinding!;

        viewModel.SelectFirstReviewableLine();

        viewModel.SelectedFinding.Should().BeSameAs(first);
    }

    [Fact]
    public async Task AcceptShouldRemoveTheRowFromTheGrid()
    {
        ViewModel_ReconciliationMain viewModel = await Loaded();
        Model_ReconciliationFinding accepted = viewModel.SelectedFinding!;

        await viewModel.AcceptCommand.ExecuteAsync(null);

        viewModel.VisibleFindings.Should().NotContain(accepted);
    }

    [Fact]
    public async Task AcceptShouldRemoveTheGroup_WhenItsLastRowIsAccepted()
    {
        _preview
            .Setup(preview => preview.PreviewDayAsync(Today))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(TodayWithOneWritableLine()));

        ViewModel_ReconciliationMain viewModel = LoadedViewModel();
        await viewModel.LoadSelectedDateAsync();

        viewModel.GroupedFindings.Should().ContainSingle();

        await viewModel.AcceptCommand.ExecuteAsync(null);

        // The grid is grouped from the rows that remain, so the header cannot outlive
        // its last row.
        viewModel.GroupedFindings.Should().BeEmpty();
        viewModel.VisibleFindings.Should().BeEmpty();
        viewModel.EmptyStateMessage.Should().Contain("is done");
    }

    [Fact]
    public async Task AcceptShouldKeepTheGroup_WhenItsOtherRowsAreStillOpen()
    {
        ViewModel_ReconciliationMain viewModel = await Loaded();

        await viewModel.AcceptCommand.ExecuteAsync(null);

        viewModel.GroupedFindings.Should().ContainSingle();
        viewModel.VisibleFindings.Should()
            .OnlyContain(finding => finding.Outcome == Enum_ReconciliationOutcome.NotEnough);
    }

    [Fact]
    public async Task CountsShouldSurviveAReload()
    {
        Model_ReconciliationDay day = Day();
        Model_ReconciliationFinding extra = Finding(
            Enum_ReconciliationOutcome.Updated,
            "R-04",
            null,
            Enum_ReconciliationFindingState.Writable
        );
        day.Findings = [.. day.Findings, extra];
        day.Rows = [.. day.Rows, extra.Row];

        _preview.Setup(preview => preview.PreviewDayAsync(Today))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(day));

        ViewModel_ReconciliationMain viewModel = LoadedViewModel();
        await viewModel.LoadSelectedDateAsync();

        await viewModel.AcceptCommand.ExecuteAsync(null);
        viewModel.SkipCommand.Execute(null);
        await viewModel.LoadSelectedDateAsync();

        viewModel.AcceptedCount.Should().Be(1);
        viewModel.SkippedCount.Should().Be(1);
    }

    private async Task<ViewModel_ReconciliationMain> Loaded()
    {
        Model_ReconciliationDay day = Day();

        _preview.Setup(preview => preview.PreviewDayAsync(Today))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(day));

        ViewModel_ReconciliationMain viewModel = LoadedViewModel();

        await viewModel.LoadSelectedDateAsync();

        return viewModel;
    }

    private ViewModel_ReconciliationMain LoadedViewModel() =>
        new(
            _preview.Object,
            new Mock<IService_ReconciliationDayReader>().Object,
            _writeService.Object,
            new Mock<IService_ReconciliationSettings>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

    private static Model_ReconciliationDay Day()
    {
        var findings = new List<Model_ReconciliationFinding>
        {
            Finding(Enum_ReconciliationOutcome.Updated, "SHIP", null, Enum_ReconciliationFindingState.Writable),
            Finding(Enum_ReconciliationOutcome.NotEnough, string.Empty, 40m, Enum_ReconciliationFindingState.Unresolved),
            Finding(Enum_ReconciliationOutcome.Unchanged, "RECV", null, Enum_ReconciliationFindingState.Proven),
        };

        return new Model_ReconciliationDay
        {
            Date = Today,
            HasActivity = true,
            Rows = findings.Select(finding => finding.Row).ToList(),
            Findings = findings,
            State = Enum_ReconciliationDayState.Previewed,
            Summary = new Model_ReconciliationDaySummary { Scanned = findings.Count },
        };
    }

    private static Model_ReconciliationDay TodayWithOneWritableLine()
    {
        Model_ReconciliationFinding finding = Finding(
            Enum_ReconciliationOutcome.Updated,
            "SHIP",
            null,
            Enum_ReconciliationFindingState.Writable
        );

        return new Model_ReconciliationDay
        {
            Date = Today,
            HasActivity = true,
            Rows = [finding.Row],
            Findings = [finding],
            State = Enum_ReconciliationDayState.Previewed,
            Summary = new Model_ReconciliationDaySummary { Scanned = 1 },
        };
    }

    private static Model_ReconciliationFinding Finding(
        Enum_ReconciliationOutcome outcome,
        string proposedLocation,
        decimal? difference,
        Enum_ReconciliationFindingState state
    )
    {
        var row = new Model_ReconciliationRow
        {
            PartId = "MMC0001000",
            PurchaseOrderNumber = "PO-066868",
            PurchaseOrderLineNumber = "1",
            TransactionDate = Today,
            Quantity = 100,
            SavedLocation = "RECV",
            UnitOfMeasure = "EA",
        };

        return new Model_ReconciliationFinding(
            row,
            outcome,
            "RECV",
            proposedLocation,
            Enum_ReconciliationDestinationSource.Derived,
            $"Reason for {outcome}.",
            difference,
            null
        )
        {
            State = state,
        };
    }
}

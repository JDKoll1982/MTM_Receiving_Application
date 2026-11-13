using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Converters;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Reconciliation.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.ViewModels;

/// <summary>
/// The working page opens on today, steps only between active days, filters by
/// outcome and text, and states an empty day explicitly (FR-008, FR-009, FR-010,
/// FR-043, FR-048).
/// </summary>
public sealed class ViewModel_ReconciliationMainTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly Yesterday = Today.AddDays(-1);

    private readonly Mock<IService_ReconciliationPreview> _preview = new();
    private readonly Mock<IService_ReconciliationDayReader> _dayReader = new();

    [Fact]
    public void ViewModelShouldOpenOnToday()
    {
        ViewModel_ReconciliationMain viewModel = Build();

        viewModel.SelectedDate.Should().Be(Today);
    }

    [Fact]
    public void SelectedDateOffsetShouldRoundTripToTheSameDayInLocalTime()
    {
        ViewModel_ReconciliationMain viewModel = Build();

        viewModel.SelectedDate = new DateOnly(2026, 11, 12);

        // A UTC offset would land midnight of the 12th on the 11th west of Greenwich,
        // so the picker showed the wrong day and every change walked it back one more.
        DateOnly.FromDateTime(viewModel.SelectedDateOffset.Date)
            .Should()
            .Be(new DateOnly(2026, 11, 12));
        viewModel.SelectedDateOffset.Offset.Should().Be(TimeZoneInfo.Local.GetUtcOffset(
            new DateTime(2026, 11, 12, 0, 0, 0, DateTimeKind.Local)));
    }

    [Fact]
    public void SweepOffsetsShouldRoundTripToTheSameDayInLocalTime()
    {
        ViewModel_ReconciliationMain viewModel = Build();

        viewModel.SweepStartDate = new DateOnly(2026, 11, 1);
        viewModel.SweepEndDate = new DateOnly(2026, 11, 12);

        DateOnly.FromDateTime(viewModel.SweepStartDateOffset.Date)
            .Should()
            .Be(new DateOnly(2026, 11, 1));
        DateOnly.FromDateTime(viewModel.SweepEndDateOffset.Date)
            .Should()
            .Be(new DateOnly(2026, 11, 12));
    }

    [Fact]
    public void MovementTimeTextShouldOmitTheTimeWhenVisualHeldOnlyADate()
    {
        var finding = new Model_ReconciliationFinding(
            new Model_ReconciliationRow { PartId = "MMC0000850" },
            Enum_ReconciliationOutcome.Updated,
            "RECV",
            "V-E0-03",
            Enum_ReconciliationDestinationSource.Derived,
            "Moved.",
            null,
            null
        )
        {
            MovementAt = new DateTime(2026, 11, 12, 0, 0, 0, DateTimeKind.Unspecified),
        };

        // A midnight value means Visual recorded no time of day. Showing 00:00 would be
        // a fabricated clock reading.
        finding.MovementTimeText.Should().Be("2026-11-12");
    }

    [Fact]
    public void MovementTimeTextShouldShowTheTimeWhenVisualRecordedOne()
    {
        var finding = new Model_ReconciliationFinding(
            new Model_ReconciliationRow { PartId = "MMC0000850" },
            Enum_ReconciliationOutcome.Updated,
            "RECV",
            "V-E0-03",
            Enum_ReconciliationDestinationSource.Derived,
            "Moved.",
            null,
            null
        )
        {
            MovementAt = new DateTime(2026, 11, 12, 14, 32, 0, DateTimeKind.Unspecified),
        };

        finding.MovementTimeText.Should().Be("2026-11-12 14:32");
    }

    [Fact]
    public void FindingShouldNameTheMoveFromItsBucketEvenWhenTheLineIsNotWritable()
    {
        var bucket = new Model_ReconciliationDestinationBucket
        {
            LocationId = "V-C0-01",
            NetQuantity = 8640m,
            RemainingQuantity = 740m,
            LatestMovementAt = new DateTime(2026, 11, 13, 9, 47, 0, DateTimeKind.Unspecified),
            TransactionUserId = "MLAURIN",
        };

        var finding = new Model_ReconciliationFinding(
            new Model_ReconciliationRow { PartId = "MMC0000667" },
            Enum_ReconciliationOutcome.ExtraSurplus,
            "RECV",
            "V-C0-01",
            Enum_ReconciliationDestinationSource.Derived,
            "Destination V-C0-01 received 8640 but its rows account for 7900, leaving 740 unaccounted for.",
            740m,
            bucket
        );

        // A surplus line is evidence about a move, so the pane names that move even
        // though the line itself cannot be written.
        finding.MovementTimeText.Should().Be("2026-11-13 09:47");
        finding.MovementUserId.Should().Be("MLAURIN");
    }

    [Fact]
    public void MovementTimeTextShouldBeEmptyWhenTheBucketCarriesNoTime()
    {
        var bucket = new Model_ReconciliationDestinationBucket
        {
            LocationId = "V-C0-01",
            NetQuantity = 8640m,
        };

        var finding = new Model_ReconciliationFinding(
            new Model_ReconciliationRow { PartId = "MMC0000667" },
            Enum_ReconciliationOutcome.NotEnough,
            "RECV",
            "V-C0-01",
            Enum_ReconciliationDestinationSource.Derived,
            "Destination V-C0-01 received less than its rows need.",
            740m,
            bucket
        );

        finding.MovementTimeText.Should().BeEmpty();
    }

    [Fact]
    public async Task GoToNextDayShouldMoveOnlyToADayThatHoldsRows()
    {
        ViewModel_ReconciliationMain viewModel = Build();
        StubDay(viewModel, YahooDay(Today));
        StubDay(viewModel, YahooDay(Yesterday.AddDays(-5)));
        _dayReader
            .Setup(reader => reader.FindNearestActiveDayAsync(Today, 1))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<DateOnly?>(Yesterday.AddDays(-5)));

        await viewModel.GoToNextDayCommand.ExecuteAsync(null);

        viewModel.SelectedDate.Should().Be(Yesterday.AddDays(-5));
        _preview.Verify(preview => preview.PreviewDayAsync(Yesterday.AddDays(-5)), Times.Once);
    }

    [Fact]
    public async Task GoToNextDayShouldStayPutWhenNoLaterDayHoldsRows()
    {
        ViewModel_ReconciliationMain viewModel = Build();
        StubDay(viewModel, YahooDay(Today));
        _dayReader
            .Setup(reader => reader.FindNearestActiveDayAsync(Today, 1))
            .ReturnsAsync(Model_Dao_Result_Factory.Success<DateOnly?>(null));

        await viewModel.GoToNextDayCommand.ExecuteAsync(null);

        viewModel.SelectedDate.Should().Be(Today);
    }

    [Fact]
    public async Task GoToTodayShouldReturnFromAnyPositionInOneAction()
    {
        ViewModel_ReconciliationMain viewModel = Build();
        StubDay(viewModel, YahooDay(Today));

        await viewModel.GoToTodayCommand.ExecuteAsync(null);

        viewModel.SelectedDate.Should().Be(Today);
        _preview.Verify(preview => preview.PreviewDayAsync(Today), Times.Once);
    }

    [Fact]
    public async Task FiltersShouldNarrowTheVisibleGridByOutcomeAndText()
    {
        ViewModel_ReconciliationMain viewModel = Build();
        StubDay(
            viewModel,
            Day(
                Finding("MMC0001000", "PO-066868", Enum_ReconciliationOutcome.Updated),
                Finding("MMC0002000", "PO-066914", Enum_ReconciliationOutcome.Unchanged),
                Finding("MMC0003000", "PO-066999", Enum_ReconciliationOutcome.NotEnough)
            )
        );

        await viewModel.LoadSelectedDateAsync();
        viewModel.VisibleFindings.Should().HaveCount(2);

        viewModel.OutcomeFilter = Enum_ReconciliationOutcome.NotEnough;
        viewModel.VisibleFindings.Should().ContainSingle()
            .Which.Row.PartId.Should().Be("MMC0003000");

        viewModel.OutcomeFilter = null;
        viewModel.TextFilter = "066868";
        viewModel.VisibleFindings.Should().ContainSingle()
            .Which.Row.PurchaseOrderNumber.Should().Be("PO-066868");

        viewModel.TextFilter = "mmc0003";
        viewModel.VisibleFindings.Should().ContainSingle()
            .Which.Row.PartId.Should().Be("MMC0003000");
    }

    [Fact]
    public void OutcomeChoicesShouldOfferEveryNamedOutcomeFromThePinnedText()
    {
        ViewModel_ReconciliationMain viewModel = Build();

        viewModel.OutcomeFilterChoices.Should().HaveCount(
            Enum.GetValues<Enum_ReconciliationOutcome>().Length + 1
        );
        viewModel.OutcomeFilterChoices[0].Outcome.Should().BeNull();
        viewModel.OutcomeFilterChoices[0].DisplayText.Should().Be("All outcomes");
        viewModel
            .OutcomeFilterChoices.Skip(1)
            .Select(choice => choice.Outcome)
            .Should()
            .BeEquivalentTo(Enum.GetValues<Enum_ReconciliationOutcome>());
        viewModel
            .OutcomeFilterChoices.Should()
            .Contain(choice => choice.Outcome == Enum_ReconciliationOutcome.Unchanged);
        viewModel
            .OutcomeFilterChoices.Skip(1)
            .Should()
            .OnlyContain(choice =>
                choice.DisplayText
                == Converter_ReconciliationOutcomeText.GetDisplayText(choice.Outcome!.Value)
            );
    }

    [Fact]
    public async Task AGridShouldHideMatchingLinesUntilTheUnchangedFilterAsksForThem()
    {
        ViewModel_ReconciliationMain viewModel = Build();
        StubDay(
            viewModel,
            Day(
                Finding("MMC0001000", "PO-066868", Enum_ReconciliationOutcome.Updated),
                Finding("MMC0002000", "PO-066914", Enum_ReconciliationOutcome.Unchanged)
            )
        );

        await viewModel.LoadSelectedDateAsync();

        viewModel.Findings.Should().HaveCount(2);
        viewModel.VisibleFindings.Should().ContainSingle()
            .Which.Row.PartId.Should().Be("MMC0001000");

        viewModel.OutcomeFilterChoice = viewModel
            .OutcomeFilterChoices.Single(choice =>
                choice.Outcome == Enum_ReconciliationOutcome.Unchanged
            );

        viewModel.VisibleFindings.Should().ContainSingle()
            .Which.Row.PartId.Should().Be("MMC0002000");
        viewModel.GroupedFindings.Should().ContainSingle();

        viewModel.OutcomeFilterChoice = viewModel.OutcomeFilterChoices[0];

        viewModel.VisibleFindings.Should().ContainSingle()
            .Which.Row.PartId.Should().Be("MMC0001000");
    }

    [Fact]
    public async Task OutcomeSelectorShouldDriveTheFilterAndClearBackToEveryVisibleOutcome()
    {
        ViewModel_ReconciliationMain viewModel = Build();
        StubDay(
            viewModel,
            Day(
                Finding("MMC0001000", "PO-066868", Enum_ReconciliationOutcome.Updated),
                Finding("MMC0002000", "PO-066914", Enum_ReconciliationOutcome.NotEnough),
                Finding("MMC0003000", "PO-066999", Enum_ReconciliationOutcome.Unchanged)
            )
        );

        await viewModel.LoadSelectedDateAsync();
        viewModel.OutcomeFilterChoice!.Outcome.Should().BeNull();
        viewModel.VisibleFindings.Should().HaveCount(2);

        viewModel.OutcomeFilterChoice = viewModel.OutcomeFilterChoices.Single(choice =>
            choice.Outcome == Enum_ReconciliationOutcome.NotEnough
        );

        viewModel.OutcomeFilter.Should().Be(Enum_ReconciliationOutcome.NotEnough);
        viewModel.VisibleFindings.Should().ContainSingle()
            .Which.Row.PartId.Should().Be("MMC0002000");

        viewModel.OutcomeFilterChoice = viewModel.OutcomeFilterChoices[0];

        viewModel.OutcomeFilter.Should().BeNull();
        viewModel.VisibleFindings.Should().HaveCount(2);
    }

    [Fact]
    public async Task LoadShouldShowTheRunsInforVisualUserAttribution()
    {
        ViewModel_ReconciliationMain viewModel = Build();
        Model_ReconciliationDay day = Day(
            Finding("MMC0001000", "PO-066868", Enum_ReconciliationOutcome.Updated)
        );
        day.Run = new Model_ReconciliationRun
        {
            RunId = Guid.NewGuid(),
            Day = Today,
            StartedAt = DateTime.Now,
            VisualUserId = "vuser, vuser2",
        };
        StubDay(viewModel, day);

        await viewModel.LoadSelectedDateAsync();

        viewModel.VisualUserText.Should().Be("vuser, vuser2");
    }

    [Fact]
    public async Task AFailedDayShouldClearTheWholePageRatherThanShowThePreviousDay()
    {
        ViewModel_ReconciliationMain viewModel = Build();

        Model_ReconciliationFinding moved = Finding(
            "MMC0001000",
            "PO-066868",
            Enum_ReconciliationOutcome.Updated
        );
        moved.State = Enum_ReconciliationFindingState.Writable;

        Model_ReconciliationDay good = Day(
            moved,
            Finding("MMC0002000", "PO-066914", Enum_ReconciliationOutcome.NotEnough),
            Finding("MMC0003000", "PO-066999", Enum_ReconciliationOutcome.Unchanged)
        );
        good.Run = new Model_ReconciliationRun
        {
            RunId = Guid.NewGuid(),
            Day = Today,
            StartedAt = DateTime.Now,
            FinishedAt = DateTime.Now,
            VisualUserId = "vuser",
        };
        StubDay(viewModel, good);

        await viewModel.LoadSelectedDateAsync();
        viewModel.VisualUserText.Should().Be("vuser");
        viewModel.RunIdentifier.Should().NotBeEmpty();
        viewModel.GroupedFindings.Should().HaveCount(2);
        viewModel.SelectedFinding.Should().BeSameAs(moved);
        viewModel.DestinationField.Should().Be("SHIP");
        viewModel.UnresolvedCount.Should().Be(1);
        viewModel.TotalLineCount.Should().BeGreaterThan(0);
        viewModel.CurrentLineIndex.Should().BeGreaterThan(0);

        _preview
            .Setup(preview => preview.PreviewDayAsync(Yesterday))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<Model_ReconciliationDay>(
                    "The day could not be read."
                )
            );

        await viewModel.SelectDateAsync(Yesterday);

        viewModel.DayState.Should().Be(Enum_ReconciliationDayState.Failed);
        viewModel.FailureMessage.Should().Be("The day could not be read.");
        viewModel.VisualUserText.Should().BeEmpty();
        viewModel.RunIdentifier.Should().BeEmpty();
        viewModel.RunDurationText.Should().BeEmpty();
        viewModel.Findings.Should().BeEmpty();
        viewModel.VisibleFindings.Should().BeEmpty();
        viewModel.GroupedFindings.Should().BeEmpty();
        viewModel.SelectedFinding.Should().BeNull();
        viewModel.DestinationField.Should().BeEmpty();
        viewModel.UnresolvedCount.Should().Be(0);
        viewModel.TotalLineCount.Should().Be(0);
        viewModel.CurrentLineIndex.Should().Be(0);
        viewModel.Summary.Scanned.Should().Be(0);
        viewModel.Summary.TotalAccounted.Should().Be(0);
        viewModel.EmptyStateMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task AFailedDayShouldClearTheEmptyStateMessageTheEarlierDaySet()
    {
        ViewModel_ReconciliationMain viewModel = Build();
        StubDay(viewModel, Day());

        await viewModel.LoadSelectedDateAsync();
        viewModel.EmptyStateMessage.Should().NotBeNullOrWhiteSpace();

        _preview
            .Setup(preview => preview.PreviewDayAsync(Yesterday))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<Model_ReconciliationDay>(
                    "The day could not be read."
                )
            );

        await viewModel.SelectDateAsync(Yesterday);

        viewModel.EmptyStateMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task AnAllMatchingDayShouldPointAtTheFilterUntilTheFilterIsChosen()
    {
        ViewModel_ReconciliationMain viewModel = Build();
        StubDay(
            viewModel,
            Day(
                Finding("MMC0001000", "PO-066868", Enum_ReconciliationOutcome.Unchanged),
                Finding("MMC0002000", "PO-066914", Enum_ReconciliationOutcome.Unchanged)
            )
        );

        await viewModel.LoadSelectedDateAsync();

        viewModel.VisibleFindings.Should().BeEmpty();
        viewModel.EmptyStateMessage.Should().Contain("Choose the Unchanged outcome");

        viewModel.OutcomeFilterChoice = viewModel
            .OutcomeFilterChoices.Single(choice =>
                choice.Outcome == Enum_ReconciliationOutcome.Unchanged
            );

        viewModel.VisibleFindings.Should().HaveCount(2);
        viewModel.EmptyStateMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadShouldReportTheEmptyStateForADayWithNoRows()
    {
        ViewModel_ReconciliationMain viewModel = Build();
        StubDay(viewModel, Day());

        await viewModel.LoadSelectedDateAsync();

        viewModel.Findings.Should().BeEmpty();
        viewModel.EmptyStateMessage.Should().NotBeNullOrWhiteSpace();
        viewModel.DayState.Should().Be(Enum_ReconciliationDayState.Previewed);
    }

    private static Model_ReconciliationDay YahooDay(DateOnly date) =>
        Day(date, Finding("MMC0001000", "PO-066868", Enum_ReconciliationOutcome.Updated));

    private void StubDay(ViewModel_ReconciliationMain viewModel, Model_ReconciliationDay day)
    {
        _preview.Setup(preview => preview.PreviewDayAsync(day.Date))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(day));
    }

    private ViewModel_ReconciliationMain Build() =>
        new(
            _preview.Object,
            _dayReader.Object,
            new Mock<IService_ReconciliationWriteService>().Object,
            new Mock<IService_ReconciliationSettings>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

    private static Model_ReconciliationDay Day(params Model_ReconciliationFinding[] findings) =>
        Day(Today, findings);

    private static Model_ReconciliationDay Day(
        DateOnly date,
        params Model_ReconciliationFinding[] findings
    ) =>
        new()
        {
            Date = date,
            HasActivity = findings.Length > 0,
            Rows = findings.Select(finding => finding.Row).ToList(),
            Findings = findings,
            State = Enum_ReconciliationDayState.Previewed,
            Summary = new Model_ReconciliationDaySummary
            {
                Scanned = findings.Length,
                Unchanged = findings.Count(finding =>
                    finding.Outcome is Enum_ReconciliationOutcome.Unchanged
                ),
                Written = findings.Count(finding =>
                    finding.Outcome is Enum_ReconciliationOutcome.Updated
                ),
                Unresolved = findings.Count(finding =>
                    finding.Outcome is Enum_ReconciliationOutcome.NotEnough
                ),
            },
        };

    private static Model_ReconciliationFinding Finding(
        string partId,
        string purchaseOrderNumber,
        Enum_ReconciliationOutcome outcome
    )
    {
        var row = new Model_ReconciliationRow
        {
            PartId = partId,
            PurchaseOrderNumber = purchaseOrderNumber,
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
            outcome is Enum_ReconciliationOutcome.Updated ? "SHIP" : string.Empty,
            Enum_ReconciliationDestinationSource.Derived,
            $"Reason for {outcome}.",
            outcome is Enum_ReconciliationOutcome.NotEnough ? 12m : null,
            null
        );
    }
}

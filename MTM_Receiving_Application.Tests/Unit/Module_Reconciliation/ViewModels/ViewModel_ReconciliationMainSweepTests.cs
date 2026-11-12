using System;
using System.Collections.Generic;
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
/// The sweep repeats the single-day pass in order, marks completed days, stops on
/// request, stops on the first failing day, and never widens a window
/// (FR-011, FR-012, FR-014, SC-006).
/// </summary>
public sealed class ViewModel_ReconciliationMainSweepTests
{
    private static readonly DateOnly FirstDay = new(2026, 11, 10);

    private readonly Mock<IService_ReconciliationPreview> _preview = new();
    private readonly Mock<IService_ReconciliationSettings> _settings = new();

    public ViewModel_ReconciliationMainSweepTests()
    {
        _settings.Setup(service => service.GetDiscoveryWindowDaysAsync()).ReturnsAsync(2);
        _settings.Setup(service => service.GetStrictWindowDaysAsync()).ReturnsAsync(1);
    }

    [Fact]
    public async Task SweepShouldRunEachDayInOrderWithPerDayProgress()
    {
        ViewModel_ReconciliationMain viewModel = Build(3);

        await viewModel.StartSweepCommand.ExecuteAsync(null);

        for (int offset = 0; offset < 3; offset++)
        {
            DateOnly day = FirstDay.AddDays(offset);
            _preview.Verify(preview => preview.PreviewDayAsync(day), Times.Once);
            viewModel.IsDayCompleted(day).Should().BeTrue();
        }

        viewModel.SweepProgressText.Should().Contain("Day 3 of 3");
        viewModel.IsSweepRunning.Should().BeFalse();
    }

    [Fact]
    public async Task SweepShouldStopBetweenDaysOnRequest()
    {
        ViewModel_ReconciliationMain viewModel = Build(5);
        _preview
            .Setup(preview => preview.PreviewDayAsync(FirstDay))
            .Callback(() => viewModel.StopSweepCommand.Execute(null))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(Day(FirstDay)));

        await viewModel.StartSweepCommand.ExecuteAsync(null);

        viewModel.IsDayCompleted(FirstDay).Should().BeTrue();
        _preview.Verify(preview => preview.PreviewDayAsync(FirstDay), Times.AtMost(2));
        viewModel.IsSweepRunning.Should().BeFalse();
    }

    [Fact]
    public async Task SweepShouldStopOnTheFirstFailingDayAndLeaveTheOperatorThere()
    {
        ViewModel_ReconciliationMain viewModel = Build(5);
        DateOnly failingDay = FirstDay.AddDays(1);

        _preview
            .Setup(preview => preview.PreviewDayAsync(failingDay))
            .ReturnsAsync(
                Model_Dao_Result_Factory.Failure<Model_ReconciliationDay>(
                    "The application database is unreachable."
                )
            );

        await viewModel.StartSweepCommand.ExecuteAsync(null);

        viewModel.IsDayCompleted(FirstDay).Should().BeTrue();
        viewModel.IsDayCompleted(failingDay).Should().BeFalse();
        viewModel.SelectedDate.Should().Be(failingDay);
        viewModel.DayState.Should().Be(Enum_ReconciliationDayState.Failed);
        viewModel.SweepProgressText.Should().Contain("failed");
        _preview.Verify(
            preview => preview.PreviewDayAsync(FirstDay.AddDays(2)),
            Times.Never
        );
    }

    [Fact]
    public async Task SweepShouldNeverWidenEitherWindow()
    {
        ViewModel_ReconciliationMain viewModel = Build(2);

        await viewModel.StartSweepCommand.ExecuteAsync(null);

        _settings.Verify(service => service.SetDiscoveryWindowDaysAsync(It.IsAny<int>()), Times.Never);
        _settings.Verify(service => service.SetStrictWindowDaysAsync(It.IsAny<int>()), Times.Never);
        _settings.Verify(service => service.GetDiscoveryWindowDaysAsync(), Times.Once);
        _settings.Verify(service => service.GetStrictWindowDaysAsync(), Times.Once);
    }

    private ViewModel_ReconciliationMain Build(int dayCount)
    {
        for (int offset = 0; offset < dayCount; offset++)
        {
            DateOnly day = FirstDay.AddDays(offset);
            _preview.Setup(preview => preview.PreviewDayAsync(day))
                .ReturnsAsync(Model_Dao_Result_Factory.Success(Day(day)));
        }

        var viewModel = new ViewModel_ReconciliationMain(
            _preview.Object,
            new Mock<IService_ReconciliationDayReader>().Object,
            new Mock<IService_ReconciliationWriteService>().Object,
            _settings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        )
        {
            SweepStartDate = FirstDay,
            SweepEndDate = FirstDay.AddDays(dayCount - 1),
        };

        return viewModel;
    }

    private static Model_ReconciliationDay Day(DateOnly date) =>
        new()
        {
            Date = date,
            HasActivity = true,
            Rows = new List<Model_ReconciliationRow>(),
            Findings = new List<Model_ReconciliationFinding>(),
            State = Enum_ReconciliationDayState.Previewed,
            Summary = new Model_ReconciliationDaySummary(),
        };
}

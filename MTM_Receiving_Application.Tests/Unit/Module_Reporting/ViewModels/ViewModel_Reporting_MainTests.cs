using System;
using System.Collections.ObjectModel;
using System.Globalization;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Models;
using MTM_Receiving_Application.Module_Reporting.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reporting.ViewModels;

public sealed class ViewModel_Reporting_MainTests
{
    [Fact]
    public void SelectingYesterdayPreset_ShouldSetBothDatesToYesterday()
    {
        var viewModel = CreateViewModel();
        var today = DateTimeOffset.Now.Date;

        viewModel.SelectedDateRangePreset = "Yesterday";

        viewModel.StartDate.Date.Should().Be(today.AddDays(-1));
        viewModel.EndDate.Date.Should().Be(today.AddDays(-1));
    }

    [Fact]
    public void SelectingTodayPreset_ShouldSetBothDatesToToday()
    {
        var viewModel = CreateViewModel();
        var today = DateTimeOffset.Now.Date;

        viewModel.SelectedDateRangePreset = "Today";

        viewModel.StartDate.Date.Should().Be(today);
        viewModel.EndDate.Date.Should().Be(today);
    }

    [Fact]
    public void SelectingThisWeekPreset_ShouldStartAtCurrentCultureWeekBoundary()
    {
        var viewModel = CreateViewModel();
        var today = DateTimeOffset.Now.Date;
        var firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        var delta = ((int)today.DayOfWeek - (int)firstDayOfWeek + 7) % 7;

        viewModel.SelectedDateRangePreset = "This Week";

        viewModel.StartDate.Date.Should().Be(today.AddDays(-delta));
        viewModel.EndDate.Date.Should().Be(today);
    }

    [Fact]
    public void ChangingDateManually_ShouldResetPresetToCustom()
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedDateRangePreset = "Today";
        viewModel.StartDate = viewModel.StartDate.AddDays(-2);

        viewModel.SelectedDateRangePreset.Should().Be("Custom");
    }

    [Fact]
    public void GetDateRangeText_ShouldReturnSingleDate_WhenStartAndEndMatch()
    {
        var viewModel = CreateViewModel();
        var singleDate = new DateTimeOffset(2026, 4, 26, 17, 45, 0, TimeSpan.Zero);

        viewModel.StartDate = singleDate;
        viewModel.EndDate = singleDate;

        var method = typeof(ViewModel_Reporting_Main).GetMethod(
            "GetDateRangeText",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );

        method.Should().NotBeNull();
        var result = method!.Invoke(viewModel, null);

        result.Should().Be("4/26/2026");
    }

    [Fact]
    public void RecipientCommands_ShouldOnlyBeEnabled_WhenRecipientsContainText()
    {
        var viewModel = CreateViewModel();
        viewModel.IncludedPreviewModuleCards =
        [
            new Model_ReportingPreviewModuleCard { ModuleName = "Reporting" },
        ];

        viewModel.CopyToRecipientsCommand.CanExecute(null).Should().BeFalse();
        viewModel.CopyCcRecipientsCommand.CanExecute(null).Should().BeFalse();

        viewModel.ToRecipients = "to@example.com";
        viewModel.CcRecipients = "cc@example.com";

        viewModel.CopyToRecipientsCommand.CanExecute(null).Should().BeTrue();
        viewModel.CopyCcRecipientsCommand.CanExecute(null).Should().BeTrue();

        viewModel.ToRecipients = string.Empty;
        viewModel.CcRecipients = "   ";

        viewModel.CopyToRecipientsCommand.CanExecute(null).Should().BeFalse();
        viewModel.CopyCcRecipientsCommand.CanExecute(null).Should().BeFalse();
    }

    private static ViewModel_Reporting_Main CreateViewModel()
    {
        return new ViewModel_Reporting_Main(
            new Mock<IService_Reporting>().Object,
            new Mock<IService_ReportingClipboard>().Object,
            new Mock<IService_ReportingRecipientSettings>().Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }
}

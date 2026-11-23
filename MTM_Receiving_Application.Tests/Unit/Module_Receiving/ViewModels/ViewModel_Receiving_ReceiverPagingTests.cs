using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Core.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Receiving.ViewModels;

/// <summary>
/// FR-010: the list pages rather than scrolls, the page-size control offers 25, 50, 100 and 200 and
/// opens on 50, and the pager shows the current page and the total.
/// </summary>
public sealed class ViewModel_Receiving_ReceiverPagingTests
{
    [Fact]
    public async Task PageSizes_ShouldOfferFourCountsAndOpenOnFifty()
    {
        var viewModel = await CreateLoadedViewModelAsync();

        viewModel.PageSizes.Should().Equal(25, 50, 100, 200);
        viewModel.PageSize.Should().Be(50);
        viewModel.Rows.Should().HaveCount(50);
    }

    [Fact]
    public async Task PageSize_ShouldRedisplayFromPageOne_WhenItChanges()
    {
        var viewModel = await CreateLoadedViewModelAsync();
        viewModel.GoToNextPageCommand.Execute(null);
        viewModel.CurrentPage.Should().Be(2);

        viewModel.PageSize = 25;
        await Task.Delay(25);

        viewModel.CurrentPage.Should().Be(1);
        viewModel.TotalPages.Should().Be(5);
        viewModel.Rows.Should().HaveCount(25);
    }

    [Fact]
    public async Task Pager_ShouldMoveThePageAndUpdateTheIndicator()
    {
        var viewModel = await CreateLoadedViewModelAsync(100);
        viewModel.PageIndicator.Should().Be("Page 1 of 2");

        viewModel.GoToNextPageCommand.Execute(null);

        viewModel.CurrentPage.Should().Be(2);
        viewModel.CanGoNextPage.Should().BeFalse();
        viewModel.CanGoPreviousPage.Should().BeTrue();
        viewModel.PageIndicator.Should().Be("Page 2 of 2");
        viewModel.Rows.Should().HaveCount(50);

        viewModel.GoToPreviousPageCommand.Execute(null);

        viewModel.CurrentPage.Should().Be(1);
        viewModel.PageIndicator.Should().Be("Page 1 of 2");
        viewModel.Rows.Should().HaveCount(50);
    }

    [Fact]
    public async Task Pager_ShouldStayPresentButInactive_WhenEverythingFitsOnOnePage()
    {
        var viewModel = await CreateLoadedViewModelAsync(20);

        viewModel.TotalPages.Should().Be(1);
        viewModel.CanGoPreviousPage.Should().BeFalse();
        viewModel
            .CanGoNextPage.Should()
            .BeFalse("one page of results disables the pager rather than hiding it");
        viewModel.PageIndicator.Should().Be("Page 1 of 1");

        var xaml = ReadReceiverSelectionXaml();
        xaml.Should().Contain("x:Name=\"ReceiverPreviousPageButton\"");
        xaml.Should().Contain("x:Name=\"ReceiverNextPageButton\"");
        xaml.Should().Contain("x:Name=\"ReceiverPageIndicator\"");
    }

    [Fact]
    public async Task SelectRow_ShouldCarryTheRowFromThePageTheOperatorWasOn()
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow.SetupProperty(service => service.SelectedReceiver);
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ReceiverSelection);

        var viewModel = await CreateLoadedViewModelAsync(workflow: workflow);
        viewModel.GoToNextPageCommand.Execute(null);
        viewModel.CurrentPage.Should().Be(2);

        var chosen = viewModel.Rows[0];
        viewModel.SelectRowCommand.Execute(chosen);

        workflow.Object.SelectedReceiver.Should().BeSameAs(chosen);
        workflow.Verify(
            service => service.GoToStep(Enum_ReceivingWorkflowStep.LoadEntry),
            Times.Once
        );
    }

    [Fact]
    public async Task Back_ShouldOpenThePageHoldingTheSelectedRow_WhenTheRowWasOnALaterPage()
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow.SetupProperty(service => service.SelectedReceiver);
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ReceiverSelection);

        var viewModel = await CreateLoadedViewModelAsync(workflow: workflow);
        viewModel.GoToNextPageCommand.Execute(null);
        viewModel.CurrentPage.Should().Be(2);

        var chosen = viewModel.Rows[5];
        viewModel.SelectRowCommand.Execute(chosen);
        workflow.SetupGet(service => service.SelectedReceiverKey).Returns(chosen.SelectionKey);

        workflow.Raise(service => service.StepChanged += null, EventArgs.Empty);
        await Task.Delay(25);

        viewModel.CurrentPage.Should().Be(2);
        viewModel.SelectedRow.Should().NotBeNull();
        viewModel.SelectedRow!.SelectionKey.Should().Be(chosen.SelectionKey);
        viewModel.Rows.Should().Contain(viewModel.SelectedRow);
    }

    private static async Task<ViewModel_Receiving_ReceiverSelection> CreateLoadedViewModelAsync(
        int rowCount = 120,
        Mock<IService_ReceivingWorkflow>? workflow = null
    )
    {
        workflow ??= new Mock<IService_ReceivingWorkflow>();
        if (workflow.Object.CurrentStep != Enum_ReceivingWorkflowStep.ReceiverSelection)
        {
            workflow
                .SetupGet(service => service.CurrentStep)
                .Returns(Enum_ReceivingWorkflowStep.ReceiverSelection);
        }

        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service => service.GetRecentReceiversAsync(It.IsAny<int>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(CreateSampleRows(rowCount)));

        var receivingSettings = new Mock<IService_ReceivingSettings>();
        receivingSettings
            .Setup(service => service.GetStringAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(string.Empty);
        receivingSettings
            .Setup(service => service.GetIntAsync(It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(50);

        var viewModel = new ViewModel_Receiving_ReceiverSelection(
            workflow.Object,
            inforVisual.Object,
            new Service_Pagination(),
            receivingSettings.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        await Task.Delay(25);
        return viewModel;
    }

    private static List<Model_InforVisualReceiverRow> CreateSampleRows(int rowCount) =>
        Enumerable
            .Range(1, rowCount)
            .Select(index => new Model_InforVisualReceiverRow
            {
                ReceiverNumber = $"TEST-RCV-{index:D4}",
                PartNumber = $"TEST-PART-{index:D4}",
            })
            .ToList();

    private static string ReadReceiverSelectionXaml()
    {
        var relativePath = System.IO.Path.Combine(
            "Module_Receiving",
            "Views",
            "View_Receiving_ReceiverSelection.xaml"
        );
        var directory = new System.IO.DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = System.IO.Path.Combine(directory.FullName, relativePath);
            if (System.IO.File.Exists(candidate))
            {
                return System.IO.File.ReadAllText(candidate);
            }

            directory = directory.Parent;
        }

        throw new System.IO.FileNotFoundException(
            $"Could not find {relativePath} walking up from {AppContext.BaseDirectory}"
        );
    }
}

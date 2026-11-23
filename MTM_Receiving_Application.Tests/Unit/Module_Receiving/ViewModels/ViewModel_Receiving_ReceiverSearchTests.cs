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
/// FR-009: the search box filters on a contains match of the receiver number, clearing it restores
/// the whole list at page one, and a search that matches nothing reads as empty rather than broken.
/// </summary>
public sealed class ViewModel_Receiving_ReceiverSearchTests
{
    [Fact]
    public async Task SearchText_ShouldNarrowToContainingReceiverNumbers_WhenDigitsAreTyped()
    {
        var viewModel = await CreateLoadedViewModelAsync();

        viewModel.SearchText = "7100";
        await Task.Delay(25);

        viewModel
            .Rows.Should()
            .OnlyContain(row => row.ReceiverNumber.Contains("7100", StringComparison.Ordinal));
        viewModel.Rows.Should().NotBeEmpty();
        viewModel
            .Rows.Should()
            .NotContain(row => row.ReceiverNumber.Contains("8200", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SearchText_ShouldMatchAnywhereInTheNumber_WhenTheTermIsNotAPrefix()
    {
        var viewModel = await CreateLoadedViewModelAsync();

        viewModel.SearchText = "71003";
        await Task.Delay(25);

        viewModel.Rows.Should().NotBeEmpty();
        viewModel.Rows.Should().OnlyContain(row => row.ReceiverNumber.Contains("71003"));
    }

    [Fact]
    public async Task SearchText_ShouldRestoreTheWholeListAtPageOne_WhenItIsCleared()
    {
        var viewModel = await CreateLoadedViewModelAsync();
        viewModel.PageSize = 25;
        viewModel.GoToNextPageCommand.Execute(null);
        viewModel.GoToNextPageCommand.Execute(null);

        viewModel
            .CurrentPage.Should()
            .Be(3, "two forward pages from the first puts the list on the third");

        viewModel.SearchText = "7100";
        await Task.Delay(25);

        viewModel
            .CurrentPage.Should()
            .Be(1, "a search starts the results over at the first page");

        viewModel.SearchText = string.Empty;
        await Task.Delay(25);

        viewModel.TotalItems.Should().Be(120);
        viewModel.CurrentPage.Should().Be(1);
        viewModel.Rows.Should().HaveCount(25);
        viewModel.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task SearchText_ShouldShowTheEmptyMessageNotAnError_WhenNothingMatches()
    {
        var viewModel = await CreateLoadedViewModelAsync();

        viewModel.SearchText = "TEST-NO-SUCH-RECEIVER";
        await Task.Delay(25);

        viewModel.Rows.Should().BeEmpty();
        viewModel
            .IsEmpty.Should()
            .BeTrue("a search that matches nothing is an empty result, not a source failure");
        viewModel.IsFailed.Should().BeFalse();
        viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task TotalItems_ShouldFollowTheFilteredCount_WhenASearchIsNarrowing()
    {
        var viewModel = await CreateLoadedViewModelAsync();
        viewModel.TotalItems.Should().Be(120);

        viewModel.SearchText = "7100";
        await Task.Delay(25);

        viewModel
            .TotalItems.Should()
            .Be(60, "the pager counts the rows the search left, not the whole window");
        viewModel.TotalPages.Should().Be(2);
    }

    private static async Task<ViewModel_Receiving_ReceiverSelection> CreateLoadedViewModelAsync()
    {
        var workflow = new Mock<IService_ReceivingWorkflow>();
        workflow
            .SetupGet(service => service.CurrentStep)
            .Returns(Enum_ReceivingWorkflowStep.ReceiverSelection);

        var inforVisual = new Mock<IService_InforVisual>();
        inforVisual
            .Setup(service => service.GetRecentReceiversAsync(It.IsAny<int>()))
            .ReturnsAsync(Model_Dao_Result_Factory.Success(CreateSampleRows()));

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

    private static List<Model_InforVisualReceiverRow> CreateSampleRows()
    {
        var rows = new List<Model_InforVisualReceiverRow>();

        for (var index = 0; index < 60; index++)
        {
            rows.Add(
                new Model_InforVisualReceiverRow
                {
                    ReceiverNumber = $"TEST-7100{index:D2}",
                    PartNumber = $"TEST-PART-71{index:D2}",
                }
            );
        }

        for (var index = 0; index < 60; index++)
        {
            rows.Add(
                new Model_InforVisualReceiverRow
                {
                    ReceiverNumber = $"TEST-8200{index:D2}",
                    PartNumber = $"TEST-PART-82{index:D2}",
                }
            );
        }

        return rows
            .OrderByDescending(row => row.ReceiverNumber, StringComparer.Ordinal)
            .ToList();
    }
}

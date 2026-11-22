using FluentAssertions;
using Microsoft.UI.Xaml.Controls;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Settings.Dunnage.Views;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Dunnage.ViewModels;

public sealed class ViewModel_Settings_Dunnage_CategoryHubTests
{
    [Fact]
    public void Steps_ShouldOfferSixTitlesIncludingLabelFiles_AndEachTitleShouldResolveThePageItNames()
    {
        var pagination = new Mock<IService_SettingsPagination>();
        pagination
            .Setup(service => service.GetTotalPages(It.IsAny<int>()))
            .Returns<int>(count => count == 0 ? 0 : 1);
        pagination.Setup(service => service.ShouldShowPagination(It.IsAny<int>())).Returns(false);
        pagination
            .Setup(service => service.GetPageIndices(It.IsAny<int>(), It.IsAny<int>()))
            .Returns<int, int>((count, _) => Enumerable.Range(0, count).ToArray());

        var viewModel = new ViewModel_Settings_Dunnage_CategoryHub(
            pagination.Object,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );

        viewModel
            .Steps.Select(step => step.Title)
            .Should()
            .Equal(
                "Personal Defaults",
                "Image Assets",
                "Image Presentation",
                "Workflow Visuals",
                "Keyboard Shortcuts",
                "Label Files"
            );

        viewModel
            .Steps.Select(step => step.Title)
            .Should()
            .OnlyHaveUniqueItems("a card resolves its step by title, so two titles may not collide");

        foreach (var step in viewModel.Steps)
        {
            step.ViewType.Should().NotBeNull($"{step.Title} must name the page it opens");
            typeof(Page).IsAssignableFrom(step.ViewType).Should().BeTrue();
        }

        var labelFiles = viewModel.Steps.Single(step => step.Title == "Label Files");
        labelFiles.ViewType.Should().Be(typeof(View_Settings_Dunnage_LabelPaths));
    }
}

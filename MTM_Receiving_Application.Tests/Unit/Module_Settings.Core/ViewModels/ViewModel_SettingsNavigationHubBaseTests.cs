using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Core.ViewModels;

public sealed class ViewModel_SettingsNavigationHubBaseTests
{
    [Fact]
    public void SetSteps_ShouldReplaceStepsAndBuildFirstPage()
    {
        var viewModel = CreateViewModel();

        viewModel.SetSteps(
            new Model_SettingsNavigationStep("One", typeof(string)),
            new Model_SettingsNavigationStep("Two", typeof(int)),
            new Model_SettingsNavigationStep("Three", typeof(double))
        );

        viewModel.Steps.Should().HaveCount(3);
        viewModel.VisibleSteps.Should().HaveCount(2);
        viewModel.VisibleSteps.Select(step => step.Title).Should().ContainInOrder("One", "Two");
        viewModel.CurrentButtonPage.Should().Be(1);
    }

    [Fact]
    public void NextButtonPage_ShouldRebuildVisibleStepsForNextPage()
    {
        var viewModel = CreateViewModel();

        viewModel.SetSteps(
            new Model_SettingsNavigationStep("One", typeof(string)),
            new Model_SettingsNavigationStep("Two", typeof(int)),
            new Model_SettingsNavigationStep("Three", typeof(double))
        );

        viewModel.NextButtonPage();

        viewModel.CurrentButtonPage.Should().Be(2);
        viewModel.VisibleSteps.Should().ContainSingle();
        viewModel.VisibleSteps[0].Title.Should().Be("Three");
    }

    private static TestSettingsNavigationHubViewModel CreateViewModel()
    {
        return new TestSettingsNavigationHubViewModel(
            new FakeSettingsPaginationService(),
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
    }

    private sealed class TestSettingsNavigationHubViewModel : ViewModel_SettingsNavigationHubBase
    {
        public TestSettingsNavigationHubViewModel(
            IService_SettingsPagination pagination,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_Notification notificationService
        )
            : base(pagination, errorHandler, logger, notificationService) { }
    }

    private sealed class FakeSettingsPaginationService : IService_SettingsPagination
    {
        public int PageSize => 2;

        public bool ShouldShowPagination(int totalCount)
        {
            return totalCount > PageSize;
        }

        public int GetTotalPages(int totalCount)
        {
            return totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / PageSize);
        }

        public IReadOnlyList<int> GetPageIndices(int totalCount, int pageNumber)
        {
            if (totalCount <= 0 || pageNumber <= 0)
            {
                return [];
            }

            var startIndex = (pageNumber - 1) * PageSize;
            return Enumerable
                .Range(startIndex, PageSize)
                .Where(index => index >= 0 && index < totalCount)
                .ToArray();
        }
    }
}

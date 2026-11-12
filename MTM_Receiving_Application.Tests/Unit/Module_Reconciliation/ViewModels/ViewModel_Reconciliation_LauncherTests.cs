using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Reconciliation.Navigation;
using MTM_Receiving_Application.Module_Reconciliation.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Reconciliation.ViewModels;

/// <summary>
/// The shell resolves a page's header title from the page, its `DataContext` or its
/// `ViewModel` property. The launcher's `DataContext` is this view model, so the object
/// below is exactly what the shell tracks, and the title it reports comes through
/// `ReconciliationHeaderTitleProvider` rather than from a literal (FR-062, R-HDR-01).
/// </summary>
public sealed class ViewModel_Reconciliation_LauncherTests
{
    [Fact]
    public void TheLauncherShouldReportTheModulesHeaderTitleThroughItsProvider()
    {
        var provider = new ReconciliationHeaderTitleProvider();
        ViewModel_Reconciliation_Launcher viewModel = Build(provider);

        IViewModel_HeaderTitleProvider resolved = viewModel;

        resolved.CurrentHeaderTitle.Should().Be("Receiving Validator");
        resolved.CurrentHeaderContextSubtitle.Should().Be("from Infor Visual");
        viewModel.Title.Should().Be(provider.CurrentHeaderTitle);
    }

    private static ViewModel_Reconciliation_Launcher Build(
        ReconciliationHeaderTitleProvider provider
    ) =>
        new(
            provider,
            new Mock<IService_ErrorHandler>().Object,
            new Mock<IService_LoggingUtility>().Object,
            new Mock<IService_Notification>().Object
        );
}

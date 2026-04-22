using System.Collections.ObjectModel;
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

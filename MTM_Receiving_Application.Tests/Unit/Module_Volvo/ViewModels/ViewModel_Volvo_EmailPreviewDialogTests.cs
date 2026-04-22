using FluentAssertions;
using Moq;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.ViewModels;
using Xunit;

namespace MTM_Receiving_Application.Tests.Unit.Module_Volvo.ViewModels;

public sealed class ViewModel_Volvo_EmailPreviewDialogTests
{
    [Fact]
    public void Initialize_ShouldPopulateDialogState_AndExposeAdditionalNotesVisibility()
    {
        var viewModel = CreateViewModel();
        var formattedDocument = new Model_FormattedReportDocument
        {
            HtmlFragment = "<p>Preview</p>",
            PlainText = "Preview",
        };

        viewModel.Initialize(
            new Model_VolvoEmailPreviewDialog
            {
                DialogTitle = "Dialog Title",
                ToRecipients = "to@example.com",
                CcRecipients = "cc@example.com",
                Subject = "Subject",
                AdditionalNotes = "Operator notes",
                PreviewHtmlDocument = "<html></html>",
                PlainTextPreview = "plain text",
                FormattedEmailDocument = formattedDocument,
            }
        );

        viewModel.DialogTitle.Should().Be("Dialog Title");
        viewModel.ToRecipients.Should().Be("to@example.com");
        viewModel.CcRecipients.Should().Be("cc@example.com");
        viewModel.Subject.Should().Be("Subject");
        viewModel.AdditionalNotes.Should().Be("Operator notes");
        viewModel.PreviewHtmlDocument.Should().Be("<html></html>");
        viewModel.PlainTextPreview.Should().Be("plain text");
        viewModel.HasAdditionalNotes.Should().BeTrue();
    }

    [Fact]
    public async Task CopyEmailBodyAsync_ShouldReturnFalse_WhenClipboardPackageCannotBeCreated()
    {
        var clipboardMock = new Mock<IService_ReportingClipboard>();
        clipboardMock
            .Setup(service =>
                service.CreateClipboardPackage(It.IsAny<Model_FormattedReportDocument>())
            )
            .Returns(
                Model_Dao_Result_Factory.Failure<Windows.ApplicationModel.DataTransfer.DataPackage>(
                    "clipboard failed"
                )
            );

        var viewModel = CreateViewModel(clipboardMock);

        viewModel.Initialize(
            new Model_VolvoEmailPreviewDialog
            {
                FormattedEmailDocument = new Model_FormattedReportDocument(),
            }
        );

        var result = await viewModel.CopyEmailBodyAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public void RecipientCommands_ShouldOnlyBeEnabled_WhenRecipientsContainText()
    {
        var viewModel = CreateViewModel();

        viewModel.CopyToRecipientsCommand.CanExecute(null).Should().BeFalse();
        viewModel.CopyCcRecipientsCommand.CanExecute(null).Should().BeFalse();

        viewModel.ToRecipients = "to@example.com";
        viewModel.CcRecipients = "cc@example.com";

        viewModel.CopyToRecipientsCommand.CanExecute(null).Should().BeTrue();
        viewModel.CopyCcRecipientsCommand.CanExecute(null).Should().BeTrue();

        viewModel.ToRecipients = "   ";
        viewModel.CcRecipients = string.Empty;

        viewModel.CopyToRecipientsCommand.CanExecute(null).Should().BeFalse();
        viewModel.CopyCcRecipientsCommand.CanExecute(null).Should().BeFalse();
    }

    private static ViewModel_Volvo_EmailPreviewDialog CreateViewModel(
        Mock<IService_ReportingClipboard>? clipboardMock = null
    )
    {
        return new ViewModel_Volvo_EmailPreviewDialog(
            (clipboardMock ?? new Mock<IService_ReportingClipboard>()).Object,
            new Mock<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_ErrorHandler>().Object,
            new Mock<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_LoggingUtility>().Object,
            new Mock<MTM_Receiving_Application.Module_Core.Contracts.Services.IService_Notification>().Object
        );
    }
}

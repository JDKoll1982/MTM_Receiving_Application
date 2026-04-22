using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Models;
using Windows.ApplicationModel.DataTransfer;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for the Volvo PO requisition email preview dialog.
/// </summary>
public partial class ViewModel_Volvo_EmailPreviewDialog : ViewModel_Shared_Base
{
    private readonly IService_ReportingClipboard _reportingClipboard;
    private Model_FormattedReportDocument _formattedEmailDocument = new();

    [ObservableProperty]
    private string _dialogTitle = "PO Requisition Email Preview";

    [ObservableProperty]
    private string _toRecipients = string.Empty;

    [ObservableProperty]
    private string _ccRecipients = string.Empty;

    [ObservableProperty]
    private string _subject = string.Empty;

    [ObservableProperty]
    private string? _additionalNotes;

    [ObservableProperty]
    private string _previewHtmlDocument = string.Empty;

    [ObservableProperty]
    private string _plainTextPreview = string.Empty;

    /// <summary>
    /// Gets a value indicating whether additional notes should be shown.
    /// </summary>
    public bool HasAdditionalNotes => !string.IsNullOrWhiteSpace(AdditionalNotes);

    public ViewModel_Volvo_EmailPreviewDialog(
        IService_ReportingClipboard reportingClipboard,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _reportingClipboard =
            reportingClipboard ?? throw new ArgumentNullException(nameof(reportingClipboard));
    }

    /// <summary>
    /// Loads the prepared dialog model into the ViewModel.
    /// </summary>
    /// <param name="dialogModel"></param>
    public void Initialize(Model_VolvoEmailPreviewDialog dialogModel)
    {
        ArgumentNullException.ThrowIfNull(dialogModel);

        DialogTitle = dialogModel.DialogTitle;
        ToRecipients = dialogModel.ToRecipients;
        CcRecipients = dialogModel.CcRecipients;
        Subject = dialogModel.Subject;
        AdditionalNotes = dialogModel.AdditionalNotes;
        PreviewHtmlDocument = dialogModel.PreviewHtmlDocument;
        PlainTextPreview = dialogModel.PlainTextPreview;
        _formattedEmailDocument = dialogModel.FormattedEmailDocument ?? new();

        OnPropertyChanged(nameof(HasAdditionalNotes));
    }

    /// <summary>
    /// Copies the formatted email body to the clipboard.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CopyEmailBodyAsync()
    {
        try
        {
            var clipboardResult = _reportingClipboard.CreateClipboardPackage(
                _formattedEmailDocument
            );
            if (!clipboardResult.IsSuccess || clipboardResult.Data is null)
            {
                await _errorHandler.ShowUserErrorAsync(
                    clipboardResult.ErrorMessage ?? "Failed to create email clipboard content.",
                    "Email Preview",
                    nameof(CopyEmailBodyAsync)
                );
                return false;
            }

            Clipboard.SetContent(clipboardResult.Data);
            return true;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(CopyEmailBodyAsync),
                nameof(ViewModel_Volvo_EmailPreviewDialog)
            );
            return false;
        }
    }

    /// <summary>
    /// Handles a WebView preview rendering failure by logging and showing a fallback warning.
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public async Task HandlePreviewRenderFailureAsync(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        await _logger.LogErrorAsync($"Error loading email preview web view: {ex.Message}", ex);
        await _errorHandler.ShowUserErrorAsync(
            "The formatted email preview could not be rendered. A plain-text preview is shown instead.",
            "Email Preview",
            nameof(HandlePreviewRenderFailureAsync)
        );
    }

    [RelayCommand(CanExecute = nameof(CanCopyToRecipients))]
    private async Task CopyToRecipientsAsync()
    {
        await CopyTextToClipboardAsync(
            ToRecipients,
            "To recipients",
            nameof(CopyToRecipientsAsync)
        );
    }

    [RelayCommand(CanExecute = nameof(CanCopyCcRecipients))]
    private async Task CopyCcRecipientsAsync()
    {
        await CopyTextToClipboardAsync(
            CcRecipients,
            "CC recipients",
            nameof(CopyCcRecipientsAsync)
        );
    }

    [RelayCommand]
    private async Task CopySubjectAsync()
    {
        await CopyTextToClipboardAsync(Subject, "email subject", nameof(CopySubjectAsync));
    }

    partial void OnToRecipientsChanged(string value)
    {
        CopyToRecipientsCommand.NotifyCanExecuteChanged();
    }

    partial void OnCcRecipientsChanged(string value)
    {
        CopyCcRecipientsCommand.NotifyCanExecuteChanged();
    }

    private bool CanCopyToRecipients() => !string.IsNullOrWhiteSpace(ToRecipients);

    private bool CanCopyCcRecipients() => !string.IsNullOrWhiteSpace(CcRecipients);

    private async Task CopyTextToClipboardAsync(string text, string description, string methodName)
    {
        try
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text ?? string.Empty);
            Clipboard.SetContent(dataPackage);
            await _logger.LogInfoAsync($"Copied {description} to clipboard.");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                methodName,
                nameof(ViewModel_Volvo_EmailPreviewDialog)
            );
        }
    }
}

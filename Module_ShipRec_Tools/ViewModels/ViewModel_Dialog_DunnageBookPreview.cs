using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the dunnage book preview dialog (WebView2 host with a fallback).
/// </summary>
public partial class ViewModel_Dialog_DunnageBookPreview : ObservableObject
{
    private readonly IService_ErrorHandler _errorHandler;
    private readonly IService_LoggingUtility _logger;
    private Model_FormattedReportDocument _document = new();

    public Func<Model_FormattedReportDocument, Task<Model_Dao_Result<bool>>>? RequestPrintAsync
    {
        get;
        set;
    }

    public string DialogTitle =>
        string.IsNullOrWhiteSpace(_document.DocumentTitle)
            ? "Dunnage Book Preview"
            : _document.DocumentTitle;

    public string PreviewHtmlDocument { get; private set; } = string.Empty;

    public string PlainTextPreview => _document.PlainText;

    public ViewModel_Dialog_DunnageBookPreview(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger
    )
    {
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Initialize(Model_FormattedReportDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        PreviewHtmlDocument = Helper_PrintDocumentHtml.BuildPage(document, autoPrint: false);

        OnPropertyChanged(nameof(DialogTitle));
        OnPropertyChanged(nameof(PreviewHtmlDocument));
        OnPropertyChanged(nameof(PlainTextPreview));
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        if (RequestPrintAsync is null)
        {
            return;
        }

        var printResult = await RequestPrintAsync(_document);
        if (!printResult.IsSuccess || !printResult.Data)
        {
            await _errorHandler.ShowUserErrorAsync(
                printResult.ErrorMessage,
                "Dunnage Book Print",
                nameof(PrintAsync)
            );
        }
    }

    public Task HandlePreviewRenderFailureAsync(Exception exception)
    {
        _logger.LogWarning(
            $"WebView2 preview unavailable; falling back to plain text. {exception.Message}"
        );
        return Task.CompletedTask;
    }
}

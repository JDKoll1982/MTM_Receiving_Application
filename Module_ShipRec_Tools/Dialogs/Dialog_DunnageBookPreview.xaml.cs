using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using Windows.UI;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;

/// <summary>
/// Dialog showing the generated dunnage book in a WebView2 preview with a print action.
/// </summary>
public sealed partial class Dialog_DunnageBookPreview : ContentDialog
{
    public ViewModel_Dialog_DunnageBookPreview ViewModel { get; }

    public Dialog_DunnageBookPreview(ViewModel_Dialog_DunnageBookPreview viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
        ViewModel.RequestPrintAsync = OpenPrintDocumentAsync;
        Opened += OnDialogOpened;
    }

    private async void OnDialogOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        await LoadPreviewAsync();
    }

    private async Task LoadPreviewAsync()
    {
        try
        {
            BookPreview.DefaultBackgroundColor = Color.FromArgb(255, 255, 255, 255);
            await BookPreview.EnsureCoreWebView2Async();
            BookPreview.NavigateToString(ViewModel.PreviewHtmlDocument);
        }
        catch (Exception ex)
        {
            BookPreview.Visibility = Visibility.Collapsed;
            BookFallbackBox.Visibility = Visibility.Visible;
            await ViewModel.HandlePreviewRenderFailureAsync(ex);
        }
    }

    private static async Task<Model_Dao_Result<bool>> OpenPrintDocumentAsync(
        Model_FormattedReportDocument document
    )
    {
        try
        {
            ArgumentNullException.ThrowIfNull(document);

            var filePath = Path.Combine(
                Path.GetTempPath(),
                $"mtm-dunnage-book-{DateTime.Now:yyyyMMdd-HHmmss}.html"
            );
            var printablePage = Helper_PrintDocumentHtml.BuildPage(document, autoPrint: true);

            await File.WriteAllTextAsync(filePath, printablePage, Encoding.UTF8);
            Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });

            return Model_Dao_Result_Factory.Success(true);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<bool>(
                $"Failed to open a print-ready dunnage book: {ex.Message}",
                ex
            );
        }
    }
}

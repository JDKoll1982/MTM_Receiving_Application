using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.ViewModels;
using Windows.UI;

namespace MTM_Receiving_Application.Module_Volvo.Views;

/// <summary>
/// Dialog showing the prepared Volvo PO requisition email preview.
/// </summary>
public sealed partial class View_Volvo_EmailPreviewDialog : ContentDialog
{
    public ViewModel_Volvo_EmailPreviewDialog ViewModel { get; }

    public View_Volvo_EmailPreviewDialog()
    {
        ViewModel = App.GetService<ViewModel_Volvo_EmailPreviewDialog>();
        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);
        Opened += OnDialogOpened;
    }

    /// <summary>
    /// Loads the prepared dialog model into the dialog viewmodel.
    /// </summary>
    /// <param name="dialogModel"></param>
    public void Initialize(Model_VolvoEmailPreviewDialog dialogModel)
    {
        ViewModel.Initialize(dialogModel);
    }

    /// <summary>
    /// Copies the formatted email body to the clipboard.
    /// </summary>
    /// <returns></returns>
    public Task<bool> CopyEmailBodyAsync()
    {
        return ViewModel.CopyEmailBodyAsync();
    }

    private async void OnDialogOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        await LoadPreviewAsync();
    }

    private async Task LoadPreviewAsync()
    {
        try
        {
            EmailBodyPreview.DefaultBackgroundColor = Color.FromArgb(255, 255, 255, 255);
            await EmailBodyPreview.EnsureCoreWebView2Async();
            EmailBodyPreview.NavigateToString(ViewModel.PreviewHtmlDocument);
        }
        catch (Exception ex)
        {
            EmailBodyPreview.Visibility = Visibility.Collapsed;
            EmailBodyFallbackBox.Visibility = Visibility.Visible;
            await ViewModel.HandlePreviewRenderFailureAsync(ex);
        }
    }
}

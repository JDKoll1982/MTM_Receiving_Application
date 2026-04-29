using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Views;

/// <summary>
/// Dedicated modal window host for Volvo shipment history details.
/// </summary>
public sealed class View_Volvo_ShipmentHistoryDetailWindow : Window
{
    private readonly View_Volvo_ShipmentHistoryDetailDialog _contentPage;

    private UIElement? _ownerContentRoot;
    private TaskCompletionSource<bool>? _showTaskCompletionSource;

    public View_Volvo_ShipmentHistoryDetailWindow(
        View_Volvo_ShipmentHistoryDetailDialog contentPage
    )
    {
        _contentPage = contentPage;
        _contentPage.CloseRequested += OnCloseRequested;
        Content = _contentPage;

        if (
            Content is FrameworkElement windowContent
            && App.MainWindow?.Content is FrameworkElement mainContent
        )
        {
            windowContent.RequestedTheme = mainContent.RequestedTheme;
        }

        this.ApplySharedIcon();
        ExtendsContentIntoTitleBar = true;
        _contentPage.Loaded += OnContentPageLoaded;
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 1180, 860);
        Closed += OnWindowClosed;
    }

    public void Initialize(Model_VolvoShipmentHistoryDetailDialog dialogModel)
    {
        _contentPage.Initialize(dialogModel);
        Title = dialogModel.WindowTitle;
    }

    public Task ShowModalAsync()
    {
        _showTaskCompletionSource = new TaskCompletionSource<bool>();
        _ownerContentRoot = App.MainWindow?.Content as UIElement;

        if (_ownerContentRoot != null)
        {
            _ownerContentRoot.IsHitTestVisible = false;
        }

        Activate();
        return _showTaskCompletionSource.Task;
    }

    private void OnContentPageLoaded(object sender, RoutedEventArgs e)
    {
        if (_contentPage.FindName("AppTitleBar") is UIElement appTitleBar)
        {
            SetTitleBar(appTitleBar);
        }
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        _contentPage.CloseRequested -= OnCloseRequested;
        _contentPage.Loaded -= OnContentPageLoaded;

        if (_ownerContentRoot != null)
        {
            _ownerContentRoot.IsHitTestVisible = true;
            _ownerContentRoot = null;
        }

        _showTaskCompletionSource?.TrySetResult(true);
        _showTaskCompletionSource = null;
    }
}

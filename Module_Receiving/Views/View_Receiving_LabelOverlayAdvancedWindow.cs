using System;
using System.Threading.Tasks;
using DevWinUI;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using WinRT.Interop;

namespace MTM_Receiving_Application.Module_Receiving.Views;

/// <summary>
/// Modal window host for the overlay's advanced setup, owned by the label overlay window so the tag list
/// behind it cannot be changed while the geometry is being measured.
/// </summary>
public sealed class View_Receiving_LabelOverlayAdvancedWindow : ModalWindow
{
    private readonly View_Receiving_LabelOverlayAdvancedDialog _contentPage;

    private TaskCompletionSource<bool>? _showTaskCompletionSource;

    public View_Receiving_LabelOverlayAdvancedWindow(
        ViewModel_Receiving_LabelOverlay viewModel,
        IntPtr parentHwnd
    )
        : base(parentHwnd)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _contentPage = new View_Receiving_LabelOverlayAdvancedDialog(viewModel);
        _contentPage.CloseRequested += OnCloseRequested;

        Content = _contentPage;

        if (App.MainWindow?.Content is FrameworkElement mainContent)
        {
            _contentPage.RequestedTheme = mainContent.RequestedTheme;
        }

        this.ApplySharedIcon();
        Title = "Advanced";
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 620, 460);

        Closed += OnWindowClosed;
    }

    /// <summary>Brings the window to the front and returns when it is closed.</summary>
    public Task ShowModalAsync()
    {
        _showTaskCompletionSource = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        Activate();
        return _showTaskCompletionSource.Task;
    }

    private void OnCloseRequested(object? sender, EventArgs e) => Close();

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        Closed -= OnWindowClosed;
        _contentPage.CloseRequested -= OnCloseRequested;

        _showTaskCompletionSource?.TrySetResult(true);
        _showTaskCompletionSource = null;
    }
}

using System;
using System.Threading.Tasks;
using DevWinUI;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using WinRT.Interop;

namespace MTM_Receiving_Application.Module_Receiving.Views;

/// <summary>
/// Modal window host for the label overlay mode.
/// </summary>
/// <remarks>
/// A DevWinUI <see cref="ModalWindow"/>, so Windows disables the window behind it while the operator is
/// choosing tags — which matters here because a stray click on the receiving screen must not change the
/// receipt the tags belong to while they are being printed.
/// </remarks>
public sealed class View_Receiving_LabelOverlayWindow : ModalWindow
{
    private readonly View_Receiving_LabelOverlayDialog _contentPage;

    private TaskCompletionSource<bool>? _showTaskCompletionSource;

    public View_Receiving_LabelOverlayWindow(
        ViewModel_Receiving_LabelOverlay viewModel,
        IntPtr parentHwnd
    )
        : base(parentHwnd)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _contentPage = new View_Receiving_LabelOverlayDialog(viewModel);
        _contentPage.CloseRequested += OnCloseRequested;

        // The printer's own dialog is owned by this window; the ViewModel does not hold window handles.
        viewModel.PrinterSetupOwnerHandle = WindowNative.GetWindowHandle(this);

        Content = _contentPage;

        if (App.MainWindow?.Content is FrameworkElement mainContent)
        {
            _contentPage.RequestedTheme = mainContent.RequestedTheme;
        }

        this.ApplySharedIcon();
        Title = "Label Overlay";
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 900, 920);

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

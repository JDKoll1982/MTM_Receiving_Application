using System;
using System.Threading.Tasks;
using DevWinUI;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Modal window host for changing one lot's marking.
/// </summary>
/// <remarks>
/// This is a DevWinUI <see cref="ModalWindow"/>, so the window behind it is disabled by Windows for as
/// long as it is open, and the system title bar is left in place so the window always shows its caption
/// buttons.
/// </remarks>
public sealed class View_Tool_LotMarkingChangeWindow : ModalWindow
{
    private readonly ViewModel_Tool_LotMarking _viewModel;
    private readonly View_Tool_LotMarkingChangeDialog _contentPage;

    private TaskCompletionSource<bool>? _showTaskCompletionSource;

    public View_Tool_LotMarkingChangeWindow(ViewModel_Tool_LotMarking viewModel, IntPtr parentHwnd)
        : base(parentHwnd)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        _contentPage = new View_Tool_LotMarkingChangeDialog(viewModel);
        _contentPage.CloseRequested += OnCloseRequested;
        _viewModel.RequestCloseChangeMarking = OnCloseChangeMarkingRequested;
        Content = _contentPage;

        if (App.MainWindow?.Content is FrameworkElement mainContent)
        {
            _contentPage.RequestedTheme = mainContent.RequestedTheme;
        }

        this.ApplySharedIcon();
        Title = "Change this marking";
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 560, 620);

        Closed += OnWindowClosed;
    }

    /// <summary>
    /// Brings the window to the front and returns when it is closed.
    /// </summary>
    public Task ShowModalAsync()
    {
        // Continuations run off the close event rather than inside it, so the work that follows a
        // close starts only once the window has finished tearing down.
        _showTaskCompletionSource = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        Activate();
        return _showTaskCompletionSource.Task;
    }

    private void OnCloseRequested(object? sender, EventArgs e) => Close();

    private void OnCloseChangeMarkingRequested() => Close();

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        Closed -= OnWindowClosed;
        _contentPage.CloseRequested -= OnCloseRequested;
        _viewModel.RequestCloseChangeMarking = null;

        _showTaskCompletionSource?.TrySetResult(true);
        _showTaskCompletionSource = null;
    }
}

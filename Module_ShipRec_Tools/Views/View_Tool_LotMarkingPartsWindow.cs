using System;
using System.Threading.Tasks;
using DevWinUI;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Modal window host for the part numbers that receive markings.
/// </summary>
/// <remarks>
/// This is a DevWinUI <see cref="ModalWindow"/>, so the window behind it is disabled by Windows for as
/// long as it is open. That is what makes the main window unreachable while the list is being edited,
/// and it is the operating system doing the blocking rather than this application switching hit
/// testing off, so the main window cannot be left unable to take clicks once this window closes.
/// The system title bar is left in place rather than extending content into it, so the window always
/// shows a title bar with its caption buttons.
/// </remarks>
public sealed class View_Tool_LotMarkingPartsWindow : ModalWindow
{
    private readonly View_Tool_LotMarkingPartsDialog _contentPage;

    private TaskCompletionSource<bool>? _showTaskCompletionSource;

    public View_Tool_LotMarkingPartsWindow(ViewModel_Tool_LotMarking viewModel, IntPtr parentHwnd)
        : base(parentHwnd)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _contentPage = new View_Tool_LotMarkingPartsDialog(viewModel);
        Content = _contentPage;

        if (App.MainWindow?.Content is FrameworkElement mainContent)
        {
            _contentPage.RequestedTheme = mainContent.RequestedTheme;
        }

        this.ApplySharedIcon();
        Title = "Part numbers that receive markings";
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 760, 620);

        Closed += OnWindowClosed;
    }

    /// <summary>
    /// Brings the window to the front and returns when it is closed. The window itself is already
    /// modal, so nothing here has to stop the main window from being used.
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

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        Closed -= OnWindowClosed;

        // The page's dialogs close with the window, so the next window sets its own.
        _contentPage.ViewModel.ShowFuzzyPickerAsync = null;
        _contentPage.ViewModel.ConfirmBackfillAsync = null;

        _showTaskCompletionSource?.TrySetResult(true);
        _showTaskCompletionSource = null;
    }
}

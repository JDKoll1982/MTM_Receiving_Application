using System;
using System.Threading.Tasks;
using DevWinUI;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// Modal window host for the lot list's filter, order and column choices.
/// </summary>
/// <remarks>
/// A DevWinUI <see cref="ModalWindow"/>, so Windows disables the tool page for as long as it is open
/// and re-enables it on close. The system title bar is left in place so the window always shows its
/// caption buttons.
/// </remarks>
public sealed class View_Tool_LotMarkingFilterWindow : ModalWindow
{
    private readonly View_Tool_LotMarkingFilterDialog _contentPage;

    private TaskCompletionSource<bool>? _showTaskCompletionSource;
    private bool _isApplyRequested;

    public View_Tool_LotMarkingFilterWindow(ViewModel_Tool_LotMarking viewModel, IntPtr parentHwnd)
        : base(parentHwnd)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _contentPage = new View_Tool_LotMarkingFilterDialog(viewModel);
        _contentPage.CloseRequested += OnCloseRequested;
        _contentPage.ApplyRequested += OnApplyRequested;
        Content = _contentPage;

        if (App.MainWindow?.Content is FrameworkElement mainContent)
        {
            _contentPage.RequestedTheme = mainContent.RequestedTheme;
        }

        this.ApplySharedIcon();
        Title = "Filter the lot list";
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 620, 760);

        Closed += OnWindowClosed;
    }

    /// <summary>
    /// Brings the window to the front and returns when it is closed, reporting whether the user asked
    /// to apply the choices. The caller applies them afterwards, on the tool page, rather than from
    /// inside the close event.
    /// </summary>
    public Task<bool> ShowModalAsync()
    {
        _showTaskCompletionSource = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        Activate();
        return _showTaskCompletionSource.Task;
    }

    private void OnApplyRequested(object? sender, EventArgs e)
    {
        _isApplyRequested = true;
        Close();
    }

    private void OnCloseRequested(object? sender, EventArgs e) => Close();

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        Closed -= OnWindowClosed;
        _contentPage.CloseRequested -= OnCloseRequested;
        _contentPage.ApplyRequested -= OnApplyRequested;

        _showTaskCompletionSource?.TrySetResult(_isApplyRequested);
        _showTaskCompletionSource = null;
    }
}

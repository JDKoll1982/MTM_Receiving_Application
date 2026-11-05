using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views;

/// <summary>
/// The label overlay mode's screen: choose the printer and the tags, check where the marking lands, then
/// print. Hosted in its own modal window from the receiving mode selection.
/// </summary>
public sealed partial class View_Receiving_LabelOverlayDialog : Page
{
    public ViewModel_Receiving_LabelOverlay ViewModel { get; }

    public event EventHandler? CloseRequested;

    /// <summary>
    /// The handle of the window this page is hosted in, so the advanced setup can open as a modal child of
    /// it. Set by <see cref="View_Receiving_LabelOverlayWindow"/>; zero means the page is not in a window yet.
    /// </summary>
    public IntPtr AdvancedOwnerHandle { get; set; }

    public View_Receiving_LabelOverlayDialog(ViewModel_Receiving_LabelOverlay viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();

        // The first control to take focus would otherwise scroll the window to itself, opening the
        // screen part-way down with the printer list cut off.
        Loaded += OnDialogLoaded;
    }

    private async void OnDialogLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnDialogLoaded;
        await ViewModel.ActivateAsync();
    }

    private async void OnPrintClick(object sender, RoutedEventArgs e) => await ViewModel.PrintAsync();

    /// <summary>
    /// Opens the advanced setup over this window. Nothing is reloaded when it closes: the fields are bound
    /// to the same view model, so the marking reflects the change straight away.
    /// </summary>
    private async void OnAdvancedClick(object sender, RoutedEventArgs e)
    {
        if (AdvancedOwnerHandle == IntPtr.Zero)
        {
            return;
        }

        var window = new View_Receiving_LabelOverlayAdvancedWindow(
            ViewModel,
            AdvancedOwnerHandle
        );

        await window.ShowModalAsync();
    }

    /// <summary>
    /// Bridges the app's own severity enum, which the view models speak, to the one the InfoBar control
    /// expects.
    /// </summary>
    internal InfoBarSeverity ToInfoBarSeverity(
        Module_Core.Models.Enums.InfoBarSeverity severity
    ) =>
        severity switch
        {
            Module_Core.Models.Enums.InfoBarSeverity.Success => InfoBarSeverity.Success,
            Module_Core.Models.Enums.InfoBarSeverity.Warning => InfoBarSeverity.Warning,
            Module_Core.Models.Enums.InfoBarSeverity.Error => InfoBarSeverity.Error,
            _ => InfoBarSeverity.Informational,
        };

    private void OnCloseClick(object sender, RoutedEventArgs e) =>
        CloseRequested?.Invoke(this, EventArgs.Empty);
}

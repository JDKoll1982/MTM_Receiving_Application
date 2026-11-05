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

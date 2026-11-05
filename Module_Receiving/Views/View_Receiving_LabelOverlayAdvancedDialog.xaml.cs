using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.Views;

/// <summary>
/// The overlay's advanced setup: where the marking lands on the tag, kept out of the main screen because it
/// is only touched when a tag comes out of the printer sitting wrong.
/// </summary>
public sealed partial class View_Receiving_LabelOverlayAdvancedDialog : Page
{
    public ViewModel_Receiving_LabelOverlay ViewModel { get; }

    public event EventHandler? CloseRequested;

    public View_Receiving_LabelOverlayAdvancedDialog(ViewModel_Receiving_LabelOverlay viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) =>
        CloseRequested?.Invoke(this, EventArgs.Empty);
}

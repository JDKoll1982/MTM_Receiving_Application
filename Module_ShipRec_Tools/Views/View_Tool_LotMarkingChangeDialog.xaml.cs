using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Views;

/// <summary>
/// The marking editor, hosted inside its own modal window. It reads and writes the lot the user
/// picked on the heat / lot list, so the selection is made there and the change is made here.
/// </summary>
public sealed partial class View_Tool_LotMarkingChangeDialog : Page
{
    public ViewModel_Tool_LotMarking ViewModel { get; }

    public event EventHandler? CloseRequested;

    public View_Tool_LotMarkingChangeDialog(ViewModel_Tool_LotMarking viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        ViewModel = viewModel;
        DataContext = ViewModel;
        InitializeComponent();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) =>
        CloseRequested?.Invoke(this, EventArgs.Empty);
}

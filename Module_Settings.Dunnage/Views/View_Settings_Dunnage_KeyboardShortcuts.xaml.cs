using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.Views;

public sealed partial class View_Settings_Dunnage_KeyboardShortcuts : Page
{
    public ViewModel_Settings_Dunnage_KeyboardShortcuts ViewModel { get; }

    public View_Settings_Dunnage_KeyboardShortcuts(
        ViewModel_Settings_Dunnage_KeyboardShortcuts viewModel
    )
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

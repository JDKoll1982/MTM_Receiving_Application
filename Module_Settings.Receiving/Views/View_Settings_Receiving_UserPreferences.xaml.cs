using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_UserPreferences : Page
{
    public ViewModel_Settings_Receiving_UserPreferences ViewModel { get; }

    public View_Settings_Receiving_UserPreferences(ViewModel_Settings_Receiving_UserPreferences viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

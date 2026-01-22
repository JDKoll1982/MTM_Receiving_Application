using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.Views;

public sealed partial class View_Settings_Dunnage_UiUx : Page
{
    public ViewModel_Settings_Dunnage_UiUx ViewModel { get; }

    public View_Settings_Dunnage_UiUx(ViewModel_Settings_Dunnage_UiUx viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_Database : Page
{
    public ViewModel_Settings_Database ViewModel { get; }

    public View_Settings_Database(ViewModel_Settings_Database viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

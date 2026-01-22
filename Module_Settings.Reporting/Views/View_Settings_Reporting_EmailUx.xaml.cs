using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.Views;

public sealed partial class View_Settings_Reporting_EmailUx : Page
{
    public ViewModel_Settings_Reporting_EmailUx ViewModel { get; }

    public View_Settings_Reporting_EmailUx(ViewModel_Settings_Reporting_EmailUx viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

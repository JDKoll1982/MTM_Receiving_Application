using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.Views;

public sealed partial class View_Settings_Reporting_SettingsOverview : Page
{
    public ViewModel_Settings_Reporting_SettingsOverview ViewModel { get; }

    public View_Settings_Reporting_SettingsOverview(ViewModel_Settings_Reporting_SettingsOverview viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

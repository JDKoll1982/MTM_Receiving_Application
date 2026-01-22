using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_SettingsOverview : Page
{
    public ViewModel_Settings_Receiving_SettingsOverview ViewModel { get; }

    public View_Settings_Receiving_SettingsOverview(ViewModel_Settings_Receiving_SettingsOverview viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void OnDefaultsClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(View_Settings_Receiving_Defaults));
    }

    private void OnValidationClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(View_Settings_Receiving_Validation));
    }

    private void OnBusinessRulesClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(View_Settings_Receiving_BusinessRules));
    }

    private void OnIntegrationsClicked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(View_Settings_Receiving_Integrations));
    }
}

using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Routing.Views;

/// <summary>
/// Settings overview page for the Routing module.
/// </summary>
public sealed partial class View_Settings_Routing_SettingsOverview : Page
{
    /// <summary>
    /// Gets the view model for this view.
    /// </summary>
    public ViewModel_Settings_Routing_SettingsOverview ViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the View_Settings_Routing_SettingsOverview class.
    /// </summary>
    /// <param name="viewModel">The view model to bind to this view.</param>
    public View_Settings_Routing_SettingsOverview(ViewModel_Settings_Routing_SettingsOverview viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        ViewModel = viewModel;
        
        InitializeComponent();
        
        DataContext = ViewModel;
    }
}

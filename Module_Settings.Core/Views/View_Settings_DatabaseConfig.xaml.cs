using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

/// <summary>
/// Code-behind for the Database Config page. The page is pure binding, so every behaviour lives in
/// the ViewModel.
/// </summary>
public sealed partial class View_Settings_DatabaseConfig : Page
{
    /// <summary>Gets the ViewModel for this page.</summary>
    public ViewModel_Settings_DatabaseConfig ViewModel { get; }

    public View_Settings_DatabaseConfig(ViewModel_Settings_DatabaseConfig viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }
}

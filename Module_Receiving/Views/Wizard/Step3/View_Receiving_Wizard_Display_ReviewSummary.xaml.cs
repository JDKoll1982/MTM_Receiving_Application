using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step3;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step3;

/// <summary>
/// UserControl for displaying read-only review summary of all loads.
/// </summary>
public sealed partial class View_Receiving_Wizard_Display_ReviewSummary : UserControl
{
    public ViewModel_Receiving_Wizard_Display_ReviewSummary ViewModel { get; }

    /// <summary>
    /// Initializes the Review Summary view with DI.
    /// </summary>
    public View_Receiving_Wizard_Display_ReviewSummary(ViewModel_Receiving_Wizard_Display_ReviewSummary viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    /// <summary>
    /// Parameterless constructor for XAML instantiation.
    /// Uses Service Locator temporarily until XAML supports constructor injection.
    /// </summary>
    public View_Receiving_Wizard_Display_ReviewSummary()
    {
        ViewModel = App.GetService<ViewModel_Receiving_Wizard_Display_ReviewSummary>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

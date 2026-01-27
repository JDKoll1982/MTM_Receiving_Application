using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step2;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step2;

/// <summary>
/// UserControl for load details data grid entry.
/// </summary>
public sealed partial class View_Receiving_Wizard_Display_LoadDetailsGrid : UserControl
{
    public ViewModel_Receiving_Wizard_Display_LoadDetailsGrid ViewModel { get; }

    /// <summary>
    /// Initializes the Load Details Grid view with DI.
    /// </summary>
    public View_Receiving_Wizard_Display_LoadDetailsGrid(ViewModel_Receiving_Wizard_Display_LoadDetailsGrid viewModel)
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
    public View_Receiving_Wizard_Display_LoadDetailsGrid()
    {
        ViewModel = App.GetService<ViewModel_Receiving_Wizard_Display_LoadDetailsGrid>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step1;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step1;

/// <summary>
/// UserControl for part number selection and part details display.
/// </summary>
public sealed partial class View_Receiving_Wizard_Display_PartSelection : UserControl
{
    public ViewModel_Receiving_Wizard_Display_PartSelection ViewModel { get; }

    /// <summary>
    /// Initializes the Part Selection view with DI.
    /// </summary>
    public View_Receiving_Wizard_Display_PartSelection(ViewModel_Receiving_Wizard_Display_PartSelection viewModel)
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
    public View_Receiving_Wizard_Display_PartSelection()
    {
        ViewModel = App.GetService<ViewModel_Receiving_Wizard_Display_PartSelection>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

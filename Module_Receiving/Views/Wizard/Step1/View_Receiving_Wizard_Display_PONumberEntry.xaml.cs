using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step1;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step1;

/// <summary>
/// UserControl for PO Number entry and validation.
/// </summary>
public sealed partial class View_Receiving_Wizard_Display_PONumberEntry : UserControl
{
    public ViewModel_Receiving_Wizard_Display_PONumberEntry ViewModel { get; }

    /// <summary>
    /// Initializes the PO Number Entry view with DI.
    /// </summary>
    public View_Receiving_Wizard_Display_PONumberEntry(ViewModel_Receiving_Wizard_Display_PONumberEntry viewModel)
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
    public View_Receiving_Wizard_Display_PONumberEntry()
    {
        ViewModel = App.GetService<ViewModel_Receiving_Wizard_Display_PONumberEntry>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

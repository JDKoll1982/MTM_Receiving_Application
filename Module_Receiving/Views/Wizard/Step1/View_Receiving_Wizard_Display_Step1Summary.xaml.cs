using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step1;

namespace MTM_Receiving_Application.Module_Receiving.Views.Wizard.Step1;

/// <summary>
/// UserControl displaying Step 1 summary (PO, Part, Load Count).
/// </summary>
public sealed partial class View_Receiving_Wizard_Display_Step1Summary : UserControl
{
    public ViewModel_Receiving_Wizard_Display_Step1Summary ViewModel { get; }

    /// <summary>
    /// Initializes the Step 1 Summary view with DI.
    /// </summary>
    public View_Receiving_Wizard_Display_Step1Summary(ViewModel_Receiving_Wizard_Display_Step1Summary viewModel)
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
    public View_Receiving_Wizard_Display_Step1Summary()
    {
        ViewModel = App.GetService<ViewModel_Receiving_Wizard_Display_Step1Summary>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

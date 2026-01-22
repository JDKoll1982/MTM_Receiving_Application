using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Routing.Views;

public sealed partial class View_Settings_Routing_BusinessRules : Page
{
    public ViewModel_Settings_Routing_BusinessRules ViewModel { get; }

    public View_Settings_Routing_BusinessRules(ViewModel_Settings_Routing_BusinessRules viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

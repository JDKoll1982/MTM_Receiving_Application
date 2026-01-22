using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class RoutingWizardContainerView : Page
{
    public RoutingWizardContainerViewModel ViewModel { get; }

    public RoutingWizardContainerView(RoutingWizardContainerViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

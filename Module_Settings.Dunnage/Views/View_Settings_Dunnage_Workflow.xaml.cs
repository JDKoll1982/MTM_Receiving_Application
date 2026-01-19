using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.Views;

public sealed partial class View_Settings_Dunnage_Workflow : Page
{
    public ViewModel_Settings_Dunnage_Workflow ViewModel { get; }

    public View_Settings_Dunnage_Workflow(ViewModel_Settings_Dunnage_Workflow viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

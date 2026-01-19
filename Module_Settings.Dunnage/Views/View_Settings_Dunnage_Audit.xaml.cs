using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.Views;

public sealed partial class View_Settings_Dunnage_Audit : Page
{
    public ViewModel_Settings_Dunnage_Audit ViewModel { get; }

    public View_Settings_Dunnage_Audit(ViewModel_Settings_Dunnage_Audit viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

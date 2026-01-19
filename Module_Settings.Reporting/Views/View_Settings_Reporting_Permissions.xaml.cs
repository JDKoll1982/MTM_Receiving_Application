using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.Views;

public sealed partial class View_Settings_Reporting_Permissions : Page
{
    public ViewModel_Settings_Reporting_Permissions ViewModel { get; }

    public View_Settings_Reporting_Permissions(ViewModel_Settings_Reporting_Permissions viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

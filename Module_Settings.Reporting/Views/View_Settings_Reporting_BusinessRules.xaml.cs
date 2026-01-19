using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.Views;

public sealed partial class View_Settings_Reporting_BusinessRules : Page
{
    public ViewModel_Settings_Reporting_BusinessRules ViewModel { get; }

    public View_Settings_Reporting_BusinessRules(ViewModel_Settings_Reporting_BusinessRules viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }
}

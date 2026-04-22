using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.Views;

public sealed partial class View_Settings_Reporting_EmailRecipients : Page
{
    public ViewModel_Settings_Reporting_EmailRecipients ViewModel { get; }

    public View_Settings_Reporting_EmailRecipients(
        ViewModel_Settings_Reporting_EmailRecipients viewModel
    )
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}

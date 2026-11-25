using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.Views;

public sealed partial class View_Settings_Reporting_PdfOutput : Page
{
    public ViewModel_Settings_Reporting_PdfOutput ViewModel { get; }

    public View_Settings_Reporting_PdfOutput(ViewModel_Settings_Reporting_PdfOutput viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}

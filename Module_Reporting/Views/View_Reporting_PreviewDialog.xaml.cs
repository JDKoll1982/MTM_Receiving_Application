using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Reporting.Views;

public sealed partial class View_Reporting_PreviewDialog : ContentDialog
{
    public ViewModel_Reporting_Main ViewModel { get; }

    public View_Reporting_PreviewDialog(ViewModel_Reporting_Main viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }
}

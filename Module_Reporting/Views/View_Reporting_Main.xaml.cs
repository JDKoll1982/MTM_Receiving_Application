using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Reporting.Views;

/// <summary>
/// Main page for End-of-Day Reporting module
/// </summary>
public sealed partial class View_Reporting_Main : Page
{
    public ViewModel_Reporting_Main ViewModel { get; }

    public View_Reporting_Main()
    {
        ViewModel = App.GetService<ViewModel_Reporting_Main>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_ScannerAccess : Page
{
    public ViewModel_Settings_ScannerAccess ViewModel { get; }

    public View_Settings_ScannerAccess(ViewModel_Settings_ScannerAccess viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }
}

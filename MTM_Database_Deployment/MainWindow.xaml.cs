using Microsoft.UI.Xaml;
using MTM_Database_Deployment.ViewModels;

namespace MTM_Database_Deployment;

/// <summary>
/// Deployment shell. The view model is created before the XAML loads so compiled bindings resolve.
/// </summary>
public sealed partial class MainWindow : Window
{
    public ViewModel_DatabaseDeployment ViewModel { get; }

    public MainWindow()
    {
        ViewModel = new ViewModel_DatabaseDeployment();
        InitializeComponent();
    }
}

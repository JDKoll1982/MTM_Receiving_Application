using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_VisualAutomation.ViewModels;

namespace MTM_Receiving_Application.Module_VisualAutomation.Views;

/// <summary>
/// Test harness for Infor VISUAL Purchase Receipt Entry macros (VMRCVENT.EXE).
/// </summary>
public sealed partial class View_VisualAutomation_MacroTester : Page
{
    public View_VisualAutomation_MacroTester()
    {
        ViewModel = App.GetService<ViewModel_VisualAutomation_MacroTester>();
        DataContext = ViewModel;
        InitializeComponent();
    }

    public ViewModel_VisualAutomation_MacroTester ViewModel { get; }

    private void OnLoaded(object sender, RoutedEventArgs e) => ViewModel.Initialize();
}

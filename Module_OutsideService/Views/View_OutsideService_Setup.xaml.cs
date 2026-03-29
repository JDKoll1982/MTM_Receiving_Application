using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_OutsideService.ViewModels;

namespace MTM_Receiving_Application.Module_OutsideService.Views;

/// <summary>
/// Setup page for the selected Outside Service line.
/// </summary>
public sealed partial class View_OutsideService_Setup : Page
{
    public ViewModel_OutsideService_Setup ViewModel { get; }

    public View_OutsideService_Setup()
    {
        ViewModel = App.GetService<ViewModel_OutsideService_Setup>();
        InitializeComponent();
    }
}

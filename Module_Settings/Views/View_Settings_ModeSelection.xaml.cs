using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Views;

public sealed partial class View_Settings_ModeSelection : UserControl
{
    public ViewModel_Settings_ModeSelection ViewModel { get; }

    public View_Settings_ModeSelection()
    {
        ViewModel = App.GetService<ViewModel_Settings_ModeSelection>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

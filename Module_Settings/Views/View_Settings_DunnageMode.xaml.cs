using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Views;

public sealed partial class View_Settings_DunnageMode : UserControl
{
    public ViewModel_Settings_DunnageMode ViewModel { get; }

    public View_Settings_DunnageMode()
    {
        ViewModel = App.GetService<ViewModel_Settings_DunnageMode>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

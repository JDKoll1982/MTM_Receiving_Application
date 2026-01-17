using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_SharedPaths : Page
{
    public ViewModel_Settings_SharedPaths ViewModel { get; }

    public View_Settings_SharedPaths()
    {
        ViewModel = App.GetService<ViewModel_Settings_SharedPaths>();
        InitializeComponent();
    }
}

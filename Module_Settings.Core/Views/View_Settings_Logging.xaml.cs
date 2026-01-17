using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_Logging : Page
{
    public ViewModel_Settings_Logging ViewModel { get; }

    public View_Settings_Logging()
    {
        ViewModel = App.GetService<ViewModel_Settings_Logging>();
        InitializeComponent();
    }
}

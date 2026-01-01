using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Settings;

namespace MTM_Receiving_Application.Views.Settings;

public sealed partial class Settings_DunnageModeView : UserControl
{
    public Settings_DunnageModeViewModel ViewModel { get; }

    public Settings_DunnageModeView()
    {
        ViewModel = App.GetService<Settings_DunnageModeViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

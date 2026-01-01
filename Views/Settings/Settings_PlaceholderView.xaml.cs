using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Settings;

namespace MTM_Receiving_Application.Views.Settings;

public sealed partial class Settings_PlaceholderView : UserControl
{
    public Settings_PlaceholderViewModel ViewModel { get; }

    public Settings_PlaceholderView()
    {
        ViewModel = App.GetService<Settings_PlaceholderViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

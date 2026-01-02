using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Views.Settings;

public sealed partial class Settings_ModeSelectionView : UserControl
{
    public ViewModels.Settings.Settings_ModeSelectionViewModel ViewModel { get; }

    public Settings_ModeSelectionView()
    {
        ViewModel = App.GetService<ViewModels.Settings.Settings_ModeSelectionViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}

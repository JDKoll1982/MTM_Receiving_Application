using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.DunnageModule.ViewModels;

namespace MTM_Receiving_Application.DunnageModule.Views;

public sealed partial class View_Dunnage_ReviewView : UserControl
{
    public ViewModel_Dunnage_Review ViewModel { get; }

    public View_Dunnage_ReviewView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_Review>();
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadSessionLoads();
    }
}

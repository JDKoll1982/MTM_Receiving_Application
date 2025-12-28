using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.ViewModels.Dunnage;

namespace MTM_Receiving_Application.Views.Dunnage;

public sealed partial class Dunnage_ReviewView : UserControl
{
    public Dunnage_ReviewViewModel ViewModel { get; }

    public Dunnage_ReviewView()
    {
        ViewModel = App.GetService<Dunnage_ReviewViewModel>();
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadSessionLoads();
    }
}

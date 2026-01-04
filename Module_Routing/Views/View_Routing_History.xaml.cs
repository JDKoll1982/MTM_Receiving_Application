using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class View_Routing_History : UserControl
{
    public ViewModel_Routing_History ViewModel { get; }

    public View_Routing_History()
    {
        ViewModel = App.GetService<ViewModel_Routing_History>();
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }
}

using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class View_Routing_LabelEntry : UserControl
{
    public ViewModel_Routing_LabelEntry ViewModel { get; }

    public View_Routing_LabelEntry()
    {
        ViewModel = App.GetService<ViewModel_Routing_LabelEntry>();
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }
}

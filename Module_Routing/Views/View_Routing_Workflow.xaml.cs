using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Routing.Enums;
using MTM_Receiving_Application.Module_Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class View_Routing_Workflow : Page
{
    public ViewModel_Routing_Workflow ViewModel { get; }

    public View_Routing_Workflow()
    {
        ViewModel = App.GetService<ViewModel_Routing_Workflow>();
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Set child ViewModels
        ViewModel.LabelEntryViewModel = LabelEntryView.ViewModel;
        ViewModel.HistoryViewModel = HistoryView.ViewModel;
        await ViewModel.InitializeAsync();
    }
}

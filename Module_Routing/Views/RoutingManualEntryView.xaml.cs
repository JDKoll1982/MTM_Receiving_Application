using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Routing.ViewModels;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Views;

public sealed partial class RoutingManualEntryView : Page
{
    public RoutingManualEntryViewModel ViewModel { get; }

    public RoutingManualEntryView()
    {
        ViewModel = App.GetService<RoutingManualEntryViewModel>();
        InitializeComponent();
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }

    private async void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        // If PO Number column was edited, validate PO
        if (e.Column.Header.ToString() == "PO Number" && !e.Cancel)
        {
            var label = e.Row.DataContext as Model_RoutingLabel;
            if (label != null)
            {
                // Defer validation to avoid concurrent access issues
                _ = Dispatcher.TryEnqueue(async () =>
                {
                    await ViewModel.ValidatePOAsync(label);
                });
            }
        }
    }
}

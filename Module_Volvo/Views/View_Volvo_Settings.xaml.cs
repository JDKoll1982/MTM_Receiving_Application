using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Volvo.ViewModels;

namespace MTM_Receiving_Application.Module_Volvo.Views;

/// <summary>
/// Settings view for Volvo parts master data management
/// </summary>
public sealed partial class View_Volvo_Settings : Page
{
    public ViewModel_Volvo_Settings ViewModel { get; }

    public View_Volvo_Settings()
    {
        ViewModel = App.GetService<ViewModel_Volvo_Settings>();
        InitializeComponent();
    }

    private async void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        await ViewModel.RefreshCommand.ExecuteAsync(null);
    }
}

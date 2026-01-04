using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_DetailsEntryView : UserControl
{
    public ViewModel_Dunnage_DetailsEntry ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_DetailsEntryView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_DetailsEntry>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this, PoNumberTextBox);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadSpecsForSelectedPartAsync();
    }
}

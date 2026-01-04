using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_QuantityEntryView : UserControl
{
    public ViewModel_Dunnage_QuantityEntry ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_QuantityEntryView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_QuantityEntry>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadContextData();
    }
}

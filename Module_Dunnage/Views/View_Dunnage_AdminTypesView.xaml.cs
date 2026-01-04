using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_AdminTypesView : Page
{
    public ViewModel_Dunnage_AdminTypes ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_AdminTypesView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_AdminTypes>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this);
    }

    private async void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        App.GetService<IService_LoggingUtility>().LogInfo("Admin Types View loaded", "AdminTypesView");
        await ViewModel.LoadTypesCommand.ExecuteAsync(null);
    }
}


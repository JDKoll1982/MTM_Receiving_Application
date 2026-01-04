using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_AdminMainView : Page
{
    public ViewModel_Dunnage_AdminMain ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_AdminMainView()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_AdminMain>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this);
    }
}

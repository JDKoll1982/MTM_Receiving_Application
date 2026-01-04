using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Views;

public sealed partial class View_Settings_DunnageMode : UserControl
{
    public ViewModel_Settings_DunnageMode ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Settings_DunnageMode()
    {
        ViewModel = App.GetService<ViewModel_Settings_DunnageMode>();
        _focusService = App.GetService<IService_Focus>();
        InitializeComponent();
        DataContext = ViewModel;

        _focusService.AttachFocusOnVisibility(this);
    }
}

using System;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Reprint.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Reprint.ViewModels;

/// <summary>
/// Landing page view model for Reprint Labels. Shows the three mode cards; selecting a mode
/// raises <see cref="ModeSelected"/> which the view code-behind uses to navigate to the mode page.
/// </summary>
public partial class ViewModel_Reprint_Main : ViewModel_Shared_Base
{
    public event EventHandler<Enum_ReprintMode>? ModeSelected;

    public ViewModel_Reprint_Main(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService) { }

    [RelayCommand]
    private void SelectReceiving() => ModeSelected?.Invoke(this, Enum_ReprintMode.Receiving);

    [RelayCommand]
    private void SelectDunnage() => ModeSelected?.Invoke(this, Enum_ReprintMode.Dunnage);

    [RelayCommand]
    private void SelectVolvo() => ModeSelected?.Invoke(this, Enum_ReprintMode.Volvo);
}

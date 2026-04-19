using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for the Volvo shipment history detail dialog.
/// </summary>
public partial class ViewModel_Volvo_ShipmentHistoryDetailDialog : ViewModel_Shared_Base
{
    [ObservableProperty]
    private string _dialogTitle = string.Empty;

    [ObservableProperty]
    private string _detailText = string.Empty;

    public ViewModel_Volvo_ShipmentHistoryDetailDialog(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService) { }

    /// <summary>
    /// Loads the prepared dialog model into the ViewModel.
    /// </summary>
    /// <param name="dialogModel"></param>
    public void Initialize(Model_VolvoShipmentHistoryDetailDialog dialogModel)
    {
        ArgumentNullException.ThrowIfNull(dialogModel);

        DialogTitle = dialogModel.DialogTitle;
        DetailText = dialogModel.DetailText;
    }
}

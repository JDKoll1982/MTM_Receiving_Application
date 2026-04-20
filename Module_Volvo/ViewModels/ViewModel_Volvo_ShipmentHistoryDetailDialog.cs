using System;
using System.Collections.ObjectModel;
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
    private string _windowTitle = string.Empty;

    [ObservableProperty]
    private Model_VolvoShipment _shipment = new();

    [ObservableProperty]
    private ObservableCollection<Model_VolvoShipmentLine> _lines = new();

    [ObservableProperty]
    private string _lineSummaryText = string.Empty;

    public bool HasLines => Lines.Count > 0;

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

        WindowTitle = dialogModel.WindowTitle;
        Shipment = dialogModel.Shipment;
        Lines = new ObservableCollection<Model_VolvoShipmentLine>(dialogModel.Lines);
        LineSummaryText = $"{Lines.Count} part line{(Lines.Count == 1 ? string.Empty : "s")}";
        OnPropertyChanged(nameof(HasLines));
    }
}

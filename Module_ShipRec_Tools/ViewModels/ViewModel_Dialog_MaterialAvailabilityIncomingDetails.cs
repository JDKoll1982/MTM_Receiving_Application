using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reporting;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Material Availability incoming-material details dialog.
/// </summary>
public sealed partial class ViewModel_Dialog_MaterialAvailabilityIncomingDetails
    : ViewModel_Shared_Base
{
    private readonly IService_Tool_MaterialAvailabilityBoard _service;
    private readonly Model_Tool_MaterialAvailabilityCard _card;

    public Func<
        Model_FormattedReportDocument,
        Task<Model_Dao_Result<bool>>
    >? RequestPrintAsync { get; set; }

    public ObservableCollection<Model_Tool_MaterialAvailabilityIncomingLine> Lines { get; }

    public string Heading => $"Incoming Material - {_card.PartId}";

    public string Subheading =>
        string.IsNullOrWhiteSpace(_card.PartDescription)
            ? _card.PartId
            : $"{_card.PartId} - {_card.PartDescription}";

    public string SummaryText =>
        $"Received {_card.IncomingRollup.ReceivedQtyDisplay} | Ordered {_card.IncomingRollup.OrderedQtyDisplay} | Remaining {_card.IncomingRollup.RemainingQtyDisplay}";

    public bool HasLines => Lines.Count > 0;

    public bool HasNoLines => !HasLines;

    public ViewModel_Dialog_MaterialAvailabilityIncomingDetails(
        Model_Tool_MaterialAvailabilityCard card,
        IService_Tool_MaterialAvailabilityBoard service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _card = card ?? throw new ArgumentNullException(nameof(card));
        _service = service ?? throw new ArgumentNullException(nameof(service));
        Lines = new ObservableCollection<Model_Tool_MaterialAvailabilityIncomingLine>(
            _card.IncomingLines
        );
    }

    [RelayCommand]
    private async Task PrintAsync()
    {
        if (RequestPrintAsync is null)
        {
            return;
        }

        var documentResult = await _service.FormatIncomingMaterialForPrintAsync(_card);
        if (!documentResult.IsSuccess || documentResult.Data is null)
        {
            await _errorHandler.ShowUserErrorAsync(
                documentResult.ErrorMessage,
                "Incoming Material Print",
                nameof(PrintAsync)
            );
            return;
        }

        var printResult = await RequestPrintAsync(documentResult.Data);
        if (!printResult.IsSuccess || !printResult.Data)
        {
            await _errorHandler.ShowUserErrorAsync(
                printResult.ErrorMessage,
                "Incoming Material Print",
                nameof(PrintAsync)
            );
        }
    }
}

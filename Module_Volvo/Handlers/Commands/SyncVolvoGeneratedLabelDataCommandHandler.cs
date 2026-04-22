using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Helpers;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Rebuilds the generated Volvo label queue rows for one shipment from the saved shipment lines.
/// </summary>
public class SyncVolvoGeneratedLabelDataCommandHandler
    : IRequestHandler<SyncVolvoGeneratedLabelDataCommand, Model_Dao_Result<int>>
{
    private readonly Dao_VolvoShipment _shipmentDao;
    private readonly Dao_VolvoShipmentLine _lineDao;
    private readonly IDao_VolvoGeneratedLabelData _generatedLabelDataDao;
    private readonly IService_InforVisual _inforVisualService;

    public SyncVolvoGeneratedLabelDataCommandHandler(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        IDao_VolvoGeneratedLabelData generatedLabelDataDao,
        IService_InforVisual inforVisualService
    )
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
        _generatedLabelDataDao =
            generatedLabelDataDao ?? throw new ArgumentNullException(nameof(generatedLabelDataDao));
        _inforVisualService =
            inforVisualService ?? throw new ArgumentNullException(nameof(inforVisualService));
    }

    public async Task<Model_Dao_Result<int>> Handle(
        SyncVolvoGeneratedLabelDataCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var shipmentResult = await _shipmentDao.GetByIdAsync(request.ShipmentId);
            if (!shipmentResult.IsSuccess || shipmentResult.Data is null)
            {
                return Model_Dao_Result_Factory.Failure<int>(
                    shipmentResult.ErrorMessage ?? "Shipment not found for generated label sync"
                );
            }

            var linesResult = await _lineDao.GetByShipmentIdAsync(request.ShipmentId);
            if (!linesResult.IsSuccess || linesResult.Data is null)
            {
                return Model_Dao_Result_Factory.Failure<int>(
                    linesResult.ErrorMessage ?? "Shipment lines not found for generated label sync"
                );
            }

            var descriptionsByPartNumber = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase
            );

            foreach (
                var partNumber in linesResult
                    .Data.Select(line => line.PartNumber)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
            )
            {
                if (string.IsNullOrWhiteSpace(partNumber))
                {
                    continue;
                }

                var partResult = await _inforVisualService.GetPartByIDAsync(partNumber.Trim());
                if (
                    partResult.IsSuccess
                    && partResult.Data is Model_InforVisualPart part
                    && string.IsNullOrWhiteSpace(part.Description) is false
                )
                {
                    descriptionsByPartNumber[partNumber] = part.Description.Trim();
                }
            }

            var rows = Helper_VolvoGeneratedLabelDataBuilder.BuildRows(
                shipmentResult.Data,
                linesResult.Data,
                descriptionsByPartNumber
            );

            return await _generatedLabelDataDao.ReplaceForShipmentAsync(request.ShipmentId, rows);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Unexpected error syncing generated Volvo label data: {ex.Message}",
                ex
            );
        }
    }
}

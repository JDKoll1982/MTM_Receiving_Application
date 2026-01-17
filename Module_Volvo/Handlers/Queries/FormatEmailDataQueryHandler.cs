using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Helpers;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// Handler for FormatEmailDataQuery - formats email data for a shipment.
/// </summary>
public class FormatEmailDataQueryHandler : IRequestHandler<FormatEmailDataQuery, Model_Dao_Result<Model_VolvoEmailData>>
{
    private readonly Dao_VolvoShipment _shipmentDao;
    private readonly Dao_VolvoShipmentLine _lineDao;
    private readonly Dao_VolvoPart _partDao;
    private readonly Dao_VolvoPartComponent _componentDao;
    private readonly IService_LoggingUtility _logger;

    public FormatEmailDataQueryHandler(
        Dao_VolvoShipment shipmentDao,
        Dao_VolvoShipmentLine lineDao,
        Dao_VolvoPart partDao,
        Dao_VolvoPartComponent componentDao,
        IService_LoggingUtility logger)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
        _lineDao = lineDao ?? throw new ArgumentNullException(nameof(lineDao));
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
        _componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Model_Dao_Result<Model_VolvoEmailData>> Handle(FormatEmailDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var shipmentResult = await _shipmentDao.GetByIdAsync(request.ShipmentId);
            if (!shipmentResult.IsSuccess || shipmentResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<Model_VolvoEmailData>(
                    shipmentResult.ErrorMessage ?? "Shipment not found");
            }

            var linesResult = await _lineDao.GetByShipmentIdAsync(request.ShipmentId);
            if (!linesResult.IsSuccess || linesResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<Model_VolvoEmailData>(
                    linesResult.ErrorMessage ?? "Failed to retrieve shipment lines");
            }

            var requestedLines = new Dictionary<string, int>();
            var explosionResult = await Helper_VolvoShipmentCalculations.CalculateComponentExplosionAsync(
                _partDao,
                _componentDao,
                linesResult.Data,
                _logger);
            if (explosionResult.IsSuccess && explosionResult.Data != null)
            {
                requestedLines = explosionResult.Data;
            }

            var emailData = BuildEmailData(shipmentResult.Data, linesResult.Data, requestedLines);

            return Model_Dao_Result_Factory.Success(emailData);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_VolvoEmailData>(
                $"Unexpected error formatting email data: {ex.Message}", ex);
        }
    }

    private static Model_VolvoEmailData BuildEmailData(
        Model_VolvoShipment shipment,
        List<Model_VolvoShipmentLine> lines,
        Dictionary<string, int> requestedLines)
    {
        var emailData = new Model_VolvoEmailData
        {
            Subject = $"PO Requisition - Volvo Dunnage - {shipment.ShipmentDate:MM/dd/yyyy} Shipment #{shipment.ShipmentNumber}",
            Greeting = "Good morning,",
            Message = $"Please create a PO for the following Volvo dunnage received on {shipment.ShipmentDate:MM/dd/yyyy}:",
            RequestedLines = requestedLines,
            AdditionalNotes = string.IsNullOrWhiteSpace(shipment.Notes) ? null : shipment.Notes,
            Signature = string.Empty
        };

        var discrepancies = lines.Where(l => l.HasDiscrepancy).ToList();
        foreach (var line in discrepancies)
        {
            int expectedPieces = line.ExpectedPieceCount ?? 0;
            int receivedPieces = line.CalculatedPieceCount;
            int difference = receivedPieces - expectedPieces;
            emailData.Discrepancies.Add(new Model_VolvoEmailData.DiscrepancyLineItem
            {
                PartNumber = line.PartNumber,
                PacklistQty = expectedPieces,
                ReceivedQty = receivedPieces,
                Difference = difference,
                Note = line.DiscrepancyNote ?? string.Empty
            });
        }

        return emailData;
    }
}

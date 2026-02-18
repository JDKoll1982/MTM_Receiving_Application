using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// [STUB] Handler for export operations - exports shipment history.
/// TODO: Implement database export operation.
/// </summary>
public class ExportShipmentsQueryHandler : IRequestHandler<ExportShipmentsQuery, Model_Dao_Result<string>>
{
    private readonly Dao_VolvoShipment _shipmentDao;

    public ExportShipmentsQueryHandler(Dao_VolvoShipment shipmentDao)
    {
        _shipmentDao = shipmentDao ?? throw new ArgumentNullException(nameof(shipmentDao));
    }

    public async Task<Model_Dao_Result<string>> Handle(ExportShipmentsQuery request, CancellationToken cancellationToken)
    {
        var startDate = (request.StartDate ?? DateTimeOffset.Now.AddDays(-30)).DateTime;
        var endDate = (request.EndDate ?? DateTimeOffset.Now).DateTime;
        var statusFilter = NormalizeStatus(request.StatusFilter);

        var historyResult = await _shipmentDao.GetHistoryAsync(startDate, endDate, statusFilter);
        if (!historyResult.IsSuccess || historyResult.Data == null)
        {
            return Model_Dao_Result_Factory.Failure<string>(
                historyResult.ErrorMessage ?? "Failed to retrieve history data");
        }

        var output = new StringBuilder();
        output.AppendLine("ShipmentNumber,Date,PONumber,ReceiverNumber,Status,EmployeeNumber,Notes");

        foreach (var shipment in historyResult.Data)
        {
            output.AppendLine($"{shipment.ShipmentNumber}," +
                           $"{shipment.ShipmentDate:yyyy-MM-dd}," +
                           $"{EscapeDataField(shipment.PONumber)}," +
                           $"{EscapeDataField(shipment.ReceiverNumber)}," +
                           $"{shipment.Status}," +
                           $"{shipment.EmployeeNumber}," +
                           $"{EscapeDataField(shipment.Notes)}");
        }

        return Model_Dao_Result_Factory.Success(output.ToString());
    }

    private static string EscapeDataField(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "all";
        }

        if (status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            return "all";
        }

        if (status.Equals("Pending PO", StringComparison.OrdinalIgnoreCase))
        {
            return VolvoShipmentStatus.PendingPo;
        }

        if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            return VolvoShipmentStatus.Completed;
        }

        return status;
    }
}

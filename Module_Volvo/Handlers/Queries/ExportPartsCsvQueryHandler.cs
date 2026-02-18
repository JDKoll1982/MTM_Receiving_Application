using System;
using System.Collections.Generic;
using System.Linq;
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
/// [STUB] Handler for export operations - exports parts master data.
/// TODO: Implement database export operation.
/// </summary>
public class ExportPartsQueryHandler : IRequestHandler<ExportPartsQuery, Model_Dao_Result<string>>
{
    private readonly Dao_VolvoPart _partDao;
    private readonly Dao_VolvoPartComponent _componentDao;

    public ExportPartsQueryHandler(Dao_VolvoPart partDao, Dao_VolvoPartComponent componentDao)
    {
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
        _componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
    }

    public async Task<Model_Dao_Result<string>> Handle(ExportPartsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var partsResult = await _partDao.GetAllAsync(request.IncludeInactive);
            if (!partsResult.IsSuccess || partsResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<string>(partsResult.ErrorMessage ?? "Failed to retrieve parts");
            }

            var output = new StringBuilder();
            output.AppendLine("PartNumber,QuantityPerSkid,Components");

            foreach (var part in partsResult.Data)
            {
                var componentsResult = await _componentDao.GetByParentPartAsync(part.PartNumber);
                var componentsStr = string.Empty;

                if (componentsResult.IsSuccess && componentsResult.Data?.Any() == true)
                {
                    componentsStr = string.Join(";", componentsResult.Data.Select(c => $"{c.ComponentPartNumber}:{c.Quantity}"));
                }

                output.AppendLine($"{EscapeDataField(part.PartNumber)},{part.QuantityPerSkid},{EscapeDataField(componentsStr)}");
            }

            return Model_Dao_Result_Factory.Success(output.ToString());
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<string>($"Export failed: {ex.Message}");
        }
    }

    private static string EscapeDataField(string? field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return string.Empty;
        }

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }
}

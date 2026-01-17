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
/// Handler for ExportPartsCsvQuery - exports parts master data as CSV content.
/// </summary>
public class ExportPartsCsvQueryHandler : IRequestHandler<ExportPartsCsvQuery, Model_Dao_Result<string>>
{
    private readonly Dao_VolvoPart _partDao;
    private readonly Dao_VolvoPartComponent _componentDao;

    public ExportPartsCsvQueryHandler(Dao_VolvoPart partDao, Dao_VolvoPartComponent componentDao)
    {
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
        _componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
    }

    public async Task<Model_Dao_Result<string>> Handle(ExportPartsCsvQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var partsResult = await _partDao.GetAllAsync(request.IncludeInactive);
            if (!partsResult.IsSuccess || partsResult.Data == null)
            {
                return Model_Dao_Result_Factory.Failure<string>(partsResult.ErrorMessage ?? "Failed to retrieve parts");
            }

            var csv = new StringBuilder();
            csv.AppendLine("PartNumber,QuantityPerSkid,Components");

            foreach (var part in partsResult.Data)
            {
                var componentsResult = await _componentDao.GetByParentPartAsync(part.PartNumber);
                var componentsStr = string.Empty;

                if (componentsResult.IsSuccess && componentsResult.Data?.Any() == true)
                {
                    componentsStr = string.Join(";", componentsResult.Data.Select(c => $"{c.ComponentPartNumber}:{c.Quantity}"));
                }

                csv.AppendLine($"{EscapeCsvField(part.PartNumber)},{part.QuantityPerSkid},{EscapeCsvField(componentsStr)}");
            }

            return Model_Dao_Result_Factory.Success(csv.ToString());
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<string>($"Export failed: {ex.Message}");
        }
    }

    private static string EscapeCsvField(string? field)
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

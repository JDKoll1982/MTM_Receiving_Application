using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// Handler for GenerateLabelCsvQuery - generates CSV label file for shipment.
/// Delegates to Service_Volvo for component explosion and CSV formatting.
/// </summary>
public class GenerateLabelCsvQueryHandler : IRequestHandler<GenerateLabelCsvQuery, Model_Dao_Result<string>>
{
    private readonly IService_Volvo _volvoService;

    public GenerateLabelCsvQueryHandler(IService_Volvo volvoService)
    {
        _volvoService = volvoService ?? throw new ArgumentNullException(nameof(volvoService));
    }

    public async Task<Model_Dao_Result<string>> Handle(GenerateLabelCsvQuery request, CancellationToken cancellationToken)
    {
        // Delegate to existing service for CSV generation
        // This service handles: auth check, component explosion, CSV formatting, file writing
        return await _volvoService.GenerateLabelCsvAsync(request.ShipmentId);
    }
}

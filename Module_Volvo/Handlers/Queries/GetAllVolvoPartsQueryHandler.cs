using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// Handler for GetAllVolvoPartsQuery - retrieves parts for settings grid.
/// </summary>
public class GetAllVolvoPartsQueryHandler : IRequestHandler<GetAllVolvoPartsQuery, Model_Dao_Result<List<Model_VolvoPart>>>
{
    private readonly Dao_VolvoPart _partDao;

    public GetAllVolvoPartsQueryHandler(Dao_VolvoPart partDao)
    {
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
    }

    public async Task<Model_Dao_Result<List<Model_VolvoPart>>> Handle(GetAllVolvoPartsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _partDao.GetAllAsync(request.IncludeInactive);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_VolvoPart>>(
                $"Unexpected error retrieving parts: {ex.Message}", ex);
        }
    }
}

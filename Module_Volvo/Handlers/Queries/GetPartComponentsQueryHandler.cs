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
/// Handler for GetPartComponentsQuery - retrieves components for a parent part.
/// </summary>
public class GetPartComponentsQueryHandler : IRequestHandler<GetPartComponentsQuery, Model_Dao_Result<List<Model_VolvoPartComponent>>>
{
    private readonly Dao_VolvoPartComponent _componentDao;

    public GetPartComponentsQueryHandler(Dao_VolvoPartComponent componentDao)
    {
        _componentDao = componentDao ?? throw new ArgumentNullException(nameof(componentDao));
    }

    public async Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> Handle(GetPartComponentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PartNumber))
            {
                return Model_Dao_Result_Factory.Failure<List<Model_VolvoPartComponent>>("Part number is required");
            }

            return await _componentDao.GetByParentPartAsync(request.PartNumber);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_VolvoPartComponent>>(
                $"Unexpected error retrieving components: {ex.Message}", ex);
        }
    }
}

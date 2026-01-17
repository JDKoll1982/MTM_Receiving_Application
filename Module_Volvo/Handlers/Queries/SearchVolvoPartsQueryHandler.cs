using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Queries;

/// <summary>
/// Handler for SearchVolvoPartsQuery - autocomplete search for part numbers.
/// </summary>
public class SearchVolvoPartsQueryHandler : IRequestHandler<SearchVolvoPartsQuery, Model_Dao_Result<List<Model_VolvoPart>>>
{
    private readonly Dao_VolvoPart _partDao;

    public SearchVolvoPartsQueryHandler(Dao_VolvoPart partDao)
    {
        _partDao = partDao ?? throw new ArgumentNullException(nameof(partDao));
    }

    public async Task<Model_Dao_Result<List<Model_VolvoPart>>> Handle(SearchVolvoPartsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all active parts
            var allPartsResult = await _partDao.GetAllAsync(includeInactive: false);

            if (!allPartsResult.IsSuccess)
            {
                return allPartsResult;
            }

            // Filter by search text (case-insensitive partial match)
            var filteredParts = (allPartsResult.Data ?? new List<Model_VolvoPart>())
                .Where(p => string.IsNullOrWhiteSpace(request.SearchText) ||
                           p.PartNumber.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase))
                .Take(request.MaxResults)
                .ToList();

            return Model_Dao_Result_Factory.Success(filteredParts);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_VolvoPart>>(
                $"Unexpected error searching parts: {ex.Message}", ex);
        }
    }
}

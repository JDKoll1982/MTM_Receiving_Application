using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Queries;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for looking up parts from Infor Visual database (READ ONLY).
/// Queries part catalog for autocomplete and selection.
/// </summary>
public class GetPartLookupQueryHandler : IRequestHandler<GetPartLookupQuery, Result<List<(int PartId, string PartNumber, string Description)>>>
{
    private readonly ILogger _logger;
    // NOTE: In production, inject a READ-ONLY Infor Visual DAO here
    // private readonly Dao_InforVisualParts _inforDao;

    public GetPartLookupQueryHandler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<Result<List<(int PartId, string PartNumber, string Description)>>> Handle(
        GetPartLookupQuery request,
        CancellationToken cancellationToken)
    {
        _logger.Information("Looking up parts with search term: {SearchTerm}", request.SearchTerm);

        // TODO: Implement actual Infor Visual query
        // For now, return mock data for development/testing
        // In production, this would query Infor Visual with ApplicationIntent=ReadOnly

        /*
        // Production implementation would look like:
        var connectionString = Helper_Database_Variables.GetInforVisualConnectionString();
        var inforDao = new Dao_InforVisualParts(connectionString);
        
        var result = await inforDao.SearchPartsAsync(
            searchTerm: request.SearchTerm,
            maxResults: 50
        );
        
        if (!result.IsSuccess)
        {
            _logger.Error("Failed to query Infor Visual parts: {Error}", result.ErrorMessage);
            return Result<List<(int, string, string)>>.Failure(result.ErrorMessage);
        }
        
        return Result<List<(int, string, string)>>.Success(result.Data);
        */

        // Mock implementation for development
        await Task.Delay(100, cancellationToken); // Simulate database query

        var mockParts = new List<(int PartId, string PartNumber, string Description)>
        {
            (12345, "PART-12345", "Steel Rod 1/2 inch"),
            (12346, "PART-12346", "Steel Plate 10x10"),
            (12347, "PART-12347", "Aluminum Tube 3 inch"),
            (12348, "PART-12348", "Copper Wire 12 AWG"),
            (12349, "PART-12349", "Brass Fitting 1/4 inch")
        };

        var filteredParts = mockParts
            .Where(p => string.IsNullOrWhiteSpace(request.SearchTerm) ||
                       p.PartNumber.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                       p.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _logger.Information("Found {Count} matching parts", filteredParts.Count);
        return Result<List<(int, string, string)>>.Success(filteredParts);
    }
}

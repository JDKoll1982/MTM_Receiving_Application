using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Shared.Contracts.Lookup;
using MTM_Receiving_Application.Module_Shared.Enums;
using MTM_Receiving_Application.Module_Shared.Helpers;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Shared.Services.Lookup;

/// <summary>
/// Default typed lookup strategy for Infor Visual warehouse locations.
/// </summary>
public sealed class Strategy_SharedLocationLookup : ISharedLookupStrategy
{
    private readonly IService_InforVisual _inforVisualService;

    public Strategy_SharedLocationLookup(IService_InforVisual inforVisualService)
    {
        _inforVisualService = inforVisualService;
    }

    public Enum_SharedLookupType LookupType => Enum_SharedLookupType.Location;

    public Model_Dao_Result ValidateRawInput(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return Model_Dao_Result_Factory.Failure("Location is required.");
        }

        return Model_Dao_Result_Factory.Success();
    }

    public Model_SharedLookupFormattingResult ApplyFormatting(Model_SharedLookupRequest request)
    {
        var raw = (request.RawInput ?? string.Empty).Trim().ToUpperInvariant();

        // One canonical formatter shared by every module so a location typed here matches the
        // same value the Scanner Workbench and ShipRec Tools write (V-A0-01, R-04, S-00, ...).
        var canonical = Helper_SharedLocationFormat.Sanitize(raw);
        if (canonical is null)
        {
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = raw,
                HasFormattingRule = false,
                WasFormatted = false,
            };
        }

        return new Model_SharedLookupFormattingResult
        {
            FormattedValue = canonical,
            HasFormattingRule = true,
            WasFormatted = string.Equals(canonical, raw, StringComparison.Ordinal) is false,
        };
    }

    public Task<Model_Dao_Result<bool>> HasExactMatchAsync(
        string formattedValue,
        Model_SharedLookupRequest request,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var warehouseCode = string.IsNullOrWhiteSpace(request.WarehouseCode)
            ? "002"
            : request.WarehouseCode.Trim().ToUpperInvariant();

        return _inforVisualService.LocationExistsAsync(formattedValue, warehouseCode);
    }

    public Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> SearchFuzzyAsync(
        string formattedValue,
        Model_SharedLookupRequest request,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var warehouseCode = string.IsNullOrWhiteSpace(request.WarehouseCode)
            ? "002"
            : request.WarehouseCode.Trim().ToUpperInvariant();

        return _inforVisualService.FuzzySearchLocationsAsync(formattedValue, warehouseCode);
    }

    public string SelectBestFuzzyResult(
        string formattedValue,
        IReadOnlyList<Model_FuzzySearchResult> candidates
    )
    {
        return SharedLookupFuzzySelector.SelectBest(formattedValue, candidates);
    }
}

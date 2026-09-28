using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Shared.Contracts.Lookup;
using MTM_Receiving_Application.Module_Shared.Enums;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Shared.Services.Lookup;

/// <summary>
/// Default typed lookup strategy for Infor Visual warehouse locations.
/// </summary>
public sealed class Strategy_SharedLocationLookup : ISharedLookupStrategy
{
    private static readonly Regex VaPattern = new(
        "^VA(?<digits>\\d{3})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private static readonly Regex RackPattern = new(
        "^R(?<digits>\\d{1,2})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

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
        var compact = raw.Replace("-", string.Empty).Replace(" ", string.Empty);

        var vaMatch = VaPattern.Match(compact);
        if (vaMatch.Success)
        {
            var digits = vaMatch.Groups["digits"].Value;
            var formattedVa = $"V-A{digits[0]}-{digits.Substring(1, 2)}";
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = formattedVa,
                HasFormattingRule = true,
                WasFormatted = string.Equals(formattedVa, raw, StringComparison.Ordinal) is false,
            };
        }

        var rackMatch = RackPattern.Match(compact);
        if (rackMatch.Success)
        {
            var digits = rackMatch.Groups["digits"].Value.PadLeft(2, '0');
            var formattedRack = $"R-{digits}";
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = formattedRack,
                HasFormattingRule = true,
                WasFormatted =
                    string.Equals(formattedRack, raw, StringComparison.Ordinal) is false,
            };
        }

        return new Model_SharedLookupFormattingResult
        {
            FormattedValue = raw,
            HasFormattingRule = false,
            WasFormatted = false,
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

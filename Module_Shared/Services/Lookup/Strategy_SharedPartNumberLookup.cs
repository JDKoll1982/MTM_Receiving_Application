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
/// Default typed lookup strategy for Infor Visual part numbers.
/// </summary>
public sealed class Strategy_SharedPartNumberLookup : ISharedLookupStrategy
{
    private readonly IService_InforVisual _inforVisualService;

    public Strategy_SharedPartNumberLookup(IService_InforVisual inforVisualService)
    {
        _inforVisualService = inforVisualService;
    }

    public Enum_SharedLookupType LookupType => Enum_SharedLookupType.PartNumber;

    public Model_Dao_Result ValidateRawInput(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return Model_Dao_Result_Factory.Failure("Part number is required.");
        }

        return Model_Dao_Result_Factory.Success();
    }

    public Model_SharedLookupFormattingResult ApplyFormatting(Model_SharedLookupRequest request)
    {
        return Helper_SharedPartNumberFormat.Format(request.RawInput, request.PrefixPaddingRules);
    }

    public Task<Model_Dao_Result<bool>> HasExactMatchAsync(
        string formattedValue,
        Model_SharedLookupRequest request,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _inforVisualService.PartExistsAsync(formattedValue);
    }

    public Task<Model_Dao_Result<List<Model_FuzzySearchResult>>> SearchFuzzyAsync(
        string formattedValue,
        Model_SharedLookupRequest request,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _inforVisualService.FuzzySearchPartsAsync(formattedValue);
    }

    public string SelectBestFuzzyResult(
        string formattedValue,
        IReadOnlyList<Model_FuzzySearchResult> candidates
    )
    {
        return SharedLookupFuzzySelector.SelectBest(formattedValue, candidates);
    }
}

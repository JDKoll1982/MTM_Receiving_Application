using System;
using System.Collections.Generic;
using System.Linq;
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
        var canonical = (request.RawInput ?? string.Empty).Trim().ToUpperInvariant();
        if (request.PrefixPaddingRules.Count == 0)
        {
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = canonical,
                HasFormattingRule = false,
                WasFormatted = false,
            };
        }

        var matchingRule = request
            .PrefixPaddingRules.Where(rule => rule.AppliesTo(canonical))
            .OrderByDescending(rule => rule.Prefix.Trim().Length)
            .FirstOrDefault();

        if (matchingRule is null)
        {
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = canonical,
                HasFormattingRule = false,
                WasFormatted = false,
            };
        }

        var normalizedPrefix = matchingRule.Prefix.Trim().ToUpperInvariant();
        if (canonical.Length >= matchingRule.MaxLength || canonical.Length <= normalizedPrefix.Length)
        {
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = canonical,
                HasFormattingRule = true,
                WasFormatted = false,
            };
        }

        var suffix = canonical.Substring(normalizedPrefix.Length);
        var paddingCount = matchingRule.MaxLength - normalizedPrefix.Length - suffix.Length;
        if (paddingCount <= 0)
        {
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = canonical,
                HasFormattingRule = true,
                WasFormatted = false,
            };
        }

        var formatted = $"{normalizedPrefix}{new string(matchingRule.PadCharacter, paddingCount)}{suffix}";
        return new Model_SharedLookupFormattingResult
        {
            FormattedValue = formatted,
            HasFormattingRule = true,
            WasFormatted = string.Equals(formatted, canonical, StringComparison.Ordinal) is false,
        };
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

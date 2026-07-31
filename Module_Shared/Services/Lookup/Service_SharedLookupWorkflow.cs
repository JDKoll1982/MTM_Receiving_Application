using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Shared.Contracts.Lookup;
using MTM_Receiving_Application.Module_Shared.Enums;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Shared.Services.Lookup;

/// <summary>
/// Executes the shared typed lookup validation pipeline for Infor Visual domains.
/// </summary>
public sealed class Service_SharedLookupWorkflow : IService_SharedLookupWorkflow
{
    private readonly IReadOnlyDictionary<Enum_SharedLookupType, ISharedLookupStrategy> _strategies;

    public Service_SharedLookupWorkflow(IEnumerable<ISharedLookupStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(strategy => strategy.LookupType);
    }

    public async Task<Model_SharedLookupValidationResult> ValidateAsync(
        Model_SharedLookupRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_strategies.TryGetValue(request.LookupType, out var strategy))
        {
            return BuildInvalidResult(
                request,
                formattedValue: string.Empty,
                hasFormattingRule: false,
                wasFormatted: false,
                hasExactMatch: false,
                usedFuzzyFallback: false,
                fuzzyCandidates: [],
                message: $"No shared lookup strategy is registered for {request.LookupType}."
            );
        }

        var validation = strategy.ValidateRawInput(request.RawInput);
        if (!validation.Success)
        {
            return BuildInvalidResult(
                request,
                formattedValue: request.RawInput?.Trim() ?? string.Empty,
                hasFormattingRule: false,
                wasFormatted: false,
                hasExactMatch: false,
                usedFuzzyFallback: false,
                fuzzyCandidates: [],
                message: validation.ErrorMessage
            );
        }

        var formatting = strategy.ApplyFormatting(request);
        var formattedValue = formatting.FormattedValue;

        var exactMatchResult = await strategy.HasExactMatchAsync(
            formattedValue,
            request,
            cancellationToken
        );

        if (!exactMatchResult.Success)
        {
            return BuildInvalidResult(
                request,
                formattedValue,
                formatting.HasFormattingRule,
                formatting.WasFormatted,
                hasExactMatch: false,
                usedFuzzyFallback: false,
                fuzzyCandidates: [],
                message: exactMatchResult.ErrorMessage
            );
        }

        if (exactMatchResult.Data)
        {
            var resolvedValue = formattedValue;
            if (request.LookupType == Enum_SharedLookupType.Location)
            {
                resolvedValue = await ResolveCanonicalLocationValueAsync(
                    strategy,
                    formattedValue,
                    request,
                    cancellationToken
                );
            }

            return new Model_SharedLookupValidationResult
            {
                LookupType = request.LookupType,
                RawInput = request.RawInput,
                FormattedValue = formattedValue,
                ResolvedValue = resolvedValue,
                IsValid = true,
                HasFormattingRule = formatting.HasFormattingRule,
                WasFormatted = formatting.WasFormatted,
                HasExactMatch = true,
                UsedFuzzyFallback = false,
                Message = "Exact match found.",
                FuzzyCandidates = [],
            };
        }

        var fuzzyResult = await strategy.SearchFuzzyAsync(formattedValue, request, cancellationToken);
        if (!fuzzyResult.Success)
        {
            return BuildInvalidResult(
                request,
                formattedValue,
                formatting.HasFormattingRule,
                formatting.WasFormatted,
                hasExactMatch: false,
                usedFuzzyFallback: false,
                fuzzyCandidates: [],
                message: fuzzyResult.ErrorMessage
            );
        }

        var candidates = fuzzyResult.Data ?? new List<Model_FuzzySearchResult>();
        if (candidates.Count == 0)
        {
            return BuildInvalidResult(
                request,
                formattedValue,
                formatting.HasFormattingRule,
                formatting.WasFormatted,
                hasExactMatch: false,
                usedFuzzyFallback: false,
                fuzzyCandidates: [],
                message: "No fuzzy matches were found."
            );
        }

        if (!request.AutoResolveFuzzyMatches)
        {
            return new Model_SharedLookupValidationResult
            {
                LookupType = request.LookupType,
                RawInput = request.RawInput,
                FormattedValue = formattedValue,
                ResolvedValue = string.Empty,
                IsValid = true,
                HasFormattingRule = formatting.HasFormattingRule,
                WasFormatted = formatting.WasFormatted,
                HasExactMatch = false,
                UsedFuzzyFallback = true,
                Message = "Fuzzy matches found.",
                FuzzyCandidates = candidates,
            };
        }

        var selected = request.AutoResolveFuzzyMatches
            ? strategy.SelectBestFuzzyResult(formattedValue, candidates)
            : null;

        if (string.IsNullOrWhiteSpace(selected))
        {
            return BuildInvalidResult(
                request,
                formattedValue,
                formatting.HasFormattingRule,
                formatting.WasFormatted,
                hasExactMatch: false,
                usedFuzzyFallback: false,
                fuzzyCandidates: candidates,
                message: "No valid fuzzy result could be resolved."
            );
        }

        return new Model_SharedLookupValidationResult
        {
            LookupType = request.LookupType,
            RawInput = request.RawInput,
            FormattedValue = formattedValue,
            ResolvedValue = selected,
            IsValid = true,
            HasFormattingRule = formatting.HasFormattingRule,
            WasFormatted = formatting.WasFormatted,
            HasExactMatch = false,
            UsedFuzzyFallback = true,
            Message = "Resolved using fuzzy fallback.",
            FuzzyCandidates = candidates,
        };
    }

    private static Model_SharedLookupValidationResult BuildInvalidResult(
        Model_SharedLookupRequest request,
        string formattedValue,
        bool hasFormattingRule,
        bool wasFormatted,
        bool hasExactMatch,
        bool usedFuzzyFallback,
        IReadOnlyList<Model_FuzzySearchResult> fuzzyCandidates,
        string message
    )
    {
        return new Model_SharedLookupValidationResult
        {
            LookupType = request.LookupType,
            RawInput = request.RawInput,
            FormattedValue = formattedValue,
            ResolvedValue = string.Empty,
            IsValid = false,
            HasFormattingRule = hasFormattingRule,
            WasFormatted = wasFormatted,
            HasExactMatch = hasExactMatch,
            UsedFuzzyFallback = usedFuzzyFallback,
            Message = string.IsNullOrWhiteSpace(message)
                ? "Lookup validation failed."
                : message,
            FuzzyCandidates = fuzzyCandidates,
        };
    }

    private static async Task<string> ResolveCanonicalLocationValueAsync(
        ISharedLookupStrategy strategy,
        string formattedValue,
        Model_SharedLookupRequest request,
        CancellationToken cancellationToken
    )
    {
        var fuzzyResult = await strategy.SearchFuzzyAsync(formattedValue, request, cancellationToken);
        if (!fuzzyResult.Success || fuzzyResult.Data is null || fuzzyResult.Data.Count == 0)
        {
            return formattedValue;
        }

        var exactCandidate = fuzzyResult.Data.FirstOrDefault(candidate =>
            string.Equals(candidate.Key?.Trim(), formattedValue, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.Label?.Trim(), formattedValue, StringComparison.OrdinalIgnoreCase)
        );

        if (exactCandidate is null)
        {
            return formattedValue;
        }

        var canonicalValue = string.IsNullOrWhiteSpace(exactCandidate.Label)
            ? exactCandidate.Key
            : exactCandidate.Label;

        return string.IsNullOrWhiteSpace(canonicalValue) ? formattedValue : canonicalValue.Trim();
    }
}

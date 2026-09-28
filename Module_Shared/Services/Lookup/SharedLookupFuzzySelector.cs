using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application.Module_Shared.Services.Lookup;

internal static class SharedLookupFuzzySelector
{
    /// <summary>
    /// Applies a deterministic best-candidate selection for fuzzy lookup results.
    /// </summary>
    public static string SelectBest(
        string formattedValue,
        IReadOnlyList<Model_FuzzySearchResult> candidates
    )
    {
        if (candidates.Count == 0)
        {
            return string.Empty;
        }

        var search = formattedValue?.Trim() ?? string.Empty;

        var best = candidates
            .Where(candidate => string.IsNullOrWhiteSpace(candidate.Label) is false)
            .Select(candidate => new
            {
                Candidate = candidate,
                Score = GetScore(search, candidate.Label.Trim()),
                LengthDelta = Math.Abs(candidate.Label.Trim().Length - search.Length),
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.LengthDelta)
            .ThenBy(item => item.Candidate.Label, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        return best?.Candidate.Label?.Trim() ?? string.Empty;
    }

    private static int GetScore(string search, string label)
    {
        if (string.Equals(search, label, StringComparison.OrdinalIgnoreCase))
        {
            return 300;
        }

        if (label.StartsWith(search, StringComparison.OrdinalIgnoreCase))
        {
            return 200;
        }

        if (label.Contains(search, StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        return 0;
    }
}

using System.Collections.Generic;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Shared.Enums;

namespace MTM_Receiving_Application.Module_Shared.Models.Lookup;

/// <summary>
/// Unified workflow result consumed by shared controls and view models.
/// </summary>
public sealed class Model_SharedLookupValidationResult
{
    public Enum_SharedLookupType LookupType { get; init; } = Enum_SharedLookupType.PartNumber;

    public string RawInput { get; init; } = string.Empty;

    public string FormattedValue { get; init; } = string.Empty;

    public string ResolvedValue { get; init; } = string.Empty;

    public bool IsValid { get; init; }

    public bool HasFormattingRule { get; init; }

    public bool WasFormatted { get; init; }

    public bool HasExactMatch { get; init; }

    public bool UsedFuzzyFallback { get; init; }

    public string Message { get; init; } = string.Empty;

    public IReadOnlyList<Model_FuzzySearchResult> FuzzyCandidates { get; init; } = [];
}

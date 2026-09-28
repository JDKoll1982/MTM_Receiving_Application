using System.Collections.Generic;
using MTM_Receiving_Application.Module_Shared.Enums;

namespace MTM_Receiving_Application.Module_Shared.Models.Lookup;

/// <summary>
/// Shared typed lookup request payload consumed by the lookup workflow.
/// </summary>
public sealed class Model_SharedLookupRequest
{
    public Enum_SharedLookupType LookupType { get; init; } = Enum_SharedLookupType.PartNumber;

    public string RawInput { get; init; } = string.Empty;

    public string WarehouseCode { get; init; } = "002";

    public IReadOnlyList<Model_SharedLookupPrefixPaddingRule> PrefixPaddingRules { get; init; } = [];

    public bool AutoResolveFuzzyMatches { get; init; } = true;
}

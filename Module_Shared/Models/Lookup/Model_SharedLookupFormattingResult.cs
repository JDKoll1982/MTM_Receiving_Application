namespace MTM_Receiving_Application.Module_Shared.Models.Lookup;

/// <summary>
/// Result of strategy-level formatting evaluation.
/// </summary>
public sealed class Model_SharedLookupFormattingResult
{
    public string FormattedValue { get; init; } = string.Empty;

    public bool HasFormattingRule { get; init; }

    public bool WasFormatted { get; init; }
}

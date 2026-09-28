using System;

namespace MTM_Receiving_Application.Module_Shared.Models.Lookup;

/// <summary>
/// Generic prefix padding rule that can be applied by typed lookup strategies.
/// </summary>
public sealed class Model_SharedLookupPrefixPaddingRule
{
    public string Prefix { get; init; } = string.Empty;

    public int MaxLength { get; init; } = 0;

    public char PadCharacter { get; init; } = '0';

    public bool IsEnabled { get; init; } = true;

    public bool AppliesTo(string value)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(Prefix) || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim().StartsWith(Prefix.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}

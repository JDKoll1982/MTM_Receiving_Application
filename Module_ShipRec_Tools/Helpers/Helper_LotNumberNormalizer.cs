using System;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;

/// <summary>
/// Builds the matching key for a heat number and decides whether one is present at all.
/// This is the only normalization path for heat numbers, so a value can never be matched
/// two different ways in two different places.
/// </summary>
public static class Helper_LotNumberNormalizer
{
    /// <summary>
    /// The value the receiving flow writes when a user leaves the heat number empty.
    /// It is treated as no heat number, not as a lot called "Nothing Entered".
    /// </summary>
    public const string BlankHeatSentinel = "Nothing Entered";

    /// <summary>
    /// Characters that carry no meaning in a heat number and are removed before matching,
    /// so values a user typed slightly differently still reach one lot.
    /// </summary>
    private static readonly char[] IgnoredCharacters =
    [
        '-',
        '_',
        '/',
        '\\',
        '.',
        '#',
        ':',
        ';',
        ',',
        '\'',
        '"',
        '(',
        ')',
    ];

    /// <summary>
    /// Returns <c>true</c> when the value carries no heat number: null, empty, whitespace only,
    /// the sentinel the receiving flow substitutes, or a value that reduces to nothing once
    /// separators are removed.
    /// </summary>
    public static bool IsBlank(string? heatNumber)
    {
        if (string.IsNullOrWhiteSpace(heatNumber))
        {
            return true;
        }

        var trimmed = heatNumber.Trim();
        if (trimmed.Equals(BlankHeatSentinel, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Normalize(trimmed).Length == 0;
    }

    /// <summary>
    /// Produces the matching key: upper case, with separators and all whitespace removed.
    /// A blank or missing value returns an empty string; callers check <see cref="IsBlank"/> first.
    /// </summary>
    /// <remarks>
    /// Whitespace is removed rather than collapsed. Collapsing alone would leave "ABC 123" and
    /// "ABC123" as two lots, which is the duplicate-marking failure this feature exists to prevent.
    /// A blank checks before this runs so an empty key is never treated as a real lot.
    /// </remarks>
    public static string Normalize(string? heatNumber)
    {
        if (string.IsNullOrWhiteSpace(heatNumber))
        {
            return string.Empty;
        }

        var upper = heatNumber.Trim().ToUpperInvariant();
        var buffer = new char[upper.Length];
        var length = 0;

        foreach (var character in upper)
        {
            if (char.IsWhiteSpace(character) || Array.IndexOf(IgnoredCharacters, character) >= 0)
            {
                continue;
            }

            buffer[length] = character;
            length++;
        }

        return new string(buffer, 0, length);
    }
}

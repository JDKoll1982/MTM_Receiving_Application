using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MTM_Receiving_Application.Module_Shared.Helpers;

/// <summary>
/// Canonical formatter for Infor Visual warehouse-location IDs. Every module that accepts a
/// typed location (Scanner, Receiving, Dunnage, ShipRec Tools, Settings) funnels through this
/// class so an operator sees the same canonical value no matter where the location was entered.
/// </summary>
/// <remarks>
/// Two layouts are recognized:
/// <list type="bullet">
/// <item>
/// Three-segment <c>PRE-MID-SUFFIX</c> (a 1-2 letter prefix, a letter+digit middle block, and a
/// 1-2 digit suffix). This keeps all 16 documented variants working, for example
/// <c>va001</c>, <c>VA001</c>, <c>Va001</c>, <c>V-A001</c>, and <c>VA0-01</c> all become
/// <c>V-A0-01</c>.
/// </item>
/// <item>
/// Two-segment <c>PRE-DIGITS</c> for locations that have no middle block, for example
/// <c>r04</c> becomes <c>R-04</c>, <c>s00</c> becomes <c>S-00</c>, and <c>w04</c> becomes
/// <c>W-04</c>.
/// </item>
/// </list>
/// Anything that matches neither layout (for example <c>RECV</c>, <c>T-00</c>, or
/// <c>FG-A1-01</c>) is treated as already canonical and left alone by
/// <see cref="FormatOrPassThrough"/>; <see cref="Sanitize"/> returns <see langword="null"/> so
/// callers can tell "no rule applied" apart from "already canonical".
/// <para>
/// Dashes stay part of the pattern rather than being stripped first because they act as
/// explicit segment boundaries: <c>ABC-01</c> must stay unmatched instead of collapsing into
/// the nonsensical <c>AB-C0-01</c>.
/// </para>
/// </remarks>
public static class Helper_SharedLocationFormat
{
    /// <summary>
    /// Three-segment layout. Prefix is 1-2 letters (the greedy quantifier keeps the legacy
    /// "VA-A001" style prefixes intact), middle is a letter plus one digit, and suffix is one
    /// or two digits. Hyphens are optional so every punctuation variant normalizes.
    /// </summary>
    private static readonly Regex ThreeSegmentRegex = new(
        @"^(?<prefix>[A-Za-z]{1,2})-?(?<mid>[A-Za-z][0-9])-?(?<suffix>[0-9]{1,2})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    /// <summary>
    /// Two-segment layout used by locations with no middle block. The prefix is a single
    /// letter so <c>R04</c> cannot be mistaken for a three-segment prefix.
    /// </summary>
    private static readonly Regex TwoSegmentRegex = new(
        @"^(?<prefix>[A-Za-z])-?(?<digits>[0-9]{1,2})$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    /// <summary>
    /// Returns the canonical uppercase location, or <see langword="null"/> when
    /// <paramref name="rawInput"/> matches neither supported layout.
    /// </summary>
    /// <param name="rawInput">Location text exactly as typed or scanned by the operator.</param>
    /// <example>
    /// <code>
    /// Helper_SharedLocationFormat.Sanitize("va001");  // "V-A0-01"
    /// Helper_SharedLocationFormat.Sanitize("r04");    // "R-04"
    /// Helper_SharedLocationFormat.Sanitize("RECV");   // null
    /// </code>
    /// </example>
    public static string? Sanitize(string? rawInput)
    {
        var candidate = Compact(rawInput);
        if (candidate.Length == 0)
        {
            return null;
        }

        var threeSegment = ThreeSegmentRegex.Match(candidate);
        if (threeSegment.Success)
        {
            var prefix = threeSegment.Groups["prefix"].Value.ToUpperInvariant();
            var mid = threeSegment.Groups["mid"].Value.ToUpperInvariant();
            var suffix = PadToTwo(threeSegment.Groups["suffix"].Value);
            return $"{prefix}-{mid}-{suffix}";
        }

        var twoSegment = TwoSegmentRegex.Match(candidate);
        if (twoSegment.Success)
        {
            var prefix = twoSegment.Groups["prefix"].Value.ToUpperInvariant();
            var digits = PadToTwo(twoSegment.Groups["digits"].Value);
            return $"{prefix}-{digits}";
        }

        return null;
    }

    /// <summary>
    /// Returns the canonical uppercase location, or the trimmed uppercase input when no rule
    /// applies. Use this when the caller must always have a value to display or compare.
    /// </summary>
    /// <param name="rawInput">Location text exactly as typed or scanned by the operator.</param>
    public static string FormatOrPassThrough(string? rawInput)
    {
        return Sanitize(rawInput) ?? rawInput?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    /// <summary>
    /// Removes whitespace so "V A0 01" and "VA001" are evaluated identically. Dashes are kept
    /// because they mark guaranteed segment boundaries.
    /// </summary>
    private static string Compact(string? rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return string.Empty;
        }

        return new string(rawInput.Where(character => char.IsWhiteSpace(character) is false).ToArray());
    }

    private static string PadToTwo(string digits)
    {
        return digits.Length == 1 ? "0" + digits : digits;
    }
}

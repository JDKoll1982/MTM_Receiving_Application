using System;
using System.Text.RegularExpressions;

namespace MTM_Receiving_Application.Module_Dunnage.Helpers;

public static class Helper_DunnagePoNumber
{
    private static readonly Regex ValidPoRegex = new(
        @"^(PO-)?\d{1,6}[Bb]?$",
        RegexOptions.IgnoreCase
    );

    private static readonly Regex PoNumberPartRegex = new(
        @"^(\d{1,6})([Bb]?)$",
        RegexOptions.None
    );

    public static string FormatForEntry(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var trimmed = input.Trim();
        string numberPart;

        if (trimmed.StartsWith("po-", StringComparison.OrdinalIgnoreCase))
        {
            numberPart = trimmed[3..];
        }
        else if (trimmed.StartsWith("po", StringComparison.OrdinalIgnoreCase) && trimmed.Length > 2)
        {
            numberPart = trimmed[2..];
        }
        else
        {
            numberPart = trimmed;
        }

        var match = PoNumberPartRegex.Match(numberPart);
        if (!match.Success)
        {
            return trimmed;
        }

        var digits = match.Groups[1].Value;
        var suffix = match.Groups[2].Value.ToUpperInvariant();
        return $"PO-{digits.PadLeft(6, '0')}{suffix}";
    }

    public static bool IsValidLookupInput(string? input)
    {
        return string.IsNullOrWhiteSpace(input) is false && ValidPoRegex.IsMatch(input.Trim());
    }
}
using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Receiving.Models;

/// <summary>
/// Represents a part number prefix padding rule for auto-formatting part IDs.
/// Example: Prefix "MMC", MaxLength 10, PadChar '0' → "MMC1000" becomes "MMC0001000"
/// </summary>
public partial class Model_PartNumberPrefixRule : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _prefix = string.Empty;

    [ObservableProperty]
    private int _maxLength = 10;

    [ObservableProperty]
    private char _padChar = '0';

    [ObservableProperty]
    private bool _isEnabled = true;

    /// <summary>
    /// Gets the status indicator color based on enabled state.
    /// Green (#10893E) when enabled, Red (#C42B1C) when disabled.
    /// </summary>
    public string StatusColor => IsEnabled ? "#10893E" : "#C42B1C";

    /// <summary>
    /// Gets the tooltip text for the status indicator.
    /// </summary>
    public string StatusTooltip => IsEnabled ? "Enabled" : "Disabled";

    /// <summary>
    /// Called when IsEnabled property changes. Updates dependent properties.
    /// </summary>
    partial void OnIsEnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(StatusColor));
        OnPropertyChanged(nameof(StatusTooltip));
    }

    /// <summary>
    /// Formats a part number according to this rule.
    /// </summary>
    /// <param name="input"></param>
    public string FormatPartNumber(string input)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var trimmed = input.Trim().ToUpperInvariant();
        var normalizedPrefix = Prefix.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizedPrefix))
        {
            return trimmed;
        }

        // Check if input starts with this prefix
        if (!trimmed.StartsWith(normalizedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return input; // Not applicable to this rule
        }

        // Already at or over max length - don't pad
        if (trimmed.Length >= MaxLength)
        {
            return trimmed;
        }

        // Extract the numeric/suffix part after the prefix
        string suffixPart = trimmed.Substring(normalizedPrefix.Length);

        // Calculate how many padding characters needed
        int totalPaddingNeeded = MaxLength - normalizedPrefix.Length - suffixPart.Length;

        if (totalPaddingNeeded <= 0)
        {
            return trimmed; // Already correct length or longer
        }

        // Build padded result: PREFIX + PADDING + SUFFIX
        string padding = new string(PadChar, totalPaddingNeeded);
        return $"{normalizedPrefix}{padding}{suffixPart}";
    }

    public static Model_PartNumberPrefixRule? FindBestMatch(
        Model_PartNumberPrefixRule[] rules,
        string input
    )
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var normalizedInput = input.Trim().ToUpperInvariant();
        Model_PartNumberPrefixRule? bestMatch = null;

        foreach (var rule in rules)
        {
            if (!rule.IsEnabled || string.IsNullOrWhiteSpace(rule.Prefix))
            {
                continue;
            }

            var candidatePrefix = rule.Prefix.Trim();
            if (!normalizedInput.StartsWith(candidatePrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (bestMatch == null || candidatePrefix.Length > bestMatch.Prefix.Trim().Length)
            {
                bestMatch = rule;
            }
        }

        return bestMatch;
    }

    public static string ApplyBestMatchingRule(Model_PartNumberPrefixRule[] rules, string input)
    {
        var matchingRule = FindBestMatch(rules, input);
        return matchingRule?.FormatPartNumber(input) ?? input;
    }
}

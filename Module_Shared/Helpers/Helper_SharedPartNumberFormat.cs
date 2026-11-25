using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MTM_Receiving_Application.Module_Shared.Models.Lookup;

namespace MTM_Receiving_Application.Module_Shared.Helpers;

/// <summary>
/// Canonical formatter for Infor Visual part numbers. This is the exact formatting step the
/// shared typed lookup textbox (Scanner Workbench, Receiving PO entry) applies before it
/// validates a typed value, so every module that accepts a part number shows and searches the
/// same value: trimmed, upper-cased, and prefix-padded against the configured rules
/// (<c>mmc1000</c> becomes <c>MMC0001000</c> for a Coil rule with max length 10).
/// </summary>
/// <remarks>
/// Only the formatting half of the lookup workflow is implemented here; existence checks and
/// fuzzy resolution stay in the lookup strategies and services. Pure logic with no database
/// dependency so it can be unit-tested.
/// </remarks>
public static class Helper_SharedPartNumberFormat
{
    private static readonly JsonSerializerOptions RulesJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Returns the canonical part number plus whether a padding rule matched and whether the
    /// value actually changed.
    /// </summary>
    /// <param name="rawInput">Part number exactly as typed or scanned by the operator.</param>
    /// <param name="rules">
    /// Prefix padding rules; an empty or null collection leaves the value trimmed and upper-cased.
    /// </param>
    public static Model_SharedLookupFormattingResult Format(
        string? rawInput,
        IReadOnlyList<Model_SharedLookupPrefixPaddingRule>? rules
    )
    {
        var canonical = (rawInput ?? string.Empty).Trim().ToUpperInvariant();
        if (rules is null || rules.Count == 0)
        {
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = canonical,
                HasFormattingRule = false,
                WasFormatted = false,
            };
        }

        // Longest matching prefix wins so "MMCCS" is preferred over "MMC".
        var matchingRule = rules
            .Where(static rule => rule is not null)
            .Where(rule => rule.AppliesTo(canonical))
            .OrderByDescending(rule => rule.Prefix.Trim().Length)
            .FirstOrDefault();

        if (matchingRule is null)
        {
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = canonical,
                HasFormattingRule = false,
                WasFormatted = false,
            };
        }

        var normalizedPrefix = matchingRule.Prefix.Trim().ToUpperInvariant();
        if (canonical.Length >= matchingRule.MaxLength || canonical.Length <= normalizedPrefix.Length)
        {
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = canonical,
                HasFormattingRule = true,
                WasFormatted = false,
            };
        }

        var suffix = canonical.Substring(normalizedPrefix.Length);
        var paddingCount = matchingRule.MaxLength - normalizedPrefix.Length - suffix.Length;
        if (paddingCount <= 0)
        {
            return new Model_SharedLookupFormattingResult
            {
                FormattedValue = canonical,
                HasFormattingRule = true,
                WasFormatted = false,
            };
        }

        var formatted = $"{normalizedPrefix}{new string(matchingRule.PadCharacter, paddingCount)}{suffix}";
        return new Model_SharedLookupFormattingResult
        {
            FormattedValue = formatted,
            HasFormattingRule = true,
            WasFormatted = string.Equals(formatted, canonical, StringComparison.Ordinal) is false,
        };
    }

    /// <summary>
    /// Returns the canonical part number, or the trimmed upper-case input when no padding rule
    /// applies. Use this when the caller must always have a value to display or search.
    /// </summary>
    /// <param name="rawInput">Part number exactly as typed or scanned by the operator.</param>
    /// <param name="rules">Prefix padding rules, or empty for trim/upper-case only.</param>
    public static string FormatOrPassThrough(
        string? rawInput,
        IReadOnlyList<Model_SharedLookupPrefixPaddingRule>? rules
    )
    {
        return Format(rawInput, rules).FormattedValue;
    }

    /// <summary>
    /// Reads the stored part-number padding rules JSON (the
    /// <c>Receiving.PartNumberPadding.RulesJson</c> setting shape) into the shared lookup rule
    /// type. Rules that are disabled, unnamed, or carry no usable length are dropped.
    /// </summary>
    /// <param name="rulesJson">Raw JSON string from the settings store.</param>
    public static IReadOnlyList<Model_SharedLookupPrefixPaddingRule> ParseRules(string? rulesJson)
    {
        if (string.IsNullOrWhiteSpace(rulesJson))
        {
            return [];
        }

        PaddingRuleJson[]? configured;
        try
        {
            configured = JsonSerializer.Deserialize<PaddingRuleJson[]>(rulesJson, RulesJsonOptions);
        }
        catch (JsonException)
        {
            return [];
        }

        if (configured is null)
        {
            return [];
        }

        var rules = new List<Model_SharedLookupPrefixPaddingRule>(configured.Length);
        foreach (var rule in configured)
        {
            if (rule is null || rule.IsEnabled is false)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(rule.Prefix) || rule.MaxLength <= 0)
            {
                continue;
            }

            rules.Add(
                new Model_SharedLookupPrefixPaddingRule
                {
                    Prefix = rule.Prefix.Trim(),
                    MaxLength = rule.MaxLength,
                    PadCharacter = string.IsNullOrEmpty(rule.PadChar) ? '0' : rule.PadChar[0],
                    IsEnabled = true,
                }
            );
        }

        return rules;
    }

    /// <summary>
    /// Storage shape of one configured rule. Only the fields the formatter needs are read, so
    /// presentation-only members such as the status color in the saved JSON are ignored.
    /// </summary>
    private sealed class PaddingRuleJson
    {
        public string? Prefix { get; set; }

        public int MaxLength { get; set; }

        public string? PadChar { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}

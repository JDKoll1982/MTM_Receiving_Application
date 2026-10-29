using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;

/// <summary>
/// Owns the category vocabulary shared by both Receiving Analytics tabs and maps the
/// application's three categories to and from the ERP's five categories.
/// </summary>
/// <remarks>
/// The ERP-side derivation of these categories (MMC for MMC Coils, MMF for MMF Flat,
/// matched by starts with) lives in the Infor Visual SQL and is intentionally not
/// implemented here.
/// </remarks>
public static class Translation_ReceivingAnalyticsCategory
{
    public const string ErpParts = "Parts";
    public const string ErpMmcCoils = "MMC Coils";
    public const string ErpMmfFlat = "MMF Flat";
    public const string ErpOutsideService = "Outside Service";
    public const string ErpUninventoried = "Uninventoried";

    public const string AppStandard = "Standard";
    public const string AppCoil = "Coil";
    public const string AppSheet = "Sheet";

    /// <summary>
    /// The five ERP category names, in display order.
    /// </summary>
    public static readonly IReadOnlyList<string> ErpCategories =
    [
        ErpParts,
        ErpMmcCoils,
        ErpMmfFlat,
        ErpOutsideService,
        ErpUninventoried,
    ];

    /// <summary>
    /// The three application category names, in display order.
    /// </summary>
    public static readonly IReadOnlyList<string> AppCategories =
    [
        AppStandard,
        AppCoil,
        AppSheet,
    ];

    /// <summary>
    /// Maps an application category to its ERP category, or null when the input is not one of
    /// the three application category names. Comparisons ignore case and surrounding whitespace.
    /// </summary>
    public static string? ToErpCategory(string? appCategory)
    {
        if (string.IsNullOrWhiteSpace(appCategory))
        {
            return null;
        }

        var trimmed = appCategory.Trim();

        if (trimmed.Equals(AppStandard, StringComparison.OrdinalIgnoreCase))
        {
            return ErpParts;
        }

        if (trimmed.Equals(AppCoil, StringComparison.OrdinalIgnoreCase))
        {
            return ErpMmcCoils;
        }

        if (trimmed.Equals(AppSheet, StringComparison.OrdinalIgnoreCase))
        {
            return ErpMmfFlat;
        }

        return null;
    }

    /// <summary>
    /// Maps an ERP category to its application category, or null when the ERP category has no
    /// application counterpart (Outside Service, Uninventoried) or is unknown.
    /// Comparisons ignore case and surrounding whitespace.
    /// </summary>
    public static string? ToAppCategory(string? erpCategory)
    {
        if (string.IsNullOrWhiteSpace(erpCategory))
        {
            return null;
        }

        var trimmed = erpCategory.Trim();

        if (trimmed.Equals(ErpParts, StringComparison.OrdinalIgnoreCase))
        {
            return AppStandard;
        }

        if (trimmed.Equals(ErpMmcCoils, StringComparison.OrdinalIgnoreCase))
        {
            return AppCoil;
        }

        if (trimmed.Equals(ErpMmfFlat, StringComparison.OrdinalIgnoreCase))
        {
            return AppSheet;
        }

        return null;
    }

    /// <summary>
    /// Derives the application category from a part type: contains MMC -> Coil,
    /// contains MMF -> Sheet, anything else (including null or blank) -> Standard.
    /// Comparisons ignore case.
    /// </summary>
    public static string FromPartType(string? partType)
    {
        if (string.IsNullOrWhiteSpace(partType))
        {
            return AppStandard;
        }

        var upper = partType.ToUpperInvariant();

        if (upper.Contains("MMC", StringComparison.Ordinal))
        {
            return AppCoil;
        }

        if (upper.Contains("MMF", StringComparison.Ordinal))
        {
            return AppSheet;
        }

        return AppStandard;
    }
}

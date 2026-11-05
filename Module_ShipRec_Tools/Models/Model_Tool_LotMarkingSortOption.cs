using System.Collections.Generic;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// One choice in the Sort by list.
/// </summary>
/// <param name="Field">The field the list is ordered by.</param>
/// <param name="DisplayName">The wording a user reads, phrased as the question it answers.</param>
/// <param name="DefaultDescending">
/// Whether this choice reads best highest-first. Used when the choice is picked, so a user does not
/// have to flip the direction every time they switch from a date to a count.
/// </param>
public sealed record Model_Tool_LotMarkingSortOption(
    Enum_LotMarkingSortField Field,
    string DisplayName,
    bool DefaultDescending
)
{
    /// <summary>Every sort choice, in menu order.</summary>
    public static IReadOnlyList<Model_Tool_LotMarkingSortOption> All { get; } =
    [
        new(
            Enum_LotMarkingSortField.LastReceiptDate,
            "Most recently received",
            DefaultDescending: true
        ),
        new(Enum_LotMarkingSortField.HeatNumber, "Heat / lot number", DefaultDescending: false),
        new(Enum_LotMarkingSortField.SkidCount, "Most skids", DefaultDescending: true),
        new(Enum_LotMarkingSortField.Supplier, "Supplier name", DefaultDescending: false),
        new(Enum_LotMarkingSortField.PartNumber, "Part number", DefaultDescending: false),
        new(Enum_LotMarkingSortField.ShapeAndColor, "Shape and color", DefaultDescending: false),
    ];

    /// <summary>The choice that matches a stored field, falling back to the newest-first default.</summary>
    public static Model_Tool_LotMarkingSortOption FromField(Enum_LotMarkingSortField field)
    {
        foreach (var option in All)
        {
            if (option.Field == field)
            {
                return option;
            }
        }

        return All[0];
    }
}

/// <summary>
/// One lot list column with whether a user has chosen to show it. Observable because the Filter window
/// binds a checkbox to it and reads the whole set back when the window is applied.
/// </summary>
public sealed partial class Model_Tool_LotMarkingColumnOption : ObservableObject
{
    public Model_Tool_LotMarkingColumnOption(string key, string displayName, bool isVisible)
    {
        Key = key;
        DisplayName = displayName;
        IsVisible = isVisible;
    }

    /// <summary>The stable column key, which is what gets stored and what a report looks columns up by.</summary>
    public string Key { get; }

    public string DisplayName { get; }

    [ObservableProperty]
    private bool _isVisible;
}

namespace MTM_Receiving_Application.Module_Core.Models.InforVisual;

/// <summary>
/// A single candidate returned by a fuzzy (LIKE '%term%') Infor Visual database search.
/// Used as the item type in <see cref="MTM_Receiving_Application.Module_Core.Dialogs.Dialog_FuzzySearchPicker"/>.
/// </summary>
public class Model_FuzzySearchResult
{
    /// <summary>The database key used in subsequent queries (e.g. Part ID or Vendor ID).</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Primary label shown in the picker list.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Optional secondary detail (e.g. part description, or vendor city/state).</summary>
    public string? Detail { get; set; }

    public override string ToString() => Label;
}

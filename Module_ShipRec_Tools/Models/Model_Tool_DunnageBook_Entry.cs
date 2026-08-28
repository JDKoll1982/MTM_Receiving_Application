using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Models;

/// <summary>
/// A selectable dunnage part shown in the book builder list and rendered as a book card.
/// </summary>
public partial class Model_Tool_DunnageBook_Entry : ObservableObject
{
    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private bool _isVisible = true;

    /// <summary>
    /// The owning type group. Set when the entry is attached to a group so that individual
    /// selection changes can keep the group-level checkbox in sync.
    /// </summary>
    public Model_Tool_DunnageBook_TypeGroup? Group { get; set; }

    public Model_DunnagePart Part { get; }

    /// <summary>
    /// Resolved custom UDC field label/value pairs (labels come from dunnage_custom_fields).
    /// </summary>
    public IReadOnlyList<Model_Tool_DunnageBook_FieldValue> CustomFieldValues { get; internal set; } =
        [];

    public Model_Tool_DunnageBook_Entry(Model_DunnagePart part)
    {
        Part = part;
    }

    public string PartId => Part.PartId;

    public int TypeId => Part.TypeId;

    public string TypeName => Part.DunnageTypeName;

    public string HomeLocation => Part.HomeLocation;

    public string QuantityType => Part.QuantityType;

    public string? ImagePath => Part.ImagePath;

    partial void OnIsSelectedChanged(bool value) => Group?.NotifyEntrySelectionChanged();
}

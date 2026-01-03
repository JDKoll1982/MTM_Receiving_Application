using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

/// <summary>
/// Represents a dunnage type classification.
/// Database Table: dunnage_types
/// </summary>
public partial class Model_DunnageType : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _typeName = string.Empty;

    [ObservableProperty]
    private string _icon = "\uDB81\uDF20"; // Default to box icon (Fluent System Icons)

    [ObservableProperty]
    private string _specsJson = string.Empty;

    [ObservableProperty]
    private string _createdBy = string.Empty;

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;

    [ObservableProperty]
    private string? _modifiedBy;

    [ObservableProperty]
    private DateTime? _modifiedDate;

    // Aliases for ViewModel compatibility (spec 010-dunnage-complete)

    public string DunnageType
    {
        get => TypeName;
        set => TypeName = value;
    }

    public DateTime DateAdded
    {
        get => CreatedDate;
        set => CreatedDate = value;
    }

    public string AddedBy
    {
        get => CreatedBy;
        set => CreatedBy = value;
    }

    public DateTime? LastModified
    {
        get => ModifiedDate;
        set => ModifiedDate = value;
    }

    /// <summary>
    /// Gets the MaterialIconKind enum for displaying the icon
    /// </summary>
    public MaterialIconKind IconKind
    {
        get
        {
            // Try to parse the Icon string as a MaterialIconKind enum
            if (!string.IsNullOrEmpty(Icon) && Enum.TryParse<MaterialIconKind>(Icon, true, out var kind))
            {
                return kind;
            }
            // Default to PackageVariantClosed if parsing fails
            return MaterialIconKind.PackageVariantClosed;
        }
    }
}

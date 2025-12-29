using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Models.Dunnage;

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
}

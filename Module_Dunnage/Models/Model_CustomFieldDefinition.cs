using System;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Dunnage.Models;

/// <summary>
/// Represents a user-defined custom field for dunnage types.
/// Corresponds to dunnage_custom_fields table.
/// </summary>
public partial class Model_CustomFieldDefinition : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private int _dunnageTypeId;

    [ObservableProperty]
    private string _fieldName = string.Empty;

    [ObservableProperty]
    private string _databaseColumnName = string.Empty;

    [ObservableProperty]
    private string _fieldType = "Text";

    [ObservableProperty]
    private int _displayOrder;

    [ObservableProperty]
    private bool _isRequired;

    [ObservableProperty]
    private string? _validationRules;

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;

    [ObservableProperty]
    private string _createdBy = string.Empty;

    public static string BuildDatabaseColumnName(string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(fieldName.Length);
        var previousWasUnderscore = false;

        foreach (var character in fieldName.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasUnderscore = false;
                continue;
            }

            if (previousWasUnderscore)
            {
                continue;
            }

            builder.Append('_');
            previousWasUnderscore = true;
        }

        return builder.ToString().Trim('_');
    }

    /// <summary>
    /// Returns a summary string for display in the UI
    /// </summary>
    public string GetSummary()
    {
        string required = IsRequired ? " (Required)" : string.Empty;
        return $"{FieldType}{required}";
    }
}

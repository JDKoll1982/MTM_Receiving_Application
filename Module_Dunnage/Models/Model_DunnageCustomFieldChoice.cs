namespace MTM_Receiving_Application.Module_Dunnage.Models;

/// <summary>
/// Represents an allowed choice value for a Choices-type custom field.
/// Corresponds to dunnage_custom_field_choices table.
/// </summary>
public class Model_DunnageCustomFieldChoice
{
    public int Id { get; set; }

    public int CustomFieldId { get; set; }

    public string Choice { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

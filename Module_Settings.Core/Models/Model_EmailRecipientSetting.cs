namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Represents a saved email recipient row for a module-specific settings surface.
/// </summary>
public class Model_EmailRecipientSetting
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string RecipientType { get; set; } = "To";

    public string Email { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public string OutlookFormat => $"\"{FullName}\" <{Email}>";
}

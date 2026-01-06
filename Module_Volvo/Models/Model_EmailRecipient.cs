using System.Text.Json.Serialization;

namespace MTM_Receiving_Application.Module_Volvo.Models;

/// <summary>
/// Represents an email recipient with name and email address
/// </summary>
public class Model_EmailRecipient
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Formats the recipient in Outlook format: "Name" &lt;email@domain.com&gt;
    /// </summary>
    public string ToOutlookFormat()
    {
        return $"\"{Name}\" <{Email}>";
    }
}

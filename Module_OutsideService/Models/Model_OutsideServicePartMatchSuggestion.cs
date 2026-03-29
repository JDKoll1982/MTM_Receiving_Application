namespace MTM_Receiving_Application.Module_OutsideService.Models;

/// <summary>
/// Represents one user-facing Part Match Helper suggestion.
/// </summary>
public class Model_OutsideServicePartMatchSuggestion
{
    /// <summary>
    /// Gets or sets the exact part identifier.
    /// </summary>
    public string PartId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user-facing description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the helper reason shown to users.
    /// </summary>
    public string MatchReason { get; set; } = string.Empty;
}

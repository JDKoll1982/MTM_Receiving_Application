namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// Describes a MySQL database the application can be pointed at on its next launch.
/// </summary>
public sealed class Model_DatabaseTarget
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Model_DatabaseTarget"/> class.
    /// </summary>
    /// <param name="id">Stable identifier used for UI selection.</param>
    /// <param name="displayName">Friendly label shown in the picker.</param>
    /// <param name="databaseName">Actual MySQL schema name.</param>
    /// <param name="description">Explanatory text shown under the label.</param>
    public Model_DatabaseTarget(
        string id,
        string displayName,
        string databaseName,
        string description
    )
    {
        Id = id;
        DisplayName = displayName;
        DatabaseName = databaseName;
        Description = description;
    }

    /// <summary>Gets the stable identifier used for UI selection.</summary>
    public string Id { get; }

    /// <summary>Gets the friendly label shown in the picker.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the actual MySQL schema name.</summary>
    public string DatabaseName { get; }

    /// <summary>Gets explanatory text shown under the label.</summary>
    public string Description { get; }
}

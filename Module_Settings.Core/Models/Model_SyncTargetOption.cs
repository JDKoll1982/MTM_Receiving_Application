namespace MTM_Receiving_Application.Module_Settings.Core.Models;

/// <summary>
/// A database the SyncTool may sync into, used for the action target selector.
/// </summary>
public sealed class Model_SyncTargetOption
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Model_SyncTargetOption"/> class.
    /// </summary>
    /// <param name="id">Target id passed to the tool: "core" or "backup_test".</param>
    /// <param name="label">Friendly label shown in the picker.</param>
    /// <param name="databaseName">Actual MySQL schema name.</param>
    public Model_SyncTargetOption(string id, string label, string databaseName)
    {
        Id = id;
        Label = label;
        DatabaseName = databaseName;
    }

    /// <summary>Gets the target id passed to the tool: "core" or "backup_test".</summary>
    public string Id { get; }

    /// <summary>Gets the friendly label shown in the picker.</summary>
    public string Label { get; }

    /// <summary>Gets the actual MySQL schema name.</summary>
    public string DatabaseName { get; }
}

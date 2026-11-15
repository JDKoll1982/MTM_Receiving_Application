namespace MTM_Database_Deployment.Models;

/// <summary>
/// Folder-driven classification of every script that the deployment tool ships.
/// </summary>
public enum Enum_SqlObjectKind
{
    Schema = 0,
    View = 1,
    Procedure = 2,
    Trigger = 3,
    Seed = 4
}

/// <summary>
/// One script file from the SQL object tree.
/// </summary>
public sealed class Model_SqlObject
{
    public required string Name { get; init; }
    public required string FullPath { get; init; }
    public required string RelativePath { get; init; }
    public required Enum_SqlObjectKind Kind { get; init; }

    public int SortOrder => (int)Kind;
}

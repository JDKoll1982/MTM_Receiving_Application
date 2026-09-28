namespace MTM_Receiving_Application.Module_Reprint.Models;

/// <summary>
/// One option in the Reprint page "Search By" combo box. The Key is the normalized column key
/// consumed by the per-module reprint-history stored procedures.
/// </summary>
public class Model_ReprintSearchByOption
{
    public string Key { get; init; } = string.Empty;

    public string Label { get; init; } = string.Empty;
}

using System.Collections.ObjectModel;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Response boundary for scanner run-history queries.
/// </summary>
public sealed class Model_ScannerRunHistoryQueryResult
{
    public ObservableCollection<Model_ScannerRun> Runs { get; } = [];

    public int TotalMatched { get; set; }
}

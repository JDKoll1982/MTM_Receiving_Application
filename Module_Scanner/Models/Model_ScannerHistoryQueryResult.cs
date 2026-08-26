using System.Collections.ObjectModel;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Result boundary for a scanner History query.
/// </summary>
public sealed class Model_ScannerHistoryQueryResult
{
    public ObservableCollection<Model_ScannerHistoryEntry> Entries { get; } = [];
}

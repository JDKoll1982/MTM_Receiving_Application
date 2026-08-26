using System;
using System.Collections.ObjectModel;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// One History row: a completed or stopped scanner session projected for display.
/// </summary>
public sealed partial class Model_ScannerHistoryEntry
{
	public Guid HistoryEntryId { get; set; } = Guid.NewGuid();

	public Guid SessionId { get; set; }

	public Guid ProfileId { get; set; }

	public string OwnerUserId { get; set; } = string.Empty;

	public string OwnerDisplayName { get; set; } = string.Empty;

	public string SessionName { get; set; } = string.Empty;

	public DateTime StartedUtc { get; set; }

	public DateTime? EndedUtc { get; set; }

	public Enum_ScannerSessionStatus FinalStatus { get; set; } = Enum_ScannerSessionStatus.Ready;

	public Enum_ScannerStopReason StopReason { get; set; } = Enum_ScannerStopReason.None;

	public int TotalItems { get; set; }

	public int SentItems { get; set; }

	public int FailedItems { get; set; }

	public int WaitingItems { get; set; }

	public string FailureSummary { get; set; } = string.Empty;

	public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

	public ObservableCollection<Model_ScannerHistoryItem> Items { get; } = [];
}

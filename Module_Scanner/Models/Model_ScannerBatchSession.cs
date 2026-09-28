using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Root aggregate for the user's current scanner list. Persisted so the list survives app
/// restarts; there is no separate Draft state.
/// </summary>
public sealed partial class Model_ScannerBatchSession
{
	public Guid SessionId { get; set; } = Guid.NewGuid();

	public string OwnerUserId { get; set; } = string.Empty;

	public string OwnerDisplayName { get; set; } = string.Empty;

	public string SessionName { get; set; } = string.Empty;

	public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

	public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

	public Enum_ScannerSessionStatus Status { get; set; } = Enum_ScannerSessionStatus.Ready;

	public Guid ActiveProfileId { get; set; }

	public string AppWindowTitleSnapshot { get; set; } = string.Empty;

	public string AppWindowClassSnapshot { get; set; } = string.Empty;

	public int TotalItems { get; set; }

	public int SentItems { get; set; }

	public int FailedItems { get; set; }

	public int WaitingItems { get; set; }

	public bool StopRequested { get; set; }

	public Enum_ScannerStopReason StopReason { get; set; } = Enum_ScannerStopReason.None;

	public DateTime? LastSendStartedUtc { get; set; }

	public DateTime? LastSendEndedUtc { get; set; }

	public string LastFailureMessage { get; set; } = string.Empty;

	public string Notes { get; set; } = string.Empty;

	public ObservableCollection<Model_ScannerBatchItem> Items { get; } = [];

	public void RecalculateItemCounters()
	{
		TotalItems = Items.Count;
		SentItems = Items.Count(item => item.ExecutionState == Enum_ScannerExecutionState.Sent);
		FailedItems = Items.Count(item => item.ExecutionState == Enum_ScannerExecutionState.Failed);
		WaitingItems = Items.Count(item => item.ExecutionState == Enum_ScannerExecutionState.Waiting);
		LastUpdatedUtc = DateTime.UtcNow;
	}
}

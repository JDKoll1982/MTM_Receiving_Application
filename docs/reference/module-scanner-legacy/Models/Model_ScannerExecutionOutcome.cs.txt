using System;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Result summary returned by the scanner execution service after one or more send cycles.
/// </summary>
public sealed class Model_ScannerExecutionOutcome
{
	public int SentCount { get; set; }

	public int FailedCount { get; set; }

	public int SkippedCount { get; set; }

	public bool Stopped { get; set; }

	public Guid? FirstFailureItemId { get; set; }

	public string FailureMessage { get; set; } = string.Empty;

	/// <summary>
	/// True when at least one item was sent and no failures occurred.
	/// </summary>
	public bool Completed => FailedCount == 0 && SentCount > 0;
}

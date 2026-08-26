using System;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// One immutable run-history row for a sent, failed, or skipped scanner item.
/// </summary>
public sealed partial class Model_ScannerRunItem
{
	public Guid RunItemId { get; set; } = Guid.NewGuid();

	public Guid RunId { get; set; }

	public Guid SessionId { get; set; }

	public long? SessionItemId { get; set; }

	public Guid ItemId { get; set; }

	public int SequenceNumber { get; set; }

	public string PartId { get; set; } = string.Empty;

	public string FromWarehouse { get; set; } = string.Empty;

	public string FromLocation { get; set; } = string.Empty;

	public string ToWarehouse { get; set; } = string.Empty;

	public string ToLocation { get; set; } = string.Empty;

	public string Quantity { get; set; } = string.Empty;

	public Enum_ScannerValidationState ValidationState { get; set; } =
		Enum_ScannerValidationState.NotValidated;

	public string ValidationMessage { get; set; } = string.Empty;

	public string ValidationNotes { get; set; } = string.Empty;

	public Enum_ScannerExecutionState ExecutionState { get; set; } = Enum_ScannerExecutionState.Waiting;

	public Enum_ScannerIssueType IssueType { get; set; } = Enum_ScannerIssueType.None;

	public string IssueMessage { get; set; } = string.Empty;

	public DateTime? SentUtc { get; set; }

	public DateTime? FailedUtc { get; set; }

	public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
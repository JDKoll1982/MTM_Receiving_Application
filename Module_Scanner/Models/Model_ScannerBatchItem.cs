using System;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Represents one staged scanner transfer row in the current list.
/// </summary>
public sealed partial class Model_ScannerBatchItem
{
	public Guid ItemId { get; set; } = Guid.NewGuid();

	public long? SessionItemId { get; set; }

	public Guid SessionId { get; set; }

	public int SequenceNumber { get; set; }

	public string PayloadPartId { get; set; } = string.Empty;

	public string PayloadFromWarehouse { get; set; } = string.Empty;

	public string PayloadFromLocation { get; set; } = string.Empty;

	public string PayloadToWarehouse { get; set; } = string.Empty;

	public string PayloadToLocation { get; set; } = string.Empty;

	public string PayloadQuantity { get; set; } = string.Empty;

	public Enum_ScannerValidationState ValidationState { get; set; } =
		Enum_ScannerValidationState.NotValidated;

	public string ValidationMessage { get; set; } = string.Empty;

	public string ValidationNotes { get; set; } = string.Empty;

	public string FuzzyMatchedPartId { get; set; } = string.Empty;

	public string FuzzyMatchedFromLocation { get; set; } = string.Empty;

	public string FuzzyMatchedToLocation { get; set; } = string.Empty;

	public Enum_ScannerExecutionState ExecutionState { get; set; } = Enum_ScannerExecutionState.Waiting;

	public DateTime? SentUtc { get; set; }

	public DateTime? FailedUtc { get; set; }

	public Enum_ScannerIssueType IssueType { get; set; } = Enum_ScannerIssueType.None;

	public string IssueMessage { get; set; } = string.Empty;

	public int RetryCount { get; set; }

	public DateTime? LastAttemptUtc { get; set; }

	public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

	public DateTime? LastUpdatedUtc { get; set; }

	public void ApplyValidationResult(Model_ScannerItemValidationResult result)
	{
		ValidationState = result.State;
		ValidationMessage = result.Message;
		ValidationNotes = result.Notes;
		FuzzyMatchedPartId = result.CanonicalPartId;
		FuzzyMatchedFromLocation = result.CanonicalFromLocation;
		FuzzyMatchedToLocation = result.CanonicalToLocation;
	}

	public Model_ScannerHistoryItem ToHistoryItem(Guid historyEntryId)
	{
		return new Model_ScannerHistoryItem
		{
			HistoryItemId = Guid.NewGuid(),
			HistoryEntryId = historyEntryId,
			SessionId = SessionId,
			SessionItemId = SessionItemId,
			ItemId = ItemId,
			SequenceNumber = SequenceNumber,
			PartId = PayloadPartId,
			FromWarehouse = PayloadFromWarehouse,
			FromLocation = PayloadFromLocation,
			ToWarehouse = PayloadToWarehouse,
			ToLocation = PayloadToLocation,
			Quantity = PayloadQuantity,
			ValidationState = ValidationState,
			ValidationMessage = ValidationMessage,
			ValidationNotes = ValidationNotes,
			ExecutionState = ExecutionState,
			IssueType = IssueType,
			IssueMessage = IssueMessage,
			SentUtc = SentUtc,
			FailedUtc = FailedUtc,
			CreatedUtc = CreatedUtc,
		};
	}
}

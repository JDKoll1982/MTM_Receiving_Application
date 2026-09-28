using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MTM_Receiving_Application.Module_Scanner.Models;

/// <summary>
/// Represents one staged scanner transfer row in the current list. The properties bound to
/// the editable table columns (destination, quantity, and validation status) raise change
/// notifications so the grid reflects edits and re-validation without a full list rebuild.
/// </summary>
public sealed partial class Model_ScannerBatchItem : ObservableObject
{
	private string _payloadToLocation = string.Empty;

	private string _payloadQuantity = string.Empty;

	private Enum_ScannerValidationState _validationState = Enum_ScannerValidationState.NotValidated;

	private string _validationMessage = string.Empty;

	public Guid ItemId { get; set; } = Guid.NewGuid();

	public long? SessionItemId { get; set; }

	public Guid SessionId { get; set; }

	public int SequenceNumber { get; set; }

	public string PayloadPartId { get; set; } = string.Empty;

	public string PayloadFromWarehouse { get; set; } = string.Empty;

	public string PayloadFromLocation { get; set; } = string.Empty;

	public string PayloadToWarehouse { get; set; } = string.Empty;

	public string PayloadToLocation
	{
		get => _payloadToLocation;
		set
		{
			if (SetProperty(ref _payloadToLocation, value))
			{
				OnPropertyChanged(nameof(StatusText));
			}
		}
	}

	public string PayloadQuantity
	{
		get => _payloadQuantity;
		set
		{
			if (SetProperty(ref _payloadQuantity, value))
			{
				OnPropertyChanged(nameof(StatusText));
			}
		}
	}

	/// <summary>
	/// On-hand quantity of the source location captured from the stock-location modal. Used
	/// to flag "Quantity too High" when the operator edits the row above the available stock.
	/// Kept in-memory only (not persisted); after an app restart the guard falls back to the
	/// generic quantity checks.
	/// </summary>
	public decimal? MaxQuantity { get; set; }

	public Enum_ScannerValidationState ValidationState
	{
		get => _validationState;
		set
		{
			if (SetProperty(ref _validationState, value))
			{
				OnPropertyChanged(nameof(StatusText));
				OnPropertyChanged(nameof(StatusIsValid));
			}
		}
	}

	public string ValidationMessage
	{
		get => _validationMessage;
		set
		{
			if (SetProperty(ref _validationMessage, value))
			{
				OnPropertyChanged(nameof(StatusText));
			}
		}
	}

	/// <summary>
	/// Friendly status text shown in the table Status column: "✅ Ready!" when valid,
	/// "! &lt;reason&gt;" when invalid, and a neutral hint before the destination is entered.
	/// The reason is derived from the row's own data first (so it survives an app restart),
	/// then falls back to the stored validation message.
	/// </summary>
	public string StatusText => ValidationState switch
	{
		Enum_ScannerValidationState.Valid => "✅ Ready!",
		Enum_ScannerValidationState.Invalid => $"! {BuildFriendlyMessage()}",
		_ => "⏳ Enter destination",
	};

	/// <summary>True = valid (green), false = invalid (red), null = not yet validated (gray).</summary>
	public bool? StatusIsValid => ValidationState switch
	{
		Enum_ScannerValidationState.Valid => true,
		Enum_ScannerValidationState.Invalid => false,
		_ => null,
	};

	private string BuildFriendlyMessage()
	{
		// Derive the most common issues straight from the row data so the Status column states
		// the actual problem even after the app reloads rows (ValidationMessage is not persisted).
		if (decimal.TryParse(PayloadQuantity, out var quantity))
		{
			if (MaxQuantity is decimal maxQuantity && quantity > maxQuantity)
			{
				return "Qty too High";
			}

			if (quantity <= 0)
			{
				return "Qty less than 1";
			}
		}
		else if (!string.IsNullOrWhiteSpace(PayloadQuantity))
		{
			return "Qty must be a number";
		}

		if (string.IsNullOrWhiteSpace(PayloadToLocation))
		{
			return "To location required";
		}

		if (string.IsNullOrWhiteSpace(PayloadFromLocation))
		{
			return "From location required";
		}

		// Fall back to the stored validation message when the row data is not the reason.
		var message = ValidationMessage;
		if (string.IsNullOrWhiteSpace(message))
		{
			return string.IsNullOrWhiteSpace(ValidationNotes) ? "Needs review" : ValidationNotes;
		}

		if (message.Contains("greater than zero", StringComparison.OrdinalIgnoreCase))
		{
			return "Qty less than 1";
		}

		if (
			message.Contains("To location", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("destination", StringComparison.OrdinalIgnoreCase)
		)
		{
			return "To location required";
		}

		if (
			message.Contains("From location", StringComparison.OrdinalIgnoreCase)
			|| message.Contains("source", StringComparison.OrdinalIgnoreCase)
		)
		{
			return "From location required";
		}

		return message;
	}

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

		// Restore the in-memory on-hand guard whenever live validation knows the source
		// location's available quantity (e.g., after an app restart revalidation).
		if (result.MaxQuantity.HasValue)
		{
			MaxQuantity = result.MaxQuantity;
		}
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

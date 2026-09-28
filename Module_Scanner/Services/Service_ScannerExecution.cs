using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Helpers;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Services;

/// <summary>
/// Batch send orchestration for the scanner feature.
/// Verifies the target window, emits the configured input sequence through the native input
/// engine, applies configurable settle delays, and records per-item results. While a send
/// cycle runs, <see cref="IsAutomationRunning"/> is raised so the Workbench can lock out
/// operator inputs (Enabled = false).
/// </summary>
public sealed partial class Service_ScannerExecution : ObservableObject, IService_ScannerExecution
{
	private const ushort VkTab = 0x09;

	private readonly IService_ScannerInputEngine _engine;
	private readonly Dao_ScannerBatchItem _itemDao;
	private readonly IService_LoggingUtility _logger;
	private readonly SemaphoreSlim _executionGate = new(1, 1);

	/// <summary>
	/// True while an automated send cycle is active. The Workbench binds inputs to the
	/// inverse of this value so operator edits are blocked during automation.
	/// </summary>
	[ObservableProperty]
	private bool _isAutomationRunning;

	public Service_ScannerExecution(
		IService_ScannerInputEngine engine,
		Dao_ScannerBatchItem itemDao,
		IService_LoggingUtility logger
	)
	{
		_engine = engine ?? throw new ArgumentNullException(nameof(engine));
		_itemDao = itemDao ?? throw new ArgumentNullException(nameof(itemDao));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<Model_Dao_Result<Model_ScannerExecutionOutcome>> SendNextItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerProfile profile,
		CancellationToken cancellationToken = default
	)
	{
		if (session is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerExecutionOutcome>(
				"Session is required."
			);
		}

		if (profile is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerExecutionOutcome>(
				"Profile is required."
			);
		}

		if (session.StopRequested)
		{
			return Model_Dao_Result_Factory.Success(
				new Model_ScannerExecutionOutcome { Stopped = true }
			);
		}

		var next = Helper_ScannerSequence.FindNextEligible(session.Items);
		if (next is null)
		{
			return Model_Dao_Result_Factory.Success(
				new Model_ScannerExecutionOutcome
				{
					SkippedCount = 1,
					FailureMessage = "No waiting, valid items are ready to send.",
				}
			);
		}

		return await ExecuteItemAsync(session, next, profile, cancellationToken);
	}

	public async Task<Model_Dao_Result<Model_ScannerExecutionOutcome>> SendSpecificItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerBatchItem item,
		Model_ScannerProfile profile,
		CancellationToken cancellationToken = default
	)
	{
		if (session is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerExecutionOutcome>(
				"Session is required."
			);
		}

		if (item is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerExecutionOutcome>(
				"Item is required."
			);
		}

		if (profile is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerExecutionOutcome>(
				"Profile is required."
			);
		}

		if (!Helper_ScannerSequence.IsEligibleForSend(item))
		{
			return Model_Dao_Result_Factory.Success(
				new Model_ScannerExecutionOutcome
				{
					SkippedCount = 1,
					FailureMessage = "The selected item is no longer waiting and valid.",
				}
			);
		}

		return await ExecuteItemAsync(session, item, profile, cancellationToken);
	}

	public Task<Model_Dao_Result> ClearTargetFormAsync(
		CancellationToken cancellationToken = default
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		// Alt+L clears the active Inventory Transfers form in Infor Visual.
		const uint modAlt = 0x0001;
		const ushort vkL = 0x4C;

		return _engine.SendChord(modAlt, vkL)
			? Task.FromResult(Model_Dao_Result_Factory.Success())
			: Task.FromResult(
				Model_Dao_Result_Factory.Failure("Could not clear the target form.")
			);
	}

	/// <summary>Shortcut that opens the Inventory Transfers window inside VMINVENT.</summary>
	private const string OpenWindowShortcutChord = "Alt+I";

	public async Task<Model_Dao_Result> OpenInventoryWindowAsync(
		Model_ScannerProfile profile,
		CancellationToken cancellationToken = default
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (profile is null)
		{
			return Model_Dao_Result_Factory.Failure("Profile is required.");
		}

		// Already running -> activate the target window and send the open shortcut.
		if (TryFindAndActivateTarget(profile, out var existing) && existing != IntPtr.Zero)
		{
			return SendOpenWindowShortcut();
		}

		// Launch VMINVENT (resolved via PATH / App Paths / shell association).
		try
		{
			Process.Start(
				new ProcessStartInfo(profile.TargetExecutableName) { UseShellExecute = true }
			);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(
				$"Could not launch {profile.TargetExecutableName}: {ex.Message}",
				nameof(Service_ScannerExecution)
			);
			return Model_Dao_Result_Factory.Failure(
				$"Could not launch {profile.TargetExecutableName}. Add it to PATH or App Paths and try again."
			);
		}

		// Wait for the window to appear, then activate and open it.
		for (var attempt = 0; attempt < 20; attempt++)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await Task.Delay(250, cancellationToken);

			if (TryFindAndActivateTarget(profile, out var hwnd) && hwnd != IntPtr.Zero)
			{
				return SendOpenWindowShortcut();
			}
		}

		return Model_Dao_Result_Factory.Failure(
			$"{profile.TargetExecutableName} started, but the Inventory Transfers window could not be found."
		);
	}

	private Model_Dao_Result SendOpenWindowShortcut()
	{
		if (!Service_ScannerHotkey.TryParseChord(OpenWindowShortcutChord, out var modifiers, out var virtualKey))
		{
			return Model_Dao_Result_Factory.Failure(
				$"The open-window shortcut '{OpenWindowShortcutChord}' is not a valid chord."
			);
		}

		return _engine.SendChord(modifiers, virtualKey)
			? Model_Dao_Result_Factory.Success()
			: Model_Dao_Result_Factory.Failure("Could not send the open-window shortcut.");
	}

	private async Task<Model_Dao_Result<Model_ScannerExecutionOutcome>> ExecuteItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerBatchItem item,
		Model_ScannerProfile profile,
		CancellationToken cancellationToken
	)
	{
		// Serialize execution so a hotkey press and a button click cannot overlap.
		await _executionGate.WaitAsync(cancellationToken);
		IsAutomationRunning = true;
		try
		{
			if (!Helper_ScannerSequence.IsEligibleForSend(item))
			{
				return Model_Dao_Result_Factory.Success(
					new Model_ScannerExecutionOutcome { SkippedCount = 1 }
				);
			}

			item.ExecutionState = Enum_ScannerExecutionState.Sending;
			item.LastAttemptUtc = DateTime.UtcNow;
			item.LastUpdatedUtc = DateTime.UtcNow;

			if (!await VerifyOrActivateTargetAsync(profile, cancellationToken))
			{
				return await MarkFailedAsync(
					session,
					item,
					Enum_ScannerIssueType.FocusLoss,
					"The target inventory window is not in the foreground. Bring VMINVENT to the foreground and try again."
				);
			}

			if (profile.ActivationDelayMs > 0)
			{
				await Task.Delay(profile.ActivationDelayMs, cancellationToken);
			}

			// Clear the Inventory Transfers form (Alt+L) so the emitted fields start on a
			// clean record instead of appending to a stale one.
			var clearResult = await ClearTargetFormAsync(cancellationToken);
			if (!clearResult.Success)
			{
				return await MarkFailedAsync(
					session,
					item,
					Enum_ScannerIssueType.Integrity,
					"Could not clear the target Inventory Transfers form before sending."
				);
			}

			var emitted = await EmitFieldSequenceAsync(item, profile, cancellationToken);
			if (!emitted)
			{
				return await MarkFailedAsync(
					session,
					item,
					Enum_ScannerIssueType.Integrity,
					"Input injection was blocked. The target may be running at a higher integrity level, or input was interrupted."
				);
			}

			if (profile.PauseAfterItemMs > 0)
			{
				await Task.Delay(profile.PauseAfterItemMs, cancellationToken);
			}

			item.ExecutionState = Enum_ScannerExecutionState.Sent;
			item.SentUtc = DateTime.UtcNow;
			item.LastUpdatedUtc = DateTime.UtcNow;

			session.RecalculateItemCounters();
			await PersistItemResultAsync(item);

			_logger.LogInfo(
				$"Scanner item {item.SequenceNumber} sent for session {session.SessionId}.",
				nameof(Service_ScannerExecution)
			);

			return Model_Dao_Result_Factory.Success(
				new Model_ScannerExecutionOutcome { SentCount = 1 }
			);
		}
		catch (OperationCanceledException)
		{
			return await MarkFailedAsync(
				session,
				item,
				Enum_ScannerIssueType.Unknown,
				"Send was cancelled before the input sequence completed."
			);
		}
		finally
		{
			_executionGate.Release();
			IsAutomationRunning = false;
		}
	}

	private async Task<bool> EmitFieldSequenceAsync(
		Model_ScannerBatchItem item,
		Model_ScannerProfile profile,
		CancellationToken cancellationToken
	)
	{
		var sequence = Helper_ScannerSequence.BuildFieldSequence(item);
		for (var index = 0; index < sequence.Count; index++)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var (value, tabsAfter) = sequence[index];

			// Empty fields are still advanced past with Tab so the field contract stays intact.
			if (!string.IsNullOrEmpty(value) && !_engine.SendText(value))
			{
				return false;
			}

			for (var tab = 0; tab < tabsAfter; tab++)
			{
				if (profile.DelayBetweenFieldsMs > 0)
				{
					await Task.Delay(profile.DelayBetweenFieldsMs, cancellationToken);
				}

				if (!_engine.SendKeyPress(VkTab))
				{
					return false;
				}
			}
		}

		return true;
	}

	private async Task<bool> VerifyOrActivateTargetAsync(
		Model_ScannerProfile profile,
		CancellationToken cancellationToken
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		if (!_engine.TryGetForegroundWindow(out var foregroundHwnd) || foregroundHwnd == IntPtr.Zero)
		{
			return false;
		}

		_engine.TryGetWindowProcessName(foregroundHwnd, out var foregroundProcess);
		_engine.TryGetWindowTitle(foregroundHwnd, out var foregroundTitle);

		var processMatches = Helper_ScannerSequence.IsTargetProcess(
			foregroundProcess,
			profile.TargetExecutableName
		);

		if (processMatches)
		{
			if (
				profile.RequireExactTitleMatch
				&& !Helper_ScannerSequence.IsTargetTitle(
					foregroundTitle,
					profile.TargetChildWindowTitle,
					requireExact: true
				)
			)
			{
				return false;
			}

			return true;
		}

		if (!profile.ActivateAppBeforeSend)
		{
			return false;
		}

		if (TryFindAndActivateTarget(profile, out var activatedHwnd) && activatedHwnd != IntPtr.Zero)
		{
			_engine.TryGetWindowProcessName(activatedHwnd, out var activatedProcess);
			return Helper_ScannerSequence.IsTargetProcess(
				activatedProcess,
				profile.TargetExecutableName
			);
		}

		return false;
	}

	private bool TryFindAndActivateTarget(Model_ScannerProfile profile, out IntPtr hwnd)
	{
		hwnd = IntPtr.Zero;

		if (!string.IsNullOrWhiteSpace(profile.AppWindowTitle))
		{
			if (_engine.TryFindWindow(profile.AppWindowClass, profile.AppWindowTitle, out hwnd))
			{
				return _engine.TrySetForeground(hwnd);
			}
		}

		if (
			!string.IsNullOrWhiteSpace(profile.TargetChildWindowTitle)
			&& _engine.TryFindWindow(null, profile.TargetChildWindowTitle, out hwnd)
		)
		{
			return _engine.TrySetForeground(hwnd);
		}

		return false;
	}

	private async Task<Model_Dao_Result<Model_ScannerExecutionOutcome>> MarkFailedAsync(
		Model_ScannerBatchSession session,
		Model_ScannerBatchItem item,
		Enum_ScannerIssueType issueType,
		string message
	)
	{
		item.ExecutionState = Enum_ScannerExecutionState.Failed;
		item.FailedUtc = DateTime.UtcNow;
		item.IssueType = issueType;
		item.IssueMessage = message;
		item.LastUpdatedUtc = DateTime.UtcNow;

		session.RecalculateItemCounters();
		session.Status = Enum_ScannerSessionStatus.Failed;
		session.LastFailureMessage = message;
		session.LastUpdatedUtc = DateTime.UtcNow;

		await PersistItemResultAsync(item);

		_logger.LogWarning(
			$"Scanner item {item.SequenceNumber} failed for session {session.SessionId}: {message}",
			nameof(Service_ScannerExecution)
		);

		return Model_Dao_Result_Factory.Success(
			new Model_ScannerExecutionOutcome
			{
				FailedCount = 1,
				FirstFailureItemId = item.ItemId,
				FailureMessage = message,
				Stopped = true,
			}
		);
	}

	private async Task PersistItemResultAsync(Model_ScannerBatchItem item)
	{
		var result = await _itemDao.UpsertItemAsync(item);
		if (!result.Success)
		{
			_logger.LogWarning(
				$"Unable to persist scanner item result: {result.ErrorMessage}",
				nameof(Service_ScannerExecution)
			);
		}
	}
}

using System;
using System.Threading;
using System.Threading.Tasks;
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
/// engine, applies configurable settle delays, and records per-item results. Persistence is
/// performed after each emission so database latency never stalls the input stream.
/// </summary>
public sealed class Service_ScannerExecution : IService_ScannerExecution
{
	private const ushort VkTab = 0x09;

	private readonly IService_ScannerInputEngine _engine;
	private readonly Dao_ScannerBatchItem _itemDao;
	private readonly Dao_ScannerRunHistory _runHistoryDao;
	private readonly IService_LoggingUtility _logger;
	private readonly SemaphoreSlim _executionGate = new(1, 1);

	public Service_ScannerExecution(
		IService_ScannerInputEngine engine,
		Dao_ScannerBatchItem itemDao,
		Dao_ScannerRunHistory runHistoryDao,
		IService_LoggingUtility logger
	)
	{
		_engine = engine ?? throw new ArgumentNullException(nameof(engine));
		_itemDao = itemDao ?? throw new ArgumentNullException(nameof(itemDao));
		_runHistoryDao = runHistoryDao ?? throw new ArgumentNullException(nameof(runHistoryDao));
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

	private async Task<Model_Dao_Result<Model_ScannerExecutionOutcome>> ExecuteItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerBatchItem item,
		Model_ScannerProfile profile,
		CancellationToken cancellationToken
	)
	{
		// Serialize execution so a hotkey press and a button click cannot overlap.
		await _executionGate.WaitAsync(cancellationToken);
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

			// First step: clear the Inventory Transfers form (Alt+L) so the emitted fields start
			// on a clean record instead of appending to a stale one.
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
			await RecordRunItemAsync(session, item, profile);

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

			// Emit the configured number of Tab presses to reach the next field. Tab gaps skip
			// fields that are not part of the payload (e.g. Reason after Quantity, From
			// Type/Status after From Location). The last field has no trailing tab.
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
			// Process match is authoritative. A strict child-title requirement can still fail
			// the check so the operator corrects focus before we inject.
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

		// The foreground window is not the target. If activation is allowed, try to find and
		// bring the target window forward, then re-verify by process.
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
		await RecordRunItemAsync(session, item, profile: null);

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

	private async Task RecordRunItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerBatchItem item,
		Model_ScannerProfile? profile
	)
	{
		// The run history tables key on the persisted session item id, which is not populated
		// until the stored procedure returns it. Until then, run history recording is
		// best-effort and skipped without failing the send.
		if (!item.SessionItemId.HasValue || item.SessionItemId.Value <= 0)
		{
			return;
		}

		try
		{
			var run = new Model_ScannerRun
			{
				RunId = Guid.NewGuid(),
				SessionId = session.SessionId,
				ProfileId = profile?.ProfileId ?? session.ActiveProfileId,
				OwnerUserId = session.OwnerUserId,
				OwnerDisplayName = session.OwnerDisplayName,
				StartedUtc = item.LastAttemptUtc ?? DateTime.UtcNow,
				EndedUtc = DateTime.UtcNow,
				FinalStatus = session.Status,
				StopReason = session.StopReason,
				TotalItems = 1,
				SentItems = item.ExecutionState == Enum_ScannerExecutionState.Sent ? 1 : 0,
				FailedItems = item.ExecutionState == Enum_ScannerExecutionState.Failed ? 1 : 0,
				WaitingItems = 0,
				FailureSummary = item.IssueMessage,
				CreatedUtc = DateTime.UtcNow,
			};

			var start = await _runHistoryDao.StartRunAsync(run);
			if (!start.Success)
			{
				return;
			}

			var runItem = item.ToRunItem(run.RunId);
			var insert = await _runHistoryDao.InsertRunItemAsync(runItem);
			if (!insert.Success)
			{
				return;
			}

			_ = await _runHistoryDao.CompleteRunAsync(run);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(
				$"Unable to record scanner run history: {ex.Message}",
				nameof(Service_ScannerExecution)
			);
		}
	}
}

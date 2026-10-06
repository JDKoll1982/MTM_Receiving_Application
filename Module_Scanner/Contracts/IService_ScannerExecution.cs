using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Batch send orchestration for the scanner feature.
/// Coordinates foreground-window verification, native input emission, per-item result
/// recording, input lockout, and stop-between-cycles behavior.
/// </summary>
public interface IService_ScannerExecution : INotifyPropertyChanged
{
	/// <summary>
	/// Sends the next waiting, valid item in the current list. Returns immediately with a
	/// no-op outcome when no eligible item exists or a stop is pending. On failure the item
	/// is marked failed and processing does not continue.
	/// </summary>
	Task<Model_Dao_Result<Model_ScannerExecutionOutcome>> SendNextItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerProfile profile,
		CancellationToken cancellationToken = default
	);

	/// <summary>
	/// Sends a specific item regardless of sequence position (used for retry or manual send).
	/// The item must still be eligible (waiting and valid) to be emitted.
	/// </summary>
	Task<Model_Dao_Result<Model_ScannerExecutionOutcome>> SendSpecificItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerBatchItem item,
		Model_ScannerProfile profile,
		CancellationToken cancellationToken = default
	);

	/// <summary>
	/// Sends Alt+L to the foreground Infor Visual window to clear the active Inventory
	/// Transfers form so the operator can re-enter a line after a failed send.
	/// </summary>
	Task<Model_Dao_Result> ClearTargetFormAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Opens the Infor Visual Inventory Transfers window: launches <see cref="Model_ScannerProfile.TargetExecutableName"/>
	/// (VMINVENT) when it is not already running, activates it, and sends the configured
	/// open-window shortcut (Alt+I) so the operator can reach the window.
	/// </summary>
	Task<Model_Dao_Result> OpenInventoryWindowAsync(
		Model_ScannerProfile profile,
		CancellationToken cancellationToken = default
	);

	/// <summary>
	/// True while an automated background send cycle is running. The Workbench binds this to
	/// lock out operator inputs (Enabled = false) during automation.
	/// </summary>
	bool IsAutomationRunning { get; }

	/// <summary>
	/// Poll interval used while watching for the Infor Visual part-assignment dialogs.
	/// </summary>
	TimeSpan TransferDialogPollInterval { get; set; }

	/// <summary>
	/// How long the part-assignment dialog watcher keeps polling before it stops on its own.
	/// </summary>
	TimeSpan TransferDialogWatchTimeout { get; set; }

	/// <summary>
	/// Starts a background watcher that auto-confirms the two Infor Visual dialogs which can
	/// appear after Save when a part has never been inventoried at the destination location:
	/// the "Inventory Transaction Entry" assignment question (Yes) and the "Add Part Location"
	/// form (OK). Both are confirmed by sending Enter to the dialog once it is in the foreground.
	/// <para>
	/// Intended to run while the operator is answering the Workbench "Was the transaction saved
	/// in Infor Visual?" prompt so the dialogs never block the save. Calling it again restarts
	/// the watcher, and it stops itself after <see cref="TransferDialogWatchTimeout"/>.
	/// </para>
	/// </summary>
	/// <param name="profile">Active scanner profile that identifies the Infor Visual process.</param>
	void StartTransferDialogWatcher(Model_ScannerProfile profile);

	/// <summary>
	/// Stops the part-assignment dialog watcher started by
	/// <see cref="StartTransferDialogWatcher"/>. Safe to call when no watcher is running.
	/// </summary>
	void StopTransferDialogWatcher();
}

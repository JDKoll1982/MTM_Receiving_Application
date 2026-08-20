using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Batch send orchestration for the scanner feature.
/// Coordinates foreground-window verification, native input emission, per-item result
/// recording, and stop-between-cycles behavior. Persistence stays outside the active input
/// emission path so database latency never stalls the automation stream.
/// </summary>
public interface IService_ScannerExecution
{
	/// <summary>
	/// Sends the next waiting, valid item in the session. Returns immediately with a
	/// no-op outcome when no eligible item exists or a stop is pending. On failure the item
	/// is marked failed and processing does not continue.
	/// </summary>
	Task<Model_Dao_Result<Model_ScannerExecutionOutcome>> SendNextItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerProfile profile,
		CancellationToken cancellationToken = default
	);

	/// <summary>
	/// Sends all waiting, valid items in sequence order. Stops before the next item when a
	/// stop is requested, and stops on the first failed item (previously sent rows stay sent,
	/// the failed row is marked failed, and the remainder stays waiting for operator review).
	/// </summary>
	Task<Model_Dao_Result<Model_ScannerExecutionOutcome>> SendAllAsync(
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
	/// Requests a stop that takes effect between send cycles rather than interrupting a live
	/// input sequence.
	/// </summary>
	Task<Model_Dao_Result> RequestStopAsync(Model_ScannerBatchSession session);

	/// <summary>
	/// Sends Alt+L to the foreground Infor Visual window to clear the active Inventory
	/// Transfers form so the operator can re-enter a line after a failed send.
	/// </summary>
	Task<Model_Dao_Result> ClearTargetFormAsync(CancellationToken cancellationToken = default);
}

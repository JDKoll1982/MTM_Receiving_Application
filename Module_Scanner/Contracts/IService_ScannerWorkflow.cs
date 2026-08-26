using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Workflow orchestration for the scanner module: the persisted current list and History.
/// </summary>
public interface IService_ScannerWorkflow
{
	/// <summary>
	/// Returns the operator's current list session, creating one when none exists. Called
	/// when the Workbench loads so the current list is always available without a manual
	/// "Start" action.
	/// </summary>
	Task<Model_Dao_Result<Model_ScannerBatchSession>> EnsureCurrentSessionAsync(
		string ownerUserId,
		string ownerDisplayName,
		Guid activeProfileId,
		CancellationToken cancellationToken = default
	);

	/// <summary>
	/// Adds or updates one item in the current list and persists the session header.
	/// </summary>
	Task<Model_Dao_Result<Model_ScannerBatchSession>> UpsertBatchItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerBatchItem item,
		CancellationToken cancellationToken = default
	);

	/// <summary>
	/// Replaces the full item set of a current list session (used after reorder/removal).
	/// </summary>
	Task<Model_Dao_Result<Model_ScannerBatchSession>> ReplaceSessionItemsAsync(
		Model_ScannerBatchSession session,
		CancellationToken cancellationToken = default
	);

	/// <summary>
	/// Queries scanner History: the operator's completed/stopped sessions.
	/// </summary>
	Task<Model_Dao_Result<Model_ScannerHistoryQueryResult>> GetHistoryAsync(
		Model_ScannerHistoryQueryRequest request,
		CancellationToken cancellationToken = default
	);

	Task<Model_Dao_Result<List<Model_ScannerProfile>>> GetProfilesAsync(
		string ownerUserId,
		CancellationToken cancellationToken = default
	);

	Task<Model_Dao_Result<Model_ScannerProfile>> SaveProfileAsync(
		Model_ScannerProfile profile,
		CancellationToken cancellationToken = default
	);

	Task<Model_Dao_Result> SetDefaultProfileAsync(
		Guid profileId,
		string ownerUserId,
		CancellationToken cancellationToken = default
	);

	Task<Model_Dao_Result> DeleteProfileAsync(
		Guid profileId,
		string ownerUserId,
		CancellationToken cancellationToken = default
	);
}

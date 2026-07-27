using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Contracts;

/// <summary>
/// Placeholder workflow contract for the scanner module scaffold.
/// Feature behavior will be added in a later implementation pass.
/// </summary>
public interface IService_ScannerWorkflow
{
	Task<Model_Dao_Result<Model_ScannerSessionStartResponse>> StartSessionAsync(
		Model_ScannerSessionStartRequest request,
		CancellationToken cancellationToken = default
	);

	Task<Model_Dao_Result<Model_ScannerBatchSession>> UpsertBatchItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerBatchItem item,
		CancellationToken cancellationToken = default
	);

	Task<Model_Dao_Result<Model_ScannerRun>> BuildRunSnapshotAsync(
		Model_ScannerBatchSession session,
		CancellationToken cancellationToken = default
	);

	Task<Model_Dao_Result<Model_ScannerRunHistoryQueryResult>> GetRunHistoryAsync(
		Model_ScannerRunHistoryQueryRequest request,
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
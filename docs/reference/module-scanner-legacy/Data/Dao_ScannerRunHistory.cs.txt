using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Data;

/// <summary>
/// Persists scanner run headers and run item outcomes through stored procedures.
/// </summary>
public sealed class Dao_ScannerRunHistory
{
	private readonly string _connectionString;

	public Dao_ScannerRunHistory(string connectionString)
	{
		_connectionString =
			connectionString ?? throw new ArgumentNullException(nameof(connectionString));
	}

	public async Task<Model_Dao_Result> StartRunAsync(Model_ScannerRun run)
	{
		if (run is null)
		{
			return Model_Dao_Result_Factory.Failure("Run is required.");
		}

		if (run.RunId == Guid.Empty || run.SessionId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure("Run id and session id are required.");
		}

		if (string.IsNullOrWhiteSpace(run.OwnerUserId))
		{
			return Model_Dao_Result_Factory.Failure("Owner user id is required.");
		}

		return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
			_connectionString,
			"sp_Receiving_ScannerRun_Start",
			new Dictionary<string, object>
			{
				{ "run_id", run.RunId.ToString() },
				{ "session_id", run.SessionId.ToString() },
				{ "user_id", run.OwnerUserId },
				{ "profile_id", run.ProfileId == Guid.Empty ? DBNull.Value : run.ProfileId.ToString() },
			}
		);
	}

	public async Task<Model_Dao_Result> CompleteRunAsync(Model_ScannerRun run)
	{
		if (run is null)
		{
			return Model_Dao_Result_Factory.Failure("Run is required.");
		}

		if (run.RunId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure("Run id is required.");
		}

		return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
			_connectionString,
			"sp_Receiving_ScannerRun_Complete",
			new Dictionary<string, object>
			{
				{ "run_id", run.RunId.ToString() },
				{ "sent_count", run.SentItems },
				{ "failed_count", run.FailedItems },
				{ "waiting_count", run.WaitingItems },
				{ "stop_reason", run.StopReason == Enum_ScannerStopReason.None ? DBNull.Value : run.StopReason.ToString() },
				{ "summary_message", string.IsNullOrWhiteSpace(run.FailureSummary) ? DBNull.Value : run.FailureSummary },
			}
		);
	}

	public async Task<Model_Dao_Result> InsertRunItemAsync(Model_ScannerRunItem runItem)
	{
		if (runItem is null)
		{
			return Model_Dao_Result_Factory.Failure("Run item is required.");
		}

		if (runItem.RunId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure("Run id is required.");
		}

		if (!runItem.SessionItemId.HasValue || runItem.SessionItemId.Value <= 0)
		{
			return Model_Dao_Result_Factory.Failure("Session item id is required.");
		}

		if (!decimal.TryParse(runItem.Quantity, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity))
		{
			return Model_Dao_Result_Factory.Failure("Quantity is invalid.");
		}

		return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
			_connectionString,
			"sp_Receiving_ScannerRunItem_Insert",
			new Dictionary<string, object>
			{
				{ "run_id", runItem.RunId.ToString() },
				{ "session_item_id", runItem.SessionItemId.Value },
				{ "item_order", runItem.SequenceNumber },
				{ "part_id", runItem.PartId },
				{ "from_warehouse_id", runItem.FromWarehouse },
				{ "from_location_id", string.IsNullOrWhiteSpace(runItem.FromLocation) ? DBNull.Value : runItem.FromLocation },
				{ "to_warehouse_id", runItem.ToWarehouse },
				{ "to_location_id", string.IsNullOrWhiteSpace(runItem.ToLocation) ? DBNull.Value : runItem.ToLocation },
				{ "quantity", quantity },
				{ "result_status", runItem.ExecutionState.ToString() },
				{ "attempt_count", 1 },
				{ "failure_code", runItem.IssueType == Enum_ScannerIssueType.None ? DBNull.Value : runItem.IssueType.ToString() },
				{ "failure_message", string.IsNullOrWhiteSpace(runItem.IssueMessage) ? DBNull.Value : runItem.IssueMessage },
			}
		);
	}
}
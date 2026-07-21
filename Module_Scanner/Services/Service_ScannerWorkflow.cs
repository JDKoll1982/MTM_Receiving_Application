using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Services;

/// <summary>
/// Placeholder scanner workflow service scaffold.
/// Feature behavior will be added in a later implementation pass.
/// </summary>
public sealed class Service_ScannerWorkflow : IService_ScannerWorkflow
{
	private readonly List<Model_ScannerRun> _runHistory = [];

	public Task<Model_Dao_Result<Model_ScannerSessionStartResponse>> StartSessionAsync(
		Model_ScannerSessionStartRequest request,
		CancellationToken cancellationToken = default
	)
	{
		if (string.IsNullOrWhiteSpace(request.OwnerUserId))
		{
			return Task.FromResult(
				Model_Dao_Result_Factory.Failure<Model_ScannerSessionStartResponse>(
					"OwnerUserId is required."
				)
			);
		}

		var response = new Model_ScannerSessionStartResponse
		{
			SessionId = Guid.NewGuid(),
			Status = Enum_ScannerSessionStatus.Draft,
			CreatedUtc = DateTime.UtcNow,
			Message = "Scanner session initialized.",
		};

		return Task.FromResult(Model_Dao_Result_Factory.Success(response));
	}

	public Task<Model_Dao_Result<Model_ScannerBatchSession>> UpsertBatchItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerBatchItem item,
		CancellationToken cancellationToken = default
	)
	{
		var existing = session.Items.FirstOrDefault(candidate => candidate.ItemId == item.ItemId);
		if (existing is null)
		{
			item.SessionId = session.SessionId;
			item.SequenceNumber = session.Items.Count + 1;
			session.Items.Add(item);
		}
		else
		{
			var index = session.Items.IndexOf(existing);
			item.SessionId = session.SessionId;
			item.SequenceNumber = existing.SequenceNumber;
			session.Items[index] = item;
		}

		session.RecalculateItemCounters();
		return Task.FromResult(Model_Dao_Result_Factory.Success(session));
	}

	public Task<Model_Dao_Result<Model_ScannerRun>> BuildRunSnapshotAsync(
		Model_ScannerBatchSession session,
		CancellationToken cancellationToken = default
	)
	{
		var snapshot = session.ToRunSnapshot();
		_runHistory.Add(snapshot);
		return Task.FromResult(Model_Dao_Result_Factory.Success(snapshot));
	}

	public Task<Model_Dao_Result<Model_ScannerRunHistoryQueryResult>> GetRunHistoryAsync(
		Model_ScannerRunHistoryQueryRequest request,
		CancellationToken cancellationToken = default
	)
	{
		if (string.IsNullOrWhiteSpace(request.OwnerUserId))
		{
			return Task.FromResult(
				Model_Dao_Result_Factory.Failure<Model_ScannerRunHistoryQueryResult>(
					"OwnerUserId is required."
				)
			);
		}

		IEnumerable<Model_ScannerRun> query = _runHistory.Where(run =>
			string.Equals(run.OwnerUserId, request.OwnerUserId, StringComparison.OrdinalIgnoreCase));

		if (request.DateFromUtc.HasValue)
		{
			query = query.Where(run => run.StartedUtc >= request.DateFromUtc.Value);
		}

		if (request.DateToUtc.HasValue)
		{
			query = query.Where(run => run.StartedUtc <= request.DateToUtc.Value);
		}

		if (request.StatusFilter.HasValue)
		{
			query = query.Where(run => run.FinalStatus == request.StatusFilter.Value);
		}

		var result = new Model_ScannerRunHistoryQueryResult();
		foreach (var run in query.OrderByDescending(run => run.StartedUtc).Take(request.MaxResults))
		{
			result.Runs.Add(run);
		}

		result.TotalMatched = result.Runs.Count;
		return Task.FromResult(Model_Dao_Result_Factory.Success(result));
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Services;

/// <summary>
/// Scanner workflow orchestration service backed by scanner DAOs.
/// </summary>
public sealed class Service_ScannerWorkflow : IService_ScannerWorkflow
{
	private readonly Dao_ScannerBatchSession _sessionDao;
	private readonly Dao_ScannerBatchItem _itemDao;
	private readonly Dao_ScannerRunHistory _runHistoryDao;
	private readonly Dao_ScannerProfile _profileDao;

	public Service_ScannerWorkflow(
		Dao_ScannerBatchSession sessionDao,
		Dao_ScannerBatchItem itemDao,
		Dao_ScannerRunHistory runHistoryDao,
		Dao_ScannerProfile profileDao
	)
	{
		_sessionDao = sessionDao ?? throw new ArgumentNullException(nameof(sessionDao));
		_itemDao = itemDao ?? throw new ArgumentNullException(nameof(itemDao));
		_runHistoryDao = runHistoryDao ?? throw new ArgumentNullException(nameof(runHistoryDao));
		_profileDao = profileDao ?? throw new ArgumentNullException(nameof(profileDao));
	}

	public Task<Model_Dao_Result<Model_ScannerSessionStartResponse>> StartSessionAsync(
		Model_ScannerSessionStartRequest request,
		CancellationToken cancellationToken = default
	)
	{
		if (request is null)
		{
			return Task.FromResult(
				Model_Dao_Result_Factory.Failure<Model_ScannerSessionStartResponse>(
					"Request is required."
				)
			);
		}

		if (string.IsNullOrWhiteSpace(request.OwnerUserId))
		{
			return Task.FromResult(
				Model_Dao_Result_Factory.Failure<Model_ScannerSessionStartResponse>(
					"OwnerUserId is required."
				)
			);
		}

		var session = new Model_ScannerBatchSession
		{
			SessionId = Guid.NewGuid(),
			OwnerUserId = request.OwnerUserId.Trim(),
			OwnerDisplayName = request.OwnerDisplayName?.Trim() ?? string.Empty,
			ActiveProfileId = request.ActiveProfileId,
			AppWindowTitleSnapshot = request.AppWindowTitleSnapshot?.Trim() ?? string.Empty,
			AppWindowClassSnapshot = request.AppWindowClassSnapshot?.Trim() ?? string.Empty,
			SessionName = BuildDefaultSessionName(request.OwnerDisplayName),
			Status = Enum_ScannerSessionStatus.Draft,
			CreatedUtc = DateTime.UtcNow,
			LastUpdatedUtc = DateTime.UtcNow,
		};

		return PersistAndReturnStartResponseAsync(session);
	}

	private async Task<Model_Dao_Result<Model_ScannerSessionStartResponse>> PersistAndReturnStartResponseAsync(
		Model_ScannerBatchSession session
	)
	{
		var persist = await _sessionDao.UpsertSessionAsync(session);
		if (!persist.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerSessionStartResponse>(
				persist.ErrorMessage,
				persist.Exception
			);
		}

		var response = new Model_ScannerSessionStartResponse
		{
			SessionId = session.SessionId,
			Status = Enum_ScannerSessionStatus.Draft,
			CreatedUtc = session.CreatedUtc,
			Message = "Scanner session initialized.",
		};

		return Model_Dao_Result_Factory.Success(response);
	}

	public async Task<Model_Dao_Result<Model_ScannerBatchSession>> UpsertBatchItemAsync(
		Model_ScannerBatchSession session,
		Model_ScannerBatchItem item,
		CancellationToken cancellationToken = default
	)
	{
		if (session is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
				"Session is required."
			);
		}

		if (item is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>("Item is required.");
		}

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

		var persistItem = await _itemDao.UpsertItemAsync(item);
		if (!persistItem.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
				persistItem.ErrorMessage,
				persistItem.Exception
			);
		}

		session.RecalculateItemCounters();
		var persistSession = await _sessionDao.UpsertSessionAsync(session);
		if (!persistSession.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
				persistSession.ErrorMessage,
				persistSession.Exception
			);
		}

		return Model_Dao_Result_Factory.Success(session);
	}

	public async Task<Model_Dao_Result<Model_ScannerBatchSession>> ReplaceSessionItemsAsync(
		Model_ScannerBatchSession session,
		CancellationToken cancellationToken = default
	)
	{
		if (session is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>("Session is required.");
		}

		var deleteResult = await _itemDao.DeleteBySessionAsync(session.SessionId);
		if (!deleteResult.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
				deleteResult.ErrorMessage,
				deleteResult.Exception
			);
		}

		foreach (var item in session.Items.OrderBy(candidate => candidate.SequenceNumber))
		{
			item.SessionId = session.SessionId;
			var persistItem = await _itemDao.UpsertItemAsync(item);
			if (!persistItem.Success)
			{
				return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
					persistItem.ErrorMessage,
					persistItem.Exception
				);
			}
		}

		session.RecalculateItemCounters();
		var persistSession = await _sessionDao.UpsertSessionAsync(session);
		if (!persistSession.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
				persistSession.ErrorMessage,
				persistSession.Exception
			);
		}

		return Model_Dao_Result_Factory.Success(session);
	}

	public async Task<Model_Dao_Result<Model_ScannerRun>> BuildRunSnapshotAsync(
		Model_ScannerBatchSession session,
		CancellationToken cancellationToken = default
	)
	{
		if (session is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerRun>("Session is required.");
		}

		session.LastSendStartedUtc ??= DateTime.UtcNow;
		var snapshot = session.ToRunSnapshot();

		var startRun = await _runHistoryDao.StartRunAsync(snapshot);
		if (!startRun.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerRun>(
				startRun.ErrorMessage,
				startRun.Exception
			);
		}

		foreach (var runItem in snapshot.Items.OrderBy(item => item.SequenceNumber))
		{
			var insertResult = await _runHistoryDao.InsertRunItemAsync(runItem);
			if (!insertResult.Success)
			{
				return Model_Dao_Result_Factory.Failure<Model_ScannerRun>(
					insertResult.ErrorMessage,
					insertResult.Exception
				);
			}
		}

		snapshot.EndedUtc = DateTime.UtcNow;
		var completeRun = await _runHistoryDao.CompleteRunAsync(snapshot);
		if (!completeRun.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerRun>(
				completeRun.ErrorMessage,
				completeRun.Exception
			);
		}

		return Model_Dao_Result_Factory.Success(snapshot);
	}

	public async Task<Model_Dao_Result<Model_ScannerRunHistoryQueryResult>> GetRunHistoryAsync(
		Model_ScannerRunHistoryQueryRequest request,
		CancellationToken cancellationToken = default
	)
	{
		if (request is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerRunHistoryQueryResult>(
				"Request is required."
			);
		}

		if (string.IsNullOrWhiteSpace(request.OwnerUserId))
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerRunHistoryQueryResult>(
				"OwnerUserId is required."
			);
		}

		var sessionsResult = await _sessionDao.GetSessionsByUserAsync(request.OwnerUserId);
		if (!sessionsResult.Success || sessionsResult.Data is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerRunHistoryQueryResult>(
				sessionsResult.ErrorMessage,
				sessionsResult.Exception
			);
		}

		var maxResults = request.MaxResults <= 0 ? 100 : request.MaxResults;
		var sessions = sessionsResult.Data.AsEnumerable();

		if (request.DateFromUtc.HasValue)
		{
			sessions = sessions.Where(session => session.CreatedUtc >= request.DateFromUtc.Value);
		}

		if (request.DateToUtc.HasValue)
		{
			sessions = sessions.Where(session => session.CreatedUtc <= request.DateToUtc.Value);
		}

		if (request.StatusFilter.HasValue)
		{
			sessions = sessions.Where(session => session.Status == request.StatusFilter.Value);
		}

		var result = new Model_ScannerRunHistoryQueryResult();
		foreach (
			var session in sessions
				.OrderByDescending(candidate => candidate.LastUpdatedUtc)
				.Take(maxResults)
		)
		{
			var run = new Model_ScannerRun
			{
				RunId = Guid.NewGuid(),
				SessionId = session.SessionId,
				ProfileId = session.ActiveProfileId,
				OwnerUserId = session.OwnerUserId,
				OwnerDisplayName = session.OwnerDisplayName,
				StartedUtc = session.LastSendStartedUtc ?? session.CreatedUtc,
				EndedUtc = session.LastSendEndedUtc,
				FinalStatus = session.Status,
				StopReason = session.StopReason,
				TotalItems = session.TotalItems,
				SentItems = session.SentItems,
				FailedItems = session.FailedItems,
				WaitingItems = session.WaitingItems,
				FailureSummary = session.LastFailureMessage,
				CreatedUtc = session.CreatedUtc,
			};

			var itemResult = await _itemDao.GetItemsBySessionAsync(session.SessionId);
			if (itemResult.Success && itemResult.Data is not null)
			{
				foreach (var item in itemResult.Data.OrderBy(item => item.SequenceNumber))
				{
					run.Items.Add(item.ToRunItem(run.RunId));
				}

				run.TotalItems = run.Items.Count;
			}

			result.Runs.Add(run);
		}

		result.TotalMatched = result.Runs.Count;
		return Model_Dao_Result_Factory.Success(result);
	}

	public async Task<Model_Dao_Result<List<Model_ScannerProfile>>> GetProfilesAsync(
		string ownerUserId,
		CancellationToken cancellationToken = default
	)
	{
		if (string.IsNullOrWhiteSpace(ownerUserId))
		{
			return Model_Dao_Result_Factory.Failure<List<Model_ScannerProfile>>(
				"OwnerUserId is required."
			);
		}

		cancellationToken.ThrowIfCancellationRequested();
		return await _profileDao.GetProfilesByUserAsync(ownerUserId.Trim());
	}

	public async Task<Model_Dao_Result<Model_ScannerProfile>> SaveProfileAsync(
		Model_ScannerProfile profile,
		CancellationToken cancellationToken = default
	)
	{
		if (profile is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerProfile>("Profile is required.");
		}

		if (string.IsNullOrWhiteSpace(profile.OwnerUserId))
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerProfile>(
				"OwnerUserId is required."
			);
		}

		if (string.IsNullOrWhiteSpace(profile.ProfileName))
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerProfile>(
				"Profile name is required."
			);
		}

		if (string.IsNullOrWhiteSpace(profile.AppWindowTitle))
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerProfile>(
				"App window title is required."
			);
		}

		cancellationToken.ThrowIfCancellationRequested();
		var save = await _profileDao.UpsertProfileAsync(profile);
		if (!save.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerProfile>(
				save.ErrorMessage,
				save.Exception
			);
		}

		return Model_Dao_Result_Factory.Success(profile);
	}

	public async Task<Model_Dao_Result> SetDefaultProfileAsync(
		Guid profileId,
		string ownerUserId,
		CancellationToken cancellationToken = default
	)
	{
		if (profileId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure("Profile id is required.");
		}

		if (string.IsNullOrWhiteSpace(ownerUserId))
		{
			return Model_Dao_Result_Factory.Failure("OwnerUserId is required.");
		}

		cancellationToken.ThrowIfCancellationRequested();
		return await _profileDao.SetDefaultProfileAsync(profileId, ownerUserId.Trim());
	}

	public async Task<Model_Dao_Result> DeleteProfileAsync(
		Guid profileId,
		string ownerUserId,
		CancellationToken cancellationToken = default
	)
	{
		if (profileId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure("Profile id is required.");
		}

		if (string.IsNullOrWhiteSpace(ownerUserId))
		{
			return Model_Dao_Result_Factory.Failure("OwnerUserId is required.");
		}

		cancellationToken.ThrowIfCancellationRequested();
		return await _profileDao.DeleteProfileAsync(profileId, ownerUserId.Trim());
	}

	private static string BuildDefaultSessionName(string? ownerDisplayName)
	{
		var display = string.IsNullOrWhiteSpace(ownerDisplayName) ? "Operator" : ownerDisplayName.Trim();
		return $"{display} Draft {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
	}
}
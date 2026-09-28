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
/// Scanner workflow orchestration: the persisted current list, History, and profiles.
/// </summary>
public sealed class Service_ScannerWorkflow : IService_ScannerWorkflow
{
	private readonly Dao_ScannerBatchSession _sessionDao;
	private readonly Dao_ScannerBatchItem _itemDao;
	private readonly Dao_ScannerProfile _profileDao;

	public Service_ScannerWorkflow(
		Dao_ScannerBatchSession sessionDao,
		Dao_ScannerBatchItem itemDao,
		Dao_ScannerProfile profileDao
	)
	{
		_sessionDao = sessionDao ?? throw new ArgumentNullException(nameof(sessionDao));
		_itemDao = itemDao ?? throw new ArgumentNullException(nameof(itemDao));
		_profileDao = profileDao ?? throw new ArgumentNullException(nameof(profileDao));
	}

	public async Task<Model_Dao_Result<Model_ScannerBatchSession>> EnsureCurrentSessionAsync(
		string ownerUserId,
		string ownerDisplayName,
		Guid activeProfileId,
		CancellationToken cancellationToken = default
	)
	{
		if (string.IsNullOrWhiteSpace(ownerUserId))
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
				"OwnerUserId is required."
			);
		}

		var sessionsResult = await _sessionDao.GetSessionsByUserAsync(ownerUserId);
		if (!sessionsResult.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
				sessionsResult.ErrorMessage,
				sessionsResult.Exception
			);
		}

		// Resume the most recent non-terminal session if one exists; otherwise create a new
		// current list.
		var current = sessionsResult.Data?
			.Where(static session => session.Status is
				Enum_ScannerSessionStatus.Ready or
				Enum_ScannerSessionStatus.Running or
				Enum_ScannerSessionStatus.Stopped)
			.OrderByDescending(session => session.LastUpdatedUtc)
			.FirstOrDefault();

		if (current is not null)
		{
			var loadItems = await _itemDao.GetItemsBySessionAsync(current.SessionId);
			if (loadItems.Success && loadItems.Data is not null)
			{
				foreach (var item in loadItems.Data.OrderBy(item => item.SequenceNumber))
				{
					current.Items.Add(item);
				}

				current.RecalculateItemCounters();
			}

			return Model_Dao_Result_Factory.Success(current);
		}

		var created = new Model_ScannerBatchSession
		{
			SessionId = Guid.NewGuid(),
			OwnerUserId = ownerUserId.Trim(),
			OwnerDisplayName = ownerDisplayName?.Trim() ?? string.Empty,
			ActiveProfileId = activeProfileId,
			SessionName = BuildDefaultSessionName(ownerDisplayName),
			Status = Enum_ScannerSessionStatus.Ready,
			CreatedUtc = DateTime.UtcNow,
			LastUpdatedUtc = DateTime.UtcNow,
		};

		var persist = await _sessionDao.UpsertSessionAsync(created);
		if (!persist.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
				persist.ErrorMessage,
				persist.Exception
			);
		}

		return Model_Dao_Result_Factory.Success(created);
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

		var existingIndex = -1;
		for (var index = 0; index < session.Items.Count; index++)
		{
			if (session.Items[index].ItemId == item.ItemId)
			{
				existingIndex = index;
				break;
			}
		}

		var isNew = existingIndex < 0;

		if (isNew)
		{
			item.SessionId = session.SessionId;
			item.SequenceNumber = session.Items.Count + 1;
		}
		else
		{
			item.SessionId = session.SessionId;
			item.SequenceNumber = session.Items[existingIndex].SequenceNumber;
		}

		var persistItem = await _itemDao.UpsertItemAsync(item);
		if (!persistItem.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
				persistItem.ErrorMessage,
				persistItem.Exception
			);
		}

		// Only mutate the in-memory session after the database write succeeded so a failed
		// save cannot leave a "ghost" row in the current list.
		if (isNew)
		{
			session.Items.Add(item);
		}
		else
		{
			session.Items[existingIndex] = item;
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
			return Model_Dao_Result_Factory.Failure<Model_ScannerBatchSession>(
				"Session is required."
			);
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

	public async Task<Model_Dao_Result<Model_ScannerHistoryQueryResult>> GetHistoryAsync(
		Model_ScannerHistoryQueryRequest request,
		CancellationToken cancellationToken = default
	)
	{
		if (request is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerHistoryQueryResult>(
				"Request is required."
			);
		}

		if (string.IsNullOrWhiteSpace(request.OwnerUserId))
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerHistoryQueryResult>(
				"OwnerUserId is required."
			);
		}

		var sessionsResult = await _sessionDao.GetSessionsByUserAsync(request.OwnerUserId);
		if (!sessionsResult.Success || sessionsResult.Data is null)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerHistoryQueryResult>(
				sessionsResult.ErrorMessage,
				sessionsResult.Exception
			);
		}

		// The current list (latest non-terminal session) is never part of History.
		var currentListId = sessionsResult.Data
			.Where(static session => session.Status is
				Enum_ScannerSessionStatus.Ready or
				Enum_ScannerSessionStatus.Running or
				Enum_ScannerSessionStatus.Stopped)
			.OrderByDescending(session => session.LastUpdatedUtc)
			.Select(session => session.SessionId)
			.FirstOrDefault();

		var maxResults = request.MaxResults <= 0 ? 100 : request.MaxResults;
		var sessions = sessionsResult.Data
			.Where(session => session.SessionId != currentListId)
			.AsEnumerable();

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

		var result = new Model_ScannerHistoryQueryResult();
		foreach (
			var session in sessions
				.OrderByDescending(candidate => candidate.LastUpdatedUtc)
				.Take(maxResults)
		)
		{
			var entry = new Model_ScannerHistoryEntry
			{
				HistoryEntryId = Guid.NewGuid(),
				SessionId = session.SessionId,
				ProfileId = session.ActiveProfileId,
				OwnerUserId = session.OwnerUserId,
				OwnerDisplayName = session.OwnerDisplayName,
				SessionName = session.SessionName,
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
					entry.Items.Add(item.ToHistoryItem(entry.HistoryEntryId));
				}

				entry.TotalItems = entry.Items.Count;
			}

			result.Entries.Add(entry);
		}

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

		var result = await _profileDao.GetProfilesByUserAsync(ownerUserId);
		if (!result.Success || result.Data is null)
		{
			return Model_Dao_Result_Factory.Failure<List<Model_ScannerProfile>>(
				result.ErrorMessage,
				result.Exception
			);
		}

		return Model_Dao_Result_Factory.Success(result.Data);
	}

	public async Task<Model_Dao_Result<Model_ScannerProfile>> SaveProfileAsync(
		Model_ScannerProfile profile,
		CancellationToken cancellationToken = default
	)
	{
		var persist = await _profileDao.UpsertProfileAsync(profile);
		if (!persist.Success)
		{
			return Model_Dao_Result_Factory.Failure<Model_ScannerProfile>(
				persist.ErrorMessage,
				persist.Exception
			);
		}

		if (profile.IsDefaultForUser)
		{
			await _profileDao.SetDefaultProfileAsync(profile.ProfileId, profile.OwnerUserId);
		}

		return Model_Dao_Result_Factory.Success(profile);
	}

	public Task<Model_Dao_Result> SetDefaultProfileAsync(
		Guid profileId,
		string ownerUserId,
		CancellationToken cancellationToken = default
	)
	{
		return _profileDao.SetDefaultProfileAsync(profileId, ownerUserId);
	}

	public Task<Model_Dao_Result> DeleteProfileAsync(
		Guid profileId,
		string ownerUserId,
		CancellationToken cancellationToken = default
	)
	{
		return _profileDao.DeleteProfileAsync(profileId, ownerUserId);
	}

	private static string BuildDefaultSessionName(string? ownerDisplayName)
	{
		var display = string.IsNullOrWhiteSpace(ownerDisplayName)
			? "Scanner"
			: ownerDisplayName.Trim();
		return $"{display} {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
	}
}

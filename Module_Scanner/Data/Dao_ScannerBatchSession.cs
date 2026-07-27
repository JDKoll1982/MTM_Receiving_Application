using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Data;

/// <summary>
/// Persists scanner session headers through stored procedures.
/// </summary>
public sealed class Dao_ScannerBatchSession
{
	private readonly string _connectionString;

	public Dao_ScannerBatchSession(string connectionString)
	{
		_connectionString =
			connectionString ?? throw new ArgumentNullException(nameof(connectionString));
	}

	public async Task<Model_Dao_Result> UpsertSessionAsync(Model_ScannerBatchSession session)
	{
		if (session is null)
		{
			return Model_Dao_Result_Factory.Failure("Session is required.");
		}

		if (string.IsNullOrWhiteSpace(session.OwnerUserId))
		{
			return Model_Dao_Result_Factory.Failure("Owner user id is required.");
		}

		if (string.IsNullOrWhiteSpace(session.SessionName))
		{
			return Model_Dao_Result_Factory.Failure("Session name is required.");
		}

		var parameters = new Dictionary<string, object>
		{
			{ "id", session.SessionId.ToString() },
			{ "user_id", session.OwnerUserId },
			{ "profile_id", session.ActiveProfileId == Guid.Empty ? DBNull.Value : session.ActiveProfileId.ToString() },
			{ "session_name", session.SessionName },
			{ "status", session.Status.ToString() },
			{ "stop_requested", session.StopRequested },
			{ "last_message", string.IsNullOrWhiteSpace(session.LastFailureMessage) ? DBNull.Value : session.LastFailureMessage },
		};

		return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
			_connectionString,
			"sp_Receiving_ScannerSession_Upsert",
			parameters
		);
	}

	public async Task<Model_Dao_Result<List<Model_ScannerBatchSession>>> GetSessionsByUserAsync(
		string ownerUserId
	)
	{
		if (string.IsNullOrWhiteSpace(ownerUserId))
		{
			return Model_Dao_Result_Factory.Failure<List<Model_ScannerBatchSession>>(
				"Owner user id is required."
			);
		}

		return await Helper_Database_StoredProcedure.ExecuteListAsync(
			_connectionString,
			"sp_Receiving_ScannerSession_GetByUser",
			MapSession,
			new Dictionary<string, object> { { "user_id", ownerUserId } }
		);
	}

	public async Task<Model_Dao_Result> DeleteSessionAsync(Guid sessionId, string ownerUserId)
	{
		if (sessionId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure("Session id is required.");
		}

		if (string.IsNullOrWhiteSpace(ownerUserId))
		{
			return Model_Dao_Result_Factory.Failure("Owner user id is required.");
		}

		return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
			_connectionString,
			"sp_Receiving_ScannerSession_Delete",
			new Dictionary<string, object>
			{
				{ "session_id", sessionId.ToString() },
				{ "user_id", ownerUserId },
			}
		);
	}

	private static Model_ScannerBatchSession MapSession(IDataReader reader)
	{
		var hasOwnerDisplayName = HasColumn(reader, "owner_display_name");
		var hasTotalItems = HasColumn(reader, "total_count");
		var hasStopReason = HasColumn(reader, "stop_reason");
		var hasLastSendStartedUtc = HasColumn(reader, "last_send_started_utc");
		var hasLastSendEndedUtc = HasColumn(reader, "last_send_ended_utc");

		return new Model_ScannerBatchSession
		{
			SessionId = ParseGuid(reader["id"]),
			OwnerUserId = reader["user_id"]?.ToString() ?? string.Empty,
			OwnerDisplayName =
				hasOwnerDisplayName ? reader["owner_display_name"]?.ToString() ?? string.Empty : string.Empty,
			ActiveProfileId = ParseGuid(reader["profile_id"]),
			SessionName = reader["session_name"]?.ToString() ?? string.Empty,
			Status = ParseSessionStatus(reader["status"]?.ToString()),
			TotalItems = hasTotalItems ? ParseInt(reader["total_count"]) : 0,
			StopRequested = ParseBool(reader["stop_requested"]),
			StopReason = hasStopReason ? ParseStopReason(reader["stop_reason"]?.ToString()) : Enum_ScannerStopReason.None,
			SentItems = ParseInt(reader["sent_count"]),
			FailedItems = ParseInt(reader["failed_count"]),
			WaitingItems = ParseInt(reader["waiting_count"]),
			LastFailureMessage = reader["last_message"]?.ToString() ?? string.Empty,
			CreatedUtc = ParseDateTime(reader["created_at"]),
			LastSendStartedUtc = hasLastSendStartedUtc ? ParseDateTimeOrNull(reader["last_send_started_utc"]) : null,
			LastSendEndedUtc = hasLastSendEndedUtc ? ParseDateTimeOrNull(reader["last_send_ended_utc"]) : null,
			LastUpdatedUtc = ParseDateTime(reader["updated_at"]),
		};
	}

	private static bool HasColumn(IDataReader reader, string columnName)
	{
		for (var index = 0; index < reader.FieldCount; index++)
		{
			if (string.Equals(reader.GetName(index), columnName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}

		return false;
	}

	private static Guid ParseGuid(object value)
	{
		return value == DBNull.Value ? Guid.Empty : Guid.TryParse(value.ToString(), out var guid) ? guid : Guid.Empty;
	}

	private static DateTime ParseDateTime(object value)
	{
		return value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(value);
	}

	private static DateTime? ParseDateTimeOrNull(object value)
	{
		return value == DBNull.Value ? null : Convert.ToDateTime(value);
	}

	private static int ParseInt(object value)
	{
		return value == DBNull.Value ? 0 : Convert.ToInt32(value);
	}

	private static bool ParseBool(object value)
	{
		return value != DBNull.Value && Convert.ToBoolean(value);
	}

	private static Enum_ScannerSessionStatus ParseSessionStatus(string? value)
	{
		return Enum.TryParse<Enum_ScannerSessionStatus>(value, true, out var parsed)
			? parsed
			: Enum_ScannerSessionStatus.Draft;
	}

	private static Enum_ScannerStopReason ParseStopReason(string? value)
	{
		return Enum.TryParse<Enum_ScannerStopReason>(value, true, out var parsed)
			? parsed
			: Enum_ScannerStopReason.None;
	}
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Data;

/// <summary>
/// Persists scanner user profiles through stored procedures.
/// </summary>
public sealed class Dao_ScannerProfile
{
	private readonly string _connectionString;

	public Dao_ScannerProfile(string connectionString)
	{
		_connectionString =
			connectionString ?? throw new ArgumentNullException(nameof(connectionString));
	}

	public async Task<Model_Dao_Result> UpsertProfileAsync(Model_ScannerProfile profile)
	{
		if (profile is null)
		{
			return Model_Dao_Result_Factory.Failure("Profile is required.");
		}

		if (string.IsNullOrWhiteSpace(profile.OwnerUserId))
		{
			return Model_Dao_Result_Factory.Failure("Owner user id is required.");
		}

		if (string.IsNullOrWhiteSpace(profile.ProfileName))
		{
			return Model_Dao_Result_Factory.Failure("Profile name is required.");
		}

		if (string.IsNullOrWhiteSpace(profile.AppWindowTitle))
		{
			return Model_Dao_Result_Factory.Failure("App window title is required.");
		}

		var parameters = new Dictionary<string, object>
		{
			{ "id", profile.ProfileId.ToString() },
			{ "user_id", profile.OwnerUserId },
			{ "profile_name", profile.ProfileName },
			{ "is_default", profile.IsDefaultForUser },
			{ "target_executable_name", profile.TargetExecutableName },
			{ "app_window_title", profile.AppWindowTitle },
			{ "target_child_window_title", profile.TargetChildWindowTitle },
			{ "app_window_class", string.IsNullOrWhiteSpace(profile.AppWindowClass) ? DBNull.Value : profile.AppWindowClass },
			{ "require_exact_title_match", profile.RequireExactTitleMatch },
			{ "from_warehouse_default", profile.FromWarehouseDefault },
			{ "to_warehouse_default", profile.ToWarehouseDefault },
			{ "activation_delay_ms", profile.ActivationDelayMs },
			{ "delay_between_fields_ms", profile.DelayBetweenFieldsMs },
			{ "pause_after_item_ms", profile.PauseAfterItemMs },
			{ "popup_timeout_ms", profile.PopupTimeoutMs },
			{ "popup_close_timeout_ms", profile.PopupCloseTimeoutMs },
			{ "send_shortcut_chord", profile.SendShortcutChord },
			{ "stop_shortcut_chord", profile.StopShortcutChord },
			{ "allow_advanced_timing", profile.AllowAdvancedTiming },
		};

		return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
			_connectionString,
			"sp_Receiving_ScannerProfile_Upsert",
			parameters
		);
	}

	public async Task<Model_Dao_Result<List<Model_ScannerProfile>>> GetProfilesByUserAsync(string ownerUserId)
	{
		if (string.IsNullOrWhiteSpace(ownerUserId))
		{
			return Model_Dao_Result_Factory.Failure<List<Model_ScannerProfile>>(
				"Owner user id is required."
			);
		}

		return await Helper_Database_StoredProcedure.ExecuteListAsync(
			_connectionString,
			"sp_Receiving_ScannerProfile_GetByUser",
			MapProfile,
			new Dictionary<string, object> { { "user_id", ownerUserId } }
		);
	}

	public async Task<Model_Dao_Result> SetDefaultProfileAsync(Guid profileId, string ownerUserId)
	{
		if (profileId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure("Profile id is required.");
		}

		if (string.IsNullOrWhiteSpace(ownerUserId))
		{
			return Model_Dao_Result_Factory.Failure("Owner user id is required.");
		}

		return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
			_connectionString,
			"sp_Receiving_ScannerProfile_SetDefault",
			new Dictionary<string, object>
			{
				{ "id", profileId.ToString() },
				{ "user_id", ownerUserId },
			}
		);
	}

	public async Task<Model_Dao_Result> DeleteProfileAsync(Guid profileId, string ownerUserId)
	{
		if (profileId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure("Profile id is required.");
		}

		if (string.IsNullOrWhiteSpace(ownerUserId))
		{
			return Model_Dao_Result_Factory.Failure("Owner user id is required.");
		}

		return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
			_connectionString,
			"sp_Receiving_ScannerProfile_Delete",
			new Dictionary<string, object>
			{
				{ "id", profileId.ToString() },
				{ "user_id", ownerUserId },
			}
		);
	}

	private static Model_ScannerProfile MapProfile(IDataReader reader)
	{
		return new Model_ScannerProfile
		{
			ProfileId = ParseGuid(reader["id"]),
			OwnerUserId = reader["user_id"]?.ToString() ?? string.Empty,
			ProfileName = reader["profile_name"]?.ToString() ?? string.Empty,
			IsDefaultForUser = ParseBool(reader["is_default"]),
			TargetExecutableName = reader["target_executable_name"]?.ToString() ?? string.Empty,
			AppWindowTitle = reader["app_window_title"]?.ToString() ?? string.Empty,
			TargetChildWindowTitle = reader["target_child_window_title"]?.ToString() ?? string.Empty,
			AppWindowClass = reader["app_window_class"]?.ToString() ?? string.Empty,
			RequireExactTitleMatch = ParseBool(reader["require_exact_title_match"]),
			FromWarehouseDefault = reader["from_warehouse_default"]?.ToString() ?? string.Empty,
			ToWarehouseDefault = reader["to_warehouse_default"]?.ToString() ?? string.Empty,
			ActivationDelayMs = ParseInt(reader["activation_delay_ms"]),
			DelayBetweenFieldsMs = ParseInt(reader["delay_between_fields_ms"]),
			PauseAfterItemMs = ParseInt(reader["pause_after_item_ms"]),
			PopupTimeoutMs = ParseInt(reader["popup_timeout_ms"]),
			PopupCloseTimeoutMs = ParseInt(reader["popup_close_timeout_ms"]),
			SendShortcutChord = reader["send_shortcut_chord"]?.ToString() ?? string.Empty,
			StopShortcutChord = reader["stop_shortcut_chord"]?.ToString() ?? string.Empty,
			AllowAdvancedTiming = ParseBool(reader["allow_advanced_timing"]),
			CreatedUtc = ParseDateTime(reader["created_at"]),
			LastUpdatedUtc = ParseDateTime(reader["updated_at"]),
		};
	}

	private static Guid ParseGuid(object value)
	{
		return value == DBNull.Value ? Guid.Empty : Guid.TryParse(value.ToString(), out var guid) ? guid : Guid.Empty;
	}

	private static int ParseInt(object value)
	{
		return value == DBNull.Value ? 0 : Convert.ToInt32(value);
	}

	private static DateTime ParseDateTime(object value)
	{
		return value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(value);
	}

	private static bool ParseBool(object value)
	{
		return value != DBNull.Value && Convert.ToBoolean(value);
	}
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Scanner.Models;

namespace MTM_Receiving_Application.Module_Scanner.Data;

/// <summary>
/// Persists scanner session items through stored procedures.
/// </summary>
public sealed class Dao_ScannerBatchItem
{
	private readonly string _connectionString;

	public Dao_ScannerBatchItem(string connectionString)
	{
		_connectionString =
			connectionString ?? throw new ArgumentNullException(nameof(connectionString));
	}

	public async Task<Model_Dao_Result> UpsertItemAsync(Model_ScannerBatchItem item)
	{
		if (item is null)
		{
			return Model_Dao_Result_Factory.Failure("Item is required.");
		}

		if (item.SessionId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure("Session id is required.");
		}

		if (item.SequenceNumber <= 0)
		{
			return Model_Dao_Result_Factory.Failure("Item order must be greater than zero.");
		}

		if (string.IsNullOrWhiteSpace(item.PayloadPartId))
		{
			return Model_Dao_Result_Factory.Failure("Part id is required.");
		}

		if (!decimal.TryParse(item.PayloadQuantity, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity))
		{
			return Model_Dao_Result_Factory.Failure("Quantity is invalid.");
		}

		var parameters = new Dictionary<string, object>
		{
			{ "session_id", item.SessionId.ToString() },
			{ "item_order", item.SequenceNumber },
			{ "part_id", item.PayloadPartId },
			{ "from_warehouse_id", item.PayloadFromWarehouse },
			{ "from_location_id", string.IsNullOrWhiteSpace(item.PayloadFromLocation) ? DBNull.Value : item.PayloadFromLocation },
			{ "to_warehouse_id", item.PayloadToWarehouse },
			{ "to_location_id", string.IsNullOrWhiteSpace(item.PayloadToLocation) ? DBNull.Value : item.PayloadToLocation },
			{ "quantity", quantity },
			{ "payload_json", BuildPayloadJson(item) },
			{ "validation_state", item.ValidationState.ToString() },
			{ "validation_notes", string.IsNullOrWhiteSpace(item.ValidationNotes) ? DBNull.Value : item.ValidationNotes },
		};

		return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
			_connectionString,
			"sp_Receiving_ScannerItem_Upsert",
			parameters
		);
	}

	public async Task<Model_Dao_Result<List<Model_ScannerBatchItem>>> GetItemsBySessionAsync(Guid sessionId)
	{
		if (sessionId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure<List<Model_ScannerBatchItem>>(
				"Session id is required."
			);
		}

		return await Helper_Database_StoredProcedure.ExecuteListAsync(
			_connectionString,
			"sp_Receiving_ScannerItem_GetBySession",
			MapItem,
			new Dictionary<string, object> { { "session_id", sessionId.ToString() } }
		);
	}

	public async Task<Model_Dao_Result> DeleteBySessionAsync(Guid sessionId)
	{
		if (sessionId == Guid.Empty)
		{
			return Model_Dao_Result_Factory.Failure("Session id is required.");
		}

		return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
			_connectionString,
			"sp_Receiving_ScannerItem_DeleteBySession",
			new Dictionary<string, object> { { "session_id", sessionId.ToString() } }
		);
	}

	private static string BuildPayloadJson(Model_ScannerBatchItem item)
	{
		var payload = new
		{
			item.PayloadPartId,
			item.PayloadFromWarehouse,
			item.PayloadFromLocation,
			item.PayloadToWarehouse,
			item.PayloadToLocation,
			item.PayloadQuantity,
			item.PayloadUnitOfMeasure,
			item.PayloadLotOrSerial,
			item.PayloadReferenceText,
			item.NavigationPattern,
		};

		return JsonSerializer.Serialize(payload);
	}

	private static Model_ScannerBatchItem MapItem(IDataReader reader)
	{
		return new Model_ScannerBatchItem
		{
			SessionItemId = ParseLong(reader["id"]),
			SessionId = ParseGuid(reader["session_id"]),
			SequenceNumber = ParseInt(reader["item_order"]),
			PayloadPartId = reader["part_id"]?.ToString() ?? string.Empty,
			PayloadFromWarehouse = reader["from_warehouse_id"]?.ToString() ?? string.Empty,
			PayloadFromLocation = reader["from_location_id"]?.ToString() ?? string.Empty,
			PayloadToWarehouse = reader["to_warehouse_id"]?.ToString() ?? string.Empty,
			PayloadToLocation = reader["to_location_id"]?.ToString() ?? string.Empty,
			PayloadQuantity = Convert.ToDecimal(reader["quantity"], CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
			ValidationState = ParseValidationState(reader["validation_state"]?.ToString()),
			ValidationNotes = reader["validation_notes"]?.ToString() ?? string.Empty,
			ExecutionState = ParseExecutionState(reader["status"]?.ToString()),
			IssueType = ParseIssueType(reader["failure_code"]?.ToString()),
			IssueMessage = reader["failure_message"]?.ToString() ?? string.Empty,
			LastUpdatedUtc = ParseDateTimeOrNull(reader["updated_at"]),
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

	private static long? ParseLong(object value)
	{
		return value == DBNull.Value ? null : Convert.ToInt64(value);
	}

	private static DateTime? ParseDateTimeOrNull(object value)
	{
		return value == DBNull.Value ? null : Convert.ToDateTime(value);
	}

	private static Enum_ScannerValidationState ParseValidationState(string? value)
	{
		return Enum.TryParse<Enum_ScannerValidationState>(value, true, out var parsed)
			? parsed
			: Enum_ScannerValidationState.NotValidated;
	}

	private static Enum_ScannerExecutionState ParseExecutionState(string? value)
	{
		return Enum.TryParse<Enum_ScannerExecutionState>(value, true, out var parsed)
			? parsed
			: Enum_ScannerExecutionState.Waiting;
	}

	private static Enum_ScannerIssueType ParseIssueType(string? failureCode)
	{
		return Enum.TryParse<Enum_ScannerIssueType>(failureCode, true, out var parsed)
			? parsed
			: Enum_ScannerIssueType.None;
	}
}
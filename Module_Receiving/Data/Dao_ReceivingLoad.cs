using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Receiving.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Receiving.Data;

public class Dao_ReceivingLoad
{
    private readonly string _connectionString;
    private const string DefaultInitialLocation = "Nothing Entered";

    public Dao_ReceivingLoad(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private string? CleanPONumber(string? poNumber)
    {
        if (string.IsNullOrEmpty(poNumber))
        {
            return null;
        }

        return poNumber.Replace("PO-", "", StringComparison.OrdinalIgnoreCase).Trim();
    }

    private static bool IsDuplicateKeyError(Model_Dao_Result result)
    {
        if (result.Exception is MySqlException mySqlException && mySqlException.Number == 1062)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(result.ErrorMessage))
        {
            return false;
        }

        var errorMessage = result.ErrorMessage;
        return errorMessage.Contains("Duplicate entry", StringComparison.OrdinalIgnoreCase)
            && (
                errorMessage.Contains("1062", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("PRIMARY", StringComparison.OrdinalIgnoreCase)
            );
    }

    private static string NormalizeLocation(string? location)
    {
        return string.IsNullOrWhiteSpace(location) ? DefaultInitialLocation : location.Trim();
    }

    public async Task<Model_Dao_Result<int>> SaveLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        if (loads == null || loads.Count == 0)
        {
            return Model_Dao_Result_Factory.Failure<int>("Loads list cannot be null or empty");
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            int savedCount = 0;

            // Pre-compute per-part skid totals and sequences for the "N of M" label counter.
            var partTotals = loads
                .GroupBy(l => l.PartID, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
            var partSequenceCounters = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase
            );

            foreach (var load in loads)
            {
                if (!partSequenceCounters.ContainsKey(load.PartID))
                {
                    partSequenceCounters[load.PartID] = 0;
                }
                partSequenceCounters[load.PartID]++;
                int skidSequence = partSequenceCounters[load.PartID];
                int skidTotal = partTotals[load.PartID];

                var roundedQuantity = Convert.ToInt32(
                    Math.Round(load.WeightQuantity, 0, MidpointRounding.AwayFromZero)
                );
                var cleanedPoNumber = CleanPONumber(load.PoNumber);
                object poNumber = cleanedPoNumber is null ? DBNull.Value : cleanedPoNumber;
                object poVendor = string.IsNullOrWhiteSpace(load.PoVendor)
                    ? DBNull.Value
                    : load.PoVendor;
                object poStatus = string.IsNullOrWhiteSpace(load.PoStatus)
                    ? DBNull.Value
                    : load.PoStatus;
                object poDueDate = load.PoDueDate.HasValue
                    ? load.PoDueDate.Value.Date
                    : DBNull.Value;
                object userId = string.IsNullOrWhiteSpace(load.UserId) ? DBNull.Value : load.UserId;
                object vendorName = string.IsNullOrWhiteSpace(load.PoVendor)
                    ? DBNull.Value
                    : load.PoVendor;
                object qualityHoldRestrictionType = string.IsNullOrWhiteSpace(
                    load.QualityHoldRestrictionType
                )
                    ? DBNull.Value
                    : load.QualityHoldRestrictionType;
                var parameters = new Dictionary<string, object>
                {
                    { "load_id", load.LoadID.ToString() },
                    { "load_number", load.LoadNumber },
                    { "quantity", roundedQuantity },
                    { "weight_quantity", load.WeightQuantity },
                    { "part_id", load.PartID },
                    { "part_description", load.PartDescription ?? string.Empty },
                    { "part_type", load.PartType ?? string.Empty },
                    { "po_number", poNumber },
                    { "po_line_number", load.PoLineNumber ?? string.Empty },
                    { "po_vendor", poVendor },
                    { "po_status", poStatus },
                    { "po_due_date", poDueDate },
                    { "qty_ordered", load.QtyOrdered },
                    { "unit_of_measure", load.UnitOfMeasure ?? "EA" },
                    { "remaining_quantity", load.RemainingQuantity },
                    { "employee_number", load.EmployeeNumber },
                    { "user_id", userId },
                    { "heat", load.HeatLotNumber ?? string.Empty },
                    { "received_date", load.ReceivedDate },
                    { "transaction_date", load.ReceivedDate.Date },
                    { "initial_location", NormalizeLocation(load.InitialLocation) },
                    { "packages_per_load", load.PackagesPerLoad },
                    { "package_type_name", load.PackageTypeName ?? string.Empty },
                    { "weight_per_package", load.WeightPerPackage },
                    { "coils_on_skid", (object)DBNull.Value },
                    { "label_number", load.LoadNumber },
                    { "vendor_name", vendorName },
                    { "is_non_po_item", load.IsNonPOItem },
                    { "is_quality_hold_required", load.IsQualityHoldRequired },
                    { "is_quality_hold_acknowledged", load.IsQualityHoldAcknowledged },
                    { "quality_hold_restriction_type", qualityHoldRestrictionType },
                    { "part_skid_sequence", skidSequence },
                    { "part_skid_total", skidTotal },
                };

                var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_Receiving_LabelData_Insert",
                    parameters
                );

                if (!result.Success)
                {
                    if (IsDuplicateKeyError(result))
                    {
                        continue;
                    }

                    throw new InvalidOperationException(result.ErrorMessage, result.Exception);
                }

                savedCount++;
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success<int>(savedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Model_Dao_Result_Factory.Failure<int>($"Failed to save loads: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> UpdateLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        if (loads == null || loads.Count == 0)
        {
            return Model_Dao_Result_Factory.Failure<int>("Loads list cannot be null or empty");
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            int updatedCount = 0;

            foreach (var load in loads)
            {
                if (load.LoadID == Guid.Empty && !load.HistoryRecordID.HasValue)
                {
                    throw new InvalidOperationException(
                        "Cannot update a receiving history row without a persisted identifier."
                    );
                }

                object loadGuid = load.LoadID == Guid.Empty ? DBNull.Value : load.LoadID.ToString();
                object historyRecordId = load.HistoryRecordID.HasValue
                    ? load.HistoryRecordID.Value
                    : DBNull.Value;
                var cleanedPoNumber = CleanPONumber(load.PoNumber);
                object poNumber = cleanedPoNumber is null ? DBNull.Value : cleanedPoNumber;
                var parameters = new Dictionary<string, object>
                {
                    { "LoadID", loadGuid },
                    { "HistoryRecordID", historyRecordId },
                    { "PartID", load.PartID },
                    { "PartType", load.PartType },
                    { "PONumber", poNumber },
                    { "POLineNumber", load.PoLineNumber },
                    { "LoadNumber", load.LoadNumber },
                    { "WeightQuantity", load.WeightQuantity },
                    { "HeatLotNumber", load.HeatLotNumber },
                    { "InitialLocation", NormalizeLocation(load.InitialLocation) },
                    { "PackagesPerLoad", load.PackagesPerLoad },
                    { "PackageTypeName", load.PackageTypeName },
                    { "WeightPerPackage", load.WeightPerPackage },
                    { "IsNonPOItem", load.IsNonPOItem },
                    { "ReceivedDate", load.ReceivedDate },
                };

                var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_Receiving_Load_Update",
                    parameters
                );

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.ErrorMessage, result.Exception);
                }

                if (result.AffectedRows <= 0)
                {
                    var historyRowExists = await ReceivingHistoryRowExistsAsync(
                        connection,
                        transaction,
                        load
                    );

                    if (!historyRowExists)
                    {
                        throw new InvalidOperationException(
                            $"No receiving history row matched the update request for load '{load.LoadNumber}'."
                        );
                    }
                }

                updatedCount++;
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success<int>(updatedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Model_Dao_Result_Factory.Failure<int>(
                $"Failed to update loads: {ex.Message}",
                ex
            );
        }
    }

    private static async Task<bool> ReceivingHistoryRowExistsAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        Model_ReceivingLoad load
    )
    {
        const string sql = @"
SELECT 1
FROM receiving_history
WHERE (@historyRecordId IS NOT NULL AND id = @historyRecordId)
   OR (
        @historyRecordId IS NULL
        AND @loadId IS NOT NULL
        AND @loadId <> ''
        AND load_guid = @loadId
   )
LIMIT 1;";

        object historyRecordId = load.HistoryRecordID.HasValue
            ? load.HistoryRecordID.Value
            : DBNull.Value;
        object loadId = load.LoadID == Guid.Empty ? DBNull.Value : load.LoadID.ToString();

        await using var command = new MySqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@historyRecordId", historyRecordId);
        command.Parameters.AddWithValue("@loadId", loadId);

        var scalar = await command.ExecuteScalarAsync();
        return scalar != null && scalar != DBNull.Value;
    }

    public async Task<Model_Dao_Result<int>> DeleteLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        if (loads == null || loads.Count == 0)
        {
            return Model_Dao_Result_Factory.Success<int>(0);
        }

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            int deletedCount = 0;

            foreach (var load in loads)
            {
                if (load.LoadID == Guid.Empty && !load.HistoryRecordID.HasValue)
                {
                    throw new InvalidOperationException(
                        "Cannot delete a receiving history row without a persisted identifier."
                    );
                }

                object loadGuid = load.LoadID == Guid.Empty ? DBNull.Value : load.LoadID.ToString();
                object historyRecordId = load.HistoryRecordID.HasValue
                    ? load.HistoryRecordID.Value
                    : DBNull.Value;
                var parameters = new Dictionary<string, object>
                {
                    { "LoadID", loadGuid },
                    { "HistoryRecordID", historyRecordId },
                };

                var result = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_Receiving_Load_Delete",
                    parameters
                );

                if (!result.Success)
                {
                    throw new InvalidOperationException(result.ErrorMessage, result.Exception);
                }

                if (result.AffectedRows <= 0)
                {
                    // sp_Receiving_Load_Delete restores FOREIGN_KEY_CHECKS as its final
                    // statement, so MySQL reports that trailing SET's row count (0) instead
                    // of the DELETE's. Only treat the delete as failed when the row is
                    // genuinely still present.
                    var historyRowStillExists = await ReceivingHistoryRowExistsAsync(
                        connection,
                        transaction,
                        load
                    );

                    if (historyRowStillExists)
                    {
                        throw new InvalidOperationException(
                            $"No receiving history row matched the delete request for load '{load.LoadNumber}'."
                        );
                    }
                }

                deletedCount++;
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success<int>(deletedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Model_Dao_Result_Factory.Failure<int>(
                $"Failed to delete loads: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetHistoryAsync(
        string partID,
        DateTime startDate,
        DateTime endDate
    )
    {
        try
        {
            var loads = new List<Model_ReceivingLoad>();
            var parameters = new Dictionary<string, object>
            {
                { "PartID", partID },
                { "StartDate", startDate },
                { "EndDate", endDate },
            };

            var result = await Helper_Database_StoredProcedure.ExecuteDataTableAsync(
                _connectionString,
                "sp_Receiving_History_Get",
                parameters
            );

            if (result.IsSuccess && result.Data != null)
            {
                foreach (DataRow row in result.Data.Rows)
                {
                    loads.Add(MapRowToLoad(row));
                }
                return Model_Dao_Result_Factory.Success(loads);
            }
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(
                $"Error retrieving history: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Loads receiving history rows for the Reprint Labels page, including whether each row is
    /// already queued for reprint (an <c>is_reprint = 1</c> row exists in receiving_label_data).
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> GetReprintHistoryAsync(
        Model_ReprintHistoryFilter filter
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_start_date", filter.StartDate is null ? DBNull.Value : filter.StartDate.Value.Date },
            { "p_end_date", filter.EndDate is null ? DBNull.Value : filter.EndDate.Value.Date },
            {
                "p_search_by",
                string.IsNullOrWhiteSpace(filter.SearchBy) ? "part" : filter.SearchBy
            },
            {
                "p_search_text",
                string.IsNullOrWhiteSpace(filter.SearchText) ? "" : filter.SearchText.Trim()
            },
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Receiving_History_GetForReprint",
            MapReprintHistoryRow,
            parameters
        );
    }

    private static Model_ReprintHistoryRow MapReprintHistoryRow(IDataReader reader)
    {
        var poNumber = reader.IsDBNull(reader.GetOrdinal("po_number"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("po_number")).Trim();
        var loadNumber = reader.IsDBNull(reader.GetOrdinal("load_number"))
            ? (int?)null
            : reader.GetInt32(reader.GetOrdinal("load_number"));
        var labelNumber = reader.IsDBNull(reader.GetOrdinal("label_number"))
            ? (int?)null
            : reader.GetInt32(reader.GetOrdinal("label_number"));

        return new Model_ReprintHistoryRow
        {
            HistoryId = reader.GetInt32(reader.GetOrdinal("history_id")).ToString(),
            RecordDate = reader.GetDateTime(reader.GetOrdinal("record_date")),
            Part = reader.GetString(reader.GetOrdinal("part_id")),
            Quantity = reader.IsDBNull(reader.GetOrdinal("quantity"))
                ? 0m
                : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("quantity"))),
            Reference = BuildReceivingReference(poNumber, loadNumber, labelNumber),
            AlreadyQueued =
                !reader.IsDBNull(reader.GetOrdinal("already_queued"))
                && reader.GetInt32(reader.GetOrdinal("already_queued")) == 1,
        };
    }

    private static string BuildReceivingReference(
        string poNumber,
        int? loadNumber,
        int? labelNumber
    )
    {
        if (!string.IsNullOrWhiteSpace(poNumber))
        {
            return poNumber;
        }

        if (loadNumber.HasValue)
        {
            return $"Load {loadNumber.Value}";
        }

        return labelNumber.HasValue ? $"Label {labelNumber.Value}" : string.Empty;
    }

    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetAllAsync(
        DateTime startDate,
        DateTime endDate
    )
    {
        try
        {
            var loads = new List<Model_ReceivingLoad>();
            var parameters = new Dictionary<string, object>
            {
                { "StartDate", startDate },
                { "EndDate", endDate },
            };

            var result = await Helper_Database_StoredProcedure.ExecuteDataTableAsync(
                _connectionString,
                "sp_Receiving_Load_GetAll",
                parameters
            );

            if (result.IsSuccess && result.Data != null)
            {
                foreach (DataRow row in result.Data.Rows)
                {
                    loads.Add(MapRowToLoad(row));
                }
                return Model_Dao_Result_Factory.Success(loads);
            }
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(
                $"Error retrieving all loads: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<int>> ClearLabelDataToHistoryAsync(string archivedBy)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(
                "sp_Receiving_LabelData_ClearToHistory",
                connection
            )
            {
                CommandType = CommandType.StoredProcedure,
            };

            command.Parameters.AddWithValue("p_archived_by", archivedBy ?? "SYSTEM");

            var rowsMovedParam = new MySqlParameter("p_rows_moved", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output,
            };
            command.Parameters.Add(rowsMovedParam);

            var batchIdParam = new MySqlParameter("p_archive_batch_id", MySqlDbType.VarChar, 36)
            {
                Direction = ParameterDirection.Output,
            };
            command.Parameters.Add(batchIdParam);

            var statusParam = new MySqlParameter("p_status", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output,
            };
            command.Parameters.Add(statusParam);

            var errorParam = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 1000)
            {
                Direction = ParameterDirection.Output,
            };
            command.Parameters.Add(errorParam);

            await command.ExecuteNonQueryAsync();

            var status = statusParam.Value == DBNull.Value ? 1 : Convert.ToInt32(statusParam.Value);
            var errorMessage =
                errorParam.Value == DBNull.Value ? null : errorParam.Value?.ToString();

            if (status != 0)
            {
                return Model_Dao_Result_Factory.Failure<int>(
                    errorMessage ?? "Clear Label Data failed"
                );
            }

            var rowsMoved =
                rowsMovedParam.Value == DBNull.Value ? 0 : Convert.ToInt32(rowsMovedParam.Value);
            return new Model_Dao_Result<int>
            {
                Success = true,
                Data = rowsMoved,
                AffectedRows = rowsMoved,
                ErrorMessage = string.Empty,
            };
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Failed to clear label data to history: {ex.Message}",
                ex
            );
        }
    }

    private Model_ReceivingLoad MapRowToLoad(DataRow row)
    {
        return new Model_ReceivingLoad
        {
            HistoryRecordID = ReadNullableInt(row, "id"),
            LoadID = ReadGuid(row, "load_id", "load_guid"),
            PartID = ReadString(row, "part_id"),
            PartDescription = ReadString(row, "part_description"),
            PartType = ReadString(row, "part_type"),
            PoNumber = ReadNullableString(row, "po_number"),
            PoLineNumber = ReadString(row, "po_line_number"),
            PoVendor = ReadString(row, "po_vendor", "vendor_name"),
            PoStatus = ReadString(row, "po_status"),
            PoDueDate = ReadNullableDateTime(row, "po_due_date"),
            QtyOrdered = ReadDecimal(row, "qty_ordered"),
            UnitOfMeasure = string.IsNullOrWhiteSpace(ReadString(row, "unit_of_measure"))
                ? "EA"
                : ReadString(row, "unit_of_measure"),
            RemainingQuantity = ReadInt(row, "remaining_quantity"),
            LoadNumber = ReadInt(row, "load_number", "label_number"),
            WeightQuantity = ReadDecimal(row, "weight_quantity", "quantity"),
            HeatLotNumber = ReadString(row, "heat"),
            InitialLocation = ReadString(row, "initial_location"),
            PackagesPerLoad = ReadInt(row, "packages_per_load"),
            PackageTypeName = ReadString(row, "package_type_name"),
            WeightPerPackage = ReadDecimal(row, "weight_per_package"),
            IsNonPOItem = ReadBool(row, "is_non_po_item"),
            ReceivedDate = ReadDateTime(row, "received_date", "created_at", "transaction_date"),
            UserId = ReadNullableString(row, "user_id"),
            EmployeeNumber = ReadInt(row, "employee_number"),
            IsQualityHoldRequired = ReadBool(row, "is_quality_hold_required"),
            IsQualityHoldAcknowledged = ReadBool(row, "is_quality_hold_acknowledged"),
            QualityHoldRestrictionType = ReadString(row, "quality_hold_restriction_type"),
        };
    }

    private static bool HasColumn(DataRow row, string columnName)
    {
        return row.Table.Columns.Contains(columnName);
    }

    private static object? ReadValue(DataRow row, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            if (HasColumn(row, columnName) && row[columnName] != DBNull.Value)
            {
                return row[columnName];
            }
        }

        return null;
    }

    private static string ReadString(DataRow row, params string[] columnNames)
    {
        return ReadValue(row, columnNames)?.ToString() ?? string.Empty;
    }

    private static string? ReadNullableString(DataRow row, params string[] columnNames)
    {
        return ReadValue(row, columnNames)?.ToString();
    }

    private static int ReadInt(DataRow row, params string[] columnNames)
    {
        var value = ReadValue(row, columnNames);
        if (value is null)
        {
            return 0;
        }

        return Convert.ToInt32(value);
    }

    private static int? ReadNullableInt(DataRow row, params string[] columnNames)
    {
        var value = ReadValue(row, columnNames);
        return value is null ? null : Convert.ToInt32(value);
    }

    private static decimal ReadDecimal(DataRow row, params string[] columnNames)
    {
        var value = ReadValue(row, columnNames);
        if (value is null)
        {
            return 0;
        }

        return Convert.ToDecimal(value);
    }

    private static bool ReadBool(DataRow row, params string[] columnNames)
    {
        var value = ReadValue(row, columnNames);
        if (value is null)
        {
            return false;
        }

        return Convert.ToBoolean(value);
    }

    private static DateTime ReadDateTime(DataRow row, params string[] columnNames)
    {
        var value = ReadValue(row, columnNames);
        if (value is null)
        {
            return DateTime.Now;
        }

        return Convert.ToDateTime(value);
    }

    private static DateTime? ReadNullableDateTime(DataRow row, params string[] columnNames)
    {
        var value = ReadValue(row, columnNames);
        return value is null ? null : Convert.ToDateTime(value);
    }

    private static Guid ReadGuid(DataRow row, params string[] columnNames)
    {
        var value = ReadValue(row, columnNames);
        if (value is null)
        {
            return Guid.Empty;
        }

        var raw = value.ToString();
        return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
    }
}

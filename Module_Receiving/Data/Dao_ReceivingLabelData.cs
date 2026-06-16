using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Receiving.Data;

public class Dao_ReceivingLabelData
{
    private readonly string _connectionString;
    private const string DefaultInitialLocation = "Nothing Entered";
    private const int MaxTransactionRetries = 3;
    private static readonly int[] RetryDelaysMs = [150, 400, 900];

    public Dao_ReceivingLabelData(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    private static string? CleanPONumber(string? poNumber)
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

    private static bool IsTransientTransactionError(Exception exception)
    {
        if (exception is MySqlException mySqlException)
        {
            return mySqlException.Number is 1205 or 1213;
        }

        return false;
    }

    public async Task<Model_Dao_Result<int>> SaveLoadsAsync(List<Model_ReceivingLoad> loads)
    {
        if (loads == null || loads.Count == 0)
        {
            return Model_Dao_Result_Factory.Failure<int>("Loads list cannot be null or empty");
        }

        var orderedLoads = loads
            .OrderBy(load => load.PartID, StringComparer.OrdinalIgnoreCase)
            .ThenBy(load => load.LoadNumber)
            .ThenBy(load => load.LoadID)
            .ToList();

        for (var attempt = 1; attempt <= MaxTransactionRetries; attempt++)
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                int savedCount = 0;

                // Pre-compute per-part skid totals and sequences for the "N of M" label counter.
                // Group by PartID to get the total skids per part, then track per-part position.
                var partTotals = orderedLoads
                    .GroupBy(load => load.PartID, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Count(),
                        StringComparer.OrdinalIgnoreCase
                    );
                var partSequenceCounters = new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase
                );

                foreach (var load in orderedLoads)
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
                    object userId = string.IsNullOrWhiteSpace(load.UserId)
                        ? DBNull.Value
                        : load.UserId;
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
                        { "user_set_customer_name", load.UserSetCustomerName },
                        { "user_set_variable", load.UserSetVariable },
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

                        throw result.Exception
                            ?? new InvalidOperationException(result.ErrorMessage);
                    }

                    savedCount++;
                }

                await transaction.CommitAsync();
                return Model_Dao_Result_Factory.Success<int>(savedCount);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (attempt < MaxTransactionRetries && IsTransientTransactionError(ex))
                {
                    await Task.Delay(RetryDelaysMs[attempt - 1]);
                    continue;
                }

                return Model_Dao_Result_Factory.Failure<int>(
                    $"Failed to save label data loads: {ex.Message}",
                    ex
                );
            }
        }

        return Model_Dao_Result_Factory.Failure<int>(
            "Failed to save label data loads after retrying transient database errors."
        );
    }

    public async Task<Model_Dao_Result<int>> ClearLabelDataToHistoryAsync(
        string archivedBy,
        int employeeNumber,
        bool clearAllRows
    )
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
            command.Parameters.AddWithValue("p_employee_number", employeeNumber);
            command.Parameters.AddWithValue("p_clear_all", clearAllRows);

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
            return Model_Dao_Result_Factory.Success<int>(rowsMoved);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Failed to clear label data to history: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<List<Model_ReceivingLoad>>> GetCurrentLabelDataAsync()
    {
        try
        {
            var loads = new List<Model_ReceivingLoad>();

            var result = await Helper_Database_StoredProcedure.ExecuteDataTableAsync(
                _connectionString,
                "sp_Receiving_LabelData_GetAll",
                new Dictionary<string, object>()
            );

            if (result.IsSuccess && result.Data != null)
            {
                foreach (DataRow row in result.Data.Rows)
                {
                    loads.Add(MapRowToLoad(row));
                }
                return Model_Dao_Result_Factory.Success(loads);
            }
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(
                result.ErrorMessage ?? "Failed to retrieve current label data"
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(
                $"Error retrieving current label data: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<int>> UpdateCurrentLabelDataAsync(
        List<Model_ReceivingLoad> loads
    )
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
            int updatedCount = 0;
            foreach (var load in loads)
            {
                if (load.LoadID == Guid.Empty && !load.LabelDataRecordID.HasValue)
                {
                    throw new InvalidOperationException(
                        "Cannot update label data without a persisted identifier."
                    );
                }

                var parameters = new Dictionary<string, object>
                {
                    { "p_label_data_record_id", load.LabelDataRecordID ?? (object)DBNull.Value },
                    { "p_load_id", load.LoadID.ToString() },
                    { "p_load_number", load.LoadNumber },
                    { "p_quantity", (int)load.WeightQuantity },
                    { "p_weight_quantity", load.WeightQuantity },
                    { "p_part_id", load.PartID },
                    { "p_part_description", load.PartDescription },
                    { "p_part_type", load.PartType },
                    { "p_po_number", (object?)load.PoNumber ?? DBNull.Value },
                    { "p_po_line_number", load.PoLineNumber },
                    { "p_po_vendor", load.PoVendor },
                    { "p_po_status", load.PoStatus },
                    { "p_po_due_date", (object?)load.PoDueDate ?? DBNull.Value },
                    { "p_qty_ordered", load.QtyOrdered },
                    { "p_unit_of_measure", load.UnitOfMeasure },
                    { "p_remaining_quantity", load.RemainingQuantity },
                    { "p_employee_number", load.EmployeeNumber },
                    { "p_user_id", (object?)load.UserId ?? DBNull.Value },
                    { "p_user_set_customer_name", load.UserSetCustomerName },
                    { "p_user_set_variable", load.UserSetVariable },
                    { "p_heat", load.HeatLotNumber },
                    { "p_received_date", load.ReceivedDate },
                    { "p_transaction_date", load.ReceivedDate.Date },
                    { "p_initial_location", NormalizeLocation(load.InitialLocation) },
                    { "p_packages_per_load", load.PackagesPerLoad },
                    { "p_package_type_name", load.PackageTypeName },
                    { "p_weight_per_package", load.WeightPerPackage },
                    { "p_coils_on_skid", 0 },
                    { "p_label_number", load.LoadNumber },
                    { "p_vendor_name", load.PoVendor },
                    { "p_is_non_po_item", load.IsNonPOItem ? 1 : 0 },
                    { "p_is_quality_hold_required", load.IsQualityHoldRequired ? 1 : 0 },
                    { "p_is_quality_hold_acknowledged", load.IsQualityHoldAcknowledged ? 1 : 0 },
                    { "p_quality_hold_restriction_type", load.QualityHoldRestrictionType },
                };

                var execResult = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_Receiving_LabelData_Update",
                    parameters
                );

                if (!execResult.Success)
                {
                    throw new InvalidOperationException(
                        execResult.ErrorMessage,
                        execResult.Exception
                    );
                }

                if (execResult.AffectedRows <= 0)
                {
                    throw new InvalidOperationException(
                        $"No receiving_label_data row matched the update request for load '{load.LoadNumber}'."
                    );
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
                $"Failed to update label data: {ex.Message}",
                ex
            );
        }
    }

    public async Task<Model_Dao_Result<int>> DeleteCurrentLabelDataAsync(
        List<Model_ReceivingLoad> loads
    )
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
                if (load.LoadID == Guid.Empty && !load.LabelDataRecordID.HasValue)
                {
                    throw new InvalidOperationException(
                        "Cannot delete label data without a persisted identifier."
                    );
                }

                var parameters = new Dictionary<string, object>
                {
                    { "p_label_data_record_id", load.LabelDataRecordID ?? (object)DBNull.Value },
                    {
                        "p_load_id",
                        load.LoadID == Guid.Empty ? DBNull.Value : load.LoadID.ToString()
                    },
                };

                var execResult = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    transaction,
                    "sp_Receiving_LabelData_Delete",
                    parameters
                );

                if (!execResult.Success)
                {
                    throw new InvalidOperationException(
                        execResult.ErrorMessage,
                        execResult.Exception
                    );
                }

                if (execResult.AffectedRows <= 0)
                {
                    throw new InvalidOperationException(
                        $"No receiving_label_data row matched the delete request for load '{load.LoadNumber}'."
                    );
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
                $"Failed to delete label data: {ex.Message}",
                ex
            );
        }
    }

    private static Model_ReceivingLoad MapRowToLoad(DataRow row)
    {
        return new Model_ReceivingLoad
        {
            LabelDataRecordID = ReadNullableInt(row, "id"),
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
            UserSetCustomerName = ReadString(row, "user_set_customer_name"),
            UserSetVariable = ReadString(row, "user_set_variable"),
            EmployeeNumber = ReadInt(row, "employee_number"),
            IsQualityHoldRequired = ReadBool(row, "is_quality_hold_required"),
            IsQualityHoldAcknowledged = ReadBool(row, "is_quality_hold_acknowledged"),
            IsReprint = ReadBool(row, "is_reprint"),
            QualityHoldRestrictionType = ReadString(row, "quality_hold_restriction_type"),
        };
    }

    public async Task<Model_Dao_Result<int>> InsertFromHistoryAsync(
        int historyId,
        string queuedBy,
        int employeeNumber
    )
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(
                "sp_Receiving_LabelData_InsertFromHistory",
                connection
            )
            {
                CommandType = CommandType.StoredProcedure,
            };

            command.Parameters.AddWithValue("p_history_id", historyId);
            command.Parameters.AddWithValue("p_queued_by", queuedBy ?? "SYSTEM");
            command.Parameters.AddWithValue("p_employee_number", employeeNumber);

            await using var reader = await command.ExecuteReaderAsync();
            int inserted = 0;
            if (await reader.ReadAsync())
            {
                inserted = Convert.ToInt32(reader["rows_inserted"]);
            }

            return Model_Dao_Result_Factory.Success<int>(inserted);
        }
        catch (MySqlException ex) when (ex.Number == 1644)
        {
            // SQLSTATE 45000 — already queued for reprint
            return Model_Dao_Result_Factory.Failure<int>(ex.Message, ex);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Error queuing history record {historyId} for reprint: {ex.Message}",
                ex
            );
        }
    }

    private static bool HasColumn(DataRow row, string columnName) =>
        row.Table.Columns.Contains(columnName);

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

    private static Guid ReadGuid(DataRow row, params string[] columnNames)
    {
        var value = ReadValue(row, columnNames);
        if (value is null)
        {
            return Guid.NewGuid();
        }

        return Guid.TryParse(value.ToString(), out var guid) ? guid : Guid.NewGuid();
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
}

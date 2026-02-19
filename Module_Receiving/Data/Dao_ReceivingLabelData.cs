using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Data;

public class Dao_ReceivingLabelData
{
    private readonly string _connectionString;

    public Dao_ReceivingLabelData(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
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
            && (errorMessage.Contains("1062", StringComparison.OrdinalIgnoreCase)
                || errorMessage.Contains("PRIMARY", StringComparison.OrdinalIgnoreCase));
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

            foreach (var load in loads)
            {
                var roundedQuantity = Convert.ToInt32(Math.Round(load.WeightQuantity, 0, MidpointRounding.AwayFromZero));
                var cleanedPoNumber = CleanPONumber(load.PoNumber);
                object poNumber = cleanedPoNumber is null ? DBNull.Value : cleanedPoNumber;
                object poVendor = string.IsNullOrWhiteSpace(load.PoVendor) ? DBNull.Value : load.PoVendor;
                object poStatus = string.IsNullOrWhiteSpace(load.PoStatus) ? DBNull.Value : load.PoStatus;
                object poDueDate = load.PoDueDate.HasValue ? load.PoDueDate.Value.Date : DBNull.Value;
                object userId = string.IsNullOrWhiteSpace(load.UserId) ? DBNull.Value : load.UserId;
                object vendorName = string.IsNullOrWhiteSpace(load.PoVendor) ? DBNull.Value : load.PoVendor;
                object qualityHoldRestrictionType = string.IsNullOrWhiteSpace(load.QualityHoldRestrictionType)
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
                    { "initial_location", (object)DBNull.Value },
                    { "packages_per_load", load.PackagesPerLoad },
                    { "package_type_name", load.PackageTypeName ?? string.Empty },
                    { "weight_per_package", load.WeightPerPackage },
                    { "coils_on_skid", (object)DBNull.Value },
                    { "label_number", load.LoadNumber },
                    { "vendor_name", vendorName },
                    { "is_non_po_item", load.IsNonPOItem },
                    { "is_quality_hold_required", load.IsQualityHoldRequired },
                    { "is_quality_hold_acknowledged", load.IsQualityHoldAcknowledged },
                    { "quality_hold_restriction_type", qualityHoldRestrictionType }
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
            return Model_Dao_Result_Factory.Failure<int>($"Failed to save label data loads: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> ClearLabelDataToHistoryAsync(string archivedBy)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_Receiving_LabelData_ClearToHistory", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("p_archived_by", archivedBy ?? "SYSTEM");

            var rowsMovedParam = new MySqlParameter("p_rows_moved", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(rowsMovedParam);

            var batchIdParam = new MySqlParameter("p_archive_batch_id", MySqlDbType.VarChar, 36)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(batchIdParam);

            var statusParam = new MySqlParameter("p_status", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(statusParam);

            var errorParam = new MySqlParameter("p_error_message", MySqlDbType.VarChar, 1000)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(errorParam);

            await command.ExecuteNonQueryAsync();

            var status = statusParam.Value == DBNull.Value ? 1 : Convert.ToInt32(statusParam.Value);
            var errorMessage = errorParam.Value == DBNull.Value ? null : errorParam.Value?.ToString();

            if (status != 0)
            {
                return Model_Dao_Result_Factory.Failure<int>(errorMessage ?? "Clear Label Data failed");
            }

            var rowsMoved = rowsMovedParam.Value == DBNull.Value ? 0 : Convert.ToInt32(rowsMovedParam.Value);
            return Model_Dao_Result_Factory.Success<int>(rowsMoved);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>($"Failed to clear label data to history: {ex.Message}", ex);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

            // Pre-compute per-part skid totals and sequences for the "N of M" label counter.
            // Group by PartID to get the total skids per part, then track per-part position.
            var partTotals = loads
                .GroupBy(l => l.PartID, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
            var partSequenceCounters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var load in loads)
            {
                if (!partSequenceCounters.ContainsKey(load.PartID))
                {
                    partSequenceCounters[load.PartID] = 0;
                }
                partSequenceCounters[load.PartID]++;
                int skidSequence = partSequenceCounters[load.PartID];
                int skidTotal = partTotals[load.PartID];

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
                    { "quality_hold_restriction_type", qualityHoldRestrictionType },
                    { "part_skid_sequence", skidSequence },
                    { "part_skid_total", skidTotal }
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
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>(result.ErrorMessage ?? "Failed to retrieve current label data");
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingLoad>>($"Error retrieving current label data: {ex.Message}", ex);
        }
    }

    public async Task<Model_Dao_Result<int>> UpdateCurrentLabelDataAsync(List<Model_ReceivingLoad> loads)
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
                var parameters = new Dictionary<string, object>
                {
                    { "p_load_id",                          load.LoadID.ToString()                              },
                    { "p_load_number",                      load.LoadNumber                                     },
                    { "p_quantity",                         (int)load.WeightQuantity                            },
                    { "p_weight_quantity",                  load.WeightQuantity                                 },
                    { "p_part_id",                          load.PartID                                         },
                    { "p_part_description",                 load.PartDescription                                },
                    { "p_part_type",                        load.PartType                                       },
                    { "p_po_number",                        (object?)load.PoNumber ?? DBNull.Value              },
                    { "p_po_line_number",                   load.PoLineNumber                                   },
                    { "p_po_vendor",                        load.PoVendor                                       },
                    { "p_po_status",                        load.PoStatus                                       },
                    { "p_po_due_date",                      (object?)load.PoDueDate ?? DBNull.Value             },
                    { "p_qty_ordered",                      load.QtyOrdered                                     },
                    { "p_unit_of_measure",                  load.UnitOfMeasure                                  },
                    { "p_remaining_quantity",               load.RemainingQuantity                              },
                    { "p_employee_number",                  load.EmployeeNumber                                 },
                    { "p_user_id",                          (object?)load.UserId ?? DBNull.Value                },
                    { "p_heat",                             load.HeatLotNumber                                  },
                    { "p_received_date",                    load.ReceivedDate                                   },
                    { "p_transaction_date",                 load.ReceivedDate.Date                              },
                    { "p_initial_location",                 string.Empty                                        },
                    { "p_packages_per_load",                load.PackagesPerLoad                                },
                    { "p_package_type_name",                load.PackageTypeName                                },
                    { "p_weight_per_package",               load.WeightPerPackage                               },
                    { "p_coils_on_skid",                    0                                                   },
                    { "p_label_number",                     load.LoadNumber                                     },
                    { "p_vendor_name",                      load.PoVendor                                       },
                    { "p_is_non_po_item",                   load.IsNonPOItem ? 1 : 0                            },
                    { "p_is_quality_hold_required",         load.IsQualityHoldRequired ? 1 : 0                 },
                    { "p_is_quality_hold_acknowledged",     load.IsQualityHoldAcknowledged ? 1 : 0             },
                    { "p_quality_hold_restriction_type",    load.QualityHoldRestrictionType                    },
                };

                var execResult = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection, transaction, "sp_Receiving_LabelData_Update", parameters);

                if (!execResult.Success)
                {
                    throw new InvalidOperationException(execResult.ErrorMessage, execResult.Exception);
                }

                updatedCount++;
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success<int>(updatedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Model_Dao_Result_Factory.Failure<int>($"Failed to update label data: {ex.Message}", ex);
        }
    }

    private static Model_ReceivingLoad MapRowToLoad(DataRow row)
    {
        return new Model_ReceivingLoad
        {
            LoadID = ReadGuid(row, "LoadID"),
            PartID = ReadString(row, "PartID"),
            PartDescription = ReadString(row, "PartDescription"),
            PartType = ReadString(row, "PartType"),
            PoNumber = ReadNullableString(row, "PONumber"),
            PoLineNumber = ReadString(row, "POLineNumber"),
            PoVendor = ReadString(row, "POVendor"),
            PoStatus = ReadString(row, "POStatus"),
            PoDueDate = ReadNullableDateTime(row, "PODueDate"),
            QtyOrdered = ReadDecimal(row, "QtyOrdered"),
            UnitOfMeasure = string.IsNullOrWhiteSpace(ReadString(row, "UnitOfMeasure")) ? "EA" : ReadString(row, "UnitOfMeasure"),
            RemainingQuantity = ReadInt(row, "RemainingQuantity"),
            LoadNumber = ReadInt(row, "LoadNumber"),
            WeightQuantity = ReadDecimal(row, "WeightQuantity"),
            HeatLotNumber = ReadString(row, "HeatLotNumber"),
            PackagesPerLoad = ReadInt(row, "PackagesPerLoad"),
            PackageTypeName = ReadString(row, "PackageTypeName"),
            WeightPerPackage = ReadDecimal(row, "WeightPerPackage"),
            IsNonPOItem = ReadBool(row, "IsNonPOItem"),
            ReceivedDate = ReadDateTime(row, "ReceivedDate"),
            UserId = ReadNullableString(row, "UserID"),
            EmployeeNumber = ReadInt(row, "EmployeeNumber"),
            IsQualityHoldRequired = ReadBool(row, "IsQualityHoldRequired"),
            IsQualityHoldAcknowledged = ReadBool(row, "IsQualityHoldAcknowledged"),
            QualityHoldRestrictionType = ReadString(row, "QualityHoldRestrictionType")
        };
    }

    private static bool HasColumn(DataRow row, string columnName) =>
        row.Table.Columns.Contains(columnName);

    private static Guid ReadGuid(DataRow row, string columnName)
    {
        if (!HasColumn(row, columnName) || row[columnName] == DBNull.Value)
        {
            return Guid.NewGuid();
        }
        return Guid.TryParse(row[columnName]?.ToString(), out var guid) ? guid : Guid.NewGuid();
    }

    private static string ReadString(DataRow row, string columnName)
    {
        if (!HasColumn(row, columnName) || row[columnName] == DBNull.Value)
        {
            return string.Empty;
        }
        return row[columnName]?.ToString() ?? string.Empty;
    }

    private static string? ReadNullableString(DataRow row, string columnName)
    {
        if (!HasColumn(row, columnName) || row[columnName] == DBNull.Value)
        {
            return null;
        }
        return row[columnName]?.ToString();
    }

    private static int ReadInt(DataRow row, string columnName)
    {
        if (!HasColumn(row, columnName) || row[columnName] == DBNull.Value)
        {
            return 0;
        }
        return Convert.ToInt32(row[columnName]);
    }

    private static decimal ReadDecimal(DataRow row, string columnName)
    {
        if (!HasColumn(row, columnName) || row[columnName] == DBNull.Value)
        {
            return 0;
        }
        return Convert.ToDecimal(row[columnName]);
    }

    private static bool ReadBool(DataRow row, string columnName)
    {
        if (!HasColumn(row, columnName) || row[columnName] == DBNull.Value)
        {
            return false;
        }
        return Convert.ToBoolean(row[columnName]);
    }

    private static DateTime ReadDateTime(DataRow row, string columnName)
    {
        if (!HasColumn(row, columnName) || row[columnName] == DBNull.Value)
        {
            return DateTime.Now;
        }
        return Convert.ToDateTime(row[columnName]);
    }

    private static DateTime? ReadNullableDateTime(DataRow row, string columnName)
    {
        if (!HasColumn(row, columnName) || row[columnName] == DBNull.Value)
        {
            return null;
        }
        return Convert.ToDateTime(row[columnName]);
    }
}

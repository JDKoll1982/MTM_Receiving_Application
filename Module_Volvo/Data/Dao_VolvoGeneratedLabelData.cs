using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data access for the dedicated Volvo generated-label queue and archive tables.
/// </summary>
public class Dao_VolvoGeneratedLabelData : IDao_VolvoGeneratedLabelData
{
    private readonly string _connectionString;

    public Dao_VolvoGeneratedLabelData(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Model_Dao_Result<int>> ReplaceForShipmentAsync(
        int shipmentId,
        List<Model_VolvoGeneratedLabelData> rows
    )
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var deleteResult = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                connection,
                (MySqlTransaction)transaction,
                "sp_Volvo_GeneratedLabelData_DeleteByShipment",
                new Dictionary<string, object> { { "shipment_id", shipmentId } }
            );

            if (!deleteResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Model_Dao_Result_Factory.Failure<int>(
                    deleteResult.ErrorMessage ?? "Failed to clear existing generated label rows",
                    deleteResult.Exception
                );
            }

            foreach (var row in rows)
            {
                var insertResult = await Helper_Database_StoredProcedure.ExecuteInTransactionAsync(
                    connection,
                    (MySqlTransaction)transaction,
                    "sp_Volvo_GeneratedLabelData_Insert",
                    new Dictionary<string, object>
                    {
                        { "shipment_id", row.ShipmentId },
                        { "shipment_number", row.ShipmentNumber },
                        { "shipment_date", row.ShipmentDate.Date },
                        { "part_number", row.PartNumber },
                        { "quantity", row.Quantity },
                        { "skid_number", row.SkidNumber },
                        { "total_skids", row.TotalSkids },
                        { "part_description", row.PartDescription },
                        { "employee_number", row.EmployeeNumber ?? (object)DBNull.Value },
                    }
                );

                if (!insertResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Model_Dao_Result_Factory.Failure<int>(
                        insertResult.ErrorMessage
                            ?? $"Failed to insert generated label row for part {row.PartNumber}",
                        insertResult.Exception
                    );
                }
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success<int>(rows.Count);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Failed to replace generated Volvo label data: {ex.Message}",
                ex
            );
        }
    }

    public async Task<
        Model_Dao_Result<List<Model_VolvoGeneratedLabelData>>
    > GetActiveLabelDataAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Volvo_GeneratedLabelData_GetAll",
            MapFromReader
        );
    }

    public async Task<Model_Dao_Result<int>> DeleteByShipmentAsync(int shipmentId)
    {
        var result = await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Volvo_GeneratedLabelData_DeleteByShipment",
            new Dictionary<string, object> { { "shipment_id", shipmentId } }
        );

        return result.IsSuccess
            ? Model_Dao_Result_Factory.Success<int>(result.AffectedRows)
            : Model_Dao_Result_Factory.Failure<int>(
                result.ErrorMessage ?? "Failed to delete generated label rows",
                result.Exception
            );
    }

    public async Task<Model_Dao_Result<int>> ClearToHistoryAsync(
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
                "sp_Volvo_GeneratedLabelData_ClearToHistory",
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
                    errorMessage ?? "Clear generated label data failed"
                );
            }

            var rowsMoved =
                rowsMovedParam.Value == DBNull.Value ? 0 : Convert.ToInt32(rowsMovedParam.Value);

            return Model_Dao_Result_Factory.Success<int>(rowsMoved);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Failed to clear generated Volvo label data to history: {ex.Message}",
                ex
            );
        }
    }

    private static Model_VolvoGeneratedLabelData MapFromReader(IDataReader reader)
    {
        return new Model_VolvoGeneratedLabelData
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            ShipmentId = reader.GetInt32(reader.GetOrdinal("shipment_id")),
            ShipmentNumber = reader.GetInt32(reader.GetOrdinal("shipment_number")),
            ShipmentDate = reader.GetDateTime(reader.GetOrdinal("shipment_date")),
            PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
            Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
            SkidNumber = reader.GetInt32(reader.GetOrdinal("skid_number")),
            TotalSkids = reader.GetInt32(reader.GetOrdinal("total_skids")),
            PartDescription = reader.GetString(reader.GetOrdinal("part_description")),
            EmployeeNumber = reader.IsDBNull(reader.GetOrdinal("employee_number"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("employee_number")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
        };
    }
}

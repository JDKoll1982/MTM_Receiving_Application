using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Dunnage.Data;

/// <summary>
/// Data access for the <c>dunnage_label_data</c> active queue.
/// Workflow completion writes here; <c>Clear Label Data</c> atomically moves rows to
/// <c>dunnage_history</c> via <see cref="ClearToHistoryAsync"/>.
/// </summary>
public class Dao_DunnageLabelData
{
    private readonly string _connectionString;

    public Dao_DunnageLabelData(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Inserts each load in <paramref name="loads"/> into <c>dunnage_label_data</c> within a
    /// single database transaction. Returns the count of rows successfully inserted.
    /// Uses explicit <see cref="MySqlParameter"/> objects with declared types to avoid
    /// "MySQL Error 0: Unhandled type encountered" when nullable values are DBNull.
    /// </summary>
    /// <param name="loads">The list of dunnage loads to insert.</param>
    /// <param name="user">The user ID to record against each inserted row.</param>
    public async Task<Model_Dao_Result<int>> InsertBatchAsync(
        List<Model_DunnageLoad> loads,
        string user
    )
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
            var orderedLoads = OrderLoadsAndAssignPartSkidCounters(loads);

            foreach (var load in orderedLoads)
            {
                await using var command = new MySqlCommand(
                    "sp_Dunnage_LabelData_Insert",
                    connection,
                    transaction
                )
                {
                    CommandType = CommandType.StoredProcedure,
                };

                command.Parameters.Add(
                    new MySqlParameter("p_load_uuid", MySqlDbType.VarChar, 36)
                    {
                        Value = load.LoadUuid.ToString(),
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_part_id", MySqlDbType.VarChar, 50) { Value = load.PartId }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_dunnage_type_id", MySqlDbType.Int32)
                    {
                        Value = load.TypeId.HasValue ? (object)load.TypeId.Value : DBNull.Value,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_dunnage_type_name", MySqlDbType.VarChar, 100)
                    {
                        Value = string.IsNullOrWhiteSpace(load.TypeName)
                            ? DBNull.Value
                            : (object)load.TypeName,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_dunnage_type_icon", MySqlDbType.VarChar, 100)
                    {
                        Value = string.IsNullOrWhiteSpace(load.TypeIcon)
                            ? DBNull.Value
                            : (object)load.TypeIcon,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_quantity", MySqlDbType.Decimal)
                    {
                        Value = load.Quantity,
                        Precision = 10,
                        Scale = 2,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_quantity_type", MySqlDbType.VarChar, 100)
                    {
                        Value = string.IsNullOrWhiteSpace(load.QuantityType)
                            ? "Quantity"
                            : (object)load.QuantityType,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_po_number", MySqlDbType.VarChar, 50)
                    {
                        Value = string.IsNullOrWhiteSpace(load.PoNumber)
                            ? DBNull.Value
                            : (object)load.PoNumber,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_received_date", MySqlDbType.DateTime)
                    {
                        Value = load.ReceivedDate,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_user_id", MySqlDbType.VarChar, 100) { Value = user }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_employee_number", MySqlDbType.Int32)
                    {
                        Value = load.EmployeeNumber.HasValue
                            ? (object)load.EmployeeNumber.Value
                            : DBNull.Value,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_location", MySqlDbType.VarChar, 100)
                    {
                        Value = string.IsNullOrWhiteSpace(load.Location)
                            ? DBNull.Value
                            : (object)load.Location,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_label_number", MySqlDbType.VarChar, 50)
                    {
                        Value = string.IsNullOrWhiteSpace(load.LabelNumber)
                            ? DBNull.Value
                            : (object)load.LabelNumber,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_part_skid_sequence", MySqlDbType.Int32)
                    {
                        Value = load.PartSkidSequence ?? (object)DBNull.Value,
                    }
                );

                command.Parameters.Add(
                    new MySqlParameter("p_part_skid_total", MySqlDbType.Int32)
                    {
                        Value = load.PartSkidTotal ?? (object)DBNull.Value,
                    }
                );

                var specsJson = BuildSpecsJson(load);
                command.Parameters.Add(
                    new MySqlParameter("p_specs_json", MySqlDbType.JSON)
                    {
                        Value = specsJson is null ? DBNull.Value : (object)specsJson,
                    }
                );

                await command.ExecuteNonQueryAsync();
                savedCount++;
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success<int>(savedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Model_Dao_Result_Factory.Failure<int>(
                $"Failed to save dunnage label data: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Atomically moves all rows from <c>dunnage_label_data</c> to <c>dunnage_history</c>
    /// and deletes them from the queue. Returns the number of rows moved.
    /// </summary>
    /// <param name="archivedBy">User or system identifier recorded in the archive record.</param>
    /// <param name="employeeNumber">Employee number used for user-scoped clears.</param>
    /// <param name="clearAllRows">When true, archives every queue row regardless of owner.</param>
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
                "sp_Dunnage_LabelData_ClearToHistory",
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
                $"Failed to clear dunnage label data to history: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Returns all rows currently in the <c>dunnage_label_data</c> active queue,
    /// ordered by received_date ascending.
    /// </summary>
    public virtual async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetActiveLabelDataAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnageLoad>(
            _connectionString,
            "sp_Dunnage_LabelData_GetAll",
            MapFromReader
        );
    }

    /// <summary>
    /// Loads dunnage history rows for the Reprint Labels page, including whether each row is
    /// already queued for reprint (an <c>is_reprint = 1</c> row exists in dunnage_label_data
    /// for the same load_uuid).
    /// </summary>
    public virtual async Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> GetReprintHistoryAsync(
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
            "sp_Dunnage_LabelHistory_GetForReprint",
            MapReprintHistoryRow,
            parameters
        );
    }

    /// <summary>
    /// Copies a single row from <c>dunnage_history</c> back into <c>dunnage_label_data</c> so it
    /// can be re-printed. Sets <c>is_reprint = 1</c>. Returns 0 rows inserted when the history
    /// row is already queued for reprint (the stored procedure raises SQLSTATE 45000).
    /// </summary>
    public virtual async Task<Model_Dao_Result<int>> InsertFromHistoryAsync(
        string loadUuid,
        string queuedBy,
        int employeeNumber
    )
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand(
                "sp_Dunnage_LabelData_InsertFromHistory",
                connection
            )
            {
                CommandType = CommandType.StoredProcedure,
            };

            command.Parameters.AddWithValue("p_load_uuid", loadUuid);
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
                $"Error queuing history record {loadUuid} for reprint: {ex.Message}",
                ex
            );
        }
    }

    private static Model_ReprintHistoryRow MapReprintHistoryRow(IDataReader reader)
    {
        var poNumber = reader.IsDBNull(reader.GetOrdinal("po_number"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("po_number")).Trim();
        var labelNumber = reader.IsDBNull(reader.GetOrdinal("label_number"))
            ? string.Empty
            : reader.GetString(reader.GetOrdinal("label_number")).Trim();

        return new Model_ReprintHistoryRow
        {
            HistoryId = reader.GetValue(reader.GetOrdinal("load_uuid")).ToString() ?? string.Empty,
            RecordDate = reader.GetDateTime(reader.GetOrdinal("record_date")),
            Part = reader.GetString(reader.GetOrdinal("part_id")),
            Quantity = reader.IsDBNull(reader.GetOrdinal("quantity"))
                ? 0m
                : Convert.ToDecimal(reader.GetValue(reader.GetOrdinal("quantity"))),
            Reference = BuildDunnageReference(poNumber, labelNumber),
            AlreadyQueued =
                !reader.IsDBNull(reader.GetOrdinal("already_queued"))
                && reader.GetInt32(reader.GetOrdinal("already_queued")) == 1,
        };
    }

    private static string BuildDunnageReference(string poNumber, string labelNumber)
    {
        if (!string.IsNullOrWhiteSpace(poNumber))
        {
            return poNumber;
        }

        return string.IsNullOrWhiteSpace(labelNumber) ? string.Empty : $"Label {labelNumber}";
    }

    /// <summary>
    /// Deletes one row from the active label queue identified by load UUID.
    /// </summary>
    /// <param name="loadUuid"></param>
    public virtual async Task<Model_Dao_Result> DeleteAsync(Guid loadUuid)
    {
        var parameters = new Dictionary<string, object> { { "load_uuid", loadUuid.ToString() } };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_LabelData_Delete",
            parameters
        );
    }

    /// <summary>
    /// Updates one row in the active <c>dunnage_label_data</c> queue identified by load UUID.
    /// </summary>
    /// <param name="load"></param>
    /// <param name="fallbackUser"></param>
    public virtual async Task<Model_Dao_Result> UpdateAsync(
        Model_DunnageLoad load,
        string fallbackUser
    )
    {
        var specsJson = BuildSpecsJson(load);
        var parameters = new MySqlParameter[]
        {
            new("@p_load_uuid", MySqlDbType.VarChar, 36) { Value = load.LoadUuid.ToString() },
            new("@p_part_id", MySqlDbType.VarChar, 50) { Value = load.PartId },
            new("@p_dunnage_type_id", MySqlDbType.Int32)
            {
                Value = load.TypeId.HasValue ? (object)load.TypeId.Value : DBNull.Value,
            },
            new("@p_dunnage_type_name", MySqlDbType.VarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(load.TypeName)
                    ? DBNull.Value
                    : (object)load.TypeName,
            },
            new("@p_dunnage_type_icon", MySqlDbType.VarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(load.TypeIcon)
                    ? DBNull.Value
                    : (object)load.TypeIcon,
            },
            new("@p_quantity", MySqlDbType.Decimal)
            {
                Value = load.Quantity,
                Precision = 10,
                Scale = 2,
            },
            new("@p_quantity_type", MySqlDbType.VarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(load.QuantityType)
                    ? "Quantity"
                    : (object)load.QuantityType,
            },
            new("@p_po_number", MySqlDbType.VarChar, 50)
            {
                Value = string.IsNullOrWhiteSpace(load.PoNumber)
                    ? DBNull.Value
                    : (object)load.PoNumber,
            },
            new("@p_received_date", MySqlDbType.DateTime) { Value = load.ReceivedDate },
            new("@p_user_id", MySqlDbType.VarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(load.CreatedBy) ? fallbackUser : load.CreatedBy,
            },
            new("@p_employee_number", MySqlDbType.Int32)
            {
                Value = load.EmployeeNumber.HasValue
                    ? (object)load.EmployeeNumber.Value
                    : DBNull.Value,
            },
            new("@p_location", MySqlDbType.VarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(load.Location)
                    ? DBNull.Value
                    : (object)load.Location,
            },
            new("@p_label_number", MySqlDbType.VarChar, 50)
            {
                Value = string.IsNullOrWhiteSpace(load.LabelNumber)
                    ? DBNull.Value
                    : (object)load.LabelNumber,
            },
            new("@p_part_skid_sequence", MySqlDbType.Int32)
            {
                Value = load.PartSkidSequence.HasValue
                    ? (object)load.PartSkidSequence.Value
                    : DBNull.Value,
            },
            new("@p_part_skid_total", MySqlDbType.Int32)
            {
                Value = load.PartSkidTotal.HasValue
                    ? (object)load.PartSkidTotal.Value
                    : DBNull.Value,
            },
            new("@p_specs_json", MySqlDbType.JSON)
            {
                Value = specsJson is null ? DBNull.Value : (object)specsJson,
            },
        };

        return await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_Dunnage_LabelData_Update",
            parameters,
            _connectionString
        );
    }

    /// <summary>
    /// Orders loads consistently for label generation and assigns per-part skid counters.
    /// </summary>
    /// <param name="loads"></param>
    private static List<Model_DunnageLoad> OrderLoadsAndAssignPartSkidCounters(
        IEnumerable<Model_DunnageLoad> loads
    )
    {
        var orderedLoads = loads
            .OrderBy(load => load.PartId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(load => load.LoadNumber)
            .ThenBy(load => load.LoadUuid)
            .ToList();

        var partTotals = orderedLoads
            .GroupBy(load => load.PartId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Count(),
                StringComparer.OrdinalIgnoreCase
            );
        var partSequenceCounters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var load in orderedLoads)
        {
            if (!partSequenceCounters.ContainsKey(load.PartId))
            {
                partSequenceCounters[load.PartId] = 0;
            }

            partSequenceCounters[load.PartId]++;
            load.PartSkidSequence = partSequenceCounters[load.PartId];
            load.PartSkidTotal = partTotals[load.PartId];
        }

        return orderedLoads;
    }

    /// <summary>
    /// Serializes the dynamic spec values from a load into a JSON string for <c>specs_json</c>.
    /// Prefers <see cref="Model_DunnageLoad.SpecValues"/> then falls back to <see cref="Model_DunnageLoad.Specs"/>.
    /// Returns <c>null</c> if both are empty.
    /// </summary>
    /// <param name="load">The dunnage load whose spec values to serialize.</param>
    private static string? BuildSpecsJson(Model_DunnageLoad load)
    {
        var specs = load.SpecValues ?? load.Specs;
        if (specs == null || specs.Count == 0)
        {
            return null;
        }

        return JsonSerializer.Serialize(specs);
    }

    private static Model_DunnageLoad MapFromReader(IDataReader reader)
    {
        var hasQuantityTypeColumn = HasColumn(reader, "quantity_type");
        var hasEmployeeNumberColumn = HasColumn(reader, "employee_number");

        return new Model_DunnageLoad
        {
            QueueRowId = reader.IsDBNull(reader.GetOrdinal("id"))
                ? 0
                : reader.GetInt32(reader.GetOrdinal("id")),
            // GetValue().ToString() handles both string and Guid returns from the connector.
            LoadUuid = Guid.Parse(reader.GetValue(reader.GetOrdinal("load_uuid")).ToString()!),
            PartId = reader.GetString(reader.GetOrdinal("part_id")),
            TypeId = reader.IsDBNull(reader.GetOrdinal("dunnage_type_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("dunnage_type_id")),
            TypeName = reader.IsDBNull(reader.GetOrdinal("dunnage_type_name"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("dunnage_type_name")),
            DunnageType = reader.IsDBNull(reader.GetOrdinal("dunnage_type_name"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("dunnage_type_name")),
            TypeIcon = reader.IsDBNull(reader.GetOrdinal("dunnage_type_icon"))
                ? "Help"
                : reader.GetString(reader.GetOrdinal("dunnage_type_icon")),
            Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
            QuantityType =
                !hasQuantityTypeColumn ? "Quantity"
                : reader.IsDBNull(reader.GetOrdinal("quantity_type")) ? "Quantity"
                : reader.GetString(reader.GetOrdinal("quantity_type")),
            PoNumber = reader.IsDBNull(reader.GetOrdinal("po_number"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("po_number")),
            ReceivedDate = reader.GetDateTime(reader.GetOrdinal("received_date")),
            CreatedBy = reader.GetString(reader.GetOrdinal("user_id")),
            EmployeeNumber =
                !hasEmployeeNumberColumn ? null
                : reader.IsDBNull(reader.GetOrdinal("employee_number")) ? null
                : reader.GetInt32(reader.GetOrdinal("employee_number")),
            CreatedDate = reader.IsDBNull(reader.GetOrdinal("created_at"))
                ? default
                : reader.GetDateTime(reader.GetOrdinal("created_at")),
            Location = reader.IsDBNull(reader.GetOrdinal("location"))
                ? null
                : reader.GetString(reader.GetOrdinal("location")),
            LabelNumber = reader.IsDBNull(reader.GetOrdinal("label_number"))
                ? null
                : reader.GetString(reader.GetOrdinal("label_number")),
            PartSkidSequence = reader.IsDBNull(reader.GetOrdinal("part_skid_sequence"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("part_skid_sequence")),
            PartSkidTotal = reader.IsDBNull(reader.GetOrdinal("part_skid_total"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("part_skid_total")),
            SpecValues = DeserializeSpecValues(reader),
        };
    }

    private static bool HasColumn(IDataReader reader, string columnName)
    {
        for (var index = 0; index < reader.FieldCount; index++)
        {
            if (
                string.Equals(reader.GetName(index), columnName, StringComparison.OrdinalIgnoreCase)
            )
            {
                return true;
            }
        }

        return false;
    }

    private static Dictionary<string, object>? DeserializeSpecValues(IDataReader reader)
    {
        var ordinal = reader.GetOrdinal("specs_json");
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        var json = reader.GetString(ordinal);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[Dao_DunnageLabelData] Failed to deserialize specs_json: {ex.Message}"
            );
            return null;
        }
    }
}

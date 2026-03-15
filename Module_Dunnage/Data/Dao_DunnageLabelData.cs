using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;

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
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Inserts each load in <paramref name="loads"/> into <c>dunnage_label_data</c> within a
    /// single database transaction. Returns the count of rows successfully inserted.
    /// Uses explicit <see cref="MySqlParameter"/> objects with declared types to avoid
    /// "MySQL Error 0: Unhandled type encountered" when nullable values are DBNull.
    /// </summary>
    public async Task<Model_Dao_Result<int>> InsertBatchAsync(List<Model_DunnageLoad> loads, string user)
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
                await using var command = new MySqlCommand("sp_Dunnage_LabelData_Insert", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add(new MySqlParameter("p_load_uuid", MySqlDbType.VarChar, 36)
                { Value = load.LoadUuid.ToString() });

                command.Parameters.Add(new MySqlParameter("p_part_id", MySqlDbType.VarChar, 50)
                { Value = load.PartId });

                command.Parameters.Add(new MySqlParameter("p_dunnage_type_id", MySqlDbType.Int32)
                { Value = load.TypeId.HasValue ? (object)load.TypeId.Value : DBNull.Value });

                command.Parameters.Add(new MySqlParameter("p_dunnage_type_name", MySqlDbType.VarChar, 100)
                { Value = string.IsNullOrWhiteSpace(load.TypeName) ? DBNull.Value : (object)load.TypeName });

                command.Parameters.Add(new MySqlParameter("p_dunnage_type_icon", MySqlDbType.VarChar, 100)
                { Value = string.IsNullOrWhiteSpace(load.TypeIcon) ? DBNull.Value : (object)load.TypeIcon });

                command.Parameters.Add(new MySqlParameter("p_quantity", MySqlDbType.Decimal)
                { Value = load.Quantity, Precision = 10, Scale = 2 });

                command.Parameters.Add(new MySqlParameter("p_po_number", MySqlDbType.VarChar, 50)
                { Value = string.IsNullOrWhiteSpace(load.PoNumber) ? DBNull.Value : (object)load.PoNumber });

                command.Parameters.Add(new MySqlParameter("p_received_date", MySqlDbType.DateTime)
                { Value = load.ReceivedDate });

                command.Parameters.Add(new MySqlParameter("p_user_id", MySqlDbType.VarChar, 100)
                { Value = user });

                command.Parameters.Add(new MySqlParameter("p_location", MySqlDbType.VarChar, 100)
                { Value = string.IsNullOrWhiteSpace(load.Location) ? DBNull.Value : (object)load.Location });

                command.Parameters.Add(new MySqlParameter("p_label_number", MySqlDbType.VarChar, 50)
                { Value = string.IsNullOrWhiteSpace(load.LabelNumber) ? DBNull.Value : (object)load.LabelNumber });

                var specsJson = BuildSpecsJson(load);
                command.Parameters.Add(new MySqlParameter("p_specs_json", MySqlDbType.JSON)
                { Value = specsJson is null ? DBNull.Value : (object)specsJson });

                await command.ExecuteNonQueryAsync();
                savedCount++;
            }

            await transaction.CommitAsync();
            return Model_Dao_Result_Factory.Success<int>(savedCount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Model_Dao_Result_Factory.Failure<int>($"Failed to save dunnage label data: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Atomically moves all rows from <c>dunnage_label_data</c> to <c>dunnage_history</c>
    /// and deletes them from the queue. Returns the number of rows moved.
    /// </summary>
    public async Task<Model_Dao_Result<int>> ClearToHistoryAsync(string archivedBy)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_Dunnage_LabelData_ClearToHistory", connection)
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
            return Model_Dao_Result_Factory.Failure<int>($"Failed to clear dunnage label data to history: {ex.Message}", ex);
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
    /// Serializes the dynamic spec values from a load into a JSON string for <c>specs_json</c>.
    /// Prefers <see cref="Model_DunnageLoad.SpecValues"/> then falls back to <see cref="Model_DunnageLoad.Specs"/>.
    /// Returns <c>null</c> if both are empty.
    /// </summary>
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
        return new Model_DunnageLoad
        {
            // GetValue().ToString() handles both string and Guid returns from the connector.
            LoadUuid = Guid.Parse(reader.GetValue(reader.GetOrdinal("load_uuid")).ToString()!),
            PartId = reader.GetString(reader.GetOrdinal("part_id")),
            TypeId = reader.IsDBNull(reader.GetOrdinal("dunnage_type_id")) ? null : reader.GetInt32(reader.GetOrdinal("dunnage_type_id")),
            TypeName = reader.IsDBNull(reader.GetOrdinal("dunnage_type_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("dunnage_type_name")),
            DunnageType = reader.IsDBNull(reader.GetOrdinal("dunnage_type_name")) ? string.Empty : reader.GetString(reader.GetOrdinal("dunnage_type_name")),
            TypeIcon = reader.IsDBNull(reader.GetOrdinal("dunnage_type_icon")) ? "Help" : reader.GetString(reader.GetOrdinal("dunnage_type_icon")),
            Quantity = reader.GetDecimal(reader.GetOrdinal("quantity")),
            PoNumber = reader.IsDBNull(reader.GetOrdinal("po_number")) ? string.Empty : reader.GetString(reader.GetOrdinal("po_number")),
            ReceivedDate = reader.GetDateTime(reader.GetOrdinal("received_date")),
            CreatedBy = reader.GetString(reader.GetOrdinal("user_id")),
            Location = reader.IsDBNull(reader.GetOrdinal("location")) ? null : reader.GetString(reader.GetOrdinal("location")),
            LabelNumber = reader.IsDBNull(reader.GetOrdinal("label_number")) ? null : reader.GetString(reader.GetOrdinal("label_number")),
            SpecValues = DeserializeSpecValues(reader)
        };
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
            System.Diagnostics.Debug.WriteLine($"[Dao_DunnageLabelData] Failed to deserialize specs_json: {ex.Message}");
            return null;
        }
    }
}

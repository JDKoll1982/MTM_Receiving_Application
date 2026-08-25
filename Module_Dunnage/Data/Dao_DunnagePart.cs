using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Dunnage.Data;

public class Dao_DunnagePart
{
    private readonly string _connectionString;

    public Dao_DunnagePart(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnagePart>(
            _connectionString,
            "sp_Dunnage_Parts_GetAll",
            MapFromReader
        );
    }

    public virtual async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetByTypeAsync(int typeId)
    {
        System.Diagnostics.Debug.WriteLine(
            $"Dao_DunnagePart: GetByTypeAsync called for typeId={typeId}"
        );
        var parameters = new Dictionary<string, object> { { "type_id", typeId } };

        var result = await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnagePart>(
            _connectionString,
            "sp_Dunnage_Parts_GetByType",
            MapFromReader,
            parameters
        );

        System.Diagnostics.Debug.WriteLine(
            $"Dao_DunnagePart: GetByTypeAsync returned {result.Data?.Count ?? 0} parts. Success: {result.IsSuccess}"
        );
        return result;
    }

    public virtual async Task<Model_Dao_Result<Model_DunnagePart>> GetByIdAsync(string partId)
    {
        var parameters = new Dictionary<string, object> { { "part_id", partId } };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<Model_DunnagePart>(
            _connectionString,
            "sp_Dunnage_Parts_GetById",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<int>> InsertAsync(
        string partId,
        int typeId,
        string? udc1,
        string? udc2,
        string? udc3,
        string? udc4,
        string? udc5,
        string? udc6,
        string? udc7,
        string? udc8,
        string? udc9,
        string? udc10,
        string? imagePath,
        string quantityType,
        string homeLocation,
        string user
    )
    {
        var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_part_id", partId),
            new MySqlParameter("@p_type_id", typeId),
            new MySqlParameter("@p_udc1", (object?)udc1 ?? DBNull.Value),
            new MySqlParameter("@p_udc2", (object?)udc2 ?? DBNull.Value),
            new MySqlParameter("@p_udc3", (object?)udc3 ?? DBNull.Value),
            new MySqlParameter("@p_udc4", (object?)udc4 ?? DBNull.Value),
            new MySqlParameter("@p_udc5", (object?)udc5 ?? DBNull.Value),
            new MySqlParameter("@p_udc6", (object?)udc6 ?? DBNull.Value),
            new MySqlParameter("@p_udc7", (object?)udc7 ?? DBNull.Value),
            new MySqlParameter("@p_udc8", (object?)udc8 ?? DBNull.Value),
            new MySqlParameter("@p_udc9", (object?)udc9 ?? DBNull.Value),
            new MySqlParameter("@p_udc10", (object?)udc10 ?? DBNull.Value),
            new MySqlParameter(
                "@p_image_path",
                string.IsNullOrWhiteSpace(imagePath) ? DBNull.Value : imagePath
            ),
            new MySqlParameter("@p_quantity_type", quantityType),
            new MySqlParameter("@p_home_location", homeLocation),
            new MySqlParameter("@p_user", user),
            pNewId,
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_dunnage_parts_insert",
            parameters,
            _connectionString
        );

        if (result.IsSuccess)
        {
            if (pNewId.Value != null && pNewId.Value != DBNull.Value)
            {
                return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
            }
            return Model_Dao_Result_Factory.Failure<int>("Failed to retrieve new ID");
        }

        return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage, result.Exception);
    }

    public virtual async Task<Model_Dao_Result<int>> InsertWithInventoryAsync(
        string partId,
        int typeId,
        string? udc1,
        string? udc2,
        string? udc3,
        string? udc4,
        string? udc5,
        string? udc6,
        string? udc7,
        string? udc8,
        string? udc9,
        string? udc10,
        string? imagePath,
        string quantityType,
        string homeLocation,
        string inventoryMethod,
        string inventoryNotes,
        string user
    )
    {
        var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_part_id", partId),
            new MySqlParameter("@p_type_id", typeId),
            new MySqlParameter("@p_udc1", (object?)udc1 ?? DBNull.Value),
            new MySqlParameter("@p_udc2", (object?)udc2 ?? DBNull.Value),
            new MySqlParameter("@p_udc3", (object?)udc3 ?? DBNull.Value),
            new MySqlParameter("@p_udc4", (object?)udc4 ?? DBNull.Value),
            new MySqlParameter("@p_udc5", (object?)udc5 ?? DBNull.Value),
            new MySqlParameter("@p_udc6", (object?)udc6 ?? DBNull.Value),
            new MySqlParameter("@p_udc7", (object?)udc7 ?? DBNull.Value),
            new MySqlParameter("@p_udc8", (object?)udc8 ?? DBNull.Value),
            new MySqlParameter("@p_udc9", (object?)udc9 ?? DBNull.Value),
            new MySqlParameter("@p_udc10", (object?)udc10 ?? DBNull.Value),
            new MySqlParameter(
                "@p_image_path",
                string.IsNullOrWhiteSpace(imagePath) ? DBNull.Value : imagePath
            ),
            new MySqlParameter("@p_quantity_type", quantityType),
            new MySqlParameter("@p_home_location", homeLocation),
            new MySqlParameter("@p_inventory_method", inventoryMethod),
            new MySqlParameter("@p_inventory_notes", inventoryNotes),
            new MySqlParameter("@p_user", user),
            pNewId,
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_Dunnage_Parts_InsertWithInventory",
            parameters,
            _connectionString
        );

        if (result.IsSuccess)
        {
            if (pNewId.Value != null && pNewId.Value != DBNull.Value)
            {
                return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
            }

            return Model_Dao_Result_Factory.Failure<int>("Failed to retrieve new ID");
        }

        return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage, result.Exception);
    }

    public virtual async Task<Model_Dao_Result> UpdateAsync(
        int id,
        string partId,
        string? udc1,
        string? udc2,
        string? udc3,
        string? udc4,
        string? udc5,
        string? udc6,
        string? udc7,
        string? udc8,
        string? udc9,
        string? udc10,
        string? imagePath,
        string quantityType,
        string homeLocation,
        string user
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id },
            { "part_id", partId },
            { "udc1", (object?)udc1 ?? DBNull.Value },
            { "udc2", (object?)udc2 ?? DBNull.Value },
            { "udc3", (object?)udc3 ?? DBNull.Value },
            { "udc4", (object?)udc4 ?? DBNull.Value },
            { "udc5", (object?)udc5 ?? DBNull.Value },
            { "udc6", (object?)udc6 ?? DBNull.Value },
            { "udc7", (object?)udc7 ?? DBNull.Value },
            { "udc8", (object?)udc8 ?? DBNull.Value },
            { "udc9", (object?)udc9 ?? DBNull.Value },
            { "udc10", (object?)udc10 ?? DBNull.Value },
            { "image_path", string.IsNullOrWhiteSpace(imagePath) ? DBNull.Value : imagePath },
            { "quantity_type", quantityType },
            { "home_location", homeLocation },
            { "user", user },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_dunnage_parts_update",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> UpdateWithInventoryAndReferencesAsync(
        int id,
        string originalPartId,
        string newPartId,
        string? udc1,
        string? udc2,
        string? udc3,
        string? udc4,
        string? udc5,
        string? udc6,
        string? udc7,
        string? udc8,
        string? udc9,
        string? udc10,
        string? imagePath,
        string quantityType,
        string homeLocation,
        string inventoryMethod,
        string inventoryNotes,
        string user
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "id", id },
            { "original_part_id", originalPartId },
            { "new_part_id", newPartId },
            { "udc1", (object?)udc1 ?? DBNull.Value },
            { "udc2", (object?)udc2 ?? DBNull.Value },
            { "udc3", (object?)udc3 ?? DBNull.Value },
            { "udc4", (object?)udc4 ?? DBNull.Value },
            { "udc5", (object?)udc5 ?? DBNull.Value },
            { "udc6", (object?)udc6 ?? DBNull.Value },
            { "udc7", (object?)udc7 ?? DBNull.Value },
            { "udc8", (object?)udc8 ?? DBNull.Value },
            { "udc9", (object?)udc9 ?? DBNull.Value },
            { "udc10", (object?)udc10 ?? DBNull.Value },
            { "image_path", string.IsNullOrWhiteSpace(imagePath) ? DBNull.Value : imagePath },
            { "quantity_type", quantityType },
            { "home_location", homeLocation },
            { "inventory_method", inventoryMethod },
            { "inventory_notes", inventoryNotes },
            { "user", user },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_Parts_UpdateWithReferences",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> DeleteAsync(int id)
    {
        var parameters = new Dictionary<string, object> { { "id", id } };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_dunnage_parts_delete",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<int>> CountTransactionsAsync(string partId)
    {
        var parameters = new Dictionary<string, object> { { "part_id", partId } };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync<int>(
            _connectionString,
            "sp_Dunnage_Parts_CountTransactions",
            reader => reader.GetInt32(reader.GetOrdinal("transaction_count")),
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchAsync(
        string searchText,
        int? typeId = null
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "search_text", searchText },
            { "type_id", typeId ?? 0 },
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnagePart>(
            _connectionString,
            "sp_dunnage_parts_search",
            MapFromReader,
            parameters
        );
    }

    private Model_DunnagePart MapFromReader(IDataReader reader)
    {
        return new Model_DunnagePart
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            PartId = reader.GetString(reader.GetOrdinal("part_id")),
            TypeId = reader.GetInt32(reader.GetOrdinal("type_id")),
            DunnageTypeName = reader.IsDBNull(reader.GetOrdinal("type_name"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("type_name")),
            QuantityType = reader.IsDBNull(reader.GetOrdinal("quantity_type"))
                ? "Quantity"
                : reader.GetString(reader.GetOrdinal("quantity_type")),
            Udc1 = ReadUdc(reader, "udc1"),
            Udc2 = ReadUdc(reader, "udc2"),
            Udc3 = ReadUdc(reader, "udc3"),
            Udc4 = ReadUdc(reader, "udc4"),
            Udc5 = ReadUdc(reader, "udc5"),
            Udc6 = ReadUdc(reader, "udc6"),
            Udc7 = ReadUdc(reader, "udc7"),
            Udc8 = ReadUdc(reader, "udc8"),
            Udc9 = ReadUdc(reader, "udc9"),
            Udc10 = ReadUdc(reader, "udc10"),
            ImagePath = reader.IsDBNull(reader.GetOrdinal("image_path"))
                ? null
                : reader.GetString(reader.GetOrdinal("image_path")),
            DunnageTypeImagePath = reader.IsDBNull(reader.GetOrdinal("type_image_path"))
                ? null
                : reader.GetString(reader.GetOrdinal("type_image_path")),
            HomeLocation = reader.IsDBNull(reader.GetOrdinal("home_location"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("home_location")),
            CreatedBy = reader.GetString(reader.GetOrdinal("created_by")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("modified_by"))
                ? null
                : reader.GetString(reader.GetOrdinal("modified_by")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("modified_date"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("modified_date")),
        };
    }

    private static string? ReadUdc(IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}

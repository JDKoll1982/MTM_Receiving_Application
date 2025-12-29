using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Core;

namespace MTM_Receiving_Application.Data.Dunnage;

public class Dao_DunnageCustomField
{
    private readonly string _connectionString;

    public Dao_DunnageCustomField(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual async Task<Model_Dao_Result<int>> InsertAsync(int typeId, Model_CustomFieldDefinition field, string user)
    {
        var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_type_id", typeId),
            new MySqlParameter("@p_field_name", field.FieldName),
            new MySqlParameter("@p_field_type", field.FieldType),
            new MySqlParameter("@p_display_order", field.DisplayOrder),
            new MySqlParameter("@p_is_required", field.IsRequired),
            new MySqlParameter("@p_user", user),
            pNewId
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_custom_fields_insert",
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

    public virtual async Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetByTypeAsync(int typeId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "type_id", typeId }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_CustomFieldDefinition>(
            _connectionString,
            "sp_custom_fields_get_by_type",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> DeleteAsync(int fieldId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "field_id", fieldId }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_custom_fields_delete",
            parameters
        );
    }

    private Model_CustomFieldDefinition MapFromReader(IDataReader reader)
    {
        return new Model_CustomFieldDefinition
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            FieldName = reader.GetString(reader.GetOrdinal("field_name")),
            FieldType = reader.GetString(reader.GetOrdinal("field_type")),
            DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order")),
            IsRequired = reader.GetBoolean(reader.GetOrdinal("is_required"))
        };
    }
}

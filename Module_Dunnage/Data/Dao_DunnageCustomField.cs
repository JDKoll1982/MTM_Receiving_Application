using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Dunnage.Data;

public class Dao_DunnageCustomField
{
    private readonly string _connectionString;

    public Dao_DunnageCustomField(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual async Task<Model_Dao_Result<int>> InsertAsync(
        int typeId,
        Model_CustomFieldDefinition field,
        string user
    )
    {
        var databaseColumnName = string.IsNullOrWhiteSpace(field.DatabaseColumnName)
            ? Model_CustomFieldDefinition.BuildDatabaseColumnName(field.FieldName)
            : field.DatabaseColumnName.Trim();
        var pNewId = new MySqlParameter("@p_new_id", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var pStatus = new MySqlParameter("@p_status", MySqlDbType.Int32)
        {
            Direction = ParameterDirection.Output,
        };
        var pErrorMessage = new MySqlParameter("@p_error_msg", MySqlDbType.VarChar, 500)
        {
            Direction = ParameterDirection.Output,
        };

        var parameters = new MySqlParameter[]
        {
            new MySqlParameter("@p_dunnage_type_id", typeId),
            new MySqlParameter("@p_field_name", field.FieldName),
            new MySqlParameter("@p_database_column_name", databaseColumnName),
            new MySqlParameter("@p_field_type", field.FieldType),
            new MySqlParameter("@p_display_order", field.DisplayOrder),
            new MySqlParameter("@p_is_required", field.IsRequired),
            new MySqlParameter("@p_validation_rules", MySqlDbType.Text)
            {
                Value = string.IsNullOrWhiteSpace(field.ValidationRules)
                    ? DBNull.Value
                    : (object)field.ValidationRules,
            },
            new MySqlParameter("@p_user", user),
            pNewId,
            pStatus,
            pErrorMessage,
        };

        var result = await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_Dunnage_CustomFields_Insert",
            parameters,
            _connectionString
        );

        if (!result.IsSuccess)
        {
            return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage, result.Exception);
        }

        var status = pStatus.Value == DBNull.Value ? 0 : Convert.ToInt32(pStatus.Value);
        var errorMessage =
            pErrorMessage.Value == DBNull.Value ? null : pErrorMessage.Value?.ToString();

        if (status <= 0)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                errorMessage ?? "Failed to insert custom field"
            );
        }

        if (pNewId.Value == null || pNewId.Value == DBNull.Value)
        {
            return Model_Dao_Result_Factory.Failure<int>("Failed to retrieve new ID");
        }

        field.DatabaseColumnName = databaseColumnName;
        return Model_Dao_Result_Factory.Success<int>(Convert.ToInt32(pNewId.Value));
    }

    public virtual async Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetByTypeAsync(
        int typeId
    )
    {
        var parameters = new Dictionary<string, object> { { "dunnage_type_id", typeId } };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_CustomFieldDefinition>(
            _connectionString,
            "sp_Dunnage_CustomFields_GetByType",
            MapFromReader,
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result> UpdateAsync(
        int fieldId,
        Model_CustomFieldDefinition field
    )
    {
        var databaseColumnName = string.IsNullOrWhiteSpace(field.DatabaseColumnName)
            ? Model_CustomFieldDefinition.BuildDatabaseColumnName(field.FieldName)
            : field.DatabaseColumnName.Trim();

        var parameters = new MySqlParameter[]
        {
            new("@p_field_id", MySqlDbType.Int32) { Value = fieldId },
            new("@p_field_name", MySqlDbType.VarChar, 100) { Value = field.FieldName },
            new("@p_database_column_name", MySqlDbType.VarChar, 64) { Value = databaseColumnName },
            new("@p_field_type", MySqlDbType.VarChar, 20) { Value = field.FieldType },
            new("@p_display_order", MySqlDbType.Int32) { Value = field.DisplayOrder },
            new("@p_is_required", MySqlDbType.Bit) { Value = field.IsRequired },
            new("@p_validation_rules", MySqlDbType.Text)
            {
                Value = string.IsNullOrWhiteSpace(field.ValidationRules)
                    ? DBNull.Value
                    : (object)field.ValidationRules,
            },
        };

        field.DatabaseColumnName = databaseColumnName;
        return await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_Dunnage_CustomFields_Update",
            parameters,
            _connectionString
        );
    }

    public virtual async Task<Model_Dao_Result> DeleteAsync(int fieldId)
    {
        var parameters = new Dictionary<string, object> { { "field_id", fieldId } };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_CustomFields_Delete",
            parameters
        );
    }

    private Model_CustomFieldDefinition MapFromReader(IDataReader reader)
    {
        return new Model_CustomFieldDefinition
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            DunnageTypeId = reader.GetInt32(reader.GetOrdinal("dunnagetypeid")),
            FieldName = reader.GetString(reader.GetOrdinal("field_name")),
            DatabaseColumnName = reader.GetString(reader.GetOrdinal("databasecolumnname")),
            FieldType = reader.GetString(reader.GetOrdinal("field_type")),
            DisplayOrder = reader.GetInt32(reader.GetOrdinal("display_order")),
            IsRequired = reader.GetBoolean(reader.GetOrdinal("is_required")),
            ValidationRules = reader.IsDBNull(reader.GetOrdinal("validationrules"))
                ? null
                : reader.GetString(reader.GetOrdinal("validationrules")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("createddate")),
            CreatedBy = reader.GetString(reader.GetOrdinal("createdby")),
        };
    }
}

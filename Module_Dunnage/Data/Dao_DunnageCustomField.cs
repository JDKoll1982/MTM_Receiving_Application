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
            new MySqlParameter("@p_field_type", field.FieldType),
            new MySqlParameter("@p_display_order", field.DisplayOrder),
            new MySqlParameter("@p_is_required", field.IsRequired),
            new MySqlParameter("@p_unit", MySqlDbType.VarChar, 50)
            {
                Value = string.IsNullOrWhiteSpace(field.Unit) ? DBNull.Value : (object)field.Unit,
            },
            new MySqlParameter("@p_min_value", MySqlDbType.Decimal)
            {
                Value = field.MinValue.HasValue ? (object)field.MinValue.Value : DBNull.Value,
                Precision = 18,
                Scale = 4,
            },
            new MySqlParameter("@p_max_value", MySqlDbType.Decimal)
            {
                Value = field.MaxValue.HasValue ? (object)field.MaxValue.Value : DBNull.Value,
                Precision = 18,
                Scale = 4,
            },
            new MySqlParameter("@p_default_value", MySqlDbType.VarChar, 255)
            {
                Value = string.IsNullOrWhiteSpace(field.DefaultValue)
                    ? DBNull.Value
                    : (object)field.DefaultValue,
            },
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
        var parameters = new MySqlParameter[]
        {
            new("@p_field_id", MySqlDbType.Int32) { Value = fieldId },
            new("@p_field_name", MySqlDbType.VarChar, 100) { Value = field.FieldName },
            new("@p_field_type", MySqlDbType.VarChar, 20) { Value = field.FieldType },
            new("@p_display_order", MySqlDbType.Int32) { Value = field.DisplayOrder },
            new("@p_is_required", MySqlDbType.Bit) { Value = field.IsRequired },
            new("@p_unit", MySqlDbType.VarChar, 50)
            {
                Value = string.IsNullOrWhiteSpace(field.Unit) ? DBNull.Value : (object)field.Unit,
            },
            new("@p_min_value", MySqlDbType.Decimal)
            {
                Value = field.MinValue.HasValue ? (object)field.MinValue.Value : DBNull.Value,
                Precision = 18,
                Scale = 4,
            },
            new("@p_max_value", MySqlDbType.Decimal)
            {
                Value = field.MaxValue.HasValue ? (object)field.MaxValue.Value : DBNull.Value,
                Precision = 18,
                Scale = 4,
            },
            new("@p_default_value", MySqlDbType.VarChar, 255)
            {
                Value = string.IsNullOrWhiteSpace(field.DefaultValue)
                    ? DBNull.Value
                    : (object)field.DefaultValue,
            },
            new("@p_validation_rules", MySqlDbType.Text)
            {
                Value = string.IsNullOrWhiteSpace(field.ValidationRules)
                    ? DBNull.Value
                    : (object)field.ValidationRules,
            },
        };

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

    public virtual async Task<Model_Dao_Result> InsertChoiceAsync(
        int customFieldId,
        string choice,
        int sortOrder
    )
    {
        var parameters = new MySqlParameter[]
        {
            new("@p_custom_field_id", MySqlDbType.Int32) { Value = customFieldId },
            new("@p_choice", MySqlDbType.VarChar, 255) { Value = choice },
            new("@p_sort_order", MySqlDbType.Int32) { Value = sortOrder },
        };

        return await Helper_Database_StoredProcedure.ExecuteAsync(
            "sp_Dunnage_CustomFieldChoices_Insert",
            parameters,
            _connectionString
        );
    }

    public virtual async Task<Model_Dao_Result> DeleteChoicesByFieldAsync(int customFieldId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "custom_field_id", customFieldId },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_CustomFieldChoices_DeleteByField",
            parameters
        );
    }

    public virtual async Task<Model_Dao_Result<List<Model_DunnageCustomFieldChoice>>>
        GetChoicesByFieldAsync(int customFieldId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "custom_field_id", customFieldId },
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnageCustomFieldChoice>(
            _connectionString,
            "sp_Dunnage_CustomFieldChoices_GetByField",
            MapChoiceFromReader,
            parameters
        );
    }

    private Model_DunnageCustomFieldChoice MapChoiceFromReader(IDataReader reader)
    {
        return new Model_DunnageCustomFieldChoice
        {
            Id = reader.GetInt32(reader.GetOrdinal("ID")),
            CustomFieldId = reader.GetInt32(reader.GetOrdinal("CustomFieldID")),
            Choice = reader.GetString(reader.GetOrdinal("Choice")),
            SortOrder = reader.GetInt32(reader.GetOrdinal("SortOrder")),
        };
    }

    private Model_CustomFieldDefinition MapFromReader(IDataReader reader)
    {
        return new Model_CustomFieldDefinition
        {
            // MySql.Data GetOrdinal is not reliably case-insensitive; use the exact
            // column casing returned by sp_Dunnage_CustomFields_GetByType.
            Id = reader.GetInt32(reader.GetOrdinal("ID")),
            DunnageTypeId = reader.GetInt32(reader.GetOrdinal("DunnageTypeID")),
            FieldName = reader.GetString(reader.GetOrdinal("FieldName")),
            FieldType = reader.GetString(reader.GetOrdinal("FieldType")),
            DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
            IsRequired = reader.GetBoolean(reader.GetOrdinal("IsRequired")),
            Unit = reader.IsDBNull(reader.GetOrdinal("Unit"))
                ? null
                : reader.GetString(reader.GetOrdinal("Unit")),
            MinValue = reader.IsDBNull(reader.GetOrdinal("MinValue"))
                ? null
                : reader.GetDecimal(reader.GetOrdinal("MinValue")),
            MaxValue = reader.IsDBNull(reader.GetOrdinal("MaxValue"))
                ? null
                : reader.GetDecimal(reader.GetOrdinal("MaxValue")),
            DefaultValue = reader.IsDBNull(reader.GetOrdinal("DefaultValue"))
                ? null
                : reader.GetString(reader.GetOrdinal("DefaultValue")),
            ValidationRules = reader.IsDBNull(reader.GetOrdinal("ValidationRules"))
                ? null
                : reader.GetString(reader.GetOrdinal("ValidationRules")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")),
        };
    }
}

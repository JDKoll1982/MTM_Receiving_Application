using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Settings.Core.Models;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data access for Volvo email-recipient settings.
/// </summary>
public class Dao_VolvoRecipientSettings
{
    private readonly string _connectionString;

    public Dao_VolvoRecipientSettings(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<Model_Dao_Result<List<Model_EmailRecipientSetting>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Settings_VolvoRecipients_GetAll",
            MapFromReader,
            new Dictionary<string, object>()
        );
    }

    public async Task<Model_Dao_Result> InsertAsync(Model_EmailRecipientSetting recipient)
    {
        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_VolvoRecipients_Insert",
            BuildParameters(recipient)
        );
    }

    public async Task<Model_Dao_Result> UpdateAsync(Model_EmailRecipientSetting recipient)
    {
        var parameters = BuildParameters(recipient);
        parameters.Add("id", recipient.Id);

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_VolvoRecipients_Update",
            parameters
        );
    }

    public async Task<Model_Dao_Result> DeleteAsync(int recipientId)
    {
        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Settings_VolvoRecipients_Delete",
            new Dictionary<string, object> { { "id", recipientId } }
        );
    }

    private static Dictionary<string, object> BuildParameters(Model_EmailRecipientSetting recipient)
    {
        return new Dictionary<string, object>
        {
            { "first_name", recipient.FirstName },
            { "last_name", recipient.LastName },
            { "recipient_type", recipient.RecipientType },
            { "email", recipient.Email },
        };
    }

    private static Model_EmailRecipientSetting MapFromReader(IDataReader reader)
    {
        return new Model_EmailRecipientSetting
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
            LastName = reader.GetString(reader.GetOrdinal("last_name")),
            RecipientType = reader.GetString(reader.GetOrdinal("recipient_type")),
            Email = reader.GetString(reader.GetOrdinal("email")),
        };
    }
}

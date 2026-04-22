using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Dunnage.Models;

namespace MTM_Receiving_Application.Module_Dunnage.Data;

public class Dao_DunnageQuantityType
{
    private readonly string _connectionString;

    public Dao_DunnageQuantityType(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public virtual async Task<Model_Dao_Result<List<Model_DunnageQuantityType>>> GetAllAsync()
    {
        return await Helper_Database_StoredProcedure.ExecuteListAsync<Model_DunnageQuantityType>(
            _connectionString,
            "sp_Dunnage_QuantityTypes_GetAll",
            MapFromReader
        );
    }

    public virtual async Task<Model_Dao_Result> InsertIfMissingAsync(
        string quantityType,
        string user
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "quantity_type", quantityType },
            { "user", user },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Dunnage_QuantityTypes_InsertIfMissing",
            parameters
        );
    }

    private static Model_DunnageQuantityType MapFromReader(IDataReader reader)
    {
        return new Model_DunnageQuantityType
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            QuantityType = reader.GetString(reader.GetOrdinal("quantity_type")),
        };
    }
}

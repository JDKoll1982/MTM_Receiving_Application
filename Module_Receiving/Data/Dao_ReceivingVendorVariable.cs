using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Receiving.Data;

/// <summary>
/// Data access object for receiving_vendor_variables table operations.
/// Manages global vendor-specific variable name mappings.
/// </summary>
public class Dao_ReceivingVendorVariable
{
    private readonly string _connectionString;

    public Dao_ReceivingVendorVariable(string connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = connectionString;
    }

    /// <summary>
    /// Retrieves all vendor variable mappings from the database.
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_ReceivingVendorVariableMapping>>> GetAllMappingsAsync()
    {
        try
        {
            var mappings = new List<Model_ReceivingVendorVariableMapping>();

            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_receiving_vendor_variables_get_all", connection)
            {
                CommandType = CommandType.StoredProcedure,
            };

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                mappings.Add(Map(reader));
            }

            return Model_Dao_Result_Factory.Success(mappings);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<List<Model_ReceivingVendorVariableMapping>>(
                "Failed to retrieve vendor variable mappings",
                ex
            );
        }
    }

    /// <summary>
    /// Retrieves a single vendor variable mapping by vendor name.
    /// </summary>
    /// <param name="vendorName"></param>
    public async Task<Model_Dao_Result<Model_ReceivingVendorVariableMapping?>> GetMappingByVendorAsync(string vendorName)
    {
        ArgumentNullException.ThrowIfNull(vendorName);

        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_receiving_vendor_variables_get_by_vendor", connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            command.Parameters.AddWithValue("p_vendor_name", vendorName.Trim());

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Model_Dao_Result_Factory.Success<Model_ReceivingVendorVariableMapping?>(Map(reader));
            }

            return Model_Dao_Result_Factory.Success<Model_ReceivingVendorVariableMapping?>(null);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<Model_ReceivingVendorVariableMapping?>(
                "Failed to retrieve vendor variable mapping",
                ex
            );
        }
    }

    /// <summary>
    /// Inserts a new vendor variable mapping into the database.
    /// </summary>
    /// <param name="vendorName"></param>
    /// <param name="variableName"></param>
    public async Task<Model_Dao_Result> InsertMappingAsync(string vendorName, string variableName)
    {
        ArgumentNullException.ThrowIfNull(vendorName);
        ArgumentNullException.ThrowIfNull(variableName);

        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_receiving_vendor_variables_insert", connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            command.Parameters.AddWithValue("p_vendor_name", vendorName.Trim());
            command.Parameters.AddWithValue("p_variable_name", variableName.Trim());

            await command.ExecuteNonQueryAsync();
            return Model_Dao_Result_Factory.Success();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Model_Dao_Result_Factory.Failure(
                $"A mapping for vendor '{vendorName}' already exists",
                ex
            );
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                "Failed to insert vendor variable mapping",
                ex
            );
        }
    }

    /// <summary>
    /// Updates an existing vendor variable mapping in the database.
    /// </summary>
    /// <param name="vendorName"></param>
    /// <param name="variableName"></param>
    public async Task<Model_Dao_Result> UpdateMappingAsync(string vendorName, string variableName)
    {
        ArgumentNullException.ThrowIfNull(vendorName);
        ArgumentNullException.ThrowIfNull(variableName);

        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_receiving_vendor_variables_update", connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            command.Parameters.AddWithValue("p_vendor_name", vendorName.Trim());
            command.Parameters.AddWithValue("p_variable_name", variableName.Trim());

            await command.ExecuteNonQueryAsync();
            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                "Failed to update vendor variable mapping",
                ex
            );
        }
    }

    /// <summary>
    /// Deletes a vendor variable mapping from the database.
    /// </summary>
    /// <param name="vendorName"></param>
    public async Task<Model_Dao_Result> DeleteMappingAsync(string vendorName)
    {
        ArgumentNullException.ThrowIfNull(vendorName);

        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_receiving_vendor_variables_delete", connection)
            {
                CommandType = CommandType.StoredProcedure,
            };
            command.Parameters.AddWithValue("p_vendor_name", vendorName.Trim());

            await command.ExecuteNonQueryAsync();
            return Model_Dao_Result_Factory.Success();
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure(
                "Failed to delete vendor variable mapping",
                ex
            );
        }
    }

    private static Model_ReceivingVendorVariableMapping Map(DbDataReader reader)
    {
        return new Model_ReceivingVendorVariableMapping
        {
            VendorName = reader.IsDBNull(reader.GetOrdinal("vendor_name"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("vendor_name")),
            VariableName = reader.IsDBNull(reader.GetOrdinal("variable_name"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("variable_name")),
        };
    }
}

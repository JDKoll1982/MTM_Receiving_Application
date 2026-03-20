using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Volvo.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data Access Object for volvo_masterdata table.
/// Provides CRUD operations using stored procedures.
/// </summary>
public class Dao_VolvoPart
{
    private readonly string _connectionString;

    public Dao_VolvoPart(string connectionString)
    {
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Gets all Volvo parts, optionally including inactive rows.
    /// </summary>
    /// <param name="includeInactive">True to include inactive parts; otherwise only active parts.</param>
    public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllAsync(
        bool includeInactive = false
    )
    {
        var parameters = new Dictionary<string, object>
        {
            { "include_inactive", includeInactive ? 1 : 0 },
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Volvo_PartMaster_GetAll",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Gets a part by part number.
    /// </summary>
    /// <param name="partNumber">Part number to retrieve.</param>
    public async Task<Model_Dao_Result<Model_VolvoPart>> GetByIdAsync(string partNumber)
    {
        var parameters = new Dictionary<string, object> { { "part_number", partNumber } };

        return await Helper_Database_StoredProcedure.ExecuteSingleAsync(
            _connectionString,
            "sp_Volvo_PartMaster_GetById",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Inserts a new Volvo part.
    /// </summary>
    /// <param name="part">Part to insert.</param>
    public async Task<Model_Dao_Result> InsertAsync(Model_VolvoPart part)
    {
        var parameters = new Dictionary<string, object>
        {
            { "part_number", part.PartNumber },
            { "quantity_per_skid", part.QuantityPerSkid },
            { "is_active", part.IsActive ? 1 : 0 },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Volvo_PartMaster_Insert",
            parameters
        );
    }

    /// <summary>
    /// Updates an existing Volvo part.
    /// </summary>
    /// <param name="part">Part to update.</param>
    public async Task<Model_Dao_Result> UpdateAsync(Model_VolvoPart part)
    {
        var parameters = new Dictionary<string, object>
        {
            { "part_number", part.PartNumber },
            { "quantity_per_skid", part.QuantityPerSkid },
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Volvo_PartMaster_Update",
            parameters
        );
    }

    /// <summary>
    /// Deactivates a Volvo part after verifying it has no active shipment references.
    /// </summary>
    /// <param name="partNumber">Part number to deactivate.</param>
    public async Task<Model_Dao_Result> DeactivateAsync(string partNumber)
    {
        try
        {
            await using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new MySqlCommand("sp_volvo_part_check_references", connection)
            {
                CommandType = CommandType.StoredProcedure,
            };

            command.Parameters.AddWithValue("p_part_number", partNumber);
            var activeReferenceCountParam = new MySqlParameter(
                "p_active_reference_count",
                MySqlDbType.Int32
            )
            {
                Direction = ParameterDirection.Output,
            };
            command.Parameters.Add(activeReferenceCountParam);

            await command.ExecuteNonQueryAsync();

            var activeReferenceCount =
                activeReferenceCountParam.Value == DBNull.Value
                    ? 0
                    : Convert.ToInt32(activeReferenceCountParam.Value);

            if (activeReferenceCount > 0)
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage =
                        $"Part '{partNumber}' cannot be deactivated because it is referenced by {activeReferenceCount} active shipment line(s).",
                    Severity = Enum_ErrorSeverity.Warning,
                };
            }

            var parameters = new Dictionary<string, object>
            {
                { "part_number", partNumber },
                { "is_active", 0 },
            };

            return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
                _connectionString,
                "sp_Volvo_PartMaster_SetActive",
                parameters
            );
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = $"Error deactivating Volvo part: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex,
            };
        }
    }

    /// <summary>
    /// Gets multiple parts by part number.
    /// </summary>
    /// <param name="partNumbers">Part numbers to resolve.</param>
    public async Task<Model_Dao_Result<Dictionary<string, Model_VolvoPart>>> GetPartsByNumbersAsync(
        List<string> partNumbers
    )
    {
        if (partNumbers == null || partNumbers.Count == 0)
        {
            return new Model_Dao_Result<Dictionary<string, Model_VolvoPart>>
            {
                Success = true,
                Data = new Dictionary<string, Model_VolvoPart>(),
            };
        }

        try
        {
            var result = new Dictionary<string, Model_VolvoPart>();

            foreach (var partNumber in partNumbers)
            {
                var partResult = await GetByIdAsync(partNumber);
                if (partResult.IsSuccess && partResult.Data != null)
                {
                    result[partNumber] = partResult.Data;
                }
            }

            return new Model_Dao_Result<Dictionary<string, Model_VolvoPart>>
            {
                Success = true,
                Data = result,
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<Dictionary<string, Model_VolvoPart>>
            {
                Success = false,
                ErrorMessage = $"Error retrieving parts batch: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex,
            };
        }
    }

    private static Model_VolvoPart MapFromReader(IDataReader reader)
    {
        return new Model_VolvoPart
        {
            PartNumber = reader.GetString(reader.GetOrdinal("part_number")),
            QuantityPerSkid = reader.GetInt32(reader.GetOrdinal("quantity_per_skid")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("is_active")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("created_date")),
            ModifiedDate = reader.GetDateTime(reader.GetOrdinal("modified_date")),
        };
    }
}

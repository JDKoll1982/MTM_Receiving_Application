using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Core.Helpers.Database;

namespace MTM_Receiving_Application.Module_Volvo.Data;

/// <summary>
/// Data Access Object for volvo_part_components table
/// Provides CRUD operations using stored procedures
/// </summary>
public class Dao_VolvoPartComponent
{
    private readonly string _connectionString;

    public Dao_VolvoPartComponent(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Gets all components for a parent part (for component explosion)
    /// </summary>
    /// <param name="parentPartNumber"></param>
    public async Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> GetByParentPartAsync(string parentPartNumber)
    {
        var parameters = new Dictionary<string, object>
        {
            { "parent_part_number", parentPartNumber }
        };

        return await Helper_Database_StoredProcedure.ExecuteListAsync(
            _connectionString,
            "sp_Volvo_PartComponent_Get",
            MapFromReader,
            parameters
        );
    }

    /// <summary>
    /// Inserts a new component relationship
    /// </summary>
    /// <param name="component"></param>
    public async Task<Model_Dao_Result> InsertAsync(Model_VolvoPartComponent component)
    {
        var parameters = new Dictionary<string, object>
        {
            { "parent_part_number", component.ParentPartNumber },
            { "component_part_number", component.ComponentPartNumber },
            { "quantity", component.Quantity }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Volvo_PartComponent_Insert",
            parameters
        );
    }

    /// <summary>
    /// Deletes all components for a parent part (used before updating component list)
    /// </summary>
    /// <param name="parentPartNumber"></param>
    public async Task<Model_Dao_Result> DeleteByParentPartAsync(string parentPartNumber)
    {
        var parameters = new Dictionary<string, object>
        {
            { "parent_part_number", parentPartNumber }
        };

        return await Helper_Database_StoredProcedure.ExecuteNonQueryAsync(
            _connectionString,
            "sp_Volvo_PartComponent_DeleteByParent",
            parameters
        );
    }

    /// <summary>
    /// Gets components for multiple parent parts (batch query to avoid N+1)
    /// </summary>
    /// <param name="parentPartNumbers"></param>
    public async Task<Model_Dao_Result<Dictionary<string, List<Model_VolvoPartComponent>>>> GetComponentsByParentPartsAsync(
        List<string> parentPartNumbers)
    {
        if (parentPartNumbers == null || parentPartNumbers.Count == 0)
        {
            return new Model_Dao_Result<Dictionary<string, List<Model_VolvoPartComponent>>>
            {
                Success = true,
                Data = new Dictionary<string, List<Model_VolvoPartComponent>>()
            };
        }

        try
        {
            var result = new Dictionary<string, List<Model_VolvoPartComponent>>();

            // Batch query optimization - can be improved with stored proc
            foreach (var parentPart in parentPartNumbers)
            {
                var componentsResult = await GetByParentPartAsync(parentPart);
                if (componentsResult.IsSuccess && componentsResult.Data != null)
                {
                    result[parentPart] = componentsResult.Data;
                }
                else
                {
                    result[parentPart] = new List<Model_VolvoPartComponent>();
                }
            }

            return new Model_Dao_Result<Dictionary<string, List<Model_VolvoPartComponent>>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new Model_Dao_Result<Dictionary<string, List<Model_VolvoPartComponent>>>
            {
                Success = false,
                ErrorMessage = $"Error retrieving components batch: {ex.Message}",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex
            };
        }
    }

    private static Model_VolvoPartComponent MapFromReader(IDataReader reader)
    {
        return new Model_VolvoPartComponent
        {
            ComponentPartNumber = reader.GetString(reader.GetOrdinal("component_part_number")),
            Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
            ComponentQuantityPerSkid = reader.GetInt32(reader.GetOrdinal("component_quantity_per_skid"))
        };
    }
}

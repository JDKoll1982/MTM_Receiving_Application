using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

/// <summary>
/// Service for managing global vendor-specific variable name mappings.
/// Provides business logic layer for receiving_vendor_variables data access.
/// </summary>
public interface IService_MySQL_ReceivingVendorVariable
{
    /// <summary>
    /// Retrieves all vendor variable mappings.
    /// </summary>
    /// <returns>DAO result containing list of all mappings</returns>
    Task<Model_Dao_Result<List<Model_ReceivingVendorVariableMapping>>> GetAllMappingsAsync();

    /// <summary>
    /// Retrieves the variable name mapping for a specific vendor.
    /// </summary>
    /// <param name="vendorName">The vendor name to look up</param>
    /// <returns>DAO result containing the mapping, or null if not found</returns>
    Task<Model_Dao_Result<Model_ReceivingVendorVariableMapping?>> GetMappingByVendorAsync(string vendorName);

    /// <summary>
    /// Saves a vendor variable mapping. Inserts if new, updates if existing.
    /// </summary>
    /// <param name="vendorName">The vendor name</param>
    /// <param name="variableName">The variable name for this vendor</param>
    /// <returns>DAO result indicating success or failure</returns>
    Task<Model_Dao_Result> SaveMappingAsync(string vendorName, string variableName);

    /// <summary>
    /// Deletes a vendor variable mapping.
    /// </summary>
    /// <param name="vendorName">The vendor name to delete</param>
    /// <returns>DAO result indicating success or failure</returns>
    Task<Model_Dao_Result> DeleteMappingAsync(string vendorName);
}

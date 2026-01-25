using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Contracts;

/// <summary>
/// Service contract for quality hold operations
/// Provides business logic layer for restricted part tracking (MMFSR/MMCSR)
/// </summary>
public interface IService_Receiving_Business_MySQL_QualityHold
{
    /// <summary>
    /// Creates a new quality hold record in the database
    /// </summary>
    /// <param name="qualityHold">Quality hold data to insert</param>
    /// <returns>Result with quality hold ID</returns>
    Task<Model_Dao_Result<int>> InsertQualityHoldAsync(Model_Receiving_Entity_QualityHold qualityHold);

    /// <summary>
    /// Retrieves all quality holds for a specific load
    /// </summary>
    /// <param name="loadId">Load ID to query</param>
    /// <returns>Result with list of quality holds</returns>
    Task<Model_Dao_Result<List<Model_Receiving_Entity_QualityHold>>> GetQualityHoldsByLoadIDAsync(int loadId);

    /// <summary>
    /// Updates quality hold with acknowledgment information
    /// </summary>
    /// <param name="qualityHoldId">Quality hold ID to update</param>
    /// <param name="acknowledgedBy">Username who acknowledged</param>
    /// <param name="acknowledgedAt">Timestamp of acknowledgment</param>
    /// <returns>Result with success status</returns>
    Task<Model_Dao_Result> UpdateQualityHoldAcknowledgmentAsync(int qualityHoldId, string acknowledgedBy, DateTime acknowledgedAt);
}

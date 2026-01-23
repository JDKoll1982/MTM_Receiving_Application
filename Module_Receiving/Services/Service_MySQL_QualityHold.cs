using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Service implementation for quality hold database operations
/// Provides business logic abstraction over Dao_QualityHold
/// </summary>
public class Service_MySQL_QualityHold : IService_MySQL_QualityHold
{
    private readonly Dao_QualityHold _dao;
    private readonly IService_LoggingUtility _logger;

    public Service_MySQL_QualityHold(
        Dao_QualityHold dao,
        IService_LoggingUtility logger)
    {
        _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new quality hold record in the database
    /// </summary>
    public async Task<Model_Dao_Result<int>> InsertQualityHoldAsync(Model_QualityHold qualityHold)
    {
        _logger.LogInfo($"Inserting quality hold for Load ID: {qualityHold.LoadID}, Part: {qualityHold.PartID}, Type: {qualityHold.RestrictionType}");

        var result = await _dao.InsertQualityHoldAsync(qualityHold);

        if (result.Success)
        {
            _logger.LogInfo($"Quality hold inserted successfully with ID: {result.Data}");
        }
        else
        {
            _logger.LogError($"Failed to insert quality hold: {result.ErrorMessage}");
        }

        return result;
    }

    /// <summary>
    /// Retrieves all quality holds for a specific load
    /// </summary>
    public async Task<Model_Dao_Result<List<Model_QualityHold>>> GetQualityHoldsByLoadIDAsync(int loadId)
    {
        _logger.LogInfo($"Retrieving quality holds for Load ID: {loadId}");

        var result = await _dao.GetQualityHoldsByLoadIDAsync(loadId);

        if (result.Success)
        {
            _logger.LogInfo($"Retrieved {result.Data?.Count ?? 0} quality hold(s) for Load ID: {loadId}");
        }
        else
        {
            _logger.LogError($"Failed to retrieve quality holds for Load ID {loadId}: {result.ErrorMessage}");
        }

        return result;
    }

    /// <summary>
    /// Updates quality hold with acknowledgment information
    /// </summary>
    public async Task<Model_Dao_Result> UpdateQualityHoldAcknowledgmentAsync(int qualityHoldId, string acknowledgedBy, DateTime acknowledgedAt)
    {
        _logger.LogInfo($"Updating quality hold acknowledgment for ID: {qualityHoldId}, By: {acknowledgedBy}");

        var result = await _dao.UpdateQualityHoldAcknowledgmentAsync(qualityHoldId, acknowledgedBy, acknowledgedAt);

        if (result.Success)
        {
            _logger.LogInfo($"Quality hold {qualityHoldId} acknowledged successfully by {acknowledgedBy}");
        }
        else
        {
            _logger.LogError($"Failed to update quality hold {qualityHoldId}: {result.ErrorMessage}");
        }

        return result;
    }
}

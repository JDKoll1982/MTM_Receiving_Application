using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;

namespace MTM_Receiving_Application.Module_Receiving.Services;

/// <summary>
/// Service for managing global vendor-specific variable name mappings.
/// Provides business logic layer over Dao_ReceivingVendorVariable.
/// </summary>
public class Service_MySQL_ReceivingVendorVariable : IService_MySQL_ReceivingVendorVariable
{
    private readonly Dao_ReceivingVendorVariable _dao;
    private readonly IService_LoggingUtility _logger;

    public Service_MySQL_ReceivingVendorVariable(
        Dao_ReceivingVendorVariable dao,
        IService_LoggingUtility logger)
    {
        ArgumentNullException.ThrowIfNull(dao);
        ArgumentNullException.ThrowIfNull(logger);
        _dao = dao;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<List<Model_ReceivingVendorVariableMapping>>> GetAllMappingsAsync()
    {
        _logger.LogInfo("Retrieving all vendor variable mappings");
        var result = await _dao.GetAllMappingsAsync();
        if (result.IsSuccess)
        {
            _logger.LogInfo($"Retrieved {result.Data?.Count ?? 0} vendor variable mappings");
        }
        else
        {
            _logger.LogError($"Failed to retrieve vendor variable mappings: {result.ErrorMessage}");
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result<Model_ReceivingVendorVariableMapping?>> GetMappingByVendorAsync(string vendorName)
    {
        ArgumentNullException.ThrowIfNull(vendorName);
        _logger.LogInfo($"Retrieving vendor variable mapping for: {vendorName}");
        var result = await _dao.GetMappingByVendorAsync(vendorName);

        #if DEBUG
        if (result.IsSuccess)
        {
            if (result.Data is not null)
            {
                Debug.WriteLine($"[Service_MySQL_ReceivingVendorVariable] GetMappingByVendorAsync: Found mapping for '{vendorName}' -> '{result.Data.VariableName}'");
            }
            else
            {
                Debug.WriteLine($"[Service_MySQL_ReceivingVendorVariable] GetMappingByVendorAsync: No mapping for '{vendorName}'");
            }
        }
        else
        {
            Debug.WriteLine($"[Service_MySQL_ReceivingVendorVariable] GetMappingByVendorAsync: ERROR for '{vendorName}': {result.ErrorMessage}");
        }
        #endif

        if (result.IsSuccess && result.Data is not null)
        {
            _logger.LogInfo($"Found mapping: {vendorName} -> {result.Data.VariableName}");
        }
        else if (result.IsSuccess)
        {
            _logger.LogInfo($"No mapping found for vendor: {vendorName}");
        }
        else
        {
            _logger.LogError($"Failed to retrieve mapping for {vendorName}: {result.ErrorMessage}");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result> SaveMappingAsync(string vendorName, string variableName)
    {
        ArgumentNullException.ThrowIfNull(vendorName);
        ArgumentNullException.ThrowIfNull(variableName);

        if (string.IsNullOrWhiteSpace(vendorName))
        {
            return Model_Dao_Result_Factory.Failure("Vendor name cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(variableName))
        {
            return Model_Dao_Result_Factory.Failure("Variable name cannot be empty");
        }

        _logger.LogInfo($"Saving vendor variable mapping: {vendorName} -> {variableName}");

        var existingResult = await _dao.GetMappingByVendorAsync(vendorName);
        if (!existingResult.IsSuccess)
        {
            _logger.LogError($"Failed to check for existing mapping: {existingResult.ErrorMessage}");
            return Model_Dao_Result_Factory.Failure(
                "Failed to verify existing mapping",
                existingResult.Exception
            );
        }

        Model_Dao_Result result;
        if (existingResult.Data is not null)
        {
            _logger.LogInfo($"Updating existing mapping for vendor: {vendorName}");
            result = await _dao.UpdateMappingAsync(vendorName, variableName);
        }
        else
        {
            _logger.LogInfo($"Inserting new mapping for vendor: {vendorName}");
            result = await _dao.InsertMappingAsync(vendorName, variableName);
        }

        if (result.IsSuccess)
        {
            _logger.LogInfo($"Successfully saved mapping: {vendorName} -> {variableName}");
        }
        else
        {
            _logger.LogError($"Failed to save mapping: {result.ErrorMessage}");
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Model_Dao_Result> DeleteMappingAsync(string vendorName)
    {
        ArgumentNullException.ThrowIfNull(vendorName);
        _logger.LogInfo($"Deleting vendor variable mapping for: {vendorName}");
        var result = await _dao.DeleteMappingAsync(vendorName);
        if (result.IsSuccess)
        {
            _logger.LogInfo($"Successfully deleted mapping for vendor: {vendorName}");
        }
        else
        {
            _logger.LogError($"Failed to delete mapping for {vendorName}: {result.ErrorMessage}");
        }
        return result;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Models;

namespace MTM_Receiving_Application.Module_Volvo.Services;

/// <summary>
/// Service for managing Volvo parts master data including CRUD operations and data import/export.
/// </summary>
[Obsolete("Legacy service - replaced by CQRS handlers. Do not use in new code.", false)]
public class Service_VolvoMasterData : IService_VolvoMasterData
{
    private readonly Dao_VolvoPart _daoPart;
    private readonly Dao_VolvoPartComponent _daoComponent;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_ErrorHandler _errorHandler;

    public Service_VolvoMasterData(
        Dao_VolvoPart daoPart,
        Dao_VolvoPartComponent daoComponent,
        IService_LoggingUtility logger,
        IService_ErrorHandler errorHandler
    )
    {
        _daoPart = daoPart ?? throw new ArgumentNullException(nameof(daoPart));
        _daoComponent = daoComponent ?? throw new ArgumentNullException(nameof(daoComponent));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
    }

    public async Task<Model_Dao_Result<List<Model_VolvoPart>>> GetAllPartsAsync(
        bool includeInactive
    )
    {
        try
        {
            await _logger.LogInfoAsync(
                $"Getting all Volvo parts (includeInactive={includeInactive})"
            );
            var result = await _daoPart.GetAllAsync(includeInactive);

            if (!result.IsSuccess)
            {
                await _logger.LogErrorAsync($"Failed to get parts: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting all parts: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                "Failed to retrieve parts catalog",
                Enum_ErrorSeverity.Medium,
                ex,
                showDialog: false
            );
            return Model_Dao_Result_Factory.Failure<List<Model_VolvoPart>>(
                $"Error retrieving parts: {ex.Message}"
            );
        }
    }

    public async Task<Model_Dao_Result<Model_VolvoPart?>> GetPartByNumberAsync(string partNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Getting part by number: {partNumber}");
            var result = await _daoPart.GetByIdAsync(partNumber);

            if (!result.IsSuccess)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get part {partNumber}: {result.ErrorMessage}"
                );
                return Model_Dao_Result_Factory.Failure<Model_VolvoPart?>(result.ErrorMessage);
            }

            return Model_Dao_Result_Factory.Success<Model_VolvoPart?>(result.Data);
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error getting part {partNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_VolvoPart?>(
                $"Error retrieving part: {ex.Message}"
            );
        }
    }

    public async Task<Model_Dao_Result> AddPartAsync(
        Model_VolvoPart part,
        List<Model_VolvoPartComponent>? components = null
    )
    {
        try
        {
            await _logger.LogInfoAsync($"Adding new part: {part.PartNumber}");

            // Validate input
            if (string.IsNullOrWhiteSpace(part.PartNumber))
            {
                return Model_Dao_Result_Factory.Failure("Part number is required");
            }

            if (part.QuantityPerSkid < 0)
            {
                return Model_Dao_Result_Factory.Failure("Quantity per skid must be non-negative");
            }

            // Insert new part
            var insertResult = await _daoPart.InsertAsync(part);
            if (!insertResult.IsSuccess)
            {
                await _logger.LogErrorAsync(
                    $"Failed to insert part {part.PartNumber}: {insertResult.ErrorMessage}"
                );
                return Model_Dao_Result_Factory.Failure(insertResult.ErrorMessage);
            }

            // Insert components if provided
            if (components?.Any() == true)
            {
                foreach (var component in components)
                {
                    component.ParentPartNumber = part.PartNumber;
                    var componentResult = await _daoComponent.InsertAsync(component);
                    if (!componentResult.IsSuccess)
                    {
                        await _logger.LogErrorAsync(
                            $"Failed to insert component {component.ComponentPartNumber}: {componentResult.ErrorMessage}"
                        );
                        return Model_Dao_Result_Factory.Failure(
                            $"Failed to save component {component.ComponentPartNumber}"
                        );
                    }
                }
            }

            await _logger.LogInfoAsync(
                $"Successfully added part {part.PartNumber} with {components?.Count ?? 0} components"
            );
            return Model_Dao_Result_Factory.Success("Part added successfully");
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error adding part {part.PartNumber}: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                $"Failed to add part {part.PartNumber}",
                Enum_ErrorSeverity.Error,
                ex,
                showDialog: false
            );
            return Model_Dao_Result_Factory.Failure($"Error adding part: {ex.Message}");
        }
    }

    public async Task<Model_Dao_Result> UpdatePartAsync(
        Model_VolvoPart part,
        List<Model_VolvoPartComponent>? components = null
    )
    {
        try
        {
            await _logger.LogInfoAsync($"Updating part: {part.PartNumber}");

            // Validate input
            if (string.IsNullOrWhiteSpace(part.PartNumber))
            {
                return Model_Dao_Result_Factory.Failure("Part number is required");
            }

            if (part.QuantityPerSkid < 0)
            {
                return Model_Dao_Result_Factory.Failure("Quantity per skid must be non-negative");
            }

            // Update existing part
            var updateResult = await _daoPart.UpdateAsync(part);
            if (!updateResult.IsSuccess)
            {
                await _logger.LogErrorAsync(
                    $"Failed to update part {part.PartNumber}: {updateResult.ErrorMessage}"
                );
                return Model_Dao_Result_Factory.Failure(updateResult.ErrorMessage);
            }

            // Update components (delete existing and re-insert)
            if (components?.Any() == true)
            {
                // Delete existing components
                var deleteResult = await _daoComponent.DeleteByParentPartAsync(part.PartNumber);
                if (!deleteResult.IsSuccess)
                {
                    await _logger.LogErrorAsync(
                        $"Failed to delete components for {part.PartNumber}: {deleteResult.ErrorMessage}"
                    );
                    return Model_Dao_Result_Factory.Failure(
                        $"Failed to update components: {deleteResult.ErrorMessage}"
                    );
                }

                // Insert new components
                foreach (var component in components)
                {
                    component.ParentPartNumber = part.PartNumber;
                    var insertResult = await _daoComponent.InsertAsync(component);
                    if (!insertResult.IsSuccess)
                    {
                        await _logger.LogErrorAsync(
                            $"Failed to insert component {component.ComponentPartNumber}: {insertResult.ErrorMessage}"
                        );
                        return Model_Dao_Result_Factory.Failure(
                            $"Failed to save component {component.ComponentPartNumber}"
                        );
                    }
                }
            }
            else
            {
                // No components - delete any existing ones
                await _daoComponent.DeleteByParentPartAsync(part.PartNumber);
            }

            await _logger.LogInfoAsync(
                $"Successfully updated part {part.PartNumber} with {components?.Count ?? 0} components"
            );
            return Model_Dao_Result_Factory.Success("Part updated successfully");
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error updating part {part.PartNumber}: {ex.Message}", ex);
            await _errorHandler.HandleErrorAsync(
                $"Failed to update part {part.PartNumber}",
                Enum_ErrorSeverity.Error,
                ex,
                showDialog: false
            );
            return Model_Dao_Result_Factory.Failure($"Error updating part: {ex.Message}");
        }
    }

    public async Task<Model_Dao_Result> DeactivatePartAsync(string partNumber)
    {
        try
        {
            await _logger.LogInfoAsync($"Deactivating part: {partNumber}");

            var result = await _daoPart.DeactivateAsync(partNumber);

            if (!result.IsSuccess)
            {
                await _logger.LogErrorAsync(
                    $"Failed to deactivate part {partNumber}: {result.ErrorMessage}"
                );
            }
            else
            {
                await _logger.LogInfoAsync($"Successfully deactivated part {partNumber}");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error deactivating part {partNumber}: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure($"Error deactivating part: {ex.Message}");
        }
    }

    public async Task<Model_Dao_Result<List<Model_VolvoPartComponent>>> GetComponentsAsync(
        string partNumber
    )
    {
        try
        {
            await _logger.LogInfoAsync($"Getting components for part: {partNumber}");
            var result = await _daoComponent.GetByParentPartAsync(partNumber);

            if (!result.IsSuccess)
            {
                await _logger.LogErrorAsync(
                    $"Failed to get components for {partNumber}: {result.ErrorMessage}"
                );
            }

            return result;
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(
                $"Error getting components for {partNumber}: {ex.Message}",
                ex
            );
            return Model_Dao_Result_Factory.Failure<List<Model_VolvoPartComponent>>(
                $"Error retrieving components: {ex.Message}"
            );
        }
    }

    public async Task<Model_Dao_Result<(int New, int Updated, int Unchanged)>> ImportDataAsync(
        System.Collections.Generic.List<(string PartNumber, int QuantityPerSkid)> parts
    )
    {
        if (parts == null || parts.Count == 0)
        {
            return Model_Dao_Result_Factory.Failure<(int, int, int)>(
                "At least one part is required"
            );
        }

        int newCount = 0;
        int updatedCount = 0;
        int unchangedCount = 0;

        foreach (var (partNumber, quantityPerSkid) in parts)
        {
            var part = new Model_VolvoPart
            {
                PartNumber = partNumber.Trim().ToUpperInvariant(),
                QuantityPerSkid = quantityPerSkid,
                IsActive = true,
            };

            var existing = await _daoPart.GetByIdAsync(part.PartNumber);
            bool isNew = !existing.IsSuccess || existing.Data == null;

            if (!isNew && existing.Data!.QuantityPerSkid == quantityPerSkid)
            {
                unchangedCount++;
                continue;
            }

            var saveResult = isNew
                ? await AddPartAsync(
                    part,
                    new System.Collections.Generic.List<Model_VolvoPartComponent>()
                )
                : await UpdatePartAsync(
                    part,
                    new System.Collections.Generic.List<Model_VolvoPartComponent>()
                );

            if (saveResult.IsSuccess)
            {
                if (isNew)
                    newCount++;
                else
                    updatedCount++;
            }
        }

        await _logger.LogInfoAsync(
            $"Import complete: {newCount} new, {updatedCount} updated, {unchangedCount} unchanged"
        );
        return Model_Dao_Result_Factory.Success((newCount, updatedCount, unchangedCount));
    }
}

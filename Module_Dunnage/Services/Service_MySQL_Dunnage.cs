using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MySql.Data.MySqlClient;

namespace MTM_Receiving_Application.Module_Dunnage.Services
{
    public class Service_MySQL_Dunnage : IService_MySQL_Dunnage
    {
        private readonly IService_ErrorHandler _errorHandler;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly Dao_DunnageLoad _daoDunnageLoad;
        private readonly Dao_DunnageLabelData _daoDunnageLabelData;
        private readonly Dao_DunnageType _daoDunnageType;
        private readonly Dao_DunnagePart _daoDunnagePart;
        private readonly Dao_DunnageQuantityType _daoDunnageQuantityType;
        private readonly Dao_DunnageSpec _daoDunnageSpec;
        private readonly Dao_InventoriedDunnage _daoInventoriedDunnage;
        private readonly Dao_DunnageCustomField _daoCustomField;
        private readonly Dao_DunnageUserPreference _daoUserPreference;
        private readonly Dao_DunnageNonPOEntry _daoNonPOEntry;
        private readonly IService_DunnageImageStorage _imageStorage;

        private string CurrentUser =>
            _sessionManager.CurrentSession?.User?.WindowsUsername ?? "System";

        public Service_MySQL_Dunnage(
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_UserSessionManager sessionManager,
            Dao_DunnageLoad daoDunnageLoad,
            Dao_DunnageLabelData daoDunnageLabelData,
            Dao_DunnageType daoDunnageType,
            Dao_DunnagePart daoDunnagePart,
            Dao_DunnageQuantityType daoDunnageQuantityType,
            Dao_DunnageSpec daoDunnageSpec,
            Dao_InventoriedDunnage daoInventoriedDunnage,
            Dao_DunnageCustomField daoCustomField,
            Dao_DunnageUserPreference daoUserPreference,
            Dao_DunnageNonPOEntry daoNonPOEntry,
            IService_DunnageImageStorage imageStorage
        )
        {
            _errorHandler = errorHandler;
            _logger = logger;
            _sessionManager = sessionManager;
            _daoDunnageLoad = daoDunnageLoad;
            _daoDunnageLabelData = daoDunnageLabelData;
            _daoDunnageType = daoDunnageType;
            _daoDunnagePart = daoDunnagePart;
            _daoDunnageQuantityType = daoDunnageQuantityType;
            _daoDunnageSpec = daoDunnageSpec;
            _daoInventoriedDunnage = daoInventoriedDunnage;
            _daoCustomField = daoCustomField;
            _daoUserPreference = daoUserPreference;
            _daoNonPOEntry = daoNonPOEntry;
            _imageStorage = imageStorage;
        }

        // ==================== Type Operations ====================

        public async Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync()
        {
            try
            {
                return await _daoDunnageType.GetAllAsync();
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetAllTypesAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageType>>(
                    $"Error retrieving dunnage types: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId)
        {
            try
            {
                return await _daoDunnageType.GetByIdAsync(typeId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetTypeByIdAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<Model_DunnageType>(
                    $"Error retrieving dunnage type: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<int>> InsertTypeAsync(
            string typeName,
            string icon,
            string? imagePath = null
        )
        {
            try
            {
                var imagePathResult = await PrepareTypeImagePathAsync(imagePath, typeName);
                if (!imagePathResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<int>(imagePathResult.ErrorMessage);
                }

                return await _daoDunnageType.InsertAsync(
                    typeName,
                    icon,
                    imagePathResult.Data,
                    CurrentUser
                );
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(InsertTypeAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<int>(
                    $"Error inserting dunnage type: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type)
        {
            try
            {
                await _logger.LogInfoAsync(
                    $"Inserting new dunnage type: {type.TypeName} (Icon: {type.Icon}) by user: {CurrentUser}"
                );
                var imagePathResult = await PrepareTypeImagePathAsync(
                    type.ImagePath,
                    type.TypeName
                );
                if (!imagePathResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure(imagePathResult.ErrorMessage);
                }

                type.ImagePath = imagePathResult.Data;
                var result = await _daoDunnageType.InsertAsync(
                    type.TypeName,
                    type.Icon,
                    type.ImagePath,
                    CurrentUser
                );
                if (result.IsSuccess)
                {
                    type.Id = result.Data;
                    await _logger.LogInfoAsync(
                        $"Successfully inserted dunnage type '{type.TypeName}' with ID: {type.Id}"
                    );
                    return Model_Dao_Result_Factory.Success();
                }

                if (
                    result.ErrorMessage.Contains("Duplicate entry")
                    || result.ErrorMessage.Contains("Dunnage type name already exists")
                )
                {
                    await _logger.LogWarningAsync(
                        $"Failed to insert dunnage type '{type.TypeName}': name already in use"
                    );
                    return Model_Dao_Result_Factory.Failure(
                        $"The dunnage type name '{type.TypeName}' is already in use."
                    );
                }

                await _logger.LogErrorAsync(
                    $"Failed to insert dunnage type '{type.TypeName}': {result.ErrorMessage}"
                );
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in InsertTypeAsync for type '{type.TypeName}': {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(InsertTypeAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error inserting dunnage type: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type)
        {
            try
            {
                await _logger.LogInfoAsync(
                    $"Updating dunnage type ID {type.Id}: {type.TypeName} (Icon: {type.Icon}) by user: {CurrentUser}"
                );
                var existingTypeResult = await _daoDunnageType.GetByIdAsync(type.Id);
                var previousImagePath =
                    existingTypeResult.IsSuccess && existingTypeResult.Data is not null
                        ? existingTypeResult.Data.ImagePath
                        : null;

                var imagePathResult = await PrepareTypeImagePathAsync(
                    type.ImagePath,
                    type.TypeName
                );
                if (!imagePathResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure(imagePathResult.ErrorMessage);
                }

                type.ImagePath = imagePathResult.Data;
                var result = await _daoDunnageType.UpdateAsync(
                    type.Id,
                    type.TypeName,
                    type.Icon,
                    type.ImagePath,
                    CurrentUser
                );

                if (
                    !result.IsSuccess
                    && (
                        result.ErrorMessage.Contains("Duplicate entry")
                        || result.ErrorMessage.Contains("Dunnage type name already exists")
                    )
                )
                {
                    await _logger.LogWarningAsync(
                        $"Failed to update dunnage type ID {type.Id}: name already in use for '{type.TypeName}'"
                    );
                    return Model_Dao_Result_Factory.Failure(
                        $"The dunnage type name '{type.TypeName}' is already in use."
                    );
                }

                if (result.IsSuccess)
                {
                    await CleanupReplacedImageAsync(previousImagePath, type.ImagePath);
                    await _logger.LogInfoAsync(
                        $"Successfully updated dunnage type ID {type.Id}: {type.TypeName}"
                    );
                }
                else
                {
                    await _logger.LogErrorAsync(
                        $"Failed to update dunnage type ID {type.Id}: {result.ErrorMessage}"
                    );
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in UpdateTypeAsync for type ID {type.Id}: {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(UpdateTypeAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error updating dunnage type: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> DeleteTypeAsync(int typeId)
        {
            try
            {
                await _logger.LogInfoAsync(
                    $"Attempting to delete dunnage type ID {typeId} by user: {CurrentUser}"
                );

                // Check if any parts are using this type
                var partsResult = await _daoDunnagePart.GetByTypeAsync(typeId);
                if (partsResult.IsSuccess && partsResult.Data?.Count > 0)
                {
                    var partCount = partsResult.Data.Count;
                    var usageText =
                        partCount == 1
                            ? "1 part still uses it."
                            : $"{partCount} parts still use it.";
                    var followUpText =
                        partCount == 1
                            ? "Reassign or delete that part first."
                            : "Reassign or delete those parts first.";

                    await _logger.LogWarningAsync(
                        $"Cannot delete dunnage type ID {typeId}: Used by {partCount} parts"
                    );
                    return Model_Dao_Result_Factory.Failure(
                        $"This type can't be deleted because {usageText} {followUpText}"
                    );
                }

                var existingTypeResult = await _daoDunnageType.GetByIdAsync(typeId);
                var deleteSpecsResult = await _daoDunnageSpec.DeleteByTypeAsync(typeId);
                if (!deleteSpecsResult.IsSuccess)
                {
                    await _logger.LogErrorAsync(
                        $"Failed to delete specs for dunnage type ID {typeId}: {deleteSpecsResult.ErrorMessage}"
                    );
                    return deleteSpecsResult;
                }

                var result = await _daoDunnageType.DeleteAsync(typeId, CurrentUser);
                if (result.IsSuccess)
                {
                    await _imageStorage.DeleteImageAsync(
                        existingTypeResult.IsSuccess ? existingTypeResult.Data?.ImagePath : null
                    );
                    await _logger.LogInfoAsync($"Successfully deleted dunnage type ID {typeId}");
                }
                else
                {
                    await _logger.LogErrorAsync(
                        $"Failed to delete dunnage type ID {typeId}: {result.ErrorMessage}"
                    );
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in DeleteTypeAsync for type ID {typeId}: {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(DeleteTypeAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error deleting dunnage type: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<int>> CheckDuplicateTypeNameAsync(string typeName)
        {
            try
            {
                var result = await _daoDunnageType.CheckDuplicateNameAsync(typeName);
                if (result.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Success<int>(result.Data ? 1 : 0);
                }
                return Model_Dao_Result_Factory.Failure<int>(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(CheckDuplicateTypeNameAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<int>(
                    $"Error checking duplicate type name: {ex.Message}"
                );
            }
        }

        // ==================== Spec Operations ====================

        public async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetSpecsForTypeAsync(
            int typeId
        )
        {
            try
            {
                return await _daoDunnageSpec.GetByTypeAsync(typeId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetSpecsForTypeAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageSpec>>(
                    $"Error retrieving specs: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> InsertSpecAsync(Model_DunnageSpec spec)
        {
            try
            {
                await _logger.LogInfoAsync(
                    $"Inserting spec '{spec.SpecKey}' for type ID {spec.TypeId} by user: {CurrentUser}"
                );
                var result = await _daoDunnageSpec.InsertAsync(
                    spec.TypeId,
                    spec.SpecKey,
                    spec.SpecValue,
                    CurrentUser
                );
                if (result.IsSuccess)
                {
                    spec.Id = result.Data;
                    await _logger.LogInfoAsync(
                        $"Successfully inserted spec '{spec.SpecKey}' with ID: {spec.Id}"
                    );
                    return Model_Dao_Result_Factory.Success();
                }
                await _logger.LogErrorAsync(
                    $"Failed to insert spec '{spec.SpecKey}' for type ID {spec.TypeId}: {result.ErrorMessage}"
                );
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in InsertSpecAsync for spec '{spec.SpecKey}': {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(InsertSpecAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure($"Error inserting spec: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec)
        {
            try
            {
                await _logger.LogInfoAsync(
                    $"Updating spec ID {spec.Id}: {spec.SpecKey} = {spec.SpecValue} by user: {CurrentUser}"
                );
                var result = await _daoDunnageSpec.UpdateAsync(
                    spec.Id,
                    spec.SpecValue,
                    CurrentUser
                );
                if (result.IsSuccess)
                {
                    await _logger.LogInfoAsync(
                        $"Successfully updated spec ID {spec.Id}: {spec.SpecKey}"
                    );
                }
                else
                {
                    await _logger.LogErrorAsync(
                        $"Failed to update spec ID {spec.Id}: {result.ErrorMessage}"
                    );
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in UpdateSpecAsync for spec ID {spec.Id}: {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(UpdateSpecAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure($"Error updating spec: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> DeleteSpecAsync(int specId)
        {
            try
            {
                return await _daoDunnageSpec.DeleteByIdAsync(specId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(DeleteSpecAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure($"Error deleting spec: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> DeleteSpecsByTypeIdAsync(int typeId)
        {
            // Not implemented in DAO yet, but typically handled by cascade delete in DB
            return Model_Dao_Result_Factory.Success();
        }

        public async Task<List<string>> GetAllSpecKeysAsync()
        {
            try
            {
                var result = await _daoDunnageSpec.GetAllAsync();
                if (result.IsSuccess && result.Data != null)
                {
                    return result.Data.Select(s => s.SpecKey).Distinct().Order().ToList();
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetAllSpecKeysAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return new List<string>();
            }
        }

        // ==================== Part Operations ====================

        public async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllPartsAsync()
        {
            try
            {
                return await _daoDunnagePart.GetAllAsync();
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetAllPartsAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnagePart>>(
                    $"Error retrieving parts: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId)
        {
            _logger.LogInfo(
                $"Service_MySQL_Dunnage: GetPartsByTypeAsync called for typeId={typeId}",
                "DunnageService"
            );
            try
            {
                var result = await _daoDunnagePart.GetByTypeAsync(typeId);
                _logger.LogInfo(
                    $"Service_MySQL_Dunnage: GetPartsByTypeAsync returned {result.Data?.Count ?? 0} parts",
                    "DunnageService"
                );
                return result;
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetPartsByTypeAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnagePart>>(
                    $"Error retrieving parts by type: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<Model_DunnagePart>> GetPartByIdAsync(string partId)
        {
            try
            {
                return await _daoDunnagePart.GetByIdAsync(partId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetPartByIdAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<Model_DunnagePart>(
                    $"Error retrieving part: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> InsertPartAsync(Model_DunnagePart part)
        {
            try
            {
                await _logger.LogInfoAsync(
                    $"Inserting new dunnage part: {part.PartId} (Type ID: {part.TypeId}, Home Location: {part.HomeLocation}) by user: {CurrentUser}"
                );
                var persistedImagePathResult = await PersistPartImagePathAsync(part);
                if (!persistedImagePathResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure(persistedImagePathResult.ErrorMessage);
                }

                var persistedImagePath = persistedImagePathResult.Data;
                part.SpecValues = BuildSpecValuesWithImagePath(part.SpecValues, persistedImagePath);
                var result = await _daoDunnagePart.InsertAsync(
                    part.PartId,
                    part.TypeId,
                    part.SpecValues,
                    persistedImagePath,
                    part.QuantityType,
                    part.HomeLocation,
                    CurrentUser
                );
                if (result.IsSuccess)
                {
                    part.Id = result.Data;
                    await _logger.LogInfoAsync(
                        $"Successfully inserted dunnage part '{part.PartId}' with ID: {part.Id}"
                    );
                    return Model_Dao_Result_Factory.Success();
                }
                await _logger.LogErrorAsync(
                    $"Failed to insert dunnage part '{part.PartId}': {result.ErrorMessage}"
                );
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in InsertPartAsync for part '{part.PartId}': {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(InsertPartAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure($"Error inserting part: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> InsertPartWithInventoryAsync(
            Model_DunnagePart part,
            string inventoryMethod,
            string inventoryNotes = ""
        )
        {
            try
            {
                var existingParts = await _daoDunnagePart.GetAllAsync();
                if (
                    existingParts.IsSuccess
                    && existingParts.Data?.Any(existingPart =>
                        existingPart.PartId.Equals(part.PartId, StringComparison.OrdinalIgnoreCase)
                    ) == true
                )
                {
                    return Model_Dao_Result_Factory.Failure(
                        $"A dunnage part with Part ID '{part.PartId}' already exists."
                    );
                }

                var persistedImagePathResult = await PersistPartImagePathAsync(part);
                if (!persistedImagePathResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure(persistedImagePathResult.ErrorMessage);
                }

                var persistedImagePath = persistedImagePathResult.Data;
                part.SpecValues = BuildSpecValuesWithImagePath(part.SpecValues, persistedImagePath);
                var result = await _daoDunnagePart.InsertWithInventoryAsync(
                    part.PartId,
                    part.TypeId,
                    part.SpecValues,
                    persistedImagePath,
                    part.QuantityType,
                    part.HomeLocation,
                    inventoryMethod,
                    inventoryNotes,
                    CurrentUser
                );

                if (result.IsSuccess)
                {
                    part.Id = result.Data;
                    return Model_Dao_Result_Factory.Success();
                }

                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in InsertPartWithInventoryAsync for part '{part.PartId}': {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(InsertPartWithInventoryAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error inserting part with inventory: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part)
        {
            try
            {
                await _logger.LogInfoAsync(
                    $"Updating dunnage part ID {part.Id} (Part ID: {part.PartId}, Home Location: {part.HomeLocation}) by user: {CurrentUser}"
                );
                var existingPartResult = await _daoDunnagePart.GetByIdAsync(part.PartId);
                var previousImagePath =
                    existingPartResult.IsSuccess && existingPartResult.Data is not null
                        ? existingPartResult.Data.ImagePath
                        : null;
                var persistedImagePathResult = await PersistPartImagePathAsync(part);
                if (!persistedImagePathResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure(persistedImagePathResult.ErrorMessage);
                }

                var persistedImagePath = persistedImagePathResult.Data;
                part.SpecValues = BuildSpecValuesWithImagePath(part.SpecValues, persistedImagePath);
                var result = await _daoDunnagePart.UpdateAsync(
                    part.Id,
                    part.PartId,
                    part.SpecValues,
                    persistedImagePath,
                    part.QuantityType,
                    part.HomeLocation,
                    CurrentUser
                );
                if (result.IsSuccess)
                {
                    await CleanupReplacedImageAsync(previousImagePath, part.ImagePath);
                    await _logger.LogInfoAsync($"Successfully updated dunnage part ID {part.Id}");
                }
                else
                {
                    await _logger.LogErrorAsync(
                        $"Failed to update dunnage part ID {part.Id}: {result.ErrorMessage}"
                    );
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in UpdatePartAsync for part ID {part.Id}: {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(UpdatePartAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure($"Error updating part: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> UpdatePartWithInventoryAndReferencesAsync(
            Model_DunnagePart part,
            string originalPartId,
            string inventoryMethod,
            string inventoryNotes = ""
        )
        {
            try
            {
                var existingParts = await _daoDunnagePart.GetAllAsync();
                if (
                    existingParts.IsSuccess
                    && existingParts.Data?.Any(existingPart =>
                        existingPart.Id != part.Id
                        && existingPart.PartId.Equals(
                            part.PartId,
                            StringComparison.OrdinalIgnoreCase
                        )
                    ) == true
                )
                {
                    return Model_Dao_Result_Factory.Failure(
                        $"A dunnage part with Part ID '{part.PartId}' already exists."
                    );
                }

                var existingPartResult = await _daoDunnagePart.GetByIdAsync(originalPartId);
                var previousImagePath =
                    existingPartResult.IsSuccess && existingPartResult.Data is not null
                        ? existingPartResult.Data.ImagePath
                        : null;

                var persistedImagePathResult = await PersistPartImagePathAsync(part);
                if (!persistedImagePathResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure(persistedImagePathResult.ErrorMessage);
                }

                var persistedImagePath = persistedImagePathResult.Data;
                part.SpecValues = BuildSpecValuesWithImagePath(part.SpecValues, persistedImagePath);

                var updateResult = await _daoDunnagePart.UpdateWithInventoryAndReferencesAsync(
                    part.Id,
                    originalPartId,
                    part.PartId,
                    part.SpecValues,
                    persistedImagePath,
                    part.QuantityType,
                    part.HomeLocation,
                    inventoryMethod,
                    inventoryNotes,
                    CurrentUser
                );

                if (updateResult.IsSuccess)
                {
                    await CleanupReplacedImageAsync(previousImagePath, part.ImagePath);
                }

                return updateResult;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in UpdatePartWithInventoryAndReferencesAsync for part '{part.PartId}': {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(UpdatePartWithInventoryAndReferencesAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error updating part with linked references: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<List<Model_DunnageQuantityType>>> GetQuantityTypesAsync()
        {
            try
            {
                return await _daoDunnageQuantityType.GetAllAsync();
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetQuantityTypesAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageQuantityType>>(
                    $"Error retrieving quantity types: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> SaveQuantityTypeIfMissingAsync(string quantityType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(quantityType))
                {
                    return Model_Dao_Result_Factory.Failure("Quantity type cannot be empty.");
                }

                return await _daoDunnageQuantityType.InsertIfMissingAsync(
                    quantityType.Trim(),
                    CurrentUser
                );
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(SaveQuantityTypeIfMissingAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error saving quantity type: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> DeletePartAsync(string partId)
        {
            try
            {
                var existingPartResult = await _daoDunnagePart.GetByIdAsync(partId);
                if (!existingPartResult.IsSuccess || existingPartResult.Data is null)
                {
                    return Model_Dao_Result_Factory.Failure(
                        existingPartResult.ErrorMessage ?? $"Part '{partId}' was not found."
                    );
                }

                var transactionCountResult = await _daoDunnagePart.CountTransactionsAsync(partId);
                if (transactionCountResult.IsSuccess && transactionCountResult.Data > 0)
                {
                    await _logger.LogWarningAsync(
                        $"Cannot delete dunnage part '{partId}': used by {transactionCountResult.Data} history record(s)"
                    );

                    return Model_Dao_Result_Factory.Failure(
                        BuildDeletePartBlockedMessage(partId, transactionCountResult.Data)
                    );
                }

                var deleteResult = await _daoDunnagePart.DeleteAsync(existingPartResult.Data.Id);
                if (deleteResult.IsSuccess)
                {
                    await _imageStorage.DeleteImageAsync(existingPartResult.Data.ImagePath);
                }

                return deleteResult;
            }
            catch (MySqlException ex) when (IsPartDeleteConstraintFailure(ex))
            {
                await _logger.LogWarningAsync(
                    $"DeletePartAsync blocked by foreign key constraint for part '{partId}': {ex.Message}"
                );

                return Model_Dao_Result_Factory.Failure(
                    BuildDeletePartBlockedMessage(partId, null)
                );
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(DeletePartAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure($"Error deleting part: {ex.Message}");
            }
        }

        private static bool IsPartDeleteConstraintFailure(MySqlException ex)
        {
            return ex.Message.Contains(
                    "FK_dunnage_history_part_id",
                    StringComparison.OrdinalIgnoreCase
                )
                || ex.Message.Contains(
                    "Cannot delete or update a parent row",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        private static string BuildDeletePartBlockedMessage(string partId, int? historyRecordCount)
        {
            var countText = historyRecordCount switch
            {
                1 => "1 Dunnage history record",
                > 1 => $"{historyRecordCount.Value} Dunnage history records",
                _ => "existing Dunnage history",
            };

            return $"Part '{partId}' can't be deleted because it is referenced by {countText}. Remove or reassign those history records first.";
        }

        public async Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(
            string searchText,
            int? typeId
        )
        {
            try
            {
                Model_Dao_Result<List<Model_DunnagePart>> result;
                if (typeId.HasValue)
                {
                    result = await _daoDunnagePart.GetByTypeAsync(typeId.Value);
                }
                else
                {
                    result = await _daoDunnagePart.GetAllAsync();
                }

                if (!result.IsSuccess || result.Data == null)
                {
                    return result;
                }

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return result;
                }

                var filtered = result
                    .Data.Where(p =>
                        p.PartId.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                        || p.DunnageTypeName.Contains(
                            searchText,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    .ToList();

                return Model_Dao_Result_Factory.Success<List<Model_DunnagePart>>(filtered);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(SearchPartsAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnagePart>>(
                    $"Error searching parts: {ex.Message}"
                );
            }
        }

        // ==================== Load Operations ====================

        public async Task<Model_Dao_Result> SaveLoadsAsync(List<Model_DunnageLoad> loads)
        {
            try
            {
                if (loads == null || loads.Count == 0)
                {
                    await _logger.LogInfoAsync("SaveLoadsAsync called with no loads to save");
                    return Model_Dao_Result_Factory.Success();
                }

                await _logger.LogInfoAsync(
                    $"Saving batch of {loads.Count} dunnage loads to active queue by user: {CurrentUser}"
                );
                var result = await _daoDunnageLabelData.InsertBatchAsync(loads, CurrentUser);
                if (result.IsSuccess)
                {
                    var totalQuantity = loads.Sum(l => l.Quantity);
                    await _logger.LogInfoAsync(
                        $"Successfully queued {result.Data} of {loads.Count} dunnage loads (Total Quantity: {totalQuantity})"
                    );
                }
                else
                {
                    await _logger.LogErrorAsync(
                        $"Failed to queue dunnage loads: {result.ErrorMessage}"
                    );
                }

                return result.IsSuccess
                    ? Model_Dao_Result_Factory.Success()
                    : Model_Dao_Result_Factory.Failure(
                        result.ErrorMessage ?? "Failed to save loads"
                    );
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in SaveLoadsAsync for {loads?.Count ?? 0} loads: {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(SaveLoadsAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure($"Error saving loads: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns all rows currently in the <c>dunnage_label_data</c> active queue.
        /// Used by Edit Mode to display labels that have been saved but not yet archived.
        /// </summary>
        public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetActiveLabelDataAsync()
        {
            try
            {
                return await _daoDunnageLabelData.GetActiveLabelDataAsync();
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetActiveLabelDataAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageLoad>>(
                    $"Error retrieving active label data: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Atomically moves all rows from <c>dunnage_label_data</c> to <c>dunnage_history</c> and
        /// clears the active queue. Returns the number of rows moved.
        /// </summary>
        public async Task<Model_Dao_Result<int>> ClearLabelDataAsync()
        {
            try
            {
                await _logger.LogInfoAsync(
                    $"Clearing dunnage label data to history by user: {CurrentUser}"
                );
                var result = await _daoDunnageLabelData.ClearToHistoryAsync(CurrentUser);
                if (result.IsSuccess)
                {
                    await _logger.LogInfoAsync(
                        $"Successfully archived {result.Data} dunnage label row(s) to history"
                    );
                }
                else
                {
                    await _logger.LogErrorAsync(
                        $"Failed to clear dunnage label data: {result.ErrorMessage}"
                    );
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Exception in ClearLabelDataAsync: {ex.Message}");
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(ClearLabelDataAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<int>(
                    $"Error clearing label data: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(
            DateTime start,
            DateTime end
        )
        {
            try
            {
                return await _daoDunnageLoad.GetByDateRangeAsync(start, end);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetLoadsByDateRangeAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageLoad>>(
                    $"Error retrieving loads: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllLoadsAsync()
        {
            try
            {
                return await _daoDunnageLoad.GetAllAsync();
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetAllLoadsAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageLoad>>(
                    $"Error retrieving all loads: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<Model_DunnageLoad>> GetLoadByIdAsync(string loadUuid)
        {
            try
            {
                if (Guid.TryParse(loadUuid, out var guid))
                {
                    return await _daoDunnageLoad.GetByIdAsync(guid);
                }
                return Model_Dao_Result_Factory.Failure<Model_DunnageLoad>("Invalid UUID format");
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetLoadByIdAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<Model_DunnageLoad>(
                    $"Error retrieving load: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load)
        {
            try
            {
                await _logger.LogInfoAsync(
                    $"Updating dunnage history load {load.LoadUuid} (Part: {load.PartId}, Quantity: {load.Quantity}) by user: {CurrentUser}"
                );
                return await _daoDunnageLoad.UpdateAsync(load, CurrentUser);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(UpdateLoadAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure($"Error updating load: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> UpdateHistoryLoadsAsync(List<Model_DunnageLoad> loads)
        {
            try
            {
                if (loads == null || loads.Count == 0)
                {
                    await _logger.LogInfoAsync(
                        "UpdateHistoryLoadsAsync called with no loads to update"
                    );
                    return Model_Dao_Result_Factory.Success();
                }

                await _logger.LogInfoAsync(
                    $"Updating {loads.Count} dunnage history row(s) by user: {CurrentUser}"
                );

                foreach (var load in loads)
                {
                    var result = await _daoDunnageLoad.UpdateAsync(load, CurrentUser);
                    if (!result.IsSuccess)
                    {
                        await _logger.LogErrorAsync(
                            $"Failed to update dunnage history load {load.LoadUuid}: {result.ErrorMessage}"
                        );
                        return result;
                    }
                }

                await _logger.LogInfoAsync(
                    $"Successfully updated {loads.Count} dunnage history row(s)"
                );
                return Model_Dao_Result_Factory.Success();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in UpdateHistoryLoadsAsync for {loads?.Count ?? 0} loads: {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(UpdateHistoryLoadsAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error updating history loads: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> UpdateActiveLabelLoadsAsync(
            List<Model_DunnageLoad> loads
        )
        {
            try
            {
                if (loads == null || loads.Count == 0)
                {
                    await _logger.LogInfoAsync(
                        "UpdateActiveLabelLoadsAsync called with no loads to update"
                    );
                    return Model_Dao_Result_Factory.Success();
                }

                await _logger.LogInfoAsync(
                    $"Updating {loads.Count} active dunnage label row(s) by user: {CurrentUser}"
                );

                foreach (var load in loads)
                {
                    var result = await _daoDunnageLabelData.UpdateAsync(load, CurrentUser);
                    if (!result.IsSuccess)
                    {
                        await _logger.LogErrorAsync(
                            $"Failed to update active dunnage label row {load.LoadUuid}: {result.ErrorMessage}"
                        );
                        return result;
                    }
                }

                await _logger.LogInfoAsync(
                    $"Successfully updated {loads.Count} active dunnage label row(s)"
                );
                return Model_Dao_Result_Factory.Success();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in UpdateActiveLabelLoadsAsync for {loads?.Count ?? 0} loads: {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(UpdateActiveLabelLoadsAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error updating active label loads: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> DeleteLoadAsync(string loadUuid)
        {
            try
            {
                if (!Guid.TryParse(loadUuid, out var guid))
                {
                    return Model_Dao_Result_Factory.Failure("Invalid load UUID format.");
                }

                await _logger.LogInfoAsync(
                    $"Deleting dunnage load {loadUuid} by user: {CurrentUser}"
                );
                return await _daoDunnageLoad.DeleteAsync(guid);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(DeleteLoadAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure($"Error deleting load: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> DeleteActiveLabelLoadAsync(string loadUuid)
        {
            try
            {
                if (!Guid.TryParse(loadUuid, out var guid))
                {
                    return Model_Dao_Result_Factory.Failure("Invalid load UUID format.");
                }

                await _logger.LogInfoAsync(
                    $"Deleting active dunnage label row {loadUuid} by user: {CurrentUser}"
                );
                return await _daoDunnageLabelData.DeleteAsync(guid);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(DeleteActiveLabelLoadAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error deleting active label load: {ex.Message}"
                );
            }
        }

        // ==================== Inventory Operations ====================

        public async Task<bool> IsPartInventoriedAsync(string partId)
        {
            try
            {
                var result = await _daoInventoriedDunnage.CheckAsync(partId);
                return result.IsSuccess && result.Data;
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Warning,
                    nameof(IsPartInventoriedAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return false;
            }
        }

        public async Task<Model_Dao_Result<Model_InventoriedDunnage>> GetInventoryDetailsAsync(
            string partId
        )
        {
            try
            {
                return await _daoInventoriedDunnage.GetByPartAsync(partId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetInventoryDetailsAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<Model_InventoriedDunnage>(
                    $"Error retrieving inventory details: {ex.Message}"
                );
            }
        }

        public async Task<
            Model_Dao_Result<List<Model_InventoriedDunnage>>
        > GetAllInventoriedPartsAsync()
        {
            try
            {
                return await _daoInventoriedDunnage.GetAllAsync();
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetAllInventoriedPartsAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_InventoriedDunnage>>(
                    $"Error retrieving inventoried parts: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Alias for GetAllInventoriedPartsAsync for ViewModel compatibility
        /// </summary>
        public async Task<
            Model_Dao_Result<List<Model_InventoriedDunnage>>
        > GetInventoriedPartsAsync()
        {
            return await GetAllInventoriedPartsAsync();
        }

        public async Task<Model_Dao_Result> AddToInventoriedListAsync(Model_InventoriedDunnage item)
        {
            try
            {
                await _logger.LogInfoAsync(
                    $"Adding part '{item.PartId}' to inventoried list (Method: {item.InventoryMethod}) by user: {CurrentUser}"
                );
                var result = await _daoInventoriedDunnage.InsertAsync(
                    item.PartId,
                    item.InventoryMethod ?? string.Empty,
                    item.Notes ?? string.Empty,
                    CurrentUser
                );
                if (result.IsSuccess)
                {
                    item.Id = result.Data;
                    await _logger.LogInfoAsync(
                        $"Successfully added part '{item.PartId}' to inventoried list with ID: {item.Id}"
                    );
                    return Model_Dao_Result_Factory.Success();
                }
                await _logger.LogErrorAsync(
                    $"Failed to add part '{item.PartId}' to inventoried list: {result.ErrorMessage}"
                );
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(
                    $"Exception in AddToInventoriedListAsync for part '{item.PartId}': {ex.Message}"
                );
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(AddToInventoriedListAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error adding to inventory list: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> RemoveFromInventoriedListAsync(string partId)
        {
            var existingEntry = await _daoInventoriedDunnage.GetByPartAsync(partId);
            if (!existingEntry.IsSuccess || existingEntry.Data is null)
            {
                return Model_Dao_Result_Factory.Success();
            }

            return await _daoInventoriedDunnage.DeleteAsync(existingEntry.Data.Id);
        }

        public async Task<Model_Dao_Result> UpdateInventoriedPartAsync(
            Model_InventoriedDunnage item
        )
        {
            try
            {
                return await _daoInventoriedDunnage.UpdateAsync(
                    item.Id,
                    item.PartId,
                    item.InventoryMethod ?? string.Empty,
                    item.Notes ?? string.Empty,
                    CurrentUser
                );
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(UpdateInventoriedPartAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error updating inventory item: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Update an inventoried part with specific values
        /// </summary>
        /// <param name="id"></param>
        /// <param name="partId"></param>
        /// <param name="inventoryMethod"></param>
        /// <param name="notes"></param>
        /// <param name="username"></param>
        public async Task<Model_Dao_Result> UpdateInventoriedPartAsync(
            int id,
            string partId,
            string inventoryMethod,
            string notes,
            string username
        )
        {
            var part = new Model_InventoriedDunnage
            {
                Id = id,
                PartId = partId,
                InventoryMethod = inventoryMethod,
                Notes = notes,
                ModifiedBy = username,
                ModifiedDate = DateTime.Now,
            };
            return await UpdateInventoriedPartAsync(part);
        }

        /// <summary>
        /// Delete an inventoried part by ID
        /// </summary>
        /// <param name="id"></param>
        public async Task<Model_Dao_Result> DeleteInventoriedPartAsync(int id)
        {
            return await _daoInventoriedDunnage.DeleteAsync(id);
        }

        // ==================== Impact Analysis ====================

        public async Task<Model_Dao_Result<int>> GetPartCountByTypeIdAsync(int typeId)
        {
            try
            {
                return await _daoDunnageType.CountPartsAsync(typeId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Warning,
                    nameof(GetPartCountByTypeIdAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<int>($"Error counting parts: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result<int>> GetTransactionCountByPartIdAsync(string partId)
        {
            try
            {
                return await _daoDunnagePart.CountTransactionsAsync(partId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Warning,
                    nameof(GetTransactionCountByPartIdAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<int>(
                    $"Error counting transactions: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<int>> GetTransactionCountByTypeIdAsync(int typeId)
        {
            try
            {
                return await _daoDunnageType.CountTransactionsAsync(typeId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Warning,
                    nameof(GetTransactionCountByTypeIdAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<int>(
                    $"Error counting transactions: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<int>> GetPartCountBySpecKeyAsync(
            int typeId,
            string specKey
        )
        {
            try
            {
                return await _daoDunnageSpec.CountPartsUsingSpecAsync(typeId, specKey);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Warning,
                    nameof(GetPartCountBySpecKeyAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<int>(
                    $"Error counting parts using spec: {ex.Message}"
                );
            }
        }

        // ==================== Custom Field Operations ====================

        public async Task<Model_Dao_Result> InsertCustomFieldAsync(
            int typeId,
            Model_CustomFieldDefinition field
        )
        {
            try
            {
                var result = await _daoCustomField.InsertAsync(typeId, field, CurrentUser);
                if (result.IsSuccess)
                {
                    field.Id = result.Data;
                    return Model_Dao_Result_Factory.Success();
                }
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(InsertCustomFieldAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error inserting custom field: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> UpdateCustomFieldAsync(
            int fieldId,
            Model_CustomFieldDefinition field
        )
        {
            try
            {
                return await _daoCustomField.UpdateAsync(fieldId, field);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(UpdateCustomFieldAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error updating custom field: {ex.Message}"
                );
            }
        }

        public async Task<
            Model_Dao_Result<List<Model_CustomFieldDefinition>>
        > GetCustomFieldsByTypeAsync(int typeId)
        {
            try
            {
                return await _daoCustomField.GetByTypeAsync(typeId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetCustomFieldsByTypeAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_CustomFieldDefinition>>(
                    $"Error retrieving custom fields: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> DeleteCustomFieldAsync(int fieldId)
        {
            try
            {
                return await _daoCustomField.DeleteAsync(fieldId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(DeleteCustomFieldAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error deleting custom field: {ex.Message}"
                );
            }
        }

        // ==================== User Preference Operations ====================

        public async Task<Model_Dao_Result> UpsertUserPreferenceAsync(string key, string value)
        {
            try
            {
                return await _daoUserPreference.UpsertAsync(CurrentUser, key, value);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(UpsertUserPreferenceAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error saving user preference: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<List<Model_IconDefinition>>> GetRecentlyUsedIconsAsync(
            int count
        )
        {
            try
            {
                return await _daoUserPreference.GetRecentlyUsedIconsAsync(CurrentUser, count);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetRecentlyUsedIconsAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_IconDefinition>>(
                    $"Error retrieving recent icons: {ex.Message}"
                );
            }
        }

        // ==================== Non-PO Entry Operations ====================

        public async Task<Model_Dao_Result<List<Model_DunnageNonPOEntry>>> GetNonPOEntriesAsync()
        {
            try
            {
                return await _daoNonPOEntry.GetAllAsync();
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetNonPOEntriesAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageNonPOEntry>>(
                    $"Error retrieving non-PO entries: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> SaveNonPOEntryAsync(string value, string createdBy)
        {
            try
            {
                return await _daoNonPOEntry.UpsertAsync(value, createdBy);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(SaveNonPOEntryAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure($"Error saving non-PO entry: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> DeleteNonPOEntryAsync(int id)
        {
            try
            {
                return await _daoNonPOEntry.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(DeleteNonPOEntryAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error deleting non-PO entry: {ex.Message}"
                );
            }
        }

        private void HandleException(
            Exception ex,
            Enum_ErrorSeverity severity,
            string method,
            string className
        )
        {
            _ = _errorHandler.HandleErrorAsync(
                $"Error in {method} ({className}): {ex.Message}",
                severity,
                ex
            );
        }

        private async Task<Model_Dao_Result<string?>> PersistPartImagePathAsync(
            Model_DunnagePart part
        )
        {
            var imagePathResult = await PreparePartImagePathAsync(part.ImagePath, part);
            if (!imagePathResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<string?>(
                    imagePathResult.ErrorMessage,
                    imagePathResult.Exception
                );
            }

            part.ImagePath = imagePathResult.Data;
            return Model_Dao_Result_Factory.Success<string?>(part.ImagePath);
        }

        private async Task<Model_Dao_Result<string?>> PreparePartImagePathAsync(
            string? imagePath,
            Model_DunnagePart part
        )
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return Model_Dao_Result_Factory.Success<string?>(null);
            }

            if (!Path.IsPathRooted(imagePath))
            {
                return Model_Dao_Result_Factory.Success<string?>(imagePath.Replace('\\', '/'));
            }

            var importResult = await _imageStorage.ImportPartImageAsync(
                imagePath,
                part.DunnageTypeName,
                part.PartId
            );
            if (!importResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<string?>(
                    importResult.ErrorMessage,
                    importResult.Exception
                );
            }

            return Model_Dao_Result_Factory.Success<string?>(importResult.Data);
        }

        private async Task<Model_Dao_Result<string?>> PrepareTypeImagePathAsync(
            string? imagePath,
            string typeName
        )
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return Model_Dao_Result_Factory.Success<string?>(null);
            }

            if (!Path.IsPathRooted(imagePath))
            {
                return Model_Dao_Result_Factory.Success<string?>(imagePath.Replace('\\', '/'));
            }

            var importResult = await _imageStorage.ImportTypeImageAsync(imagePath, typeName);
            if (!importResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<string?>(
                    importResult.ErrorMessage,
                    importResult.Exception
                );
            }

            return Model_Dao_Result_Factory.Success<string?>(importResult.Data);
        }

        private string BuildSpecValuesWithImagePath(string? specValuesJson, string? imagePath)
        {
            Dictionary<string, JsonElement> specValues;

            try
            {
                specValues =
                    JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                        string.IsNullOrWhiteSpace(specValuesJson) ? "{}" : specValuesJson
                    ) ?? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }
            catch (JsonException)
            {
                specValues = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
            }

            var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in specValues)
            {
                if (string.Equals(pair.Key, "image_path", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                payload[pair.Key] = pair.Value.Clone();
            }

            var normalizedImagePath = _imageStorage.GetNormalizedFullPath(imagePath);
            if (string.IsNullOrWhiteSpace(normalizedImagePath) is false)
            {
                payload["image_path"] = normalizedImagePath;
            }

            return payload.Count == 0 ? "{}" : JsonSerializer.Serialize(payload);
        }

        private async Task CleanupReplacedImageAsync(
            string? previousImagePath,
            string? currentImagePath
        )
        {
            if (string.IsNullOrWhiteSpace(previousImagePath))
            {
                return;
            }

            if (
                string.Equals(
                    previousImagePath,
                    currentImagePath,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return;
            }

            await _imageStorage.DeleteImageAsync(previousImagePath);
        }
    }
}

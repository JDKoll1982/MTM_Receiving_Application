using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
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

                var existingTypeResult = await _daoDunnageType.GetByIdAsync(typeId);

                // Clean up part image files too. sp_Dunnage_Types_Delete removes the
                // parts and every label-data / history / inventory row that
                // references the type or its parts.
                var partsResult = await _daoDunnagePart.GetByTypeAsync(typeId);
                if (partsResult.IsSuccess && partsResult.Data is not null)
                {
                    foreach (var part in partsResult.Data)
                    {
                        if (!string.IsNullOrWhiteSpace(part.ImagePath))
                        {
                            await _imageStorage.DeleteImageAsync(part.ImagePath);
                        }
                    }
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
                var result = await _daoDunnagePart.InsertAsync(
                    part.PartId,
                    part.TypeId,
                    part.Udc1,
                    part.Udc2,
                    part.Udc3,
                    part.Udc4,
                    part.Udc5,
                    part.Udc6,
                    part.Udc7,
                    part.Udc8,
                    part.Udc9,
                    part.Udc10,
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
                        string.Equals(
                            existingPart.PartId,
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

                var persistedImagePathResult = await PersistPartImagePathAsync(part);
                if (!persistedImagePathResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure(persistedImagePathResult.ErrorMessage);
                }

                var persistedImagePath = persistedImagePathResult.Data;
                var result = await _daoDunnagePart.InsertWithInventoryAsync(
                    part.PartId,
                    part.TypeId,
                    part.Udc1,
                    part.Udc2,
                    part.Udc3,
                    part.Udc4,
                    part.Udc5,
                    part.Udc6,
                    part.Udc7,
                    part.Udc8,
                    part.Udc9,
                    part.Udc10,
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
                var result = await _daoDunnagePart.UpdateAsync(
                    part.Id,
                    part.PartId,
                    part.Udc1,
                    part.Udc2,
                    part.Udc3,
                    part.Udc4,
                    part.Udc5,
                    part.Udc6,
                    part.Udc7,
                    part.Udc8,
                    part.Udc9,
                    part.Udc10,
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
                        && string.Equals(
                            existingPart.PartId,
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

                var updateResult = await _daoDunnagePart.UpdateWithInventoryAndReferencesAsync(
                    part.Id,
                    originalPartId,
                    part.PartId,
                    part.Udc1,
                    part.Udc2,
                    part.Udc3,
                    part.Udc4,
                    part.Udc5,
                    part.Udc6,
                    part.Udc7,
                    part.Udc8,
                    part.Udc9,
                    part.Udc10,
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

        /// <summary>
        /// Re-associates a part with a new dunnage type. Source spec fields that the
        /// target type does not define are added to the target type (as optional) so
        /// the part keeps its specs; values map by field name into the new type's
        /// slots. Required target fields without a mapped value must be supplied via
        /// <paramref name="providedValues"/> or a default, otherwise the transfer is
        /// rejected. Also rewrites the type snapshot and udc layout on the part's
        /// label-data and history rows.
        /// </summary>
        public async Task<Model_Dao_Result<string>> ChangePartTypeAsync(
            Model_DunnagePart part,
            int newTypeId,
            IReadOnlyDictionary<string, string?> providedValues
        )
        {
            try
            {
                if (part is null)
                {
                    return Model_Dao_Result_Factory.Failure<string>(
                        "No part was selected to change type."
                    );
                }

                if (part.TypeId == newTypeId)
                {
                    return Model_Dao_Result_Factory.Failure<string>(
                        "The part already belongs to that dunnage type."
                    );
                }

                var targetTypeResult = await _daoDunnageType.GetByIdAsync(newTypeId);
                if (!targetTypeResult.IsSuccess || targetTypeResult.Data is null)
                {
                    return Model_Dao_Result_Factory.Failure<string>(
                        targetTypeResult.ErrorMessage ?? "Target dunnage type not found."
                    );
                }

                var targetType = targetTypeResult.Data;

                var sourceFieldsResult = await _daoCustomField.GetByTypeAsync(part.TypeId);
                var targetFieldsResult = await _daoCustomField.GetByTypeAsync(newTypeId);
                if (!sourceFieldsResult.IsSuccess || !targetFieldsResult.IsSuccess)
                {
                    return Model_Dao_Result_Factory.Failure<string>(
                        "Unable to load the type specifications."
                    );
                }

                var sourceFields =
                    (sourceFieldsResult.Data ?? new List<Model_CustomFieldDefinition>())
                        .OrderBy(field => field.DisplayOrder)
                        .ToList();
                var targetFields =
                    (targetFieldsResult.Data ?? new List<Model_CustomFieldDefinition>())
                        .OrderBy(field => field.DisplayOrder)
                        .ToList();

                await HydrateCustomFieldChoicesAsync(sourceFields);
                await HydrateCustomFieldChoicesAsync(targetFields);

                var sourceValues = Helper_Dunnage_PartSpecs.ExtractUdc(part);
                var targetNames = new HashSet<string>(
                    targetFields.Select(field => field.FieldName),
                    StringComparer.OrdinalIgnoreCase
                );

                // Source spec fields the target type does not already define.
                var missingSourceFields = sourceFields
                    .Where(field => targetNames.Contains(field.FieldName) is false)
                    .ToList();

                var usedSlots = new HashSet<int>(
                    targetFields
                        .Where(field => field.DisplayOrder is >= 1 and <= 10)
                        .Select(field => field.DisplayOrder)
                );
                var freeSlots = Enumerable
                    .Range(1, Helper_Dunnage_PartSpecs.MaxUdcCount)
                    .Where(slot => usedSlots.Contains(slot) is false)
                    .ToList();

                if (missingSourceFields.Count > freeSlots.Count)
                {
                    return Model_Dao_Result_Factory.Failure<string>(
                        $"'{targetType.TypeName}' has no open spec slots for the {missingSourceFields.Count} spec field(s) this part uses. The part can't be transferred."
                    );
                }

                // Add missing source fields to the target type as OPTIONAL so existing
                // parts of the target type are not invalidated by a new requirement.
                var addedFields = new List<Model_CustomFieldDefinition>();
                for (var index = 0; index < missingSourceFields.Count; index++)
                {
                    var sourceField = missingSourceFields[index];
                    var copy = new Model_CustomFieldDefinition
                    {
                        DunnageTypeId = newTypeId,
                        FieldName = sourceField.FieldName,
                        FieldType = sourceField.FieldType,
                        DisplayOrder = freeSlots[index],
                        IsRequired = false,
                        Unit = sourceField.Unit,
                        MinValue = sourceField.MinValue,
                        MaxValue = sourceField.MaxValue,
                        DefaultValue = sourceField.DefaultValue,
                        ValidationRules = sourceField.ValidationRules,
                        Choices = sourceField.Choices?.ToList() ?? new List<string>(),
                    };

                    var insertResult = await _daoCustomField.InsertAsync(
                        newTypeId,
                        copy,
                        CurrentUser
                    );
                    if (!insertResult.IsSuccess)
                    {
                        return Model_Dao_Result_Factory.Failure<string>(
                            insertResult.ErrorMessage
                                ?? "Failed to add a spec field to the target type."
                        );
                    }

                    copy.Id = insertResult.Data;
                    await InsertFieldChoicesAsync(copy.Id, copy.Choices);
                    addedFields.Add(copy);
                }

                var finalTargetFields = targetFields.Concat(addedFields).ToList();

                // Map the part's existing values by field NAME into the new type's slots.
                var finalValues = new string?[Helper_Dunnage_PartSpecs.MaxUdcCount];
                foreach (var sourceField in sourceFields)
                {
                    var value = Helper_Dunnage_PartSpecs.GetValueForSlot(
                        sourceValues,
                        sourceField.DisplayOrder
                    );
                    var match = finalTargetFields.FirstOrDefault(field =>
                        string.Equals(
                            field.FieldName,
                            sourceField.FieldName,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );
                    if (match is not null && string.IsNullOrWhiteSpace(value) is false)
                    {
                        finalValues[match.DisplayOrder - 1] = value;
                    }
                }

                // Required target fields without a mapped value: user-provided or default.
                foreach (var targetField in finalTargetFields.OrderBy(field => field.DisplayOrder))
                {
                    var slotIndex = targetField.DisplayOrder - 1;
                    if (string.IsNullOrWhiteSpace(finalValues[slotIndex]) is false)
                    {
                        continue;
                    }

                    var provided = providedValues.TryGetValue(
                        targetField.FieldName,
                        out var rawValue
                    )
                        ? rawValue?.Trim()
                        : null;
                    if (string.IsNullOrWhiteSpace(provided) is false)
                    {
                        finalValues[slotIndex] = provided;
                    }
                    else if (targetField.IsRequired)
                    {
                        if (string.IsNullOrWhiteSpace(targetField.DefaultValue) is false)
                        {
                            finalValues[slotIndex] = targetField.DefaultValue.Trim();
                        }
                        else
                        {
                            return Model_Dao_Result_Factory.Failure<string>(
                                $"Required field '{targetField.FieldName}' needs a value before the part can be transferred."
                            );
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(targetField.DefaultValue) is false)
                    {
                        finalValues[slotIndex] = targetField.DefaultValue.Trim();
                    }
                }

                var changeResult = await _daoDunnagePart.ChangeTypeAsync(
                    part.PartId,
                    newTypeId,
                    finalValues[0],
                    finalValues[1],
                    finalValues[2],
                    finalValues[3],
                    finalValues[4],
                    finalValues[5],
                    finalValues[6],
                    finalValues[7],
                    finalValues[8],
                    finalValues[9],
                    CurrentUser
                );

                if (changeResult.IsSuccess)
                {
                    await _logger.LogInfoAsync(
                        $"Changed dunnage part '{part.PartId}' to type '{targetType.TypeName}' (ID {newTypeId}) by user: {CurrentUser}"
                    );
                    return Model_Dao_Result_Factory.Success<string>(targetType.TypeName);
                }

                return Model_Dao_Result_Factory.Failure<string>(changeResult.ErrorMessage);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(ChangePartTypeAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<string>(
                    $"Error changing part type: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Loads the choice list for each Choices custom field (definitions returned by
        /// GetByType do not hydrate them).
        /// </summary>
        private async Task HydrateCustomFieldChoicesAsync(
            List<Model_CustomFieldDefinition> fields
        )
        {
            foreach (var field in fields)
            {
                if (
                    string.Equals(field.FieldType, "Choices", StringComparison.OrdinalIgnoreCase)
                    is false
                    || field.Choices.Count > 0
                )
                {
                    continue;
                }

                var choicesResult = await _daoCustomField.GetChoicesByFieldAsync(field.Id);
                if (choicesResult.IsSuccess && choicesResult.Data is not null)
                {
                    field.Choices = choicesResult.Data
                        .OrderBy(choice => choice.SortOrder)
                        .Select(choice => choice.Choice)
                        .ToList();
                }
            }
        }

        private async Task InsertFieldChoicesAsync(int fieldId, List<string> choices)
        {
            for (var index = 0; index < choices.Count; index++)
            {
                await _daoCustomField.InsertChoiceAsync(fieldId, choices[index], index + 1);
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

                // sp_Dunnage_Parts_Delete cascades the part's label-data queue rows,
                // archived history rows, inventory rows, and non-PO defaults before
                // deleting the part itself.
                var deleteResult = await _daoDunnagePart.DeleteAsync(existingPartResult.Data.Id);
                if (deleteResult.IsSuccess)
                {
                    await _imageStorage.DeleteImageAsync(existingPartResult.Data.ImagePath);
                }

                return deleteResult;
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

        public async Task<bool> HasActiveLabelDataAsync()
        {
            try
            {
                var result = await _daoDunnageLabelData.GetActiveLabelDataAsync();
                if (!result.IsSuccess)
                {
                    await _logger.LogErrorAsync(
                        $"Failed to check dunnage label data availability: {result.ErrorMessage}"
                    );
                    return false;
                }

                return (result.Data?.Count ?? 0) > 0;
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(HasActiveLabelDataAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return false;
            }
        }

        /// <summary>
        /// Atomically moves matching rows from <c>dunnage_label_data</c> to <c>dunnage_history</c>
        /// and clears them from the active queue. Returns the number of rows moved.
        /// </summary>
        /// <param name="archivedBy"></param>
        /// <param name="employeeNumber"></param>
        /// <param name="clearAllRows"></param>
        public async Task<Model_Dao_Result<int>> ClearLabelDataAsync(
            string archivedBy,
            int employeeNumber,
            bool clearAllRows
        )
        {
            try
            {
                await _logger.LogInfoAsync(
                    clearAllRows
                        ? $"Clearing all dunnage label data to history by user: {archivedBy}"
                        : $"Clearing dunnage label data to history for employee {employeeNumber} by user: {archivedBy}"
                );
                var result = await _daoDunnageLabelData.ClearToHistoryAsync(
                    archivedBy,
                    employeeNumber,
                    clearAllRows
                );
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

        /// <summary>
        /// Loads dunnage history rows for the Reprint Labels page, including whether each row is
        /// already queued for reprint.
        /// </summary>
        public async Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> GetReprintHistoryAsync(
            Model_ReprintHistoryFilter filter
        )
        {
            try
            {
                return await _daoDunnageLabelData.GetReprintHistoryAsync(filter);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetReprintHistoryAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_ReprintHistoryRow>>(
                    $"Error retrieving dunnage history for reprint: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Copies a single row from <c>dunnage_history</c> back into the active label queue so it
        /// can be re-printed. Sets <c>is_reprint = 1</c>.
        /// </summary>
        public async Task<Model_Dao_Result<int>> InsertFromHistoryAsync(string loadUuid)
        {
            try
            {
                var employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;
                _logger.LogInfo($"Queuing dunnage history record {loadUuid} for reprint by {CurrentUser}");
                return await _daoDunnageLabelData.InsertFromHistoryAsync(
                    loadUuid,
                    CurrentUser,
                    employeeNumber
                );
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(InsertFromHistoryAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<int>(
                    $"Error queuing dunnage history record {loadUuid} for reprint: {ex.Message}"
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

        public async Task<Model_Dao_Result<Model_DunnagePartDeleteImpact>>
            GetPartDeleteImpactAsync(string partId)
        {
            try
            {
                return await _daoDunnagePart.GetDeleteImpactAsync(partId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Warning,
                    nameof(GetPartDeleteImpactAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<Model_DunnagePartDeleteImpact>(
                    $"Error loading part delete impact: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<Model_DunnageTypeDeleteImpact>>
            GetTypeDeleteImpactAsync(int typeId)
        {
            try
            {
                return await _daoDunnageType.GetDeleteImpactAsync(typeId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Warning,
                    nameof(GetTypeDeleteImpactAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<Model_DunnageTypeDeleteImpact>(
                    $"Error loading type delete impact: {ex.Message}"
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

        public async Task<Model_Dao_Result> InsertCustomFieldChoiceAsync(
            int customFieldId,
            string choice,
            int sortOrder
        )
        {
            try
            {
                return await _daoCustomField.InsertChoiceAsync(customFieldId, choice, sortOrder);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(InsertCustomFieldChoiceAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error inserting custom field choice: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> DeleteCustomFieldChoicesAsync(int customFieldId)
        {
            try
            {
                return await _daoCustomField.DeleteChoicesByFieldAsync(customFieldId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(DeleteCustomFieldChoicesAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error deleting custom field choices: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result<List<Model_DunnageCustomFieldChoice>>>
            GetCustomFieldChoicesAsync(int customFieldId)
        {
            try
            {
                return await _daoCustomField.GetChoicesByFieldAsync(customFieldId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetCustomFieldChoicesAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageCustomFieldChoice>>(
                    $"Error retrieving custom field choices: {ex.Message}"
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

        public async Task<Model_Dao_Result<string?>> GetNonPOPartDefaultAsync(string partId)
        {
            try
            {
                return await _daoNonPOEntry.GetPartDefaultAsync(partId);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(GetNonPOPartDefaultAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure<string?>(
                    $"Error retrieving dunnage non-PO part default: {ex.Message}"
                );
            }
        }

        public async Task<Model_Dao_Result> SaveNonPOPartDefaultAsync(
            string partId,
            string value,
            string updatedBy
        )
        {
            try
            {
                return await _daoNonPOEntry.UpsertPartDefaultAsync(partId, value, updatedBy);
            }
            catch (Exception ex)
            {
                HandleException(
                    ex,
                    Enum_ErrorSeverity.Error,
                    nameof(SaveNonPOPartDefaultAsync),
                    nameof(Service_MySQL_Dunnage)
                );
                return Model_Dao_Result_Factory.Failure(
                    $"Error saving dunnage non-PO part default: {ex.Message}"
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

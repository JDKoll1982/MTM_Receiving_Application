using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Dunnage.Services
{
    public class Service_MySQL_Dunnage : IService_MySQL_Dunnage
    {
        private readonly IService_ErrorHandler _errorHandler;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly Dao_DunnageLoad _daoDunnageLoad;
        private readonly Dao_DunnageType _daoDunnageType;
        private readonly Dao_DunnagePart _daoDunnagePart;
        private readonly Dao_DunnageSpec _daoDunnageSpec;
        private readonly Dao_InventoriedDunnage _daoInventoriedDunnage;
        private readonly Dao_DunnageCustomField _daoCustomField;
        private readonly Dao_DunnageUserPreference _daoUserPreference;

        private string CurrentUser => _sessionManager.CurrentSession?.User?.WindowsUsername ?? "System";

        public Service_MySQL_Dunnage(
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_UserSessionManager sessionManager,
            Dao_DunnageLoad daoDunnageLoad,
            Dao_DunnageType daoDunnageType,
            Dao_DunnagePart daoDunnagePart,
            Dao_DunnageSpec daoDunnageSpec,
            Dao_InventoriedDunnage daoInventoriedDunnage,
            Dao_DunnageCustomField daoCustomField,
            Dao_DunnageUserPreference daoUserPreference)
        {
            _errorHandler = errorHandler;
            _logger = logger;
            _sessionManager = sessionManager;
            _daoDunnageLoad = daoDunnageLoad;
            _daoDunnageType = daoDunnageType;
            _daoDunnagePart = daoDunnagePart;
            _daoDunnageSpec = daoDunnageSpec;
            _daoInventoriedDunnage = daoInventoriedDunnage;
            _daoCustomField = daoCustomField;
            _daoUserPreference = daoUserPreference;
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetAllTypesAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageType>>($"Error retrieving dunnage types: {ex.Message}");
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetTypeByIdAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<Model_DunnageType>($"Error retrieving dunnage type: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result<int>> InsertTypeAsync(string typeName, string icon)
        {
            try
            {
                return await _daoDunnageType.InsertAsync(typeName, icon, CurrentUser);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(InsertTypeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<int>($"Error inserting dunnage type: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type)
        {
            try
            {
                await _logger.LogInfoAsync($"Inserting new dunnage type: {type.TypeName} (Icon: {type.Icon}) by user: {CurrentUser}");
                var result = await _daoDunnageType.InsertAsync(type.TypeName, type.Icon, CurrentUser);
                if (result.IsSuccess)
                {
                    type.Id = result.Data;
                    await _logger.LogInfoAsync($"Successfully inserted dunnage type '{type.TypeName}' with ID: {type.Id}");
                    return Model_Dao_Result_Factory.Success();
                }

                if (result.ErrorMessage.Contains("Duplicate entry"))
                {
                    await _logger.LogWarningAsync($"Failed to insert dunnage type '{type.TypeName}': Duplicate entry");
                    return Model_Dao_Result_Factory.Failure($"The dunnage type name '{type.TypeName}' is already in use.");
                }

                await _logger.LogErrorAsync($"Failed to insert dunnage type '{type.TypeName}': {result.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Exception in InsertTypeAsync for type '{type.TypeName}': {ex.Message}");
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(InsertTypeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error inserting dunnage type: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type)
        {
            try
            {
                await _logger.LogInfoAsync($"Updating dunnage type ID {type.Id}: {type.TypeName} (Icon: {type.Icon}) by user: {CurrentUser}");
                var result = await _daoDunnageType.UpdateAsync(type.Id, type.TypeName, type.Icon, CurrentUser);

                if (!result.IsSuccess && result.ErrorMessage.Contains("Duplicate entry"))
                {
                    await _logger.LogWarningAsync($"Failed to update dunnage type ID {type.Id}: Duplicate entry for '{type.TypeName}'");
                    return Model_Dao_Result_Factory.Failure($"The dunnage type name '{type.TypeName}' is already in use.");
                }

                if (result.IsSuccess)
                {
                    await _logger.LogInfoAsync($"Successfully updated dunnage type ID {type.Id}: {type.TypeName}");
                }
                else
                {
                    await _logger.LogErrorAsync($"Failed to update dunnage type ID {type.Id}: {result.ErrorMessage}");
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Exception in UpdateTypeAsync for type ID {type.Id}: {ex.Message}");
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(UpdateTypeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error updating dunnage type: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> DeleteTypeAsync(int typeId)
        {
            try
            {
                await _logger.LogInfoAsync($"Attempting to delete dunnage type ID {typeId} by user: {CurrentUser}");

                // Check if any parts are using this type
                var partsResult = await _daoDunnagePart.GetByTypeAsync(typeId);
                if (partsResult.IsSuccess && partsResult.Data?.Count > 0)
                {
                    await _logger.LogWarningAsync($"Cannot delete dunnage type ID {typeId}: Used by {partsResult.Data.Count} parts");
                    return Model_Dao_Result_Factory.Failure($"Cannot delete type. It is used by {partsResult.Data.Count} parts.");
                }

                // Check if any specs are defined for this type
                var specsResult = await _daoDunnageSpec.GetByTypeAsync(typeId);
                if (specsResult.IsSuccess && specsResult.Data?.Count > 0)
                {
                    await _logger.LogWarningAsync($"Cannot delete dunnage type ID {typeId}: Has {specsResult.Data.Count} specifications defined");
                    return Model_Dao_Result_Factory.Failure($"Cannot delete type. It has {specsResult.Data.Count} specifications defined. Please delete them first.");
                }

                var result = await _daoDunnageType.DeleteAsync(typeId);
                if (result.IsSuccess)
                {
                    await _logger.LogInfoAsync($"Successfully deleted dunnage type ID {typeId}");
                }
                else
                {
                    await _logger.LogErrorAsync($"Failed to delete dunnage type ID {typeId}: {result.ErrorMessage}");
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Exception in DeleteTypeAsync for type ID {typeId}: {ex.Message}");
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(DeleteTypeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error deleting dunnage type: {ex.Message}");
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(CheckDuplicateTypeNameAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<int>($"Error checking duplicate type name: {ex.Message}");
            }
        }

        // ==================== Spec Operations ====================

        public async Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetSpecsForTypeAsync(int typeId)
        {
            try
            {
                return await _daoDunnageSpec.GetByTypeAsync(typeId);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetSpecsForTypeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageSpec>>($"Error retrieving specs: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> InsertSpecAsync(Model_DunnageSpec spec)
        {
            try
            {
                await _logger.LogInfoAsync($"Inserting spec '{spec.SpecKey}' for type ID {spec.TypeId} by user: {CurrentUser}");
                var result = await _daoDunnageSpec.InsertAsync(spec.TypeId, spec.SpecKey, spec.SpecValue, CurrentUser);
                if (result.IsSuccess)
                {
                    spec.Id = result.Data;
                    await _logger.LogInfoAsync($"Successfully inserted spec '{spec.SpecKey}' with ID: {spec.Id}");
                    return Model_Dao_Result_Factory.Success();
                }
                await _logger.LogErrorAsync($"Failed to insert spec '{spec.SpecKey}' for type ID {spec.TypeId}: {result.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Exception in InsertSpecAsync for spec '{spec.SpecKey}': {ex.Message}");
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(InsertSpecAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error inserting spec: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec)
        {
            try
            {
                await _logger.LogInfoAsync($"Updating spec ID {spec.Id}: {spec.SpecKey} = {spec.SpecValue} by user: {CurrentUser}");
                var result = await _daoDunnageSpec.UpdateAsync(spec.Id, spec.SpecValue, CurrentUser);
                if (result.IsSuccess)
                {
                    await _logger.LogInfoAsync($"Successfully updated spec ID {spec.Id}: {spec.SpecKey}");
                }
                else
                {
                    await _logger.LogErrorAsync($"Failed to update spec ID {spec.Id}: {result.ErrorMessage}");
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Exception in UpdateSpecAsync for spec ID {spec.Id}: {ex.Message}");
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(UpdateSpecAsync), nameof(Service_MySQL_Dunnage));
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(DeleteSpecAsync), nameof(Service_MySQL_Dunnage));
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
                    return result.Data
                        .Select(s => s.SpecKey)
                        .Distinct()
                        .Order()
                        .ToList();
                }

                return new List<string>();
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetAllSpecKeysAsync), nameof(Service_MySQL_Dunnage));
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetAllPartsAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<List<Model_DunnagePart>>($"Error retrieving parts: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId)
        {
            _logger.LogInfo($"Service_MySQL_Dunnage: GetPartsByTypeAsync called for typeId={typeId}", "DunnageService");
            try
            {
                var result = await _daoDunnagePart.GetByTypeAsync(typeId);
                _logger.LogInfo($"Service_MySQL_Dunnage: GetPartsByTypeAsync returned {result.Data?.Count ?? 0} parts", "DunnageService");
                return result;
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetPartsByTypeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<List<Model_DunnagePart>>($"Error retrieving parts by type: {ex.Message}");
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetPartByIdAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<Model_DunnagePart>($"Error retrieving part: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> InsertPartAsync(Model_DunnagePart part)
        {
            try
            {
                await _logger.LogInfoAsync($"Inserting new dunnage part: {part.PartId} (Type ID: {part.TypeId}, Home Location: {part.HomeLocation}) by user: {CurrentUser}");
                var result = await _daoDunnagePart.InsertAsync(part.PartId, part.TypeId, part.SpecValues, part.HomeLocation, CurrentUser);
                if (result.IsSuccess)
                {
                    part.Id = result.Data;
                    await _logger.LogInfoAsync($"Successfully inserted dunnage part '{part.PartId}' with ID: {part.Id}");
                    return Model_Dao_Result_Factory.Success();
                }
                await _logger.LogErrorAsync($"Failed to insert dunnage part '{part.PartId}': {result.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Exception in InsertPartAsync for part '{part.PartId}': {ex.Message}");
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(InsertPartAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error inserting part: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part)
        {
            try
            {
                await _logger.LogInfoAsync($"Updating dunnage part ID {part.Id} (Part ID: {part.PartId}, Home Location: {part.HomeLocation}) by user: {CurrentUser}");
                var result = await _daoDunnagePart.UpdateAsync(part.Id, part.SpecValues, part.HomeLocation, CurrentUser);
                if (result.IsSuccess)
                {
                    await _logger.LogInfoAsync($"Successfully updated dunnage part ID {part.Id}");
                }
                else
                {
                    await _logger.LogErrorAsync($"Failed to update dunnage part ID {part.Id}: {result.ErrorMessage}");
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Exception in UpdatePartAsync for part ID {part.Id}: {ex.Message}");
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(UpdatePartAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error updating part: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> DeletePartAsync(string partId)
        {
            // Dao_DunnagePart does not have DeleteAsync yet.
            return Model_Dao_Result_Factory.Failure("Delete part not implemented in DAO yet.");
        }

        public async Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchText, int? typeId)
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

                var filtered = result.Data.Where(p =>
                    p.PartId.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    p.DunnageTypeName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                return Model_Dao_Result_Factory.Success<List<Model_DunnagePart>>(filtered);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(SearchPartsAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<List<Model_DunnagePart>>($"Error searching parts: {ex.Message}");
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

                await _logger.LogInfoAsync($"Saving batch of {loads.Count} dunnage loads by user: {CurrentUser}");
                var result = await _daoDunnageLoad.InsertBatchAsync(loads, CurrentUser);
                if (result.IsSuccess)
                {
                    var totalQuantity = loads.Sum(l => l.Quantity);
                    await _logger.LogInfoAsync($"Successfully saved {loads.Count} dunnage loads (Total Quantity: {totalQuantity})");
                }
                else
                {
                    await _logger.LogErrorAsync($"Failed to save {loads.Count} dunnage loads: {result.ErrorMessage}");
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Exception in SaveLoadsAsync for {loads?.Count ?? 0} loads: {ex.Message}");
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(SaveLoadsAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error saving loads: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(DateTime start, DateTime end)
        {
            try
            {
                return await _daoDunnageLoad.GetByDateRangeAsync(start, end);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetLoadsByDateRangeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageLoad>>($"Error retrieving loads: {ex.Message}");
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetAllLoadsAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<List<Model_DunnageLoad>>($"Error retrieving all loads: {ex.Message}");
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetLoadByIdAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<Model_DunnageLoad>($"Error retrieving load: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load)
        {
            return Model_Dao_Result_Factory.Failure("Update load not implemented in DAO yet.");
        }

        public async Task<Model_Dao_Result> DeleteLoadAsync(string loadUuid)
        {
            return Model_Dao_Result_Factory.Failure("Delete load not implemented in DAO yet.");
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
                HandleException(ex, Enum_ErrorSeverity.Warning, nameof(IsPartInventoriedAsync), nameof(Service_MySQL_Dunnage));
                return false;
            }
        }

        public async Task<Model_Dao_Result<Model_InventoriedDunnage>> GetInventoryDetailsAsync(string partId)
        {
            try
            {
                return await _daoInventoriedDunnage.GetByPartAsync(partId);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetInventoryDetailsAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<Model_InventoriedDunnage>($"Error retrieving inventory details: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllInventoriedPartsAsync()
        {
            try
            {
                return await _daoInventoriedDunnage.GetAllAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetAllInventoriedPartsAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<List<Model_InventoriedDunnage>>($"Error retrieving inventoried parts: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> AddToInventoriedListAsync(Model_InventoriedDunnage item)
        {
            try
            {
                await _logger.LogInfoAsync($"Adding part '{item.PartId}' to inventoried list (Method: {item.InventoryMethod}) by user: {CurrentUser}");
                var result = await _daoInventoriedDunnage.InsertAsync(item.PartId, item.InventoryMethod ?? string.Empty, item.Notes ?? string.Empty, CurrentUser);
                if (result.IsSuccess)
                {
                    item.Id = result.Data;
                    await _logger.LogInfoAsync($"Successfully added part '{item.PartId}' to inventoried list with ID: {item.Id}");
                    return Model_Dao_Result_Factory.Success();
                }
                await _logger.LogErrorAsync($"Failed to add part '{item.PartId}' to inventoried list: {result.ErrorMessage}");
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Exception in AddToInventoriedListAsync for part '{item.PartId}': {ex.Message}");
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(AddToInventoriedListAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error adding to inventory list: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> RemoveFromInventoriedListAsync(string partId)
        {
            return Model_Dao_Result_Factory.Failure("Remove from inventory list not implemented in DAO yet.");
        }

        public async Task<Model_Dao_Result> UpdateInventoriedPartAsync(Model_InventoriedDunnage item)
        {
            try
            {
                return await _daoInventoriedDunnage.UpdateAsync(item.Id, item.InventoryMethod ?? string.Empty, item.Notes ?? string.Empty, CurrentUser);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(UpdateInventoriedPartAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error updating inventory item: {ex.Message}");
            }
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
                HandleException(ex, Enum_ErrorSeverity.Warning, nameof(GetPartCountByTypeIdAsync), nameof(Service_MySQL_Dunnage));
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
                HandleException(ex, Enum_ErrorSeverity.Warning, nameof(GetTransactionCountByPartIdAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<int>($"Error counting transactions: {ex.Message}");
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
                HandleException(ex, Enum_ErrorSeverity.Warning, nameof(GetTransactionCountByTypeIdAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<int>($"Error counting transactions: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result<int>> GetPartCountBySpecKeyAsync(int typeId, string specKey)
        {
            try
            {
                return await _daoDunnageSpec.CountPartsUsingSpecAsync(typeId, specKey);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Warning, nameof(GetPartCountBySpecKeyAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<int>($"Error counting parts using spec: {ex.Message}");
            }
        }

        // ==================== Custom Field Operations ====================

        public async Task<Model_Dao_Result> InsertCustomFieldAsync(int typeId, Model_CustomFieldDefinition field)
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(InsertCustomFieldAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error inserting custom field: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetCustomFieldsByTypeAsync(int typeId)
        {
            try
            {
                return await _daoCustomField.GetByTypeAsync(typeId);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetCustomFieldsByTypeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<List<Model_CustomFieldDefinition>>($"Error retrieving custom fields: {ex.Message}");
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(DeleteCustomFieldAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error deleting custom field: {ex.Message}");
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
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(UpsertUserPreferenceAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error saving user preference: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result<List<Model_IconDefinition>>> GetRecentlyUsedIconsAsync(int count)
        {
            try
            {
                return await _daoUserPreference.GetRecentlyUsedIconsAsync(CurrentUser, count);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(GetRecentlyUsedIconsAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure<List<Model_IconDefinition>>($"Error retrieving recent icons: {ex.Message}");
            }
        }

        private void HandleException(Exception ex, Enum_ErrorSeverity severity, string method, string className)
        {
            _ = _errorHandler.HandleErrorAsync($"Error in {method} ({className}): {ex.Message}", severity, ex);
        }
    }
}





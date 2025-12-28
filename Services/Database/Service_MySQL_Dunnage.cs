using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Data.Dunnage;
using MTM_Receiving_Application.Models.Dunnage;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.Services.Database
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

        private string CurrentUser => _sessionManager.CurrentSession?.User.WindowsUsername ?? "System";

        public Service_MySQL_Dunnage(
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger,
            IService_UserSessionManager sessionManager,
            Dao_DunnageLoad daoDunnageLoad,
            Dao_DunnageType daoDunnageType,
            Dao_DunnagePart daoDunnagePart,
            Dao_DunnageSpec daoDunnageSpec,
            Dao_InventoriedDunnage daoInventoriedDunnage)
        {
            _errorHandler = errorHandler;
            _logger = logger;
            _sessionManager = sessionManager;
            _daoDunnageLoad = daoDunnageLoad;
            _daoDunnageType = daoDunnageType;
            _daoDunnagePart = daoDunnagePart;
            _daoDunnageSpec = daoDunnageSpec;
            _daoInventoriedDunnage = daoInventoriedDunnage;
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

        public async Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type)
        {
            try
            {
                var result = await _daoDunnageType.InsertAsync(type.TypeName, CurrentUser);
                if (result.IsSuccess)
                {
                    type.Id = result.Data;
                    return Model_Dao_Result_Factory.Success();
                }
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(InsertTypeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error inserting dunnage type: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type)
        {
            try
            {
                return await _daoDunnageType.UpdateAsync(type.Id, type.TypeName, CurrentUser);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(UpdateTypeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error updating dunnage type: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> DeleteTypeAsync(int typeId)
        {
            try
            {
                // Check if any parts are using this type
                var partsResult = await _daoDunnagePart.GetByTypeAsync(typeId);
                if (partsResult.IsSuccess && partsResult.Data?.Count > 0)
                {
                    return Model_Dao_Result_Factory.Failure($"Cannot delete type. It is used by {partsResult.Data.Count} parts.");
                }

                // Check if any specs are defined for this type
                var specsResult = await _daoDunnageSpec.GetByTypeAsync(typeId);
                if (specsResult.IsSuccess && specsResult.Data?.Count > 0)
                {
                    return Model_Dao_Result_Factory.Failure($"Cannot delete type. It has {specsResult.Data.Count} specifications defined. Please delete them first.");
                }

                return await _daoDunnageType.DeleteAsync(typeId);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(DeleteTypeAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error deleting dunnage type: {ex.Message}");
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
                var result = await _daoDunnageSpec.InsertAsync(spec.TypeId, spec.SpecKey, spec.SpecValue, CurrentUser);
                if (result.IsSuccess)
                {
                    spec.Id = result.Data;
                    return Model_Dao_Result_Factory.Success();
                }
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(InsertSpecAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error inserting spec: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec)
        {
            try
            {
                return await _daoDunnageSpec.UpdateAsync(spec.Id, spec.SpecValue, CurrentUser);
            }
            catch (Exception ex)
            {
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
            try
            {
                return await _daoDunnageSpec.DeleteByTypeAsync(typeId);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(DeleteSpecsByTypeIdAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error deleting specs for type: {ex.Message}");
            }
        }

        public async Task<List<string>> GetAllSpecKeysAsync()
        {
            try
            {
                var result = await _daoDunnageSpec.GetAllAsync();
                if (result.IsSuccess && result.Data != null)
                {
                    return [.. result.Data.Select(s => s.SpecKey).Distinct().Order()];
                }
                return new List<string>();
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Warning, nameof(GetAllSpecKeysAsync), nameof(Service_MySQL_Dunnage));
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
            try
            {
                return await _daoDunnagePart.GetByTypeAsync(typeId);
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
                var result = await _daoDunnagePart.InsertAsync(part.PartId, part.TypeId, part.SpecValues, CurrentUser);
                if (result.IsSuccess)
                {
                    part.Id = result.Data;
                    return Model_Dao_Result_Factory.Success();
                }
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Error, nameof(InsertPartAsync), nameof(Service_MySQL_Dunnage));
                return Model_Dao_Result_Factory.Failure($"Error inserting part: {ex.Message}");
            }
        }

        public async Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part)
        {
            try
            {
                return await _daoDunnagePart.UpdateAsync(part.Id, part.SpecValues, CurrentUser);
            }
            catch (Exception ex)
            {
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
                    return Model_Dao_Result_Factory.Success();
                }

                return await _daoDunnageLoad.InsertBatchAsync(loads, CurrentUser);
            }
            catch (Exception ex)
            {
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
                var result = await _daoInventoriedDunnage.InsertAsync(item.PartId, item.InventoryMethod ?? string.Empty, item.Notes ?? string.Empty, CurrentUser);
                if (result.IsSuccess)
                {
                    item.Id = result.Data;
                    return Model_Dao_Result_Factory.Success();
                }
                return Model_Dao_Result_Factory.Failure(result.ErrorMessage);
            }
            catch (Exception ex)
            {
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

        public async Task<int> GetPartCountByTypeIdAsync(int typeId)
        {
            try
            {
                var result = await _daoDunnagePart.GetByTypeAsync(typeId);
                return result.IsSuccess ? (result.Data?.Count ?? 0) : 0;
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Warning, nameof(GetPartCountByTypeIdAsync), nameof(Service_MySQL_Dunnage));
                return 0;
            }
        }

        public async Task<int> GetTransactionCountByPartIdAsync(string partId)
        {
            // Not implemented in DAO.
            return 0;
        }

        public async Task<int> GetTransactionCountByTypeIdAsync(int typeId)
        {
            // Not implemented in DAO.
            return 0;
        }

        public async Task<int> GetPartCountBySpecKeyAsync(int typeId, string specKey)
        {
            try
            {
                var result = await _daoDunnageSpec.CountPartsUsingSpecAsync(typeId, specKey);
                return result.IsSuccess ? result.Data : 0;
            }
            catch (Exception ex)
            {
                HandleException(ex, Enum_ErrorSeverity.Warning, nameof(GetPartCountBySpecKeyAsync), nameof(Service_MySQL_Dunnage));
                return 0;
            }
        }

        private void HandleException(Exception ex, Enum_ErrorSeverity severity, string method, string className)
        {
            _ = _errorHandler.HandleErrorAsync($"Error in {method} ({className}): {ex.Message}", severity, ex);
        }
    }
}




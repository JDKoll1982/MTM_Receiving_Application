using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.DunnageModule.Models;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for dunnage database operations.
    /// Wraps DAOs with business logic validation and error handling.
    /// </summary>
    public interface IService_MySQL_Dunnage
    {
        // ==================== Type Operations (7 methods) ====================

        public Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync();
        public Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId);
        public Task<Model_Dao_Result<int>> InsertTypeAsync(string typeName, string icon);
        public Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type);
        public Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type);
        public Task<Model_Dao_Result> DeleteTypeAsync(int typeId);
        public Task<Model_Dao_Result<int>> CheckDuplicateTypeNameAsync(string typeName);

        // ==================== Spec Operations (6 methods) ====================

        public Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetSpecsForTypeAsync(int typeId);
        public Task<Model_Dao_Result> InsertSpecAsync(Model_DunnageSpec spec);
        public Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec);
        public Task<Model_Dao_Result> DeleteSpecAsync(int specId);
        public Task<Model_Dao_Result> DeleteSpecsByTypeIdAsync(int typeId);
        public Task<List<string>> GetAllSpecKeysAsync();

        // ==================== Part Operations (7 methods) ====================

        public Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllPartsAsync();
        public Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId);
        public Task<Model_Dao_Result<Model_DunnagePart>> GetPartByIdAsync(string partId);
        public Task<Model_Dao_Result> InsertPartAsync(Model_DunnagePart part);
        public Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part);
        public Task<Model_Dao_Result> DeletePartAsync(string partId);
        public Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchText, int? typeId = null);

        // ==================== Load Operations (6 methods) ====================

        public Task<Model_Dao_Result> SaveLoadsAsync(List<Model_DunnageLoad> loads);
        public Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(DateTime start, DateTime end);
        public Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllLoadsAsync();
        public Task<Model_Dao_Result<Model_DunnageLoad>> GetLoadByIdAsync(string loadUuid);
        public Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load);
        public Task<Model_Dao_Result> DeleteLoadAsync(string loadUuid);

        // ==================== Inventory Operations (6 methods) ====================

        public Task<bool> IsPartInventoriedAsync(string partId);
        public Task<Model_Dao_Result<Model_InventoriedDunnage>> GetInventoryDetailsAsync(string partId);
        public Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllInventoriedPartsAsync();
        public Task<Model_Dao_Result> AddToInventoriedListAsync(Model_InventoriedDunnage item);
        public Task<Model_Dao_Result> RemoveFromInventoriedListAsync(string partId);
        public Task<Model_Dao_Result> UpdateInventoriedPartAsync(Model_InventoriedDunnage item);

        // ==================== Impact Analysis (4 methods) ====================

        public Task<Model_Dao_Result<int>> GetPartCountByTypeIdAsync(int typeId);
        public Task<Model_Dao_Result<int>> GetTransactionCountByPartIdAsync(string partId);
        public Task<Model_Dao_Result<int>> GetTransactionCountByTypeIdAsync(int typeId);
        public Task<Model_Dao_Result<int>> GetPartCountBySpecKeyAsync(int typeId, string specKey);

        // Aliases for compatibility (spec 010-dunnage-complete)
        public Task<Model_Dao_Result<int>> GetPartCountByTypeAsync(int typeId) => GetPartCountByTypeIdAsync(typeId);
        public Task<Model_Dao_Result<int>> GetTransactionCountByTypeAsync(int typeId) => GetTransactionCountByTypeIdAsync(typeId);
        public Task<Model_Dao_Result<int>> GetTransactionCountByPartAsync(string partId) => GetTransactionCountByPartIdAsync(partId);

        // ==================== Custom Field Operations (3 methods) ====================

        public Task<Model_Dao_Result> InsertCustomFieldAsync(int typeId, Model_CustomFieldDefinition field);
        public Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetCustomFieldsByTypeAsync(int typeId);
        public Task<Model_Dao_Result> DeleteCustomFieldAsync(int fieldId);

        // ==================== User Preference Operations (2 methods) ====================

        public Task<Model_Dao_Result> UpsertUserPreferenceAsync(string key, string value);
        public Task<Model_Dao_Result<List<Model_IconDefinition>>> GetRecentlyUsedIconsAsync(int count);
    }
}

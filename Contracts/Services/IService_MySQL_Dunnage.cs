using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Contracts.Services
{
    /// <summary>
    /// Service for dunnage database operations.
    /// Wraps DAOs with business logic validation and error handling.
    /// </summary>
    public interface IService_MySQL_Dunnage
    {
        // ==================== Type Operations (5 methods) ====================

        public Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync();
        public Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId);
        public Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type);
        public Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type);
        public Task<Model_Dao_Result> DeleteTypeAsync(int typeId);

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
        public Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchText, int? typeId);

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

        public Task<int> GetPartCountByTypeIdAsync(int typeId);
        public Task<int> GetTransactionCountByPartIdAsync(string partId);
        public Task<int> GetTransactionCountByTypeIdAsync(int typeId);
        public Task<int> GetPartCountBySpecKeyAsync(int typeId, string specKey);
    }
}

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

        Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync();
        Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int typeId);
        Task<Model_Dao_Result> InsertTypeAsync(Model_DunnageType type);
        Task<Model_Dao_Result> UpdateTypeAsync(Model_DunnageType type);
        Task<Model_Dao_Result> DeleteTypeAsync(int typeId);

        // ==================== Spec Operations (6 methods) ====================

        Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetSpecsForTypeAsync(int typeId);
        Task<Model_Dao_Result> InsertSpecAsync(Model_DunnageSpec spec);
        Task<Model_Dao_Result> UpdateSpecAsync(Model_DunnageSpec spec);
        Task<Model_Dao_Result> DeleteSpecAsync(int specId);
        Task<Model_Dao_Result> DeleteSpecsByTypeIdAsync(int typeId);
        Task<List<string>> GetAllSpecKeysAsync();

        // ==================== Part Operations (7 methods) ====================

        Task<Model_Dao_Result<List<Model_DunnagePart>>> GetAllPartsAsync();
        Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId);
        Task<Model_Dao_Result<Model_DunnagePart>> GetPartByIdAsync(string partId);
        Task<Model_Dao_Result> InsertPartAsync(Model_DunnagePart part);
        Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part);
        Task<Model_Dao_Result> DeletePartAsync(string partId);
        Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchText, int? typeId);

        // ==================== Load Operations (6 methods) ====================

        Task<Model_Dao_Result> SaveLoadsAsync(List<Model_DunnageLoad> loads);
        Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(DateTime start, DateTime end);
        Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetAllLoadsAsync();
        Task<Model_Dao_Result<Model_DunnageLoad>> GetLoadByIdAsync(string loadUuid);
        Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load);
        Task<Model_Dao_Result> DeleteLoadAsync(string loadUuid);

        // ==================== Inventory Operations (6 methods) ====================

        Task<bool> IsPartInventoriedAsync(string partId);
        Task<Model_Dao_Result<Model_InventoriedDunnage>> GetInventoryDetailsAsync(string partId);
        Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetAllInventoriedPartsAsync();
        Task<Model_Dao_Result> AddToInventoriedListAsync(Model_InventoriedDunnage item);
        Task<Model_Dao_Result> RemoveFromInventoriedListAsync(string partId);
        Task<Model_Dao_Result> UpdateInventoriedPartAsync(Model_InventoriedDunnage item);

        // ==================== Impact Analysis (4 methods) ====================

        Task<int> GetPartCountByTypeIdAsync(int typeId);
        Task<int> GetTransactionCountByPartIdAsync(string partId);
        Task<int> GetTransactionCountByTypeIdAsync(int typeId);
        Task<int> GetPartCountBySpecKeyAsync(int typeId, string specKey);
    }
}

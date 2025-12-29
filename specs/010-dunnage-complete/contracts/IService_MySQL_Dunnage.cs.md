using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Dunnage;

namespace MTM_Receiving_Application.Contracts.Services;

/// <summary>
/// Service interface for MySQL CRUD operations on dunnage entities
/// EXTENDED for Admin UI support (types/parts/specs/inventory management)
/// </summary>
public interface IService_MySQL_Dunnage
{
    // ============================================
    // EXISTING METHODS (from spec 006-dunnage-services)
    // ============================================
    
    /// <summary>
    /// Get all dunnage types
    /// </summary>
    Task<Model_Dao_Result<List<Model_DunnageType>>> GetAllTypesAsync();
    
    /// <summary>
    /// Get dunnage type by ID
    /// </summary>
    Task<Model_Dao_Result<Model_DunnageType>> GetTypeByIdAsync(int id);
    
    /// <summary>
    /// Insert new dunnage type (wizard workflow)
    /// </summary>
    Task<Model_Dao_Result<int>> InsertTypeAsync(string typeName, string icon, string user);
    
    /// <summary>
    /// Get all parts for a dunnage type
    /// </summary>
    Task<Model_Dao_Result<List<Model_DunnagePart>>> GetPartsByTypeAsync(int typeId);
    
    /// <summary>
    /// Search parts by partial Part ID
    /// </summary>
    Task<Model_Dao_Result<List<Model_DunnagePart>>> SearchPartsAsync(string searchTerm);
    
    /// <summary>
    /// Insert new dunnage part
    /// </summary>
    Task<Model_Dao_Result<int>> InsertPartAsync(Model_DunnagePart part);
    
    /// <summary>
    /// Get specs for a dunnage type
    /// </summary>
    Task<Model_Dao_Result<List<Model_DunnageSpec>>> GetSpecsByTypeAsync(int typeId);
    
    /// <summary>
    /// Insert batch of dunnage loads (wizard completion)
    /// </summary>
    Task<Model_Dao_Result<int>> InsertLoadsAsync(List<Model_DunnageLoad> loads);
    
    // ============================================
    // NEW METHODS (spec 010-dunnage-complete)
    // ============================================
    
    // --- Type Management (Admin UI) ---
    
    /// <summary>
    /// Update existing dunnage type name and icon
    /// </summary>
    /// <param name="id">Type ID to update</param>
    /// <param name="typeName">New type name</param>
    /// <param name="icon">New icon glyph (Unicode)</param>
    /// <param name="user">Username for audit trail</param>
    /// <returns>Success/failure result</returns>
    Task<Model_Dao_Result> UpdateTypeAsync(int id, string typeName, string icon, string user);
    
    /// <summary>
    /// Delete dunnage type (with impact analysis validation)
    /// </summary>
    /// <param name="id">Type ID to delete</param>
    /// <returns>Success/failure result with error if type has dependencies</returns>
    Task<Model_Dao_Result> DeleteTypeAsync(int id);
    
    /// <summary>
    /// Get count of parts using this type (for delete confirmation)
    /// </summary>
    Task<Model_Dao_Result<int>> GetPartCountByTypeAsync(int typeId);
    
    /// <summary>
    /// Get count of transaction records referencing this type
    /// </summary>
    Task<Model_Dao_Result<int>> GetTransactionCountByTypeAsync(int typeId);
    
    // --- Part Management (Admin UI) ---
    
    /// <summary>
    /// Update existing dunnage part
    /// </summary>
    Task<Model_Dao_Result> UpdatePartAsync(Model_DunnagePart part);
    
    /// <summary>
    /// Delete dunnage part (with transaction count validation)
    /// </summary>
    Task<Model_Dao_Result> DeletePartAsync(string partId);
    
    /// <summary>
    /// Get count of transaction records for a part (for delete confirmation)
    /// </summary>
    Task<Model_Dao_Result<int>> GetTransactionCountByPartAsync(string partId);
    
    // --- Spec Management (Admin UI) ---
    
    /// <summary>
    /// Update spec definition for a dunnage type
    /// </summary>
    Task<Model_Dao_Result> UpdateSpecAsync(int typeId, string specJson, string user);
    
    /// <summary>
    /// Delete all specs for a dunnage type
    /// </summary>
    Task<Model_Dao_Result> DeleteSpecsByTypeAsync(int typeId);
    
    /// <summary>
    /// Get union of all unique spec keys across all types (for CSV export)
    /// </summary>
    Task<Model_Dao_Result<List<string>>> GetAllSpecKeysAsync();
    
    // --- Load Management (Edit Mode) ---
    
    /// <summary>
    /// Get dunnage loads by date range with optional user filter
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="userId">Optional username filter (null = all users)</param>
    /// <returns>List of loads in date range</returns>
    Task<Model_Dao_Result<List<Model_DunnageLoad>>> GetLoadsByDateRangeAsync(
        System.DateTime startDate, 
        System.DateTime endDate, 
        string? userId = null);
    
    /// <summary>
    /// Update existing dunnage load record
    /// </summary>
    Task<Model_Dao_Result> UpdateLoadAsync(Model_DunnageLoad load);
    
    /// <summary>
    /// Delete dunnage load record
    /// </summary>
    Task<Model_Dao_Result> DeleteLoadAsync(string loadId);
    
    // --- Inventoried List Management (Admin UI) ---
    
    /// <summary>
    /// Get all parts on inventoried dunnage list
    /// </summary>
    Task<Model_Dao_Result<List<Model_InventoriedDunnage>>> GetInventoriedDunnageListAsync();
    
    /// <summary>
    /// Insert part into inventoried dunnage list
    /// </summary>
    Task<Model_Dao_Result<int>> InsertInventoriedDunnageAsync(Model_InventoriedDunnage item);
    
    /// <summary>
    /// Update inventoried dunnage entry (method and notes)
    /// </summary>
    Task<Model_Dao_Result> UpdateInventoriedDunnageAsync(Model_InventoriedDunnage item);
    
    /// <summary>
    /// Delete part from inventoried dunnage list
    /// </summary>
    Task<Model_Dao_Result> DeleteInventoriedDunnageAsync(int id);
    
    // --- Custom Fields (Add Type Dialog) ---
    
    /// <summary>
    /// Get custom field definitions for a dunnage type
    /// </summary>
    Task<Model_Dao_Result<List<Model_CustomFieldDefinition>>> GetCustomFieldsByTypeAsync(int typeId);
    
    /// <summary>
    /// Insert custom field definition
    /// </summary>
    Task<Model_Dao_Result<int>> InsertCustomFieldAsync(Model_CustomFieldDefinition field);
    
    /// <summary>
    /// Delete custom field definition
    /// </summary>
    Task<Model_Dao_Result> DeleteCustomFieldAsync(int fieldId);
    
    /// <summary>
    /// Reorder custom fields (update DisplayOrder values)
    /// </summary>
    Task<Model_Dao_Result> ReorderCustomFieldsAsync(List<Model_CustomFieldDefinition> fields);
    
    // --- User Preferences ---
    
    /// <summary>
    /// Get user preference by key (icon usage history, pagination size, etc.)
    /// </summary>
    Task<Model_Dao_Result<string>> GetUserPreferenceAsync(string userId, string key);
    
    /// <summary>
    /// Insert or update user preference
    /// </summary>
    Task<Model_Dao_Result> UpsertUserPreferenceAsync(string userId, string key, string jsonValue);
}

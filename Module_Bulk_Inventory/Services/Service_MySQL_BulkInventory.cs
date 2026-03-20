using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Bulk_Inventory.Contracts.Services;
using MTM_Receiving_Application.Module_Bulk_Inventory.Data;
using MTM_Receiving_Application.Module_Bulk_Inventory.Enums;
using MTM_Receiving_Application.Module_Bulk_Inventory.Models;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;

namespace MTM_Receiving_Application.Module_Bulk_Inventory.Services;

/// <summary>
/// Business-layer service for bulk inventory transaction persistence.
/// Delegates to <see cref="Dao_BulkInventoryTransaction"/>; adds logging at
/// key state-transition points.
/// </summary>
public class Service_MySQL_BulkInventory : IService_MySQL_BulkInventory
{
    private readonly Dao_BulkInventoryTransaction _dao;
    private readonly IService_LoggingUtility _logger;

    public Service_MySQL_BulkInventory(
        Dao_BulkInventoryTransaction dao,
        IService_LoggingUtility logger
    )
    {
        _dao = dao;
        _logger = logger;
    }

    // ── Batch entry ───────────────────────────────────────────────────────────

    public async Task<Model_Dao_Result<int>> StartRowAsync(Model_BulkInventoryTransaction row)
    {
        _logger.LogInfo(
            $"BulkInventory: Inserting row — PartId={row.PartId}, Type={row.TransactionType}"
        );
        var result = await _dao.InsertAsync(row);
        if (!result.IsSuccess)
        {
            _logger.LogError($"BulkInventory: Insert failed — {result.ErrorMessage}");
        }
        return result;
    }

    public async Task<Model_Dao_Result> UpdateRowAsync(Model_BulkInventoryTransaction row)
    {
        _logger.LogInfo(
            $"BulkInventory: Updating row Id={row.Id} — PartId={row.PartId}, Type={row.TransactionType}"
        );
        var result = await _dao.UpdateAsync(row);
        if (!result.IsSuccess)
        {
            _logger.LogError(
                $"BulkInventory: Update failed for Id={row.Id} — {result.ErrorMessage}"
            );
        }
        return result;
    }

    // ── Automation pipeline ───────────────────────────────────────────────────

    public async Task<Model_Dao_Result> WriteAuditAsync(int id)
    {
        _logger.LogInfo($"BulkInventory: Row {id} → InProgress");
        return await _dao.UpdateStatusAsync(id, Enum_BulkInventoryStatus.InProgress, null);
    }

    public async Task<Model_Dao_Result> CompleteRowAsync(
        int id,
        Enum_BulkInventoryStatus status,
        string? errorMessage = null
    )
    {
        if (status == Enum_BulkInventoryStatus.Failed)
        {
            _logger.LogError($"BulkInventory: Row {id} → Failed — {errorMessage}");
        }
        else
        {
            _logger.LogInfo($"BulkInventory: Row {id} → {status}");
        }

        return await _dao.UpdateStatusAsync(id, status, errorMessage);
    }

    // ── General CRUD ──────────────────────────────────────────────────────────

    public Task<Model_Dao_Result> UpdateStatusAsync(
        int id,
        Enum_BulkInventoryStatus status,
        string? errorMessage = null
    ) => _dao.UpdateStatusAsync(id, status, errorMessage);

    public Task<Model_Dao_Result<List<Model_BulkInventoryTransaction>>> GetByUserAsync(
        string username,
        Enum_BulkInventoryStatus? status = null
    ) => _dao.GetByUserAsync(username, status);

    public Task<Model_Dao_Result> DeleteByIdAsync(int id) => _dao.DeleteByIdAsync(id);
}

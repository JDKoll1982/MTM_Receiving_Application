using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Routing.Models;

namespace MTM_Receiving_Application.Module_Routing.Interfaces;

/// <summary>
/// Interface for routing label data access operations
/// Issue #25: Created for testability and dependency injection
/// </summary>
public interface IDao_RoutingLabel
{
    Task<Model_Dao_Result<int>> InsertLabelAsync(Model_RoutingLabel label);
    Task<Model_Dao_Result> UpdateLabelAsync(Model_RoutingLabel label);
    Task<Model_Dao_Result<Model_RoutingLabel>> GetLabelByIdAsync(int labelId);
    Task<Model_Dao_Result<List<Model_RoutingLabel>>> GetAllLabelsAsync(int limit = 100, int offset = 0);
    Task<Model_Dao_Result> DeleteLabelAsync(int labelId);
    Task<Model_Dao_Result> MarkLabelExportedAsync(int labelId);
    Task<Model_Dao_Result<Model_RoutingLabel>> CheckDuplicateLabelAsync(string poNumber, string lineNumber, int recipientId, int hoursWindow = 24);
    Task<Model_Dao_Result<(bool Exists, int? ExistingLabelId)>> CheckDuplicateAsync(string poNumber, string lineNumber, int recipientId, DateTime createdWithinDate);
    Task<Model_Dao_Result> MarkExportedAsync(List<int> labelIds);
}

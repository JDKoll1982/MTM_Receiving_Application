using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries;

/// <summary>
/// Handler for QueryRequest_Receiving_Shared_Search_Transactions
/// Performs multi-criteria search across transactions
/// </summary>
public class QueryHandler_Receiving_Shared_Search_Transactions
    : IRequestHandler<QueryRequest_Receiving_Shared_Search_Transactions, Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingTransaction>>>
{
    private readonly Dao_Receiving_Repository_Transaction _dao;
    private readonly IService_LoggingUtility _logger;

    public QueryHandler_Receiving_Shared_Search_Transactions(
        Dao_Receiving_Repository_Transaction dao,
        IService_LoggingUtility logger)
    {
        _dao = dao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<List<Model_Receiving_TableEntitys_ReceivingTransaction>>> Handle(
        QueryRequest_Receiving_Shared_Search_Transactions request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo("Searching transactions with filters");

            var allTransactions = new List<Model_Receiving_TableEntitys_ReceivingTransaction>();

            // Strategy: Query by most specific filter first, then filter in-memory
            if (!string.IsNullOrWhiteSpace(request.PONumber))
            {
                // Start with PO number search
                var poResult = await _dao.SelectByPOAsync(request.PONumber);
                if (poResult.Success && poResult.Data != null)
                {
                    allTransactions = poResult.Data;
                }
            }
            else if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                // Use date range search
                var startDate = request.StartDate ?? DateTime.MinValue;
                var endDate = request.EndDate ?? DateTime.MaxValue;
                var dateResult = await _dao.SelectByDateRangeAsync(startDate, endDate);
                if (dateResult.Success && dateResult.Data != null)
                {
                    allTransactions = dateResult.Data;
                }
            }
            else
            {
                // No specific filters - this would require a "SelectAll" method
                // For now, return empty to avoid loading all transactions
                _logger.LogWarning("Search query too broad - at least one filter required");
                return Model_Dao_Result_Factory.Success(new List<Model_Receiving_TableEntitys_ReceivingTransaction>());
            }

            // Apply in-memory filters
            var filtered = allTransactions.AsQueryable();

            // Note: PartNumber filter would require joining with Lines table - skipping for now
            // Note: Status filter would require a Status property on Transaction entity - skipping for now

            if (request.StartDate.HasValue)
            {
                filtered = filtered.Where(t => t.TransactionDate >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                filtered = filtered.Where(t => t.TransactionDate <= request.EndDate.Value);
            }

            var results = filtered.ToList();

            _logger.LogInfo($"Search complete: {results.Count} transactions found");

            return Model_Dao_Result_Factory.Success(results);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in QueryHandler_Receiving_Shared_Search_Transactions: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<List<Model_Receiving_TableEntitys_ReceivingTransaction>>(
                $"Error searching transactions: {ex.Message}", ex);
        }
    }
}

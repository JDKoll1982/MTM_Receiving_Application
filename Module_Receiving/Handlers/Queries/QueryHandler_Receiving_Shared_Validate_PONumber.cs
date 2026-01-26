using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.DTOs;
using MTM_Receiving_Application.Module_Receiving.Requests.Queries;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Queries;

/// <summary>
/// Handler for ValidatePONumberQuery
/// Validates PO format and checks for duplicates
/// </summary>
public class QueryHandler_Receiving_Shared_Validate_PONumber
    : IRequestHandler<QueryRequest_Receiving_Shared_Validate_PONumber, Model_Dao_Result<Model_Receiving_DataTransferObjects_POValidationResult>>
{
    private readonly Dao_Receiving_Repository_Transaction _transactionDao;
    private readonly IService_LoggingUtility _logger;

    // PO Number format: Letters + numbers, optional hyphens (e.g., PO-123456, ABC123, etc.)
    private static readonly Regex PONumberRegex = new Regex(@"^[A-Z0-9-]{3,50}$", RegexOptions.IgnoreCase);

    public QueryHandler_Receiving_Shared_Validate_PONumber(
        Dao_Receiving_Repository_Transaction transactionDao,
        IService_LoggingUtility logger)
    {
        _transactionDao = transactionDao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<Model_Receiving_DataTransferObjects_POValidationResult>> Handle(
        QueryRequest_Receiving_Shared_Validate_PONumber request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Validating PO number: {request.PONumber}");

            var result = new Model_Receiving_DataTransferObjects_POValidationResult();

            // Step 1: Format validation
            if (string.IsNullOrWhiteSpace(request.PONumber))
            {
                result.IsValid = false;
                result.ErrorMessage = "PO Number cannot be empty";
                return Model_Dao_Result_Factory.Success(result);
            }

            var trimmedPO = request.PONumber.Trim().ToUpperInvariant();

            if (!PONumberRegex.IsMatch(trimmedPO))
            {
                result.IsValid = false;
                result.ErrorMessage = "Invalid PO Number format. Use alphanumeric characters and hyphens only (3-50 characters)";
                return Model_Dao_Result_Factory.Success(result);
            }

            result.FormattedPONumber = trimmedPO;

            // Step 2: Duplicate check
            var existingTransactions = await _transactionDao.SelectByPOAsync(trimmedPO);
            if (existingTransactions.Success && existingTransactions.Data != null && existingTransactions.Data.Count > 0)
            {
                result.AlreadyReceived = true;
                result.Warnings.Add($"PO {trimmedPO} has been received {existingTransactions.Data.Count} time(s) previously");
            }

            // Step 3: ERP validation (future enhancement - query Infor Visual)
            // For now, just mark as valid if format passes
            result.ExistsInERP = false; // TODO: Implement ERP lookup

            result.IsValid = true;

            _logger.LogInfo($"PO validation complete: {trimmedPO} - Valid={result.IsValid}");

            return Model_Dao_Result_Factory.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in ValidatePONumberQueryHandler: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<Model_Receiving_DataTransferObjects_POValidationResult>(
                $"Error validating PO: {ex.Message}", ex);
        }
    }
}

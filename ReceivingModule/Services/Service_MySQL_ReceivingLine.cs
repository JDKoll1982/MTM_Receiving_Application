using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.ReceivingModule.Data;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.Models.Core;
using MTM_Receiving_Application.Models.Enums;
using MTM_Receiving_Application.ReceivingModule.Models;

namespace MTM_Receiving_Application.ReceivingModule.Services;

public class Service_MySQL_ReceivingLine : IService_MySQL_ReceivingLine
{
    private readonly Dao_ReceivingLine _receivingLineDao;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_ErrorHandler _errorHandler;

    public Service_MySQL_ReceivingLine(
        Dao_ReceivingLine receivingLineDao,
        IService_LoggingUtility logger,
        IService_ErrorHandler errorHandler)
    {
        _receivingLineDao = receivingLineDao;
        _logger = logger;
        _errorHandler = errorHandler;
    }

    public async Task<Model_Dao_Result> InsertReceivingLineAsync(Model_ReceivingLine line)
    {
        try
        {
            // DELEGATE TO DAO
            var result = await _receivingLineDao.InsertReceivingLineAsync(line);

            if (!result.IsSuccess)
            {
                _logger.LogError(
                    $"Failed to insert receiving line: {result.ErrorMessage}", null, "ReceivingLine");
            }
            else
            {
                _logger.LogInfo(
                   $"Inserted receiving line for PO {line.PONumber}, Part {line.PartID}", "ReceivingLine");
            }

            return result;
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Error in {nameof(InsertReceivingLineAsync)}: {ex.Message}",
                Enum_ErrorSeverity.Critical,
                ex);

            return Model_Dao_Result_Factory.Failure(
                "An error occurred while inserting receiving line.", ex);
        }
    }
}

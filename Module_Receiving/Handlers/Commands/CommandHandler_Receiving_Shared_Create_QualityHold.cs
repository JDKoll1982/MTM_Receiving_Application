using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models.Entities;
using MTM_Receiving_Application.Module_Receiving.Requests.Commands;

namespace MTM_Receiving_Application.Module_Receiving.Handlers.Commands;

/// <summary>
/// <para>
/// Handler for CommandRequest_Receiving_Shared_Create_QualityHold.
/// Creates a new quality hold record with initial user acknowledgment (Step 1 of 2).
/// </para>
/// <para>
/// Dependencies:
/// - Dao_Receiving_Repository_QualityHold: Injected DAO for quality hold database operations.
/// - IService_LoggingUtility: Logging service for tracking operations.
/// - Model_Receiving_TableEntitys_QualityHold: Entity model for quality hold data.
/// - CommandRequest_Receiving_Shared_Create_QualityHold: Command request containing quality hold data.
/// - Model_Dao_Result: Result wrapper for DAO operations.
/// </para>
/// <para>
/// User Requirements:
/// - Configurable part patterns (not hardcoded MMFSR/MMCSR)
/// - Two-step acknowledgment tracking (this creates Step 1)
/// - Full audit trail (all fields tracked)
/// </para>
/// </summary>
public class CommandHandler_Receiving_Shared_Create_QualityHold
    : IRequestHandler<CommandRequest_Receiving_Shared_Create_QualityHold, Model_Dao_Result<string>>
{
    private readonly Dao_Receiving_Repository_QualityHold _qualityHoldDao;
    private readonly IService_LoggingUtility _logger;

    public CommandHandler_Receiving_Shared_Create_QualityHold(
        Dao_Receiving_Repository_QualityHold qualityHoldDao,
        IService_LoggingUtility logger)
    {
        _qualityHoldDao = qualityHoldDao;
        _logger = logger;
    }

    public async Task<Model_Dao_Result<string>> Handle(
        CommandRequest_Receiving_Shared_Create_QualityHold request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInfo($"Creating quality hold for LineId={request.LineId}, PartNumber={request.PartNumber}, Pattern={request.PartPattern}");

            var qualityHold = new Model_Receiving_TableEntitys_QualityHold
            {
                QualityHoldId = Guid.NewGuid().ToString(),
                LineId = request.LineId,
                TransactionId = request.TransactionId,
                PartNumber = request.PartNumber,
                PartPattern = request.PartPattern,
                RestrictionType = request.RestrictionType,
                LoadNumber = request.LoadNumber,
                TotalWeight = request.TotalWeight,
                PackageType = request.PackageType,
                UserAcknowledgedDate = request.UserAcknowledgedDate,
                UserAcknowledgedBy = request.UserAcknowledgedBy,
                UserAcknowledgmentMessage = request.UserAcknowledgmentMessage,
                IsFullyAcknowledged = false, // Step 1 only
                Notes = request.Notes,
                CreatedBy = request.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            var result = await _qualityHoldDao.InsertQualityHoldAsync(qualityHold);

            if (result.Success)
            {
                _logger.LogInfo($"Quality hold created successfully: {result.Data}");
                return Model_Dao_Result_Factory.Success(result.Data);
            }

            _logger.LogError($"Failed to create quality hold: {result.ErrorMessage}");
            return Model_Dao_Result_Factory.Failure<string>(result.ErrorMessage ?? "Failed to create quality hold");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception in CommandHandler_Receiving_Shared_Create_QualityHold: {ex.Message}", ex);
            return Model_Dao_Result_Factory.Failure<string>($"Error creating quality hold: {ex.Message}", ex);
        }
    }
}

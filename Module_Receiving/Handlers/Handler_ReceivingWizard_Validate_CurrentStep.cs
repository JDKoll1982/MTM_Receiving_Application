using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Models;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Receiving.Models.Enums;
using MTM_Receiving_Application.Module_Receiving.Queries;
using Serilog;

namespace MTM_Receiving_Application.Module_Receiving.Handlers;

/// <summary>
/// Handler for validating current step and aggregating validation errors.
/// Returns validation status with list of errors and severity levels.
/// </summary>
public class Handler_ReceivingWizard_Validate_CurrentStep : IRequestHandler<Query_ReceivingWizard_Validate_CurrentStep, Result<ValidationStatus>>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public Handler_ReceivingWizard_Validate_CurrentStep(
        Dao_ReceivingWorkflowSession sessionDao,
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<ValidationStatus>> Handle(Query_ReceivingWizard_Validate_CurrentStep request, CancellationToken cancellationToken)
    {
        _logger.Information("Validating session {SessionId} step {Step}", request.SessionId, request.Step);

        var errors = new List<ValidationError>();

        // Get session
        var sessionResult = await _sessionDao.GetSessionAsync(request.SessionId);
        if (!sessionResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve session {SessionId}: {Error}", request.SessionId, sessionResult.ErrorMessage);
            return Result<ValidationStatus>.Failure(sessionResult.ErrorMessage);
        }

        var session = sessionResult.Data;

        // Validate Step 1 (Order & Part Selection)
        if (request.Step >= 1)
        {
            if (string.IsNullOrWhiteSpace(session.PONumber))
            {
                errors.Add(new ValidationError
                {
                    FieldName = "PONumber",
                    ErrorMessage = "PO Number is required",
                    Severity = ErrorSeverity.Error
                });
            }

            if (session.PartId <= 0)
            {
                errors.Add(new ValidationError
                {
                    FieldName = "PartId",
                    ErrorMessage = "Part must be selected",
                    Severity = ErrorSeverity.Error
                });
            }

            if (session.LoadCount <= 0 || session.LoadCount > 100)
            {
                errors.Add(new ValidationError
                {
                    FieldName = "LoadCount",
                    ErrorMessage = "Load count must be between 1 and 100",
                    Severity = ErrorSeverity.Error
                });
            }
        }

        // Validate Step 2 (Load Details Entry)
        if (request.Step >= 2)
        {
            var loadsResult = await _loadDao.GetLoadsBySessionAsync(request.SessionId);
            if (loadsResult.IsSuccess && loadsResult.Data != null)
            {
                var loads = loadsResult.Data;

                if (loads.Count != session.LoadCount)
                {
                    errors.Add(new ValidationError
                    {
                        FieldName = "LoadDetails",
                        ErrorMessage = $"Expected {session.LoadCount} loads, but found {loads.Count}",
                        Severity = ErrorSeverity.Warning
                    });
                }

                foreach (var load in loads)
                {
                    if (load.WeightOrQuantity <= 0)
                    {
                        errors.Add(new ValidationError
                        {
                            FieldName = $"Load{load.LoadNumber}.WeightOrQuantity",
                            ErrorMessage = $"Load {load.LoadNumber}: Weight is required and must be greater than 0",
                            Severity = ErrorSeverity.Error
                        });
                    }

                    if (string.IsNullOrWhiteSpace(load.HeatLot))
                    {
                        errors.Add(new ValidationError
                        {
                            FieldName = $"Load{load.LoadNumber}.HeatLot",
                            ErrorMessage = $"Load {load.LoadNumber}: Heat Lot is required",
                            Severity = ErrorSeverity.Error
                        });
                    }

                    if (string.IsNullOrWhiteSpace(load.PackageType))
                    {
                        errors.Add(new ValidationError
                        {
                            FieldName = $"Load{load.LoadNumber}.PackageType",
                            ErrorMessage = $"Load {load.LoadNumber}: Package Type is required",
                            Severity = ErrorSeverity.Error
                        });
                    }

                    if (load.PackagesPerLoad <= 0)
                    {
                        errors.Add(new ValidationError
                        {
                            FieldName = $"Load{load.LoadNumber}.PackagesPerLoad",
                            ErrorMessage = $"Load {load.LoadNumber}: Packages Per Load must be greater than 0",
                            Severity = ErrorSeverity.Error
                        });
                    }
                }
            }
        }

        var isValid = !errors.Any(e => e.Severity == ErrorSeverity.Error);
        var errorCount = errors.Count(e => e.Severity == ErrorSeverity.Error);
        var warningCount = errors.Count(e => e.Severity == ErrorSeverity.Warning);
        
        var status = new ValidationStatus(isValid, errors, errorCount, warningCount);
        _logger.Information("Validation complete for session {SessionId} step {Step}: {ErrorCount} errors, {WarningCount} warnings", 
            request.SessionId, request.Step, errorCount, warningCount);
        return Result<ValidationStatus>.Success(status);
    }
}

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
/// Returns list of errors with severity levels.
/// </summary>
public class GetValidationStatusQueryHandler : IRequestHandler<GetValidationStatusQuery, Result<List<ValidationError>>>
{
    private readonly Dao_ReceivingWorkflowSession _sessionDao;
    private readonly Dao_ReceivingLoadDetail _loadDao;
    private readonly ILogger _logger;

    public GetValidationStatusQueryHandler(
        Dao_ReceivingWorkflowSession sessionDao,
        Dao_ReceivingLoadDetail loadDao,
        ILogger logger)
    {
        _sessionDao = sessionDao;
        _loadDao = loadDao;
        _logger = logger;
    }

    public async Task<Result<List<ValidationError>>> Handle(GetValidationStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.Information("Validating session {SessionId}", request.SessionId);

        var errors = new List<ValidationError>();

        // Get session
        var sessionResult = await _sessionDao.GetSessionAsync(request.SessionId);
        if (!sessionResult.IsSuccess)
        {
            _logger.Error("Failed to retrieve session {SessionId}: {Error}", request.SessionId, sessionResult.ErrorMessage);
            return Result<List<ValidationError>>.Failure(sessionResult.ErrorMessage);
        }

        var session = sessionResult.Data;

        // Validate Step 1
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

        // Validate Step 2 - Load Details
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
                if (load.Weight <= 0)
                {
                    errors.Add(new ValidationError
                    {
                        FieldName = $"Load{load.LoadNumber}.Weight",
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

        _logger.Information("Validation complete for session {SessionId}: {ErrorCount} errors found", request.SessionId, errors.Count);
        return Result<List<ValidationError>>.Success(errors);
    }
}

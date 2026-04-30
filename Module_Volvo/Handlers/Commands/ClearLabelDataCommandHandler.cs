using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Services;

namespace MTM_Receiving_Application.Module_Volvo.Handlers.Commands;

/// <summary>
/// Handler for ClearLabelDataCommand — moves all active generated Volvo label rows
/// from the dedicated label queue into the generated-label history table.
/// </summary>
public class ClearLabelDataCommandHandler
    : IRequestHandler<ClearLabelDataCommand, Model_Dao_Result<int>>
{
    private readonly IDao_VolvoGeneratedLabelData _generatedLabelDataDao;
    private readonly IService_VolvoAuthorization _authService;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_UserPrivileges _userPrivileges;

    public ClearLabelDataCommandHandler(
        IDao_VolvoGeneratedLabelData generatedLabelDataDao,
        IService_VolvoAuthorization authService,
        IService_UserSessionManager sessionManager,
        IService_UserPrivileges userPrivileges
    )
    {
        _generatedLabelDataDao =
            generatedLabelDataDao ?? throw new ArgumentNullException(nameof(generatedLabelDataDao));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _userPrivileges = userPrivileges ?? throw new ArgumentNullException(nameof(userPrivileges));
    }

    public async Task<Model_Dao_Result<int>> Handle(
        ClearLabelDataCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var authResult = await _authService.CanCompleteShipmentsAsync();
            if (!authResult.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(
                    "You are not authorized to clear label data"
                );
            }

            var employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;
            if (!request.ClearAllRows && employeeNumber <= 0)
            {
                return Model_Dao_Result_Factory.Failure<int>(
                    "A valid 4-digit employee number is required to clear your Volvo generated label rows."
                );
            }

            if (request.ClearAllRows)
            {
                if (employeeNumber <= 0)
                {
                    return Model_Dao_Result_Factory.Failure<int>(
                        "Only Admin or Developer users can clear all Volvo generated label rows."
                    );
                }

                if (
                    !_userPrivileges.IsInitialized
                    || _userPrivileges.CurrentUserId != employeeNumber
                )
                {
                    var initializeResult = await _userPrivileges.InitializeAsync(employeeNumber);
                    if (!initializeResult.IsSuccess)
                    {
                        return Model_Dao_Result_Factory.Failure<int>(
                            initializeResult.ErrorMessage
                                ?? "Failed to load user roles for Volvo label clearing.",
                            initializeResult.Exception
                        );
                    }
                }

                if (!_userPrivileges.HasAnyRole("Admin", "Developer"))
                {
                    return Model_Dao_Result_Factory.Failure<int>(
                        "Only Admin or Developer users can clear all Volvo generated label rows."
                    );
                }
            }

            var archivedBy = string.IsNullOrWhiteSpace(request.ArchivedBy)
                ? "SYSTEM"
                : request.ArchivedBy;
            var result = await _generatedLabelDataDao.ClearToHistoryAsync(
                archivedBy,
                employeeNumber,
                request.ClearAllRows
            );

            if (!result.IsSuccess)
            {
                return Model_Dao_Result_Factory.Failure<int>(
                    result.ErrorMessage ?? "Clear label data failed"
                );
            }

            return Model_Dao_Result_Factory.Success<int>(result.Data);
        }
        catch (Exception ex)
        {
            return Model_Dao_Result_Factory.Failure<int>(
                $"Unexpected error during clear label data: {ex.Message}",
                ex
            );
        }
    }
}

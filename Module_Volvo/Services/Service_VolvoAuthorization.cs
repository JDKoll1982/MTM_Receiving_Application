using System;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Volvo.Services;

/// <summary>
/// Authorization service for Volvo module operations
/// Currently implements basic authorization - can be extended with role-based checks
/// </summary>
public class Service_VolvoAuthorization : IService_VolvoAuthorization
{
    private readonly IService_LoggingUtility _logger;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_UserPrivileges _userPrivileges;

    public Service_VolvoAuthorization(
        IService_LoggingUtility logger,
        IService_UserSessionManager sessionManager,
        IService_UserPrivileges userPrivileges
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _userPrivileges = userPrivileges ?? throw new ArgumentNullException(nameof(userPrivileges));
    }

    /// <summary>
    /// Checks if current user can create/modify Volvo shipments
    /// </summary>
    public async Task<Model_Dao_Result> CanManageShipmentsAsync()
    {
        try
        {
            await _logger.LogInfoAsync("Authorization check: CanManageShipments");

            return await AuthorizeAuthenticatedUserAsync();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(
                $"Error checking shipment management authorization: {ex.Message}",
                ex
            );
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = "Authorization check failed",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex,
            };
        }
    }

    /// <summary>
    /// Checks if current user can manage Volvo master data (parts, components)
    /// </summary>
    public async Task<Model_Dao_Result> CanManageMasterDataAsync()
    {
        try
        {
            await _logger.LogInfoAsync("Authorization check: CanManageMasterData");

            return await AuthorizeByRoleAsync("Supervisor", "Admin", "Developer");
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(
                $"Error checking master data authorization: {ex.Message}",
                ex
            );
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = "Authorization check failed",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex,
            };
        }
    }

    /// <summary>
    /// Checks if current user can complete/close Volvo shipments
    /// </summary>
    public async Task<Model_Dao_Result> CanCompleteShipmentsAsync()
    {
        try
        {
            await _logger.LogInfoAsync("Authorization check: CanCompleteShipments");

            return await AuthorizeAuthenticatedUserAsync();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(
                $"Error checking shipment completion authorization: {ex.Message}",
                ex
            );
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = "Authorization check failed",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex,
            };
        }
    }

    /// <summary>
    /// Checks if current user can generate labels for Volvo shipments
    /// </summary>
    public async Task<Model_Dao_Result> CanGenerateLabelsAsync()
    {
        try
        {
            await _logger.LogInfoAsync("Authorization check: CanGenerateLabels");

            return await AuthorizeAuthenticatedUserAsync();
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync(
                $"Error checking label generation authorization: {ex.Message}",
                ex
            );
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = "Authorization check failed",
                Severity = Enum_ErrorSeverity.Error,
                Exception = ex,
            };
        }
    }

    private Task<Model_Dao_Result> AuthorizeAuthenticatedUserAsync()
    {
        var employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        if (employeeNumber.HasValue && employeeNumber.Value > 0)
        {
            return Task.FromResult(new Model_Dao_Result { Success = true });
        }

        return Task.FromResult(
            new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = "No active authenticated user session found.",
                Severity = Enum_ErrorSeverity.Warning,
            }
        );
    }

    private async Task<Model_Dao_Result> AuthorizeByRoleAsync(params string[] allowedRoles)
    {
        var employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        if (!employeeNumber.HasValue || employeeNumber.Value <= 0)
        {
            return new Model_Dao_Result
            {
                Success = false,
                ErrorMessage = "No active authenticated user session found.",
                Severity = Enum_ErrorSeverity.Warning,
            };
        }

        if (!_userPrivileges.IsInitialized || _userPrivileges.CurrentUserId != employeeNumber.Value)
        {
            var initializeResult = await _userPrivileges.InitializeAsync(employeeNumber.Value);
            if (!initializeResult.Success)
            {
                return new Model_Dao_Result
                {
                    Success = false,
                    ErrorMessage = initializeResult.ErrorMessage ?? "Failed to load user roles.",
                    Severity = Enum_ErrorSeverity.Error,
                    Exception = initializeResult.Exception,
                };
            }
        }

        if (_userPrivileges.HasAnyRole(allowedRoles))
        {
            return new Model_Dao_Result { Success = true };
        }

        return new Model_Dao_Result
        {
            Success = false,
            ErrorMessage =
                $"You are not authorized for this Volvo operation. Required role: {string.Join(" or ", allowedRoles)}.",
            Severity = Enum_ErrorSeverity.Warning,
        };
    }
}

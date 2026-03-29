using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.Views;

namespace MTM_Receiving_Application.Module_Core.Services.Authentication;

/// <summary>
/// Coordinates session creation and the manual switch-user flow.
/// </summary>
public class Service_UserLoginCoordinator : IService_UserLoginCoordinator
{
    private readonly IService_Authentication _authenticationService;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_UserPrivileges _userPrivileges;
    private readonly IService_SettingsCoreFacade _settingsCoreFacade;
    private readonly IService_Window _windowService;
    private readonly IService_ApplicationShutdown _applicationShutdown;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_ErrorHandler _errorHandler;
    private readonly IServiceProvider _serviceProvider;

    public Service_UserLoginCoordinator(
        IService_Authentication authenticationService,
        IService_UserSessionManager sessionManager,
        IService_UserPrivileges userPrivileges,
        IService_SettingsCoreFacade settingsCoreFacade,
        IService_Window windowService,
        IService_ApplicationShutdown applicationShutdown,
        IService_LoggingUtility logger,
        IService_ErrorHandler errorHandler,
        IServiceProvider serviceProvider
    )
    {
        _authenticationService =
            authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _userPrivileges = userPrivileges ?? throw new ArgumentNullException(nameof(userPrivileges));
        _settingsCoreFacade =
            settingsCoreFacade ?? throw new ArgumentNullException(nameof(settingsCoreFacade));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        _applicationShutdown =
            applicationShutdown ?? throw new ArgumentNullException(nameof(applicationShutdown));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
        _serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc/>
    public async Task<Model_Dao_Result<Model_UserSession>> InitializeAuthenticatedSessionAsync(
        Model_User user,
        Model_WorkstationConfig workstationConfig,
        string authenticationMethod
    )
    {
        if (user == null)
        {
            return Model_Dao_Result_Factory.Failure<Model_UserSession>(
                "Authenticated user is required."
            );
        }

        if (workstationConfig == null)
        {
            return Model_Dao_Result_Factory.Failure<Model_UserSession>(
                "Workstation configuration is required."
            );
        }

        try
        {
            ApplySafeUserDefaults(user);

            var session = _sessionManager.CreateSession(
                user,
                workstationConfig,
                authenticationMethod
            );
            _sessionManager.StartTimeoutMonitoring();

            await InitializePrivilegesAsync(user.EmployeeNumber);
            await InitializeSettingsDefaultsAsync(user.EmployeeNumber);

            _logger.LogInfo(
                $"Initialized authenticated session for employee {user.EmployeeNumber} using {authenticationMethod}.",
                nameof(Service_UserLoginCoordinator)
            );

            return Model_Dao_Result_Factory.Success(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Failed to initialize authenticated session.",
                ex,
                nameof(Service_UserLoginCoordinator)
            );
            return Model_Dao_Result_Factory.Failure<Model_UserSession>(
                "Failed to initialize the authenticated session.",
                ex
            );
        }
    }

    /// <inheritdoc/>
    public async Task<Model_Dao_Result<Model_UserSession>> LogOutAndPromptForLoginAsync(
        string logoutReason
    )
    {
        try
        {
            await _sessionManager.EndSessionAsync(logoutReason);
            _userPrivileges.Clear();

            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot == null)
            {
                return Model_Dao_Result_Factory.Failure<Model_UserSession>(
                    "Unable to open the login dialog because the application window is not ready."
                );
            }

            var workstationConfig = await _authenticationService.DetectWorkstationTypeAsync();
            var loginDialog =
                _serviceProvider.GetRequiredService<View_Shared_SharedTerminalLoginDialog>();
            loginDialog.XamlRoot = xamlRoot;

            _logger.LogInfo(
                "Prompting manual login after user-requested logout.",
                nameof(Service_UserLoginCoordinator)
            );
            _ = await loginDialog.ShowAsync();

            if (loginDialog.ViewModel.IsLockedOut)
            {
                _applicationShutdown.RequestShutdown("manual_logout_locked_out");
                App.MainWindow?.Close();

                return Model_Dao_Result_Factory.Failure<Model_UserSession>(
                    "Maximum login attempts exceeded after logout. The application is closing."
                );
            }

            if (loginDialog.ViewModel.IsCancelled)
            {
                _applicationShutdown.RequestShutdown("manual_logout_login_cancelled");
                App.MainWindow?.Close();

                return Model_Dao_Result_Factory.Failure<Model_UserSession>(
                    "Login was cancelled after logout. The application is closing."
                );
            }

            if (loginDialog.ViewModel.AuthenticatedUser == null)
            {
                return Model_Dao_Result_Factory.Failure<Model_UserSession>(
                    "Manual login did not return an authenticated user."
                );
            }

            return await InitializeAuthenticatedSessionAsync(
                loginDialog.ViewModel.AuthenticatedUser,
                workstationConfig,
                "manual_switch_user"
            );
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to log out and prompt for a new user login.",
                Enum_ErrorSeverity.Error,
                ex,
                false
            );

            return Model_Dao_Result_Factory.Failure<Model_UserSession>(
                "Failed to complete the logout and re-login flow.",
                ex
            );
        }
    }

    private async Task InitializePrivilegesAsync(int employeeNumber)
    {
        var privilegesResult = await _userPrivileges.InitializeAsync(employeeNumber);
        if (!privilegesResult.Success)
        {
            _logger.LogWarning(
                privilegesResult.ErrorMessage ?? "Failed to initialize user privileges.",
                nameof(Service_UserLoginCoordinator)
            );
        }
    }

    private async Task InitializeSettingsDefaultsAsync(int employeeNumber)
    {
        try
        {
            await _settingsCoreFacade.InitializeDefaultsAsync(employeeNumber);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"Failed to initialize settings defaults for employee {employeeNumber}: {ex.Message}",
                nameof(Service_UserLoginCoordinator)
            );
        }
    }

    private static void ApplySafeUserDefaults(Model_User user)
    {
        if (string.IsNullOrWhiteSpace(user.DefaultReceivingMode))
        {
            user.DefaultReceivingMode = "guided";
        }

        if (string.IsNullOrWhiteSpace(user.DefaultDunnageMode))
        {
            user.DefaultDunnageMode = "guided";
        }
    }
}

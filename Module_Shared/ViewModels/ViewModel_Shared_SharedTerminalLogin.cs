using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Shared.ViewModels
{
    /// <summary>
    /// ViewModel for SharedTerminalLoginDialog.
    /// Manages PIN-based authentication for shared terminal workstations.
    /// </summary>
    public partial class ViewModel_Shared_SharedTerminalLogin : ViewModel_Shared_Base
    {
        private readonly IService_Authentication _authService;

        // ====================================================================
        // Properties
        // ====================================================================

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _pin = string.Empty;

        [ObservableProperty]
        private int _attemptCount = 0;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        /// <summary>
        /// Indicates if the user was locked out after 3 failed attempts
        /// </summary>
        public bool IsLockedOut { get; set; } = false;

        /// <summary>
        /// Indicates if the user cancelled the login dialog
        /// </summary>
        public bool IsCancelled { get; set; } = false;

        /// <summary>
        /// The authenticated user (null if authentication failed)
        /// </summary>
        public Model_User? AuthenticatedUser { get; private set; }

        // ====================================================================
        // Constructor
        // ====================================================================

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        /// <param name="authService"></param>
        /// <param name="errorHandler"></param>
        /// <param name="logger"></param>
        public ViewModel_Shared_SharedTerminalLogin(
            IService_Authentication authService,
            IService_ErrorHandler errorHandler,
            IService_LoggingUtility logger)
            : base(errorHandler, logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        // ====================================================================
        // Commands
        // ====================================================================

        /// <summary>
        /// Attempts to authenticate the user with provided credentials
        /// </summary>
        /// <returns>True if authentication successful, false otherwise</returns>
        public async Task<bool> LoginAsync()
        {
            if (IsBusy)
            {
                return false;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Authenticating...";
                ErrorMessage = string.Empty;

                // Validate input (basic checks)
                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Username is required.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(Pin))
                {
                    ErrorMessage = "PIN is required.";
                    return false;
                }

                // Call authentication service
                var progress = new Progress<string>(msg => StatusMessage = msg);
                var result = await _authService.AuthenticateByPinAsync(Username, Pin, progress);

                if (result.Success && result.User != null)
                {
                    // Authentication successful
                    AuthenticatedUser = result.User;
                    StatusMessage = $"Welcome, {result.User.FullName}";

                    _logger.LogInfo($"PIN authentication successful for user: {Username}");

                    return true;
                }
                else
                {
                    // Authentication failed
                    ErrorMessage = result.ErrorMessage ?? "Invalid username or PIN.";
                    StatusMessage = "Authentication failed";

                    _logger.LogWarning($"PIN authentication failed for user: {Username}. Attempt {AttemptCount}");

                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred during authentication.";
                StatusMessage = "Error";

                await _errorHandler.HandleErrorAsync(
                    $"Login error for user {Username}",
                    Enum_ErrorSeverity.Error,
                    ex,
                    showDialog: false);

                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}


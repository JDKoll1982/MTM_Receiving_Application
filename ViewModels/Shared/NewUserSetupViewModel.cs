using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.ViewModels.Shared
{
    /// <summary>
    /// ViewModel for NewUserSetupDialog.
    /// Manages new user account creation workflow.
    /// </summary>
    public partial class NewUserSetupViewModel : BaseViewModel
    {
        private readonly IService_Authentication _authService;

        // ====================================================================
        // Properties
        // ====================================================================

        [ObservableProperty]
        private string _windowsUsername = string.Empty;

        [ObservableProperty]
        private string _employeeNumber = string.Empty;

        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private string _department = string.Empty;

        [ObservableProperty]
        private string _shift = string.Empty;

        [ObservableProperty]
        private string _pin = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _showCustomDepartment = false;

        [ObservableProperty]
        private bool _configureErpAccess = false;

        [ObservableProperty]
        private string? _visualUsername;

        [ObservableProperty]
        private string? _visualPassword;

        /// <summary>
        /// List of available departments from database
        /// </summary>
        public ObservableCollection<string> Departments { get; } = new ObservableCollection<string>();

        /// <summary>
        /// The new employee number assigned after successful account creation
        /// </summary>
        public int NewEmployeeNumber { get; private set; }

        /// <summary>
        /// Indicates if the user cancelled the dialog
        /// </summary>
        public bool IsCancelled { get; set; } = false;

        /// <summary>
        /// The Windows username of the person creating this account (for audit trail)
        /// </summary>
        public string CreatedBy { get; set; } = Environment.UserName;

        // ====================================================================
        // Constructor
        // ====================================================================

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public NewUserSetupViewModel(
            IService_Authentication authService,
            IService_ErrorHandler errorHandler,
            ILoggingService logger) 
            : base(errorHandler, logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        // ====================================================================
        // Methods
        // ====================================================================

        /// <summary>
        /// Load active departments from database
        /// </summary>
        public async Task LoadDepartmentsAsync()
        {
            try
            {
                IsBusy = true;
                StatusMessage = "Loading departments...";

                var departmentList = await _authService.GetActiveDepartmentsAsync();

                Departments.Clear();
                foreach (var dept in departmentList)
                {
                    Departments.Add(dept);
                }

                StatusMessage = $"Loaded {Departments.Count} departments";
                _logger.LogInfo($"Loaded {Departments.Count} departments for new user setup");
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error loading departments.";
                await _errorHandler.HandleErrorAsync(
                    "Failed to load departments",
                    Enum_ErrorSeverity.Warning,
                    ex,
                    showDialog: false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Create new user account
        /// </summary>
        public async Task<bool> CreateAccountAsync()
        {
            if (IsBusy) return false;

            try
            {
                IsBusy = true;
                StatusMessage = "Creating account...";
                ErrorMessage = string.Empty;

                // Validate employee number
                if (!int.TryParse(EmployeeNumber, out int empNum) || empNum <= 0)
                {
                    ErrorMessage = "Employee number must be a positive number";
                    StatusMessage = "Validation failed";
                    return false;
                }

                // Validate PIN uniqueness via service
                var pinValidation = await _authService.ValidatePinAsync(Pin, empNum);
                if (!pinValidation.IsValid)
                {
                    ErrorMessage = pinValidation.ErrorMessage;
                    StatusMessage = "PIN validation failed";
                    return false;
                }

                // Construct Model_User object
                var newUser = new Models.Model_User
                {
                    EmployeeNumber = empNum,
                    WindowsUsername = WindowsUsername,
                    FullName = FullName,
                    Department = Department,
                    Shift = Shift,
                    Pin = Pin,
                    IsActive = true,
                    VisualUsername = ConfigureErpAccess ? VisualUsername : null,
                    VisualPassword = ConfigureErpAccess ? VisualPassword : null
                };

                // Call authentication service to create new user
                var progress = new Progress<string>(msg => StatusMessage = msg);
                var result = await _authService.CreateNewUserAsync(newUser, CreatedBy, progress);

                if (result.Success && result.EmployeeNumber > 0)
                {
                    // Account created successfully
                    NewEmployeeNumber = result.EmployeeNumber;
                    StatusMessage = $"Account created! Employee #: {NewEmployeeNumber}";
                    
                    _logger.LogInfo($"New user account created: {FullName} (Emp #{NewEmployeeNumber}) by {CreatedBy}");
                    
                    return true;
                }
                else
                {
                    // Account creation failed
                    ErrorMessage = result.ErrorMessage ?? "Failed to create account.";
                    StatusMessage = "Account creation failed";
                    
                    _logger.LogWarning($"Failed to create account for {WindowsUsername}: {ErrorMessage}");
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred while creating the account.";
                StatusMessage = "Error";
                
                await _errorHandler.HandleErrorAsync(
                    $"Account creation error for {WindowsUsername}",
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

        /// <summary>
        /// Validate PIN format (4 digits)
        /// </summary>
        public bool ValidatePinFormat(string pin)
        {
            if (string.IsNullOrWhiteSpace(pin))
                return false;

            if (pin.Length != 4)
                return false;

            foreach (char c in pin)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validate that PINs match
        /// </summary>
        public bool ValidatePinMatch(string pin, string confirmPin)
        {
            return !string.IsNullOrWhiteSpace(pin) && pin == confirmPin;
        }

        /// <summary>
        /// Validate full name (at least 2 characters)
        /// </summary>
        public bool ValidateFullName(string fullName)
        {
            return !string.IsNullOrWhiteSpace(fullName) && fullName.Trim().Length >= 2;
        }
    }
}

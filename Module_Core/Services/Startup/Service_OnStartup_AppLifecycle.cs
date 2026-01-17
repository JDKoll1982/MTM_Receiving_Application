using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.WinUI;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Shared.Views;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Core.Services.Startup
{
    public class Service_OnStartup_AppLifecycle : IService_OnStartup_AppLifecycle
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IService_Authentication _authService;
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_ErrorHandler _errorHandler;
        private readonly IService_CSVWriter _csvWriter;
        private View_Shared_SplashScreenWindow? _splashScreen;

        public Service_OnStartup_AppLifecycle(
            IServiceProvider serviceProvider,
            IService_Authentication authService,
            IService_UserSessionManager sessionManager,
            IService_ErrorHandler errorHandler,
            IService_CSVWriter csvWriter)
        {
            _serviceProvider = serviceProvider;
            _authService = authService;
            _sessionManager = sessionManager;
            _errorHandler = errorHandler;
            _csvWriter = csvWriter;
        }

        public async Task StartAsync()
        {
            try
            {
                // 1. Show splash screen immediately (0-15%)
                _splashScreen = _serviceProvider.GetRequiredService<View_Shared_SplashScreenWindow>();
                _splashScreen.Activate();
                UpdateSplash(5, "Starting application...");
                await Task.Delay(100);

                // 2. Create Main Window (hidden initially)
                UpdateSplash(15, "Loading main window...");
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                App.MainWindow = mainWindow;

                // Give UI thread time to initialize
                await Task.Delay(100);

                // 3. Initialize Services (20%)
                UpdateSplash(20, "Initializing services...");
                await Task.Delay(300);

                // 3. Check if Windows user exists (30%)
                UpdateSplash(30, "Checking user account...");
                var windowsUser = Environment.UserName;
                var userCheckResult = await _authService.AuthenticateByWindowsUsernameAsync(windowsUser);

                Model_User? authenticatedUser = null;
                string authMethod = "";

                // If user doesn't exist, create account first (before workstation detection)
                if (!userCheckResult.Success)
                {
                    SetSplashIndeterminate("User account not found. Waiting for account creation...");

                    // Show New User Setup Dialog as child of splash screen
                    var newUserViewModel = _serviceProvider.GetRequiredService<ViewModel_Shared_NewUserSetup>();
                    newUserViewModel.WindowsUsername = windowsUser;
                    newUserViewModel.CreatedBy = windowsUser; // Self-creation (physical presence is authorization)

                    var newUserDialog = new View_Shared_NewUserSetupDialog(newUserViewModel);

                    // Set splash screen as parent
                    if (_splashScreen?.Content is Microsoft.UI.Xaml.UIElement rootElement)
                    {
                        newUserDialog.XamlRoot = rootElement.XamlRoot;
                    }

                    // Show dialog and wait for result
                    var dialogResult = await newUserDialog.ShowAsync();

                    // Check result
                    if (newUserViewModel.NewEmployeeNumber > 0 && !newUserViewModel.IsCancelled)
                    {
                        // Account created successfully - re-authenticate to get full user data
                        UpdateSplash(40, "Employee account created...");
                        userCheckResult = await _authService.AuthenticateByWindowsUsernameAsync(windowsUser);

                        if (userCheckResult.Success)
                        {
                            authenticatedUser = userCheckResult.User;
                            UpdateSplash(45, $"Account created for {authenticatedUser?.FullName}");
                        }
                        else
                        {
                            // Resilience: if the DB lookup doesn't immediately return the user (e.g. mismatch or timing),
                            // continue startup with the known data instead of shutting down.
                            authenticatedUser = new Model_User
                            {
                                EmployeeNumber = newUserViewModel.NewEmployeeNumber,
                                WindowsUsername = windowsUser,
                                FullName = newUserViewModel.FullName,
                                Department = newUserViewModel.Department,
                                Shift = newUserViewModel.Shift,
                                Pin = newUserViewModel.Pin,
                                IsActive = true,
                                VisualUsername = newUserViewModel.ConfigureErpAccess ? newUserViewModel.VisualUsername : null,
                                VisualPassword = newUserViewModel.ConfigureErpAccess ? newUserViewModel.VisualPassword : null
                            };

                            ApplySafeUserDefaults(authenticatedUser);

                            await _errorHandler.HandleErrorAsync(
                                $"User account was created but could not be reloaded by username '{windowsUser}'. Continuing with in-memory user for this session.",
                                Models.Enums.Enum_ErrorSeverity.Warning,
                                showDialog: false);

                            UpdateSplash(45, $"Account created for {authenticatedUser.FullName}");
                        }
                    }
                    else if (newUserViewModel.IsCancelled)
                    {
                        // User cancelled account creation - close app properly
                        await _errorHandler.HandleErrorAsync(
                            "User account creation cancelled. Application closing.",
                            Models.Enums.Enum_ErrorSeverity.Info,
                            showDialog: false);

                        // Close splash screen
                        if (_splashScreen != null)
                        {
                            _splashScreen.IsProgrammaticClose = true;
                            _splashScreen.Close();
                        }

                        // Close main window if it exists
                        App.MainWindow?.Close();

                        // Exit application properly
                        Application.Current.Exit();
                        return;
                    }
                }
                else
                {
                    // User exists - use existing user account
                    authenticatedUser = userCheckResult.User;
                }

                // Ensure first-run never proceeds with missing defaults.
                // Database schema may be mid-migration; apply safe app-level defaults.
                ApplySafeUserDefaults(authenticatedUser);

                // 4. Detect Workstation (50%)
                UpdateSplash(50, "Detecting workstation configuration...");
                var workstationConfig = await _authService.DetectWorkstationTypeAsync();

                // Debug logging
                System.Diagnostics.Debug.WriteLine($"Workstation: {workstationConfig.ComputerName}");
                System.Diagnostics.Debug.WriteLine($"Type: {workstationConfig.WorkstationType}");
                System.Diagnostics.Debug.WriteLine($"Is Shared: {workstationConfig.IsSharedTerminal}");
                System.Diagnostics.Debug.WriteLine($"Is Personal: {workstationConfig.IsPersonalWorkstation}");

                // 5. Determine authentication method based on workstation type (55-80%)
                if (workstationConfig.IsPersonalWorkstation)
                {
                    // Personal workstation - use Windows auto-login
                    UpdateSplash(55, "Personal workstation - Windows authentication");
                    authMethod = "windows_auto";
                    UpdateSplash(80, $"Welcome, {authenticatedUser?.FullName}");
                }
                else if (workstationConfig.IsSharedTerminal)
                {
                    // Shared terminal - require PIN authentication
                    // Clear any pre-authenticated user (from Windows auth) to ensure PIN is required
                    authenticatedUser = null;

                    SetSplashIndeterminate("Shared terminal detected. Waiting for PIN login...");

                    // Show PIN login dialog as child of splash screen
                    var loginViewModel = _serviceProvider.GetRequiredService<ViewModel_Shared_SharedTerminalLogin>();
                    var loginDialog = new View_Shared_SharedTerminalLoginDialog(loginViewModel);

                    // Set splash screen as parent
                    if (_splashScreen?.Content is Microsoft.UI.Xaml.UIElement rootElement)
                    {
                        loginDialog.XamlRoot = rootElement.XamlRoot;
                    }

                    // Show dialog and wait for result
                    var dialogResult = await loginDialog.ShowAsync();

                    // Check result
                    if (loginViewModel.AuthenticatedUser != null && !loginViewModel.IsCancelled && !loginViewModel.IsLockedOut)
                    {
                        // Authentication successful - override with PIN-authenticated user
                        authenticatedUser = loginViewModel.AuthenticatedUser;
                        authMethod = "pin_login";
                        UpdateSplash(80, $"Welcome, {authenticatedUser.FullName}");
                    }
                    else if (loginViewModel.IsLockedOut)
                    {
                        // Maximum attempts exceeded - log and close app
                        await _errorHandler.HandleErrorAsync(
                            "Maximum login attempts exceeded. Application closing for security.",
                            Models.Enums.Enum_ErrorSeverity.Warning,
                            showDialog: false);

                        // Close application
                        Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().UnregisterKey();
                        System.Environment.Exit(0);
                        return;
                    }
                    else if (loginViewModel.IsCancelled)
                    {
                        // User cancelled login - close app
                        await _errorHandler.HandleErrorAsync(
                            "User cancelled login. Application closing.",
                            Models.Enums.Enum_ErrorSeverity.Info,
                            showDialog: false);

                        System.Environment.Exit(0);
                        return;
                    }
                    else
                    {
                        // Fallback for unexpected dialog closure (e.g. Esc key if not handled)
                        await _errorHandler.HandleErrorAsync(
                            "Login dialog closed unexpectedly. Application closing.",
                            Models.Enums.Enum_ErrorSeverity.Info,
                            showDialog: false);

                        System.Environment.Exit(0);
                        return;
                    }
                }

                // 6. Create Session (90%)
                if (authenticatedUser != null)
                {
                    UpdateSplash(90, "Creating user session...");
                    _sessionManager.CreateSession(authenticatedUser, workstationConfig, authMethod);
                    _sessionManager.StartTimeoutMonitoring();

                    // Update MainWindow user display
                    if (App.MainWindow is MainWindow mainWin)
                    {
                        mainWin.DispatcherQueue.TryEnqueue(() => mainWin.UpdateUserDisplay());
                    }

                    var settingsFacade = _serviceProvider.GetService<IService_SettingsCoreFacade>();
                    if (settingsFacade != null)
                    {
                        UpdateSplash(95, "Initializing core settings...");
                        await settingsFacade.InitializeDefaultsAsync(authenticatedUser.EmployeeNumber);
                    }
                }
                else
                {
                    // No authenticated user - should not reach here unless cancelled
                    await _errorHandler.HandleErrorAsync(
                        "Authentication failed. Application closing.",
                        Models.Enums.Enum_ErrorSeverity.Warning,
                        showDialog: false);
                    System.Environment.Exit(0);
                    return;
                }

                // 7. Finalize (100%) - Show main window, hide splash
                UpdateSplash(100, "Ready!");
                await Task.Delay(500);

                // Show main window and hide splash screen
                App.MainWindow?.Activate();
                if (_splashScreen != null)
                {
                    _splashScreen.IsProgrammaticClose = true;
                    _splashScreen.Close();
                }
                _splashScreen = null;

                // CSV reset dialog removed - only appears when user explicitly clicks Reset CSV button
            }
            catch (Exception ex)
            {
                await _errorHandler.HandleErrorAsync("Startup failed", Models.Enums.Enum_ErrorSeverity.Critical, ex);
                if (_splashScreen != null)
                {
                    _splashScreen.IsProgrammaticClose = true;
                    _splashScreen.Close();
                }
                System.Environment.Exit(1);
            }
        }

        private void UpdateSplash(double percentage, string message)
        {
            _splashScreen?.ViewModel.UpdateProgress(percentage, message);
        }

        private void SetSplashIndeterminate(string message)
        {
            _splashScreen?.ViewModel.SetIndeterminate(message);
        }

        private static void ApplySafeUserDefaults(Model_User? user)
        {
            if (user == null)
            {
                return;
            }

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
}


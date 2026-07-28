using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Enums;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_CoreNavigationHub : Page
{
    private readonly IServiceProvider _services;
    private readonly IService_ErrorHandler _errorHandler;
    private readonly IService_LoggingUtility _logger;
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_UserPrivileges _userPrivileges;

    public View_Settings_CoreNavigationHub(
        IServiceProvider services,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_UserSessionManager sessionManager,
        IService_UserPrivileges userPrivileges
    )
    {
        InitializeComponent();
        _services = services;
        _errorHandler = errorHandler;
        _logger = logger;
        _sessionManager = sessionManager;
        _userPrivileges = userPrivileges;
        _logger.LogInfo("View_Settings_CoreNavigationHub initialized", "Settings.Navigation");

        _ = InitializeAccessAsync();
    }

    private async Task InitializeAccessAsync()
    {
        try
        {
            var currentUserId = _sessionManager.CurrentSession?.User?.EmployeeNumber;
            if (
                currentUserId.HasValue
                && (
                    _userPrivileges.IsInitialized is false
                    || _userPrivileges.CurrentUserId != currentUserId.Value
                )
            )
            {
                await _userPrivileges.InitializeAsync(currentUserId.Value);
            }

            SystemSettingsButton.Visibility = _userPrivileges.HasPermissionLevel(
                Enum_SettingsPermissionLevel.Admin
            )
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"Failed to initialize system settings access gate: {ex.Message}",
                "Settings.Navigation"
            );
            SystemSettingsButton.Visibility = Visibility.Collapsed;
        }
    }

    private Frame? GetHostFrame()
    {
        var parent = Parent;
        while (parent != null)
        {
            if (parent is Frame frame)
            {
                return frame;
            }

            parent = (parent as FrameworkElement)?.Parent;
        }

        return null;
    }

    /// <summary>
    /// Navigates to a page by resolving it through the service provider.
    /// This ensures all constructor dependencies (ViewModels, Services) are properly injected.
    /// </summary>
    /// <param name="pageType"></param>
    private void NavigateUsingServiceProvider(Type pageType)
    {
        _logger.LogInfo($"Navigating to page: {pageType.Name}", "Settings.Navigation");
        var frame = GetHostFrame();
        if (frame is null)
        {
            _logger.LogError(
                "Navigation frame not found - cannot navigate",
                null,
                "Settings.Navigation"
            );
            _ = _errorHandler.LogErrorAsync(
                "Navigation frame not found",
                Enum_ErrorSeverity.Warning,
                null
            );
            return;
        }

        try
        {
            var page = ActivatorUtilities.CreateInstance(_services, pageType) as Page;
            if (page is null)
            {
                _logger.LogError(
                    $"Failed to create page instance: {pageType.Name}",
                    null,
                    "Settings.Navigation"
                );
                _ = _errorHandler.LogErrorAsync(
                    $"Failed to create page instance of type {pageType.Name}",
                    Enum_ErrorSeverity.Error,
                    null
                );
                return;
            }

            frame.Content = page;
            _logger.LogInfo($"Successfully navigated to: {pageType.Name}", "Settings.Navigation");
            UpdateParentWindowHeader(pageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Navigation exception for page {pageType.Name}: {ex.Message}",
                ex,
                "Settings.Navigation"
            );
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(NavigateUsingServiceProvider),
                nameof(View_Settings_CoreNavigationHub)
            );
        }
    }

    private void UpdateParentWindowHeader(Type pageType)
    {
        try
        {
            View_Settings_CoreWindow.GetActiveHost()?.UpdateHeaderForPageType(pageType);
        }
        catch { }
    }

    private void OnNavigateUsers(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_Users));
    }

    private void OnNavigateTheme(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_Theme));
    }

    private void OnNavigateSystemSettings(object sender, RoutedEventArgs e)
    {
        if (_userPrivileges.HasPermissionLevel(Enum_SettingsPermissionLevel.Admin) is false)
        {
            _logger.LogWarning(
                "Blocked navigation to System Settings due to insufficient privileges.",
                "Settings.Navigation"
            );
            return;
        }

        NavigateUsingServiceProvider(typeof(View_Settings_System));
    }

    private void OnNavigateSharedPaths(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_SharedPaths));
    }

    private void OnNavigateLabelView(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_LabelViewExecutable));
    }

    private void OnNavigateMaterialAvailabilityFields(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_MaterialAvailabilityBoardFields));
    }
}

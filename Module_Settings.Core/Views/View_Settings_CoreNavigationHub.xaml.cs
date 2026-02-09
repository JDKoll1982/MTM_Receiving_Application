using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using System;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;


public sealed partial class View_Settings_CoreNavigationHub : Page
{
    private readonly IServiceProvider _services;
    private readonly IService_ErrorHandler _errorHandler;
    private readonly IService_LoggingUtility _logger;

    public View_Settings_CoreNavigationHub(
        IServiceProvider services,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
    {
        InitializeComponent();
        _services = services;
        _errorHandler = errorHandler;
        _logger = logger;
        _logger.LogInfo("View_Settings_CoreNavigationHub initialized", "Settings.Navigation");
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
            _logger.LogError("Navigation frame not found - cannot navigate", null, "Settings.Navigation");
            _ = _errorHandler.LogErrorAsync(
                "Navigation frame not found",
                Enum_ErrorSeverity.Warning,
                null);
            return;
        }

        try
        {
            var page = ActivatorUtilities.CreateInstance(_services, pageType) as Page;
            if (page is null)
            {
                _logger.LogError($"Failed to create page instance: {pageType.Name}", null, "Settings.Navigation");
                _ = _errorHandler.LogErrorAsync(
                    $"Failed to create page instance of type {pageType.Name}",
                    Enum_ErrorSeverity.Error,
                    null);
                return;
            }

            frame.Content = page;
            _logger.LogInfo($"Successfully navigated to: {pageType.Name}", "Settings.Navigation");
            UpdateParentWindowHeader(pageType);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Navigation exception for page {pageType.Name}: {ex.Message}", ex, "Settings.Navigation");
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(NavigateUsingServiceProvider),
                nameof(View_Settings_CoreNavigationHub));
        }
    }

    private void UpdateParentWindowHeader(Type pageType)
    {
        try
        {
            var coreWindow = View_Settings_CoreWindow.GetInstance();
            if (coreWindow != null)
            {
                var method = coreWindow.GetType().GetMethod(
                    "UpdateHeaderForPageType",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                method?.Invoke(coreWindow, new object[] { pageType });
            }
        }
        catch { }
    }

    private void OnNavigateSystem(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_System));
    }

    private void OnNavigateUsers(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_Users));
    }

    private void OnNavigateTheme(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_Theme));
    }

    private void OnNavigateDatabase(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_Database));
    }

    private void OnNavigateLogging(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_Logging));
    }

    private void OnNavigateSharedPaths(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_SharedPaths));
    }
}

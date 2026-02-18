using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

/// <summary>
/// Core Settings window shell.
/// </summary>
public sealed partial class View_Settings_CoreWindow : Window
{
    // Static reference to allow child pages to access this window
    private static View_Settings_CoreWindow? _instance;

    public ViewModel_SettingsWindow? ViewModel { get; }
    private bool _hasSetTitleBarDragRegion;
    private bool _isHandlingSelectionChanged;
    private readonly IServiceProvider _serviceProvider;
    private readonly IService_LoggingUtility _logger;

    public View_Settings_CoreWindow(ViewModel_SettingsWindow viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _instance = this;
        ViewModel = viewModel;
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<IService_LoggingUtility>();

        _logger.LogInfo("View_Settings_CoreWindow initialized", "Settings.CoreWindow");

        SettingsFrame.Navigated += OnSettingsFrameNavigated;

        Title = ViewModel.Title;
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 1400, 900);

        ConfigureTitleBar();
        Activated += OnWindowActivated;

        // Subscribe to theme changes to update title bar colors
        if (Content is FrameworkElement contentElement)
        {
            contentElement.ActualThemeChanged += (s, e) => UpdateTitleBarColors();
        }

        SettingsNavView.SelectedItem = SettingsNavView.MenuItems[0];
        NavigateToCoreHub();
        SetHeader("Configuration", "Manage core system defaults, users, and infrastructure settings.");
        UpdateHeaderActions();
    }

    /// <summary>
    /// Gets the current instance of the CoreWindow for use by child pages/NavigationHubs.
    /// </summary>
    public static View_Settings_CoreWindow? GetInstance() => _instance;

    private void OnSettingsFrameNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        _logger.LogInfo("Frame navigation event triggered", "Settings.Navigation");
        UpdateHeaderForCurrentPage();
        UpdateHeaderActions();
    }

    /// <summary>
    /// Updates the header title and description based on the currently displayed page in the SettingsFrame.
    /// This is called automatically when navigating via the Frame.Navigate() event, but must be called manually
    /// when navigating directly via frame.Content assignment (as with DI-based navigation).
    /// </summary>
    public void UpdateHeaderForCurrentPage()
    {
        if (SettingsFrame.Content is null)
        {
            return;
        }

        var pageType = SettingsFrame.Content.GetType();
        var pageName = pageType.Name;

        // Extract friendly name from class name (e.g., "View_Settings_Receiving_Defaults" → "Receiving Defaults")
        var friendlyName = ExtractFriendlyPageName(pageName);
        if (string.IsNullOrEmpty(friendlyName))
        {
            return;
        }

        // Get page-specific header based on type
        var (title, description) = GetPageHeader(pageType);
        if (!string.IsNullOrEmpty(title))
        {
            SetHeader(title, description);
        }
    }

    /// <summary>
    /// Updates the header for a specific page type. Used by NavigationHubs to tell CoreWindow
    /// which child page is currently being displayed (since the SettingsFrame contains the NavigationHub,
    /// not the actual child page).
    /// </summary>
    /// <param name="pageType"></param>
    public void UpdateHeaderForPageType(Type pageType)
    {
        if (pageType is null)
        {
            _logger.LogWarning("UpdateHeaderForPageType called with null pageType", "Settings.Navigation");
            return;
        }

        _logger.LogInfo($"UpdateHeaderForPageType called with: {pageType.Name} (Full: {pageType.FullName})", "Settings.Navigation");

        var (title, description) = GetPageHeader(pageType);

        if (!string.IsNullOrEmpty(title))
        {
            _logger.LogInfo($"Header mapping found - Title: '{title}', Description: '{description}'", "Settings.Navigation");
            SetHeader(title, description);
            _logger.LogInfo($"SetHeader called with: {title}", "Settings.Navigation");
        }
        else
        {
            _logger.LogWarning($"NO HEADER MAPPING FOUND for page type: {pageType.Name} (Full: {pageType.FullName})", "Settings.Navigation");
        }
    }

    /// <summary>
    /// Gets the header title and description for a specific page type.
    /// </summary>
    /// <param name="pageType"></param>
    private static (string Title, string Description) GetPageHeader(Type pageType)
    {
        return pageType.Name switch
        {
            // Receiving Settings Pages
            "View_Settings_Receiving_SettingsOverview" =>
                ("Receiving Settings", "Review the current Receiving configuration and jump to common sections."),
            "View_Settings_Receiving_Defaults" =>
                ("Receiving Defaults", "Set common default values used when creating new loads and packages."),
            "View_Settings_Receiving_Validation" =>
                ("Receiving Validation", "Control required fields and validation rules applied during Receiving."),
            "View_Settings_Receiving_UserPreferences" =>
                ("User Preferences", "Configure how the Receiving workflow behaves for your user account."),
            "View_Settings_Receiving_BusinessRules" =>
                ("Business Rules", "Configure workflow behavior, auto-save, and storage options."),
            "View_Settings_Receiving_Integrations" =>
                ("ERP Integration", "Configure ERP synchronization and automated data pulls."),
            "View_Settings_Receiving_NavigationHub" =>
                ("Receiving Navigation", "Manage Receiving module defaults and configuration pages."),

            // Dunnage Settings Pages
            "View_Settings_Dunnage_SettingsOverview" =>
                ("Dunnage Settings", "Review the current Dunnage configuration and jump to common sections."),
            "View_Settings_Dunnage_UserPreferences" =>
                ("User Preferences", "Configure how the Dunnage workflow behaves for your user account."),
            "View_Settings_Dunnage_UiUx" =>
                ("UI/UX Settings", "Customize the Dunnage user interface and experience."),
            "View_Settings_Dunnage_Workflow" =>
                ("Workflow Settings", "Configure Dunnage workflow behavior and automation."),
            "View_Settings_Dunnage_Permissions" =>
                ("Permissions", "Manage user permissions and access controls."),
            "View_Settings_Dunnage_Audit" =>
                ("Audit Log", "Review system audit logs and activity history."),
            "View_Settings_Dunnage_NavigationHub" =>
                ("Dunnage Navigation", "Manage Dunnage module defaults and configuration pages."),

            // Reporting Settings Pages
            "View_Settings_Reporting_SettingsOverview" =>
                ("Reporting Settings", "Review the current Reporting configuration and jump to common sections."),
            "View_Settings_Reporting_FileIO" =>
                ("File I/O Settings", "Configure file export options and output locations."),
            "View_Settings_Reporting_Csv" =>
                ("Data Export Settings", "Configure data export format and options."),
            "View_Settings_Reporting_EmailUx" =>
                ("Email Settings", "Configure email delivery and formatting options."),
            "View_Settings_Reporting_BusinessRules" =>
                ("Business Rules", "Define business rules for report generation."),
            "View_Settings_Reporting_Permissions" =>
                ("Permissions", "Manage user permissions for reporting features."),
            "View_Settings_Reporting_NavigationHub" =>
                ("Reporting Navigation", "Manage Reporting module defaults and configuration pages."),

            // Volvo Settings Pages
            "View_Settings_Volvo_SettingsOverview" =>
                ("Volvo Settings", "Review the current Volvo configuration and jump to common sections."),
            "View_Settings_Volvo_DatabaseSettings" =>
                ("Database Settings", "Configure Volvo database connection and options."),
            "View_Settings_Volvo_ConnectionStrings" =>
                ("Connection Strings", "Manage database and service connection strings."),
            "View_Settings_Volvo_FilePaths" =>
                ("File Paths", "Configure file locations and paths for Volvo operations."),
            "View_Settings_Volvo_UiConfiguration" =>
                ("UI Configuration", "Customize the Volvo user interface."),
            "View_Settings_Volvo_ExternalizationBacklog" =>
                ("Backlog", "Review pending items for externalization."),
            "View_Settings_Volvo_NavigationHub" =>
                ("Volvo Navigation", "Manage Volvo module defaults and configuration pages."),

            // Developer Tools Pages
            "View_Settings_DeveloperTools_SettingsOverview" =>
                ("Developer Tools", "Access diagnostic and developer utilities."),
            "View_Settings_DeveloperTools_FeatureA" =>
                ("Feature A", "Developer feature A settings."),
            "View_Settings_DeveloperTools_FeatureB" =>
                ("Feature B", "Developer feature B settings."),
            "View_Settings_DeveloperTools_FeatureC" =>
                ("Feature C", "Developer feature C settings."),
            "View_Settings_DeveloperTools_FeatureD" =>
                ("Feature D", "Developer feature D settings."),
            "View_SettingsDeveloperTools_DatabaseTest" =>
                ("Database Test", "Test database connectivity and queries."),
            "View_Settings_DeveloperTools_NavigationHub" =>
                ("Developer Tools", "Access diagnostic and developer utilities."),

            // Core Settings Pages
            "View_Settings_CoreNavigationHub" =>
                ("Configuration", "Manage core system defaults, users, and infrastructure settings."),
            "View_Settings_System" =>
                ("System Settings", "Configure core system settings and defaults."),
            "View_Settings_Users" =>
                ("User Management", "Manage application users and accounts."),
            "View_Settings_Theme" =>
                ("Theme Settings", "Configure application appearance and theme."),
            "View_Settings_Database" =>
                ("Database Settings", "Configure database connections and options."),
            "View_Settings_Logging" =>
                ("Logging Settings", "Configure logging and diagnostic options."),
            "View_Settings_SharedPaths" =>
                ("Shared Paths", "Configure shared file paths and locations."),

            _ => (string.Empty, string.Empty),
        };
    }

    /// <summary>
    /// Extracts a friendly page name from a View class name.
    /// E.g., "View_Settings_Receiving_Defaults" → "Receiving Defaults"
    /// </summary>
    /// <param name="className"></param>
    private static string ExtractFriendlyPageName(string className)
    {
        // Remove "View_Settings_" prefix
        if (className.StartsWith("View_Settings_", StringComparison.Ordinal))
        {
            className = className.Substring("View_Settings_".Length);
        }
        else if (className.StartsWith("View_", StringComparison.Ordinal))
        {
            className = className.Substring("View_".Length);
        }

        // Replace underscores with spaces
        return className.Replace("_", " ");
    }

    private void UpdateHeaderActions()
    {
        var selectedTag = (SettingsNavView.SelectedItem as NavigationViewItem)?.Tag?.ToString();
        if (TryGetModuleContext(selectedTag, out var moduleNamespacePrefix, out var hubViewType) is false)
        {
            BackToHubButton.Visibility = Visibility.Collapsed;
            return;
        }

        if (SettingsFrame.Content is null || hubViewType is null)
        {
            BackToHubButton.Visibility = Visibility.Collapsed;
            return;
        }

        if (SettingsFrame.Content.GetType() == hubViewType)
        {
            BackToHubButton.Visibility = Visibility.Collapsed;
            return;
        }

        if (SettingsFrame.Content is FrameworkElement element
            && element.GetType().Namespace?.StartsWith(moduleNamespacePrefix, StringComparison.Ordinal) == true)
        {
            BackToHubButton.Visibility = Visibility.Visible;
            return;
        }

        BackToHubButton.Visibility = Visibility.Collapsed;
    }

    private static bool TryGetModuleContext(string? selectedTag, out string moduleNamespacePrefix, out Type? hubViewType)
    {
        moduleNamespacePrefix = string.Empty;
        hubViewType = null;

        if (string.IsNullOrWhiteSpace(selectedTag))
        {
            return false;
        }

        switch (selectedTag)
        {
            case "ReceivingSettingsHub":
                moduleNamespacePrefix = "MTM_Receiving_Application.Module_Settings.Receiving.Views";
                hubViewType = typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_NavigationHub);
                return true;
            case "DunnageSettingsHub":
                moduleNamespacePrefix = "MTM_Receiving_Application.Module_Settings.Dunnage.Views";
                hubViewType = typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_NavigationHub);
                return true;
            case "ReportingSettingsHub":
                moduleNamespacePrefix = "MTM_Receiving_Application.Module_Settings.Reporting.Views";
                hubViewType = typeof(Module_Settings.Reporting.Views.View_Settings_Reporting_NavigationHub);
                return true;
            case "VolvoSettingsHub":
                moduleNamespacePrefix = "MTM_Receiving_Application.Module_Settings.Volvo.Views";
                hubViewType = typeof(Module_Settings.Volvo.Views.View_Settings_Volvo_NavigationHub);
                return true;
            case "DeveloperToolsSettingsHub":
                moduleNamespacePrefix = "MTM_Receiving_Application.Module_Settings.DeveloperTools.Views";
                hubViewType = typeof(Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_NavigationHub);
                return true;
            default:
                return false;
        }
    }

    private void OnBackToHubClicked(object sender, RoutedEventArgs e)
    {
        _logger.LogInfo("Back to Hub button clicked", "Settings.Navigation");
        var selectedTag = (SettingsNavView.SelectedItem as NavigationViewItem)?.Tag?.ToString();
        if (!TryGetModuleContext(selectedTag, out _, out var hubViewType) || hubViewType is null)
        {
            _logger.LogWarning($"Failed to navigate back: Invalid module context for {selectedTag}", "Settings.Navigation");
            return;
        }

        // Avoid walking the back stack which can transiently null `Content`.
        // Instead, replace the content with the hub page for the currently selected module.
        SettingsFrame.Content = _serviceProvider.GetRequiredService(hubViewType);
        _logger.LogInfo($"Navigated back to hub: {selectedTag}", "Settings.Navigation");
        UpdateHeaderActions();
    }

    private void NavigateToCoreHub()
    {
        _logger.LogInfo("Navigating to Core Hub", "Settings.Navigation");
        SettingsFrame.Content = _serviceProvider.GetRequiredService<View_Settings_CoreNavigationHub>();
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState != WindowActivationState.Deactivated && !_hasSetTitleBarDragRegion)
        {
            _hasSetTitleBarDragRegion = true;
            SetTitleBarDragRegion();
        }
    }

    private void ConfigureTitleBar()
    {
        try
        {
            if (AppWindow.TitleBar != null)
            {
                AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

                var transparentColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
                AppWindow.TitleBar.ButtonBackgroundColor = transparentColor;
                AppWindow.TitleBar.ButtonInactiveBackgroundColor = transparentColor;

                // Set button foreground colors based on current theme
                UpdateTitleBarColors();
            }

            if (AppTitleBar != null)
            {
                SetTitleBar(AppTitleBar);
            }
        }
        catch
        {
            // Ignore title bar customization errors
        }
    }

    /// <summary>
    /// Updates title bar button colors based on the current theme.
    /// Called during initialization and when theme changes.
    /// </summary>
    private void UpdateTitleBarColors()
    {
        if (AppWindow.TitleBar is null)
        {
            return;
        }

        // Determine if we're in light or dark mode
        var isDarkMode = (Content as FrameworkElement)?.ActualTheme == Microsoft.UI.Xaml.ElementTheme.Dark;

        if (isDarkMode)
        {
            // Dark mode - use light/white buttons
            var foregroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
            AppWindow.TitleBar.ButtonForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonHoverForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonPressedForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255, 160, 160, 160);
        }
        else
        {
            // Light mode - use dark/black buttons
            var foregroundColor = Windows.UI.Color.FromArgb(255, 0, 0, 0);
            AppWindow.TitleBar.ButtonForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonHoverForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonPressedForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255, 96, 96, 96);
        }
    }

    private void SetTitleBarDragRegion()
    {
        try
        {
            if (AppWindow.TitleBar is not null && AppTitleBar?.XamlRoot is not null)
            {
                var scale = AppTitleBar.XamlRoot.RasterizationScale;
                var titleBarHeight = (int)(AppTitleBar.ActualHeight * scale);

                var dragRect = new Windows.Graphics.RectInt32
                {
                    X = 0,
                    Y = 0,
                    Width = (int)(AppTitleBar.ActualWidth * scale),
                    Height = titleBarHeight
                };

                AppWindow.TitleBar.SetDragRectangles(new[] { dragRect });
            }
        }
        catch
        {
            // Ignore drag region setup errors
        }
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_isHandlingSelectionChanged)
        {
            return;
        }

        _isHandlingSelectionChanged = true;
        try
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                var selectedModule = item.Tag?.ToString() ?? "Unknown";
                _logger.LogInfo($"Navigation selection changed to: {selectedModule}", "Settings.Navigation");

                switch (item.Tag?.ToString())
                {
                    case "CoreSettingsHub":
                        SettingsFrame.Content = _serviceProvider.GetRequiredService<View_Settings_CoreNavigationHub>();
                        SetHeader("Configuration", "Manage core system defaults, users, and infrastructure settings.");
                        UpdateHeaderActions();
                        break;
                    case "ReceivingSettingsHub":
                        SettingsFrame.Content = _serviceProvider.GetRequiredService<Module_Settings.Receiving.Views.View_Settings_Receiving_NavigationHub>();
                        SetHeader("Receiving Navigation", "Manage Receiving module defaults and configuration pages.");
                        UpdateHeaderActions();
                        break;
                    case "DunnageSettingsHub":
                        SettingsFrame.Content = _serviceProvider.GetRequiredService<Module_Settings.Dunnage.Views.View_Settings_Dunnage_NavigationHub>();
                        SetHeader("Dunnage Navigation", "Manage Dunnage module defaults and configuration pages.");
                        UpdateHeaderActions();
                        break;
                    case "ReportingSettingsHub":
                        SettingsFrame.Content = _serviceProvider.GetRequiredService<Module_Settings.Reporting.Views.View_Settings_Reporting_NavigationHub>();
                        SetHeader("Reporting Navigation", "Manage Reporting module defaults and configuration pages.");
                        UpdateHeaderActions();
                        break;
                    case "VolvoSettingsHub":
                        SettingsFrame.Content = _serviceProvider.GetRequiredService<Module_Settings.Volvo.Views.View_Settings_Volvo_NavigationHub>();
                        SetHeader("Volvo Navigation", "Manage Volvo module defaults and configuration pages.");
                        UpdateHeaderActions();
                        break;
                    case "DeveloperToolsSettingsHub":
                        SettingsFrame.Content = _serviceProvider.GetRequiredService<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_NavigationHub>();
                        SetHeader("Developer Tools", "Access diagnostic and developer utilities.");
                        UpdateHeaderActions();
                        break;
                }
            }
        }
        finally
        {
            _isHandlingSelectionChanged = false;
        }
    }

    private void SetHeader(string title, string description)
    {
        HeaderTitleText.Text = title;
        HeaderDescriptionText.Text = description;
    }
}

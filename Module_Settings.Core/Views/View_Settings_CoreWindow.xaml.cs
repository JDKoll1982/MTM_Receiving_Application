using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

/// <summary>
/// Core Settings window shell.
/// </summary>
public sealed partial class View_Settings_CoreWindow : Window
{
    public ViewModel_SettingsWindow? ViewModel { get; }
    private bool _hasSetTitleBarDragRegion;
    private bool _isHandlingSelectionChanged;
    private readonly IServiceProvider _serviceProvider;

    public View_Settings_CoreWindow(
        ViewModel_SettingsWindow viewModel,
        IServiceProvider serviceProvider
    )
    {
        InitializeComponent();
        ViewModel = viewModel;
        _serviceProvider = serviceProvider;

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
        SetHeader(
            "Configuration",
            "Manage core system defaults, users, and infrastructure settings."
        );
        UpdateHeaderActions();
    }

    private void OnSettingsFrameNavigated(
        object sender,
        Microsoft.UI.Xaml.Navigation.NavigationEventArgs e
    )
    {
        UpdateHeaderActions();
    }

    private void UpdateHeaderActions()
    {
        var selectedTag = (SettingsNavView.SelectedItem as NavigationViewItem)?.Tag?.ToString();
        if (
            TryGetModuleContext(selectedTag, out var moduleNamespacePrefix, out var hubViewType)
            is false
        )
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

        if (
            SettingsFrame.Content is FrameworkElement element
            && element
                .GetType()
                .Namespace?.StartsWith(moduleNamespacePrefix, StringComparison.Ordinal) == true
        )
        {
            BackToHubButton.Visibility = Visibility.Visible;
            return;
        }

        BackToHubButton.Visibility = Visibility.Collapsed;
    }

    private static bool TryGetModuleContext(
        string? selectedTag,
        out string moduleNamespacePrefix,
        out Type? hubViewType
    )
    {
        moduleNamespacePrefix = string.Empty;
        hubViewType = null;

        if (string.IsNullOrWhiteSpace(selectedTag))
        {
            return false;
        }

        switch (selectedTag)
        {
            // TODO: Re-enable when Receiving module settings are available
            //case "ReceivingSettingsHub":
            //    moduleNamespacePrefix = "MTM_Receiving_Application.Module_Settings.Receiving.Views";
            //    hubViewType = typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_NavigationHub);
            //    return true;
            case "DunnageSettingsHub":
                moduleNamespacePrefix = "MTM_Receiving_Application.Module_Settings.Dunnage.Views";
                hubViewType =
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_NavigationHub);
                return true;
            case "RoutingSettingsHub":
                moduleNamespacePrefix = "MTM_Receiving_Application.Module_Settings.Routing.Views";
                hubViewType =
                    typeof(Module_Settings.Routing.Views.View_Settings_Routing_NavigationHub);
                return true;
            case "ReportingSettingsHub":
                moduleNamespacePrefix = "MTM_Receiving_Application.Module_Settings.Reporting.Views";
                hubViewType =
                    typeof(Module_Settings.Reporting.Views.View_Settings_Reporting_NavigationHub);
                return true;
            case "VolvoSettingsHub":
                moduleNamespacePrefix = "MTM_Receiving_Application.Module_Settings.Volvo.Views";
                hubViewType = typeof(Module_Settings.Volvo.Views.View_Settings_Volvo_NavigationHub);
                return true;
            case "DeveloperToolsSettingsHub":
                moduleNamespacePrefix =
                    "MTM_Receiving_Application.Module_Settings.DeveloperTools.Views";
                hubViewType =
                    typeof(Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_NavigationHub);
                return true;
            default:
                return false;
        }
    }

    private void OnBackToHubClicked(object sender, RoutedEventArgs e)
    {
        var selectedTag = (SettingsNavView.SelectedItem as NavigationViewItem)?.Tag?.ToString();
        if (!TryGetModuleContext(selectedTag, out _, out var hubViewType) || hubViewType is null)
        {
            return;
        }

        // Avoid walking the back stack which can transiently null `Content`.
        // Instead, replace the content with the hub page for the currently selected module.
        SettingsFrame.Content = _serviceProvider.GetRequiredService(hubViewType);
        UpdateHeaderActions();
    }

    private void NavigateToCoreHub()
    {
        SettingsFrame.Content =
            _serviceProvider.GetRequiredService<View_Settings_CoreNavigationHub>();
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (
            args.WindowActivationState != WindowActivationState.Deactivated
            && !_hasSetTitleBarDragRegion
        )
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
                AppWindow.TitleBar.PreferredHeightOption = Microsoft
                    .UI
                    .Windowing
                    .TitleBarHeightOption
                    .Tall;

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
        var isDarkMode =
            (Content as FrameworkElement)?.ActualTheme == Microsoft.UI.Xaml.ElementTheme.Dark;

        if (isDarkMode)
        {
            // Dark mode - use light/white buttons
            var foregroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
            AppWindow.TitleBar.ButtonForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonHoverForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonPressedForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(
                255,
                160,
                160,
                160
            );
        }
        else
        {
            // Light mode - use dark/black buttons
            var foregroundColor = Windows.UI.Color.FromArgb(255, 0, 0, 0);
            AppWindow.TitleBar.ButtonForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonHoverForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonPressedForegroundColor = foregroundColor;
            AppWindow.TitleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(
                255,
                96,
                96,
                96
            );
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
                    Height = titleBarHeight,
                };

                AppWindow.TitleBar.SetDragRectangles(new[] { dragRect });
            }
        }
        catch
        {
            // Ignore drag region setup errors
        }
    }

    private void OnSelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args
    )
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
                switch (item.Tag?.ToString())
                {
                    case "CoreSettingsHub":
                        SettingsFrame.Content =
                            _serviceProvider.GetRequiredService<View_Settings_CoreNavigationHub>();
                        SetHeader(
                            "Configuration",
                            "Manage core system defaults, users, and infrastructure settings."
                        );
                        UpdateHeaderActions();
                        break;
                    // TODO: Re-enable when Receiving module settings are available
                    //case "ReceivingSettingsHub":
                    //    SettingsFrame.Content = _serviceProvider.GetRequiredService<Module_Settings.Receiving.Views.View_Settings_Receiving_NavigationHub>();
                    //    SetHeader("Receiving Navigation", "Manage Receiving module defaults and configuration pages.");
                    //    UpdateHeaderActions();
                    //    break;
                    case "DunnageSettingsHub":
                        SettingsFrame.Content =
                            _serviceProvider.GetRequiredService<Module_Settings.Dunnage.Views.View_Settings_Dunnage_NavigationHub>();
                        SetHeader(
                            "Dunnage Navigation",
                            "Manage Dunnage module defaults and configuration pages."
                        );
                        UpdateHeaderActions();
                        break;
                    case "RoutingSettingsHub":
                        SettingsFrame.Content =
                            _serviceProvider.GetRequiredService<Module_Settings.Routing.Views.View_Settings_Routing_NavigationHub>();
                        SetHeader(
                            "Routing Navigation",
                            "Manage Routing module defaults and configuration pages."
                        );
                        UpdateHeaderActions();
                        break;
                    case "ReportingSettingsHub":
                        SettingsFrame.Content =
                            _serviceProvider.GetRequiredService<Module_Settings.Reporting.Views.View_Settings_Reporting_NavigationHub>();
                        SetHeader(
                            "Reporting Navigation",
                            "Manage Reporting module defaults and configuration pages."
                        );
                        UpdateHeaderActions();
                        break;
                    case "VolvoSettingsHub":
                        SettingsFrame.Content =
                            _serviceProvider.GetRequiredService<Module_Settings.Volvo.Views.View_Settings_Volvo_NavigationHub>();
                        SetHeader(
                            "Volvo Navigation",
                            "Manage Volvo module defaults and configuration pages."
                        );
                        UpdateHeaderActions();
                        break;
                    case "DeveloperToolsSettingsHub":
                        SettingsFrame.Content =
                            _serviceProvider.GetRequiredService<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_NavigationHub>();
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

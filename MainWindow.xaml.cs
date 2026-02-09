using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using System;
using Microsoft.UI.Windowing;
using Microsoft.UI.Input;
using Windows.Graphics;
using Microsoft.Extensions.DependencyInjection;

namespace MTM_Receiving_Application
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ViewModel_Shared_MainWindow ViewModel { get; }
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_LoggingUtility _logger;
        private readonly IService_SettingsWindowHost _settingsWindowHost;
        private readonly IServiceProvider _serviceProvider;
        private bool _hasNavigatedOnStartup = false;

        /// <summary>
        /// Gets the main content frame for navigation
        /// </summary>
        public Frame GetContentFrame() => ContentFrame;

        public MainWindow(
            ViewModel_Shared_MainWindow viewModel,
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger,
            IService_SettingsWindowHost settingsWindowHost,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();
            ViewModel = viewModel;
            _sessionManager = sessionManager;
            _logger = logger;
            _settingsWindowHost = settingsWindowHost;
            _serviceProvider = serviceProvider;

            // Configure Frame to use DI for view activation
            ContentFrame.NavigationFailed += ContentFrame_NavigationFailed;

            // Set initial window size (1450x900 to accommodate wide data grids and toolbars)
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1450, 900));

            // Center window on screen
            CenterWindow();

            // Configure custom title bar
            ConfigureTitleBar();

            // Set window icon
            SetWindowIcon();

            // Set user display from current session
            if (_sessionManager.CurrentSession?.User != null)
            {
                var user = _sessionManager.CurrentSession.User;
                UserDisplayTextBlock.Text = user.DisplayName;
                UserPicture.DisplayName = user.DisplayName;
            }

            // Wire up activity tracking
            if (Content is UIElement rootElement)
            {
                rootElement.PointerMoved += (s, e) => _sessionManager.UpdateLastActivity();
                rootElement.KeyDown += (s, e) => _sessionManager.UpdateLastActivity();
            }

            // Subscribe to theme changes to update title bar colors
            if (Content is FrameworkElement contentElement)
            {
                contentElement.ActualThemeChanged += (s, e) => UpdateTitleBarColors();
            }

            this.Activated += MainWindow_Activated;
            this.Closed += MainWindow_Closed;

            // Subscribe to navigation events once
            ContentFrame.Navigated += ContentFrame_Navigated;

            // Wire up title bar events
            AppTitleBar.Loaded += AppTitleBar_Loaded;
            AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;

            // Show development tools in debug builds
#if DEBUG
            DatabaseTestMenuItem.Visibility = Visibility.Visible;
#endif
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // Ensure the application process terminates when the main window is closed
            Application.Current.Exit();
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            // Update title bar text color based on activation state
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                TitleBarTextBlock.Foreground =
                    (Microsoft.UI.Xaml.Media.SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
            }
            else
            {
                TitleBarTextBlock.Foreground =
                    (Microsoft.UI.Xaml.Media.SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
            }

            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                _sessionManager.UpdateLastActivity();

                // Navigate to Receiving workflow on first activation
                if (!_hasNavigatedOnStartup)
                {
                    _hasNavigatedOnStartup = true;
                    // Title will be set by ContentFrame_Navigated
                    NavigateWithDI(typeof(Module_Receiving.Views.View_Receiving_Workflow));
                }
            }
        }

        /// <summary>
        /// Handle pane toggle button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PaneToggleButton_Click(object sender, RoutedEventArgs e)
        {
            NavView.IsPaneOpen = !NavView.IsPaneOpen;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                PageTitleTextBlock.Text = "Settings";
                _settingsWindowHost.ShowSettingsWindow();
            }
            else if (args.SelectedItem is NavigationViewItem item)
            {
                var tag = item.Tag?.ToString();
                switch (tag)
                {
                    case "ReceivingWorkflowView":
                        // Title will be set by ContentFrame_Navigated
                        NavigateWithDI(typeof(Module_Receiving.Views.View_Receiving_Workflow));
                        break;
                    case "DunnageLabelPage":
                        // Title will be set by ContentFrame_Navigated
                        ContentFrame.Navigate(typeof(Module_Dunnage.Views.View_Dunnage_WorkflowView));
                        break;
                    case "VolvoShipmentEntry":
                        PageTitleTextBlock.Text = "Volvo Dunnage Requisition";
                        ContentFrame.Navigate(typeof(Module_Volvo.Views.View_Volvo_ShipmentEntry));
                        break;
                    case "VolvoHistory":
                        PageTitleTextBlock.Text = "Volvo Shipment History";
                        ContentFrame.Navigate(typeof(Module_Volvo.Views.View_Volvo_History));
                        break;
                    case "ReportingMainPage":
                        PageTitleTextBlock.Text = "End of Day Reports";
                        ContentFrame.Navigate(typeof(Module_Reporting.Views.View_Reporting_Main));
                        break;
                    case "DatabaseTestView":
                        try
                        {
                            _logger.LogInfo("Navigating to Settings Database Test view", "MainWindow");

                            // Defensive checks
                            if (ContentFrame == null)
                            {
                                _logger.LogError("ContentFrame is null - cannot navigate", null, "MainWindow");
                                throw new InvalidOperationException("ContentFrame is null");
                            }

                            _logger.LogInfo($"ContentFrame state: IsLoaded={ContentFrame.IsLoaded}, BackStackDepth={ContentFrame.BackStackDepth}", "MainWindow");

                            var viewType = typeof(Module_Settings.DeveloperTools.Views.View_SettingsDeveloperTools_DatabaseTest);
                            _logger.LogInfo($"View type: {viewType.FullName}", "MainWindow");

                            PageTitleTextBlock.Text = "Settings Database Test";

                            // Try navigation with detailed logging
                            _logger.LogInfo("Calling ContentFrame.Navigate()", "MainWindow");
                            var navigated = ContentFrame.Navigate(viewType);
                            _logger.LogInfo($"Navigation result: {navigated}", "MainWindow");

                            if (!navigated)
                            {
                                _logger.LogError("Navigation returned false", null, "MainWindow");
                            }
                            else
                            {
                                _logger.LogInfo("Successfully navigated to Settings Database Test view", "MainWindow");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Failed to navigate to Settings Database Test view: {ex.Message}", ex, "MainWindow");

                            // Build detailed error message
                            var errorDetails = new System.Text.StringBuilder();
                            errorDetails.AppendLine("Failed to load Settings Database Test view:");
                            errorDetails.AppendLine();
                            errorDetails.AppendLine($"Error Type: {ex.GetType().FullName}");
                            errorDetails.AppendLine($"Message: {ex.Message}");
                            errorDetails.AppendLine();
                            errorDetails.AppendLine("Stack Trace:");
                            errorDetails.AppendLine(ex.StackTrace ?? "No stack trace available");

                            // Add inner exception details if present
                            if (ex.InnerException != null)
                            {
                                errorDetails.AppendLine();
                                errorDetails.AppendLine("Inner Exception:");
                                errorDetails.AppendLine($"Type: {ex.InnerException.GetType().FullName}");
                                errorDetails.AppendLine($"Message: {ex.InnerException.Message}");
                                errorDetails.AppendLine($"Stack: {ex.InnerException.StackTrace ?? "No stack trace"}");
                            }

                            // Show detailed error to user in scrollable view
                            var scrollViewer = new ScrollViewer
                            {
                                Content = new TextBlock
                                {
                                    Text = errorDetails.ToString(),
                                    TextWrapping = TextWrapping.Wrap,
                                    FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas"),
                                    FontSize = 12,
                                    IsTextSelectionEnabled = true
                                },
                                MaxHeight = 500,
                                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                            };

                            var errorDialog = new ContentDialog
                            {
                                Title = "Navigation Error - Detailed Information",
                                Content = scrollViewer,
                                CloseButtonText = "OK",
                                XamlRoot = this.Content.XamlRoot
                            };
                            _ = errorDialog.ShowAsync();
                        }
                        break;

                }
            }
        }

        private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            // If navigated to ReceivingWorkflowView, subscribe to ViewModel changes to update header
            if (ContentFrame.Content is Module_Receiving.Views.View_Receiving_Workflow receivingView)
            {
                var viewModel = receivingView.ViewModel;
                if (viewModel != null)
                {
                    // Update header with current step title (ensure UI thread)
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        PageTitleTextBlock.Text = viewModel.CurrentStepTitle;
                    });

                    // Subscribe to property changes to keep header updated
                    viewModel.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == nameof(viewModel.CurrentStepTitle))
                        {
                            // Ensure UI update happens on UI thread
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                PageTitleTextBlock.Text = viewModel.CurrentStepTitle;
                            });
                        }
                    };
                }
            }
            // If navigated to DunnageWorkflowView, subscribe to ViewModel changes to update header
            else if (ContentFrame.Content is Module_Dunnage.Views.View_Dunnage_WorkflowView dunnageView)
            {
                var viewModel = dunnageView.ViewModel;
                if (viewModel != null)
                {
                    // Update header with current step title (ensure UI thread)
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        PageTitleTextBlock.Text = viewModel.CurrentStepTitle;
                    });

                    // Subscribe to property changes to keep header updated
                    viewModel.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == nameof(viewModel.CurrentStepTitle))
                        {
                            // Ensure UI update happens on UI thread
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                PageTitleTextBlock.Text = viewModel.CurrentStepTitle;
                            });
                        }
                    };
                }
            }
            // Settings Window runs in a separate window (no ContentFrame integration).
        }

        /// <summary>
        /// Update user display from current session
        /// Call this after session is created during startup
        /// </summary>
        public void UpdateUserDisplay()
        {
            if (_sessionManager.CurrentSession?.User != null)
            {
                var user = _sessionManager.CurrentSession.User;
                UserDisplayTextBlock.Text = user.DisplayName;
                UserPicture.DisplayName = user.DisplayName;
            }
        }

        /// <summary>
        /// Center the window on the primary display
        /// </summary>
        private void CenterWindow()
        {
            var displayArea = Microsoft.UI.Windowing.DisplayArea.Primary;
            var workArea = displayArea.WorkArea;

            var centerX = (workArea.Width - 1450) / 2;
            var centerY = (workArea.Height - 900) / 2;

            AppWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
        }

        /// <summary>
        /// Configure custom title bar according to Windows App SDK best practices
        /// </summary>
        private void ConfigureTitleBar()
        {
            // Hide the default system title bar and extend content into the title bar area
            ExtendsContentIntoTitleBar = true;

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = AppWindow.TitleBar;

                // Set title bar to tall mode for better touch interaction
                titleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

                // Make caption buttons transparent to show Mica backdrop
                var transparentColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
                titleBar.ButtonBackgroundColor = transparentColor;
                titleBar.ButtonInactiveBackgroundColor = transparentColor;

                // Set button foreground colors based on current theme
                UpdateTitleBarColors();
            }
        }

        /// <summary>
        /// Updates title bar button colors based on the current theme.
        /// Called during initialization and when theme changes.
        /// </summary>
        private void UpdateTitleBarColors()
        {
            if (!AppWindowTitleBar.IsCustomizationSupported())
            {
                return;
            }

            var titleBar = AppWindow.TitleBar;

            // Determine if we're in light or dark mode
            // Check the root element's ActualTheme (works for both system and app-level theme)
            var isDarkMode = (Content as FrameworkElement)?.ActualTheme == ElementTheme.Dark;

            if (isDarkMode)
            {
                // Dark mode - use light/white buttons
                var foregroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
                titleBar.ButtonForegroundColor = foregroundColor;
                titleBar.ButtonHoverForegroundColor = foregroundColor;
                titleBar.ButtonPressedForegroundColor = foregroundColor;
                titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255, 160, 160, 160);
            }
            else
            {
                // Light mode - use dark/black buttons
                var foregroundColor = Windows.UI.Color.FromArgb(255, 0, 0, 0);
                titleBar.ButtonForegroundColor = foregroundColor;
                titleBar.ButtonHoverForegroundColor = foregroundColor;
                titleBar.ButtonPressedForegroundColor = foregroundColor;
                titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255, 96, 96, 96);
            }
        }

        /// <summary>
        /// Set the custom window icon
        /// </summary>
        private void SetWindowIcon()
        {
            try
            {
                // Try multiple icon paths
                var iconPaths = new[]
                {
                    System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "MTMIcon.ico"),
                    System.IO.Path.Combine(AppContext.BaseDirectory, "MTMIcon.ico"),
                    "Assets/MTMIcon.ico",
                    "MTMIcon.ico"
                };

                string? foundIconPath = null;
                foreach (var path in iconPaths)
                {
                    if (System.IO.File.Exists(path))
                    {
                        foundIconPath = path;
                        break;
                    }
                }

                if (foundIconPath != null)
                {
                    AppWindow.SetIcon(foundIconPath);
                    _logger?.LogInfo($"Window icon set successfully: {foundIconPath}", "MainWindow");
                }
                else
                {
                    _logger?.LogWarning($"Icon file not found. Searched paths: {string.Join(", ", iconPaths)}", "MainWindow");
                    _logger?.LogWarning($"Current directory: {AppContext.BaseDirectory}", "MainWindow");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Failed to set window icon: {ex.Message}", ex, "MainWindow");
            }
        }

        /// <summary>
        /// Called when the AppTitleBar element is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (ExtendsContentIntoTitleBar)
            {
                // Set the initial interactive regions
                SetRegionsForCustomTitleBar();
            }
        }

        /// <summary>
        /// Called when the AppTitleBar element size changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppTitleBar_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
        {
            if (ExtendsContentIntoTitleBar)
            {
                // Update interactive regions if the size of the window changes
                SetRegionsForCustomTitleBar();
            }
        }

        /// <summary>
        /// Define drag regions and interactive areas for the custom title bar
        /// </summary>
        private void SetRegionsForCustomTitleBar()
        {
            try
            {
                // Ensure XamlRoot is available
                if (AppTitleBar?.XamlRoot == null)
                {
                    return;
                }

                double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;

                // Set padding columns for caption buttons
                RightPaddingColumn.Width = new Microsoft.UI.Xaml.GridLength(AppWindow.TitleBar.RightInset / scaleAdjustment);
                LeftPaddingColumn.Width = new Microsoft.UI.Xaml.GridLength(AppWindow.TitleBar.LeftInset / scaleAdjustment);

                // Define passthrough regions for interactive elements
                var rectArray = new System.Collections.Generic.List<RectInt32>();

                // Add PaneToggleButton region
                if (PaneToggleButton != null)
                {
                    var transform = PaneToggleButton.TransformToVisual(null);
                    var bounds = transform.TransformBounds(new Windows.Foundation.Rect(0, 0,
                                                                 PaneToggleButton.ActualWidth,
                                                                 PaneToggleButton.ActualHeight));
                    rectArray.Add(GetRect(bounds, scaleAdjustment));
                }

                // Add AutoSuggestBox region
                if (TitleBarSearchBox != null)
                {
                    var transform = TitleBarSearchBox.TransformToVisual(null);
                    var bounds = transform.TransformBounds(new Windows.Foundation.Rect(0, 0,
                                                                 TitleBarSearchBox.ActualWidth,
                                                                 TitleBarSearchBox.ActualHeight));
                    rectArray.Add(GetRect(bounds, scaleAdjustment));
                }

                // Set the interactive regions
                if (rectArray.Count > 0)
                {
                    InputNonClientPointerSource nonClientInputSrc =
                        InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
                    nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray.ToArray());
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to set title bar regions: {ex.Message}", "MainWindow");
            }
        }

        /// <summary>
        /// Helper method to convert bounds to RectInt32 with scale adjustment
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="scale"></param>
        private RectInt32 GetRect(Windows.Foundation.Rect bounds, double scale)
        {
            return new RectInt32(
                _X: (int)Math.Round(bounds.X * scale),
                _Y: (int)Math.Round(bounds.Y * scale),
                _Width: (int)Math.Round(bounds.Width * scale),
                _Height: (int)Math.Round(bounds.Height * scale)
            );
        }

        /// <summary>
        /// Navigate to a page type using dependency injection for view instantiation
        /// </summary>
        /// <param name="pageType"></param>
        private bool NavigateWithDI(Type pageType)
        {
            try
            {
                // Resolve the view from DI container
                var page = _serviceProvider.GetService(pageType);
                if (page == null)
                {
                    _logger.LogError($"Failed to resolve view type: {pageType.Name}", null, "MainWindow");
                    return false;
                }

                // Set the content directly instead of using Navigate
                ContentFrame.Content = page;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Navigation failed for {pageType.Name}: {ex.Message}", ex, "MainWindow");
                return false;
            }
        }

        /// <summary>
        /// Handle navigation failures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentFrame_NavigationFailed(object sender, Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e)
        {
            _logger.LogError($"Navigation failed: {e.Exception?.Message}", e.Exception, "MainWindow");
            e.Handled = true;

            // Try to resolve using DI as fallback
            if (e.SourcePageType != null)
            {
                NavigateWithDI(e.SourcePageType);
            }
        }
    }

}


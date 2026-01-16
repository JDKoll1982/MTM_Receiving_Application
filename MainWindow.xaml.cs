using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using System;

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
        private bool _hasNavigatedOnStartup = false;

        /// <summary>
        /// Gets the main content frame for navigation
        /// </summary>
        public Frame GetContentFrame() => ContentFrame;

        public MainWindow(
            ViewModel_Shared_MainWindow viewModel,
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger)
        {
            InitializeComponent();
            ViewModel = viewModel;
            _sessionManager = sessionManager;
            _logger = logger;

            // Set initial window size (1450x900 to accommodate wide data grids and toolbars)
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1450, 900));

            // Center window on screen
            CenterWindow();

            // Configure title bar to blend with UI (drag region will be set on first activation)
            ConfigureTitleBar();

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

            this.Activated += MainWindow_Activated;
            this.Closed += MainWindow_Closed;

            // Subscribe to navigation events once
            ContentFrame.Navigated += ContentFrame_Navigated;

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
            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                _sessionManager.UpdateLastActivity();

                // Set title bar drag region on first activation (XamlRoot will be available)
                SetTitleBarDragRegion();

                // Navigate to Receiving workflow on first activation
                if (!_hasNavigatedOnStartup)
                {
                    _hasNavigatedOnStartup = true;
                    // Title will be set by ContentFrame_Navigated
                    ContentFrame.Navigate(typeof(Module_Receiving.Views.View_Receiving_Workflow));
                }
            }
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                PageTitleTextBlock.Text = "Settings";
                // TODO: Restore when Settings Views are implemented
                // ContentFrame.Navigate(typeof(Module_Settings.Views.View_Settings_Workflow));
            }
            else if (args.SelectedItem is NavigationViewItem item)
            {
                var tag = item.Tag?.ToString();
                switch (tag)
                {
                    case "ReceivingWorkflowView":
                        // Title will be set by ContentFrame_Navigated
                        ContentFrame.Navigate(typeof(Module_Receiving.Views.View_Receiving_Workflow));
                        break;
                    case "DunnageLabelPage":
                        // Title will be set by ContentFrame_Navigated
                        ContentFrame.Navigate(typeof(Module_Dunnage.Views.View_Dunnage_WorkflowView));
                        break;
                    case "RoutingLabelPage":
                        // Title will be set by ContentFrame_Navigated
                        ContentFrame.Navigate(typeof(Module_Routing.Views.RoutingModeSelectionView));
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

                            var viewType = typeof(Module_Settings.Views.View_Settings_DatabaseTest);
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
            // If navigated to RoutingModeSelectionView, set simple title
            else if (ContentFrame.Content is Module_Routing.Views.RoutingModeSelectionView)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    PageTitleTextBlock.Text = "Internal Routing";
                });
            }
            // TODO: Restore when Settings Views are implemented
            /*
            // If navigated to SettingsWorkflowView, subscribe to ViewModel changes to update header
            else if (ContentFrame.Content is Module_Settings.Views.View_Settings_Workflow settingsView)
            {
                var viewModel = settingsView.ViewModel;
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
            */
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
        /// Configure title bar to blend with the application UI
        /// </summary>
        private void ConfigureTitleBar()
        {
            try
            {
                if (AppWindow.TitleBar != null)
                {
                    // Make title bar transparent to blend with Mica backdrop
                    AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;

                    // Set the title bar drag region to the AppTitleBar element
                    AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

                    // Set button colors to match theme
                    var transparentColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);

                    // Button colors (will use theme colors)
                    AppWindow.TitleBar.ButtonBackgroundColor = transparentColor;
                    AppWindow.TitleBar.ButtonInactiveBackgroundColor = transparentColor;

                    // Foreground colors for buttons
                    var foregroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
                    AppWindow.TitleBar.ButtonForegroundColor = foregroundColor;
                    AppWindow.TitleBar.ButtonHoverForegroundColor = foregroundColor;
                    AppWindow.TitleBar.ButtonPressedForegroundColor = foregroundColor;

                    // Drag region will be set after window loads (in MainWindow_Loaded)
                }
            }
            catch
            {
                // Ignore title bar customization errors
            }
        }

        /// <summary>
        /// Set the drag region for the custom title bar
        /// MUST be called after window is fully loaded (XamlRoot will be available)
        /// </summary>
        private void SetTitleBarDragRegion()
        {
            try
            {
                if (AppWindow.TitleBar != null && AppTitleBar != null && AppTitleBar.XamlRoot != null)
                {
                    // Get the title bar height
                    var scale = AppTitleBar.XamlRoot.RasterizationScale;
                    var titleBarHeight = (int)(AppTitleBar.ActualHeight * scale);

                    // The entire AppTitleBar is draggable
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
    }
}


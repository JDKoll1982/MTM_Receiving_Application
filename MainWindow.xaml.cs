using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Material.Icons;
using Material.Icons.WinUI3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Helpers;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.Views;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using Windows.Graphics;

namespace MTM_Receiving_Application
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window, ISettingsNavigationHost
    {
        public ViewModel_Shared_MainWindow ViewModel { get; }
        private readonly IService_UserSessionManager _sessionManager;
        private readonly IService_LoggingUtility _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IService_LabelViewLauncher _labelViewLauncher;
        private readonly IService_SettingsUserLabelButtons _labelButtonSettings;
        private readonly IService_ErrorHandler _errorHandler;
        private readonly IService_HeaderBackNavigation _headerBackNavigation;
        private const int LabelButtonsPerPage = 4;
        private readonly List<Model_MainWindowLabelButton> _allLabelButtons = new();
        private readonly List<Model_MainWindowLabelButton> _configuredLabelButtons = new();
        private readonly List<object> _applicationMenuItems = new();
        private readonly List<object> _applicationFooterItems = new();
        private readonly List<object> _settingsMenuItems = new();
        private bool _hasNavigatedOnStartup = false;
        private bool _isWindowActive = true;
        private bool _isUpdatingNavSelection;
        private bool _isSettingsMode;
        private int _labelButtonsPageIndex;
        private System.ComponentModel.INotifyPropertyChanged? _currentWorkflowViewModel;
        private System.ComponentModel.PropertyChangedEventHandler? _currentPropertyChangedHandler;
        private string? _settingsReturnRouteTag;
        private Type? _currentSettingsPageType;

        private enum SearchDestinationKind
        {
            FrameRoute,
            SettingsPage,
            Command,
        }

        private sealed class SearchDestination
        {
            private readonly string[] _aliases;

            public SearchDestination(
                string key,
                string label,
                SearchDestinationKind kind,
                string? routeTag,
                Type? settingsPageType,
                string detail,
                string? commandText = null,
                params string[] aliases
            )
            {
                Key = key;
                Label = label;
                Kind = kind;
                RouteTag = routeTag;
                SettingsPageType = settingsPageType;
                Detail = detail;
                CommandText = commandText;
                _aliases = aliases;
            }

            public string Key { get; }
            public string Label { get; }
            public SearchDestinationKind Kind { get; }
            public string? RouteTag { get; }
            public Type? SettingsPageType { get; }
            public string Detail { get; }
            public string? CommandText { get; }

            public IEnumerable<string> SearchTerms
            {
                get
                {
                    yield return Label;

                    if (!string.IsNullOrWhiteSpace(RouteTag))
                    {
                        yield return RouteTag;
                    }

                    if (!string.IsNullOrWhiteSpace(CommandText))
                    {
                        yield return CommandText;
                    }

                    foreach (var alias in _aliases)
                    {
                        yield return alias;
                    }
                }
            }

            public override string ToString() => Label;
        }

        /// <summary>
        /// Gets the main content frame for navigation
        /// </summary>
        public Frame GetContentFrame() => ContentFrame;

        public void SetContentPage(Page page, string? fallbackTitle = null)
        {
            ArgumentNullException.ThrowIfNull(page);

            ContentFrame.Content = page;
            SyncPageHeader(page, fallbackTitle ?? GetFallbackTitle(page.GetType()));
        }

        public MainWindow(
            ViewModel_Shared_MainWindow viewModel,
            IService_UserSessionManager sessionManager,
            IService_LoggingUtility logger,
            IServiceProvider serviceProvider,
            IService_LabelViewLauncher labelViewLauncher,
            IService_SettingsUserLabelButtons labelButtonSettings,
            IService_HeaderBackNavigation headerBackNavigation,
            IService_ErrorHandler errorHandler
        )
        {
            InitializeComponent();
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _labelViewLauncher = labelViewLauncher ?? throw new ArgumentNullException(nameof(labelViewLauncher));
            _labelButtonSettings = labelButtonSettings ?? throw new ArgumentNullException(nameof(labelButtonSettings));
            _headerBackNavigation = headerBackNavigation ?? throw new ArgumentNullException(nameof(headerBackNavigation));
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));

            if (ViewModel.NotificationService is not null)
            {
                ViewModel.NotificationService.PropertyChanged += NotificationService_PropertyChanged;
            }

            _headerBackNavigation.PropertyChanged += HeaderBackNavigation_PropertyChanged;

            if (NavView is not null)
            {
                _applicationMenuItems.AddRange(NavView.MenuItems.Cast<object>());
                _applicationFooterItems.AddRange(NavView.FooterMenuItems.Cast<object>());
            }

            _settingsMenuItems.AddRange(CreateSettingsNavigationItems());

            // Configure Frame to use DI for view activation
            if (ContentFrame is not null)
            {
                ContentFrame.NavigationFailed += ContentFrame_NavigationFailed;
            }

            try
            {
                // Set initial window size (1450x900 to accommodate wide data grids and toolbars)
                AppWindow.Resize(this.GetScaledWindowSize(1450, 900));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"Unable to resize main window during startup: {ex.Message}",
                    nameof(MainWindow)
                );
            }

            try
            {
                // Center window on screen
                CenterWindow();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"Unable to center main window during startup: {ex.Message}",
                    nameof(MainWindow)
                );
            }

            try
            {
                // Configure custom title bar
                ConfigureTitleBar();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"Unable to configure title bar during startup: {ex.Message}",
                    nameof(MainWindow)
                );
            }

            try
            {
                // Apply the shared window icon so published builds match debug behavior.
                this.ApplySharedIcon(_logger);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"Unable to apply shared window icon during startup: {ex.Message}",
                    nameof(MainWindow)
                );
            }

            // Set user display from current session
            if (_sessionManager.CurrentSession?.User != null)
            {
                var user = _sessionManager.CurrentSession.User;
                if (UserDisplayTextBlock is not null)
                {
                    UserDisplayTextBlock.Text = user.DisplayName;
                }

                if (UserPicture is not null)
                {
                    UserPicture.DisplayName = user.DisplayName;
                }
            }

            // Wire up activity tracking
            if (Content is UIElement rootElement)
            {
                rootElement.PointerMoved += (s, e) => _sessionManager.UpdateLastActivity();
                rootElement.KeyDown += (s, e) => _sessionManager.UpdateLastActivity();
            }

            // Subscribe to theme changes to update the title bar colors and the
            // theme-aware module accent borders in the header.
            if (Content is FrameworkElement contentElement)
            {
                contentElement.ActualThemeChanged += (s, e) =>
                {
                    UpdateTitleBarColors();
                    UpdateTitleBarTextColor(_isWindowActive);
                    ApplyHeaderAccent(ContentFrame?.Content?.GetType());
                };
            }

            this.Activated += MainWindow_Activated;

            // Subscribe to navigation events once
            if (ContentFrame is not null)
            {
                ContentFrame.Navigated += ContentFrame_Navigated;
            }

            // Wire up title bar events
            if (AppTitleBar is not null)
            {
                AppTitleBar.Loaded += AppTitleBar_Loaded;
                AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
            }

            if (NavView is not null)
            {
                ApplyNavigationMode(isSettingsMode: false);
            }

            ApplyScannerNavigationGate();
            InitializeScannerHotkeys();

            UpdateHeaderBackButton();
            UpdateStatusInfoBarActionButton();

            _ = LoadConfiguredLabelButtonsAsync();
        }

        private void HeaderBackNavigation_PropertyChanged(
            object? sender,
            PropertyChangedEventArgs e
        )
        {
            if (
                e.PropertyName
                is nameof(IService_HeaderBackNavigation.IsBackButtonVisible)
                    or nameof(IService_HeaderBackNavigation.BackButtonToolTip)
            )
            {
                UpdateHeaderBackButton();
            }
        }

        private void UpdateHeaderBackButton()
        {
            if (HeaderBackButton is null)
            {
                return;
            }

            HeaderBackButton.Visibility = _headerBackNavigation.IsBackButtonVisible
                ? Visibility.Visible
                : Visibility.Collapsed;
            ToolTipService.SetToolTip(HeaderBackButton, _headerBackNavigation.BackButtonToolTip);
        }

        private async void HeaderBackButton_Click(object sender, RoutedEventArgs e)
        {
            await _headerBackNavigation.ExecuteBackActionAsync();
        }

        private void NotificationService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (
                e.PropertyName
                is nameof(IService_Notification.StatusActionLabel)
                    or nameof(IService_Notification.IsStatusActionVisible)
            )
            {
                UpdateStatusInfoBarActionButton();
            }
        }

        private void UpdateStatusInfoBarActionButton()
        {
            if (StatusInfoBarActionButton is null)
            {
                return;
            }

            StatusInfoBarActionButton.Content = ViewModel.NotificationService.StatusActionLabel;
            StatusInfoBarActionButton.Visibility = ViewModel
                .NotificationService
                .IsStatusActionVisible
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>
        /// Shows the Scanner navigation entry only for developer users; for everyone else the
        /// entry is hidden entirely from the nav bar. The scanner automation engine is
        /// implemented but not yet verified against a real VMINVENT terminal, so it is
        /// restricted to developers during rollout.
        /// </summary>
        private void ApplyScannerNavigationGate()
        {
            try
            {
                var scannerItem = FindNavigationItemByTag("ScannerMainPage");
                if (scannerItem is null)
                {
                    return;
                }

                var isDeveloper = Helper_ScannerAccess.IsDeveloperUser(
                    _sessionManager.CurrentSession?.User,
                    Environment.UserName
                );
                scannerItem.Visibility = isDeveloper
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                scannerItem.IsEnabled = isDeveloper;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"Unable to apply Scanner navigation gate: {ex.Message}",
                    nameof(MainWindow)
                );
            }
        }

        /// <summary>
        /// Registers the global scanner send hotkey (Ctrl+Alt+M) against the main window and
        /// unregisters it when the window closes. The shortcut is active for the app lifetime
        /// so the operator can trigger a send while VMINVENT is focused.
        /// </summary>
        private void InitializeScannerHotkeys()
        {
            try
            {
                var hotkey = _serviceProvider.GetService<IService_ScannerHotkey>();
                if (hotkey is null)
                {
                    return;
                }

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                var registered = hotkey.TryRegister(hwnd, "Ctrl+Alt+M");
                if (!registered)
                {
                    _logger.LogWarning(
                        "Unable to register global scanner hotkeys. Another application may already own the chord.",
                        nameof(MainWindow)
                    );
                }

                Closed += (_, _) => hotkey.Unregister();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    $"Unable to initialize scanner hotkeys: {ex.Message}",
                    nameof(MainWindow)
                );
            }
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            // Update the app title color based on activation state using the
            // standard theme-aware text brushes so it stays readable in both
            // light and dark mode. Re-resolved on theme changes so a live
            // light/dark toggle updates the title immediately.
            _isWindowActive = args.WindowActivationState != WindowActivationState.Deactivated;
            UpdateTitleBarTextColor(_isWindowActive);

            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                _sessionManager.UpdateLastActivity();
                _ = LoadConfiguredLabelButtonsAsync();

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

        private async void StatusInfoBarActionButton_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.NotificationService.ExecuteStatusActionAsync();
        }

        private static readonly Dictionary<string, (Type PageType, string Title)> _navRoutes = new()
        {
            ["ReceivingWorkflowView"] = (
                typeof(Module_Receiving.Views.View_Receiving_Workflow),
                string.Empty
            ),
            ["ScannerMainPage"] = (
                typeof(Module_Scanner.Views.View_Scanner_Main),
                "Scanner"
            ),
            ["ReprintLabelsPage"] = (
                typeof(Module_Reprint.Views.View_Reprint_Main),
                "Reprint Labels"
            ),
            ["DunnageLabelPage"] = (
                typeof(Module_Dunnage.Views.View_Dunnage_WorkflowView),
                string.Empty
            ),
            // MODULE_OUTSIDESERVICE_DISABLED: Route intentionally removed while the module is offline.
            ["VolvoShipmentEntry"] = (
                typeof(Module_Volvo.Views.View_Volvo_ShipmentEntry),
                "Volvo Dunnage Requisition"
            ),
            ["VolvoHistory"] = (
                typeof(Module_Volvo.Views.View_Volvo_History),
                "Volvo Shipment History"
            ),
            ["ReportingMainPage"] = (
                typeof(Module_Reporting.Views.View_Reporting_Main),
                "End of Day Reports"
            ),
            ["ShipRecToolsPage"] = (
                typeof(Module_ShipRec_Tools.Views.View_ShipRecTools_Main),
                string.Empty
            ),
        };

        private static readonly Dictionary<
            string,
            (Type PageType, string NamespacePrefix)
        > _settingsRoutes = new()
        {
            ["CoreSettingsHub"] = (
                typeof(View_Settings_CoreNavigationHub),
                "MTM_Receiving_Application.Module_Settings.Core.Views"
            ),
            ["ReceivingSettingsHub"] = (
                typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_CategoryHub),
                "MTM_Receiving_Application.Module_Settings.Receiving.Views"
            ),
            ["DunnageSettingsHub"] = (
                typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_CategoryHub),
                "MTM_Receiving_Application.Module_Settings.Dunnage.Views"
            ),
            ["ReportingSettingsHub"] = (
                typeof(Module_Settings.Reporting.Views.View_Settings_Reporting_NavigationHub),
                "MTM_Receiving_Application.Module_Settings.Reporting.Views"
            ),
            ["VolvoSettingsHub"] = (
                typeof(Module_Settings.Volvo.Views.View_Settings_Volvo_NavigationHub),
                "MTM_Receiving_Application.Module_Settings.Volvo.Views"
            ),
        };

        private static readonly List<SearchDestination> _searchDestinations =
            CreateSearchDestinations();

        private static readonly List<SearchDestination> _searchCommandDestinations =
            CreateSearchCommandDestinations();

        private static readonly Dictionary<string, SearchDestination> _searchDestinationsByKey =
            _searchDestinations
                .Concat(_searchCommandDestinations)
                .ToDictionary(destination => destination.Key, StringComparer.Ordinal);

        private static readonly HashSet<string> _searchCommandNames = new(
            [
                "/all",
                "/apps",
                "/settings",
                "/help",
                "/labels",
                "/tools",
                "/receiving",
                "/dunnage",
                "/volvo",
                "/reporting",
                "/docs",
            ],
            StringComparer.OrdinalIgnoreCase
        );

        private async void NavView_SelectionChanged(
            NavigationView sender,
            NavigationViewSelectionChangedEventArgs args
        )
        {
            if (_isUpdatingNavSelection)
            {
                return;
            }

            if (args.IsSettingsSelected)
            {
                var navigationSucceeded = _isSettingsMode
                    ? await ExitSettingsModeAsync()
                    : await EnterSettingsModeAsync();

                if (!navigationSucceeded)
                {
                    if (_isSettingsMode)
                    {
                        SetNavigationSelectionByTag(GetCurrentSettingsTag());
                    }
                    else
                    {
                        SetNavigationSelectionByTag(
                            GetCurrentRouteTag() ?? "ReceivingWorkflowView"
                        );
                    }
                }

                return;
            }

            if (args.SelectedItem is not NavigationViewItem item)
            {
                return;
            }

            var tag = item.Tag?.ToString();
            if (tag is null)
            {
                return;
            }

            if (_isSettingsMode)
            {
                NavigateToSettingsTag(tag);
                return;
            }

            if (tag == "AppDocumentation")
            {
                var docsPath = Path.Combine(AppContext.BaseDirectory, "docs", "index.html");
                var docsUri = new Uri(docsPath);
                _ = Windows.System.Launcher.LaunchUriAsync(docsUri);
                SetNavigationSelectionByTag(GetCurrentRouteTag());
                return;
            }

            if (!_navRoutes.TryGetValue(tag, out var route))
            {
                return;
            }

            if (!await ClearModuleDraftStateBeforeNavigationAsync(route.PageType, route.Title))
            {
                SetNavigationSelectionByTag(GetCurrentRouteTag());
                return;
            }

            NavigateWithDI(route.PageType, route.Title);
        }

        private async Task OpenLabelAsync(Model_MainWindowLabelButton button)
        {
            var executablePath = await _labelViewLauncher.ResolveExecutablePathAsync();
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                await RedirectToLabelSettingsPageAsync(
                    typeof(View_Settings_LabelViewExecutable),
                    "LabelView is not configured. Opening LabelView settings."
                );
                return;
            }

            if (!_labelViewLauncher.IsLabelFilePathValid(button.LabelPath))
            {
                await RedirectToLabelSettingsPageAsync(
                    typeof(View_Settings_LabelViewExecutable),
                    $"{button.Label} path is missing or invalid. Opening LabelView settings."
                );
                return;
            }

            var launchResult = await _labelViewLauncher.LaunchLabelAsync(button.LabelPath);
            if (!launchResult.IsSuccess)
            {
                await _errorHandler.HandleDaoErrorAsync(launchResult, nameof(OpenLabelAsync));
                return;
            }

            ViewModel.NotificationService.ShowStatus(
                $"{button.Label} opened in LabelView.",
                Module_Core.Models.Enums.InfoBarSeverity.Success
            );
        }

        private async Task LoadConfiguredLabelButtonsAsync()
        {
            var buttons = await _labelButtonSettings.GetButtonsAsync();
            _allLabelButtons.Clear();
            _allLabelButtons.AddRange(buttons.OrderBy(button => button.SortOrder));

            _configuredLabelButtons.Clear();
            _configuredLabelButtons.AddRange(
                buttons
                    .Where(button => button.IsEnabled)
                    .OrderBy(button => button.SortOrder)
            );

            ClampLabelButtonsPageIndex();
            RenderConfiguredLabelButtons();
        }

        private void RenderConfiguredLabelButtons()
        {
            PrimaryLabelButtonsPanel.Children.Clear();

            foreach (var button in GetLabelButtonsForCurrentPage())
            {
                PrimaryLabelButtonsPanel.Children.Add(CreateLabelButton(button));
            }

            UpdateLabelButtonsPagingState();
        }

        private IReadOnlyList<Model_MainWindowLabelButton> GetLabelButtonsForCurrentPage()
        {
            if (_configuredLabelButtons.Count == 0)
            {
                return Array.Empty<Model_MainWindowLabelButton>();
            }

            var pageCount = GetLabelButtonsPageCount();
            if (_labelButtonsPageIndex >= pageCount)
            {
                _labelButtonsPageIndex = pageCount - 1;
            }

            return _configuredLabelButtons
                .Skip(_labelButtonsPageIndex * LabelButtonsPerPage)
                .Take(LabelButtonsPerPage)
                .ToList();
        }

        private int GetLabelButtonsPageCount()
        {
            if (_configuredLabelButtons.Count == 0)
            {
                return 1;
            }

            return (_configuredLabelButtons.Count + LabelButtonsPerPage - 1) / LabelButtonsPerPage;
        }

        private void ClampLabelButtonsPageIndex()
        {
            var maxPageIndex = Math.Max(0, GetLabelButtonsPageCount() - 1);
            _labelButtonsPageIndex = Math.Clamp(_labelButtonsPageIndex, 0, maxPageIndex);
        }

        private void UpdateLabelButtonsPagingState()
        {
            var hasPaging = _configuredLabelButtons.Count > LabelButtonsPerPage;
            var contentRoot = GetContentRoot();

            if (contentRoot?.FindName("PrimaryLabelButtonsPagingPanel") is FrameworkElement pagingPanel)
            {
                pagingPanel.Visibility = hasPaging ? Visibility.Visible : Visibility.Collapsed;
            }

            if (contentRoot?.FindName("PrimaryLabelButtonsUpButton") is Button upButton)
            {
                upButton.IsEnabled = hasPaging && _labelButtonsPageIndex > 0;
            }

            if (contentRoot?.FindName("PrimaryLabelButtonsDownButton") is Button downButton)
            {
                downButton.IsEnabled = hasPaging && _labelButtonsPageIndex < GetLabelButtonsPageCount() - 1;
            }
        }

        private void MoveLabelButtonsPage(int delta)
        {
            if (_configuredLabelButtons.Count <= LabelButtonsPerPage)
            {
                return;
            }

            var nextPage = _labelButtonsPageIndex + delta;
            var maxPageIndex = Math.Max(0, GetLabelButtonsPageCount() - 1);
            _labelButtonsPageIndex = Math.Clamp(nextPage, 0, maxPageIndex);
            RenderConfiguredLabelButtons();
        }

        private Button CreateLabelButton(Model_MainWindowLabelButton configuredButton)
        {
            var contentGrid = new Grid
            {
                Width = 35,
                Height = 35,
            };

            contentGrid.Children.Add(
                new Border
                {
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 248, 250, 251)),
                    BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 55, 65, 81)),
                    BorderThickness = new Thickness(1.5),
                    CornerRadius = new CornerRadius(3),
                }
            );

            contentGrid.Children.Add(
                new MaterialIcon
                {
                    Width = 22,
                    Height = 22,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Kind = ParseIconKind(configuredButton.IconKey),
                    Foreground = ResolveAccentBrush(configuredButton),
                }
            );

            var button = new Button
            {
                Content = contentGrid,
            };

            ToolTipService.SetToolTip(button, $"Open {configuredButton.Label}");
            button.Click += async (_, _) => await OpenLabelAsync(configuredButton);
            button.ContextFlyout = BuildLabelButtonContextFlyout(configuredButton);

            return button;
        }

        private MenuFlyout BuildLabelButtonContextFlyout(Model_MainWindowLabelButton configuredButton)
        {
            var flyout = new MenuFlyout();

            var editItem = new MenuFlyoutItem { Text = "Edit" };
            editItem.Click += async (_, _) =>
                await RedirectToLabelSettingsPageAsync(
                    typeof(View_Settings_LabelViewExecutable),
                    "Opening LabelView settings."
                );

            var duplicateItem = new MenuFlyoutItem { Text = "Duplicate" };
            duplicateItem.Click += async (_, _) =>
            {
                if (_configuredLabelButtons.Count >= 10)
                {
                    ViewModel.NotificationService.ShowStatus(
                        "You can configure up to 10 label buttons.",
                        Module_Core.Models.Enums.InfoBarSeverity.Warning
                    );
                    return;
                }

                var updated = _allLabelButtons.ToList();
                var index = updated.FindIndex(button => button.Id == configuredButton.Id);
                if (index < 0)
                {
                    index = updated.Count - 1;
                }

                updated.Insert(
                    index + 1,
                    new Model_MainWindowLabelButton
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Label = $"{configuredButton.Label} Copy",
                        LabelPath = configuredButton.LabelPath,
                        IconKey = configuredButton.IconKey,
                        Accent = configuredButton.Accent,
                        IsEnabled = true,
                        SortOrder = configuredButton.SortOrder + 1,
                    }
                );

                NormalizeSortOrder(updated);
                var saveResult = await _labelButtonSettings.SaveButtonsAsync(updated);
                if (!saveResult.IsSuccess)
                {
                    await _errorHandler.HandleDaoErrorAsync(saveResult, nameof(BuildLabelButtonContextFlyout));
                    return;
                }

                await LoadConfiguredLabelButtonsAsync();
            };

            var deleteItem = new MenuFlyoutItem { Text = "Delete" };
            deleteItem.Click += async (_, _) =>
            {
                var updated = _allLabelButtons
                    .Where(button => button.Id != configuredButton.Id)
                    .ToList();

                NormalizeSortOrder(updated);
                var saveResult = await _labelButtonSettings.SaveButtonsAsync(updated);
                if (!saveResult.IsSuccess)
                {
                    await _errorHandler.HandleDaoErrorAsync(saveResult, nameof(BuildLabelButtonContextFlyout));
                    return;
                }

                await LoadConfiguredLabelButtonsAsync();
            };

            flyout.Items.Add(editItem);
            flyout.Items.Add(duplicateItem);
            flyout.Items.Add(deleteItem);
            return flyout;
        }

        private static void NormalizeSortOrder(List<Model_MainWindowLabelButton> buttons)
        {
            for (var index = 0; index < buttons.Count; index++)
            {
                buttons[index].SortOrder = index;
            }
        }

        private void PrimaryLabelButtonsPageUpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveLabelButtonsPage(-1);
        }

        private void PrimaryLabelButtonsPageDownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveLabelButtonsPage(1);
        }

        private static MaterialIconKind ParseIconKind(string iconKey)
        {
            return Enum.TryParse<MaterialIconKind>(iconKey, true, out var iconKind)
                ? iconKind
                : MaterialIconKind.PackageVariantClosed;
        }

        private static Brush ResolveAccentBrush(Model_MainWindowLabelButton button)
        {
            return button.Accent switch
            {
                Module_Settings.Core.Enums.Enum_MainWindowLabelButtonAccent.Green =>
                    new SolidColorBrush(Windows.UI.Color.FromArgb(255, 22, 101, 52)),
                Module_Settings.Core.Enums.Enum_MainWindowLabelButtonAccent.Red =>
                    new SolidColorBrush(Windows.UI.Color.FromArgb(255, 185, 28, 28)),
                Module_Settings.Core.Enums.Enum_MainWindowLabelButtonAccent.Blue =>
                    new SolidColorBrush(Windows.UI.Color.FromArgb(255, 30, 64, 175)),
                Module_Settings.Core.Enums.Enum_MainWindowLabelButtonAccent.Black =>
                    new SolidColorBrush(Windows.UI.Color.FromArgb(255, 17, 24, 39)),
                _ => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 55, 65, 81)),
            };
        }

        private async Task RedirectToLabelSettingsPageAsync(Type pageType, string statusMessage)
        {
            var navigationSucceeded = await NavigateToSettingsPageAsync(pageType);
            if (navigationSucceeded)
            {
                ViewModel.NotificationService.ShowStatus(
                    statusMessage,
                    Module_Core.Models.Enums.InfoBarSeverity.Warning
                );
                return;
            }

            await _errorHandler.HandleErrorAsync(
                "Unable to open the required settings page.",
                Enum_ErrorSeverity.Warning
            );
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateSettingsToggleText();
            UpdateSettingsBackButtonState();
        }

        private void NavView_BackRequested(
            NavigationView sender,
            NavigationViewBackRequestedEventArgs args
        )
        {
            if (!_isSettingsMode)
            {
                return;
            }

            NavigateToSettingsTag(GetCurrentSettingsTag());
        }

        private static List<SearchDestination> CreateSearchDestinations()
        {
            static SearchDestination CreateFrameDestination(
                string routeTag,
                string label,
                string detail,
                params string[] aliases
            )
            {
                return new SearchDestination(
                    key: routeTag,
                    label: label,
                    kind: SearchDestinationKind.FrameRoute,
                    routeTag: routeTag,
                    settingsPageType: null,
                    detail: detail,
                    aliases: aliases
                );
            }

            static SearchDestination CreateSettingsDestination(
                Type pageType,
                string label,
                string detail,
                params string[] aliases
            )
            {
                return new SearchDestination(
                    key: pageType.FullName ?? pageType.Name,
                    label: label,
                    kind: SearchDestinationKind.SettingsPage,
                    routeTag: null,
                    settingsPageType: pageType,
                    detail: detail,
                    aliases: aliases
                );
            }

            return
            [
                CreateFrameDestination(
                    "ReceivingWorkflowView",
                    "Receiving Labels",
                    "Guided, manual, and edit receiving workflow",
                    "receiving",
                    "receiving labels",
                    "receiving workflow",
                    "receiving mode selection",
                    "guided receiving",
                    "manual receiving",
                    "edit receiving"
                ),
                CreateFrameDestination(
                    "DunnageLabelPage",
                    "Dunnage Labels",
                    "Guided and manual dunnage workflow",
                    "dunnage",
                    "dunnage labels",
                    "dunnage workflow",
                    "dunnage mode selection",
                    "guided dunnage",
                    "manual dunnage"
                ),
                // MODULE_OUTSIDESERVICE_DISABLED: Search destination intentionally removed while the module is offline.
                CreateFrameDestination(
                    "VolvoShipmentEntry",
                    "Volvo Dunnage Requisition",
                    "Volvo requisition entry page",
                    "volvo",
                    "volvo requisition",
                    "volvo shipment entry"
                ),
                CreateFrameDestination(
                    "VolvoHistory",
                    "Volvo Shipment History",
                    "Historical Volvo shipment and requisition lookup",
                    "volvo history",
                    "shipment history",
                    "volvo shipment history"
                ),
                CreateFrameDestination(
                    "ReportingMainPage",
                    "End of Day Reports",
                    "Reporting and export entry point",
                    "reports",
                    "reporting",
                    "end of day reports"
                ),
                CreateFrameDestination(
                    "ShipRecToolsPage",
                    "Ship/Rec Tools",
                    "Shipping and receiving tools hub",
                    "ship rec",
                    "ship rec tools",
                    "shipping receiving tools",
                    "tools"
                ),
                CreateFrameDestination(
                    "ScannerMainPage",
                    "Scanner",
                    "Scanner workbench, history, and settings",
                    "scanner",
                    "scanner module",
                    "scan"
                ),
                CreateFrameDestination(
                    "ReprintLabelsPage",
                    "Reprint Labels",
                    "Reprint Receiving, Dunnage, or Volvo labels from history",
                    "reprint",
                    "reprint labels",
                    "reprint labels page"
                ),
                CreateFrameDestination(
                    "AppDocumentation",
                    "Documentation",
                    "Open the in-app documentation index",
                    "documentation",
                    "docs",
                    "help docs",
                    "application docs"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Core.Views.View_Settings_CoreNavigationHub),
                    "Configuration",
                    "Core settings shell",
                    "settings",
                    "configuration",
                    "core settings"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Core.Views.View_Settings_Users),
                    "User Management",
                    "Application users and account setup",
                    "users",
                    "user settings",
                    "user management"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Core.Views.View_Settings_Theme),
                    "Theme Settings",
                    "Application appearance and theme options",
                    "theme",
                    "theme settings",
                    "appearance"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Core.Views.View_Settings_SharedPaths),
                    "Shared Paths",
                    "Shared file paths and output locations",
                    "paths",
                    "shared paths",
                    "file paths"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Core.Views.View_Settings_LabelViewExecutable),
                    "LabelView",
                    "Configure the LabelView executable path used when opening label template files",
                    "labelview",
                    "labelview executable",
                    "labelview path",
                    "label view"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Core.Views.View_Settings_MaterialAvailabilityBoardFields),
                    "Material Availability Fields",
                    "Control which work-order details are shown in the Material Availability dialog and print output",
                    "material availability",
                    "material availability board",
                    "material availability fields",
                    "work order details"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_CategoryHub),
                    "Receiving Settings",
                    "Receiving settings categories",
                    "receiving settings",
                    "receiving categories",
                    "receiving navigation"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_EntryDefaults),
                    "Receiving Entry Defaults",
                    "Default location and entry-start settings for Receiving",
                    "receiving entry defaults",
                    "receiving defaults"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_ValidationRules),
                    "Receiving Validation Rules",
                    "Required fields, warnings, and threshold rules for Receiving",
                    "receiving validation rules",
                    "receiving validation",
                    "validation settings"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_PartFormatting),
                    "Receiving Part Formatting",
                    "Receiving part-number auto-padding rules and formatting test tools",
                    "part formatting",
                    "part padding",
                    "receiving part formatting"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_Reconciliation),
                    "Receiving Reconciliation",
                    "Ignored recommended locations and reconciliation validation scope",
                    "reconciliation",
                    "reconciliation settings",
                    "ignored locations",
                    "recommended locations"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_WorkflowDefaults),
                    "Receiving Workflow Defaults",
                    "Receiving startup mode and workflow behavior defaults",
                    "workflow defaults",
                    "receiving workflow defaults",
                    "receiving workflow options"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_KeyboardShortcuts),
                    "Receiving Keyboard Shortcuts",
                    "Configure the Receiving workflow shortcuts and the simple navigation toggle",
                    "receiving shortcuts",
                    "receiving keyboard shortcuts",
                    "keyboard shortcuts receiving"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_CategoryHub),
                    "Dunnage Settings",
                    "Dunnage settings categories",
                    "dunnage settings",
                    "dunnage categories",
                    "dunnage navigation"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_PersonalDefaults),
                    "Dunnage Personal Defaults",
                    "User-specific default location and personal image preferences for Dunnage",
                    "dunnage personal defaults",
                    "dunnage preferences"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_ImageAssets),
                    "Dunnage Image Assets",
                    "Shared Dunnage image folder and file size limits",
                    "dunnage image assets",
                    "dunnage image folder",
                    "dunnage image settings"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_ImagePresentation),
                    "Dunnage Image Presentation",
                    "Application-wide Dunnage image display settings",
                    "dunnage image presentation",
                    "dunnage display settings",
                    "dunnage ui"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_WorkflowVisuals),
                    "Dunnage Workflow Visuals",
                    "Image behavior during the Dunnage workflow",
                    "dunnage workflow visuals",
                    "dunnage workflow image settings",
                    "dunnage workflow"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_KeyboardShortcuts),
                    "Dunnage Keyboard Shortcuts",
                    "Configure the Dunnage workflow shortcuts and the simple navigation toggle",
                    "dunnage shortcuts",
                    "dunnage keyboard shortcuts",
                    "keyboard shortcuts dunnage"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Reporting.Views.View_Settings_Reporting_NavigationHub),
                    "Reporting Settings",
                    "Reporting settings placeholder",
                    "reporting settings",
                    "reporting navigation"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Reporting.Views.View_Settings_Reporting_EmailRecipients),
                    "Reporting Email Recipients",
                    "Configure email recipients for end-of-day report delivery",
                    "reporting email",
                    "reporting email recipients",
                    "report recipients"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Volvo.Views.View_Settings_Volvo_NavigationHub),
                    "Volvo Settings",
                    "Volvo settings hub",
                    "volvo settings",
                    "volvo navigation"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Volvo.Views.View_Settings_Volvo_PartCatalog),
                    "Volvo Part Catalog",
                    "Manage Volvo part master data, import catalog rows, and launch part maintenance actions",
                    "volvo parts",
                    "volvo part catalog",
                    "volvo catalog"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Volvo.Views.View_Settings_Volvo_EmailRecipients),
                    "Volvo Email Recipients",
                    "Configure email recipients for Volvo dunnage requisition notifications",
                    "volvo email",
                    "volvo email recipients",
                    "volvo notifications"
                ),
            ];
        }

        private static List<SearchDestination> CreateSearchCommandDestinations()
        {
            static SearchDestination CreateCommandDestination(
                string commandText,
                string label,
                string detail,
                params string[] aliases
            )
            {
                return new SearchDestination(
                    key: $"command:{commandText}",
                    label: label,
                    kind: SearchDestinationKind.Command,
                    routeTag: null,
                    settingsPageType: null,
                    detail: detail,
                    commandText: commandText,
                    aliases: aliases
                );
            }

            return
            [
                CreateCommandDestination(
                    "/all",
                    "/all",
                    "Show all searchable links",
                    "all"
                ),
                CreateCommandDestination(
                    "/apps",
                    "/apps",
                    "Show application pages only",
                    "apps",
                    "pages"
                ),
                CreateCommandDestination(
                    "/settings",
                    "/settings",
                    "Show settings pages only",
                    "settings"
                ),
                CreateCommandDestination(
                    "/labels",
                    "/labels",
                    "Show label-related destinations",
                    "labels"
                ),
                CreateCommandDestination(
                    "/tools",
                    "/tools",
                    "Show tools-related destinations",
                    "tools"
                ),
                CreateCommandDestination(
                    "/receiving",
                    "/receiving",
                    "Show receiving-related destinations",
                    "receiving"
                ),
                CreateCommandDestination(
                    "/dunnage",
                    "/dunnage",
                    "Show dunnage-related destinations",
                    "dunnage"
                ),
                CreateCommandDestination(
                    "/volvo",
                    "/volvo",
                    "Show volvo-related destinations",
                    "volvo"
                ),
                CreateCommandDestination(
                    "/reporting",
                    "/reporting",
                    "Show reporting-related destinations",
                    "reporting"
                ),
                CreateCommandDestination(
                    "/docs",
                    "/docs",
                    "Show documentation destination",
                    "docs",
                    "documentation"
                ),
            ];
        }

        private static IReadOnlyList<SearchDestination> GetSearchMatches(string queryText)
        {
            var normalizedQuery = NormalizeSearchText(queryText);
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return Array.Empty<SearchDestination>();
            }

            if (TryGetCommandMatches(normalizedQuery, out var commandMatches))
            {
                return commandMatches;
            }

            return _searchDestinations
                .Select(destination => new
                {
                    Destination = destination,
                    Rank = GetSearchMatchRank(destination, normalizedQuery),
                })
                .Where(result => result.Rank < int.MaxValue)
                .OrderBy(result => result.Rank)
                .ThenBy(result => result.Destination.Label, StringComparer.OrdinalIgnoreCase)
                .Select(result => result.Destination)
                .ToList();
        }

        private static bool TryResolveConfidentSearchDestination(
            string queryText,
            out SearchDestination? destination
        )
        {
            var normalizedQuery = NormalizeSearchText(queryText);
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                destination = null;
                return false;
            }

            if (TryGetCommandMatches(normalizedQuery, out var commandMatches))
            {
                if (commandMatches.Count == 1)
                {
                    destination = commandMatches[0];
                    return true;
                }

                destination = null;
                return false;
            }

            var exactMatches = _searchDestinations
                .Where(candidate =>
                    candidate.SearchTerms.Any(term =>
                        string.Equals(term, normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    )
                )
                .ToList();
            if (exactMatches.Count == 1)
            {
                destination = exactMatches[0];
                return true;
            }

            var startsWithMatches = _searchDestinations
                .Where(candidate =>
                    candidate.SearchTerms.Any(term =>
                        term.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    )
                )
                .ToList();
            if (startsWithMatches.Count == 1)
            {
                destination = startsWithMatches[0];
                return true;
            }

            var containsMatches = _searchDestinations
                .Where(candidate =>
                    candidate.SearchTerms.Any(term =>
                        term.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                    )
                )
                .ToList();
            if (containsMatches.Count == 1)
            {
                destination = containsMatches[0];
                return true;
            }

            destination = null;
            return false;
        }

        private static bool TryGetCommandMatches(
            string normalizedQuery,
            out IReadOnlyList<SearchDestination> matches
        )
        {
            matches = Array.Empty<SearchDestination>();

            if (!normalizedQuery.StartsWith('/'))
            {
                return false;
            }

            var parts = normalizedQuery.Split(' ', 2, StringSplitOptions.TrimEntries);
            var command = parts[0];
            var optionalFilter = parts.Length > 1 ? parts[1] : string.Empty;

            if (string.Equals(command, "/help", StringComparison.OrdinalIgnoreCase))
            {
                var commandSet = _searchCommandDestinations.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(optionalFilter))
                {
                    commandSet = commandSet
                        .Select(destination => new
                        {
                            Destination = destination,
                            Rank = GetSearchMatchRank(destination, optionalFilter),
                        })
                        .Where(result => result.Rank < int.MaxValue)
                        .OrderBy(result => result.Rank)
                        .ThenBy(result => result.Destination.Label, StringComparer.OrdinalIgnoreCase)
                        .Select(result => result.Destination);
                }
                else
                {
                    commandSet = commandSet.OrderBy(
                        destination => destination.Label,
                        StringComparer.OrdinalIgnoreCase
                    );
                }

                matches = commandSet.ToList();
                return true;
            }

            IEnumerable<SearchDestination> baseSet = command.ToLowerInvariant() switch
            {
                "/all" => _searchDestinations,
                "/apps" => _searchDestinations.Where(destination =>
                    destination.Kind == SearchDestinationKind.FrameRoute
                ),
                "/settings" => _searchDestinations.Where(destination =>
                    destination.Kind == SearchDestinationKind.SettingsPage
                ),
                "/labels" => _searchDestinations.Where(destination =>
                    destination.SearchTerms.Any(term =>
                        term.Contains("label", StringComparison.OrdinalIgnoreCase)
                    )
                ),
                "/tools" => _searchDestinations.Where(destination =>
                    destination.SearchTerms.Any(term =>
                        term.Contains("tool", StringComparison.OrdinalIgnoreCase)
                    )
                    || destination.SearchTerms.Any(term =>
                        term.Contains("scanner", StringComparison.OrdinalIgnoreCase)
                    )
                    || destination.SearchTerms.Any(term =>
                        term.Contains("report", StringComparison.OrdinalIgnoreCase)
                    )
                ),
                "/receiving" => _searchDestinations.Where(destination =>
                    destination.SearchTerms.Any(term =>
                        term.Contains("receiving", StringComparison.OrdinalIgnoreCase)
                    )
                ),
                "/dunnage" => _searchDestinations.Where(destination =>
                    destination.SearchTerms.Any(term =>
                        term.Contains("dunnage", StringComparison.OrdinalIgnoreCase)
                    )
                ),
                "/volvo" => _searchDestinations.Where(destination =>
                    destination.SearchTerms.Any(term =>
                        term.Contains("volvo", StringComparison.OrdinalIgnoreCase)
                    )
                ),
                "/reporting" => _searchDestinations.Where(destination =>
                    destination.SearchTerms.Any(term =>
                        term.Contains("report", StringComparison.OrdinalIgnoreCase)
                    )
                ),
                "/docs" => _searchDestinations.Where(destination =>
                    string.Equals(destination.RouteTag, "AppDocumentation", StringComparison.Ordinal)
                ),
                _ => Enumerable.Empty<SearchDestination>(),
            };

            if (!string.IsNullOrWhiteSpace(optionalFilter))
            {
                baseSet = baseSet
                    .Select(destination => new
                    {
                        Destination = destination,
                        Rank = GetSearchMatchRank(destination, optionalFilter),
                    })
                    .Where(result => result.Rank < int.MaxValue)
                    .OrderBy(result => result.Rank)
                    .ThenBy(result => result.Destination.Label, StringComparer.OrdinalIgnoreCase)
                    .Select(result => result.Destination);
            }
            else
            {
                baseSet = baseSet.OrderBy(destination => destination.Label, StringComparer.OrdinalIgnoreCase);
            }

            matches = baseSet.ToList();
            return true;
        }

        private static string NormalizeSearchText(string queryText)
        {
            return queryText.Trim();
        }

        private static int GetSearchMatchRank(SearchDestination destination, string normalizedQuery)
        {
            if (
                destination.SearchTerms.Any(term =>
                    string.Equals(term, normalizedQuery, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return 0;
            }

            if (
                destination.SearchTerms.Any(term =>
                    term.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return 1;
            }

            if (
                destination.SearchTerms.Any(term =>
                    term.Contains(normalizedQuery, StringComparison.OrdinalIgnoreCase)
                )
            )
            {
                return 2;
            }

            return int.MaxValue;
        }

        private void TitleBarSearchBox_TextChanged(
            AutoSuggestBox sender,
            AutoSuggestBoxTextChangedEventArgs args
        )
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
            {
                return;
            }

            var queryText = sender.Text?.Trim();
            if (string.IsNullOrWhiteSpace(queryText))
            {
                sender.ItemsSource = null;
                return;
            }

            sender.ItemsSource = GetSearchMatches(queryText).Take(24).ToList();
        }

        private void TitleBarSearchBox_SuggestionChosen(
            AutoSuggestBox sender,
            AutoSuggestBoxSuggestionChosenEventArgs args
        )
        {
            if (args.SelectedItem is SearchDestination destination)
            {
                sender.Text = destination.Label;
            }
        }

        private async void TitleBarSearchBox_QuerySubmitted(
            AutoSuggestBox sender,
            AutoSuggestBoxQuerySubmittedEventArgs args
        )
        {
            if (args.ChosenSuggestion is SearchDestination chosenDestination)
            {
                await NavigateToSearchDestinationAsync(chosenDestination);
                return;
            }

            var queryText = args.QueryText?.Trim();
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return;
            }

            var normalizedQuery = NormalizeSearchText(queryText);
            if (normalizedQuery.StartsWith('/'))
            {
                var commandMatches = GetSearchMatches(normalizedQuery).Take(50).ToList();
                if (commandMatches.Count == 1)
                {
                    await NavigateToSearchDestinationAsync(commandMatches[0]);
                    return;
                }

                if (commandMatches.Count > 1)
                {
                    await ShowSearchDisambiguationAsync(queryText, commandMatches);
                    return;
                }

                ViewModel.NotificationService.ShowStatus(
                    $"Unknown search command '{normalizedQuery}'. Try: {string.Join(", ", _searchCommandNames.OrderBy(command => command, StringComparer.OrdinalIgnoreCase))}",
                    global::MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity.Warning
                );
                return;
            }

            if (TryResolveConfidentSearchDestination(queryText, out var resolvedDestination))
            {
                await NavigateToSearchDestinationAsync(resolvedDestination!);
                return;
            }

            var matches = GetSearchMatches(queryText).Take(10).ToList();
            if (matches.Count == 0)
            {
                ViewModel.NotificationService.ShowStatus(
                    $"No page matched '{queryText}'.",
                    global::MTM_Receiving_Application
                        .Module_Core
                        .Models
                        .Enums
                        .InfoBarSeverity
                        .Warning
                );
                return;
            }

            await ShowSearchDisambiguationAsync(queryText, matches);
        }

        private async Task ShowSearchDisambiguationAsync(
            string queryText,
            IReadOnlyList<SearchDestination> matches
        )
        {
            var pickerItems = matches
                .Select(match => new Model_FuzzySearchResult
                {
                    Key = match.Key,
                    Label = match.Label,
                    Detail = match.Detail,
                })
                .ToList();

            var dialog = new Dialog_FuzzySearchPicker(
                pickerItems,
                "Open Page",
                $"Matching pages for '{queryText}'"
            );

            if (Content is FrameworkElement rootElement)
            {
                dialog.XamlRoot = rootElement.XamlRoot;
            }

            var dialogResult = await dialog.ShowAsync();
            if (
                dialogResult != ContentDialogResult.Primary
                || dialog.SelectedResult is null
                || !_searchDestinationsByKey.TryGetValue(
                    dialog.SelectedResult.Key,
                    out var destination
                )
            )
            {
                return;
            }

            await NavigateToSearchDestinationAsync(destination);
        }

        private async Task NavigateToSearchDestinationAsync(SearchDestination destination)
        {
            if (destination.Kind == SearchDestinationKind.Command)
            {
                var commandText = destination.CommandText ?? destination.Label;
                TitleBarSearchBox.Text = commandText;
                TitleBarSearchBox.ItemsSource = GetSearchMatches(commandText).Take(24).ToList();
                return;
            }

            bool navigationSucceeded;
            if (destination.Kind == SearchDestinationKind.FrameRoute)
            {
                navigationSucceeded = await NavigateToRouteTagAsync(destination.RouteTag!);
            }
            else
            {
                navigationSucceeded = await EnterSettingsModeAsync(destination.SettingsPageType!);
            }

            if (!navigationSucceeded)
            {
                ViewModel.NotificationService.ShowStatus(
                    $"Unable to open {destination.Label}.",
                    global::MTM_Receiving_Application.Module_Core.Models.Enums.InfoBarSeverity.Error
                );
                return;
            }

            TitleBarSearchBox.ItemsSource = null;
            TitleBarSearchBox.Text = destination.Label;
        }

        private async Task<bool> NavigateToRouteTagAsync(string routeTag)
        {
            if (string.Equals(routeTag, "AppDocumentation", StringComparison.Ordinal))
            {
                var docsPath = Path.Combine(AppContext.BaseDirectory, "docs", "index.html");
                var docsUri = new Uri(docsPath);
                _ = await Windows.System.Launcher.LaunchUriAsync(docsUri);
                SetNavigationSelectionByTag("AppDocumentation");
                return true;
            }

            if (!_navRoutes.TryGetValue(routeTag, out var route))
            {
                return false;
            }

            if (!await ClearModuleDraftStateBeforeNavigationAsync(route.PageType, route.Title))
            {
                return false;
            }

            SetNavigationSelectionByTag(routeTag);
            return NavigateWithDI(route.PageType, route.Title);
        }

        private string? GetCurrentRouteTag()
        {
            var currentPageType = ContentFrame.Content?.GetType();
            if (currentPageType is null)
            {
                return null;
            }

            return _navRoutes.FirstOrDefault(route => route.Value.PageType == currentPageType).Key;
        }

        private string GetCurrentSettingsTag()
        {
            if (NavView.SelectedItem is NavigationViewItem item && item.Tag is string selectedTag)
            {
                return selectedTag;
            }

            if (_currentSettingsPageType != null)
            {
                return View_Settings_CoreWindow.GetSettingsTagForPageType(_currentSettingsPageType)
                    ?? "CoreSettingsHub";
            }

            return "CoreSettingsHub";
        }

        private void SetNavigationSelectionByTag(string? routeTag)
        {
            try
            {
                _isUpdatingNavSelection = true;
                NavView.SelectedItem = FindNavigationItemByTag(routeTag);
            }
            finally
            {
                _isUpdatingNavSelection = false;
            }
        }

        private NavigationViewItem? FindNavigationItemByTag(string? routeTag)
        {
            return GetNavigationItems()
                .FirstOrDefault(item =>
                    string.Equals(item.Tag?.ToString(), routeTag, StringComparison.Ordinal)
                );
        }

        private IEnumerable<NavigationViewItem> GetNavigationItems()
        {
            foreach (var menuItem in NavView.MenuItems.OfType<NavigationViewItem>())
            {
                yield return menuItem;
            }

            foreach (var footerItem in NavView.FooterMenuItems.OfType<NavigationViewItem>())
            {
                yield return footerItem;
            }
        }

        private async Task<bool> ClearModuleDraftStateBeforeNavigationAsync(
            Type destinationPageType,
            string destinationTitle
        )
        {
            if (
                ContentFrame.Content is Module_Receiving.Views.View_Receiving_Workflow receivingView
            )
            {
                if (destinationPageType != typeof(Module_Receiving.Views.View_Receiving_Workflow))
                {
                    return await receivingView.ConfirmLeaveModuleAsync(destinationTitle);
                }

                return true;
            }

            if (ContentFrame.Content is Module_Dunnage.Views.View_Dunnage_WorkflowView dunnageView)
            {
                if (destinationPageType != typeof(Module_Dunnage.Views.View_Dunnage_WorkflowView))
                {
                    return await dunnageView.ConfirmLeaveModuleAsync(destinationTitle);
                }
            }

            return true;
        }

        private async Task<bool> ConfirmEnterSettingsModeAsync()
        {
            if (ContentFrame.Content is Module_Dunnage.Views.View_Dunnage_WorkflowView dunnageView)
            {
                return await dunnageView.ConfirmLeaveModuleAsync("Settings");
            }

            if (
                ContentFrame.Content is Module_Receiving.Views.View_Receiving_Workflow receivingView
            )
            {
                return await receivingView.ConfirmLeaveModuleAsync("Settings");
            }

            return true;
        }

        public Task<bool> NavigateToSettingsPageAsync(Type pageType)
        {
            ArgumentNullException.ThrowIfNull(pageType);
            return EnterSettingsModeAsync(pageType);
        }

        private async Task<bool> EnterSettingsModeAsync(Type? targetPageType = null)
        {
            if (!_isSettingsMode)
            {
                if (!await ConfirmEnterSettingsModeAsync())
                {
                    return false;
                }

                _settingsReturnRouteTag = ResolveSettingsReturnRouteTag();
                _isSettingsMode = true;
                View_Settings_CoreWindow.SetActiveHost(this);
                ApplyNavigationMode(isSettingsMode: true);
            }

            var pageType = targetPageType ?? typeof(View_Settings_CoreNavigationHub);
            var selectedTag =
                View_Settings_CoreWindow.GetSettingsTagForPageType(pageType) ?? "CoreSettingsHub";

            SetNavigationSelectionByTag(selectedTag);

            var navigationSucceeded = View_Settings_CoreWindow.IsHubPageType(pageType)
                ? NavigateToSettingsTag(selectedTag)
                : NavigateToPage(pageType);

            if (navigationSucceeded)
            {
                UpdateSettingsToggleText();
                UpdateSettingsBackButtonState();
            }

            return navigationSucceeded;
        }

        private async Task<bool> ExitSettingsModeAsync()
        {
            _isSettingsMode = false;
            _currentSettingsPageType = null;
            View_Settings_CoreWindow.SetActiveHost(null);
            ApplyNavigationMode(isSettingsMode: false);
            UpdateSettingsToggleText();

            var routeTag = _settingsReturnRouteTag ?? "ReceivingWorkflowView";
            _settingsReturnRouteTag = null;

            SetNavigationSelectionByTag(routeTag);
            var navigationSucceeded = await NavigateToRouteTagAsync(routeTag);
            if (navigationSucceeded)
            {
                await LoadConfiguredLabelButtonsAsync();
            }

            UpdateSettingsBackButtonState();
            return navigationSucceeded;
        }

        private string ResolveSettingsReturnRouteTag()
        {
            var currentRouteTag = GetCurrentRouteTag();
            return currentRouteTag switch
            {
                "VolvoHistory" => "VolvoShipmentEntry",
                null or "" => "ReceivingWorkflowView",
                _ => currentRouteTag,
            };
        }

        private void ApplyNavigationMode(bool isSettingsMode)
        {
            ReplaceNavigationItems(
                NavView.MenuItems,
                isSettingsMode ? _settingsMenuItems : _applicationMenuItems
            );
            ReplaceNavigationItems(
                NavView.FooterMenuItems,
                isSettingsMode ? Array.Empty<object>() : _applicationFooterItems
            );
            NavView.IsBackButtonVisible = isSettingsMode
                ? NavigationViewBackButtonVisible.Visible
                : NavigationViewBackButtonVisible.Collapsed;
            UpdateSettingsBackButtonState();
        }

        private static void ReplaceNavigationItems(
            IList<object> targetCollection,
            IEnumerable<object> items
        )
        {
            targetCollection.Clear();
            foreach (var item in items)
            {
                targetCollection.Add(item);
            }
        }

        private List<object> CreateSettingsNavigationItems()
        {
            return
            [
                CreateNavigationItem("Core Settings", "CoreSettingsHub", "\uE713"),
                new NavigationViewItemSeparator(),
                CreateNavigationItem("Receiving Navigation", "ReceivingSettingsHub", "\uE74C"),
                CreateNavigationItem("Dunnage Navigation", "DunnageSettingsHub", "\uE7B8"),
                CreateNavigationItem("Reporting Navigation", "ReportingSettingsHub", "\uE9F9"),
                CreateNavigationItem("Volvo Navigation", "VolvoSettingsHub", "\uE7C1"),
            ];
        }

        private static NavigationViewItem CreateNavigationItem(
            string content,
            string tag,
            string glyph
        )
        {
            return new NavigationViewItem
            {
                Margin = new Thickness(0, 0, 0, 4),
                Content = content,
                Tag = tag,
                Icon = new FontIcon { Glyph = glyph },
            };
        }

        private void UpdateSettingsToggleText()
        {
            if (NavView.SettingsItem is NavigationViewItem settingsItem)
            {
                settingsItem.Content = _isSettingsMode ? "Return" : "Settings";
            }
        }

        private void UpdateSettingsBackButtonState()
        {
            if (!_isSettingsMode)
            {
                NavView.IsBackEnabled = false;
                return;
            }

            if (
                !View_Settings_CoreWindow.TryGetModuleContext(
                    GetCurrentSettingsTag(),
                    out var namespacePrefix,
                    out var hubViewType
                ) || hubViewType is null
            )
            {
                NavView.IsBackEnabled = false;
                return;
            }

            var activePageType = _currentSettingsPageType ?? ContentFrame.Content?.GetType();
            NavView.IsBackEnabled =
                activePageType != null
                && activePageType != hubViewType
                && activePageType.Namespace?.StartsWith(namespacePrefix, StringComparison.Ordinal)
                    == true;
        }

        private bool NavigateToSettingsTag(string? selectedTag)
        {
            if (selectedTag is null || !_settingsRoutes.TryGetValue(selectedTag, out var route))
            {
                return false;
            }

            var navigationSucceeded = NavigateWithDI(
                route.PageType,
                View_Settings_CoreWindow.GetPageHeader(route.PageType).Title
            );

            if (!navigationSucceeded)
            {
                return false;
            }

            UpdateHeaderForPageType(route.PageType);
            return true;
        }

        public bool NavigateToPage(Type pageType)
        {
            ArgumentNullException.ThrowIfNull(pageType);

            var navigationSucceeded = NavigateWithDI(
                pageType,
                View_Settings_CoreWindow.GetPageHeader(pageType).Title
            );

            if (!navigationSucceeded)
            {
                return false;
            }

            UpdateHeaderForPageType(pageType);
            return true;
        }

        public void UpdateHeaderForPageType(Type pageType)
        {
            ArgumentNullException.ThrowIfNull(pageType);

            _currentSettingsPageType = pageType;
            var (title, _) = View_Settings_CoreWindow.GetPageHeader(pageType);
            if (!string.IsNullOrWhiteSpace(title))
            {
                UpdateHeaderText(title);
            }

            UpdateSettingsBackButtonState();
        }

        public FrameworkElement? GetContentRoot() => Content as FrameworkElement;

        public Window? GetHostWindow() => this;

        private void ClearHeaderSubscription()
        {
            if (_currentWorkflowViewModel != null && _currentPropertyChangedHandler != null)
            {
                _logger.LogInfo(
                    $"Unsubscribing from previous ViewModel: {_currentWorkflowViewModel.GetType().Name}"
                );
                _currentWorkflowViewModel.PropertyChanged -= _currentPropertyChangedHandler;
            }

            _currentWorkflowViewModel = null;
            _currentPropertyChangedHandler = null;
        }

        private void UpdateHeaderText(string title)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                PageTitleTextBlock.Text = title;
            });
        }

        /// <summary>
        /// Namespace prefix to module accent brush keys. The generic
        /// Module_Settings prefix is checked last so settings pages for a
        /// specific module (Receiving, Dunnage, Volvo, Reporting) resolve to
        /// their own module color.
        /// </summary>
        private static readonly (string NamespacePrefix, string FillKey, string BorderKey, string HighlightKey)[]
            ModuleAccentMap =
            [
                ("MTM_Receiving_Application.Module_Receiving", "ReceivingAccentBrush", "ReceivingAccentBorderBrush", "ReceivingAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_Settings.Receiving", "ReceivingAccentBrush", "ReceivingAccentBorderBrush", "ReceivingAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_Dunnage", "DunnageAccentBrush", "DunnageAccentBorderBrush", "DunnageAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_Settings.Dunnage", "DunnageAccentBrush", "DunnageAccentBorderBrush", "DunnageAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_Volvo", "VolvoAccentBrush", "VolvoAccentBorderBrush", "VolvoAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_Settings.Volvo", "VolvoAccentBrush", "VolvoAccentBorderBrush", "VolvoAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_Reporting", "ReportingAccentBrush", "ReportingAccentBorderBrush", "ReportingAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_Settings.Reporting", "ReportingAccentBrush", "ReportingAccentBorderBrush", "ReportingAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_ShipRec_Tools", "ShipRecAccentBrush", "ShipRecAccentBorderBrush", "ShipRecAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_Reprint", "ReprintAccentBrush", "ReprintAccentBorderBrush", "ReprintAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_Scanner", "ScannerAccentBrush", "ScannerAccentBorderBrush", "ScannerAccentHighlightBrush"),
                ("MTM_Receiving_Application.Module_Settings", "SettingsAccentBrush", "SettingsAccentBorderBrush", "SettingsAccentHighlightBrush"),
            ];

        /// <summary>
        /// Resolved accent brushes for a module: the accent fill used as the
        /// header background, the theme-aware border brush key used for the user
        /// card and the initials circle, and the fixed highlight brush key used
        /// for the header card border on top of the accent fill.
        /// </summary>
        private sealed record ModuleAccent(
            SolidColorBrush? Fill,
            string BorderKey,
            string HighlightKey
        );

        /// <summary>
        /// Colors the shell header with the active module's accent. The header
        /// card background becomes the module fill (title and back icon turn
        /// white), the header card border becomes a fixed accent highlight, and
        /// the user card and initials circle receive a theme-aware accent border
        /// so they stay visible in light and dark mode. Pages outside the
        /// branded modules keep the neutral theme header. Driven by the page
        /// type so settings pages for a module match their module too.
        /// </summary>
        private void ApplyHeaderAccent(Type? pageType)
        {
            if (HeaderBarBorder is null)
            {
                return;
            }

            var accent = GetModuleAccent(pageType);
            if (accent?.Fill is not null)
            {
                HeaderBarBorder.Background = accent.Fill;
                HeaderBarBorder.BorderBrush =
                    ResolveAppBrush(accent.HighlightKey) ?? accent.Fill;
                HeaderBarBorder.BorderThickness = new Thickness(2);

                PageTitleTextBlock.Foreground = new SolidColorBrush(Colors.White);
                HeaderBackButton.BorderBrush = new SolidColorBrush(
                    ColorHelper.FromArgb(64, 255, 255, 255)
                );
                if (HeaderBackButtonIcon is not null)
                {
                    HeaderBackButtonIcon.Foreground = new SolidColorBrush(Colors.White);
                }

                ApplyUserCardAccentBorder(ResolveThemeAwareBrush(accent.BorderKey));

                // Blend the user card into the accent fill and render the user
                // name in white so the whole header reads consistently on the
                // accent in both light and dark themes.
                UserMenuButton.Background = new SolidColorBrush(Colors.Transparent);
                UserDisplayTextBlock.Foreground = new SolidColorBrush(Colors.White);
                return;
            }

            HeaderBarBorder.Background = ResolveAppBrush("LayerFillColorDefaultBrush");
            HeaderBarBorder.BorderBrush = ResolveAppBrush("CardStrokeColorDefaultBrush");
            HeaderBarBorder.BorderThickness = new Thickness(1);
            PageTitleTextBlock.ClearValue(TextBlock.ForegroundProperty);
            HeaderBackButton.BorderBrush = ResolveAppBrush("CardStrokeColorDefaultBrush");
            HeaderBackButtonIcon?.ClearValue(IconElement.ForegroundProperty);
            UserMenuButton.ClearValue(Button.BackgroundProperty);
            UserDisplayTextBlock.ClearValue(TextBlock.ForegroundProperty);
            ApplyUserCardNeutralBorder();
        }

        /// <summary>
        /// Applies the module accent border to the user card and the initials
        /// circle. Falls back to the theme card stroke when the theme-aware
        /// brush cannot be resolved.
        /// </summary>
        private void ApplyUserCardAccentBorder(SolidColorBrush? accentBorder)
        {
            var borderBrush = accentBorder ?? ResolveAppBrush("CardStrokeColorDefaultBrush");
            if (UserMenuButton is not null)
            {
                UserMenuButton.BorderBrush = borderBrush;
                UserMenuButton.BorderThickness = new Thickness(2);
            }

            if (UserPictureBorder is not null)
            {
                UserPictureBorder.BorderBrush = borderBrush;
                UserPictureBorder.BorderThickness = new Thickness(2);
            }
        }

        /// <summary>
        /// Restores the neutral theme card stroke on the user card and the
        /// initials circle for non-branded pages.
        /// </summary>
        private void ApplyUserCardNeutralBorder()
        {
            var borderBrush = ResolveAppBrush("CardStrokeColorDefaultBrush");
            if (UserMenuButton is not null)
            {
                UserMenuButton.BorderBrush = borderBrush;
                UserMenuButton.BorderThickness = new Thickness(1);
            }

            if (UserPictureBorder is not null)
            {
                UserPictureBorder.BorderBrush = borderBrush;
                UserPictureBorder.BorderThickness = new Thickness(1);
            }
        }

        /// <summary>
        /// Resolves a theme-aware accent border brush from the app theme
        /// dictionaries. The Light theme holds the dark brand accents (visible
        /// on light surfaces) and the Dark theme holds lighter tints (visible on
        /// dark surfaces), so the header borders stay readable in both modes.
        /// Searches the root dictionary and every merged dictionary because the
        /// theme dictionaries live inside the merged ModuleAccentBrushes.xaml.
        /// </summary>
        private SolidColorBrush? ResolveThemeAwareBrush(string key)
        {
            var themeKey = GetThemeKey(
                (Content as FrameworkElement)?.ActualTheme ?? ElementTheme.Default
            );

            if (TryResolveThemeBrush(App.Current.Resources, themeKey, key, out var brush))
            {
                return brush;
            }

            foreach (var merged in App.Current.Resources.MergedDictionaries)
            {
                if (TryResolveThemeBrush(merged, themeKey, key, out brush))
                {
                    return brush;
                }
            }

            return null;
        }

        private static bool TryResolveThemeBrush(
            ResourceDictionary dictionary,
            string themeKey,
            string key,
            out SolidColorBrush? brush
        )
        {
            brush = null;

            if (dictionary.ThemeDictionaries is not { } themeDictionaries)
            {
                return false;
            }

            if (
                themeDictionaries.TryGetValue(themeKey, out var themeDictionary)
                && themeDictionary is ResourceDictionary resourceDictionary
                && resourceDictionary.TryGetValue(key, out var value)
                && value is SolidColorBrush solidBrush
            )
            {
                brush = solidBrush;
                return true;
            }

            return false;
        }

        private static string GetThemeKey(ElementTheme theme) =>
            theme switch
            {
                ElementTheme.Dark => "Dark",
                ElementTheme.Light => "Light",
                _ => Application.Current.RequestedTheme == ApplicationTheme.Dark
                    ? "Dark"
                    : "Light",
            };

        /// <summary>
        /// Maps a page type to its module accent brushes, or null when the page
        /// does not belong to a branded module.
        /// </summary>
        private static ModuleAccent? GetModuleAccent(Type? pageType)
        {
            if (pageType?.FullName is not string fullName)
            {
                return null;
            }

            foreach (var mapping in ModuleAccentMap)
            {
                if (fullName.StartsWith(mapping.NamespacePrefix, StringComparison.Ordinal))
                {
                    return new ModuleAccent(
                        ResolveAppBrush(mapping.FillKey),
                        mapping.BorderKey,
                        mapping.HighlightKey
                    );
                }
            }

            return null;
        }

        private static SolidColorBrush? ResolveAppBrush(string key) =>
            App.Current.Resources[key] as SolidColorBrush;

        private void ResetHeaderContext() { }

        private void UpdateHeader(IViewModel_HeaderTitleProvider viewModel)
        {
            UpdateHeaderText(viewModel.CurrentHeaderTitle);
        }

        private void TrackHeaderProvider(IViewModel_HeaderTitleProvider viewModel)
        {
            UpdateHeader(viewModel);

            _currentPropertyChangedHandler = (s, args) =>
            {
                if (
                    string.IsNullOrEmpty(args.PropertyName)
                    || args.PropertyName
                        == nameof(IViewModel_HeaderTitleProvider.CurrentHeaderTitle)
                )
                {
                    UpdateHeader(viewModel);
                }
            };

            viewModel.PropertyChanged += _currentPropertyChangedHandler;
            _currentWorkflowViewModel = viewModel;
        }

        private void SyncPageHeader(object? content, string? fallbackTitle = null)
        {
            ClearHeaderSubscription();
            _headerBackNavigation.ClearBackAction();

            ApplyHeaderAccent(content?.GetType());

            var headerProvider = ResolveHeaderProvider(content);
            if (headerProvider != null)
            {
                TrackHeaderProvider(headerProvider);
                return;
            }

            if (!string.IsNullOrWhiteSpace(fallbackTitle))
            {
                UpdateHeaderText(fallbackTitle);
                ResetHeaderContext();
            }
            else
            {
                ResetHeaderContext();
            }
        }

        private static IViewModel_HeaderTitleProvider? ResolveHeaderProvider(object? content)
        {
            if (content is null)
            {
                return null;
            }

            if (content is IViewModel_HeaderTitleProvider directProvider)
            {
                return directProvider;
            }

            if (
                content is FrameworkElement frameworkElement
                && frameworkElement.DataContext
                    is IViewModel_HeaderTitleProvider dataContextProvider
            )
            {
                return dataContextProvider;
            }

            var viewModelProperty = content.GetType().GetProperty("ViewModel");
            return viewModelProperty?.GetValue(content) as IViewModel_HeaderTitleProvider;
        }

        private static string GetFallbackTitle(Type? pageType)
        {
            if (pageType is null)
            {
                return string.Empty;
            }

            return _navRoutes.Values.FirstOrDefault(route => route.PageType == pageType).Title
                ?? string.Empty;
        }

        private void ContentFrame_Navigated(
            object sender,
            Microsoft.UI.Xaml.Navigation.NavigationEventArgs e
        )
        {
            _logger.LogInfo(
                $"ContentFrame_Navigated: Navigated to {e.SourcePageType?.Name ?? "Unknown"}"
            );

            SyncPageHeader(ContentFrame.Content, GetFallbackTitle(e.SourcePageType));
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
                return;
            }

            UserDisplayTextBlock.Text = "Not Logged In";
            UserPicture.DisplayName = string.Empty;
        }

        private async void UserLogOutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var reloginSucceeded = await ViewModel.LogOutAndPromptForLoginAsync();
                if (reloginSucceeded)
                {
                    await ResetForAuthenticatedUserAsync();
                }
                else
                {
                    UpdateUserDisplay();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to complete the logout and re-login flow.",
                    ex,
                    "MainWindow"
                );
            }
        }

        public async Task ResetForAuthenticatedUserAsync()
        {
            _isSettingsMode = false;
            _currentSettingsPageType = null;
            _settingsReturnRouteTag = null;
            View_Settings_CoreWindow.SetActiveHost(null);
            ApplyNavigationMode(isSettingsMode: false);
            UpdateSettingsToggleText();

            await ClearCurrentModuleStateAsync();
            UpdateUserDisplay();

            SetNavigationSelectionByTag("ReceivingWorkflowView");

            NavigateWithDI(typeof(Module_Receiving.Views.View_Receiving_Workflow));
        }

        private async Task ClearCurrentModuleStateAsync()
        {
            if (ContentFrame.Content is Module_Receiving.Views.View_Receiving_Workflow)
            {
                var receivingWorkflow =
                    _serviceProvider.GetRequiredService<IService_ReceivingWorkflow>();
                await receivingWorkflow.ResetWorkflowAsync();
                return;
            }

            if (ContentFrame.Content is Module_Dunnage.Views.View_Dunnage_WorkflowView)
            {
                var dunnageWorkflow =
                    _serviceProvider.GetRequiredService<IService_DunnageWorkflow>();
                dunnageWorkflow.ClearSession();
            }
        }

        public async Task RefreshSettingsDependentStateAsync()
        {
            if (ContentFrame.Content is Module_Receiving.Views.View_Receiving_Workflow workflowView)
            {
                await workflowView.RefreshSettingsDependentStateAsync();
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
                titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(
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
                titleBar.ButtonForegroundColor = foregroundColor;
                titleBar.ButtonHoverForegroundColor = foregroundColor;
                titleBar.ButtonPressedForegroundColor = foregroundColor;
                titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255, 96, 96, 96);
            }
        }

        /// <summary>
        /// Applies a theme-aware color to the title-bar app title: the primary
        /// text fill for an active window (near-black in Light mode, near-white
        /// in Dark mode) and a muted secondary fill when the window is
        /// deactivated.
        /// </summary>
        private void UpdateTitleBarTextColor(bool isActive)
        {
            var brush = isActive
                ? ResolveAppBrush("TextFillColorPrimaryBrush")
                    ?? new SolidColorBrush(Colors.Black)
                : ResolveAppBrush("TextFillColorSecondaryBrush")
                    ?? ResolveAppBrush("TextFillColorDisabledBrush")
                    ?? new SolidColorBrush(Colors.Gray);
            TitleBarTextBlock.Foreground = brush;
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
        private void AppTitleBar_SizeChanged(
            object sender,
            Microsoft.UI.Xaml.SizeChangedEventArgs e
        )
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
                RightPaddingColumn.Width = new Microsoft.UI.Xaml.GridLength(
                    AppWindow.TitleBar.RightInset / scaleAdjustment
                );
                LeftPaddingColumn.Width = new Microsoft.UI.Xaml.GridLength(
                    AppWindow.TitleBar.LeftInset / scaleAdjustment
                );

                // Define passthrough regions for interactive elements
                var rectArray = new System.Collections.Generic.List<RectInt32>();

                // Add PaneToggleButton region
                if (PaneToggleButton != null)
                {
                    var transform = PaneToggleButton.TransformToVisual(null);
                    var bounds = transform.TransformBounds(
                        new Windows.Foundation.Rect(
                            0,
                            0,
                            PaneToggleButton.ActualWidth,
                            PaneToggleButton.ActualHeight
                        )
                    );
                    rectArray.Add(GetRect(bounds, scaleAdjustment));
                }

                // Add AutoSuggestBox region
                if (TitleBarSearchBox != null)
                {
                    var transform = TitleBarSearchBox.TransformToVisual(null);
                    var bounds = transform.TransformBounds(
                        new Windows.Foundation.Rect(
                            0,
                            0,
                            TitleBarSearchBox.ActualWidth,
                            TitleBarSearchBox.ActualHeight
                        )
                    );
                    rectArray.Add(GetRect(bounds, scaleAdjustment));
                }

                // Set the interactive regions
                if (rectArray.Count > 0)
                {
                    InputNonClientPointerSource nonClientInputSrc =
                        InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
                    nonClientInputSrc.SetRegionRects(
                        NonClientRegionKind.Passthrough,
                        rectArray.ToArray()
                    );
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
        /// <param name="fallbackTitle"></param>
        private bool NavigateWithDI(Type pageType, string? fallbackTitle = null)
        {
            try
            {
                // Resolve the view from DI container
                var page = _serviceProvider.GetService(pageType);
                if (page == null)
                {
                    _logger.LogError(
                        $"Failed to resolve view type: {pageType.Name}",
                        null,
                        "MainWindow"
                    );
                    return false;
                }

                SetContentPage((Page)page, fallbackTitle ?? GetFallbackTitle(pageType));
                _logger.LogInfo($"NavigateWithDI: Navigated to {pageType.Name}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Navigation failed for {pageType.Name}: {ex.Message}",
                    ex,
                    "MainWindow"
                );
                return false;
            }
        }

        /// <summary>
        /// Handle navigation failures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentFrame_NavigationFailed(
            object sender,
            Microsoft.UI.Xaml.Navigation.NavigationFailedEventArgs e
        )
        {
            _logger.LogError(
                $"Navigation failed: {e.Exception?.Message}",
                e.Exception,
                "MainWindow"
            );
            e.Handled = true;

            // Try to resolve using DI as fallback
            if (e.SourcePageType != null)
            {
                NavigateWithDI(e.SourcePageType, GetFallbackTitle(e.SourcePageType));
            }
        }
    }
}

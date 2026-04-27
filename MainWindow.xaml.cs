using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
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
        private readonly List<object> _applicationMenuItems = new();
        private readonly List<object> _applicationFooterItems = new();
        private readonly List<object> _settingsMenuItems = new();
        private bool _hasNavigatedOnStartup = false;
        private bool _isUpdatingNavSelection;
        private bool _isSettingsMode;
        private System.ComponentModel.INotifyPropertyChanged? _currentWorkflowViewModel;
        private System.ComponentModel.PropertyChangedEventHandler? _currentPropertyChangedHandler;
        private string? _settingsReturnRouteTag;
        private Type? _currentSettingsPageType;

        private enum SearchDestinationKind
        {
            FrameRoute,
            SettingsPage,
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
                params string[] aliases
            )
            {
                Key = key;
                Label = label;
                Kind = kind;
                RouteTag = routeTag;
                SettingsPageType = settingsPageType;
                Detail = detail;
                _aliases = aliases;
            }

            public string Key { get; }
            public string Label { get; }
            public SearchDestinationKind Kind { get; }
            public string? RouteTag { get; }
            public Type? SettingsPageType { get; }
            public string Detail { get; }

            public IEnumerable<string> SearchTerms
            {
                get
                {
                    yield return Label;

                    if (!string.IsNullOrWhiteSpace(RouteTag))
                    {
                        yield return RouteTag;
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
            IServiceProvider serviceProvider
        )
        {
            InitializeComponent();
            ViewModel = viewModel;
            _sessionManager = sessionManager;
            _logger = logger;
            _serviceProvider = serviceProvider;

            _applicationMenuItems.AddRange(NavView.MenuItems.Cast<object>());
            _applicationFooterItems.AddRange(NavView.FooterMenuItems.Cast<object>());
            _settingsMenuItems.AddRange(CreateSettingsNavigationItems());

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

            // Subscribe to navigation events once
            ContentFrame.Navigated += ContentFrame_Navigated;

            // Wire up title bar events
            AppTitleBar.Loaded += AppTitleBar_Loaded;
            AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;

            ApplyNavigationMode(isSettingsMode: false);
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            // Update title bar text color based on activation state
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                TitleBarTextBlock.Foreground = (Microsoft.UI.Xaml.Media.SolidColorBrush)
                    App.Current.Resources["WindowCaptionForegroundDisabled"];
            }
            else
            {
                TitleBarTextBlock.Foreground = (Microsoft.UI.Xaml.Media.SolidColorBrush)
                    App.Current.Resources["WindowCaptionForeground"];
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

        private static readonly Dictionary<string, (Type PageType, string Title)> _navRoutes = new()
        {
            ["ReceivingWorkflowView"] = (
                typeof(Module_Receiving.Views.View_Receiving_Workflow),
                string.Empty
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

        private static readonly Dictionary<string, SearchDestination> _searchDestinationsByKey =
            _searchDestinations.ToDictionary(
                destination => destination.Key,
                StringComparer.Ordinal
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
                    typeof(Module_Settings.Reporting.Views.View_Settings_Reporting_NavigationHub),
                    "Reporting Settings",
                    "Reporting settings placeholder",
                    "reporting settings",
                    "reporting navigation"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Volvo.Views.View_Settings_Volvo_NavigationHub),
                    "Volvo Settings",
                    "Volvo settings hub",
                    "volvo settings",
                    "volvo navigation"
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

            sender.ItemsSource = GetSearchMatches(queryText).Take(8).ToList();
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

        private void TrackHeaderProvider(IViewModel_HeaderTitleProvider viewModel)
        {
            UpdateHeaderText(viewModel.CurrentHeaderTitle);

            _currentPropertyChangedHandler = (s, args) =>
            {
                if (args.PropertyName == nameof(IViewModel_HeaderTitleProvider.CurrentHeaderTitle))
                {
                    UpdateHeaderText(viewModel.CurrentHeaderTitle);
                }
            };

            viewModel.PropertyChanged += _currentPropertyChangedHandler;
            _currentWorkflowViewModel = viewModel;
        }

        private void SyncPageHeader(object? content, string? fallbackTitle = null)
        {
            ClearHeaderSubscription();

            var headerProvider = ResolveHeaderProvider(content);
            if (headerProvider != null)
            {
                TrackHeaderProvider(headerProvider);
                return;
            }

            if (!string.IsNullOrWhiteSpace(fallbackTitle))
            {
                UpdateHeaderText(fallbackTitle);
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
                    "MTMIcon.ico",
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
                    _logger?.LogInfo(
                        $"Window icon set successfully: {foundIconPath}",
                        "MainWindow"
                    );
                }
                else
                {
                    _logger?.LogWarning(
                        $"Icon file not found. Searched paths: {string.Join(", ", iconPaths)}",
                        "MainWindow"
                    );
                    _logger?.LogWarning(
                        $"Current directory: {AppContext.BaseDirectory}",
                        "MainWindow"
                    );
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Dialogs;
using MTM_Receiving_Application.Module_Core.Models.InforVisual;

namespace MTM_Receiving_Application
{
    /// <summary>
    /// The title-bar search catalog and the one route opener both the navigation pane and the
    /// search box use. Kept in its own file so the page's shell wiring and its search audit never
    /// edit the same source.
    /// </summary>
    public sealed partial class MainWindow
    {
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
            ["ReconciliationLauncherPage"] = (
                typeof(Module_Reconciliation.Views.View_Reconciliation_Launcher),
                "Receiving Validator"
            ),
            ["ShipRecToolsPage"] = (
                typeof(Module_ShipRec_Tools.Views.View_ShipRecTools_Main),
                string.Empty
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
                await NavigateToRouteTagAsync(tag);
                return;
            }

            if (!_navRoutes.TryGetValue(tag, out _))
            {
                return;
            }

            if (!await NavigateToRouteTagAsync(tag))
            {
                SetNavigationSelectionByTag(GetCurrentRouteTag());
            }
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
                    "receiving modes",
                    "guided receiving",
                    "manual receiving",
                    "edit receiving"
                ),
                CreateFrameDestination(
                    "DunnageLabelPage",
                    "Non-Inventoried Labels",
                    "Guided, edit, and image-search non-inventoried labels workflow",
                    "dunnage",
                    "non-inventoried labels",
                    "non-inventoried labels workflow",
                    "guided non-inventoried labels",
                    "edit non-inventoried labels",
                    "non-inventoried labels image search"
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
                    "ReconciliationLauncherPage",
                    "Receiving Validator",
                    "Validation workbench for receiving movements, exception buckets, and write-back",
                    "receiving validator",
                    "validator",
                    "reconciliation",
                    "movement discovery",
                    "validation workbench"
                ),
                CreateFrameDestination(
                    "ShipRecToolsPage",
                    "Extra Tools",
                    "Hub of extra shipping, receiving, and reporting tools",
                    "extra",
                    "extra tools",
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
                    typeof(Module_Settings.Core.Views.View_Settings_System),
                    "System Settings",
                    "Core system settings and defaults",
                    "system",
                    "system settings",
                    "core system"
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
                    typeof(Module_Settings.Core.Views.View_Settings_LotMarking),
                    "Heat / Lot Management",
                    "Choose how lots are marked, which shapes and colors are offered, and which parts receive markings",
                    "heat lot management",
                    "lot marking",
                    "heat number",
                    "lot marking palette",
                    "marked parts"
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
                    typeof(Module_Settings.Reconciliation.Views.View_Settings_Reconciliation_Main),
                    "Receiving Validator Settings",
                    "Movement discovery and strict validation windows used by the Receiving Validator",
                    "receiving validator settings",
                    "reconciliation windows",
                    "movement discovery window",
                    "strict validation window"
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
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_LabelPaths),
                    "Receiving Label Files",
                    "Configure the LabelView template paths used by Receiving workflow label buttons",
                    "receiving label paths",
                    "receiving label files",
                    "receiving labelview paths"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_VendorVariables),
                    "Custom Vendor Variables",
                    "Assign vendor-specific variable names used for Receiving vendor data",
                    "custom vendor variables",
                    "vendor variables",
                    "receiving vendor variables",
                    "vendor variable mapping"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_WeldedParts),
                    "Welded Part Numbers",
                    "Maintain the part numbers whose coil needs its inner diameter welded",
                    "welded part numbers",
                    "welded parts",
                    "welded coils",
                    "receiving welded parts"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_CategoryHub),
                    "Non-Inventoried Labels Settings",
                    "Non-Inventoried Labels settings categories",
                    "non-inventoried labels settings",
                    "non-inventoried labels categories",
                    "non-inventoried labels navigation"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_PersonalDefaults),
                    "Non-Inventoried Labels Personal Defaults",
                    "User-specific default location and personal image preferences for Non-Inventoried Labels",
                    "non-inventoried labels personal defaults",
                    "non-inventoried labels preferences"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_ImageAssets),
                    "Non-Inventoried Labels Image Assets",
                    "Shared Non-Inventoried Labels image folder and file size limits",
                    "non-inventoried labels image assets",
                    "non-inventoried labels image folder",
                    "non-inventoried labels image settings"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_ImagePresentation),
                    "Non-Inventoried Labels Image Presentation",
                    "Application-wide Non-Inventoried Labels image display settings",
                    "non-inventoried labels image presentation",
                    "non-inventoried labels display settings",
                    "non-inventoried labels ui"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_WorkflowVisuals),
                    "Non-Inventoried Labels Workflow Visuals",
                    "Image behavior during the Non-Inventoried Labels workflow",
                    "non-inventoried labels workflow visuals",
                    "non-inventoried labels workflow image settings",
                    "non-inventoried labels workflow"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_KeyboardShortcuts),
                    "Non-Inventoried Labels Keyboard Shortcuts",
                    "Configure the Non-Inventoried Labels workflow shortcuts and the simple navigation toggle",
                    "non-inventoried labels shortcuts",
                    "non-inventoried labels keyboard shortcuts",
                    "keyboard shortcuts non-inventoried labels"
                ),
                CreateSettingsDestination(
                    typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_LabelPaths),
                    "Non-Inventoried Label Files",
                    "Configure the LabelView template path used for non-inventoried labels",
                    "non-inventoried label paths",
                    "non-inventoried label files",
                    "non-inventoried labelview paths"
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
                CreateSettingsDestination(
                    typeof(Module_Settings.Volvo.Views.View_Settings_Volvo_LabelPaths),
                    "Volvo Label Files",
                    "Configure the LabelView template path used for Volvo labels",
                    "volvo label paths",
                    "volvo label files",
                    "volvo labelview paths"
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
                    "Show non-inventoried destinations",
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

            sender.ItemsSource = GetOfferedSearchMatches(queryText).Take(24).ToList();
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
                var commandMatches = GetOfferedSearchMatches(normalizedQuery).Take(50).ToList();
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

            var matches = GetOfferedSearchMatches(queryText).Take(10).ToList();
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
                TitleBarSearchBox.ItemsSource = GetOfferedSearchMatches(commandText)
                    .Take(24)
                    .ToList();
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

        /// <summary>
        /// Opens a route, whether the navigation pane or the search box asked for it. Both paths
        /// meet the same scanner allow-list check and the same leave confirmation here, so a page
        /// cannot be reached from search while navigation refuses it.
        /// </summary>
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

            // Scanner is gated by a plant-wide allow-list that an admin can change at any time,
            // so re-read it before allowing entry instead of trusting the cached nav state.
            if (routeTag == "ScannerMainPage" && await RefreshScannerAccessAsync() is false)
            {
                NotifyScannerAccessDenied();
                return false;
            }

            if (!await ClearModuleDraftStateBeforeNavigationAsync(route.PageType, route.Title))
            {
                return false;
            }

            SetNavigationSelectionByTag(routeTag);
            return NavigateWithDI(route.PageType, route.Title);
        }

        /// <summary>
        /// Drops the pages the signed-in user may not open from the results the search box offers,
        /// so the list needs no change when the allow-list grows or shrinks.
        /// </summary>
        private static IReadOnlyList<SearchDestination> FilterOfferedResults(
            IEnumerable<SearchDestination> matches,
            bool isScannerAllowed
        )
        {
            if (isScannerAllowed)
            {
                return matches.ToList();
            }

            return matches
                .Where(destination =>
                    string.Equals(destination.RouteTag, "ScannerMainPage", StringComparison.Ordinal)
                        is false
                )
                .ToList();
        }

        private IReadOnlyList<SearchDestination> GetOfferedSearchMatches(string queryText) =>
            FilterOfferedResults(GetSearchMatches(queryText), _isScannerAccessAllowed);
    }
}

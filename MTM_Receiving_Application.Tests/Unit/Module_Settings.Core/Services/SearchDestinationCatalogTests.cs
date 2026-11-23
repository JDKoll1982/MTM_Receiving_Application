using System.Collections;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MTM_Receiving_Application.Infrastructure.DependencyInjection;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Tests.Unit.Module_Settings.Core.Services;

public sealed class SearchDestinationCatalogTests
{
    private static readonly Type WindowType = typeof(MainWindow);
    private static readonly Type DestinationType =
        WindowType.GetNestedType("SearchDestination", BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("SearchDestination is missing from MainWindow");

    [Fact]
    public void Catalog_ShouldNameEveryRouteInNavRoutes_ExceptTheDisabledModule()
    {
        var navRouteTags = ((IDictionary)
                WindowType
                    .GetField("_navRoutes", BindingFlags.NonPublic | BindingFlags.Static)!
                    .GetValue(null)!)
            .Keys.Cast<string>()
            .ToArray();

        var catalogRouteTags = Catalog()
            .Select(RouteTag)
            .Where(tag => tag is not null)
            .ToArray();

        foreach (var routeTag in navRouteTags)
        {
            catalogRouteTags
                .Should()
                .Contain(
                    routeTag,
                    $"'{routeTag}' is reachable from the navigation pane, so search must find it"
                );
        }
    }

    [Fact]
    public void Catalog_ShouldNameEveryCardOnTheReceivingAndNonInventoriedLabelsHubs()
    {
        var catalogPageTypes = Catalog()
            .Select(SettingsPageType)
            .Where(type => type is not null)
            .ToHashSet();

        ViewModel_SettingsNavigationHubBase[] hubs =
        [
            new global::MTM_Receiving_Application.Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_CategoryHub(
                Pagination().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            ),
            new global::MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_CategoryHub(
                Pagination().Object,
                new Mock<IService_ErrorHandler>().Object,
                new Mock<IService_LoggingUtility>().Object,
                new Mock<IService_Notification>().Object
            ),
        ];

        foreach (var hub in hubs)
        {
            hub.Steps.Should().NotBeEmpty($"{hub.GetType().Name} has to offer its cards");

            foreach (var step in hub.Steps)
            {
                catalogPageTypes
                    .Should()
                    .Contain(
                        step.ViewType,
                        $"'{step.Title}' is a card on {hub.GetType().Name}, so search has to offer the page it opens"
                    );
            }
        }
    }

    [Fact]
    public void Catalog_ShouldOfferNoRetiredPageOrInnerStep_AndEverySettingsDestinationShouldResolve()
    {
        var catalog = Catalog().ToArray();

        catalog
            .SelectMany(Terms)
            .Should()
            .NotContain(
                term => term.Contains("mode selection", StringComparison.OrdinalIgnoreCase),
                "the mode-selection screen was deleted, so no alias may offer it"
            );

        var registered = RegisteredServiceTypes();

        foreach (var settingsDestination in catalog.Where(item => SettingsPageType(item) is not null))
        {
            registered
                .Should()
                .Contain(
                    SettingsPageType(settingsDestination)!,
                    $"search offers '{Label(settingsDestination)}', so it has to resolve"
                );
        }

        AssertEverySettingsPageCanBeActivated();
    }

    [Fact]
    public void OfferedResults_ShouldHideTheScannerEntry_WhenThePolicyRefusesTheUser_AndRestoreItWhenTheListChanges()
    {
        var filter = WindowType.GetMethod(
            "FilterOfferedResults",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        filter.Should().NotBeNull("the search box filters its results through one place");

        var all = CatalogList();
        var refused = Invoke(filter!, all, isScannerAllowed: false);
        var allowed = Invoke(filter!, all, isScannerAllowed: true);

        refused.Select(RouteTag).Should().NotContain("ScannerMainPage");
        allowed.Select(RouteTag).Should().Contain("ScannerMainPage");
        refused
            .Should()
            .HaveCount(
                allowed.Length - 1,
                "only the scanner entry is refused, so the list needs no change when the allow-list grows"
            );
    }

    [Fact]
    public void BothEntryPaths_ShouldUseTheOneOpenerThatCarriesTheScannerGateAndTheLeaveConfirmation()
    {
        var root = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")
        );
        var searchWiring = File.ReadAllText(Path.Combine(root, "MainWindow.Search.cs"));

        var openerStarts = searchWiring.IndexOf(
            "private async Task<bool> NavigateToRouteTagAsync(",
            StringComparison.Ordinal
        );
        openerStarts.Should().BeGreaterThan(0);

        var openerEnds = searchWiring.IndexOf(
            "private static IReadOnlyList<SearchDestination> FilterOfferedResults(",
            StringComparison.Ordinal
        );
        openerEnds.Should().BeGreaterThan(openerStarts);

        var opener = searchWiring[openerStarts..openerEnds];
        opener
            .Should()
            .Contain("RefreshScannerAccessAsync()", "the opener carries the scanner allow-list gate");
        opener
            .Should()
            .Contain(
                "ClearModuleDraftStateBeforeNavigationAsync",
                "the opener carries the leave confirmation navigation already used"
            );

        var paneHandlerStarts = searchWiring.IndexOf(
            "private async void NavView_SelectionChanged(",
            StringComparison.Ordinal
        );
        paneHandlerStarts.Should().BeGreaterThan(0);
        var paneHandler = searchWiring[paneHandlerStarts..openerStarts];
        paneHandler
            .Should()
            .Contain(
                "NavigateToRouteTagAsync(tag)",
                "the navigation pane reaches a page only through the opener"
            );

        var searchPathStarts = searchWiring.IndexOf(
            "private async Task NavigateToSearchDestinationAsync(",
            StringComparison.Ordinal
        );
        var searchPath = searchWiring[searchPathStarts..openerStarts];
        searchPath
            .Should()
            .Contain(
                "NavigateToRouteTagAsync(destination.RouteTag!)",
                "the search box reaches a page only through the same opener"
            );
    }

    /// <summary>
    /// A registered page is not the same as an openable one: a settings page is constructed through
    /// its view model, so every dependency that view model declares has to be registered too.
    /// </summary>
    private static void AssertEverySettingsPageCanBeActivated()
    {
        var registered = RegisteredServiceTypes();
        var catalog = Catalog().Where(item => SettingsPageType(item) is not null);

        foreach (var destination in catalog)
        {
            var pageType = SettingsPageType(destination)!;
            var viewModelType = pageType
                .GetConstructors()
                .Select(constructor => constructor.GetParameters().FirstOrDefault()?.ParameterType)
                .FirstOrDefault(type =>
                    type is not null && type.Name.StartsWith("ViewModel_", StringComparison.Ordinal)
                );

            if (viewModelType is null)
            {
                // A hub page may be composed differently; the pages this check guards all follow
                // the one-view-model convention.
                continue;
            }

            foreach (
                var parameter in viewModelType.GetConstructors().First().GetParameters()
            )
            {
                registered
                    .Should()
                    .Contain(
                        parameter.ParameterType,
                        $"{viewModelType.Name} needs {parameter.ParameterType.Name}, so offering '{Label(destination)}' in search means it has to be registered"
                    );
            }
        }
    }

    private static HashSet<Type> RegisteredServiceTypes()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:MySql"] =
                        "Server=172.16.1.104;Port=3306;Database=test;Uid=test;Pwd=test;",
                    ["ConnectionStrings:InforVisual"] =
                        "Server=172.16.1.104;Database=TEST;User Id=test;Password=test;ApplicationIntent=ReadOnly;TrustServerCertificate=True;",
                }
            )
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddCoreServices(configuration);
        services.AddCqrsInfrastructure();
        services.AddModuleServices(configuration);

        return services.Select(descriptor => descriptor.ServiceType).ToHashSet();
    }

    private static Mock<IService_SettingsPagination> Pagination()
    {
        var pagination = new Mock<IService_SettingsPagination>();
        pagination
            .Setup(service => service.GetTotalPages(It.IsAny<int>()))
            .Returns<int>(count => count == 0 ? 0 : 1);
        pagination.Setup(service => service.ShouldShowPagination(It.IsAny<int>())).Returns(false);
        pagination
            .Setup(service => service.GetPageIndices(It.IsAny<int>(), It.IsAny<int>()))
            .Returns<int, int>((count, _) => Enumerable.Range(0, count).ToArray());
        return pagination;
    }

    private static IEnumerable<object> Catalog() => CatalogList().Cast<object>();

    private static IEnumerable CatalogList() =>
        (IEnumerable)
            WindowType
                .GetField("_searchDestinations", BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null)!;

    private static object[] Invoke(MethodInfo filter, object matches, bool isScannerAllowed) =>
        ((IEnumerable)filter.Invoke(null, [matches, isScannerAllowed])!).Cast<object>().ToArray();

    private static string? RouteTag(object destination) =>
        (string?)DestinationType.GetProperty("RouteTag")!.GetValue(destination);

    private static string Label(object destination) =>
        (string)DestinationType.GetProperty("Label")!.GetValue(destination)!;

    private static Type? SettingsPageType(object destination) =>
        (Type?)DestinationType.GetProperty("SettingsPageType")!.GetValue(destination);

    private static IEnumerable<string> Terms(object destination) =>
        ((IEnumerable<string>)DestinationType.GetProperty("SearchTerms")!.GetValue(destination)!);
}

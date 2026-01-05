using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Services.Database;
using MTM_Receiving_Application.Module_Core.Services.Authentication;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Volvo.Services;
using MTM_Receiving_Application.Module_Routing.Services;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Routing.ViewModels;
using MTM_Receiving_Application.Module_Settings.ViewModels;
using MTM_Receiving_Application.Module_Settings.Services;
using MTM_Receiving_Application.Module_Settings.Interfaces;
using MTM_Receiving_Application.Module_Settings.Views;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Shared.Views;
using MTM_Receiving_Application.Module_Core.ViewModels.Main;
using MTM_Receiving_Application.Module_Core.Views.Main;
using MTM_Receiving_Application.Module_Core.Models.Systems;

using MTM_Receiving_Application.Module_Core.Services;

using MTM_Receiving_Application.Module_Core.Services.Startup;
using MTM_Receiving_Application.Module_Core.Services.UI;
using MTM_Receiving_Application.Module_Reporting.Data;
using MTM_Receiving_Application.Module_Reporting.Services;
using MTM_Receiving_Application.Module_Reporting.ViewModels;
using MTM_Receiving_Application.Module_Reporting.Views;

namespace MTM_Receiving_Application;

/// <summary>
/// Provides application-specific behavior with dependency injection support
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    /// <summary>
    /// Gets the main window for the application
    /// </summary>
    public static Window? MainWindow { get; internal set; }

    /// <summary>
    /// Initializes the singleton application object and sets up dependency injection
    /// </summary>
    public App()
    {
        InitializeComponent();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Core Services
                services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
                services.AddSingleton<IService_LoggingUtility, Service_LoggingUtility>();
                services.AddSingleton<IService_Notification, Service_Notification>();
                services.AddSingleton<IService_Focus, Service_Focus>();

                // Authentication Services
                var mySqlConnectionString = Helper_Database_Variables.GetConnectionString();
                var inforVisualConnectionString = Helper_Database_Variables.GetInforVisualConnectionString();

                services.AddSingleton(sp => new Dao_User(mySqlConnectionString));

                // Register NEW MySQL DAOs (Singleton - stateless data access)
                services.AddSingleton(sp => new Dao_ReceivingLoad(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_ReceivingLine(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_PackageTypePreference(mySqlConnectionString));

                // Register Dunnage DAOs (Singleton)
                services.AddSingleton(sp => new Dao_DunnageLoad(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_DunnageType(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_DunnagePart(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_DunnageSpec(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_InventoriedDunnage(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_DunnageCustomField(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_DunnageUserPreference(mySqlConnectionString));

                // Register Routing DAOs (Singleton - 001-routing-module)
                services.AddSingleton(sp => new Dao_Routing_Label(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_Routing_Recipient(mySqlConnectionString));
                
                // Register NEW Routing Module DAOs (Phase 2 implementation)
                services.AddSingleton(sp => new Dao_RoutingLabel(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_RoutingRecipient(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_RoutingOtherReason(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_RoutingUsageTracking(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_RoutingUserPreference(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_RoutingLabelHistory(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_InforVisualPO(inforVisualConnectionString));

                // Register Volvo DAOs (Singleton)
                services.AddSingleton(sp => new Dao_VolvoShipment(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_VolvoShipmentLine(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_VolvoPart(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_VolvoPartComponent(mySqlConnectionString));

                // Register Routing Services
                services.AddSingleton<IService_RoutingWorkflow, Service_RoutingWorkflow>();
                services.AddSingleton<IService_Routing, Service_Routing>();
                services.AddSingleton<IService_Routing_History, Service_Routing_History>();
                services.AddSingleton<IService_Routing_RecipientLookup, Service_Routing_RecipientLookup>();
                
                // Register NEW Routing Module Services (Phase 2 implementation)
                services.AddSingleton<IRoutingService>(sp =>
                {
                    var daoLabel = sp.GetRequiredService<Dao_RoutingLabel>();
                    var daoHistory = sp.GetRequiredService<Dao_RoutingLabelHistory>();
                    var inforVisualService = sp.GetRequiredService<IRoutingInforVisualService>();
                    var usageTrackingService = sp.GetRequiredService<IRoutingUsageTrackingService>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    var configuration = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                    return new RoutingService(daoLabel, daoHistory, inforVisualService, usageTrackingService, logger, configuration);
                });
                services.AddSingleton<IRoutingInforVisualService>(sp =>
                {
                    var daoInforVisual = sp.GetRequiredService<Dao_InforVisualPO>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new RoutingInforVisualService(daoInforVisual, logger);
                });
                services.AddSingleton<IRoutingRecipientService>(sp =>
                {
                    var daoRecipient = sp.GetRequiredService<Dao_RoutingRecipient>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new RoutingRecipientService(daoRecipient, logger);
                });
                services.AddSingleton<IRoutingUsageTrackingService>(sp =>
                {
                    var daoUsageTracking = sp.GetRequiredService<Dao_RoutingUsageTracking>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new RoutingUsageTrackingService(daoUsageTracking, logger);
                });
                services.AddSingleton<IRoutingUserPreferenceService>(sp =>
                {
                    var daoUserPreference = sp.GetRequiredService<Dao_RoutingUserPreference>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new RoutingUserPreferenceService(daoUserPreference, logger);
                });

                // Register NEW Infor Visual DAOs (READ-ONLY)
                services.AddSingleton(sp =>
                {
                    var logger = sp.GetService<IService_LoggingUtility>();
                    return new Dao_InforVisualPO(inforVisualConnectionString, logger);
                });
                services.AddSingleton(sp =>
                {
                    var logger = sp.GetService<IService_LoggingUtility>();
                    return new Dao_InforVisualPart(inforVisualConnectionString, logger);
                });
                services.AddSingleton<IService_Dispatcher>(sp =>
                {
                    var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
                    return new Service_Dispatcher(dispatcherQueue);
                });
                services.AddSingleton<IService_Window, Service_Window>();
                services.AddSingleton<IService_Help, Module_Core.Services.Help.Service_Help>();
                services.AddSingleton<IService_Authentication>(sp =>
                {
                    var daoUser = sp.GetRequiredService<Dao_User>();
                    var errorHandler = sp.GetRequiredService<IService_ErrorHandler>();
                    return new Service_Authentication(daoUser, errorHandler);
                });
                services.AddSingleton<IService_UserSessionManager>(sp =>
                {
                    var daoUser = sp.GetRequiredService<Dao_User>();
                    var dispatcherService = sp.GetRequiredService<IService_Dispatcher>();
                    return new Service_UserSessionManager(daoUser, dispatcherService);
                });
                services.AddTransient<IService_UserPreferences, Service_UserPreferences>();

                // Startup Service
                services.AddTransient<IService_OnStartup_AppLifecycle, Service_OnStartup_AppLifecycle>();

                // NEW: Infor Visual Connection Service with mock data support
                var useMockData = true; // Set to true to use mock data instead of connecting to VISUAL server
                services.AddSingleton(sp =>
                {
                    var logger = sp.GetService<IService_LoggingUtility>();
                    return new Dao_InforVisualConnection(
                        Helper_Database_Variables.GetInforVisualConnectionString(), logger);
                });
                services.AddSingleton<IService_InforVisual>(sp =>
                {
                    var dao = sp.GetRequiredService<Dao_InforVisualConnection>();
                    var logger = sp.GetService<IService_LoggingUtility>();
                    return new Service_InforVisualConnect(dao, useMockData, logger);
                });

                // Receiving Workflow Services (003-database-foundation)
                services.AddSingleton<IService_MySQL_Receiving>(sp => { var logger = sp.GetRequiredService<IService_LoggingUtility>(); return new Service_MySQL_Receiving(Helper_Database_Variables.GetConnectionString(), logger); });
                services.AddTransient<IService_MySQL_ReceivingLine, Service_MySQL_ReceivingLine>();
                services.AddSingleton<IService_MySQL_PackagePreferences>(sp => new Service_MySQL_PackagePreferences(Helper_Database_Variables.GetConnectionString()));
                services.AddSingleton<IService_SessionManager>(sp => { var logger = sp.GetRequiredService<IService_LoggingUtility>(); return new Service_SessionManager(logger); });
                services.AddSingleton<IService_CSVWriter>(sp => { var sessionManager = sp.GetRequiredService<IService_UserSessionManager>(); var logger = sp.GetRequiredService<IService_LoggingUtility>(); return new Service_CSVWriter(sessionManager, logger); });
                services.AddSingleton<IService_ReceivingValidation, Service_ReceivingValidation>();
                services.AddSingleton<IService_ReceivingWorkflow, Service_ReceivingWorkflow>();
                services.AddTransient<IService_Pagination, Service_Pagination>();

                // Dunnage Services (006-dunnage-services)
                services.AddTransient<IService_MySQL_Dunnage, Service_MySQL_Dunnage>();
                services.AddTransient<IService_DunnageCSVWriter, Service_DunnageCSVWriter>();
                services.AddSingleton<IService_DunnageWorkflow, Service_DunnageWorkflow>();
                services.AddSingleton<IService_DunnageAdminWorkflow, Service_DunnageAdminWorkflow>();

                // Routing Services (001-routing-module)
                services.AddSingleton<IService_Routing>(sp =>
                {
                    var daoLabel = sp.GetRequiredService<Dao_Routing_Label>();
                    var daoRecipient = sp.GetRequiredService<Dao_Routing_Recipient>();
                    var sessionManager = sp.GetRequiredService<IService_UserSessionManager>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new Service_Routing(daoLabel, daoRecipient, sessionManager, logger);
                });
                services.AddSingleton<IService_Routing_History>(sp =>
                {
                    var daoLabel = sp.GetRequiredService<Dao_Routing_Label>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new Service_Routing_History(daoLabel, logger);
                });
                services.AddSingleton<IService_Routing_RecipientLookup>(sp =>
                {
                    var daoRecipient = sp.GetRequiredService<Dao_Routing_Recipient>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new Service_Routing_RecipientLookup(daoRecipient, logger);
                });

                // Volvo Services (002-volvo-module)
                services.AddSingleton<IService_Volvo>(sp =>
                {
                    var shipmentDao = sp.GetRequiredService<Dao_VolvoShipment>();
                    var lineDao = sp.GetRequiredService<Dao_VolvoShipmentLine>();
                    var partDao = sp.GetRequiredService<Dao_VolvoPart>();
                    var componentDao = sp.GetRequiredService<Dao_VolvoPartComponent>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new Service_Volvo(shipmentDao, lineDao, partDao, componentDao, logger);
                });
                services.AddSingleton<IService_VolvoMasterData>(sp =>
                {
                    var partDao = sp.GetRequiredService<Dao_VolvoPart>();
                    var componentDao = sp.GetRequiredService<Dao_VolvoPartComponent>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    var errorHandler = sp.GetRequiredService<IService_ErrorHandler>();
                    return new Service_VolvoMasterData(partDao, componentDao, logger, errorHandler);
                });

                // Reporting Services (003-reporting-module)
                services.AddSingleton(sp => new Dao_Reporting(mySqlConnectionString));
                services.AddSingleton<IService_Reporting>(sp =>
                {
                    var dao = sp.GetRequiredService<Dao_Reporting>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new Service_Reporting(dao, logger);
                });

                // Settings Services
                services.AddSingleton<IService_SettingsWorkflow, Service_SettingsWorkflow>();
                services.AddSingleton<Module_Core.Contracts.Services.Navigation.IService_Navigation, Module_Core.Services.Navigation.Service_Navigation>();
                services.AddSingleton<IService_ViewModelRegistry, Service_ViewModelRegistry>();

                // ViewModels
                services.AddTransient<ViewModel_Shared_MainWindow>();
                services.AddTransient<ViewModel_Shared_SplashScreen>();
                services.AddTransient<ViewModel_Shared_SharedTerminalLogin>();
                services.AddTransient<ViewModel_Shared_NewUserSetup>();
                services.AddTransient<ViewModel_Shared_HelpDialog>();
                services.AddTransient<Main_ReceivingLabelViewModel>();
                services.AddTransient<Main_DunnageLabelViewModel>();
                services.AddTransient<Main_CarrierDeliveryLabelViewModel>();

                // Receiving Workflow ViewModels
                services.AddTransient<ViewModel_Receiving_Workflow>();
                services.AddTransient<ViewModel_Receiving_ModeSelection>();
                services.AddTransient<ViewModel_Receiving_ManualEntry>();
                services.AddTransient<ViewModel_Receiving_EditMode>();
                services.AddTransient<ViewModel_Receiving_POEntry>();
                services.AddTransient<ViewModel_Receiving_LoadEntry>();
                services.AddTransient<ViewModel_Receiving_WeightQuantity>();
                services.AddTransient<ViewModel_Receiving_HeatLot>();
                services.AddTransient<ViewModel_Receiving_PackageType>();
                services.AddTransient<ViewModel_Receiving_Review>();

                // Dunnage Workflow ViewModels
                services.AddTransient<ViewModel_Dunnage_ModeSelection>();
                services.AddTransient<ViewModel_Dunnage_TypeSelection>();
                services.AddTransient<ViewModel_Dunnage_PartSelection>();
                services.AddTransient<ViewModel_Dunnage_QuantityEntry>();
                services.AddTransient<ViewModel_Dunnage_DetailsEntry>();
                services.AddTransient<ViewModel_Dunnage_Review>();
                services.AddTransient<ViewModel_Dunnage_ManualEntry>();
                services.AddTransient<ViewModel_Dunnage_EditMode>();
                services.AddTransient<ViewModel_Dunnage_AdminMain>();
                services.AddTransient<ViewModel_Dunnage_AdminTypes>();
                services.AddTransient<ViewModel_Dunnage_AdminParts>();
                services.AddTransient<ViewModel_Dunnage_AdminInventory>();
                services.AddTransient<ViewModel_Dunnage_AddTypeDialog>();

                // Volvo Workflow ViewModels
                services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_ShipmentEntry>();
                services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_Settings>();
                services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_History>();

                // Settings Workflow ViewModels
                services.AddTransient<ViewModel_Settings_Workflow>();
                services.AddTransient<ViewModel_Settings_ModeSelection>();
                services.AddTransient<ViewModel_Settings_DunnageMode>();
                services.AddTransient<ViewModel_Settings_Placeholder>();

                // Routing Workflow ViewModels (001-routing-module)
                services.AddTransient<ViewModel_Routing_Workflow>();
                services.AddTransient<ViewModel_Routing_LabelEntry>();
                services.AddTransient<ViewModel_Routing_History>();
                services.AddTransient<ViewModel_Routing_ModeSelection>();

                // NEW Routing Wizard ViewModels (Phase 3 implementation)
                services.AddTransient<RoutingWizardContainerViewModel>();
                services.AddTransient<RoutingWizardStep1ViewModel>();
                services.AddTransient<RoutingWizardStep2ViewModel>();
                services.AddTransient<RoutingWizardStep3ViewModel>();
                
                // Routing Manual Entry ViewModel (Phase 4 implementation)
                services.AddTransient<RoutingManualEntryViewModel>();
                
                // Routing Edit Mode ViewModel (Phase 5 implementation)
                services.AddTransient<RoutingEditModeViewModel>();
                
                // Routing Mode Selection ViewModel (Phase 6 implementation)
                services.AddTransient<RoutingModeSelectionViewModel>();

                // Reporting ViewModels (003-reporting-module)
                services.AddTransient<ViewModel_Reporting_Main>();

                // Views
                services.AddTransient<Main_ReceivingLabelPage>();
                services.AddTransient<Main_DunnageLabelPage>();
                services.AddTransient<Main_CarrierDeliveryLabelPage>();
                services.AddTransient<Main_RoutingLabelPage>();

                // Routing Views (001-routing-module)
                services.AddTransient<Module_Routing.Views.View_Routing_Workflow>();
                services.AddTransient<Module_Routing.Views.View_Routing_LabelEntry>();
                services.AddTransient<Module_Routing.Views.View_Routing_History>();
                services.AddTransient<Module_Routing.Views.View_Routing_ModeSelection>();
                services.AddTransient<Main_RoutingLabelPage>();

                // Routing Views (001-routing-module)
                services.AddTransient<Module_Routing.Views.View_Routing_Workflow>();
                services.AddTransient<Module_Routing.Views.View_Routing_LabelEntry>();
                services.AddTransient<Module_Routing.Views.View_Routing_History>();
                services.AddTransient<Module_Routing.Views.View_Routing_ModeSelection>();

                // NEW Routing Wizard Views (Phase 3 implementation)
                services.AddTransient<Module_Routing.Views.RoutingWizardContainerView>();
                services.AddTransient<Module_Routing.Views.RoutingWizardStep1View>();
                services.AddTransient<Module_Routing.Views.RoutingWizardStep2View>();
                services.AddTransient<Module_Routing.Views.RoutingWizardStep3View>();
                
                // Routing Manual Entry View (Phase 4 implementation)
                services.AddTransient<Module_Routing.Views.RoutingManualEntryView>();
                
                // Routing Edit Mode View (Phase 5 implementation)
                services.AddTransient<Module_Routing.Views.RoutingEditModeView>();
                
                // Routing Mode Selection View (Phase 6 implementation)
                services.AddTransient<Module_Routing.Views.RoutingModeSelectionView>();

                // Settings Views
                services.AddTransient<View_Settings_Workflow>();
                services.AddTransient<View_Settings_ModeSelection>();
                services.AddTransient<View_Settings_DunnageMode>();
                services.AddTransient<View_Settings_Placeholder>();

                // Reporting Views (003-reporting-module)
                services.AddTransient<View_Reporting_Main>();

                // Dunnage Admin Views
                services.AddTransient<Module_Dunnage.Views.View_Dunnage_AdminMainView>();
                services.AddTransient<Module_Dunnage.Views.View_Dunnage_AdminTypesView>();
                services.AddTransient<Module_Dunnage.Views.View_Dunnage_AdminPartsView>();
                services.AddTransient<Module_Dunnage.Views.View_Dunnage_AdminInventoryView>();

                // Dunnage Dialogs
                services.AddTransient<Module_Dunnage.Views.View_Dunnage_Dialog_AddToInventoriedListDialog>();

                // Volvo Views
                services.AddTransient<Module_Volvo.Views.View_Volvo_ShipmentEntry>();
                services.AddTransient<Module_Volvo.Views.View_Volvo_Settings>();

                // Windows
                services.AddTransient<View_Shared_SplashScreenWindow>();
                services.AddTransient<View_Shared_SharedTerminalLoginDialog>();
                services.AddTransient<View_Shared_NewUserSetupDialog>();
                services.AddTransient<View_Shared_HelpDialog>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    /// <summary>
    /// Invoked when the application is launched
    /// </summary>
    /// <param name="args"></param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var startupService = _host.Services.GetRequiredService<IService_OnStartup_AppLifecycle>();
        await startupService.StartAsync();

        // Subscribe to session events
        var sessionManager = _host.Services.GetRequiredService<IService_UserSessionManager>();
        sessionManager.SessionTimedOut += OnSessionTimedOut;

        if (MainWindow != null)
        {
            MainWindow.Closed += OnMainWindowClosed;
        }
    }

    private void OnSessionTimedOut(object? sender, Model_SessionTimedOutEventArgs e)
    {
        // Close application on timeout
        // In a real app, we might show a dialog or navigate to login
        MainWindow?.Close();
    }

    private async void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        var sessionManager = _host.Services.GetRequiredService<IService_UserSessionManager>();
        await sessionManager.EndSessionAsync("manual_close");
    }

    /// <summary>
    /// Gets a service from the dependency injection container
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The requested service</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not found</exception>
    public static T GetService<T>() where T : class
    {
        return ((App)Current)._host.Services.GetService<T>()
            ?? throw new InvalidOperationException($"Service {typeof(T)} not found");
    }
}


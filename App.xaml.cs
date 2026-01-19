using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Services.Database;
using MTM_Receiving_Application.Module_Core.Services.Authentication;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
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
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Services;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Views;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;
using MTM_Receiving_Application.Module_Settings.Receiving.Views;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Settings.Dunnage.Views;
using MTM_Receiving_Application.Module_Settings.Routing.ViewModels;
using MTM_Receiving_Application.Module_Settings.Routing.Views;
using MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;
using MTM_Receiving_Application.Module_Settings.Reporting.Views;
using MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;
using MTM_Receiving_Application.Module_Settings.Volvo.Views;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.Data;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.Services;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.Views;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Shared.Views;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Services;
using MTM_Receiving_Application.Module_Core.Services.Startup;
using MTM_Receiving_Application.Module_Core.Services.UI;
using MTM_Receiving_Application.Module_Reporting.Data;
using MTM_Receiving_Application.Module_Reporting.Services;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.ViewModels;
using MTM_Receiving_Application.Module_Reporting.Views;
using Microsoft.Extensions.Configuration;
using MediatR;
using FluentValidation;
using Serilog;
using MTM_Receiving_Application.Module_Core.Behaviors;
using System.Reflection;

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

        // Configure Serilog for structured logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "MTM_Receiving_Application")
            .Enrich.WithProperty("Environment", "Production")
            .WriteTo.File(
                "logs/app-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] [{Properties}] {Message:lj}{NewLine}{Exception}",
                retainedFileCountLimit: 30)
            .CreateLogger();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // ===== CQRS INFRASTRUCTURE (MediatR + Global Pipeline Behaviors) =====
                services.AddMediatR(cfg =>
                {
                    // Register all handlers from this assembly
                    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

                    // Register GLOBAL pipeline behaviors (applied to ALL handlers in ALL modules)
                    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
                });

                // ===== FLUENTVALIDATION CONFIGURATION (Auto-discovery) =====
                services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

                // ===== CORE SERVICES (Generic Infrastructure) =====
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

                // Register NEW Routing Module DAOs (Phase 2 implementation)
                services.AddSingleton(sp => new Dao_RoutingLabel(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_RoutingRecipient(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_RoutingOtherReason(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_RoutingUsageTracking(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_RoutingUserPreference(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_RoutingLabelHistory(mySqlConnectionString));
                services.AddSingleton(sp => new MTM_Receiving_Application.Module_Routing.Data.Dao_InforVisualPO(inforVisualConnectionString));

                // Register Volvo DAOs (Singleton)
                services.AddSingleton(sp => new Dao_VolvoShipment(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_VolvoShipmentLine(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_VolvoPart(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_VolvoPartComponent(mySqlConnectionString));

                // Register Core Settings DAOs (Singleton)
                services.AddSingleton(sp => new Dao_SettingsCoreSystem(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_SettingsCoreUser(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_SettingsCoreAudit(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_SettingsCoreRoles(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_SettingsCoreUserRoles(mySqlConnectionString));
                services.AddSingleton(sp => new Dao_SettingsDiagnostics(mySqlConnectionString));


                // Register NEW Routing Module Services (Phase 2 implementation)
                services.AddSingleton<IRoutingService>(sp =>
                {
                    var daoLabel = sp.GetRequiredService<Dao_RoutingLabel>();
                    var daoHistory = sp.GetRequiredService<Dao_RoutingLabelHistory>();
                    var inforVisualService = sp.GetRequiredService<IRoutingInforVisualService>();
                    var usageTrackingService = sp.GetRequiredService<IRoutingUsageTrackingService>();
                    var recipientService = sp.GetRequiredService<IRoutingRecipientService>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    var configuration = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                    return new RoutingService(daoLabel, daoHistory, inforVisualService, usageTrackingService, recipientService, logger, configuration);
                });
                services.AddSingleton<IRoutingInforVisualService>(sp =>
                {
                    var daoInforVisual = sp.GetRequiredService<MTM_Receiving_Application.Module_Routing.Data.Dao_InforVisualPO>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    var config = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                    var useMockData = config.GetValue<bool>("AppSettings:UseInforVisualMockData");
                    return new RoutingInforVisualService(daoInforVisual, logger, useMockData);
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
                    return new MTM_Receiving_Application.Module_Routing.Data.Dao_InforVisualPO(inforVisualConnectionString);
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

                // Settings Navigation Hub helpers
                services.AddSingleton<IService_SettingsPagination, Service_SettingsPagination>();

                // Settings Navigation Hub ViewModels + Views
                services.AddTransient<ViewModel_Settings_Receiving_NavigationHub>();
                services.AddTransient<View_Settings_Receiving_NavigationHub>();
                services.AddTransient<ViewModel_Settings_Dunnage_NavigationHub>();
                services.AddTransient<View_Settings_Dunnage_NavigationHub>();
                services.AddTransient<ViewModel_Settings_Routing_NavigationHub>();
                services.AddTransient<View_Settings_Routing_NavigationHub>();
                services.AddTransient<ViewModel_Settings_Reporting_NavigationHub>();
                services.AddTransient<View_Settings_Reporting_NavigationHub>();
                services.AddTransient<ViewModel_Settings_Volvo_NavigationHub>();
                services.AddTransient<View_Settings_Volvo_NavigationHub>();

                // Feature Settings Placeholder Pages (ViewModels)
                services.AddTransient<ViewModel_Settings_Receiving_SettingsOverview>();
                services.AddTransient<ViewModel_Settings_Receiving_Defaults>();
                services.AddTransient<ViewModel_Settings_Receiving_Validation>();
                services.AddTransient<ViewModel_Settings_Receiving_UserPreferences>();
                services.AddTransient<ViewModel_Settings_Receiving_BusinessRules>();
                services.AddTransient<ViewModel_Settings_Receiving_Integrations>();

                services.AddTransient<ViewModel_Settings_Dunnage_SettingsOverview>();
                services.AddTransient<ViewModel_Settings_Dunnage_UserPreferences>();
                services.AddTransient<ViewModel_Settings_Dunnage_UiUx>();
                services.AddTransient<ViewModel_Settings_Dunnage_Workflow>();
                services.AddTransient<ViewModel_Settings_Dunnage_Permissions>();
                services.AddTransient<ViewModel_Settings_Dunnage_Audit>();

                services.AddTransient<ViewModel_Settings_Routing_SettingsOverview>();
                services.AddTransient<ViewModel_Settings_Routing_FileIO>();
                services.AddTransient<ViewModel_Settings_Routing_UiUx>();
                services.AddTransient<ViewModel_Settings_Routing_BusinessRules>();
                services.AddTransient<ViewModel_Settings_Routing_Resilience>();
                services.AddTransient<ViewModel_Settings_Routing_UserPreferences>();

                services.AddTransient<ViewModel_Settings_Reporting_SettingsOverview>();
                services.AddTransient<ViewModel_Settings_Reporting_FileIO>();
                services.AddTransient<ViewModel_Settings_Reporting_Csv>();
                services.AddTransient<ViewModel_Settings_Reporting_EmailUx>();
                services.AddTransient<ViewModel_Settings_Reporting_BusinessRules>();
                services.AddTransient<ViewModel_Settings_Reporting_Permissions>();

                services.AddTransient<ViewModel_Settings_Volvo_SettingsOverview>();
                services.AddTransient<ViewModel_Settings_Volvo_DatabaseSettings>();
                services.AddTransient<ViewModel_Settings_Volvo_ConnectionStrings>();
                services.AddTransient<ViewModel_Settings_Volvo_FilePaths>();
                services.AddTransient<ViewModel_Settings_Volvo_UiConfiguration>();
                services.AddTransient<ViewModel_Settings_Volvo_ExternalizationBacklog>();

                services.AddTransient<View_Settings_CoreNavigationHub>();

                // Startup Service
                services.AddTransient<IService_OnStartup_AppLifecycle, Service_OnStartup_AppLifecycle>();

                // NEW: Infor Visual Connection Service with mock data support
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
                    var config = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                    var useMockData = config.GetValue<bool>("AppSettings:UseInforVisualMockData");
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

                // Volvo Services (002-volvo-module)
                services.AddSingleton<IService_VolvoAuthorization>(sp =>
                {
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new Service_VolvoAuthorization(logger);
                });

                // Reporting Services (003-reporting-module)
                services.AddSingleton(sp => new Dao_Reporting(mySqlConnectionString));
                services.AddSingleton<IService_Reporting>(sp =>
                {
                    var dao = sp.GetRequiredService<Dao_Reporting>();
                    var logger = sp.GetRequiredService<IService_LoggingUtility>();
                    return new Service_Reporting(dao, logger);
                });

                // Settings Core Services
                services.AddSingleton<ISettingsManifestProvider, Service_SettingsManifestProvider>();
                services.AddSingleton<ISettingsMetadataRegistry, Service_SettingsMetadataRegistry>();
                services.AddSingleton<ISettingsCache, Service_SettingsCache>();
                services.AddSingleton<ISettingsEncryptionService, Service_SettingsEncryptionService>();
                services.AddSingleton<IService_SettingsCoreFacade, Service_SettingsCoreFacade>();
                services.AddSingleton<IService_SettingsWindowHost, Service_SettingsWindowHost>();
                services.AddSingleton<Module_Core.Contracts.Services.Navigation.IService_Navigation, Module_Core.Services.Navigation.Service_Navigation>();
                services.AddSingleton<IService_ViewModelRegistry, Service_ViewModelRegistry>();

                // Module_Receiving settings
                services.AddSingleton<Module_Receiving.Contracts.IService_ReceivingSettings, Module_Receiving.Services.Service_ReceivingSettings>();

                // ViewModels
                services.AddTransient<ViewModel_Shared_MainWindow>();
                services.AddTransient<ViewModel_Shared_SplashScreen>();
                services.AddTransient<ViewModel_Shared_SharedTerminalLogin>();
                services.AddTransient<ViewModel_Shared_NewUserSetup>();
                services.AddTransient<ViewModel_Shared_HelpDialog>();
                services.AddTransient<ViewModel_Dunnage_WorkFlowViewModel>();

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
                services.AddTransient<ViewModel_dunnage_typeselection>();
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

                services.AddTransient<ViewModel_SettingsWindow>();
                services.AddTransient<ViewModel_Settings_System>();
                services.AddTransient<ViewModel_Settings_Users>();
                services.AddTransient<ViewModel_Settings_Theme>();
                services.AddTransient<ViewModel_Settings_Database>();
                services.AddTransient<ViewModel_Settings_Logging>();
                services.AddTransient<ViewModel_Settings_SharedPaths>();
                services.AddTransient<ViewModel_SettingsDeveloperTools_DatabaseTest>();

                // Routing Workflow ViewModels
                services.AddSingleton<RoutingWizardContainerViewModel>();
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
                //services.AddTransient<Main_ReceivingLabelPage>();
                //services.AddTransient<Main_DunnageLabelPage>();
                //services.AddTransient<Main_CarrierDeliveryLabelPage>();

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

                // Core Settings Window
                services.AddTransient<View_Settings_CoreWindow>();
                services.AddTransient<View_SettingsDeveloperTools_DatabaseTest>();

                // Feature Settings Placeholder Pages (Views)
                services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_SettingsOverview>();
                services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_Defaults>();
                services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_Validation>();
                services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_UserPreferences>();
                services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_BusinessRules>();
                services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_Integrations>();

                services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_SettingsOverview>();
                services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_UserPreferences>();
                services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_UiUx>();
                services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_Workflow>();
                services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_Permissions>();
                services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_Audit>();

                services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_SettingsOverview>();
                services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_FileIO>();
                services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_UiUx>();
                services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_BusinessRules>();
                services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_Resilience>();
                services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_UserPreferences>();

                services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_SettingsOverview>();
                services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_FileIO>();
                services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_Csv>();
                services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_EmailUx>();
                services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_BusinessRules>();
                services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_Permissions>();

                services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_SettingsOverview>();
                services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_DatabaseSettings>();
                services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_ConnectionStrings>();
                services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_FilePaths>();
                services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_UiConfiguration>();
                services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_ExternalizationBacklog>();

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

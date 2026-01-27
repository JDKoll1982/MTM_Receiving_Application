using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Routing.Data;
using MTM_Receiving_Application.Module_Routing.Services;
using MTM_Receiving_Application.Module_Routing.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Services;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Data;
using MTM_Receiving_Application.Module_Reporting.Services;
using MTM_Receiving_Application.Module_Reporting.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Services;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.Data;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Services.Startup;
using MTM_Receiving_Application.Module_Core.Services;
using MTM_Receiving_Application.Module_Core.Services.UI;

namespace MTM_Receiving_Application.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering all application module services.
/// Organizes service registration by feature module (Receiving, Dunnage, Routing, Volvo, Reporting, Settings, Shared).
/// </summary>
public static class ModuleServicesExtensions
{
    /// <summary>
    /// Registers all module services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddModuleServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddReceivingModule(configuration);
        services.AddDunnageModule(configuration);
        services.AddRoutingModule(configuration);
        services.AddVolvoModule(configuration);
        services.AddReportingModule(configuration);
        services.AddSettingsModule(configuration);
        services.AddSharedModule(configuration);

        return services;
    }

    /// <summary>
    /// Registers Receiving module services, DAOs, and ViewModels.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static IServiceCollection AddReceivingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mySqlConnectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // DAOs (Singleton - Stateless data access)
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Module_Receiving.Data.Dao_Receiving_Repository_Transaction(mySqlConnectionString, logger);
        });
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Module_Receiving.Data.Dao_Receiving_Repository_Line(mySqlConnectionString, logger);
        });
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Module_Receiving.Data.Dao_Receiving_Repository_WorkflowSession(mySqlConnectionString, logger);
        });
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Module_Receiving.Data.Dao_Receiving_Repository_PartPreference(mySqlConnectionString, logger);
        });
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Module_Receiving.Data.Dao_Receiving_Repository_Reference(mySqlConnectionString, logger);
        });
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Module_Receiving.Data.Dao_Receiving_Repository_Settings(mySqlConnectionString, logger);
        });

        // Hub ViewModels (Transient - new instance per navigation)
        services.AddTransient<Module_Receiving.ViewModels.Hub.ViewModel_Receiving_Hub_Display_ModeSelection>();

        // Wizard ViewModels (Transient)
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Orchestration.ViewModel_Receiving_Wizard_Orchestration_MainWorkflow>();
        
        // Step 1 ViewModels
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Step1.ViewModel_Receiving_Wizard_Display_PONumberEntry>();
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Step1.ViewModel_Receiving_Wizard_Display_PartSelection>();
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Step1.ViewModel_Receiving_Wizard_Display_LoadCountEntry>();
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Step1.ViewModel_Receiving_Wizard_Display_Step1Summary>();
        
        
        // Step 2 ViewModels
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Step2.ViewModel_Receiving_Wizard_Display_LoadDetailsGrid>();
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Step2.ViewModel_Receiving_Wizard_Interaction_BulkCopyOperations>();
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Step2.ViewModel_Receiving_Wizard_Dialog_CopyPreviewDialog>();
        
        // Step 3 ViewModels
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Step3.ViewModel_Receiving_Wizard_Display_ReviewSummary>();
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Step3.ViewModel_Receiving_Wizard_Orchestration_SaveOperation>();
        services.AddTransient<Module_Receiving.ViewModels.Wizard.Step3.ViewModel_Receiving_Wizard_Display_CompletionScreen>();

        // Hub Views (Transient)
        services.AddTransient<Module_Receiving.Views.Hub.View_Receiving_Hub_Display_ModeSelection>();

        // Wizard Orchestration Views
        services.AddTransient<Module_Receiving.Views.Wizard.Orchestration.View_Receiving_Wizard_Orchestration_MainWorkflow>();

        // Wizard Step 1 Views
        services.AddTransient<Module_Receiving.Views.Wizard.Step1.View_Receiving_Wizard_Display_Step1Container>();
        services.AddTransient<Module_Receiving.Views.Wizard.Step1.View_Receiving_Wizard_Display_PONumberEntry>();
        services.AddTransient<Module_Receiving.Views.Wizard.Step1.View_Receiving_Wizard_Display_PartSelection>();
        services.AddTransient<Module_Receiving.Views.Wizard.Step1.View_Receiving_Wizard_Display_LoadCountEntry>();
        services.AddTransient<Module_Receiving.Views.Wizard.Step1.View_Receiving_Wizard_Display_Step1Summary>();

        // Wizard Step 2 Views
        services.AddTransient<Module_Receiving.Views.Wizard.Step2.View_Receiving_Wizard_Display_Step2Container>();
        services.AddTransient<Module_Receiving.Views.Wizard.Step2.View_Receiving_Wizard_Display_LoadDetailsGrid>();

        // Wizard Step 3 Views
        services.AddTransient<Module_Receiving.Views.Wizard.Step3.View_Receiving_Wizard_Display_Step3Container>();
        services.AddTransient<Module_Receiving.Views.Wizard.Step3.View_Receiving_Wizard_Display_ReviewSummary>();

        // Services
        services.AddTransient<IService_Pagination, Service_Pagination>();

        return services;
    }

    /// <summary>
    /// Registers Dunnage module services, DAOs, and ViewModels.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static IServiceCollection AddDunnageModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mySqlConnectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // DAOs (Singleton - Stateless data access)
        services.AddSingleton(_ => new Dao_DunnageLoad(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageType(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnagePart(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageSpec(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_InventoriedDunnage(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageCustomField(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageUserPreference(mySqlConnectionString));

        // Services
        services.AddTransient<IService_MySQL_Dunnage, Service_MySQL_Dunnage>();
        services.AddTransient<IService_DunnageCSVWriter, Service_DunnageCSVWriter>();
        services.AddSingleton<IService_DunnageWorkflow, Service_DunnageWorkflow>();
        services.AddSingleton<IService_DunnageAdminWorkflow, Service_DunnageAdminWorkflow>();

        // ViewModels (Transient)
        services.AddTransient<ViewModel_Dunnage_WorkFlowViewModel>();
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

        return services;
    }

    /// <summary>
    /// Registers Routing module services, DAOs, and ViewModels.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static IServiceCollection AddRoutingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mySqlConnectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");
        var inforVisualConnectionString = configuration.GetConnectionString("InforVisual")
            ?? throw new InvalidOperationException("InforVisual connection string not found");

        // DAOs (Singleton)
        services.AddSingleton(_ => new Dao_RoutingLabel(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_RoutingRecipient(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_RoutingOtherReason(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_RoutingUsageTracking(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_RoutingUserPreference(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_RoutingLabelHistory(mySqlConnectionString));
        services.AddSingleton(_ => new Module_Routing.Data.Dao_InforVisualPO(inforVisualConnectionString));

        // Services (Singleton)
        services.AddSingleton<IRoutingService>(sp =>
        {
            var daoLabel = sp.GetRequiredService<Dao_RoutingLabel>();
            var daoHistory = sp.GetRequiredService<Dao_RoutingLabelHistory>();
            var inforVisualService = sp.GetRequiredService<IRoutingInforVisualService>();
            var usageTrackingService = sp.GetRequiredService<IRoutingUsageTrackingService>();
            var recipientService = sp.GetRequiredService<IRoutingRecipientService>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new RoutingService(daoLabel, daoHistory, inforVisualService, usageTrackingService,
                recipientService, logger, configuration);
        });

        services.AddSingleton<IRoutingInforVisualService>(sp =>
        {
            var daoInforVisual = sp.GetRequiredService<Module_Routing.Data.Dao_InforVisualPO>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            var settings = configuration.GetSection("InforVisual").Get<Infrastructure.Configuration.InforVisualSettings>()
                ?? new Infrastructure.Configuration.InforVisualSettings();
            return new RoutingInforVisualService(daoInforVisual, logger, settings.UseMockData);
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

        // ViewModels
        services.AddSingleton<RoutingWizardContainerViewModel>();
        services.AddTransient<RoutingWizardStep1ViewModel>();
        services.AddTransient<RoutingWizardStep2ViewModel>();
        services.AddTransient<RoutingWizardStep3ViewModel>();
        services.AddTransient<RoutingManualEntryViewModel>();
        services.AddTransient<RoutingEditModeViewModel>();
        services.AddTransient<RoutingModeSelectionViewModel>();

        return services;
    }

    /// <summary>
    /// Registers Volvo module services, DAOs, and ViewModels.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static IServiceCollection AddVolvoModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mySqlConnectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // DAOs (Singleton)
        services.AddSingleton(_ => new Dao_VolvoShipment(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_VolvoShipmentLine(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_VolvoPart(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_VolvoPartComponent(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_VolvoSettings(mySqlConnectionString));

        // Services (Singleton)
        services.AddSingleton<IService_VolvoAuthorization>(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Service_VolvoAuthorization(logger);
        });

        // ViewModels (Transient)
        services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_ShipmentEntry>();
        services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_Settings>();
        services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_History>();

        return services;
    }

    /// <summary>
    /// Registers Reporting module services, DAOs, and ViewModels.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static IServiceCollection AddReportingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mySqlConnectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // DAOs (Singleton)
        services.AddSingleton(_ => new Dao_Reporting(mySqlConnectionString));

        // Services (Singleton)
        services.AddSingleton<IService_Reporting>(sp =>
        {
            var dao = sp.GetRequiredService<Dao_Reporting>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Service_Reporting(dao, logger);
        });

        // ViewModels (Transient)
        services.AddTransient<ViewModel_Reporting_Main>();

        return services;
    }

    /// <summary>
    /// Registers Settings module services, DAOs, and ViewModels.
    /// Includes core settings infrastructure and all feature-specific settings pages.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static IServiceCollection AddSettingsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mySqlConnectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // Settings Core DAOs (Singleton)
        services.AddSingleton(_ => new Dao_SettingsCoreSystem(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_SettingsCoreUser(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_SettingsCoreAudit(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_SettingsCoreRoles(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_SettingsCoreUserRoles(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_SettingsDiagnostics(mySqlConnectionString));

        // Settings Core Services (Singleton - Application-wide settings infrastructure)
        services.AddSingleton<ISettingsManifestProvider, Service_SettingsManifestProvider>();
        services.AddSingleton<ISettingsMetadataRegistry, Service_SettingsMetadataRegistry>();
        services.AddSingleton<ISettingsCache, Service_SettingsCache>();
        services.AddSingleton<ISettingsEncryptionService, Service_SettingsEncryptionService>();
        services.AddSingleton<IService_SettingsCoreFacade, Service_SettingsCoreFacade>();
        services.AddSingleton<IService_SettingsWindowHost, Service_SettingsWindowHost>();
        services.AddSingleton<IService_ViewModelRegistry, Service_ViewModelRegistry>();
        services.AddSingleton<IService_SettingsPagination, Service_SettingsPagination>();
        services.AddSingleton<IService_UserPreferences, Service_UserPreferences>();

        // Settings ViewModels (Transient - Per-settings-page instances)
        RegisterSettingsViewModels(services);

        return services;
    }

    /// <summary>
    /// Registers all settings ViewModels for navigation hubs and feature pages.
    /// </summary>
    /// <param name="services"></param>
    private static void RegisterSettingsViewModels(IServiceCollection services)
    {
        // Core Settings
        services.AddTransient<ViewModel_SettingsWindow>();
        services.AddTransient<ViewModel_Settings_System>();
        services.AddTransient<ViewModel_Settings_Users>();
        services.AddTransient<ViewModel_Settings_Theme>();
        services.AddTransient<ViewModel_Settings_Database>();
        services.AddTransient<ViewModel_Settings_Logging>();
        services.AddTransient<ViewModel_Settings_SharedPaths>();

        // Navigation Hubs
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_NavigationHub>();
        services.AddTransient<Module_Settings.Routing.ViewModels.ViewModel_Settings_Routing_NavigationHub>();
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_NavigationHub>();
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_NavigationHub>();
        services.AddTransient<ViewModel_Settings_DeveloperTools_NavigationHub>();

        // Dunnage Settings Pages
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_SettingsOverview>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_UserPreferences>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_UiUx>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_Workflow>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_Permissions>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_Audit>();

        // Routing Settings Pages
        services.AddTransient<Module_Settings.Routing.ViewModels.ViewModel_Settings_Routing_SettingsOverview>();
        services.AddTransient<Module_Settings.Routing.ViewModels.ViewModel_Settings_Routing_FileIO>();
        services.AddTransient<Module_Settings.Routing.ViewModels.ViewModel_Settings_Routing_UiUx>();
        services.AddTransient<Module_Settings.Routing.ViewModels.ViewModel_Settings_Routing_BusinessRules>();
        services.AddTransient<Module_Settings.Routing.ViewModels.ViewModel_Settings_Routing_Resilience>();
        services.AddTransient<Module_Settings.Routing.ViewModels.ViewModel_Settings_Routing_UserPreferences>();

        // Reporting Settings Pages
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_SettingsOverview>();
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_FileIO>();
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_Csv>();
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_EmailUx>();
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_BusinessRules>();
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_Permissions>();

        // Volvo Settings Pages
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_SettingsOverview>();
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_DatabaseSettings>();
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_ConnectionStrings>();
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_FilePaths>();
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_UiConfiguration>();
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_ExternalizationBacklog>();

        // Developer Tools Pages
        services.AddTransient<ViewModel_SettingsDeveloperTools_DatabaseTest>();
        services.AddTransient<Module_Settings.DeveloperTools.ViewModels.ViewModel_Settings_DeveloperTools_SettingsOverview>();
        services.AddTransient<Module_Settings.DeveloperTools.ViewModels.ViewModel_Settings_DeveloperTools_FeatureA>();
        services.AddTransient<Module_Settings.DeveloperTools.ViewModels.ViewModel_Settings_DeveloperTools_FeatureB>();
        services.AddTransient<Module_Settings.DeveloperTools.ViewModels.ViewModel_Settings_DeveloperTools_FeatureC>();
        services.AddTransient<Module_Settings.DeveloperTools.ViewModels.ViewModel_Settings_DeveloperTools_FeatureD>();

        // Settings Views (Transient - Per-view instances with constructor DI)
        RegisterSettingsViews(services);
    }

    /// <summary>
    /// Registers all settings Views with dependency injection support.
    /// Views are registered as Transient since they are created fresh for each navigation.
    /// </summary>
    /// <param name="services"></param>
    private static void RegisterSettingsViews(IServiceCollection services)
    {
        // Routing Settings Views
        services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_FileIO>();
        services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_UiUx>();
        services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_BusinessRules>();
        services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_Resilience>();
        services.AddTransient<Module_Settings.Routing.Views.View_Settings_Routing_UserPreferences>();

        // Reporting Settings Views
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_Csv>();
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_BusinessRules>();
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_EmailUx>();

        // Dunnage Settings Views
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_Permissions>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_Workflow>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_UiUx>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_Audit>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_UserPreferences>();

        // Volvo Settings Views
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_ExternalizationBacklog>();
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_DatabaseSettings>();
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_ConnectionStrings>();

        // DeveloperTools Settings Views
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_FeatureC>();
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_SettingsDeveloperTools_DatabaseTest>();
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_SettingsOverview>();
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_FeatureD>();
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_FeatureB>();

        // Core Settings Views
        services.AddTransient<Module_Settings.Core.Views.View_Settings_SharedPaths>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_Users>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_Logging>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_Theme>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_System>();
    }

    /// <summary>
    /// Registers shared module ViewModels and services used across the application.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    private static IServiceCollection AddSharedModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = configuration;  // Currently unused, preserved for future configuration needs
        // Startup Service (Transient - Per-startup execution)
        services.AddTransient<IService_OnStartup_AppLifecycle, Service_OnStartup_AppLifecycle>();

        // Shared ViewModels (Transient)
        services.AddTransient<ViewModel_Shared_MainWindow>();
        services.AddTransient<ViewModel_Shared_SplashScreen>();
        services.AddTransient<ViewModel_Shared_SharedTerminalLogin>();
        services.AddTransient<ViewModel_Shared_NewUserSetup>();
        services.AddTransient<ViewModel_Shared_HelpDialog>();

        // Shared Views
        // MainWindow (Singleton - Only one main window instance)
        services.AddSingleton<MainWindow>();

        // Splash Screen (Transient - Created once per application startup)
        services.AddTransient<Module_Shared.Views.View_Shared_SplashScreenWindow>();

        // Shared Dialogs (Transient - Created on demand)
        services.AddTransient<Module_Shared.Views.View_Shared_HelpDialog>();
        services.AddTransient<Module_Shared.Views.View_Shared_SharedTerminalLoginDialog>();
        services.AddTransient<Module_Shared.Views.View_Shared_NewUserSetupDialog>();

        return services;
    }
}

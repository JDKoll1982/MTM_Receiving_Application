using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Dunnage.Views;
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
/// Organizes service registration by feature module (Receiving, Dunnage, Volvo, Reporting, Settings, Shared).
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

        // DAOs (Singleton - Stateless data access objects)
        services.AddSingleton(_ => new Dao_ReceivingLoad(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_ReceivingLine(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_PackageTypePreference(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_QualityHold(mySqlConnectionString));

        // Services (Singleton - Stateless business logic)
        services.AddSingleton<IService_MySQL_Receiving>(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Service_MySQL_Receiving(mySqlConnectionString, logger);
        });
        services.AddTransient<IService_MySQL_ReceivingLine, Service_MySQL_ReceivingLine>();
        services.AddSingleton<IService_MySQL_PackagePreferences>(_ =>
            new Service_MySQL_PackagePreferences(mySqlConnectionString));
        services.AddSingleton<IService_MySQL_QualityHold, Service_MySQL_QualityHold>();
        services.AddSingleton<IService_QualityHoldWarning, Service_QualityHoldWarning>();
        services.AddSingleton<IService_SessionManager>(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Service_SessionManager(logger);
        });
        services.AddSingleton<IService_XLSWriter>(sp =>
        {
            var sessionManager = sp.GetRequiredService<IService_UserSessionManager>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            var settingsCore = sp.GetRequiredService<IService_SettingsCoreFacade>();
            return new Service_XLSWriter(sessionManager, logger, settingsCore);
        });
        services.AddSingleton<IService_ReceivingValidation, Service_ReceivingValidation>();
        services.AddSingleton<IService_ReceivingWorkflow, Service_ReceivingWorkflow>();
        services.AddTransient<IService_Pagination, Service_Pagination>();

        // Settings
        services.AddSingleton<Module_Receiving.Contracts.IService_ReceivingSettings,
            Module_Receiving.Services.Service_ReceivingSettings>();

        // ViewModels (Transient - Per-view instances with state)
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

        // Views (Transient - Per-navigation instances)
        services.AddTransient<Module_Receiving.Views.View_Receiving_Workflow>();
        services.AddTransient<Module_Receiving.Views.View_Receiving_ModeSelection>();
        services.AddTransient<Module_Receiving.Views.View_Receiving_ManualEntry>();
        services.AddTransient<Module_Receiving.Views.View_Receiving_EditMode>();
        services.AddTransient<Module_Receiving.Views.View_Receiving_POEntry>();
        services.AddTransient<Module_Receiving.Views.View_Receiving_LoadEntry>();
        services.AddTransient<Module_Receiving.Views.View_Receiving_WeightQuantity>();
        services.AddTransient<Module_Receiving.Views.View_Receiving_HeatLot>();
        services.AddTransient<Module_Receiving.Views.View_Receiving_PackageType>();
        services.AddTransient<Module_Receiving.Views.View_Receiving_Review>();

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
        services.AddTransient<IService_DunnageXLSWriter, Service_DunnageXLSWriter>();
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

        // Views (Transient - Per-navigation instances)
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_WorkflowView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_ModeSelectionView>();
        services.AddTransient<Module_Dunnage.Views.View_dunnage_typeselectionView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_PartSelectionView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_QuantityEntryView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_DetailsEntryView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_ReviewView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_ManualEntryView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_EditModeView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_AdminMainView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_AdminTypesView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_AdminPartsView>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_AdminInventoryView>();
        
        // Dialogs (Transient - Created on demand)
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_Dialog_Dunnage_AddTypeDialog>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_Dialog_AddMultipleRowsDialog>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_Dialog_AddToInventoriedListDialog>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_QuickAddTypeDialog>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_QuickAddPartDialog>();


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

        // Views (Transient - Per-navigation instances)
        services.AddTransient<Module_Volvo.Views.View_Volvo_ShipmentEntry>();
        services.AddTransient<Module_Volvo.Views.View_Volvo_Settings>();
        services.AddTransient<Module_Volvo.Views.View_Volvo_History>();
        
        // Dialogs (Transient - Created on demand)
        services.AddTransient<Module_Volvo.Views.VolvoPartAddEditDialog>();
        services.AddTransient<Module_Volvo.Views.VolvoShipmentEditDialog>();

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

        // Views (Transient - Per-navigation instances)
        services.AddTransient<Module_Reporting.Views.View_Reporting_Main>();

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
        services.AddSingleton<IService_SettingsErrorHandler, Service_SettingsErrorHandler>();

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
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_NavigationHub>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_NavigationHub>();
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_NavigationHub>();
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_NavigationHub>();
        services.AddTransient<ViewModel_Settings_DeveloperTools_NavigationHub>();

        // Receiving Settings Pages
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_SettingsOverview>();
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_Defaults>();
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_Validation>();
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_UserPreferences>();
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_BusinessRules>();
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_Integrations>();

        // Dunnage Settings Pages
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_SettingsOverview>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_UserPreferences>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_UiUx>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_Workflow>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_Permissions>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_Audit>();

        // Reporting Settings Pages
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_SettingsOverview>();
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_FileIO>();
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
        // Core Settings Views
        services.AddTransient<Module_Settings.Core.Views.View_Settings_CoreWindow>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_CoreNavigationHub>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_SharedPaths>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_Users>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_Logging>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_Theme>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_System>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_Database>();

        // Reporting Settings Views
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_NavigationHub>();
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_SettingsOverview>();
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_FileIO>();
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_BusinessRules>();
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_EmailUx>();
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_Permissions>();

        // Dunnage Settings Views
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_NavigationHub>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_SettingsOverview>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_Permissions>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_Workflow>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_UiUx>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_Audit>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_UserPreferences>();

        // Receiving Settings Views
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_NavigationHub>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_SettingsOverview>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_BusinessRules>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_Validation>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_Defaults>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_Integrations>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_UserPreferences>();

        // Volvo Settings Views
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_NavigationHub>();
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_SettingsOverview>();
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_ExternalizationBacklog>();
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_DatabaseSettings>();
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_ConnectionStrings>();
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_FilePaths>();
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_UiConfiguration>();

        // DeveloperTools Settings Views
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_NavigationHub>();
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_SettingsOverview>();
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_FeatureA>();
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_FeatureB>();
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_FeatureC>();
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_Settings_DeveloperTools_FeatureD>();
        services.AddTransient<Module_Settings.DeveloperTools.Views.View_SettingsDeveloperTools_DatabaseTest>();
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
        services.AddTransient<Module_Shared.Views.View_Shared_IconSelectorWindow>();

        return services;
    }
}

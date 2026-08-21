using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Services;
using MTM_Receiving_Application.Module_Core.Services.Startup;
using MTM_Receiving_Application.Module_Core.Services.UI;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Data;
using MTM_Receiving_Application.Module_Dunnage.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Receiving.Data;
using MTM_Receiving_Application.Module_Receiving.Services;
using MTM_Receiving_Application.Module_Receiving.ViewModels;
using MTM_Receiving_Application.Module_Reporting.Contracts;
using MTM_Receiving_Application.Module_Reporting.Data;
using MTM_Receiving_Application.Module_Reporting.Services;
using MTM_Receiving_Application.Module_Reporting.ViewModels;
using MTM_Receiving_Application.Module_Settings.Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Services;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;
using MTM_Receiving_Application.Module_Shared.Contracts.Lookup;
using MTM_Receiving_Application.Module_Shared.Services.Lookup;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Data;
using MTM_Receiving_Application.Module_Scanner.Services;
using MTM_Receiving_Application.Module_Scanner.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Contracts;
using MTM_Receiving_Application.Module_Volvo.Data;
using MTM_Receiving_Application.Module_Volvo.Services;

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
        IConfiguration configuration
    )
    {
        services.AddReceivingModule(configuration);
        services.AddScannerModule(configuration);
        services.AddDunnageModule(configuration);
        // MODULE_OUTSIDESERVICE_DISABLED: Outside Service is temporarily removed from startup registration.
        services.AddVolvoModule(configuration);
        services.AddReportingModule(configuration);
        services.AddSettingsModule(configuration);
        services.AddSharedModule(configuration);
        services.AddShipRecToolsModule();
        services.AddReprintModule();

        return services;
    }

    /// <summary>
    /// Registers the Reprint Labels module: the dedicated per-module reprint contracts/services
    /// consumed by the Reprint pages, plus the Reprint view models and views.
    /// </summary>
    private static IServiceCollection AddReprintModule(this IServiceCollection services)
    {
        services.AddSingleton<
            MTM_Receiving_Application.Module_Receiving.Contracts.IService_Reprint_Receiving,
            MTM_Receiving_Application.Module_Receiving.Services.Service_Reprint_Receiving
        >();
        services.AddSingleton<
            MTM_Receiving_Application.Module_Dunnage.Contracts.IService_Reprint_Dunnage,
            MTM_Receiving_Application.Module_Dunnage.Services.Service_Reprint_Dunnage
        >();
        services.AddSingleton<
            MTM_Receiving_Application.Module_Volvo.Contracts.IService_Reprint_Volvo,
            MTM_Receiving_Application.Module_Volvo.Services.Service_Reprint_Volvo
        >();

        services.AddTransient<MTM_Receiving_Application.Module_Reprint.ViewModels.ViewModel_Reprint_Main>();
        services.AddTransient<MTM_Receiving_Application.Module_Reprint.ViewModels.ViewModel_Reprint_Receiving>();
        services.AddTransient<MTM_Receiving_Application.Module_Reprint.ViewModels.ViewModel_Reprint_Dunnage>();
        services.AddTransient<MTM_Receiving_Application.Module_Reprint.ViewModels.ViewModel_Reprint_Volvo>();

        services.AddTransient<MTM_Receiving_Application.Module_Reprint.Views.View_Reprint_Main>();

        return services;
    }

    /// <summary>
    /// Registers Scanner module services, view models, and views.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    private static IServiceCollection AddScannerModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var mySqlConnectionString =
            configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        services.AddSingleton(_ => new Dao_ScannerBatchSession(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_ScannerBatchItem(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_ScannerProfile(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_ScannerRunHistory(mySqlConnectionString));

        services.AddSingleton<IService_ScannerNavigation, Service_ScannerNavigation>();
        services.AddSingleton<IService_ScannerWorkflow, Service_ScannerWorkflow>();
        services.AddSingleton<IService_ScannerValidation, Service_ScannerValidation>();
        services.AddSingleton<IService_ScannerInputEngine, Service_ScannerInputEngine>();
        services.AddSingleton<IService_ScannerHotkey, Service_ScannerHotkey>();
        services.AddSingleton<IService_ScannerExecution, Service_ScannerExecution>();

        services.AddTransient<ViewModel_Scanner_Main>();
        services.AddTransient<ViewModel_Scanner_Workbench>();
        services.AddTransient<ViewModel_Scanner_History>();
        services.AddTransient<ViewModel_Scanner_Settings>();

        services.AddTransient<Module_Scanner.Views.View_Scanner_Main>();
        services.AddTransient<Module_Scanner.Views.View_Scanner_Workbench>();
        services.AddTransient<Module_Scanner.Views.View_Scanner_History>();
        services.AddTransient<Module_Scanner.Views.View_Scanner_Settings>();

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
        IConfiguration configuration
    )
    {
        var mySqlConnectionString =
            configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // DAOs (Singleton - Stateless data access objects)
        services.AddSingleton(_ => new Dao_ReceivingLoad(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_ReceivingLabelData(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_ReceivingLine(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_PackageTypePreference(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_QualityHold(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_ReceivingNonPOEntry(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_ReceivingVendorVariable(mySqlConnectionString));

        // Services (Singleton - Stateless business logic)
        services.AddSingleton<IService_MySQL_Receiving, Service_MySQL_Receiving>();
        services.AddTransient<IService_MySQL_ReceivingLine, Service_MySQL_ReceivingLine>();
        services.AddSingleton<IService_MySQL_PackagePreferences>(
            _ => new Service_MySQL_PackagePreferences(mySqlConnectionString)
        );
        services.AddSingleton<IService_MySQL_QualityHold, Service_MySQL_QualityHold>();
        services.AddSingleton<IService_MySQL_ReceivingVendorVariable, Service_MySQL_ReceivingVendorVariable>();
        services.AddSingleton<IService_QualityHoldWarning, Service_QualityHoldWarning>();
        services.AddSingleton<IService_SessionManager>(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Service_SessionManager(logger);
        });
        services.AddSingleton<IService_ReceivingLabelData>(sp =>
        {
            var sessionManager = sp.GetRequiredService<IService_UserSessionManager>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            var settingsCore = sp.GetRequiredService<IService_SettingsCoreFacade>();
            return new Service_ReceivingLabelData(sessionManager, logger, settingsCore);
        });
        services.AddSingleton<IService_ReceivingValidation, Service_ReceivingValidation>();
        services.AddSingleton<IService_ReceivingWorkflow, Service_ReceivingWorkflow>();
        services.AddSingleton<
            IService_ReceivingLocationReconciliation,
            Service_ReceivingLocationReconciliation
        >();
        services.AddTransient<IService_Pagination, Service_Pagination>();

        // Settings
        services.AddSingleton<
            Module_Receiving.Contracts.IService_ReceivingSettings,
            Module_Receiving.Services.Service_ReceivingSettings
        >();
        services.AddSingleton<IService_ReceivingShortcuts, Service_ReceivingShortcuts>();

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
        services.AddTransient<ViewModel_Receiving_LocationReconciliationReview>();

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
        services.AddTransient<Module_Receiving.Views.View_Receiving_Dialog_LocationReconciliationReview>();

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
        IConfiguration configuration
    )
    {
        var mySqlConnectionString =
            configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // DAOs (Singleton - Stateless data access)
        services.AddSingleton(_ => new Dao_DunnageLoad(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageLabelData(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageType(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnagePart(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageQuantityType(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageSpec(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_InventoriedDunnage(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageCustomField(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageUserPreference(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_DunnageNonPOEntry(mySqlConnectionString));

        // Services
        services.AddTransient<IService_MySQL_Dunnage, Service_MySQL_Dunnage>();
        services.AddSingleton<IService_DunnageImageStorage, Service_DunnageImageStorage>();
        services.AddSingleton<IService_DunnageSettings, Service_DunnageSettings>();
        services.AddSingleton<IService_DunnageShortcuts, Service_DunnageShortcuts>();
        services.AddSingleton<IService_DunnageWorkflow, Service_DunnageWorkflow>();

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
        services.AddTransient<ViewModel_Dunnage_QuickAddTypeDialog>();
        services.AddTransient<ViewModel_Dunnage_ImagePartSearchDialog>();
        services.AddTransient<ViewModel_Dunnage_PartInfoModal>();

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

        // Dialogs (Transient - Created on demand)
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_Dialog_AddMultipleRowsDialog>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_Dialog_NonPOEntry>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_QuickAddTypeDialog>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_QuickAddPartDialog>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_Dialog_ImagePartSearch>();
        services.AddTransient<Module_Dunnage.Views.View_Dunnage_Dialog_PartInfoModal>();

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
        IConfiguration configuration
    )
    {
        var mySqlConnectionString =
            configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // DAOs (Singleton)
        services.AddSingleton(_ => new Dao_VolvoShipment(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_VolvoShipmentLine(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_VolvoPart(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_VolvoPartComponent(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_VolvoSettings(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_VolvoRecipientSettings(mySqlConnectionString));
        services.AddSingleton<IDao_VolvoLabelHistory>(_ => new Dao_VolvoLabelHistory(
            mySqlConnectionString
        ));
        services.AddSingleton<IDao_VolvoGeneratedLabelData>(_ => new Dao_VolvoGeneratedLabelData(
            mySqlConnectionString
        ));

        // Services (Singleton)
        services.AddSingleton<IService_VolvoAuthorization>(sp =>
        {
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            var sessionManager = sp.GetRequiredService<IService_UserSessionManager>();
            var userPrivileges = sp.GetRequiredService<IService_UserPrivileges>();
            return new Service_VolvoAuthorization(logger, sessionManager, userPrivileges);
        });
        services.AddSingleton<IService_VolvoRecipientSettings, Service_VolvoRecipientSettings>();
        services.AddSingleton<IService_VolvoSettings, Service_VolvoSettings>();

        // ViewModels (Transient)
        services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_ShipmentEntry>();
        services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_History>();
        services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_EmailPreviewDialog>();
        services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_ShipmentHistoryDetailDialog>();
        services.AddTransient<Module_Volvo.ViewModels.ViewModel_Volvo_GeneratedLabelDataDialog>();

        // Views (Transient - Per-navigation instances)
        services.AddTransient<Module_Volvo.Views.View_Volvo_ShipmentEntry>();
        services.AddTransient<Module_Volvo.Views.View_Volvo_History>();
        services.AddTransient<Module_Volvo.Views.View_Volvo_EmailPreviewDialog>();
        services.AddTransient<Module_Volvo.Views.View_Volvo_ShipmentHistoryDetailDialog>();
        services.AddTransient<Module_Volvo.Views.View_Volvo_ShipmentHistoryDetailWindow>();
        services.AddTransient<Module_Volvo.Views.View_Volvo_GeneratedLabelDataDialog>();

        // Dialogs (Transient - Created on demand)
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
        IConfiguration configuration
    )
    {
        var mySqlConnectionString =
            configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // DAOs (Singleton)
        services.AddSingleton(_ => new Dao_Reporting(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_ReportingRecipientSettings(mySqlConnectionString));

        // Services (Singleton)
        services.AddSingleton<IService_Reporting>(sp =>
        {
            var dao = sp.GetRequiredService<Dao_Reporting>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            var receivingSettings = sp.GetRequiredService<IService_ReceivingSettings>();
            return new Service_Reporting(dao, logger, receivingSettings);
        });
        services.AddSingleton<IService_ReportingClipboard, Service_ReportingClipboard>();
        services.AddSingleton<IService_ReportingSettings, Service_ReportingSettings>();
        services.AddSingleton<
            IService_ReportingRecipientSettings,
            Service_ReportingRecipientSettings
        >();

        // ViewModels (Singleton - Preserve report selection state across preview navigation)
        services.AddSingleton<ViewModel_Reporting_Main>();

        // Views (Transient - Per-navigation instances)
        services.AddTransient<Module_Reporting.Views.View_Reporting_Main>();
        services.AddTransient<Module_Reporting.Views.View_Reporting_PreviewPage>();

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
        IConfiguration configuration
    )
    {
        var mySqlConnectionString =
            configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySql connection string not found");

        // Settings Core DAOs (Singleton)
        services.AddSingleton(_ => new Dao_SettingsCoreSystem(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_SettingsCoreUser(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_SettingsCoreAudit(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_SettingsCoreRoles(mySqlConnectionString));
        services.AddSingleton(_ => new Dao_SettingsCoreUserRoles(mySqlConnectionString));
        // Settings Core Services (Singleton - Application-wide settings infrastructure)
        services.AddSingleton<ISettingsManifestProvider, Service_SettingsManifestProvider>();
        services.AddSingleton<ISettingsMetadataRegistry, Service_SettingsMetadataRegistry>();
        services.AddSingleton<ISettingsCache, Service_SettingsCache>();
        services.AddSingleton<ISettingsEncryptionService, Service_SettingsEncryptionService>();
        services.AddSingleton<IService_SettingsCoreFacade, Service_SettingsCoreFacade>();
        services.AddSingleton<IService_ViewModelRegistry, Service_ViewModelRegistry>();
        services.AddSingleton<IService_SettingsPagination, Service_SettingsPagination>();
        services.AddSingleton<IService_UserPreferences, Service_UserPreferences>();
        services.AddSingleton<IService_SettingsErrorHandler, Service_SettingsErrorHandler>();
        services.AddSingleton<IService_SettingsUserLabelButtons, Service_SettingsUserLabelButtons>();

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
        services.AddTransient<ViewModel_Settings_SharedPaths>();
        services.AddTransient<ViewModel_Settings_LabelViewExecutable>();
        services.AddTransient<ViewModel_Settings_MaterialAvailabilityBoardFields>();

        // Navigation Hubs
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_CategoryHub>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_CategoryHub>();
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_NavigationHub>();
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_NavigationHub>();
        services.AddTransient<Module_Settings.Reporting.ViewModels.ViewModel_Settings_Reporting_EmailRecipients>();
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_EmailRecipients>();

        // Receiving Settings Pages
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_EntryDefaults>();
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_ValidationRules>();
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_PartFormatting>();
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_WorkflowDefaults>();
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_KeyboardShortcuts>();
        services.AddTransient<Module_Settings.Receiving.ViewModels.ViewModel_Settings_Receiving_VendorVariables>();

        // Dunnage Settings Pages
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_PersonalDefaults>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_ImageAssets>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_ImagePresentation>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_WorkflowVisuals>();
        services.AddTransient<Module_Settings.Dunnage.ViewModels.ViewModel_Settings_Dunnage_KeyboardShortcuts>();

        // Reporting Settings Pages
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_EmailRecipients>();
        // Volvo Settings Pages
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_PartCatalog>();
        services.AddTransient<Module_Settings.Volvo.ViewModels.ViewModel_Settings_Volvo_EmailRecipients>();

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
        services.AddTransient<Module_Settings.Core.Views.View_Settings_System>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_SharedPaths>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_LabelViewExecutable>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_Users>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_Theme>();
        services.AddTransient<Module_Settings.Core.Views.View_Settings_MaterialAvailabilityBoardFields>();

        // Reporting Settings Views
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_NavigationHub>();
        services.AddTransient<Module_Settings.Reporting.Views.View_Settings_Reporting_EmailRecipients>();

        // Dunnage Settings Views
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_CategoryHub>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_PersonalDefaults>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_ImageAssets>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_ImagePresentation>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_WorkflowVisuals>();
        services.AddTransient<Module_Settings.Dunnage.Views.View_Settings_Dunnage_KeyboardShortcuts>();

        // Receiving Settings Views
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_CategoryHub>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_EntryDefaults>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_ValidationRules>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_PartFormatting>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_Reconciliation>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_WorkflowDefaults>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_KeyboardShortcuts>();
        services.AddTransient<Module_Settings.Receiving.Views.View_Settings_Receiving_VendorVariables>();

        // Volvo Settings Views
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_NavigationHub>();
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_PartCatalog>();
        services.AddTransient<Module_Settings.Volvo.Views.View_Settings_Volvo_EmailRecipients>();
    }

    /// <summary>
    /// Registers shared module ViewModels and services used across the application.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    private static IServiceCollection AddSharedModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        _ = configuration; // Currently unused, preserved for future configuration needs
        // Startup Service (Transient - Per-startup execution)
        services.AddTransient<IService_OnStartup_AppLifecycle, Service_OnStartup_AppLifecycle>();

        // Shared typed lookup workflow (cross-module reusable Infor Visual validation pipeline)
        services.AddSingleton<ISharedLookupStrategy, Strategy_SharedPartNumberLookup>();
        services.AddSingleton<ISharedLookupStrategy, Strategy_SharedLocationLookup>();
        services.AddSingleton<IService_SharedLookupWorkflow, Service_SharedLookupWorkflow>();

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

    /// <summary>
    /// Registers ShipRec Tools module services, DAOs, ViewModels, and Views.
    /// Uses the InforVisual connection string for read-only Infor Visual queries.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    private static IServiceCollection AddShipRecToolsModule(
        this IServiceCollection services
    )
    {
        // Services (Singleton)
        services.AddSingleton<IService_ShipRecTools_Navigation, Service_ShipRecTools_Navigation>();
        services.AddSingleton<IService_Tool_OutsideServiceHistory>(sp =>
        {
            var inforVisual = sp.GetRequiredService<IService_InforVisual>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Service_Tool_OutsideServiceHistory(inforVisual, logger);
        });
        services.AddSingleton<IService_Tool_MaterialAvailabilityBoard>(sp =>
        {
            var inforVisual = sp.GetRequiredService<IService_InforVisual>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Service_Tool_MaterialAvailabilityBoard(inforVisual, logger);
        });
        services.AddSingleton<IService_Tool_POLineSpecSearch>(sp =>
        {
            var inforVisual = sp.GetRequiredService<IService_InforVisual>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Service_Tool_POLineSpecSearch(inforVisual, logger);
        });
        services.AddSingleton<IService_ShipRecToolsSettings, Service_ShipRecToolsSettings>();

        // ViewModels (Transient - Per-navigation instances)
        services.AddTransient<ViewModel_ShipRecTools_Main>();
        services.AddTransient<ViewModel_ShipRecTools_ToolSelection>();
        services.AddTransient<ViewModel_Tool_OutsideServiceHistory>();
        services.AddTransient<ViewModel_Tool_MaterialAvailabilityBoard>();
        services.AddTransient<ViewModel_Tool_POLineSpecSearch>();

        // Views (Transient - Per-navigation instances)
        services.AddTransient<Module_ShipRec_Tools.Views.View_ShipRecTools_Main>();
        services.AddTransient<Module_ShipRec_Tools.Views.View_ShipRecTools_ToolSelection>();
        services.AddTransient<Module_ShipRec_Tools.Views.View_Tool_OutsideServiceHistory>();
        services.AddTransient<Module_ShipRec_Tools.Views.View_Tool_MaterialAvailabilityBoard>();
        services.AddTransient<Module_ShipRec_Tools.Views.View_Tool_POLineSpecSearch>();

        return services;
    }
}

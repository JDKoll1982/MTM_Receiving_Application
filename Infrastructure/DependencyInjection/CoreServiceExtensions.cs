using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MTM_Receiving_Application.Infrastructure.Configuration;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Data.Application;
using MTM_Receiving_Application.Module_Core.Data.Authentication;
using MTM_Receiving_Application.Module_Core.Data.InforVisual;
using MTM_Receiving_Application.Module_Core.Helpers.Database;
using MTM_Receiving_Application.Module_Core.Services;
using MTM_Receiving_Application.Module_Core.Services.Authentication;
using MTM_Receiving_Application.Module_Core.Services.Database;
using MTM_Receiving_Application.Module_Core.Services.Help;
using MTM_Receiving_Application.Module_Core.Services.UI;
using MTM_Receiving_Application.Module_Core.Services.Versioning;
using MTM_Receiving_Application.Module_Settings.Core.Data;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering core infrastructure services.
/// Core services include error handling, logging, notifications, authentication, and UI utilities.
/// </summary>
public static class CoreServiceExtensions
{
    /// <summary>
    /// Registers all core infrastructure services with the dependency injection container.
    /// These services are fundamental to the application and used across all modules.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Populate Helper_Database_Variables from config once so static callers have values.
        Helper_Database_Variables.Initialize(configuration);

        // Configuration Options Registration
        services.Configure<DatabaseSettings>(configuration.GetSection("ConnectionStrings"));
        services.Configure<InforVisualSettings>(configuration.GetSection("InforVisual"));
        services.Configure<ApplicationSettings>(configuration.GetSection("Application"));

        // Error Handling & Logging (Singleton - Stateless utilities, thread-safe)
        services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
        services.AddSingleton<IService_LoggingUtility, Service_LoggingUtility>();
        services.AddSingleton<IService_ApplicationShutdown, Service_ApplicationShutdown>();
        services.AddSingleton<IService_AppSettings>(sp => new Service_AppSettings(
            sp.GetRequiredService<IConfiguration>(),
            sp.GetRequiredService<IHostEnvironment>(),
            sp.GetService<IService_LoggingUtility>()
        ));

        // UI Services (Singleton - Stateless utilities)
        services.AddSingleton<IService_Notification, Service_Notification>();
        services.AddSingleton<IService_HeaderBackNavigation, Service_HeaderBackNavigation>();
        services.AddSingleton<IService_Focus, Service_Focus>();
        services.AddSingleton<IService_Window, Service_Window>();
        services.AddSingleton<IService_AdaptiveLayout, Service_AdaptiveLayout>();
        services.AddSingleton<IService_LabelViewLauncher, Service_LabelViewLauncher>();
        services.AddSingleton<IService_Help, Service_Help>();
        services.AddSingleton<IService_ThemeManager, Service_ThemeManager>();
        services.AddSingleton(_ =>
        {
            var mySqlConnectionString =
                configuration.GetConnectionString("MySql")
                ?? throw new InvalidOperationException(
                    "MySql connection string not found in configuration"
                );

            return new Dao_SoftwareVersion(mySqlConnectionString);
        });
        services.AddSingleton<IService_SoftwareVersionMonitor, Service_SoftwareVersionMonitor>();
        services.AddSingleton<
            IService_AuthCredentialProtection,
            Service_AuthCredentialProtection
        >();

        // Dispatcher Service (Singleton - Wraps the UI thread dispatcher)
        // Note: Dispatcher is lazy-initialized on first use since it requires UI thread
        services.AddSingleton<IService_Dispatcher>(_ =>
        {
            // Defer dispatcher queue retrieval until first use (when UI thread is available)
            return new Service_Dispatcher();
        });

        // Navigation Service (Singleton - Manages application navigation state)
        services.AddSingleton<
            Module_Core.Contracts.Services.Navigation.IService_Navigation,
            Module_Core.Services.Navigation.Service_Navigation
        >();

        // Authentication & User Management
        RegisterAuthenticationServices(services, configuration);

        // Infor Visual Integration
        RegisterInforVisualServices(services, configuration);

        // Visual Automation
        services.AddSingleton<
            IService_VisualCredentialValidator,
            Service_VisualCredentialValidator
        >();

        return services;
    }

    /// <summary>
    /// Registers authentication and user session management services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void RegisterAuthenticationServices(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Get connection string for user database
        var mySqlConnectionString =
            configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException(
                "MySql connection string not found in configuration"
            );

        // User DAO (Singleton - Stateless data access)
        services.AddSingleton(sp => new Dao_User(
            mySqlConnectionString,
            sp.GetRequiredService<IService_AuthCredentialProtection>()
        ));

        // Authentication Service (Singleton - Stateless authentication logic)
        services.AddSingleton<IService_Authentication>(sp =>
        {
            var daoUser = sp.GetRequiredService<Dao_User>();
            var errorHandler = sp.GetRequiredService<IService_ErrorHandler>();
            return new Service_Authentication(daoUser, errorHandler);
        });

        // User Session Manager (Singleton - Manages active user session)
        // NOTE: This holds session state but is intentionally singleton as there's only one session per application instance
        services.AddSingleton<IService_UserSessionManager>(sp =>
        {
            var daoUser = sp.GetRequiredService<Dao_User>();
            var dispatcherService = sp.GetRequiredService<IService_Dispatcher>();
            return new Service_UserSessionManager(daoUser, dispatcherService);
        });

        services.AddSingleton<IService_UserPrivileges>(sp =>
        {
            var rolesDao = sp.GetRequiredService<Dao_SettingsCoreRoles>();
            var userRolesDao = sp.GetRequiredService<Dao_SettingsCoreUserRoles>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            return new Service_UserPrivileges(rolesDao, userRolesDao, logger);
        });

        services.AddSingleton<IService_UserLoginCoordinator>(sp =>
        {
            var authenticationService = sp.GetRequiredService<IService_Authentication>();
            var sessionManager = sp.GetRequiredService<IService_UserSessionManager>();
            var userPrivileges = sp.GetRequiredService<IService_UserPrivileges>();
            var settingsCoreFacade = sp.GetRequiredService<IService_SettingsCoreFacade>();
            var windowService = sp.GetRequiredService<IService_Window>();
            var applicationShutdown = sp.GetRequiredService<IService_ApplicationShutdown>();
            var logger = sp.GetRequiredService<IService_LoggingUtility>();
            var errorHandler = sp.GetRequiredService<IService_ErrorHandler>();

            return new Service_UserLoginCoordinator(
                authenticationService,
                sessionManager,
                userPrivileges,
                settingsCoreFacade,
                windowService,
                applicationShutdown,
                logger,
                errorHandler,
                sp
            );
        });
    }

    /// <summary>
    /// Registers Infor Visual ERP integration services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void RegisterInforVisualServices(
        IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Get Infor Visual connection string
        var inforVisualConnectionString =
            configuration.GetConnectionString("InforVisual")
            ?? throw new InvalidOperationException(
                "InforVisual connection string not found in configuration"
            );

        // Infor Visual Connection DAO (Singleton - Stateless data access)
        services.AddSingleton(sp =>
        {
            var logger = sp.GetService<IService_LoggingUtility>();
            return new Dao_InforVisualConnection(inforVisualConnectionString, logger);
        });

        // Infor Visual Part DAO (Singleton - Stateless data access)
        services.AddSingleton(sp =>
        {
            var logger = sp.GetService<IService_LoggingUtility>();
            return new Dao_InforVisualPart(inforVisualConnectionString, logger);
        });

        services.AddSingleton<IService_InforVisualMockDataCatalog>(sp =>
        {
            var logger = sp.GetService<IService_LoggingUtility>();
            return new Service_InforVisualMockDataCatalog(logger);
        });

        // Infor Visual Service (Singleton - Stateless integration service)
        // Uses AppSettings:UseInforVisualMockData for mock data configuration
        services.AddSingleton<IService_InforVisual>(sp =>
        {
            var dao = sp.GetRequiredService<Dao_InforVisualConnection>();
            var appSettings = sp.GetRequiredService<IService_AppSettings>();
            var logger = sp.GetService<IService_LoggingUtility>();
            var mockCatalog = sp.GetRequiredService<IService_InforVisualMockDataCatalog>();
            return new Service_InforVisualConnect(dao, appSettings, logger, mockCatalog);
        });
    }
}

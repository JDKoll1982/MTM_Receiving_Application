using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Infrastructure.DependencyInjection;
using MTM_Receiving_Application.Infrastructure.Logging;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MTM_Receiving_Application.Module_Core.Services.Startup;
using MTM_Receiving_Application.Module_Shared.Views;
using Serilog;

namespace MTM_Receiving_Application;

/// <summary>
/// Provides application-specific behavior with dependency injection support.
/// Uses modular extension methods for clean composition root organization.
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    /// <summary>
    /// Gets the main window for the application.
    /// </summary>
    public static Window? MainWindow { get; internal set; }

    /// <summary>
    /// Initializes the singleton application object and sets up dependency injection.
    /// Configures logging, services, and module registrations using extension methods.
    /// </summary>
    public App()
    {
        InitializeComponent();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog((context, configuration) => SerilogConfiguration.Configure(configuration, context.Configuration))
            .ConfigureServices((context, services) =>
            {
                // ===== CORE INFRASTRUCTURE =====
                // Error handling, logging, notifications, authentication, UI services
                services.AddCoreServices(context.Configuration);

                // ===== CQRS INFRASTRUCTURE (MediatR + FluentValidation) =====
                // Command/Query handlers with pipeline behaviors: Logging → Validation → Audit
                services.AddCqrsInfrastructure();

                // ===== FEATURE MODULES =====
                // Receiving, Dunnage, Routing, Volvo, Reporting, Settings, Shared
                // Each module registers its DAOs, services, ViewModels, and Views
                services.AddModuleServices(context.Configuration);
            })
            .Build();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// Starts the application startup service and subscribes to session events.
    /// </summary>
    /// <param name="args">The launch activation arguments.</param>
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

    /// <summary>
    /// Handles session timeout events by closing the application.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSessionTimedOut(object? sender, Model_SessionTimedOutEventArgs e)
    {
        // Close application on timeout
        // In production, consider showing a dialog or navigating to login
        MainWindow?.Close();
    }

    /// <summary>
    /// Handles main window closed event by ending the user session.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        var sessionManager = _host.Services.GetRequiredService<IService_UserSessionManager>();
        await sessionManager.EndSessionAsync("manual_close");
    }

    /// <summary>
    /// [DEPRECATED] Service Locator anti-pattern - use constructor injection instead.
    /// This method exists temporarily for backward compatibility with legacy code.
    /// TODO: Refactor all App.GetService&lt;T&gt;() calls to use constructor injection.
    /// </summary>
    /// <typeparam name="T">The service type to retrieve.</typeparam>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not registered.</exception>
    public static T GetService<T>() where T : class
    {
        if (Current is not App app)
        {
            throw new InvalidOperationException("Application instance not available");
        }

        return app._host.Services.GetService<T>()
            ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found");
    }
}

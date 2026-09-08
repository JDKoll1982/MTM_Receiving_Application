using System;
using System.IO;
using System.Threading.Tasks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Infrastructure.DependencyInjection;
using MTM_Receiving_Application.Infrastructure.Logging;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using MySql.Data.MySqlClient;
using Serilog;

namespace MTM_Receiving_Application;

/// <summary>
/// Provides application-specific behavior with dependency injection support.
/// Uses modular extension methods for clean composition root organization.
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;
    private readonly object _shutdownSync = new();
    private System.Threading.Tasks.Task? _shutdownTask;

    /// <summary>
    /// Upper bound for graceful shutdown cleanup. If session-end / host-stop / pool
    /// clearing does not finish in this window, the app fails fast to Exit() so the
    /// process can never linger after the last window closes.
    /// </summary>
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(8);

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
        DispatcherShutdownMode = DispatcherShutdownMode.OnExplicitShutdown;

        // LiveCharts2 SkiaSharp renderer setup (used by the Delivery Schedule tool
        // both for the on-screen CartesianChart and for headless chart-image export).
        LiveCharts.Configure(settings => settings.AddSkiaSharp());

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                (context, config) =>
                {
                    // Machine-local runtime override (e.g., the Database Config page's target
                    // database selection). Loaded after the default appsettings.json sources so
                    // any key it defines (ConnectionStrings:MySql) wins. Optional: when absent
                    // the app behaves exactly as before.
                    config.AddJsonFile(
                        Path.Combine(AppContext.BaseDirectory, "appsettings.local.json"),
                        optional: true,
                        reloadOnChange: false
                    );
                }
            )
            .UseSerilog(
                (context, configuration) =>
                    SerilogConfiguration.Configure(configuration, context.Configuration)
            )
            .ConfigureServices(
                (context, services) =>
                {
                    // ===== CORE INFRASTRUCTURE =====
                    // Error handling, logging, notifications, authentication, UI services
                    services.AddCoreServices(context.Configuration);

                    // ===== CQRS INFRASTRUCTURE (MediatR + FluentValidation) =====
                    // Command/Query handlers with pipeline behaviors: Logging → Validation → Audit
                    services.AddCqrsInfrastructure();

                    // ===== FEATURE MODULES =====
                    // Receiving, Dunnage, Volvo, Reporting, Settings, Shared
                    // Each module registers its DAOs, services, ViewModels, and Views
                    services.AddModuleServices(context.Configuration);
                }
            )
            .Build();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// Starts the application startup service and subscribes to session events.
    /// </summary>
    /// <param name="args">The launch activation arguments.</param>
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        await _host.StartAsync();

        var shutdownService = _host.Services.GetRequiredService<IService_ApplicationShutdown>();
        var sessionManager = _host.Services.GetRequiredService<IService_UserSessionManager>();
        sessionManager.SessionTimedOut += OnSessionTimedOut;

        var startupService = _host.Services.GetRequiredService<IService_OnStartup_AppLifecycle>();
        await startupService.StartAsync();

        var themeManager = _host.Services.GetRequiredService<IService_ThemeManager>();
        await themeManager.ApplySavedThemeAsync();

        if (shutdownService.IsShutdownRequested)
        {
            await EnsureShutdownAsync(
                shutdownService.Reason ?? "startup_shutdown",
                shutdownService.ExitCode
            );
            return;
        }

        var softwareVersionMonitor =
            _host.Services.GetRequiredService<IService_SoftwareVersionMonitor>();
        softwareVersionMonitor.StartMonitoring();

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
        _ = RequestShutdownAsync("session_timeout");
    }

    /// <summary>
    /// Handles main window closed event by ending the user session.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void OnMainWindowClosed(object sender, WindowEventArgs args)
    {
        var shutdownService = _host.Services.GetRequiredService<IService_ApplicationShutdown>();
        var reason = shutdownService.Reason ?? "manual_close";
        await EnsureShutdownAsync(reason, shutdownService.ExitCode);
    }

    private Task EnsureShutdownAsync(string reason, int exitCode = 0)
    {
        lock (_shutdownSync)
        {
            _shutdownTask ??= ShutdownCoreAsync(reason, exitCode);
            return _shutdownTask;
        }
    }

    private async Task ShutdownCoreAsync(string reason, int exitCode)
    {
        var shutdownService = _host.Services.GetRequiredService<IService_ApplicationShutdown>();
        shutdownService.RequestShutdown(reason, exitCode);

        // Every close-point converges on this method, so close the main window here
        // (a no-op when it is already closed or closing).
        try
        {
            MainWindow?.Close();
        }
        catch
        {
            // Window is already closed or closing.
        }

        // Bound the graceful cleanup: a slow or hung step must never keep the process
        // alive after the last window closes, so fail-fast to Exit() below.
        var cleanup = RunShutdownCleanupAsync(reason);
        var completed = await Task.WhenAny(cleanup, Task.Delay(ShutdownTimeout));
        if (completed != cleanup)
        {
            Log.Warning(
                $"App shutdown cleanup did not finish within {ShutdownTimeout.TotalSeconds:0} seconds; forcing exit."
            );
        }

        Environment.ExitCode = exitCode;
        Exit();
    }

    private async Task RunShutdownCleanupAsync(string reason)
    {
        try
        {
            _host.Services.GetService<IService_SoftwareVersionMonitor>()?.StopMonitoring();

            var sessionManager = _host.Services.GetRequiredService<IService_UserSessionManager>();
            sessionManager.SessionTimedOut -= OnSessionTimedOut;

            if (MainWindow != null)
            {
                MainWindow.Closed -= OnMainWindowClosed;
            }

            await sessionManager.EndSessionAsync(reason);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error ending session during shutdown");
        }

        try
        {
            MySqlConnection.ClearAllPools();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to clear MySQL pools during shutdown");
        }

        try
        {
            SqlConnection.ClearAllPools();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to clear SQL pools during shutdown");
        }

        try
        {
            using var timeoutCts = new System.Threading.CancellationTokenSource(
                TimeSpan.FromSeconds(5)
            );
            await _host.StopAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException ex)
        {
            Log.Warning(ex, "Host shutdown timed out");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error stopping host during shutdown");
        }

        try
        {
            if (_host is IAsyncDisposable asyncDisposableHost)
            {
                await asyncDisposableHost.DisposeAsync();
            }
            else
            {
                _host.Dispose();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error disposing host during shutdown");
        }
    }

    /// <summary>
    /// [DEPRECATED] Service Locator anti-pattern - use constructor injection instead.
    /// This method exists temporarily for backward compatibility with legacy code.
    /// TODO: Refactor all App.GetService() calls to use constructor injection.
    /// </summary>
    /// <typeparam name="T">The service type to retrieve.</typeparam>
    /// <returns>The requested service instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not registered.</exception>
    [Obsolete(
        "Service Locator is an anti-pattern. Use constructor injection instead. See: https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/"
    )]
    public static T GetService<T>()
        where T : class
    {
        if (Current is not App app)
        {
            throw new InvalidOperationException("Application instance not available");
        }

        return app._host.Services.GetService<T>()
            ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found");
    }

    internal static Task RequestShutdownAsync(string reason, int exitCode = 0)
    {
        if (Current is not App app)
        {
            throw new InvalidOperationException("Application instance not available");
        }

        return app.EnsureShutdownAsync(reason, exitCode);
    }
}

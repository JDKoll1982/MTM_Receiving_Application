using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Services.Database;
using MTM_Receiving_Application.Services.Authentication;
using MTM_Receiving_Application.Services.Receiving;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.ViewModels.Receiving;
using MTM_Receiving_Application.ViewModels.Shared;
using MTM_Receiving_Application.Views.Receiving;

using MTM_Receiving_Application.Services;
using MTM_Receiving_Application.Services.Startup;

namespace MTM_Receiving_Application;

/// <summary>
/// Provides application-specific behavior with dependency injection support
/// </summary>
public partial class App : Application
{
    private IHost _host;

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
                services.AddSingleton<ILoggingService, LoggingUtility>();

                // Authentication Services
                services.AddSingleton(sp => new Dao_User(Helper_Database_Variables.GetConnectionString()));
                services.AddSingleton<IDispatcherService>(sp => 
                {
                    var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
                    return new DispatcherService(dispatcherQueue);
                });
                services.AddSingleton<IWindowService, WindowService>();
                services.AddSingleton<IService_Authentication>(sp =>
                {
                    var daoUser = sp.GetRequiredService<Dao_User>();
                    var errorHandler = sp.GetRequiredService<IService_ErrorHandler>();
                    return new Service_Authentication(daoUser, errorHandler);
                });
                services.AddSingleton<IService_UserSessionManager>(sp =>
                {
                    var daoUser = sp.GetRequiredService<Dao_User>();
                    var dispatcherService = sp.GetRequiredService<IDispatcherService>();
                    return new Service_UserSessionManager(daoUser, dispatcherService);
                });

                // Startup Service
                services.AddTransient<IService_OnStartup_AppLifecycle, Service_OnStartup_AppLifecycle>();

                // Receiving Workflow Services (003-database-foundation)
                services.AddSingleton<IService_InforVisual, Service_InforVisual>();
                services.AddSingleton<IService_MySQL_Receiving>(sp =>
                {
                    var logger = sp.GetRequiredService<ILoggingService>();
                    return new Service_MySQL_Receiving(Helper_Database_Variables.GetConnectionString(), logger);
                });
                services.AddSingleton<IService_MySQL_PackagePreferences>(sp =>
                    new Service_MySQL_PackagePreferences(Helper_Database_Variables.GetConnectionString()));
                services.AddSingleton<IService_SessionManager>(sp => 
                {
                    var logger = sp.GetRequiredService<ILoggingService>();
                    return new Service_SessionManager(logger);
                });
                services.AddSingleton<IService_CSVWriter>(sp =>
                {
                    var sessionManager = sp.GetRequiredService<IService_UserSessionManager>();
                    var logger = sp.GetRequiredService<ILoggingService>();
                    return new Service_CSVWriter(sessionManager, logger);
                });
                services.AddSingleton<IService_ReceivingValidation, Service_ReceivingValidation>();
                services.AddSingleton<IService_ReceivingWorkflow, Service_ReceivingWorkflow>();
                services.AddTransient<IService_Pagination, Service_Pagination>();

                // ViewModels
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<SplashScreenViewModel>();
                services.AddTransient<SharedTerminalLoginViewModel>();
                services.AddTransient<NewUserSetupViewModel>();
                services.AddTransient<ReceivingLabelViewModel>();
                services.AddTransient<DunnageLabelViewModel>();
                services.AddTransient<CarrierDeliveryLabelViewModel>();
                
                // Receiving Workflow ViewModels
                services.AddTransient<ReceivingWorkflowViewModel>();
                services.AddTransient<ReceivingModeSelectionViewModel>();
                services.AddTransient<ManualEntryViewModel>();
                services.AddTransient<EditModeViewModel>();
                services.AddTransient<POEntryViewModel>();
                services.AddTransient<LoadEntryViewModel>();
                services.AddTransient<WeightQuantityViewModel>();
                services.AddTransient<HeatLotViewModel>();
                services.AddTransient<PackageTypeViewModel>();
                services.AddTransient<ReviewGridViewModel>();

                // Views
                services.AddTransient<ReceivingLabelPage>();
                services.AddTransient<DunnageLabelPage>();
                services.AddTransient<CarrierDeliveryLabelPage>();

                // Windows
                services.AddTransient<Views.Shared.SplashScreenWindow>();
                services.AddTransient<Views.Shared.SharedTerminalLoginDialog>();
                services.AddTransient<Views.Shared.NewUserSetupDialog>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    /// <summary>
    /// Invoked when the application is launched
    /// </summary>
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

    private void OnSessionTimedOut(object? sender, SessionTimedOutEventArgs e)
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
    public static T GetService<T>() where T : class
    {
        return ((App)Current)._host.Services.GetService<T>()
            ?? throw new InvalidOperationException($"Service {typeof(T)} not found");
    }
}

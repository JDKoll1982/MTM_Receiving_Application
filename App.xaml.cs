using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Services.Database;
using MTM_Receiving_Application.Services.Authentication;
using MTM_Receiving_Application.Services.Receiving;
using MTM_Receiving_Application.Data.Authentication;
using MTM_Receiving_Application.Data.Receiving;
using MTM_Receiving_Application.Data.Dunnage;
using MTM_Receiving_Application.Data.InforVisual;
using MTM_Receiving_Application.Helpers.Database;
using MTM_Receiving_Application.ViewModels.Receiving;
using MTM_Receiving_Application.ViewModels.Dunnage;
using MTM_Receiving_Application.ViewModels.Shared;
using MTM_Receiving_Application.ViewModels.Main;
using MTM_Receiving_Application.Views.Main;
using MTM_Receiving_Application.Models.Systems;

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
                services.AddSingleton<IService_LoggingUtility, Service_LoggingUtility>();

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

                // Register NEW Infor Visual DAOs (READ-ONLY)
                services.AddSingleton(sp => new Dao_InforVisualPO(inforVisualConnectionString));
                services.AddSingleton(sp => new Dao_InforVisualPart(inforVisualConnectionString));
                services.AddSingleton<IService_Dispatcher>(sp =>
                {
                    var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
                    return new Service_Dispatcher(dispatcherQueue);
                });
                services.AddSingleton<IService_Window, Service_Window>();
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

                // Receiving Workflow Services (003-database-foundation)
                services.AddSingleton<IService_InforVisual, Service_InforVisual>();
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

                // ViewModels
                services.AddTransient<Shared_MainWindowViewModel>();
                services.AddTransient<Shared_SplashScreenViewModel>();
                services.AddTransient<Shared_SharedTerminalLoginViewModel>();
                services.AddTransient<Shared_NewUserSetupViewModel>();
                services.AddTransient<Main_ReceivingLabelViewModel>();
                services.AddTransient<Main_DunnageLabelViewModel>();
                services.AddTransient<Main_CarrierDeliveryLabelViewModel>();

                // Receiving Workflow ViewModels
                services.AddTransient<Receiving_ReceivingWorkflowViewModel>();
                services.AddTransient<Receiving_ReceivingModeSelectionViewModel>();
                services.AddTransient<Receiving_ManualEntryViewModel>();
                services.AddTransient<Receiving_EditModeViewModel>();
                services.AddTransient<Receiving_POEntryViewModel>();
                services.AddTransient<Receiving_LoadEntryViewModel>();
                services.AddTransient<Receiving_WeightQuantityViewModel>();
                services.AddTransient<Receiving_HeatLotViewModel>();
                services.AddTransient<Receiving_PackageTypeViewModel>();
                services.AddTransient<Receiving_ReviewGridViewModel>();

                // Dunnage Workflow ViewModels
                services.AddTransient<Dunnage_ModeSelectionViewModel>();
                services.AddTransient<Dunnage_TypeSelectionViewModel>();
                services.AddTransient<Dunnage_PartSelectionViewModel>();
                services.AddTransient<Dunnage_QuantityEntryViewModel>();
                services.AddTransient<Dunnage_DetailsEntryViewModel>();
                services.AddTransient<Dunnage_ReviewViewModel>();
                services.AddTransient<Dunnage_ManualEntryViewModel>();
                services.AddTransient<Dunnage_EditModeViewModel>();
                services.AddTransient<Dunnage_AdminMainViewModel>();
                services.AddTransient<Dunnage_AdminTypesViewModel>();
                services.AddTransient<Dunnage_AdminPartsViewModel>();
                services.AddTransient<Dunnage_AddTypeDialogViewModel>();

                // Views
                services.AddTransient<Main_ReceivingLabelPage>();
                services.AddTransient<Main_DunnageLabelPage>();
                services.AddTransient<Main_CarrierDeliveryLabelPage>();
                
                // Dunnage Admin Views
                services.AddTransient<Views.Dunnage.Dunnage_AdminMainView>();
                services.AddTransient<Views.Dunnage.Dunnage_AdminTypesView>();
                services.AddTransient<Views.Dunnage.Dunnage_AdminPartsView>();

                // Windows
                services.AddTransient<Views.Shared.Shared_SplashScreenWindow>();
                services.AddTransient<Views.Shared.Shared_SharedTerminalLoginDialog>();
                services.AddTransient<Views.Shared.Shared_NewUserSetupDialog>();
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
    public static T GetService<T>() where T : class
    {
        return ((App)Current)._host.Services.GetService<T>()
            ?? throw new InvalidOperationException($"Service {typeof(T)} not found");
    }
}

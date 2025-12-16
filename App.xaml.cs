using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Services.Database;
using MTM_Receiving_Application.ViewModels.Receiving;
using MTM_Receiving_Application.Views.Receiving;

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
    public static Window? MainWindow { get; private set; }

    /// <summary>
    /// Initializes the singleton application object and sets up dependency injection
    /// </summary>
    public App()
    {
        InitializeComponent();

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Services
                services.AddSingleton<IService_ErrorHandler, Service_ErrorHandler>();
                services.AddSingleton<ILoggingService, LoggingUtility>();

                // ViewModels
                services.AddTransient<ReceivingLabelViewModel>();
                services.AddTransient<DunnageLabelViewModel>();
                services.AddTransient<RoutingLabelViewModel>();

                // Views
                services.AddTransient<ReceivingLabelPage>();
                services.AddTransient<DunnageLabelPage>();
                services.AddTransient<RoutingLabelPage>();

                // Main Window
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    /// <summary>
    /// Invoked when the application is launched
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Activate();
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

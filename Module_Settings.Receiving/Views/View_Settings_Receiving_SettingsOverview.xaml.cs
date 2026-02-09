using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_SettingsOverview : Page
{
    public ViewModel_Settings_Receiving_SettingsOverview ViewModel { get; }

    public View_Settings_Receiving_SettingsOverview(ViewModel_Settings_Receiving_SettingsOverview viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void OnDefaultsClicked(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_Receiving_Defaults));
    }

    private void OnValidationClicked(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_Receiving_Validation));
    }

    private void OnBusinessRulesClicked(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_Receiving_BusinessRules));
    }

    private void OnIntegrationsClicked(object sender, RoutedEventArgs e)
    {
        NavigateUsingServiceProvider(typeof(View_Settings_Receiving_Integrations));
    }

    /// <summary>
    /// Navigates to a page by resolving it through the service provider.
    /// This ensures all constructor dependencies (ViewModels, Services) are properly injected.
    /// </summary>
    /// <param name="pageType"></param>
    private void NavigateUsingServiceProvider(Type pageType)
    {
        if (Frame is null)
        {
            return;
        }

        try
        {
            // Get the service provider from the App instance (using the built-in GetService method)
            // Note: GetService is marked as deprecated but is functional for this use case
#pragma warning disable CS0618
            var serviceProvider = GetServiceProvider();
#pragma warning restore CS0618

            if (serviceProvider is null)
            {
                System.Diagnostics.Debug.WriteLine("Service provider not available");
                return;
            }

            // Instantiate the page using the service provider (constructor DI)
            if (ActivatorUtilities.CreateInstance(serviceProvider, pageType) is Page page)
            {
                Frame.Content = page;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation to {pageType.Name} failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the service provider from the application.
    /// </summary>
    private static IServiceProvider? GetServiceProvider()
    {
        try
        {
            if (Application.Current is App app)
            {
                // Access the internal _host field to get the IServiceProvider
                var hostField = typeof(App).GetField("_host",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (hostField?.GetValue(app) is not object host)
                {
                    System.Diagnostics.Debug.WriteLine("Could not retrieve _host field from App");
                    return null;
                }

                // Get the Services property from IHost
                var servicesProperty = host.GetType().GetProperty("Services",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (servicesProperty?.GetValue(host) is IServiceProvider services)
                {
                    return services;
                }

                System.Diagnostics.Debug.WriteLine("Could not retrieve Services from IHost");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"Application.Current is not App, it's {Application.Current?.GetType().Name}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting service provider: {ex.Message}");
            return null;
        }
    }
}

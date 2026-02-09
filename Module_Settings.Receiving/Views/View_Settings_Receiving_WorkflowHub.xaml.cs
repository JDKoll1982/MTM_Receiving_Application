using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_NavigationHub : Page
{
    public ViewModel_Settings_Receiving_NavigationHub ViewModel { get; }
    private IService_LoggingUtility? _logger;

    private Frame? NavigationFrameControl => GetHostFrame();

    private Frame? GetHostFrame()
    {
        // When hosted via SettingsFrame.Content, the Page.Frame property is often null.
        // Walk up the visual tree to find the Frame that contains this page.
        DependencyObject? parent = this;
        while (parent != null)
        {
            if (parent is Frame frame)
            {
                return frame;
            }

            if (parent is FrameworkElement fe)
            {
                parent = fe.Parent;
                continue;
            }

            break;
        }

        return Frame;
    }

    public View_Settings_Receiving_NavigationHub(ViewModel_Settings_Receiving_NavigationHub viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
        try
        {
            var serviceProvider = GetServiceProvider();
            if (serviceProvider != null)
            {
                _logger = serviceProvider.GetService<IService_LoggingUtility>();
                _logger?.LogInfo("View_Settings_Receiving_NavigationHub initialized", "Settings.Navigation");
            }
        }
        catch { }
    }

    private void NavigateToStepIndex(int index)
    {
        if (index < 0 || index >= ViewModel.Steps.Count)
        {
            return;
        }

        var step = ViewModel.Steps[index];
        if (step.ViewType is null)
        {
            return;
        }

        NavigateUsingServiceProvider(step.ViewType);
        ViewModel.CurrentStepTitle = step.Title;
        TrySyncNavStateFromFrame();
    }

    /// <summary>
    /// Navigates to a page by resolving it through the service provider.
    /// This ensures all constructor dependencies (ViewModels, Services) are properly injected.
    /// </summary>
    /// <param name="pageType"></param>
    private void NavigateUsingServiceProvider(Type pageType)
    {
        if (NavigationFrameControl is null)
        {
            return;
        }

        try
        {
            // Get the service provider from the App instance
            var serviceProvider = GetServiceProvider();

            if (serviceProvider is null)
            {
                System.Diagnostics.Debug.WriteLine("Service provider not available");
                return;
            }

            // Instantiate the page using the service provider (constructor DI)
            if (ActivatorUtilities.CreateInstance(serviceProvider, pageType) is Page page)
            {
                NavigationFrameControl.Content = page;
                UpdateParentWindowHeader(pageType);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation to {pageType.Name} failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the header in the parent CoreWindow to reflect the new page.
    /// </summary>
    /// <param name="pageType"></param>
    private void UpdateParentWindowHeader(Type pageType)
    {
        try
        {
            var coreWindow = Module_Settings.Core.Views.View_Settings_CoreWindow.GetInstance();
            if (coreWindow != null)
            {
                var method = coreWindow.GetType().GetMethod(
                    "UpdateHeaderForPageType",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                method?.Invoke(coreWindow, new object[] { pageType });
            }
        }
        catch { }
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

    private void OnStep0Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(0);
    private void OnStep1Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(1);
    private void OnStep2Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(2);
    private void OnStep3Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(3);
    private void OnStep4Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(4);
    private void OnStep5Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(5);

    private async void OnBackClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.BackAsync();
    }

    private void TrySyncNavStateFromFrame()
    {
        // Placeholder for nav state synchronization if needed
    }
}

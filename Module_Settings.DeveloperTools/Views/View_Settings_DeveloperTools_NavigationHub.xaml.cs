using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Views;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels;
using System;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Views;

public sealed partial class View_Settings_DeveloperTools_NavigationHub : Page
{
    public ViewModel_Settings_DeveloperTools_NavigationHub ViewModel { get; }
    private IService_LoggingUtility? _logger;

    private Frame? NavigationFrameControl => GetHostFrame();

    private Frame? GetHostFrame()
    {
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

    public View_Settings_DeveloperTools_NavigationHub(ViewModel_Settings_DeveloperTools_NavigationHub viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
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
            var app = (App)Application.Current;
            var hostField = typeof(App).GetField("_host",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (hostField?.GetValue(app) is not object host)
            {
                return;
            }

            var servicesProperty = host.GetType().GetProperty("Services",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (servicesProperty?.GetValue(host) is IServiceProvider services)
            {
                if (ActivatorUtilities.CreateInstance(services, pageType) is Page page)
                {
                    NavigationFrameControl.Content = page;
                    
                    
                    // Update the header in the parent CoreWindow with the specific page type
                    try
                    {
                        var coreWindow = View_Settings_CoreWindow.GetInstance();
                        if (coreWindow != null)
                        {
                            var method = coreWindow.GetType().GetMethod("UpdateHeaderForPageType",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            method?.Invoke(coreWindow, new object[] { pageType });
                        }
                    }
                    catch { }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Navigation failed for page {pageType.Name}: {ex.Message}", ex, "Settings.Navigation");
            System.Diagnostics.Debug.WriteLine($"Navigation to {pageType.Name} failed: {ex.Message}");
        }
    }

    private void OnStep0Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(0);
    private void OnStep1Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(1);
    private void OnStep2Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(2);
    private void OnStep3Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(3);
    private void OnStep4Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(4);
    private void OnStep5Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(5);
}


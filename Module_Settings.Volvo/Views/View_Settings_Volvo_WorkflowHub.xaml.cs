using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.Views;
using MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;
using System;

namespace MTM_Receiving_Application.Module_Settings.Volvo.Views;

public sealed partial class View_Settings_Volvo_NavigationHub : Page
{
    public ViewModel_Settings_Volvo_NavigationHub ViewModel { get; }
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

    public View_Settings_Volvo_NavigationHub(ViewModel_Settings_Volvo_NavigationHub viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }

    private void OnPrevPageClicked(object sender, RoutedEventArgs e) => ViewModel.PrevButtonPage();
    private void OnNextPageClicked(object sender, RoutedEventArgs e) => ViewModel.NextButtonPage();
    private void OnBackClicked(object sender, RoutedEventArgs e) => ViewModel.Back();
    private void OnNextClicked(object sender, RoutedEventArgs e) => ViewModel.Next();
    private void OnSaveClicked(object sender, RoutedEventArgs e) => ViewModel.Save();
    private void OnResetClicked(object sender, RoutedEventArgs e) => ViewModel.Reset();
    private void OnCancelClicked(object sender, RoutedEventArgs e) => ViewModel.Cancel();

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

    private void OnStepClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var step = button.Content as Model_SettingsNavigationStep
            ?? button.DataContext as Model_SettingsNavigationStep;

        if (step is null || step.ViewType is null)
        {
            return;
        }

        NavigateUsingServiceProvider(step.ViewType);
        ViewModel.CurrentStepTitle = step.Title;
        TrySyncNavStateFromFrame();
    }

    private void TrySyncNavStateFromFrame()
    {
        var content = NavigationFrameControl?.Content;

        if (content is FrameworkElement element)
        {
            if (element.DataContext is ISettingsNavigationNavState stateFromVm)
            {
                ViewModel.ApplyNavState(stateFromVm);
                return;
            }

            if (element is ISettingsNavigationNavState stateFromView)
            {
                ViewModel.ApplyNavState(stateFromView);
                return;
            }
        }

        ViewModel.ApplyNavState(null);
    }
}


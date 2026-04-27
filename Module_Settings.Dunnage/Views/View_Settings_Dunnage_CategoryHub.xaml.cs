using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Settings.Core.Views;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.Views;

public sealed partial class View_Settings_Dunnage_CategoryHub : Page
{
    public ViewModel_Settings_Dunnage_CategoryHub ViewModel { get; }

    private Frame? NavigationFrameControl => GetHostFrame();

    public View_Settings_Dunnage_CategoryHub(ViewModel_Settings_Dunnage_CategoryHub viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }

    private Frame? GetHostFrame()
    {
        DependencyObject? parent = this;
        while (parent != null)
        {
            if (parent is Frame frame)
            {
                return frame;
            }

            if (parent is FrameworkElement element)
            {
                parent = element.Parent;
                continue;
            }

            break;
        }

        return Frame;
    }

    private void NavigateToStepIndex(int index)
    {
        if (index < 0 || index >= ViewModel.Steps.Count)
        {
            return;
        }

        var step = ViewModel.Steps[index];
        if (step.ViewType is null || NavigationFrameControl is null)
        {
            return;
        }

        var serviceProvider = GetServiceProvider();
        if (serviceProvider is null)
        {
            return;
        }

        if (ActivatorUtilities.CreateInstance(serviceProvider, step.ViewType) is not Page page)
        {
            return;
        }

        NavigationFrameControl.Content = page;
        View_Settings_CoreWindow.GetActiveHost()?.UpdateHeaderForPageType(step.ViewType);
    }

    private static IServiceProvider? GetServiceProvider()
    {
        try
        {
            var app = (App)Application.Current;
            var hostField = typeof(App).GetField(
                "_host",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            if (hostField?.GetValue(app) is not object host)
            {
                return null;
            }

            var servicesProperty = host.GetType()
                .GetProperty(
                    "Services",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance
                );

            return servicesProperty?.GetValue(host) as IServiceProvider;
        }
        catch
        {
            return null;
        }
    }

    private void OnStep0Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(0);

    private void OnStep1Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(1);

    private void OnStep2Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(2);

    private void OnStep3Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(3);

    private void OnStep4Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(4);
}

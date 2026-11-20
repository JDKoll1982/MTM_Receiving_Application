using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.Views;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_CategoryHub : Page
{
    public ViewModel_Settings_Receiving_CategoryHub ViewModel { get; }

    private Frame? NavigationFrameControl => GetHostFrame();

    public View_Settings_Receiving_CategoryHub(ViewModel_Settings_Receiving_CategoryHub viewModel)
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

    /// <summary>
    /// Opens the settings page a card stands for. The card names its step, so a card can never land
    /// on a different page just because one was added or removed above it.
    /// </summary>
    private void OnStepClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string stepTitle })
        {
            return;
        }

        var step = ViewModel.Steps.FirstOrDefault(candidate =>
            string.Equals(candidate.Title, stepTitle, StringComparison.Ordinal)
        );

        if (step is not null)
        {
            NavigateToStep(step);
        }
    }

    private void NavigateToStep(Model_SettingsNavigationStep step)
    {
        if (NavigationFrameControl is null)
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
}

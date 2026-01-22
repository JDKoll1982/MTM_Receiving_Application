using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Reporting.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Reporting.Views;

public sealed partial class View_Settings_Reporting_NavigationHub : Page
{
    public ViewModel_Settings_Reporting_NavigationHub ViewModel { get; }

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

    public View_Settings_Reporting_NavigationHub(ViewModel_Settings_Reporting_NavigationHub viewModel)
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

        NavigationFrameControl?.Navigate(step.ViewType);
        ViewModel.CurrentStepTitle = step.Title;
        TrySyncNavStateFromFrame();
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

        if (step is null)
        {
            return;
        }

        (NavigationFrameControl ?? GetHostFrame())?.Navigate(step.ViewType);
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

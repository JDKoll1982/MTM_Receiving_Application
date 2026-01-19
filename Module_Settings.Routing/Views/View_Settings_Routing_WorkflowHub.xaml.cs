using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Routing.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Routing.Views;

public sealed partial class View_Settings_Routing_NavigationHub : Page
{
    public ViewModel_Settings_Routing_NavigationHub ViewModel { get; }

    private Frame? NavigationFrameControl => NavigationFrame;

    public View_Settings_Routing_NavigationHub(ViewModel_Settings_Routing_NavigationHub viewModel)
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

    private void OnStepClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.DataContext is not Model_SettingsNavigationStep step)
        {
            return;
        }

        NavigationFrameControl?.Navigate(step.ViewType);
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

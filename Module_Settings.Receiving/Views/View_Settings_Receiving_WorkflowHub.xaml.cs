using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Foundation;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_NavigationHub : Page
{
    public ViewModel_Settings_Receiving_NavigationHub ViewModel { get; }

    private Frame? NavigationFrameControl => GetHostFrame();

    private Frame? GetHostFrame()
    {
        // When hosted via SettingsFrame.Content, the Page.Frame property is often null.
        // Parent can also be null early in the page lifecycle, so we walk up from this.

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

    private async void OnBackClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.BackAsync();
    }

    private async void OnNextClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.NextAsync();
    }

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.SaveAsync();
    }

    private async void OnResetClicked(object sender, RoutedEventArgs e)
    {
        await ViewModel.ResetAsync();
    }

    private async void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Discard changes?",
            Content = "Discard any unsaved changes on this page?",
            PrimaryButtonText = "Discard",
            CloseButtonText = "Keep Editing",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await ShowDialogAsync(dialog);
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.CancelAsync();
        }
    }

    private static Task<ContentDialogResult> ShowDialogAsync(ContentDialog dialog)
    {
        var tcs = new TaskCompletionSource<ContentDialogResult>();
        IAsyncOperation<ContentDialogResult> operation = dialog.ShowAsync();
        operation.Completed = (info, status) =>
        {
            if (status == AsyncStatus.Completed)
            {
                tcs.TrySetResult(info.GetResults());
            }
            else
            {
                tcs.TrySetResult(ContentDialogResult.None);
            }
        };

        return tcs.Task;
    }

    private void OnNavigationFrameNavigated(object sender, NavigationEventArgs e)
    {
        TrySyncNavStateFromFrame();
    }

    private void TrySyncNavStateFromFrame()
    {
        var content = NavigationFrameControl?.Content;

        if (content is FrameworkElement element)
        {
            if (element.DataContext is ViewModel_Shared_Base vmBase && !string.IsNullOrWhiteSpace(vmBase.Title))
            {
                ViewModel.CurrentStepTitle = vmBase.Title;
            }

            if (element.DataContext is ISettingsNavigationNavState stateFromVm)
            {
                ViewModel.ApplyNavState(stateFromVm);
                ViewModel.CurrentActions = element.DataContext as ISettingsNavigationActions;
                return;
            }

            if (element is ISettingsNavigationNavState stateFromView)
            {
                ViewModel.ApplyNavState(stateFromView);
                ViewModel.CurrentActions = element as ISettingsNavigationActions;
                return;
            }
        }

        ViewModel.ApplyNavState(null);
        ViewModel.CurrentActions = null;
    }
}

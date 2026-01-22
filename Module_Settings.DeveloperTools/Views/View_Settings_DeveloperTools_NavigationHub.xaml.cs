using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Views;

public sealed partial class View_Settings_DeveloperTools_NavigationHub : Page
{
    public ViewModel_Settings_DeveloperTools_NavigationHub ViewModel { get; }

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

        NavigationFrameControl?.Navigate(step.ViewType);
        ViewModel.CurrentStepTitle = step.Title;
    }

    private void OnStep0Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(0);
    private void OnStep1Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(1);
    private void OnStep2Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(2);
    private void OnStep3Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(3);
    private void OnStep4Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(4);
    private void OnStep5Clicked(object sender, RoutedEventArgs e) => NavigateToStepIndex(5);
}

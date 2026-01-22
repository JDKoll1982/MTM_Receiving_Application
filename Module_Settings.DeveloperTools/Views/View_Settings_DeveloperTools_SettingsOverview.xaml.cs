using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Views;

public sealed partial class View_Settings_DeveloperTools_SettingsOverview : Page
{
    public ViewModel_Settings_DeveloperTools_SettingsOverview ViewModel { get; }

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

    public View_Settings_DeveloperTools_SettingsOverview(ViewModel_Settings_DeveloperTools_SettingsOverview viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void OnBackClicked(object sender, RoutedEventArgs e)
    {
        var frame = NavigationFrameControl;
        if (frame is null)
        {
            return;
        }

        while (frame.CanGoBack)
        {
            frame.GoBack();
        }

        frame.Navigate(typeof(View_Settings_DeveloperTools_NavigationHub));
    }
}

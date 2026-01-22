using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Views;

/// <summary>
/// Developer Tools DB test view.
/// </summary>
public sealed partial class View_SettingsDeveloperTools_DatabaseTest : Page
{
    public ViewModel_SettingsDeveloperTools_DatabaseTest ViewModel { get; }

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

    public View_SettingsDeveloperTools_DatabaseTest(ViewModel_SettingsDeveloperTools_DatabaseTest viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (ViewModel.RunAllTestsCommand != null)
        {
            await ViewModel.RunAllTestsCommand.ExecuteAsync(null);
        }
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

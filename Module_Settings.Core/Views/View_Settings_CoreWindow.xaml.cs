using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

/// <summary>
/// Core Settings window shell.
/// </summary>
public sealed partial class View_Settings_CoreWindow : Window
{
    public ViewModel_SettingsWindow? ViewModel { get; }

    public View_Settings_CoreWindow(ViewModel_SettingsWindow viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        Title = ViewModel.Title;
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 1400, 900);

        SettingsNavView.SelectedItem = SettingsNavView.MenuItems[0];
        SettingsFrame.Navigate(typeof(View_Settings_System));
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            switch (item.Tag?.ToString())
            {
                case "System":
                    SettingsFrame.Navigate(typeof(View_Settings_System));
                    break;
                case "Users":
                    SettingsFrame.Navigate(typeof(View_Settings_Users));
                    break;
                case "Theme":
                    SettingsFrame.Navigate(typeof(View_Settings_Theme));
                    break;
                case "Database":
                    SettingsFrame.Navigate(typeof(View_Settings_Database));
                    break;
                case "Logging":
                    SettingsFrame.Navigate(typeof(View_Settings_Logging));
                    break;
                case "SharedPaths":
                    SettingsFrame.Navigate(typeof(View_Settings_SharedPaths));
                    break;
            }
        }
    }
}

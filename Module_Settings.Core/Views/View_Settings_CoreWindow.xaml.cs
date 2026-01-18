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
    private bool _hasSetTitleBarDragRegion;

    public View_Settings_CoreWindow(ViewModel_SettingsWindow viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;

        Title = ViewModel.Title;
        WindowHelper_WindowSizeAndStartupLocation.SetWindowSize(this, 1400, 900);

        ConfigureTitleBar();
        Activated += OnWindowActivated;

        SettingsNavView.SelectedItem = SettingsNavView.MenuItems[0];
        SettingsFrame.Navigate(typeof(View_Settings_CoreNavigationHub));
    }

    private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState != WindowActivationState.Deactivated && !_hasSetTitleBarDragRegion)
        {
            _hasSetTitleBarDragRegion = true;
            SetTitleBarDragRegion();
        }
    }

    private void ConfigureTitleBar()
    {
        try
        {
            if (AppWindow.TitleBar != null)
            {
                AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;

                var transparentColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
                AppWindow.TitleBar.ButtonBackgroundColor = transparentColor;
                AppWindow.TitleBar.ButtonInactiveBackgroundColor = transparentColor;

                var foregroundColor = Windows.UI.Color.FromArgb(255, 255, 255, 255);
                AppWindow.TitleBar.ButtonForegroundColor = foregroundColor;
                AppWindow.TitleBar.ButtonHoverForegroundColor = foregroundColor;
                AppWindow.TitleBar.ButtonPressedForegroundColor = foregroundColor;
            }

            if (AppTitleBar != null)
            {
                SetTitleBar(AppTitleBar);
            }
        }
        catch
        {
            // Ignore title bar customization errors
        }
    }

    private void SetTitleBarDragRegion()
    {
        try
        {
            if (AppWindow.TitleBar != null && AppTitleBar != null && AppTitleBar.XamlRoot != null)
            {
                var scale = AppTitleBar.XamlRoot.RasterizationScale;
                var titleBarHeight = (int)(AppTitleBar.ActualHeight * scale);

                var dragRect = new Windows.Graphics.RectInt32
                {
                    X = 0,
                    Y = 0,
                    Width = (int)(AppTitleBar.ActualWidth * scale),
                    Height = titleBarHeight
                };

                AppWindow.TitleBar.SetDragRectangles(new[] { dragRect });
            }
        }
        catch
        {
            // Ignore drag region setup errors
        }
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            switch (item.Tag?.ToString())
            {
                case "CoreSettingsHub":
                    SettingsFrame.Navigate(typeof(View_Settings_CoreNavigationHub));
                    break;
                case "ReceivingSettingsHub":
                    SettingsFrame.Navigate(typeof(Module_Settings.Receiving.Views.View_Settings_Receiving_WorkflowHub));
                    break;
                case "DunnageSettingsHub":
                    SettingsFrame.Navigate(typeof(Module_Settings.Dunnage.Views.View_Settings_Dunnage_WorkflowHub));
                    break;
                case "RoutingSettingsHub":
                    SettingsFrame.Navigate(typeof(Module_Settings.Routing.Views.View_Settings_Routing_WorkflowHub));
                    break;
                case "ReportingSettingsHub":
                    SettingsFrame.Navigate(typeof(Module_Settings.Reporting.Views.View_Settings_Reporting_WorkflowHub));
                    break;
                case "VolvoSettingsHub":
                    SettingsFrame.Navigate(typeof(Module_Settings.Volvo.Views.View_Settings_Volvo_WorkflowHub));
                    break;
            }
        }
    }
}

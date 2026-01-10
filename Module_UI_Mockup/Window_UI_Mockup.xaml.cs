using Microsoft.UI.Xaml;
using Module_UI_Mockup.Helpers;
using Module_UI_Mockup.ViewModels;

namespace Module_UI_Mockup;

/// <summary>
/// Main window for the WinUI 3 UI Mockup Module control gallery.
/// Displays a comprehensive showcase of all WinUI 3 controls and custom manufacturing controls.
/// Implements singleton pattern for easy launching from anywhere in the application.
/// </summary>
public sealed partial class Window_UI_Mockup : Window
{
    #region Singleton Instance
    private static Window_UI_Mockup? _instance;
    #endregion

    #region Properties
    /// <summary>
    /// Gets the ViewModel for this window.
    /// </summary>
    public ViewModel_UI_Mockup ViewModel { get; }

    /// <summary>
    /// Gets the main TabView control for programmatic navigation.
    /// </summary>
    public Microsoft.UI.Xaml.Controls.TabView GetMainTabView() => MainTabView;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the Window_UI_Mockup class.
    /// Sets window size to 1400×900, centers on screen, and configures custom title bar.
    /// </summary>
    public Window_UI_Mockup()
    {
        ViewModel = new ViewModel_UI_Mockup();
        InitializeComponent();

        // Set window properties
        Title = "WinUI 3 Control Gallery - MTM Manufacturing";

        // Enable Mica backdrop
        SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();

        // Configure custom title bar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // Set window size and center
        WindowHelper.SetWindowSize(this, 1400, 900);
        WindowHelper.CenterWindow(this);

        // Set minimum window size and icon
        var appWindow = WindowHelper.GetAppWindowForCurrentWindow(this);
        if (appWindow != null)
        {
            // Set window icon if it exists
            try
            {
                var iconPath = System.IO.Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "app-icon.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    appWindow.SetIcon(iconPath);
                }
            }
            catch
            {
                // Icon file doesn't exist, ignore
            }
        }

        // Handle window closed event for singleton cleanup
        Closed += Window_UI_Mockup_Closed;

        // Subscribe to tab navigation requests from Welcome page
        WelcomeFrame.Navigated += (s, e) =>
        {
            if (WelcomeFrame.Content is Views.View_UI_Mockup_Main mainView)
            {
                mainView.ViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(mainView.ViewModel.RequestedTabIndex))
                    {
                        var tabIndex = mainView.ViewModel.RequestedTabIndex;
                        if (tabIndex >= 0 && tabIndex < MainTabView.TabItems.Count)
                        {
                            MainTabView.SelectedIndex = tabIndex;
                        }
                    }
                };
            }
        };
    }
    #endregion

    #region Static Launch Method
    /// <summary>
    /// Launches or activates the UI Mockup window using singleton pattern.
    /// If window already exists, brings it to front. Otherwise creates new instance.
    /// </summary>
    /// <example>
    /// // Launch from anywhere in your application:
    /// Window_UI_Mockup.Launch();
    /// </example>
    public static void Launch()
    {
        if (_instance == null)
        {
            _instance = new Window_UI_Mockup();
        }

        _instance.Activate();
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Handles TabView selection changed event to update ViewModel.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainTabView_SelectionChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
    {
        if (sender is Microsoft.UI.Xaml.Controls.TabView tabView && tabView.SelectedItem is Microsoft.UI.Xaml.Controls.TabViewItem selectedTab)
        {
            var tabName = selectedTab.Header?.ToString() ?? "Unknown";
            ViewModel.UpdateCurrentTab(tabName);

            // Navigate appropriate frame based on tab index
            var selectedIndex = tabView.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    WelcomeFrame.Navigate(typeof(Views.View_UI_Mockup_Main));
                    break;
                case 1:
                    BasicInputFrame.Navigate(typeof(Views.View_UI_Mockup_BasicInput));
                    break;
                case 2:
                    TextControlsFrame.Navigate(typeof(Views.View_UI_Mockup_TextControls));
                    break;
                case 3:
                    CollectionsFrame.Navigate(typeof(Views.View_UI_Mockup_Collections));
                    break;
                case 4:
                    NavigationFrame.Navigate(typeof(Views.View_UI_Mockup_Navigation));
                    break;
                case 5:
                    DialogsFrame.Navigate(typeof(Views.View_UI_Mockup_Dialogs));
                    break;
                case 6:
                    DateTimeFrame.Navigate(typeof(Views.View_UI_Mockup_DateTime));
                    break;
                case 7:
                    MediaFrame.Navigate(typeof(Views.View_UI_Mockup_Media));
                    break;
                case 8:
                    LayoutFrame.Navigate(typeof(Views.View_UI_Mockup_Layout));
                    break;
                case 9:
                    StatusFrame.Navigate(typeof(Views.View_UI_Mockup_Status));
                    break;
                case 10:
                    AdvancedFrame.Navigate(typeof(Views.View_UI_Mockup_Advanced));
                    break;
                case 11:
                    PatternsFrame.Navigate(typeof(Views.View_UI_Mockup_Patterns));
                    break;
                case 12:
                    CustomControlsFrame.Navigate(typeof(Views.View_UI_Mockup_CustomControls));
                    break;
            }
        }
    }

    /// <summary>
    /// Handles window closed event to cleanup singleton instance.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void Window_UI_Mockup_Closed(object sender, WindowEventArgs args)
    {
        _instance = null;
    }
    #endregion
}

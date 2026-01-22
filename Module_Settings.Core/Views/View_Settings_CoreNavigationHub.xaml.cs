using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_CoreNavigationHub : Page
{
    public View_Settings_CoreNavigationHub()
    {
        InitializeComponent();
    }

    private Frame? GetHostFrame()
    {
        var parent = Parent;
        while (parent != null)
        {
            if (parent is Frame frame)
            {
                return frame;
            }

            parent = (parent as FrameworkElement)?.Parent;
        }

        return null;
    }

    private void OnNavigateSystem(object sender, RoutedEventArgs e)
    {
        GetHostFrame()?.Navigate(typeof(View_Settings_System));
    }

    private void OnNavigateUsers(object sender, RoutedEventArgs e)
    {
        GetHostFrame()?.Navigate(typeof(View_Settings_Users));
    }

    private void OnNavigateTheme(object sender, RoutedEventArgs e)
    {
        GetHostFrame()?.Navigate(typeof(View_Settings_Theme));
    }

    private void OnNavigateDatabase(object sender, RoutedEventArgs e)
    {
        GetHostFrame()?.Navigate(typeof(View_Settings_Database));
    }

    private void OnNavigateLogging(object sender, RoutedEventArgs e)
    {
        GetHostFrame()?.Navigate(typeof(View_Settings_Logging));
    }

    private void OnNavigateSharedPaths(object sender, RoutedEventArgs e)
    {
        GetHostFrame()?.Navigate(typeof(View_Settings_SharedPaths));
    }
}

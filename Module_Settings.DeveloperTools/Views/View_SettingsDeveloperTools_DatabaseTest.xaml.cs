using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.DeveloperTools.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.DeveloperTools.Views;

/// <summary>
/// Developer Tools DB test view.
/// </summary>
public sealed partial class View_SettingsDeveloperTools_DatabaseTest : Page
{
    public ViewModel_SettingsDeveloperTools_DatabaseTest ViewModel { get; }

    public View_SettingsDeveloperTools_DatabaseTest()
    {
        ViewModel = App.GetService<ViewModel_SettingsDeveloperTools_DatabaseTest>();
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
}

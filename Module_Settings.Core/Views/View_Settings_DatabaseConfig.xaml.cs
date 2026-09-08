using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

/// <summary>
/// Code-behind for the Database Config page. Only handles view-only concerns
/// (file pickers and secure password entry); all behavior lives in the ViewModel.
/// </summary>
public sealed partial class View_Settings_DatabaseConfig : Page
{
    /// <summary>Gets the ViewModel for this page.</summary>
    public ViewModel_Settings_DatabaseConfig ViewModel { get; }

    public View_Settings_DatabaseConfig(ViewModel_Settings_DatabaseConfig viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Clear the password box whenever a run ends so a stale secret never lingers.
        if (
            e.PropertyName == nameof(ViewModel_Settings_DatabaseConfig.IsSyncRunning)
            && !ViewModel.IsSyncRunning
        )
        {
            SyncPasswordBox.Password = string.Empty;
            ViewModel.SyncPassword = null;
        }
    }

    private void SyncPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        ViewModel.SyncPassword = SyncPasswordBox.Password;
    }

    private async void BrowseSyncToolPathButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            FileTypeFilter = { ".py" },
        };

        InitializeWithWindow(picker);
        var file = await picker.PickSingleFileAsync();
        if (file is null)
        {
            return;
        }

        ViewModel.SyncToolPath = file.Path;
    }

    private async void BrowseGenerateSqlButton_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = "review_sync.sql",
            FileTypeChoices = { ["SQL file"] = new System.Collections.Generic.List<string> { ".sql" } },
        };

        InitializeWithWindow(picker);
        var file = await picker.PickSaveFileAsync();
        if (file is null)
        {
            return;
        }

        ViewModel.GenerateSqlOutputPath = file.Path;
    }

    private static void InitializeWithWindow(FileOpenPicker picker)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
    }

    private static void InitializeWithWindow(FileSavePicker picker)
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
    }
}

using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_SharedPaths : Page
{
    public ViewModel_Settings_SharedPaths ViewModel { get; }

    public View_Settings_SharedPaths(ViewModel_Settings_SharedPaths viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void BrowseExportRootPathButton_Click(
        object sender,
        Microsoft.UI.Xaml.RoutedEventArgs e
    )
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync().AsTask();
        if (folder is null)
        {
            return;
        }

        ViewModel.ExportRootPath = folder.Path;
    }
}

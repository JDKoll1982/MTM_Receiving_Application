using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Core.ViewModels;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Settings.Core.Views;

public sealed partial class View_Settings_LabelViewExecutable : Page
{
    public ViewModel_Settings_LabelViewExecutable ViewModel { get; }

    public View_Settings_LabelViewExecutable(ViewModel_Settings_LabelViewExecutable viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void PickExecutableFileButton_Click(
        object sender,
        Microsoft.UI.Xaml.RoutedEventArgs e
    )
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".exe");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync().AsTask();
        if (file is null)
        {
            return;
        }

        ViewModel.ExecutablePath = file.Path;
    }
}

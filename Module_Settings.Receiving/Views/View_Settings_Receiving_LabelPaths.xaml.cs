using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Settings.Receiving.Views;

public sealed partial class View_Settings_Receiving_LabelPaths : Page
{
    public ViewModel_Settings_Receiving_LabelPaths ViewModel { get; }

    public View_Settings_Receiving_LabelPaths(ViewModel_Settings_Receiving_LabelPaths viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void PickReceivingLabelFileButton_Click(
        object sender,
        Microsoft.UI.Xaml.RoutedEventArgs e
    )
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".lbl");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync().AsTask();
        if (file is null)
        {
            return;
        }

        ViewModel.ReceivingLabelPath = file.Path;
    }

    private async void PickMiniReceivingLabelFileButton_Click(
        object sender,
        Microsoft.UI.Xaml.RoutedEventArgs e
    )
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".lbl");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync().AsTask();
        if (file is null)
        {
            return;
        }

        ViewModel.MiniReceivingLabelPath = file.Path;
    }
}

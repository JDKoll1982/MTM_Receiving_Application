using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.Views;

public sealed partial class View_Settings_Dunnage_LabelPaths : Page
{
    public ViewModel_Settings_Dunnage_LabelPaths ViewModel { get; }

    public View_Settings_Dunnage_LabelPaths(ViewModel_Settings_Dunnage_LabelPaths viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void PickDunnageLabelFileButton_Click(
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

        ViewModel.DunnageLabelPath = file.Path;
    }
}

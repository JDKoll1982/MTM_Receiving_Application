using System;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Settings.Volvo.Views;

public sealed partial class View_Settings_Volvo_LabelPaths : Page
{
    public ViewModel_Settings_Volvo_LabelPaths ViewModel { get; }

    public View_Settings_Volvo_LabelPaths(ViewModel_Settings_Volvo_LabelPaths viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void PickVolvoLabelFileButton_Click(
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

        ViewModel.VolvoLabelPath = file.Path;
    }
}

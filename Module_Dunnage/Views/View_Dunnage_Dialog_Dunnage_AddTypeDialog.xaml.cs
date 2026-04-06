using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Shared.Views;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_Dialog_Dunnage_AddTypeDialog : ContentDialog
{
    public ViewModel_Dunnage_AddTypeDialog ViewModel { get; }
    private readonly IService_Focus _focusService;

    public View_Dunnage_Dialog_Dunnage_AddTypeDialog()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_AddTypeDialog>();
        _focusService = App.GetService<IService_Focus>();
        DataContext = ViewModel;
        InitializeComponent();

        _focusService.AttachFocusOnVisibility(this);
        Closed += OnClosed;
    }

    private async void OnSelectIconClick(object sender, RoutedEventArgs e)
    {
        var iconSelector = new View_Shared_IconSelectorWindow();

        // Set initial selection to current icon
        iconSelector.SetInitialSelection(ViewModel.SelectedIcon);

        iconSelector.Activate();

        // Wait for the window to close and get the result
        var selectedIcon = await iconSelector.WaitForSelectionAsync();

        if (selectedIcon.HasValue)
        {
            ViewModel.SelectedIcon = selectedIcon.Value;
        }
    }

    private async void OnChooseImageClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".png");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync().AsTask();
        if (file is null)
        {
            return;
        }

        if (
            string.Equals(
                Path.GetExtension(file.Path),
                ".png",
                System.StringComparison.OrdinalIgnoreCase
            )
            is false
        )
        {
            return;
        }

        ViewModel.SelectedImagePath = file.Path;
    }

    private void OnClearImageClick(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedImagePath = null;
    }

    private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        if (ViewModel is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

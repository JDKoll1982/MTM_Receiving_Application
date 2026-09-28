using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using Material.Icons;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Shared.Views;
using Windows.Foundation;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_QuickAddTypeDialog : ContentDialog
{
    private readonly IService_Focus _focusService;
    private readonly IService_DunnageImageStorage _imageStorage;

    public ViewModel_Dunnage_QuickAddTypeDialog ViewModel { get; }

    public bool WasAccepted { get; private set; }

    public string TypeName => ViewModel.TypeName;

    public MaterialIconKind SelectedIconKind => ViewModel.SelectedIconKind;

    public string? SelectedImagePath => ViewModel.SelectedImagePathForSave;

    public ObservableCollection<Model_SpecItem> Specs => ViewModel.Specs;

    public bool RequestDelete { get; private set; }

    public View_Dunnage_QuickAddTypeDialog()
    {
        ViewModel = App.GetService<ViewModel_Dunnage_QuickAddTypeDialog>();
        _focusService = App.GetService<IService_Focus>();
        _imageStorage = App.GetService<IService_DunnageImageStorage>();
        DataContext = ViewModel;
        WasAccepted = false;
        RequestDelete = false;

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        ViewModel.InitializeForCreate();
        UpdateDeleteVisibility(false);
        _focusService.AttachFocusOnVisibility(this);
    }

    public void PrepareDialogSize()
    {
        if (XamlRoot is null)
        {
            return;
        }

        RootGrid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        var desiredWidth = Math.Ceiling(Math.Max(RootGrid.DesiredSize.Width, 1180) + 32);
        var availableWidth = Math.Max(1120, XamlRoot.Size.Width - 32);
        var availableHeight = Math.Max(680, XamlRoot.Size.Height - 48);

        Width = Math.Min(desiredWidth, availableWidth);
        MinWidth = Math.Min(1180, availableWidth);
        MinHeight = Math.Min(680, availableHeight);
        MaxHeight = availableHeight;
    }

    public void InitializeForEdit(
        string typeName,
        string iconName,
        string? imagePath,
        List<Model_CustomFieldDefinition> customFields,
        bool canDelete
    )
    {
        WasAccepted = false;
        RequestDelete = false;
        ViewModel.InitializeForEdit(typeName, iconName, imagePath, customFields);
        UpdateDeleteVisibility(canDelete);
    }

    private void UpdateDeleteVisibility(bool canDelete)
    {
        FooterDeleteButton.Visibility = canDelete ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OnSelectIconClick(object sender, RoutedEventArgs e)
    {
        var iconWindow = new View_Shared_IconSelectorWindow();
        iconWindow.SetInitialSelection(ViewModel.SelectedIconKind);
        iconWindow.Activate();

        MaterialIconKind? selectedIcon = await iconWindow.WaitForSelectionAsync();
        if (selectedIcon.HasValue)
        {
            ViewModel.SelectedIconKind = selectedIcon.Value;
            ViewModel.IsImageMode = false;
        }
    }

    private async void OnChooseImageClick(object sender, RoutedEventArgs e)
    {
        if (App.MainWindow is null)
        {
            return;
        }

        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync().AsTask();
        if (file is null)
        {
            return;
        }

        var extension = Path.GetExtension(file.Path);
        if (
            string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase) is false
            && string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase) is false
            && string.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase) is false
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

    private void OnOpenImageFolderTapped(object sender, TappedRoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.SelectedImagePath))
        {
            return;
        }

        var absoluteImagePath = Path.IsPathRooted(ViewModel.SelectedImagePath)
            ? ViewModel.SelectedImagePath
            : _imageStorage.GetAbsolutePath(ViewModel.SelectedImagePath);

        if (string.IsNullOrWhiteSpace(absoluteImagePath))
        {
            return;
        }

        var directoryPath = File.Exists(absoluteImagePath)
            ? Path.GetDirectoryName(absoluteImagePath)
            : Directory.Exists(absoluteImagePath)
                ? absoluteImagePath
                : Path.GetDirectoryName(absoluteImagePath);

        if (string.IsNullOrWhiteSpace(directoryPath) || Directory.Exists(directoryPath) is false)
        {
            return;
        }

        var explorerArguments = File.Exists(absoluteImagePath)
            ? $"/select,\"{absoluteImagePath}\""
            : $"\"{directoryPath}\"";

        Process.Start(
            new ProcessStartInfo("explorer.exe", explorerArguments) { UseShellExecute = true }
        );
    }

    private async void OnRotateImageClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.SelectedImagePath))
        {
            return;
        }

        var result = await _imageStorage.CreateRotatedWorkingCopyAsync(ViewModel.SelectedImagePath);
        if (!result.IsSuccess)
        {
            return;
        }

        ViewModel.SelectedImagePath = result.Data;
    }

    private void OnRemoveChoiceClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: string choice })
        {
            ViewModel.RemoveChoiceCommand.Execute(choice);
        }
    }

    private void OnRemoveSpecClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: Model_SpecItem spec })
        {
            ViewModel.RemoveSpecCommand.Execute(spec);
        }
    }

    private void OnFooterPrimaryButtonClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.TryCommit())
        {
            WasAccepted = true;
            Hide();
        }
    }

    private void OnFooterCancelButtonClick(object sender, RoutedEventArgs e)
    {
        WasAccepted = false;
        Hide();
    }

    private void OnFooterDeleteButtonClick(object sender, RoutedEventArgs e)
    {
        WasAccepted = false;
        RequestDelete = true;
        Hide();
    }
}

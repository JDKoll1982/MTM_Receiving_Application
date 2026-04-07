using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Material.Icons;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.ViewModels;
using MTM_Receiving_Application.Module_Shared.Views;
using Windows.Foundation;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Dunnage.Views;

public sealed partial class View_Dunnage_QuickAddTypeDialog : ContentDialog
{
    private readonly IService_Focus _focusService;

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
        DataContext = ViewModel;
        WasAccepted = false;
        RequestDelete = false;

        InitializeComponent();

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
        Dictionary<string, SpecDefinition> specs,
        bool canDelete
    )
    {
        WasAccepted = false;
        RequestDelete = false;
        ViewModel.InitializeForEdit(typeName, iconName, imagePath, specs);
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
            string.Equals(Path.GetExtension(file.Path), ".png", StringComparison.OrdinalIgnoreCase)
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

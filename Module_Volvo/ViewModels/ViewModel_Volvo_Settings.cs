using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for Volvo master data settings (parts catalog management)
/// </summary>
public partial class ViewModel_Volvo_Settings : ViewModel_Shared_Base
{
    private readonly IService_VolvoMasterData _masterDataService;

    [ObservableProperty]
    private ObservableCollection<Model_VolvoPart> _parts = new();

    [ObservableProperty]
    private Model_VolvoPart? _selectedPart;

    [ObservableProperty]
    private bool _showInactive = false;

    [ObservableProperty]
    private int _totalPartsCount;

    [ObservableProperty]
    private int _activePartsCount;

    public ViewModel_Volvo_Settings(
        IService_VolvoMasterData masterDataService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _masterDataService = masterDataService ?? throw new ArgumentNullException(nameof(masterDataService));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Loading parts catalog...";

            var result = await _masterDataService.GetAllPartsAsync(ShowInactive);

            if (result.IsSuccess && result.Data != null)
            {
                Parts.Clear();
                foreach (var part in result.Data.OrderBy(p => p.PartNumber))
                {
                    Parts.Add(part);
                }

                TotalPartsCount = Parts.Count;
                ActivePartsCount = Parts.Count(p => p.IsActive);
                StatusMessage = $"Loaded {ActivePartsCount} active parts (Total: {TotalPartsCount})";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to load parts catalog",
                    "Load Error",
                    nameof(RefreshAsync));
                StatusMessage = "Failed to load parts";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(RefreshAsync),
                nameof(ViewModel_Volvo_Settings));
            StatusMessage = "Error loading parts";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddPartAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            StatusMessage = "Opening add part dialog...";

            var dialog = new Views.VolvoPartAddEditDialog();
            dialog.InitializeForAdd();

            var windowService = App.GetService<IService_Window>();
            if (windowService != null)
            {
                dialog.XamlRoot = windowService.GetXamlRoot();
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Part != null)
            {
                IsBusy = true;
                StatusMessage = $"Adding part {dialog.Part.PartNumber}...";

                // No components for now (simplified version)
                var saveResult = await _masterDataService.AddPartAsync(dialog.Part, new System.Collections.Generic.List<Model_VolvoPartComponent>());

                if (saveResult.IsSuccess)
                {
                    StatusMessage = $"Part {dialog.Part.PartNumber} added successfully";
                    await RefreshAsync();
                }
                else
                {
                    await _errorHandler.ShowUserErrorAsync(
                        saveResult.ErrorMessage ?? "Failed to add part",
                        "Add Error",
                        nameof(AddPartAsync));
                    StatusMessage = "Failed to add part";
                }

                IsBusy = false;
            }
            else
            {
                StatusMessage = "Add part cancelled";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(AddPartAsync),
                nameof(ViewModel_Volvo_Settings));
            StatusMessage = "Error adding part";
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditPart))]
    private async Task EditPartAsync()
    {
        if (IsBusy || SelectedPart == null)
        {
            return;
        }

        try
        {
            StatusMessage = $"Opening edit dialog for {SelectedPart.PartNumber}...";

            var dialog = new Views.VolvoPartAddEditDialog();
            dialog.InitializeForEdit(SelectedPart);

            var windowService = App.GetService<IService_Window>();
            if (windowService != null)
            {
                dialog.XamlRoot = windowService.GetXamlRoot();
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Part != null)
            {
                IsBusy = true;
                StatusMessage = $"Updating part {dialog.Part.PartNumber}...";

                // No components for now (simplified version)
                var saveResult = await _masterDataService.UpdatePartAsync(dialog.Part, new System.Collections.Generic.List<Model_VolvoPartComponent>());

                if (saveResult.IsSuccess)
                {
                    StatusMessage = $"Part {dialog.Part.PartNumber} updated successfully";
                    await RefreshAsync();
                }
                else
                {
                    await _errorHandler.ShowUserErrorAsync(
                        saveResult.ErrorMessage ?? "Failed to update part",
                        "Update Error",
                        nameof(EditPartAsync));
                    StatusMessage = "Failed to update part";
                }

                IsBusy = false;
            }
            else
            {
                StatusMessage = "Edit cancelled";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(EditPartAsync),
                nameof(ViewModel_Volvo_Settings));
            StatusMessage = "Error editing part";
        }
    }

    private bool CanEditPart() => SelectedPart != null && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanDeactivatePart))]
    private async Task DeactivatePartAsync()
    {
        if (IsBusy || SelectedPart == null)
        {
            return;
        }

        try
        {
            // Confirmation dialog
            var dialog = new ContentDialog
            {
                Title = "Deactivate Part",
                Content = $"Are you sure you want to deactivate part {SelectedPart.PartNumber}?\n\n" +
                          "This will hide the part from active lists, but it will remain in historical shipments.",
                PrimaryButtonText = "Deactivate",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close
            };

            // Get XamlRoot from service
            var windowService = App.GetService<IService_Window>();
            if (windowService != null)
            {
                dialog.XamlRoot = windowService.GetXamlRoot();
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                IsBusy = true;
                StatusMessage = $"Deactivating part {SelectedPart.PartNumber}...";

                var deactivateResult = await _masterDataService.DeactivatePartAsync(SelectedPart.PartNumber);

                if (deactivateResult.IsSuccess)
                {
                    StatusMessage = $"Part {SelectedPart.PartNumber} deactivated";
                    await RefreshAsync();
                }
                else
                {
                    await _errorHandler.ShowUserErrorAsync(
                        deactivateResult.ErrorMessage ?? "Failed to deactivate part",
                        "Deactivate Error",
                        nameof(DeactivatePartAsync));
                    StatusMessage = "Failed to deactivate part";
                }

                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(DeactivatePartAsync),
                nameof(ViewModel_Volvo_Settings));
            IsBusy = false;
        }
    }

    private bool CanDeactivatePart() => SelectedPart?.IsActive == true && !IsBusy;

    [RelayCommand(CanExecute = nameof(CanViewComponents))]
    private async Task ViewComponentsAsync()
    {
        if (IsBusy || SelectedPart == null)
        {
            return;
        }

        try
        {
            StatusMessage = $"Loading components for {SelectedPart.PartNumber}...";

            var result = await _masterDataService.GetComponentsAsync(SelectedPart.PartNumber);

            if (result.IsSuccess && result.Data != null)
            {
                var componentsList = result.Data.Count > 0
                    ? string.Join("\n", result.Data.Select(c => $"• {c.ComponentPartNumber} (Qty: {c.Quantity})"))
                    : "No components defined";

                var dialog = new ContentDialog
                {
                    Title = $"Components for {SelectedPart.PartNumber}",
                    Content = componentsList,
                    CloseButtonText = "Close"
                };

                var windowService = App.GetService<IService_Window>();
                if (windowService != null)
                {
                    dialog.XamlRoot = windowService.GetXamlRoot();
                }

                await dialog.ShowAsync();
                StatusMessage = $"Showing {result.Data.Count} components";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to load components",
                    "Load Error",
                    nameof(ViewComponentsAsync));
                StatusMessage = "Failed to load components";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Low,
                nameof(ViewComponentsAsync),
                nameof(ViewModel_Volvo_Settings));
        }
    }

    private bool CanViewComponents() => SelectedPart != null && !IsBusy;

    [RelayCommand]
    private async Task ImportCsvAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            // File picker
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".csv");
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // Get window handle
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }

            IsBusy = true;
            StatusMessage = "Importing CSV...";

            var csvContent = await FileIO.ReadTextAsync(file);
            var result = await _masterDataService.ImportCsvAsync(csvContent);

            if (result.IsSuccess)
            {
                var (newCount, updatedCount, unchangedCount) = result.Data;
                var summary = $"Import complete: {newCount} new, {updatedCount} updated, {unchangedCount} unchanged";
                StatusMessage = summary;
                await RefreshAsync();

                // Show summary dialog
                var dialog = new ContentDialog
                {
                    Title = "Import Complete",
                    Content = summary,
                    CloseButtonText = "OK"
                };

                var windowService = App.GetService<IService_Window>();
                if (windowService != null)
                {
                    dialog.XamlRoot = windowService.GetXamlRoot();
                }

                await dialog.ShowAsync();
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to import CSV",
                    "Import Error",
                    nameof(ImportCsvAsync));
                StatusMessage = "Import failed";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(ImportCsvAsync),
                nameof(ViewModel_Volvo_Settings));
            StatusMessage = "Error importing CSV";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            // File picker
            var picker = new FileSavePicker();
            picker.FileTypeChoices.Add("CSV File", new[] { ".csv" });
            picker.SuggestedFileName = $"volvo_parts_{DateTime.Now:yyyyMMdd}.csv";
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            // Get window handle
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                return;
            }

            IsBusy = true;
            StatusMessage = "Exporting to CSV...";

            var result = await _masterDataService.ExportCsvAsync(file.Path, ShowInactive);

            if (result.IsSuccess && result.Data != null)
            {
                await FileIO.WriteTextAsync(file, result.Data);
                StatusMessage = $"Exported to {file.Path}";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to export CSV",
                    "Export Error",
                    nameof(ExportCsvAsync));
                StatusMessage = "Export failed";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(ExportCsvAsync),
                nameof(ViewModel_Volvo_Settings));
            StatusMessage = "Error exporting CSV";
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnShowInactiveChanged(bool value)
    {
        // Refresh when toggle changes
        _ = RefreshAsync();
    }

    partial void OnSelectedPartChanged(Model_VolvoPart? value)
    {
        // Notify commands that depend on selection
        EditPartCommand.NotifyCanExecuteChanged();
        DeactivatePartCommand.NotifyCanExecuteChanged();
        ViewComponentsCommand.NotifyCanExecuteChanged();
    }
}

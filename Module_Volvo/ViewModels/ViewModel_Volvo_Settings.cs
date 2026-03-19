using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Volvo.ViewModels;

/// <summary>
/// ViewModel for Volvo master data settings (parts catalog management)
/// </summary>
public partial class ViewModel_Volvo_Settings : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly IService_Window _windowService;

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
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService,
        IService_Window windowService) : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
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

            var result = await _mediator.Send(new GetAllVolvoPartsQuery
            {
                IncludeInactive = ShowInactive
            });

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

            if (_windowService != null)
            {
                dialog.XamlRoot = _windowService.GetXamlRoot();
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Part != null)
            {
                IsBusy = true;
                StatusMessage = $"Adding part {dialog.Part.PartNumber}...";

                var saveResult = await _mediator.Send(new AddVolvoPartCommand
                {
                    PartNumber = dialog.Part.PartNumber,
                    QuantityPerSkid = dialog.Part.QuantityPerSkid
                });

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

            if (_windowService != null)
            {
                dialog.XamlRoot = _windowService.GetXamlRoot();
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary && dialog.Part != null)
            {
                IsBusy = true;
                StatusMessage = $"Updating part {dialog.Part.PartNumber}...";

                var saveResult = await _mediator.Send(new UpdateVolvoPartCommand
                {
                    PartNumber = dialog.Part.PartNumber,
                    QuantityPerSkid = dialog.Part.QuantityPerSkid
                });

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
            if (_windowService != null)
            {
                dialog.XamlRoot = _windowService.GetXamlRoot();
            }

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                IsBusy = true;
                StatusMessage = $"Deactivating part {SelectedPart.PartNumber}...";

                var deactivateResult = await _mediator.Send(new DeactivateVolvoPartCommand
                {
                    PartNumber = SelectedPart.PartNumber
                });

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

            var result = await _mediator.Send(new GetPartComponentsQuery
            {
                PartNumber = SelectedPart.PartNumber
            });

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

                if (_windowService != null)
                {
                    dialog.XamlRoot = _windowService.GetXamlRoot();
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
    private async Task ImportDataAsync()
    {
        if (IsBusy)
            return;

        var xamlRoot = _windowService?.GetXamlRoot();
        if (xamlRoot == null)
        {
            await _errorHandler.ShowUserErrorAsync(
                "Cannot show import dialog — window not available",
                "Import Error",
                nameof(ImportDataAsync));
            return;
        }

        // Ask the user for bulk part data: one "PartNumber,QuantityPerSkid" per line.
        var inputBox = new TextBox
        {
            PlaceholderText = "PartNumber,QuantityPerSkid — one per line",
            AcceptsReturn = true,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            Height = 220,
            VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top
        };

        var dialog = new ContentDialog
        {
            Title = "Bulk Import Parts",
            Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "Enter parts to import (one per line):" },
                    new TextBlock
                    {
                        Text = "Format:  PartNumber,QuantityPerSkid",
                        FontSize = 11,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
                    },
                    inputBox
                }
            },
            PrimaryButtonText = "Import",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
            MinWidth = 420
        };

        var dialogResult = await dialog.ShowAsync();
        if (dialogResult != ContentDialogResult.Primary)
            return;

        var rawText = inputBox.Text ?? string.Empty;
        var parts = new System.Collections.Generic.List<PartImportItem>();
        var parseErrors = new System.Collections.Generic.List<string>();

        int lineNumber = 0;
        foreach (var rawLine in rawText.Split('\n', System.StringSplitOptions.RemoveEmptyEntries))
        {
            lineNumber++;
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            var fields = line.Split(',');
            if (fields.Length < 2)
            {
                parseErrors.Add($"Line {lineNumber}: expected 'PartNumber,QuantityPerSkid'");
                continue;
            }

            var partNumber = fields[0].Trim();
            if (string.IsNullOrWhiteSpace(partNumber))
            {
                parseErrors.Add($"Line {lineNumber}: part number is empty");
                continue;
            }

            if (!int.TryParse(fields[1].Trim(), out int qty) || qty <= 0)
            {
                parseErrors.Add($"Line {lineNumber}: invalid quantity '{fields[1].Trim()}'");
                continue;
            }

            parts.Add(new PartImportItem { PartNumber = partNumber, QuantityPerSkid = qty });
        }

        if (parseErrors.Count > 0 && parts.Count == 0)
        {
            await _errorHandler.ShowUserErrorAsync(
                "No valid rows to import:\n" + string.Join('\n', parseErrors),
                "Import Error",
                nameof(ImportDataAsync));
            return;
        }

        if (parts.Count == 0)
        {
            await _errorHandler.ShowUserErrorAsync(
                "No data entered.",
                "Import Error",
                nameof(ImportDataAsync));
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = $"Importing {parts.Count} part(s)…";

            var command = new ImportPartsCommand { Parts = parts };
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                var d = result.Data!;
                var summary = $"Import complete: {d.SuccessCount} succeeded, {d.FailureCount} failed";
                if (parseErrors.Count > 0)
                    summary += $"\n{parseErrors.Count} line(s) could not be parsed";

                StatusMessage = summary;
                await _logger.LogInfoAsync(summary);
                await RefreshAsync();
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Import failed",
                    "Import Error",
                    nameof(ImportDataAsync));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(ImportDataAsync),
                nameof(ViewModel_Volvo_Settings));
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

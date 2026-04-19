using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_Volvo.Models;
using MTM_Receiving_Application.Module_Volvo.Requests.Commands;
using MTM_Receiving_Application.Module_Volvo.Requests.Queries;

namespace MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

/// <summary>
/// Settings page ViewModel for Volvo part master-data maintenance.
/// </summary>
public partial class ViewModel_Settings_Volvo_PartCatalog : ViewModel_Shared_Base
{
    private readonly IMediator _mediator;
    private readonly IService_Window _windowService;

    [ObservableProperty]
    private ObservableCollection<Model_VolvoPart> _parts = new();

    [ObservableProperty]
    private Model_VolvoPart? _selectedPart;

    [ObservableProperty]
    private bool _showInactive;

    [ObservableProperty]
    private int _totalPartsCount;

    [ObservableProperty]
    private int _activePartsCount;

    public ViewModel_Settings_Volvo_PartCatalog(
        IMediator mediator,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService,
        IService_Window windowService
    )
        : base(errorHandler, logger, notificationService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));

        Title = "Part Catalog";
        StatusMessage = "Load the Volvo parts catalog to review and maintain master data.";
    }

    public int InactivePartsCount => Math.Max(TotalPartsCount - ActivePartsCount, 0);

    public string CatalogFilterSummary =>
        ShowInactive
            ? "Active and inactive parts are visible in the catalog."
            : "Only active parts are visible in the catalog.";

    public string PartsLoadedSummary =>
        TotalPartsCount == 0
            ? "No Volvo parts are currently loaded."
            : $"{ActivePartsCount} active / {InactivePartsCount} inactive parts.";

    public bool HasSelectedPart => SelectedPart is not null;

    public string SelectedPartNumberText => SelectedPart?.PartNumber ?? "No part selected";

    public string SelectedPartQuantityText =>
        SelectedPart is null
            ? "Select a part from the catalog to review its skid quantity and available actions."
            : $"{SelectedPart.QuantityPerSkid} units per skid";

    public string SelectedPartStatusText =>
        SelectedPart is null ? "Selection required"
        : SelectedPart.IsActive ? "Active part"
        : "Inactive part";

    public string SelectedPartHintText =>
        SelectedPart is null
            ? "Edit, deactivate, and component-review actions become available after you choose a part."
        : SelectedPart.IsActive
            ? "This part can be edited, reviewed, or deactivated from this page."
        : "Inactive parts remain reviewable; they stay visible only when inactive rows are included.";

    [RelayCommand(CanExecute = nameof(CanRunCatalogAction))]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            SetBusyState(true);
            await ReloadCatalogAsync();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(RefreshAsync),
                nameof(ViewModel_Settings_Volvo_PartCatalog)
            );
            StatusMessage = "Error loading Volvo parts.";
        }
        finally
        {
            SetBusyState(false);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRunCatalogAction))]
    private async Task AddPartAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            StatusMessage = "Opening the part editor...";

            var dialog = new Views.View_Settings_Volvo_PartAddEditDialog();
            dialog.InitializeForAdd();

            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot is not null)
            {
                dialog.XamlRoot = xamlRoot;
                dialog.PrepareDialogSize();
            }

            var dialogResult = await dialog.ShowAsync();
            if (dialogResult != ContentDialogResult.Primary || dialog.Part is null)
            {
                StatusMessage = "Add part cancelled.";
                return;
            }

            SetBusyState(true);
            StatusMessage = $"Adding part {dialog.Part.PartNumber}...";

            var saveResult = await _mediator.Send(
                new AddVolvoPartCommand
                {
                    PartNumber = dialog.Part.PartNumber,
                    QuantityPerSkid = dialog.Part.QuantityPerSkid,
                }
            );

            if (saveResult.IsSuccess)
            {
                await ReloadCatalogAsync();
                StatusMessage = $"Part {dialog.Part.PartNumber} added successfully.";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    saveResult.ErrorMessage ?? "Failed to add part",
                    "Add Error",
                    nameof(AddPartAsync)
                );
                StatusMessage = "Failed to add part.";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(AddPartAsync),
                nameof(ViewModel_Settings_Volvo_PartCatalog)
            );
            StatusMessage = "Error adding a Volvo part.";
        }
        finally
        {
            SetBusyState(false);
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditPart))]
    private async Task EditPartAsync()
    {
        if (SelectedPart is null || IsBusy)
        {
            return;
        }

        try
        {
            StatusMessage = $"Opening the editor for {SelectedPart.PartNumber}...";

            var dialog = new Views.View_Settings_Volvo_PartAddEditDialog();
            dialog.InitializeForEdit(SelectedPart);

            var xamlRoot = _windowService.GetXamlRoot();
            if (xamlRoot is not null)
            {
                dialog.XamlRoot = xamlRoot;
                dialog.PrepareDialogSize();
            }

            var dialogResult = await dialog.ShowAsync();
            if (dialogResult != ContentDialogResult.Primary || dialog.Part is null)
            {
                StatusMessage = "Edit part cancelled.";
                return;
            }

            SetBusyState(true);
            StatusMessage = $"Updating part {dialog.Part.PartNumber}...";

            var saveResult = await _mediator.Send(
                new UpdateVolvoPartCommand
                {
                    PartNumber = dialog.Part.PartNumber,
                    QuantityPerSkid = dialog.Part.QuantityPerSkid,
                }
            );

            if (saveResult.IsSuccess)
            {
                await ReloadCatalogAsync();
                StatusMessage = $"Part {dialog.Part.PartNumber} updated successfully.";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    saveResult.ErrorMessage ?? "Failed to update part",
                    "Update Error",
                    nameof(EditPartAsync)
                );
                StatusMessage = "Failed to update part.";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(EditPartAsync),
                nameof(ViewModel_Settings_Volvo_PartCatalog)
            );
            StatusMessage = "Error editing the selected part.";
        }
        finally
        {
            SetBusyState(false);
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeactivatePart))]
    private async Task DeactivatePartAsync()
    {
        if (SelectedPart is null || IsBusy)
        {
            return;
        }

        try
        {
            var dialog = new ContentDialog
            {
                Title = "Deactivate Part",
                Content =
                    $"Are you sure you want to deactivate part {SelectedPart.PartNumber}?\n\n"
                    + "The part will remain available in shipment history, but it will no longer appear in active part selections.",
                PrimaryButtonText = "Deactivate",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = _windowService.GetXamlRoot(),
            };

            Helper_UI_ContentDialogTheme.ApplyTheme(dialog, dialog.XamlRoot);

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            SetBusyState(true);
            StatusMessage = $"Deactivating part {SelectedPart.PartNumber}...";

            var deactivateResult = await _mediator.Send(
                new DeactivateVolvoPartCommand { PartNumber = SelectedPart.PartNumber }
            );

            if (deactivateResult.IsSuccess)
            {
                await ReloadCatalogAsync();
                StatusMessage = $"Part {SelectedPart.PartNumber} deactivated.";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    deactivateResult.ErrorMessage ?? "Failed to deactivate part",
                    "Deactivate Error",
                    nameof(DeactivatePartAsync)
                );
                StatusMessage = "Failed to deactivate part.";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(DeactivatePartAsync),
                nameof(ViewModel_Settings_Volvo_PartCatalog)
            );
            StatusMessage = "Error deactivating the selected part.";
        }
        finally
        {
            SetBusyState(false);
        }
    }

    [RelayCommand(CanExecute = nameof(CanViewComponents))]
    private async Task ViewComponentsAsync()
    {
        if (SelectedPart is null || IsBusy)
        {
            return;
        }

        try
        {
            SetBusyState(true);
            StatusMessage = $"Loading components for {SelectedPart.PartNumber}...";

            var result = await _mediator.Send(
                new GetPartComponentsQuery { PartNumber = SelectedPart.PartNumber }
            );

            if (result.IsSuccess && result.Data != null)
            {
                var componentsList =
                    result.Data.Count > 0
                        ? string.Join(
                            "\n",
                            result.Data.Select(component =>
                                $"• {component.ComponentPartNumber} (Qty: {component.Quantity})"
                            )
                        )
                        : "No components defined for this part.";

                var dialog = new ContentDialog
                {
                    Title = $"Components for {SelectedPart.PartNumber}",
                    Content = componentsList,
                    CloseButtonText = "Close",
                    XamlRoot = _windowService.GetXamlRoot(),
                };

                Helper_UI_ContentDialogTheme.ApplyTheme(dialog, dialog.XamlRoot);

                await dialog.ShowAsync();
                StatusMessage = $"Showing {result.Data.Count} component record(s).";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Failed to load components",
                    "Load Error",
                    nameof(ViewComponentsAsync)
                );
                StatusMessage = "Failed to load part components.";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Low,
                nameof(ViewComponentsAsync),
                nameof(ViewModel_Settings_Volvo_PartCatalog)
            );
            StatusMessage = "Error loading part components.";
        }
        finally
        {
            SetBusyState(false);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRunCatalogAction))]
    private async Task ImportDataAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var xamlRoot = _windowService.GetXamlRoot();
        if (xamlRoot is null)
        {
            await _errorHandler.ShowUserErrorAsync(
                "Cannot show the import dialog because the window host is unavailable.",
                "Import Error",
                nameof(ImportDataAsync)
            );
            return;
        }

        var inputBox = new TextBox
        {
            PlaceholderText = "PartNumber,QuantityPerSkid — one per line",
            AcceptsReturn = true,
            TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
            Height = 220,
            VerticalContentAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top,
        };

        var dialog = new ContentDialog
        {
            Title = "Bulk Import Parts",
            Content = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = "Enter parts to import, one per line." },
                    new TextBlock
                    {
                        Text = "Format: PartNumber,QuantityPerSkid",
                        FontSize = 11,
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                            Microsoft.UI.Colors.Gray
                        ),
                    },
                    inputBox,
                },
            },
            PrimaryButtonText = "Import",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = xamlRoot,
            MinWidth = 420,
        };

        Helper_UI_ContentDialogTheme.ApplyTheme(dialog, xamlRoot);

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        var parseErrors = new List<string>();
        var parts = ParseImportLines(inputBox.Text ?? string.Empty, parseErrors);

        if (parseErrors.Count > 0 && parts.Count == 0)
        {
            await _errorHandler.ShowUserErrorAsync(
                "No valid rows to import:\n" + string.Join('\n', parseErrors),
                "Import Error",
                nameof(ImportDataAsync)
            );
            return;
        }

        if (parts.Count == 0)
        {
            await _errorHandler.ShowUserErrorAsync(
                "No data entered.",
                "Import Error",
                nameof(ImportDataAsync)
            );
            return;
        }

        try
        {
            SetBusyState(true);
            StatusMessage = $"Importing {parts.Count} part(s)...";

            var result = await _mediator.Send(new ImportPartsCommand { Parts = parts });
            if (result.IsSuccess)
            {
                await ReloadCatalogAsync();

                var importResult = result.Data!;
                var summary =
                    $"Import complete: {importResult.SuccessCount} succeeded, {importResult.FailureCount} failed.";
                if (parseErrors.Count > 0)
                {
                    summary += $" {parseErrors.Count} line(s) were skipped during parsing.";
                }

                StatusMessage = summary;
                await _logger.LogInfoAsync(summary);
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(
                    result.ErrorMessage ?? "Import failed",
                    "Import Error",
                    nameof(ImportDataAsync)
                );
                StatusMessage = "Part import failed.";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ImportDataAsync),
                nameof(ViewModel_Settings_Volvo_PartCatalog)
            );
            StatusMessage = "Error importing Volvo parts.";
        }
        finally
        {
            SetBusyState(false);
        }
    }

    partial void OnShowInactiveChanged(bool value)
    {
        OnPropertyChanged(nameof(CatalogFilterSummary));
        _ = RefreshAsync();
    }

    partial void OnSelectedPartChanged(Model_VolvoPart? value)
    {
        UpdateCommandStates();
        OnPropertyChanged(nameof(HasSelectedPart));
        OnPropertyChanged(nameof(SelectedPartNumberText));
        OnPropertyChanged(nameof(SelectedPartQuantityText));
        OnPropertyChanged(nameof(SelectedPartStatusText));
        OnPropertyChanged(nameof(SelectedPartHintText));
    }

    partial void OnTotalPartsCountChanged(int value)
    {
        OnPropertyChanged(nameof(InactivePartsCount));
        OnPropertyChanged(nameof(PartsLoadedSummary));
    }

    partial void OnActivePartsCountChanged(int value)
    {
        OnPropertyChanged(nameof(InactivePartsCount));
        OnPropertyChanged(nameof(PartsLoadedSummary));
    }

    private bool CanRunCatalogAction() => IsBusy is false;

    private bool CanEditPart() => SelectedPart is not null && IsBusy is false;

    private bool CanDeactivatePart() => SelectedPart?.IsActive == true && IsBusy is false;

    private bool CanViewComponents() => SelectedPart is not null && IsBusy is false;

    private async Task ReloadCatalogAsync()
    {
        var selectedPartNumber = SelectedPart?.PartNumber;
        StatusMessage = ShowInactive
            ? "Loading Volvo parts catalog, including inactive parts..."
            : "Loading active Volvo parts catalog...";

        var result = await _mediator.Send(
            new GetAllVolvoPartsQuery { IncludeInactive = ShowInactive }
        );
        if (result.IsSuccess && result.Data != null)
        {
            var orderedParts = result.Data.OrderBy(part => part.PartNumber).ToList();
            Parts = new ObservableCollection<Model_VolvoPart>(orderedParts);
            TotalPartsCount = Parts.Count;
            ActivePartsCount = Parts.Count(part => part.IsActive);
            SelectedPart = selectedPartNumber is null
                ? null
                : Parts.FirstOrDefault(part => part.PartNumber == selectedPartNumber);

            StatusMessage =
                TotalPartsCount == 0
                    ? "No Volvo parts match the current filter."
                    : $"Loaded {TotalPartsCount} Volvo part(s).";
        }
        else
        {
            Parts = new ObservableCollection<Model_VolvoPart>();
            TotalPartsCount = 0;
            ActivePartsCount = 0;
            SelectedPart = null;

            await _errorHandler.ShowUserErrorAsync(
                result.ErrorMessage ?? "Failed to load parts catalog",
                "Load Error",
                nameof(RefreshAsync)
            );
            StatusMessage = "Failed to load the Volvo parts catalog.";
        }
    }

    private void SetBusyState(bool isBusy)
    {
        IsBusy = isBusy;
        UpdateCommandStates();
    }

    private void UpdateCommandStates()
    {
        RefreshCommand.NotifyCanExecuteChanged();
        AddPartCommand.NotifyCanExecuteChanged();
        EditPartCommand.NotifyCanExecuteChanged();
        DeactivatePartCommand.NotifyCanExecuteChanged();
        ViewComponentsCommand.NotifyCanExecuteChanged();
        ImportDataCommand.NotifyCanExecuteChanged();
    }

    private static List<PartImportItem> ParseImportLines(
        string rawText,
        ICollection<string> parseErrors
    )
    {
        var parts = new List<PartImportItem>();
        var lineNumber = 0;

        foreach (var rawLine in rawText.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            lineNumber++;
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            {
                continue;
            }

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

            if (!int.TryParse(fields[1].Trim(), out var quantityPerSkid) || quantityPerSkid <= 0)
            {
                parseErrors.Add($"Line {lineNumber}: invalid quantity '{fields[1].Trim()}'");
                continue;
            }

            parts.Add(
                new PartImportItem { PartNumber = partNumber, QuantityPerSkid = quantityPerSkid }
            );
        }

        return parts;
    }
}

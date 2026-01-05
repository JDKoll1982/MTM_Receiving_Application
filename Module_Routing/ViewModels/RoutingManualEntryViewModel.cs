using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Routing.Services;
using MTM_Receiving_Application.ViewModels.Shared;

namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// ViewModel for Manual Entry mode - batch label creation via DataGrid
/// </summary>
public partial class RoutingManualEntryViewModel : BaseViewModel
{
    private readonly IRoutingService _routingService;
    private readonly IRoutingInforVisualService _inforVisualService;
    private readonly IRoutingRecipientService _recipientService;
    private readonly IRoutingUsageTrackingService _usageTrackingService;

    [ObservableProperty]
    private ObservableCollection<Model_RoutingLabel> _labels = new();

    [ObservableProperty]
    private ObservableCollection<Model_RoutingRecipient> _allRecipients = new();

    [ObservableProperty]
    private Model_RoutingLabel? _selectedLabel;

    [ObservableProperty]
    private int _labelCount;

    public RoutingManualEntryViewModel(
        IRoutingService routingService,
        IRoutingInforVisualService inforVisualService,
        IRoutingRecipientService recipientService,
        IRoutingUsageTrackingService usageTrackingService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
        : base(errorHandler, logger)
    {
        _routingService = routingService;
        _inforVisualService = inforVisualService;
        _recipientService = recipientService;
        _usageTrackingService = usageTrackingService;
    }

    /// <summary>
    /// Initialize - load recipients
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading recipients...";

            var recipientsResult = await _recipientService.GetAllRecipientsAsync();
            if (recipientsResult.IsSuccess)
            {
                AllRecipients = new ObservableCollection<Model_RoutingRecipient>(recipientsResult.Data);
                StatusMessage = $"{AllRecipients.Count} recipients loaded";
            }
            else
            {
                _errorHandler.ShowUserError(recipientsResult.ErrorMessage, "Load Failed", nameof(InitializeAsync));
            }

            // Add initial blank row
            AddNewRow();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Models.Enums.Enum_ErrorSeverity.Medium,
                callerName: nameof(InitializeAsync), controlName: nameof(RoutingManualEntryViewModel));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Add new blank row to collection
    /// </summary>
    [RelayCommand]
    private void AddNewRow()
    {
        var newLabel = new Model_RoutingLabel
        {
            CreatedDate = DateTime.Now,
            Quantity = 1
        };
        Labels.Add(newLabel);
        LabelCount = Labels.Count;
        StatusMessage = $"{LabelCount} labels ready to save";
    }

    /// <summary>
    /// Delete selected row
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteRow))]
    private void DeleteSelectedRow()
    {
        if (SelectedLabel != null)
        {
            Labels.Remove(SelectedLabel);
            LabelCount = Labels.Count;
            StatusMessage = $"{LabelCount} labels ready to save";
        }
    }

    private bool CanDeleteRow() => SelectedLabel != null && !IsBusy;

    /// <summary>
    /// Validate PO and populate Part info
    /// Called from View's CellEditEnding event
    /// </summary>
    public async Task ValidatePOAsync(Model_RoutingLabel label)
    {
        if (string.IsNullOrWhiteSpace(label.PONumber))
            return;

        try
        {
            var poResult = await _inforVisualService.ValidatePOAsync(label.PONumber);
            if (poResult.IsSuccess)
            {
                var linesResult = await _inforVisualService.GetPOLinesAsync(label.PONumber);
                if (linesResult.IsSuccess && linesResult.Data.Any())
                {
                    // If only one line, auto-populate
                    if (linesResult.Data.Count == 1)
                    {
                        var line = linesResult.Data.First();
                        label.PartID = line.PartID;
                        label.POLine = line.LineNumber;
                    }
                    // TODO: If multiple lines, user must select Line in DataGrid
                }
            }
            else
            {
                // PO not found - user can treat as OTHER
                StatusMessage = $"PO {label.PONumber} not found in Infor Visual";
            }
        }
        catch (Exception ex)
        {
            await _logger.LogErrorAsync($"Error validating PO: {ex.Message}", nameof(ValidatePOAsync));
        }
    }

    /// <summary>
    /// Save all labels in batch
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveAll))]
    private async Task SaveAllLabelsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Validating labels...";

            // Validate all rows
            var validationErrors = ValidateAllRows();
            if (validationErrors.Any())
            {
                _errorHandler.ShowUserError(string.Join("\n", validationErrors), "Validation Failed", nameof(SaveAllLabelsAsync));
                return;
            }

            StatusMessage = $"Creating {Labels.Count} labels...";
            int successCount = 0;

            foreach (var label in Labels.ToList())
            {
                // Create label
                var createResult = await _routingService.CreateLabelAsync(label);
                if (createResult.IsSuccess)
                {
                    successCount++;
                    
                    // Increment usage tracking
                    if (label.RecipientID.HasValue)
                    {
                        await _usageTrackingService.IncrementUsageAsync(
                            label.EmployeeNumber, 
                            label.RecipientID.Value);
                    }
                }
                else
                {
                    await _logger.LogErrorAsync($"Failed to create label: {createResult.ErrorMessage}");
                }
            }

            StatusMessage = $"Created {successCount} of {Labels.Count} labels. Exporting to CSV...";

            // Export all to CSV
            var exportResult = await _routingService.ExportLabelsToCSVAsync(Labels.ToList());
            if (!exportResult.IsSuccess)
            {
                await _logger.LogWarningAsync($"CSV export failed: {exportResult.ErrorMessage}");
            }

            // Clear collection
            Labels.Clear();
            LabelCount = 0;
            StatusMessage = $"Successfully created and exported {successCount} labels!";

            // Add new blank row for next batch
            AddNewRow();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Models.Enums.Enum_ErrorSeverity.High,
                callerName: nameof(SaveAllLabelsAsync), controlName: nameof(RoutingManualEntryViewModel));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSaveAll() => !IsBusy && Labels.Count > 0;

    private System.Collections.Generic.List<string> ValidateAllRows()
    {
        var errors = new System.Collections.Generic.List<string>();

        for (int i = 0; i < Labels.Count; i++)
        {
            var label = Labels[i];
            int rowNum = i + 1;

            if (string.IsNullOrWhiteSpace(label.PONumber) && !label.OtherReasonID.HasValue)
                errors.Add($"Row {rowNum}: PO Number or OTHER reason required");

            if (!label.RecipientID.HasValue)
                errors.Add($"Row {rowNum}: Recipient required");

            if (label.Quantity <= 0)
                errors.Add($"Row {rowNum}: Quantity must be greater than 0");
        }

        return errors;
    }

    partial void OnSelectedLabelChanged(Model_RoutingLabel? value)
    {
        DeleteSelectedRowCommand.NotifyCanExecuteChanged();
    }

    partial void OnLabelsChanged(ObservableCollection<Model_RoutingLabel> value)
    {
        SaveAllLabelsCommand.NotifyCanExecuteChanged();
    }
}

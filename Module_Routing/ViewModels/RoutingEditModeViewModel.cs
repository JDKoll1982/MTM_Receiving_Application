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
/// ViewModel for Edit Mode - searchable history with edit dialog and reprint
/// </summary>
public partial class RoutingEditModeViewModel : BaseViewModel
{
    private readonly IRoutingService _routingService;
    private readonly IRoutingRecipientService _recipientService;

    [ObservableProperty]
    private ObservableCollection<Model_RoutingLabel> _labels = new();

    [ObservableProperty]
    private ObservableCollection<Model_RoutingLabel> _filteredLabels = new();

    [ObservableProperty]
    private ObservableCollection<Model_RoutingRecipient> _allRecipients = new();

    [ObservableProperty]
    private Model_RoutingLabel? _selectedLabel;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public RoutingEditModeViewModel(
        IRoutingService routingService,
        IRoutingRecipientService recipientService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger)
        : base(errorHandler, logger)
    {
        _routingService = routingService;
        _recipientService = recipientService;
    }

    /// <summary>
    /// Initialize by loading labels and recipients
    /// </summary>
    public async Task InitializeAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Loading labels...";

            // Load all labels (limit 1000 for performance)
            var labelsResult = await _routingService.GetAllLabelsAsync(limit: 1000, offset: 0);
            if (labelsResult.IsSuccess)
            {
                Labels.Clear();
                foreach (var label in labelsResult.Data ?? new System.Collections.Generic.List<Model_RoutingLabel>())
                {
                    Labels.Add(label);
                }
            }

            // Load all recipients for edit dialog dropdown
            var recipientsResult = await _recipientService.GetAllActiveRecipientsAsync();
            if (recipientsResult.IsSuccess)
            {
                AllRecipients.Clear();
                foreach (var recipient in recipientsResult.Data ?? new System.Collections.Generic.List<Model_RoutingRecipient>())
                {
                    AllRecipients.Add(recipient);
                }
            }

            // Initialize filtered collection
            ApplySearchFilter();

            StatusMessage = $"Loaded {Labels.Count} labels";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Models.Enums.Enum_ErrorSeverity.Medium,
                callerName: nameof(InitializeAsync), controlName: nameof(RoutingEditModeViewModel));
            StatusMessage = "Error loading labels";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Apply search filter to labels collection
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        FilteredLabels.Clear();

        var searchLower = SearchText?.ToLower() ?? string.Empty;

        var filtered = string.IsNullOrWhiteSpace(searchLower)
            ? Labels
            : Labels.Where(l =>
                (l.PONumber?.ToLower().Contains(searchLower) ?? false) ||
                (l.RecipientName?.ToLower().Contains(searchLower) ?? false) ||
                (l.PartID?.ToLower().Contains(searchLower) ?? false)
            );

        foreach (var label in filtered)
        {
            FilteredLabels.Add(label);
        }

        StatusMessage = $"Showing {FilteredLabels.Count} of {Labels.Count} labels";
    }

    /// <summary>
    /// Edit selected label - to be called from dialog
    /// </summary>
    [RelayCommand]
    private async Task SaveEditedLabelAsync(Model_RoutingLabel editedLabel)
    {
        if (IsBusy || SelectedLabel == null) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Saving changes...";

            // Compare and log changes
            await CompareAndLogChangesAsync(SelectedLabel, editedLabel);

            // Update label
            var updateResult = await _routingService.UpdateLabelAsync(editedLabel);

            if (updateResult.IsSuccess)
            {
                // Update in collection
                var index = Labels.IndexOf(SelectedLabel);
                if (index >= 0)
                {
                    Labels[index] = editedLabel;
                }

                ApplySearchFilter();
                StatusMessage = "Label updated successfully";
            }
            else
            {
                _errorHandler.ShowUserError(updateResult.ErrorMessage, "Update Failed", nameof(SaveEditedLabelAsync));
                StatusMessage = "Failed to update label";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Models.Enums.Enum_ErrorSeverity.Medium,
                callerName: nameof(SaveEditedLabelAsync), controlName: nameof(RoutingEditModeViewModel));
            StatusMessage = "Error saving changes";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Compare old and new label, log changes to audit trail
    /// </summary>
    private async Task CompareAndLogChangesAsync(Model_RoutingLabel oldLabel, Model_RoutingLabel newLabel)
    {
        // Check RecipientID change
        if (oldLabel.RecipientID != newLabel.RecipientID)
        {
            var history = new Model_RoutingLabelHistory
            {
                LabelID = oldLabel.Id,
                FieldName = "RecipientID",
                OldValue = oldLabel.RecipientID.ToString(),
                NewValue = newLabel.RecipientID.ToString(),
                ChangedBy = oldLabel.EmployeeNumber, // TODO: Get current user from session
                ChangedDate = DateTime.Now
            };

            await _routingService.LogLabelHistoryAsync(history);
        }

        // Check Quantity change
        if (oldLabel.Quantity != newLabel.Quantity)
        {
            var history = new Model_RoutingLabelHistory
            {
                LabelID = oldLabel.Id,
                FieldName = "Quantity",
                OldValue = oldLabel.Quantity.ToString(),
                NewValue = newLabel.Quantity.ToString(),
                ChangedBy = oldLabel.EmployeeNumber,
                ChangedDate = DateTime.Now
            };

            await _routingService.LogLabelHistoryAsync(history);
        }

        // Check OtherReasonID change
        if (oldLabel.OtherReasonID != newLabel.OtherReasonID)
        {
            var history = new Model_RoutingLabelHistory
            {
                LabelID = oldLabel.Id,
                FieldName = "OtherReasonID",
                OldValue = oldLabel.OtherReasonID?.ToString() ?? "NULL",
                NewValue = newLabel.OtherReasonID?.ToString() ?? "NULL",
                ChangedBy = oldLabel.EmployeeNumber,
                ChangedDate = DateTime.Now
            };

            await _routingService.LogLabelHistoryAsync(history);
        }
    }

    /// <summary>
    /// Reprint selected label (CSV export only, no DB update)
    /// </summary>
    [RelayCommand]
    private async Task ReprintLabelAsync()
    {
        if (IsBusy || SelectedLabel == null) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Reprinting label...";

            // Export single label to CSV
            var exportResult = await _routingService.ExportLabelToCSVAsync(SelectedLabel);

            if (exportResult.IsSuccess)
            {
                StatusMessage = "Label reprinted successfully";
            }
            else
            {
                _errorHandler.ShowUserError(exportResult.ErrorMessage, "Reprint Failed", nameof(ReprintLabelAsync));
                StatusMessage = "Failed to reprint label";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Models.Enums.Enum_ErrorSeverity.Low,
                callerName: nameof(ReprintLabelAsync), controlName: nameof(RoutingEditModeViewModel));
            StatusMessage = "Error reprinting label";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Routing.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// ViewModel for Edit Mode - searchable history with edit dialog and reprint
/// </summary>
public partial class RoutingEditModeViewModel : ViewModel_Shared_Base
{
    private readonly IRoutingService _routingService;
    private readonly IRoutingRecipientService _recipientService;
    private readonly IService_UserSessionManager _sessionManager;

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
        IService_LoggingUtility logger,
        IService_UserSessionManager sessionManager,
        IService_Notification notificationService)
        : base(errorHandler, logger, notificationService)
    {
        _routingService = routingService;
        _recipientService = recipientService;
        _sessionManager = sessionManager;
    }

    /// <summary>
    /// Initialize by loading labels and recipients
    /// </summary>
    public async Task InitializeAsync()
    {
        if (IsBusy)
        {
            return;
        }

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
            var recipientsResult = await _recipientService.GetActiveRecipientsSortedByUsageAsync(0);
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
            _errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(InitializeAsync), nameof(RoutingEditModeViewModel));
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
    /// <param name="value"></param>
    partial void OnSearchTextChanged(string value)
    {
        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        FilteredLabels.Clear();

        var searchLower = SearchText?.ToLower() ?? string.Empty;

        // Issue #19: Optimized collection filtering - using ToList() to materialize before loop
        var filtered = string.IsNullOrWhiteSpace(searchLower)
            ? Labels.ToList()
            : Labels.Where(l =>
                (l.PONumber?.ToLower().Contains(searchLower) ?? false) ||
                (l.RecipientName?.ToLower().Contains(searchLower) ?? false) ||
                (l.Description?.ToLower().Contains(searchLower) ?? false)
            ).ToList();

        foreach (var label in filtered)
        {
            FilteredLabels.Add(label);
        }

        StatusMessage = $"Showing {FilteredLabels.Count} of {Labels.Count} labels";
    }

    /// <summary>
    /// Edit selected label - to be called from dialog
    /// </summary>
    /// <param name="editedLabel"></param>
    [RelayCommand]
    private async Task SaveEditedLabelAsync(Model_RoutingLabel editedLabel)
    {
        if (IsBusy || SelectedLabel == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Saving changes...";

            // Compare and log changes
            await CompareAndLogChangesAsync(SelectedLabel, editedLabel);

            // Issue #13: Update label - Get current employee number from session
            // Issue #7: Implemented using IService_UserSessionManager
            var currentEmployeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber ?? 0;
            var updateResult = await _routingService.UpdateLabelAsync(editedLabel, currentEmployeeNumber);

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
                await _errorHandler.ShowUserErrorAsync(updateResult.ErrorMessage, "Update Failed", nameof(SaveEditedLabelAsync));
                StatusMessage = "Failed to update label";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(SaveEditedLabelAsync), nameof(RoutingEditModeViewModel));
            StatusMessage = "Error saving changes";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Compare old and new label - history logging handled by RoutingService.UpdateLabelAsync
    /// </summary>
    /// <param name="oldLabel"></param>
    /// <param name="newLabel"></param>
    private async Task CompareAndLogChangesAsync(Model_RoutingLabel oldLabel, Model_RoutingLabel newLabel)
    {
        _ = oldLabel;
        _ = newLabel;
        // History logging is now handled automatically by RoutingService.UpdateLabelAsync
        // This method is kept for potential future custom validation before save
        await Task.CompletedTask;
    }

    /// <summary>
    /// Reprint selected label (CSV export only, no DB update)
    /// </summary>
    [RelayCommand]
    private async Task ReprintLabelAsync()
    {
        if (IsBusy || SelectedLabel == null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Reprinting label...";

            // Export single label to CSV
            var exportResult = await _routingService.ExportLabelToCsvAsync(SelectedLabel);

            if (exportResult.IsSuccess)
            {
                StatusMessage = "Label reprinted successfully";
            }
            else
            {
                await _errorHandler.ShowUserErrorAsync(exportResult.ErrorMessage, "Reprint Failed", nameof(ReprintLabelAsync));
                StatusMessage = "Failed to reprint label";
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(ex, Module_Core.Models.Enums.Enum_ErrorSeverity.Low,
                nameof(ReprintLabelAsync), nameof(RoutingEditModeViewModel));
            StatusMessage = "Error reprinting label";
        }
        finally
        {
            IsBusy = false;
        }
    }
}


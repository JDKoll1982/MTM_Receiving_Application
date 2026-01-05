using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// ViewModel for routing label entry with data grid, duplicate row, and auto-fill functionality.
/// </summary>
public partial class ViewModel_Routing_LabelEntry : ViewModel_Shared_Base
{
    private readonly IService_Routing _routingService;
    private readonly IService_Routing_RecipientLookup _recipientLookup;
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private ObservableCollection<Model_Routing_Label> _labels = new();

    [ObservableProperty]
    private Model_Routing_Label? _selectedLabel;

    [ObservableProperty]
    private ObservableCollection<Model_Routing_Recipient> _recipients = new();

    [ObservableProperty]
    private int _nextLabelNumber = 1;

    public ViewModel_Routing_LabelEntry(
        IService_Routing routingService,
        IService_Routing_RecipientLookup recipientLookup,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
        _recipientLookup = recipientLookup ?? throw new ArgumentNullException(nameof(recipientLookup));
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
    }

    /// <summary>
    /// Initializes the ViewModel - loads today's labels and recipient list.
    /// </summary>
    public async Task InitializeAsync()
    {
        await LoadTodayLabelsAsync();
        await LoadRecipientsAsync();
        await UpdateNextLabelNumberAsync();
    }

    [RelayCommand]
    private async Task LoadTodayLabelsAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            StatusMessage = "Loading today's labels...";
            _logger.LogInfo("Loading today's routing labels");

            var result = await _routingService.GetTodayLabelsAsync();
            if (result.IsSuccess)
            {
                Labels.Clear();
                foreach (var label in result.Data.OrderBy(l => l.LabelNumber))
                {
                    Labels.Add(label);
                }
                StatusMessage = $"Loaded {Labels.Count} labels";
                _logger.LogInfo($"Loaded {Labels.Count} labels for today");
            }
            else
            {
                await _errorHandler.HandleErrorAsync(result.ErrorMessage, Enum_ErrorSeverity.Error, null, true);
                StatusMessage = "Error loading labels";
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Error loading labels", Enum_ErrorSeverity.Error, ex, true);
            StatusMessage = "Error loading labels";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadRecipientsAsync()
    {
        try
        {
            _logger.LogInfo("Loading recipient list");
            var result = await _recipientLookup.GetAllRecipientsAsync();
            if (result.IsSuccess)
            {
                Recipients.Clear();
                foreach (var recipient in result.Data.OrderBy(r => r.Name))
                {
                    Recipients.Add(recipient);
                }
                _logger.LogInfo($"Loaded {Recipients.Count} recipients");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading recipients: {ex.Message}", ex);
        }
    }

    [RelayCommand]
    private async Task AddNewLabelAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            StatusMessage = "Adding new label...";

            var employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber.ToString() ?? "0";

            var newLabel = new Model_Routing_Label
            {
                LabelNumber = NextLabelNumber,
                DeliverTo = string.Empty,
                Department = string.Empty,
                EmployeeNumber = employeeNumber,
                CreatedDate = DateTime.Today
            };

            var result = await _routingService.AddLabelAsync(newLabel);
            if (result.IsSuccess)
            {
                newLabel.Id = result.Data;
                Labels.Add(newLabel);
                SelectedLabel = newLabel;
                await UpdateNextLabelNumberAsync();
                StatusMessage = $"Label #{newLabel.LabelNumber} added";
                _logger.LogInfo($"Added label #{newLabel.LabelNumber}");
            }
            else
            {
                await _errorHandler.HandleErrorAsync(result.ErrorMessage, Enum_ErrorSeverity.Error, null, true);
                StatusMessage = "Error adding label";
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Error adding label", Enum_ErrorSeverity.Error, ex, true);
            StatusMessage = "Error adding label";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DuplicateSelectedLabelAsync()
    {
        if (SelectedLabel == null || IsBusy)
            return;

        try
        {
            IsBusy = true;
            StatusMessage = "Duplicating label...";

            var result = await _routingService.DuplicateLabelAsync(SelectedLabel.Id);
            if (result.IsSuccess)
            {
                await LoadTodayLabelsAsync();
                StatusMessage = $"Label duplicated with new number #{result.Data}";
                _logger.LogInfo($"Duplicated label ID {SelectedLabel.Id} as #{result.Data}");
            }
            else
            {
                await _errorHandler.HandleErrorAsync(result.ErrorMessage, Enum_ErrorSeverity.Error, null, true);
                StatusMessage = "Error duplicating label";
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Error duplicating label", Enum_ErrorSeverity.Error, ex, true);
            StatusMessage = "Error duplicating label";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedLabelAsync()
    {
        if (SelectedLabel == null || IsBusy)
            return;

        try
        {
            IsBusy = true;
            StatusMessage = "Deleting label...";

            var result = await _routingService.DeleteLabelAsync(SelectedLabel.Id);
            if (result.IsSuccess)
            {
                Labels.Remove(SelectedLabel);
                SelectedLabel = null;
                StatusMessage = "Label deleted";
                _logger.LogInfo("Label deleted successfully");
            }
            else
            {
                await _errorHandler.HandleErrorAsync(result.ErrorMessage, Enum_ErrorSeverity.Error, null, true);
                StatusMessage = "Error deleting label";
            }
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync("Error deleting label", Enum_ErrorSeverity.Error, ex, true);
            StatusMessage = "Error deleting label";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Auto-fills department when a recipient is selected.
    /// </summary>
    public async Task OnRecipientSelectedAsync(string recipientName)
    {
        if (string.IsNullOrWhiteSpace(recipientName))
            return;

        try
        {
            var result = await _recipientLookup.GetDefaultDepartmentAsync(recipientName);
            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data) && SelectedLabel != null)
            {
                SelectedLabel.Department = result.Data;
                _logger.LogInfo($"Auto-filled department: {result.Data}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error auto-filling department: {ex.Message}", ex);
        }
    }

    private async Task UpdateNextLabelNumberAsync()
    {
        try
        {
            var result = await _routingService.GetNextLabelNumberAsync();
            if (result.IsSuccess)
            {
                NextLabelNumber = result.Data;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating next label number: {ex.Message}", ex);
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Module_Routing.Models;
using MTM_Receiving_Application.Module_Routing.Services;
using MTM_Receiving_Application.Models.Enums;

namespace MTM_Receiving_Application.Module_Routing.ViewModels;

/// <summary>
/// ViewModel for Wizard Step 2: Recipient Selection with Quick Add
/// </summary>
public partial class RoutingWizardStep2ViewModel : ObservableObject
{
    private readonly IRoutingRecipientService _recipientService;
    private readonly IService_ErrorHandler _errorHandler;
    private readonly ILoggingService _logger;
    private readonly RoutingWizardContainerViewModel _containerViewModel;

    // Full list for filtering
    private ObservableCollection<Model_RoutingRecipient> _allRecipients = new();

    #region Constructor
    public RoutingWizardStep2ViewModel(
        IRoutingRecipientService recipientService,
        IService_ErrorHandler errorHandler,
        ILoggingService logger,
        RoutingWizardContainerViewModel containerViewModel)
    {
        _recipientService = recipientService;
        _errorHandler = errorHandler;
        _logger = logger;
        _containerViewModel = containerViewModel;
    }
    #endregion

    #region Observable Properties
    /// <summary>
    /// Quick Add buttons (top 5 most-used recipients)
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Model_RoutingRecipient> _quickAddRecipients = new();

    /// <summary>
    /// Filtered recipient list for DataGrid
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<Model_RoutingRecipient> _filteredRecipients = new();

    /// <summary>
    /// Selected recipient from DataGrid
    /// </summary>
    [ObservableProperty]
    private Model_RoutingRecipient? _selectedRecipient;

    /// <summary>
    /// Search text for real-time filtering
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Busy indicator
    /// </summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Status message
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Select a recipient";
    #endregion

    #region Initialization
    /// <summary>
    /// Load recipients and populate Quick Add buttons
    /// </summary>
    [RelayCommand]
    public async Task LoadRecipientsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Loading recipients...";

            // Load Quick Add recipients (top 5 for current employee)
            var quickAddResult = await _recipientService.GetQuickAddRecipientsAsync(
                GetCurrentEmployeeNumber(),
                count: 5);

            if (quickAddResult.IsSuccess && quickAddResult.Data != null)
            {
                QuickAddRecipients.Clear();
                foreach (var recipient in quickAddResult.Data)
                {
                    QuickAddRecipients.Add(recipient);
                }
            }

            // Load all active recipients for searchable list
            var allRecipientsResult = await _recipientService.GetAllActiveRecipientsAsync();

            if (allRecipientsResult.IsSuccess && allRecipientsResult.Data != null)
            {
                _allRecipients.Clear();
                foreach (var recipient in allRecipientsResult.Data)
                {
                    _allRecipients.Add(recipient);
                }

                // Initially show all
                ApplyFilter();
                StatusMessage = $"Loaded {_allRecipients.Count} recipients ({QuickAddRecipients.Count} Quick Add)";
            }
            else
            {
                _errorHandler.ShowUserError(
                    "Failed to load recipients",
                    "Data Load Error",
                    nameof(LoadRecipientsAsync));
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadRecipientsAsync),
                nameof(RoutingWizardStep2ViewModel));
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region Quick Add Commands
    /// <summary>
    /// Quick Add button clicked - auto-select recipient and navigate to Step 3
    /// </summary>
    [RelayCommand]
    private void QuickAddRecipient(Model_RoutingRecipient recipient)
    {
        if (recipient == null) return;

        SelectedRecipient = recipient;
        StatusMessage = $"Quick Add selected: {recipient.Name}";

        // Immediately proceed to Step 3
        ProceedToStep3();
    }
    #endregion

    #region Search and Filter
    /// <summary>
    /// Apply real-time search filter to recipient list
    /// </summary>
    private void ApplyFilter()
    {
        FilteredRecipients.Clear();

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // Show all when no search text
            foreach (var recipient in _allRecipients)
            {
                FilteredRecipients.Add(recipient);
            }
        }
        else
        {
            // Filter by name, location, or department (case-insensitive)
            var searchLower = SearchText.ToLower();
            var filtered = _allRecipients.Where(r =>
                r.Name.ToLower().Contains(searchLower) ||
                (r.Location?.ToLower().Contains(searchLower) ?? false) ||
                (r.Department?.ToLower().Contains(searchLower) ?? false));

            foreach (var recipient in filtered)
            {
                FilteredRecipients.Add(recipient);
            }
        }

        StatusMessage = $"Showing {FilteredRecipients.Count} of {_allRecipients.Count} recipients";
    }

    /// <summary>
    /// Trigger filter when search text changes
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }
    #endregion

    #region Navigation Commands
    /// <summary>
    /// Proceed to Step 3 (Review)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanProceedToStep3))]
    private void ProceedToStep3()
    {
        if (SelectedRecipient == null) return;

        // Update container with selected recipient
        _containerViewModel.SelectedRecipient = SelectedRecipient;

        // Navigate to Step 3
        _containerViewModel.NavigateToStep3Command.Execute(null);
    }

    /// <summary>
    /// Can proceed if recipient is selected
    /// </summary>
    private bool CanProceedToStep3()
    {
        return SelectedRecipient != null;
    }

    /// <summary>
    /// Go back to Step 1
    /// </summary>
    [RelayCommand]
    private void NavigateBackToStep1()
    {
        _containerViewModel.NavigateToStep1Command.Execute(null);
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Get current employee number from session
    /// TODO: Replace with actual session service
    /// </summary>
    private int GetCurrentEmployeeNumber()
    {
        // Placeholder - replace with ISessionService
        return 6229; // Default employee number
    }

    /// <summary>
    /// Notify command can execute changed when selection changes
    /// </summary>
    partial void OnSelectedRecipientChanged(Model_RoutingRecipient? value)
    {
        ProceedToStep3Command.NotifyCanExecuteChanged();
    }
    #endregion
}

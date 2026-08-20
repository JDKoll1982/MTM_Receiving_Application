using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Dunnage.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for the image-backed Dunnage part search dialog.
/// </summary>
public partial class ViewModel_Dunnage_ImagePartSearchDialog : ViewModel_Shared_Base
{
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_DunnageSettings _dunnageSettings;
    private readonly IService_UserSessionManager _sessionManager;
    private List<Model_DunnagePart> _allParts = new();
    private bool _isRestoringShowPartsPreference;
    private bool _persistedShowPartsWithoutImages;

    public ViewModel_Dunnage_ImagePartSearchDialog(
        IService_MySQL_Dunnage dunnageService,
        IService_DunnageWorkflow workflowService,
        IService_DunnageSettings dunnageSettings,
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _dunnageService = dunnageService;
        _workflowService = workflowService;
        _dunnageSettings = dunnageSettings;
        _sessionManager = sessionManager;
        _workflowService.StepChanged += OnWorkflowStepChanged;

        _ = LoadShowPartsPreferenceAsync();

        if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.ImagePartSearch)
        {
            RefreshForCurrentEntry();
        }
    }

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_DunnagePart> _displayedParts = new();

    [ObservableProperty]
    private string _emptyStateMessage = "Loading parts with images...";

    [ObservableProperty]
    private bool _showPartsWithoutImages;

    public bool HasNoResults => DisplayedParts.Count == 0;

    public string Heading => "Search Dunnage Parts by Image";

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        if (_workflowService.CurrentStep != Enum_DunnageWorkflowStep.ImagePartSearch)
        {
            return;
        }

        RefreshForCurrentEntry();
    }

    private void RefreshForCurrentEntry()
    {
        FilterText = string.Empty;
        _ = LoadPartsCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadPartsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            EmptyStateMessage = ShowPartsWithoutImages
                ? "Loading Dunnage parts..."
                : "Loading parts with images...";

            var result = await _dunnageService.GetAllPartsAsync();
            if (!result.IsSuccess || result.Data is null)
            {
                DisplayedParts = new ObservableCollection<Model_DunnagePart>();
                EmptyStateMessage = result.ErrorMessage ?? "Failed to load Dunnage parts.";
                OnPropertyChanged(nameof(HasNoResults));
                return;
            }

            _allParts = result.Data.OrderBy(static part => part.PartId).ToList();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadPartsAsync),
                nameof(ViewModel_Dunnage_ImagePartSearchDialog)
            );
            DisplayedParts = new ObservableCollection<Model_DunnagePart>();
            EmptyStateMessage = "Failed to load Dunnage parts.";
            OnPropertyChanged(nameof(HasNoResults));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectPartAsync(Model_DunnagePart? part)
    {
        if (part is null || IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            var session = _workflowService.CurrentSession;
            session.SelectedPart = part;
            session.SelectedTypeId = part.TypeId;
            session.SelectedTypeName = part.DunnageTypeName;
            session.IsPartSelectionFromImageSearch = true;

            var typeResult = await _dunnageService.GetTypeByIdAsync(part.TypeId);
            if (typeResult.IsSuccess && typeResult.Data is not null)
            {
                session.SelectedType = typeResult.Data;
            }
            else
            {
                session.SelectedType = new Model_DunnageType
                {
                    Id = part.TypeId,
                    TypeName = part.DunnageTypeName,
                    ImagePath = part.DunnageTypeImagePath,
                };
            }

            _logger.LogInfo(
                $"ImagePartSearch: Selected part {part.PartId} and navigating to PartSelection",
                "Dunnage"
            );

            _workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SelectPartAsync),
                nameof(ViewModel_Dunnage_ImagePartSearchDialog)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnFilterTextChanged(string value)
    {
        ApplyFilter();
    }

    public async Task HandleShowPartsWithoutImagesChangedAsync(bool isChecked)
    {
        if (_isRestoringShowPartsPreference)
        {
            return;
        }

        if (ShowPartsWithoutImages != isChecked)
        {
            _isRestoringShowPartsPreference = true;
            ShowPartsWithoutImages = isChecked;
            _isRestoringShowPartsPreference = false;
            ApplyFilter();
        }

        if (isChecked == _persistedShowPartsWithoutImages)
        {
            return;
        }

        await PersistShowPartsPreferenceAsync(isChecked);
    }

    private async Task LoadShowPartsPreferenceAsync()
    {
        _isRestoringShowPartsPreference = true;

        try
        {
            var savedPreference = await _dunnageSettings.GetBoolAsync(
                DunnageSettingsKeys.UserPreferences.ShowPartsWithoutImages,
                GetCurrentUserId()
            );

            _persistedShowPartsWithoutImages = savedPreference;
            ShowPartsWithoutImages = savedPreference;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"ImagePartSearch: Failed to load 'show parts without images' preference. Falling back to images only. Error: {ex.Message}",
                "ImagePartSearch"
            );

            _persistedShowPartsWithoutImages = false;
            ShowPartsWithoutImages = false;
        }
        finally
        {
            _isRestoringShowPartsPreference = false;
            ApplyFilter();
        }
    }

    private async Task PersistShowPartsPreferenceAsync(bool isChecked)
    {
        try
        {
            await _dunnageSettings.SaveStringAsync(
                DunnageSettingsKeys.UserPreferences.ShowPartsWithoutImages,
                isChecked ? "true" : "false",
                GetCurrentUserId()
            );

            _persistedShowPartsWithoutImages = isChecked;

            _logger.LogInfo(
                $"ImagePartSearch: 'Show parts without images' saved as {isChecked}",
                "ImagePartSearch"
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                $"ImagePartSearch: Failed to save 'show parts without images' preference. Error: {ex.Message}",
                "ImagePartSearch"
            );

            _isRestoringShowPartsPreference = true;
            ShowPartsWithoutImages = _persistedShowPartsWithoutImages;
            _isRestoringShowPartsPreference = false;
            ApplyFilter();
        }
    }

    private int? GetCurrentUserId()
    {
        int? employeeNumber = _sessionManager.CurrentSession?.User?.EmployeeNumber;
        return employeeNumber.HasValue && employeeNumber.Value > 0 ? employeeNumber : null;
    }

    private void ApplyFilter()
    {
        IEnumerable<Model_DunnagePart> filteredParts = _allParts;

        if (ShowPartsWithoutImages is false)
        {
            filteredParts = filteredParts.Where(static part =>
                string.IsNullOrWhiteSpace(part.ImagePath) is false
            );
        }

        if (string.IsNullOrWhiteSpace(FilterText) is false)
        {
            filteredParts = filteredParts.Where(part =>
                part.PartId.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || part.DunnageTypeName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || part.HomeLocation.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
            );
        }

        var filteredList = filteredParts.ToList();
        DisplayedParts = new ObservableCollection<Model_DunnagePart>(filteredList);
        EmptyStateMessage = BuildEmptyStateMessage();
        OnPropertyChanged(nameof(HasNoResults));
    }

    private string BuildEmptyStateMessage()
    {
        if (_allParts.Count == 0)
        {
            return ShowPartsWithoutImages
                ? "No Dunnage parts found."
                : "No Dunnage parts currently have image paths configured.";
        }

        return "No Dunnage parts matched the current filter.";
    }
}

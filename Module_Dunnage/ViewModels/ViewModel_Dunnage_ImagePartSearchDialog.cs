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
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for the image-backed Dunnage part search dialog.
/// </summary>
public partial class ViewModel_Dunnage_ImagePartSearchDialog : ViewModel_Shared_Base
{
    private readonly IService_MySQL_Dunnage _dunnageService;
    private readonly IService_DunnageWorkflow _workflowService;
    private List<Model_DunnagePart> _allImageParts = new();

    public ViewModel_Dunnage_ImagePartSearchDialog(
        IService_MySQL_Dunnage dunnageService,
        IService_DunnageWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _dunnageService = dunnageService;
        _workflowService = workflowService;
        _workflowService.StepChanged += OnWorkflowStepChanged;

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
            EmptyStateMessage = "Loading parts with images...";

            var result = await _dunnageService.GetAllPartsAsync();
            if (!result.IsSuccess || result.Data is null)
            {
                DisplayedParts = new ObservableCollection<Model_DunnagePart>();
                EmptyStateMessage = result.ErrorMessage ?? "Failed to load Dunnage parts.";
                OnPropertyChanged(nameof(HasNoResults));
                return;
            }

            _allImageParts = result
                .Data.Where(static part => string.IsNullOrWhiteSpace(part.ImagePath) is false)
                .OrderBy(static part => part.PartId)
                .ToList();

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

    private void ApplyFilter()
    {
        IEnumerable<Model_DunnagePart> filteredParts = _allImageParts;
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
        EmptyStateMessage =
            _allImageParts.Count == 0
                ? "No Dunnage parts currently have image paths configured."
                : "No Dunnage parts matched the current filter.";
        OnPropertyChanged(nameof(HasNoResults));
    }
}

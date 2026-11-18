using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Dunnage.Enums;
using MTM_Receiving_Application.Module_Dunnage.Helpers;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Dunnage.ViewModels;

/// <summary>
/// ViewModel for Dunnage Quantity Entry.
/// </summary>
public partial class ViewModel_Dunnage_QuantityEntry : ViewModel_Shared_Base, IResettableViewModel
{
    private readonly IService_DunnageWorkflow _workflowService;
    private readonly IService_Dispatcher _dispatcher;
    private readonly IService_Help _helpService;
    private readonly IService_ViewModelRegistry _viewModelRegistry;
    private const int ThumbnailDecodeWidth = 192;
    private bool _isSynchronizingLoads;

    public ViewModel_Dunnage_QuantityEntry(
        IService_DunnageWorkflow workflowService,
        IService_Dispatcher dispatcher,
        IService_Help helpService,
        IService_ViewModelRegistry viewModelRegistry,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _workflowService = workflowService;
        _dispatcher = dispatcher;
        _helpService = helpService;
        _viewModelRegistry = viewModelRegistry;

        _workflowService.StepChanged += OnWorkflowStepChanged;
        _viewModelRegistry.Register(this);
    }

    public void ResetToDefaults()
    {
        NumberOfLoads = 1;
        Quantity = 1;
        ValidationMessage = string.Empty;
        ReplaceLoads(Array.Empty<Model_DunnageLoad>());
        StatusMessage = string.Empty;
    }

    private void OnWorkflowStepChanged(object? sender, EventArgs e)
    {
        if (_workflowService.CurrentStep == Enum_DunnageWorkflowStep.QuantityEntry)
        {
            _dispatcher.TryEnqueue(LoadContextData);
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private int _numberOfLoads = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private int _quantity = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private ObservableCollection<Model_DunnageLoad> _loads = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private string _currentTypeName = string.Empty;

    [ObservableProperty]
    private string _currentPartId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentHomeLocationDisplay))]
    private string _currentHomeLocation = string.Empty;

    [ObservableProperty]
    private string _currentQuantityType = "Quantity";

    [ObservableProperty]
    private int _currentLoadCount;

    [ObservableProperty]
    private string _currentTotal = "0";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCurrentVisual))]
    private ImageSource? _currentVisualSource;

    /// <summary>
    /// Gets a value indicating whether the part or type carries an image to show in the summary card.
    /// </summary>
    public bool HasCurrentVisual => CurrentVisualSource is not null;

    /// <summary>
    /// Gets the absolute path of the summary image, used when the user opens the full image.
    /// </summary>
    public string? CurrentVisualAbsolutePath { get; private set; }

    /// <summary>
    /// Gets the saved home location, or a plain fallback when the part has none.
    /// </summary>
    public string CurrentHomeLocationDisplay =>
        string.IsNullOrWhiteSpace(CurrentHomeLocation) ? "Not set" : CurrentHomeLocation;

    public bool IsValid =>
        NumberOfLoads > 0 && Loads.Count > 0 && Loads.All(load => load.Quantity > 0);

    public void LoadContextData()
    {
        try
        {
            NumberOfLoads = _workflowService.NumberOfLoads;
            RebuildLoadEditors();
            RefreshCurrentPartSummary();

            var selectedTypeName = _workflowService.CurrentSession.SelectedTypeName ?? string.Empty;
            var selectedPartName =
                _workflowService.CurrentSession.SelectedPart?.PartId ?? string.Empty;

            _logger.LogInfo(
                $"Loaded context: Type={selectedTypeName}, Part={selectedPartName}, LoadCount={NumberOfLoads}",
                "QuantityEntry"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error loading context data: {ex.Message}", ex, "QuantityEntry");
        }
    }

    /// <summary>
    /// Refreshes the summary card with the type, part, location, quantity type, and visual of the
    /// dunnage currently being processed.
    /// </summary>
    private void RefreshCurrentPartSummary()
    {
        var session = _workflowService.CurrentSession;
        var selectedPart = session.SelectedPart;

        CurrentTypeName = session.SelectedTypeName ?? string.Empty;
        CurrentPartId = selectedPart?.PartId ?? string.Empty;
        CurrentHomeLocation = selectedPart?.HomeLocation ?? string.Empty;
        CurrentQuantityType = string.IsNullOrWhiteSpace(selectedPart?.QuantityType)
            ? "Quantity"
            : selectedPart!.QuantityType;

        var visualPath =
            string.IsNullOrWhiteSpace(selectedPart?.ImagePath)
                ? session.SelectedType?.ImagePath
                : selectedPart.ImagePath;

        CurrentVisualSource = Helper_DunnageImagePaths.CreateImageSource(
            visualPath,
            ThumbnailDecodeWidth
        );
        CurrentVisualAbsolutePath = Helper_DunnageImagePaths.GetDisplayAbsolutePath(visualPath);
    }

    /// <summary>
    /// Recalculates the load count and the running total of every Quantity Per Load value, the
    /// same way the receiving weight/quantity step keeps its running total.
    /// </summary>
    private void UpdateLoadTotals()
    {
        CurrentLoadCount = Loads.Count;
        CurrentTotal = Loads.Sum(load => load.Quantity).ToString("N0", CultureInfo.CurrentCulture);
    }

    partial void OnQuantityChanged(int value)
    {
        if (_isSynchronizingLoads)
        {
            return;
        }

        if (Loads.Count > 0)
        {
            Loads[0].Quantity = value;
        }

        ValidateQuantity();
        GoNextCommand.NotifyCanExecuteChanged();
    }

    partial void OnNumberOfLoadsChanged(int value)
    {
        if (_isSynchronizingLoads)
        {
            return;
        }

        _workflowService.NumberOfLoads = value;
        RebuildLoadEditors();
        ValidateQuantity();
        GoNextCommand.NotifyCanExecuteChanged();
    }

    private void RebuildLoadEditors()
    {
        _isSynchronizingLoads = true;
        try
        {
            EnsureSessionLoadQuantities();

            var selectedTypeName = _workflowService.CurrentSession.SelectedTypeName ?? string.Empty;
            var selectedTypeIcon = _workflowService.CurrentSession.SelectedType?.Icon ?? "Help";
            var selectedPartName =
                _workflowService.CurrentSession.SelectedPart?.PartId ?? string.Empty;
            var selectedTypeImagePath = _workflowService.CurrentSession.SelectedType?.ImagePath;
            var selectedPartImagePath = _workflowService.CurrentSession.SelectedPart?.ImagePath;
            var selectedQuantityType =
                _workflowService.CurrentSession.SelectedPart?.QuantityType ?? "Quantity";

            var rebuiltLoads = new List<Model_DunnageLoad>(NumberOfLoads);
            for (var index = 0; index < NumberOfLoads; index++)
            {
                var load = new Model_DunnageLoad
                {
                    LoadNumber = index + 1,
                    Quantity = _workflowService.CurrentSession.LoadQuantities[index],
                    PartId = selectedPartName,
                    TypeName = selectedTypeName,
                    TypeIcon = selectedTypeIcon,
                    TypeImagePath = selectedTypeImagePath,
                    PartImagePath = selectedPartImagePath,
                    QuantityType = selectedQuantityType,
                };
                rebuiltLoads.Add(load);
            }

            ReplaceLoads(rebuiltLoads);

            Quantity = Loads.FirstOrDefault() is { } firstLoad
                ? decimal.ToInt32(firstLoad.Quantity)
                : 1;
        }
        finally
        {
            _isSynchronizingLoads = false;
        }
    }

    private void EnsureSessionLoadQuantities()
    {
        if (
            _workflowService.CurrentSession.LoadQuantities.Count == 0
            && _workflowService.CurrentSession.Quantity > 0
        )
        {
            _workflowService.CurrentSession.LoadQuantities.Add(
                _workflowService.CurrentSession.Quantity
            );
        }

        while (_workflowService.CurrentSession.LoadQuantities.Count < NumberOfLoads)
        {
            var defaultQuantity =
                _workflowService.CurrentSession.LoadQuantities.Count == 0
                    ? Math.Max(1, Quantity)
                    : _workflowService.CurrentSession.LoadQuantities[^1];
            _workflowService.CurrentSession.LoadQuantities.Add(defaultQuantity);
        }

        while (_workflowService.CurrentSession.LoadQuantities.Count > NumberOfLoads)
        {
            _workflowService.CurrentSession.LoadQuantities.RemoveAt(
                _workflowService.CurrentSession.LoadQuantities.Count - 1
            );
        }

        _workflowService.CurrentSession.NumberOfLoads = NumberOfLoads;
        _workflowService.CurrentSession.Quantity =
            _workflowService.CurrentSession.LoadQuantities.FirstOrDefault();
    }

    private void Load_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (
            sender is not Model_DunnageLoad load
            || e.PropertyName != nameof(Model_DunnageLoad.Quantity)
        )
        {
            return;
        }

        var index = Loads.IndexOf(load);
        if (index < 0 || index >= _workflowService.CurrentSession.LoadQuantities.Count)
        {
            return;
        }

        _workflowService.CurrentSession.LoadQuantities[index] = load.Quantity;
        _workflowService.CurrentSession.Quantity =
            _workflowService.CurrentSession.LoadQuantities.FirstOrDefault();

        if (index == 0)
        {
            _isSynchronizingLoads = true;
            Quantity = decimal.ToInt32(load.Quantity);
            _isSynchronizingLoads = false;
        }

        ValidateQuantity();
        UpdateLoadTotals();
        GoNextCommand.NotifyCanExecuteChanged();
    }

    private void ReplaceLoads(IEnumerable<Model_DunnageLoad> loads)
    {
        foreach (var load in Loads)
        {
            load.PropertyChanged -= Load_PropertyChanged;
        }

        Loads = new ObservableCollection<Model_DunnageLoad>(loads);

        foreach (var load in Loads)
        {
            load.PropertyChanged += Load_PropertyChanged;
        }

        UpdateLoadTotals();
    }

    private void ValidateQuantity()
    {
        if (NumberOfLoads <= 0)
        {
            ValidationMessage = "Number of loads must be at least 1";
        }
        else if (Loads.Any(load => load.Quantity <= 0))
        {
            ValidationMessage = "Every load quantity must be greater than 0";
        }
        else
        {
            ValidationMessage = string.Empty;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _logger.LogInfo("Navigating back to Part Selection", "QuantityEntry");
        _workflowService.GoToStep(Enum_DunnageWorkflowStep.PartSelection);
    }

    [RelayCommand(CanExecute = nameof(IsValid))]
    private async Task GoNextAsync()
    {
        if (!IsValid || IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            _workflowService.SetNavigationLock(true);
            StatusMessage = "Saving load quantities...";

            _workflowService.CurrentSession.NumberOfLoads = NumberOfLoads;

            _logger.LogInfo(
                $"Prepared {NumberOfLoads} non-inventoried label load(s) for the current part",
                "QuantityEntry"
            );

            _workflowService.GoToStep(Enum_DunnageWorkflowStep.DetailsEntry);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Error saving load quantities",
                Enum_ErrorSeverity.Error,
                ex,
                true
            );
        }
        finally
        {
            _workflowService.SetNavigationLock(false);
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShowHelpAsync()
    {
        await _helpService.ShowHelpAsync("Dunnage.QuantityEntry");
    }

    public string GetTooltip(string key) => _helpService.GetTooltip(key);

    public string GetPlaceholder(string key) => _helpService.GetPlaceholder(key);

    public string GetTip(string key) => _helpService.GetTip(key);
}

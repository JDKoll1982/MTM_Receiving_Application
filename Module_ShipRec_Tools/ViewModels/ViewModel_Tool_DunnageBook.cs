using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Shared.ViewModels;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the dunnage book generator tool.
/// </summary>
public partial class ViewModel_Tool_DunnageBook : ViewModel_Shared_Base
{
    private const string AllTypesFilter = "All Types";

    private readonly IService_Tool_DunnageBook _service;
    private readonly List<Model_Tool_DunnageBook_TypeGroup> _allGroups = [];
    private bool _isLoaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasGroups))]
    [NotifyPropertyChangedFor(nameof(FilteredEntryCount))]
    private ObservableCollection<Model_Tool_DunnageBook_TypeGroup> _filteredGroups = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedTypeFilter = AllTypesFilter;

    [ObservableProperty]
    private ObservableCollection<string> _typeFilterOptions = [AllTypesFilter];

    [ObservableProperty]
    private string _coverTitle = Model_Tool_DunnageBook_Config.DefaultCoverTitle;

    [ObservableProperty]
    private bool _includeCoverPage = true;

    [ObservableProperty]
    private bool _includeTableOfContents = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyPropertyChangedFor(nameof(SelectionSummary))]
    private int _selectedCount;

    public bool HasGroups => FilteredGroups.Count > 0;

    public bool HasSelection => SelectedCount > 0;

    public string SelectionSummary => $"{SelectedCount} of {TotalEntryCount} selected";

    public int FilteredEntryCount => FilteredGroups.Sum(group =>
        group.Entries.Count(entry => entry.IsVisible)
    );

    public int TotalEntryCount => _allGroups.Sum(group => group.Entries.Count);

    public Func<ViewModel_Dialog_DunnageBookPreview, Task>? ShowPreviewDialogAsync { get; set; }

    public ViewModel_Tool_DunnageBook(
        IService_Tool_DunnageBook service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public void ActivateView()
    {
        if (_isLoaded)
        {
            ShowStatus(
                $"Loaded {TotalEntryCount} dunnage part(s). Select parts and click Generate Book.",
                InfoBarSeverity.Informational
            );
            return;
        }

        _ = LoadAsync();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();

    partial void OnSelectedTypeFilterChanged(string value) => ApplyFilters();

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            ShowStatus("Loading dunnage parts...");

            var result = await _service.LoadDunnageGroupsAsync();
            if (!result.IsSuccess || result.Data is null)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            _allGroups.Clear();
            _allGroups.AddRange(result.Data);

            foreach (var group in _allGroups)
            {
                foreach (var entry in group.Entries)
                {
                    entry.PropertyChanged += OnEntryPropertyChanged;
                }
            }

            TypeFilterOptions = new ObservableCollection<string>(
                new[] { AllTypesFilter }.Concat(
                    _allGroups
                        .Select(group => group.TypeName)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                )
            );

            _isLoaded = true;
            ApplyFilters();
            RecalculateSelectionCounts();
            ShowStatus(
                $"Loaded {TotalEntryCount} dunnage part(s) across {_allGroups.Count} type(s). Select parts and click Generate Book.",
                InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadAsync),
                nameof(ViewModel_Tool_DunnageBook)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var entry in FilteredGroups.SelectMany(group => group.Entries))
        {
            entry.IsSelected = true;
        }

        ShowStatus(
            $"Selected {SelectedCount} part(s).",
            InfoBarSeverity.Informational
        );
    }

    [RelayCommand]
    private void ClearAll()
    {
        foreach (var entry in FilteredGroups.SelectMany(group => group.Entries))
        {
            entry.IsSelected = false;
        }

        ShowStatus("Cleared the current selection.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private async Task GenerateBookAsync()
    {
        if (!HasSelection)
        {
            ShowStatus(
                "Select at least one dunnage part to generate the book.",
                InfoBarSeverity.Warning
            );
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus($"Generating '{GetSafeCoverTitle()}'...");

            var config = new Model_Tool_DunnageBook_Config
            {
                CoverTitle = GetSafeCoverTitle(),
                IncludeCoverPage = IncludeCoverPage,
                IncludeTableOfContents = IncludeTableOfContents,
            };

            var result = await _service.BuildBookAsync(_allGroups, config);
            if (!result.IsSuccess || result.Data is null)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Warning);
                return;
            }

            if (ShowPreviewDialogAsync is null)
            {
                ShowStatus("Preview is not available from this view.", InfoBarSeverity.Warning);
                return;
            }

            var dialogViewModel = new ViewModel_Dialog_DunnageBookPreview(
                _errorHandler,
                _logger
            );
            dialogViewModel.Initialize(result.Data);

            await ShowPreviewDialogAsync(dialogViewModel);
            ShowStatus("Dunnage book preview is ready.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(GenerateBookAsync),
                nameof(ViewModel_Tool_DunnageBook)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string GetSafeCoverTitle()
    {
        return string.IsNullOrWhiteSpace(CoverTitle)
            ? Model_Tool_DunnageBook_Config.DefaultCoverTitle
            : CoverTitle.Trim();
    }

    private void ApplyFilters()
    {
        var typeFilterActive = !string.Equals(
            SelectedTypeFilter,
            AllTypesFilter,
            StringComparison.OrdinalIgnoreCase
        );
        var searchText = SearchText?.Trim();

        foreach (var entry in _allGroups.SelectMany(group => group.Entries))
        {
            var matchesType = !typeFilterActive
                || string.Equals(
                    entry.TypeName,
                    SelectedTypeFilter,
                    StringComparison.OrdinalIgnoreCase
                );
            var matchesSearch = string.IsNullOrEmpty(searchText)
                || MatchesSearch(entry, searchText);

            entry.IsVisible = matchesType && matchesSearch;
        }

        var visibleGroups = _allGroups
            .Where(group => group.Entries.Any(entry => entry.IsVisible))
            .ToList();

        ReplaceFilteredGroups(visibleGroups);
    }

    private static bool MatchesSearch(Model_Tool_DunnageBook_Entry entry, string searchText)
    {
        if (entry.PartId.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (entry.TypeName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (entry.HomeLocation.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (entry.QuantityType.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return entry.CustomFieldValues.Any(field =>
            field.Value?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true
        );
    }

    private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model_Tool_DunnageBook_Entry.IsSelected))
        {
            RecalculateSelectionCounts();
        }
    }

    private void RecalculateSelectionCounts()
    {
        SelectedCount = _allGroups.Sum(group =>
            group.Entries.Count(entry => entry.IsSelected)
        );
    }

    private void ReplaceFilteredGroups(IEnumerable<Model_Tool_DunnageBook_TypeGroup> groups)
    {
        FilteredGroups = new ObservableCollection<Model_Tool_DunnageBook_TypeGroup>(groups);
    }
}

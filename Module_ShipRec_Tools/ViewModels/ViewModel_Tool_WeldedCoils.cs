using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// ViewModel for the Welded Coils tool: lists the settings_weldedcoils table and
/// supports add, search, active/inactive toggling, and delete-selected. Rows are
/// read-only after they are added; they can only be activated/deactivated or removed.
/// </summary>
public partial class ViewModel_Tool_WeldedCoils : ViewModel_Tool_Base
{
    private readonly IService_Tool_WeldedCoils _service;
    private bool _isLoaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    private string _newPartId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilteredEmpty))]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilteredEmpty))]
    [NotifyPropertyChangedFor(nameof(EmptyStateText))]
    private ObservableCollection<Model_Tool_WeldedCoil> _coils = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilteredEmpty))]
    [NotifyPropertyChangedFor(nameof(EmptyStateText))]
    private ObservableCollection<Model_Tool_WeldedCoil> _filteredCoils = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanDeleteSelected))]
    private Model_Tool_WeldedCoil? _selectedCoil;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveCountText))]
    [NotifyPropertyChangedFor(nameof(InactiveCountText))]
    private int _activeCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveCountText))]
    [NotifyPropertyChangedFor(nameof(InactiveCountText))]
    private int _inactiveCount;

    /// <summary>True when the Add box has a non-empty part number.</summary>
    public bool CanAdd => !string.IsNullOrWhiteSpace(NewPartId);

    /// <summary>True when a row is selected so Delete Selected is available.</summary>
    public bool CanDeleteSelected => SelectedCoil is not null && !IsBusy;

    /// <summary>True when no rows are shown (no data, or nothing matches the search).</summary>
    public bool IsFilteredEmpty => FilteredCoils.Count == 0;

    public string EmptyStateText =>
        Coils.Count == 0
            ? "No welded coils defined. Add a part to get started."
            : "No coils match your search.";

    public string ActiveCountText => $"{ActiveCount} active";
    public string InactiveCountText => $"{InactiveCount} inactive";

    public ViewModel_Tool_WeldedCoils(
        IService_Tool_WeldedCoils service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    partial void OnNewPartIdChanged(string value)
    {
        AddCoilCommand.NotifyCanExecuteChanged();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedCoilChanged(Model_Tool_WeldedCoil? value)
    {
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

    /// <summary>Loads the coil list the first time the tool becomes active.</summary>
    public async void ActivateView()
    {
        if (_isLoaded)
        {
            ShowStatus("Welded coils loaded.", InfoBarSeverity.Informational);
            return;
        }

        _isLoaded = true;
        await LoadCoilsAsync();
    }

    /// <summary>Reloads the full coil list from the database.</summary>
    public async Task LoadCoilsAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _service.GetAllAsync();
            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                return;
            }

            var loaded = result.Data ?? [];
            Coils = new ObservableCollection<Model_Tool_WeldedCoil>(loaded);
            ActiveCount = loaded.Count(c => c.IsActive);
            InactiveCount = loaded.Count - ActiveCount;
            ApplyFilter();
            ShowStatus(
                loaded.Count == 0
                    ? "No welded coils defined. Add a part to get started."
                    : $"{loaded.Count} welded coil(s) loaded.",
                InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadCoilsAsync),
                nameof(ViewModel_Tool_WeldedCoils)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Adds a new coil part (defaults active).</summary>
    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task AddCoilAsync()
    {
        if (IsBusy)
        {
            return;
        }

        var partId = NewPartId.Trim();
        if (string.IsNullOrWhiteSpace(partId))
        {
            return;
        }

        if (
            Coils.Any(c =>
                string.Equals(c.PartId, partId, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            ShowStatus($"Part {partId} is already in the list.", InfoBarSeverity.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _service.InsertAsync(partId);
            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                return;
            }

            NewPartId = string.Empty;
            await LoadCoilsAsync();
            ShowStatus($"Added {partId}.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(AddCoilAsync),
                nameof(ViewModel_Tool_WeldedCoils)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Persists an Active / Inactive change for a row. Called from the view on the
    /// row's Activate / Deactivate button click.
    /// </summary>
    public async Task ToggleActiveAsync(Model_Tool_WeldedCoil coil)
    {
        if (coil is null || coil.Id <= 0 || IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _service.SetActiveAsync(coil.Id, coil.IsActive);
            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                await LoadCoilsAsync();
                return;
            }

            RecalculateCounts();
            ShowStatus(
                coil.IsActive ? $"Activated {coil.PartId}." : $"Deactivated {coil.PartId}.",
                InfoBarSeverity.Success
            );
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(ToggleActiveAsync),
                nameof(ViewModel_Tool_WeldedCoils)
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Deletes the currently selected coil row.</summary>
    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedCoil is null || IsBusy)
        {
            return;
        }

        var coil = SelectedCoil;
        IsBusy = true;
        try
        {
            var result = await _service.DeleteAsync(coil.Id);
            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                return;
            }

            Coils.Remove(coil);
            SelectedCoil = null;
            RecalculateCounts();
            ApplyFilter();
            ShowStatus($"Removed {coil.PartId}.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(DeleteSelectedAsync),
                nameof(ViewModel_Tool_WeldedCoils)
            );
        }
        finally
        {
            IsBusy = false;
            DeleteSelectedCommand.NotifyCanExecuteChanged();
        }
    }

    private void RecalculateCounts()
    {
        ActiveCount = Coils.Count(c => c.IsActive);
        InactiveCount = Coils.Count - ActiveCount;
    }

    /// <summary>Filters the displayed rows by the current search text.</summary>
    private void ApplyFilter()
    {
        var term = SearchText?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(term)
            ? Coils.ToList()
            : Coils
                .Where(c => c.PartId.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();

        FilteredCoils = new ObservableCollection<Model_Tool_WeldedCoil>(filtered);
    }
}

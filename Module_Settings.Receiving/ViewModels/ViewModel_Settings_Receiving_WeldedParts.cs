using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Receiving.Contracts;
using MTM_Receiving_Application.Module_Settings.Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Receiving.ViewModels;

/// <summary>
/// Maintains the welded part numbers list: the parts whose coil needs its inner diameter welded
/// before it goes on the press cradle.
/// </summary>
/// <remarks>
/// The list is shared with the Extra Tools Welded Coils page, so a change made here is visible there
/// and the other way round. Rows are read-only once added; a row can only be removed.
/// </remarks>
public sealed partial class ViewModel_Settings_Receiving_WeldedParts : ViewModel_Shared_Base
{
    private readonly IService_Settings_ReceivingWeldedParts _service;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAdd))]
    private string _newPartId = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilteredEmpty))]
    [NotifyPropertyChangedFor(nameof(EmptyStateText))]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilteredEmpty))]
    [NotifyPropertyChangedFor(nameof(EmptyStateText))]
    [NotifyPropertyChangedFor(nameof(TotalCountText))]
    private ObservableCollection<Model_Settings_ReceivingWeldedPart> _weldedParts = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFilteredEmpty))]
    [NotifyPropertyChangedFor(nameof(EmptyStateText))]
    private ObservableCollection<Model_Settings_ReceivingWeldedPart> _filteredParts = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanDeleteSelected))]
    private Model_Settings_ReceivingWeldedPart? _selectedPart;

    /// <summary>True when the Add box holds something to add.</summary>
    public bool CanAdd => !string.IsNullOrWhiteSpace(NewPartId);

    /// <summary>True when a row is selected, so Remove Selected is available.</summary>
    public bool CanDeleteSelected => SelectedPart is not null;

    /// <summary>True when no rows are shown, either because the list is empty or nothing matches.</summary>
    public bool IsFilteredEmpty => FilteredParts.Count == 0;

    /// <summary>What the empty list says, so the operator knows which case they are in.</summary>
    public string EmptyStateText =>
        WeldedParts.Count == 0
            ? "No welded part numbers yet. Add a part to start the list."
            : "No part numbers match your search.";

    /// <summary>The row count shown above the list.</summary>
    public string TotalCountText => $"{WeldedParts.Count} part(s)";

    public ViewModel_Settings_Receiving_WeldedParts(
        IService_Settings_ReceivingWeldedParts service,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        Title = "Welded Part Numbers";
        _ = LoadAsync();
    }

    partial void OnNewPartIdChanged(string value) => AddPartCommand.NotifyCanExecuteChanged();

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedPartChanged(Model_Settings_ReceivingWeldedPart? value) =>
        DeleteSelectedCommand.NotifyCanExecuteChanged();

    /// <summary>Reloads the list from the database.</summary>
    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await ReloadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Loads the list without touching <see cref="ViewModel_Shared_Base.IsBusy"/>, so callers that
    /// already own the busy flag can refresh after a write.
    /// </summary>
    private async Task ReloadAsync()
    {
        try
        {
            var result = await _service.GetAllAsync();
            if (!result.IsSuccess)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "Load welded part numbers");
                return;
            }

            WeldedParts = new ObservableCollection<Model_Settings_ReceivingWeldedPart>(
                (result.Data ?? []).OrderBy(part => part.PartId, StringComparer.OrdinalIgnoreCase)
            );
            ApplyFilter();
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to load welded part numbers",
                Enum_ErrorSeverity.Medium,
                ex
            );
        }
    }

    /// <summary>Adds the part number in the box.</summary>
    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task AddPartAsync()
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
            WeldedParts.Any(part =>
                string.Equals(part.PartId, partId, StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            ShowStatus($"Part {partId} is already on the list.", InfoBarSeverity.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _service.AddAsync(partId);
            if (!result.IsSuccess)
            {
                ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
                return;
            }

            NewPartId = string.Empty;
            await ReloadAsync();
            ShowStatus($"Added {partId}.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Failed to add welded part {partId}",
                Enum_ErrorSeverity.Medium,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Removes the selected part number.</summary>
    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedPart is not null)
        {
            await RemoveAsync(SelectedPart);
        }
    }

    /// <summary>Removes one part number, called by a row's Remove button.</summary>
    /// <param name="part">The row to remove.</param>
    public async Task RemoveAsync(Model_Settings_ReceivingWeldedPart part)
    {
        if (part is null || part.Id <= 0 || IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _service.DeleteAsync(part.Id);
            if (!result.IsSuccess)
            {
                await _errorHandler.HandleDaoErrorAsync(result, $"Remove {part.PartId}");
                return;
            }

            WeldedParts.Remove(part);
            if (ReferenceEquals(SelectedPart, part))
            {
                SelectedPart = null;
            }

            ApplyFilter();
            ShowStatus($"Removed {part.PartId}.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                $"Failed to remove welded part {part.PartId}",
                Enum_ErrorSeverity.Medium,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var term = SearchText?.Trim() ?? string.Empty;
        var matching = string.IsNullOrWhiteSpace(term)
            ? WeldedParts
            : new ObservableCollection<Model_Settings_ReceivingWeldedPart>(
                WeldedParts.Where(part =>
                    part.PartId.Contains(term, StringComparison.OrdinalIgnoreCase)
                )
            );

        // Replace the collection instead of clearing and refilling it, so the list rebinds once.
        FilteredParts = new ObservableCollection<Model_Settings_ReceivingWeldedPart>(matching);
        OnPropertyChanged(nameof(TotalCountText));
    }
}

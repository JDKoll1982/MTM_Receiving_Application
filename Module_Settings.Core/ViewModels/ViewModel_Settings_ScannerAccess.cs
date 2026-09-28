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
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

/// <summary>
/// ViewModel for the Core Settings ▸ Scanner Access page. The allow-list is plant-wide: saving
/// here changes Scanner availability for every workstation at once.
/// </summary>
public partial class ViewModel_Settings_ScannerAccess : ViewModel_Shared_Base
{
    private readonly IService_SettingsUserDirectory _userDirectory;
    private readonly IService_ScannerAccessPolicy _accessPolicy;

    private List<ViewModel_SettingsScannerAccessRow> _allRows = new();

    [ObservableProperty]
    private ObservableCollection<ViewModel_SettingsScannerAccessRow> _users = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showInactiveUsers;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccessSummary))]
    private int _allowedUserCount;

    /// <summary>
    /// True while no allow-list has been saved, meaning the legacy developer-only rule still
    /// controls Scanner access.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AccessSummary))]
    private bool _usesLegacyDeveloperRule;

    public ViewModel_Settings_ScannerAccess(
        IService_SettingsUserDirectory userDirectory,
        IService_ScannerAccessPolicy accessPolicy,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _userDirectory = userDirectory ?? throw new ArgumentNullException(nameof(userDirectory));
        _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
        Title = "Scanner Access";
        _ = LoadAsync();
    }

    public bool HasUsers => _allRows.Count > 0;

    public string AccessSummary =>
        UsesLegacyDeveloperRule
            ? "No allow-list saved. Scanner is currently limited to the legacy developer accounts."
            : $"{AllowedUserCount} user(s) may use Scanner on every workstation.";

    [RelayCommand]
    private async Task ReloadAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private void ClearAll()
    {
        foreach (var row in _allRows)
        {
            row.IsAllowed = false;
        }

        ShowStatus("All users cleared. Save to apply, or leave empty to restore the legacy rule.");
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            var allowedEmployeeNumbers = _allRows
                .Where(row => row.IsAllowed)
                .Select(row => row.EmployeeNumber)
                .ToArray();

            var result = await _accessPolicy.SaveAllowedEmployeeNumbersAsync(
                allowedEmployeeNumbers
            );

            if (!result.Success)
            {
                await _errorHandler.HandleDaoErrorAsync(result, nameof(SaveAsync));
                ShowStatus("Failed to save the Scanner access allow-list.");
                return;
            }

            UsesLegacyDeveloperRule = allowedEmployeeNumbers.Length == 0;
            RefreshSummary();
            ShowStatus(
                UsesLegacyDeveloperRule
                    ? "Allow-list cleared. Scanner is back to the legacy developer-only rule."
                    : $"Scanner access saved for {allowedEmployeeNumbers.Length} user(s)."
            );
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save Scanner access.", ex, nameof(SaveAsync));
            await _errorHandler.HandleErrorAsync(
                "Failed to save Scanner access.",
                Enum_ErrorSeverity.Medium,
                ex,
                true
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnShowInactiveUsersChanged(bool value)
    {
        ApplyFilter();
    }

    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;

            var usersResult = await _userDirectory.GetAllUsersAsync();
            if (!usersResult.Success || usersResult.Data is null)
            {
                await _errorHandler.HandleDaoErrorAsync(usersResult, nameof(LoadAsync));
                ShowStatus("Failed to load application users.");
                return;
            }

            var allowedResult = await _accessPolicy.GetAllowedEmployeeNumbersAsync();
            if (!allowedResult.Success || allowedResult.Data is null)
            {
                await _errorHandler.HandleDaoErrorAsync(allowedResult, nameof(LoadAsync));
                ShowStatus("Failed to load the Scanner access allow-list.");
                return;
            }

            var allowedEmployeeNumbers = allowedResult.Data.ToHashSet();
            DetachRows(_allRows);
            _allRows = usersResult
                .Data.OrderBy(user => user.FullName, StringComparer.CurrentCultureIgnoreCase)
                .Select(
                    user =>
                        new ViewModel_SettingsScannerAccessRow(
                            user,
                            allowedEmployeeNumbers.Contains(user.EmployeeNumber)
                        )
                )
                .ToList();
            AttachRows(_allRows);

            UsesLegacyDeveloperRule = allowedEmployeeNumbers.Count == 0;
            ApplyFilter();
            RefreshSummary();
            ShowStatus("Scanner access loaded.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load Scanner access.", ex, nameof(LoadAsync));
            ShowStatus("Failed to load Scanner access.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var query = _allRows.AsEnumerable();

        if (!ShowInactiveUsers)
        {
            query = query.Where(row => row.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            query = query.Where(
                row =>
                    row.FullName.Contains(term, StringComparison.CurrentCultureIgnoreCase)
                    || row.WindowsUsername.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || row.Department.Contains(term, StringComparison.CurrentCultureIgnoreCase)
            );
        }

        // Replace the collection instance so the list rebuilds in a single change notification.
        Users = new ObservableCollection<ViewModel_SettingsScannerAccessRow>(query);
        OnPropertyChanged(nameof(HasUsers));
    }

    private void AttachRows(IEnumerable<ViewModel_SettingsScannerAccessRow> rows)
    {
        foreach (var row in rows)
        {
            row.PropertyChanged += OnRowPropertyChanged;
        }
    }

    private void DetachRows(IEnumerable<ViewModel_SettingsScannerAccessRow> rows)
    {
        foreach (var row in rows)
        {
            row.PropertyChanged -= OnRowPropertyChanged;
        }
    }

    private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel_SettingsScannerAccessRow.IsAllowed))
        {
            RefreshSummary();
        }
    }

    private void RefreshSummary()
    {
        AllowedUserCount = _allRows.Count(row => row.IsAllowed);
    }
}

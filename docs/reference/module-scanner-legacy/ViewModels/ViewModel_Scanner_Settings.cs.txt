using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Scanner.Contracts;
using MTM_Receiving_Application.Module_Scanner.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Scanner.ViewModels;

/// <summary>
/// Placeholder ViewModel for the scanner settings page.
/// </summary>
public partial class ViewModel_Scanner_Settings : ViewModel_Shared_Base
{
    private readonly IService_ScannerNavigation _navigationService;
    private readonly IService_ScannerWorkflow _workflowService;

    [ObservableProperty]
    private ObservableCollection<Model_ScannerProfile> _profiles = [];

    [ObservableProperty]
    private Model_ScannerProfile? _selectedProfile;

    [ObservableProperty]
    private string _ownerUserId = Environment.UserName;

    [ObservableProperty]
    private string _profileName = "Default";

    [ObservableProperty]
    private string _targetExecutableName = "VMINVENT.exe";

    [ObservableProperty]
    private string _appWindowTitle = "Inventory Transfers";

    [ObservableProperty]
    private string _targetChildWindowTitle = "Inventory Transfers";

    [ObservableProperty]
    private string _appWindowClass = string.Empty;

    [ObservableProperty]
    private string _fromWarehouseDefault = "002";

    [ObservableProperty]
    private string _toWarehouseDefault = "002";

    [ObservableProperty]
    private bool _requireExactTitleMatch;

    [ObservableProperty]
    private double _activationDelayMs = 250;

    [ObservableProperty]
    private double _delayBetweenFieldsMs = 50;

    [ObservableProperty]
    private double _pauseAfterItemMs = 150;

    [ObservableProperty]
    private double _popupTimeoutMs = 1500;

    [ObservableProperty]
    private double _popupCloseTimeoutMs = 1500;

    [ObservableProperty]
    private string _sendShortcutChord = "Ctrl+Alt+M";

    [ObservableProperty]
    private bool _allowAdvancedTiming;

    public ViewModel_Scanner_Settings(
        IService_ScannerNavigation navigationService,
        IService_ScannerWorkflow workflowService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(navigationService);
        ArgumentNullException.ThrowIfNull(workflowService);
        _navigationService = navigationService;
        _workflowService = workflowService;
    }

    [RelayCommand]
    private void NavigateToWorkbench()
    {
        _navigationService.ShowWorkbench();
    }

    [RelayCommand]
    private void NavigateToHistory()
    {
        _navigationService.ShowHistory();
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        _navigationService.ShowSettings();
    }

    [RelayCommand]
    private async Task LoadProfilesAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _workflowService.GetProfilesAsync(OwnerUserId);
            if (!result.Success || result.Data is null)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "Unable to load scanner profiles."
                        : result.ErrorMessage,
                    InfoBarSeverity.Error
                );
                return;
            }

            Profiles = [.. result.Data.OrderByDescending(profile => profile.IsDefaultForUser).ThenBy(profile => profile.ProfileName)];

            SelectedProfile = Profiles.FirstOrDefault();
            if (SelectedProfile is not null)
            {
                ApplyProfileToEditor(SelectedProfile);
            }

            ShowStatus($"Loaded {Profiles.Count} scanner profiles.", InfoBarSeverity.Success);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (string.IsNullOrWhiteSpace(TargetExecutableName))
        {
            ShowStatus("Target executable name is required.", InfoBarSeverity.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(TargetChildWindowTitle))
        {
            ShowStatus("Target child screen title is required.", InfoBarSeverity.Warning);
            return;
        }

        var normalizedProfileName = ProfileName.Trim();
        var duplicateProfileExists = Profiles.Any(profile =>
            !string.Equals(profile.ProfileId.ToString(), SelectedProfile?.ProfileId.ToString(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(profile.OwnerUserId, OwnerUserId, StringComparison.OrdinalIgnoreCase)
            && string.Equals(profile.ProfileName.Trim(), normalizedProfileName, StringComparison.OrdinalIgnoreCase));

        if (duplicateProfileExists)
        {
            ShowStatus("Profile name must be unique for the current user.", InfoBarSeverity.Warning);
            return;
        }

        var profile = BuildProfileFromEditor();

        IsBusy = true;
        try
        {
            var result = await _workflowService.SaveProfileAsync(profile);
            if (!result.Success || result.Data is null)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "Unable to save scanner profile."
                        : result.ErrorMessage,
                    InfoBarSeverity.Error
                );
                return;
            }

            SelectedProfile = result.Data;
            await LoadProfilesAsync();
            ShowStatus("Scanner profile saved.", InfoBarSeverity.Success);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SetDefaultProfileAsync()
    {
        if (SelectedProfile is null)
        {
            ShowStatus("Select a profile before setting default.", InfoBarSeverity.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _workflowService.SetDefaultProfileAsync(
                SelectedProfile.ProfileId,
                OwnerUserId
            );
            if (!result.Success)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "Unable to set default profile."
                        : result.ErrorMessage,
                    InfoBarSeverity.Error
                );
                return;
            }

            await LoadProfilesAsync();
            ShowStatus("Default scanner profile updated.", InfoBarSeverity.Success);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NewProfile()
    {
        SelectedProfile = null;
        ProfileName = "New Profile";
        TargetExecutableName = "VMINVENT.exe";
        AppWindowTitle = "Inventory Transfers";
        TargetChildWindowTitle = "Inventory Transfers";
        AppWindowClass = string.Empty;
        FromWarehouseDefault = "002";
        ToWarehouseDefault = "002";
        RequireExactTitleMatch = false;
        ApplySafeDefaults();
        ShowStatus("New scanner profile initialized.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private void DuplicateProfile()
    {
        if (SelectedProfile is null)
        {
            ShowStatus("Select a profile before duplicating.", InfoBarSeverity.Warning);
            return;
        }

        var duplicate = BuildProfileFromEditor();
        duplicate.ProfileId = Guid.NewGuid();
        duplicate.ProfileName = $"{SelectedProfile.ProfileName} Copy";
        SelectedProfile = duplicate;
        ApplyProfileToEditor(duplicate);
        ShowStatus("Profile duplicated in editor. Save to persist.", InfoBarSeverity.Informational);
    }

    [RelayCommand]
    private async Task DeleteProfileAsync()
    {
        if (SelectedProfile is null)
        {
            ShowStatus("Select a profile before deleting.", InfoBarSeverity.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _workflowService.DeleteProfileAsync(SelectedProfile.ProfileId, OwnerUserId);
            if (!result.Success)
            {
                ShowStatus(
                    string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "Unable to delete scanner profile."
                        : result.ErrorMessage,
                    InfoBarSeverity.Error
                );
                return;
            }

            await LoadProfilesAsync();
            ShowStatus("Scanner profile deleted.", InfoBarSeverity.Success);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ResetEditor()
    {
        if (SelectedProfile is not null)
        {
            ApplyProfileToEditor(SelectedProfile);
            ShowStatus("Editor reset to selected profile values.", InfoBarSeverity.Informational);
            return;
        }

        NewProfile();
    }

    [RelayCommand]
    private void ApplySafeDefaults()
    {
        ActivationDelayMs = 250;
        DelayBetweenFieldsMs = 50;
        PauseAfterItemMs = 150;
        PopupTimeoutMs = 1500;
        PopupCloseTimeoutMs = 1500;
        AllowAdvancedTiming = false;
    }

    partial void OnSelectedProfileChanged(Model_ScannerProfile? value)
    {
        if (value is null)
        {
            return;
        }

        ApplyProfileToEditor(value);
    }

    private void ApplyProfileToEditor(Model_ScannerProfile profile)
    {
        ProfileName = profile.ProfileName;
        TargetExecutableName = profile.TargetExecutableName;
        AppWindowTitle = profile.AppWindowTitle;
        TargetChildWindowTitle = profile.TargetChildWindowTitle;
        AppWindowClass = profile.AppWindowClass;
        FromWarehouseDefault = profile.FromWarehouseDefault;
        ToWarehouseDefault = profile.ToWarehouseDefault;
        RequireExactTitleMatch = profile.RequireExactTitleMatch;
        ActivationDelayMs = profile.ActivationDelayMs;
        DelayBetweenFieldsMs = profile.DelayBetweenFieldsMs;
        PauseAfterItemMs = profile.PauseAfterItemMs;
        PopupTimeoutMs = profile.PopupTimeoutMs;
        PopupCloseTimeoutMs = profile.PopupCloseTimeoutMs;
        SendShortcutChord = profile.SendShortcutChord;
        AllowAdvancedTiming = profile.AllowAdvancedTiming;
    }

    private Model_ScannerProfile BuildProfileFromEditor()
    {
        return new Model_ScannerProfile
        {
            ProfileId = SelectedProfile?.ProfileId ?? Guid.NewGuid(),
            OwnerUserId = OwnerUserId,
            ProfileName = ProfileName,
            TargetExecutableName = TargetExecutableName,
            AppWindowTitle = AppWindowTitle,
            TargetChildWindowTitle = TargetChildWindowTitle,
            AppWindowClass = AppWindowClass,
            FromWarehouseDefault = FromWarehouseDefault,
            ToWarehouseDefault = ToWarehouseDefault,
            RequireExactTitleMatch = RequireExactTitleMatch,
            ActivationDelayMs = Convert.ToInt32(ActivationDelayMs),
            DelayBetweenFieldsMs = Convert.ToInt32(DelayBetweenFieldsMs),
            PauseAfterItemMs = Convert.ToInt32(PauseAfterItemMs),
            PopupTimeoutMs = Convert.ToInt32(PopupTimeoutMs),
            PopupCloseTimeoutMs = Convert.ToInt32(PopupCloseTimeoutMs),
            SendShortcutChord = SendShortcutChord,
            AllowAdvancedTiming = AllowAdvancedTiming,
        };
    }
}
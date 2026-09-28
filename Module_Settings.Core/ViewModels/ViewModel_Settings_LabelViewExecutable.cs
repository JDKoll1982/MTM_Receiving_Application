using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Models;
using MTM_Receiving_Application.Module_Settings.Core.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Core.Settings;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

public partial class ViewModel_Settings_LabelViewExecutable : ViewModel_Shared_Base
{
    private const int MaxButtons = 10;

    private readonly IService_LabelViewLauncher _labelViewLauncher;
    private readonly IService_SettingsCoreFacade _settingsCore;
    private readonly IService_SettingsUserLabelButtons _buttonSettings;

    [ObservableProperty]
    private string _statusMessage = "Loading settings...";

    [ObservableProperty]
    private string _executablePath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Model_MainWindowLabelButton> _labelButtons = new();

    [ObservableProperty]
    private Model_MainWindowLabelButton? _selectedLabelButton;

    [ObservableProperty]
    private string _selectedButtonLabel = string.Empty;

    [ObservableProperty]
    private string _selectedButtonPath = string.Empty;

    [ObservableProperty]
    private bool _isSelectedButtonEnabled;

    [ObservableProperty]
    private MaterialIconKind? _selectedIconKind = MaterialIconKind.PackageVariantClosed;

    [ObservableProperty]
    private string _selectedAccentOption = nameof(Enum_MainWindowLabelButtonAccent.Neutral);

    [ObservableProperty]
    private ObservableCollection<Model_IconDefinition> _recentlyUsedIcons = new();

    public IReadOnlyList<string> AccentOptions { get; } = Enum
        .GetNames<Enum_MainWindowLabelButtonAccent>()
        .ToList();

    public bool IsButtonSelected => SelectedLabelButton is not null;

    public string DefaultExecutablePath => _labelViewLauncher.DefaultExecutablePath;

    public ViewModel_Settings_LabelViewExecutable(
        IService_SettingsCoreFacade settingsCore,
        IService_SettingsUserLabelButtons buttonSettings,
        IService_LabelViewLauncher labelViewLauncher,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _settingsCore = settingsCore ?? throw new ArgumentNullException(nameof(settingsCore));
        _buttonSettings = buttonSettings ?? throw new ArgumentNullException(nameof(buttonSettings));
        _labelViewLauncher =
            labelViewLauncher ?? throw new ArgumentNullException(nameof(labelViewLauncher));
        Title = "LabelView Executable";
        _ = LoadSettingsAsync();
    }

    [RelayCommand]
    private void UseDefaultPath()
    {
        ExecutablePath = DefaultExecutablePath;
        StatusMessage = "Default LabelView path loaded into the editor.";
    }

    [RelayCommand]
    private async Task OpenFolderAsync(string? configuredPath)
    {
        var result = await _labelViewLauncher.OpenFolderForPathAsync(configuredPath);
        if (!result.IsSuccess)
        {
            await _errorHandler.HandleDaoErrorAsync(result, nameof(OpenFolderAsync));
            return;
        }

        StatusMessage = "Opened File Explorer for the current path.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (!_labelViewLauncher.IsExecutablePathValid(ExecutablePath))
        {
            StatusMessage = "LabelView path must point to an existing LV.exe file.";
            return;
        }

        try
        {
            IsBusy = true;
            var pathResult = await _settingsCore.SetSettingAsync(
                CoreSettingsKeys.SystemCategory,
                CoreSettingsKeys.LabelView.ExecutablePath,
                ExecutablePath
            );

            if (!pathResult.IsSuccess)
            {
                await _errorHandler.HandleDaoErrorAsync(
                    pathResult,
                    $"Save {CoreSettingsKeys.LabelView.ExecutablePath}"
                );
                StatusMessage = "Failed to save LabelView path.";
                return;
            }

            var buttonsResult = await _buttonSettings.SaveButtonsAsync(LabelButtons.ToList());
            if (!buttonsResult.IsSuccess)
            {
                await _errorHandler.HandleDaoErrorAsync(
                    buttonsResult,
                    nameof(SaveAsync)
                );
                StatusMessage = "LabelView path saved, but button configuration failed to save.";
                return;
            }

            StatusMessage = "LabelView and MainWindow label button settings saved.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(SaveAsync),
                nameof(ViewModel_Settings_LabelViewExecutable)
            );
            StatusMessage = "Failed to save LabelView path.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddButton()
    {
        if (LabelButtons.Count >= MaxButtons)
        {
            StatusMessage = $"You can configure up to {MaxButtons} label buttons.";
            return;
        }

        var nextIndex = LabelButtons.Count;
        var newButton = new Model_MainWindowLabelButton
        {
            Id = Guid.NewGuid().ToString("N"),
            Label = $"Label {nextIndex + 1}",
            LabelPath = string.Empty,
            IconKey = nameof(MaterialIconKind.PackageVariantClosed),
            Accent = Enum_MainWindowLabelButtonAccent.Neutral,
            IsEnabled = true,
            SortOrder = nextIndex,
        };

        var updated = LabelButtons.ToList();
        updated.Add(newButton);
        ReplaceButtons(updated, newButton.Id);
        StatusMessage = "New label button added.";
    }

    [RelayCommand]
    private void DuplicateSelectedButton()
    {
        if (SelectedLabelButton is null)
        {
            return;
        }

        if (LabelButtons.Count >= MaxButtons)
        {
            StatusMessage = $"You can configure up to {MaxButtons} label buttons.";
            return;
        }

        var clone = new Model_MainWindowLabelButton
        {
            Id = Guid.NewGuid().ToString("N"),
            Label = $"{SelectedLabelButton.Label} Copy",
            LabelPath = SelectedLabelButton.LabelPath,
            IconKey = SelectedLabelButton.IconKey,
            Accent = SelectedLabelButton.Accent,
            IsEnabled = SelectedLabelButton.IsEnabled,
            SortOrder = SelectedLabelButton.SortOrder + 1,
        };

        var updated = LabelButtons.ToList();
        var selectedIndex = updated.FindIndex(button => button.Id == SelectedLabelButton.Id);
        if (selectedIndex < 0)
        {
            selectedIndex = updated.Count - 1;
        }

        updated.Insert(selectedIndex + 1, clone);
        ReplaceButtons(updated, clone.Id);
        StatusMessage = "Selected label button duplicated.";
    }

    [RelayCommand]
    private void RemoveSelectedButton()
    {
        if (SelectedLabelButton is null)
        {
            return;
        }

        var updated = LabelButtons.Where(button => button.Id != SelectedLabelButton.Id).ToList();
        ReplaceButtons(updated, updated.FirstOrDefault()?.Id);
        StatusMessage = "Selected label button removed.";
    }

    [RelayCommand]
    private void MoveSelectedButtonUp()
    {
        if (SelectedLabelButton is null)
        {
            return;
        }

        var updated = LabelButtons.ToList();
        var index = updated.FindIndex(button => button.Id == SelectedLabelButton.Id);
        if (index <= 0)
        {
            return;
        }

        (updated[index - 1], updated[index]) = (updated[index], updated[index - 1]);
        ReplaceButtons(updated, SelectedLabelButton.Id);
    }

    [RelayCommand]
    private void MoveSelectedButtonDown()
    {
        if (SelectedLabelButton is null)
        {
            return;
        }

        var updated = LabelButtons.ToList();
        var index = updated.FindIndex(button => button.Id == SelectedLabelButton.Id);
        if (index < 0 || index >= updated.Count - 1)
        {
            return;
        }

        (updated[index + 1], updated[index]) = (updated[index], updated[index + 1]);
        ReplaceButtons(updated, SelectedLabelButton.Id);
    }

    [RelayCommand]
    private async Task ResetButtonsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _buttonSettings.ResetToDefaultsAsync();
            if (!result.IsSuccess)
            {
                await _errorHandler.HandleDaoErrorAsync(result, nameof(ResetButtonsAsync));
                StatusMessage = "Failed to reset label buttons.";
                return;
            }

            var buttons = await _buttonSettings.GetButtonsAsync();
            ReplaceButtons(buttons.ToList(), buttons.FirstOrDefault()?.Id);
            StatusMessage = "MainWindow label buttons reset to defaults.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void SetSelectedButtonPath(string path)
    {
        SelectedButtonPath = path ?? string.Empty;
    }

    partial void OnSelectedLabelButtonChanged(Model_MainWindowLabelButton? value)
    {
        OnPropertyChanged(nameof(IsButtonSelected));

        if (value is null)
        {
            SelectedButtonLabel = string.Empty;
            SelectedButtonPath = string.Empty;
            IsSelectedButtonEnabled = false;
            SelectedIconKind = MaterialIconKind.PackageVariantClosed;
            SelectedAccentOption = Enum_MainWindowLabelButtonAccent.Neutral.ToString();
            return;
        }

        SelectedButtonLabel = value.Label;
        SelectedButtonPath = value.LabelPath;
        IsSelectedButtonEnabled = value.IsEnabled;
        SelectedAccentOption = value.Accent.ToString();
        SelectedIconKind = value.IconKind;
    }

    partial void OnSelectedButtonLabelChanged(string value)
    {
        if (SelectedLabelButton is null)
        {
            return;
        }

        SelectedLabelButton.Label = value;
    }

    partial void OnSelectedButtonPathChanged(string value)
    {
        if (SelectedLabelButton is null)
        {
            return;
        }

        SelectedLabelButton.LabelPath = value;
    }

    partial void OnIsSelectedButtonEnabledChanged(bool value)
    {
        if (SelectedLabelButton is null)
        {
            return;
        }

        SelectedLabelButton.IsEnabled = value;
    }

    partial void OnSelectedIconKindChanged(MaterialIconKind? value)
    {
        if (SelectedLabelButton is null || value is null)
        {
            return;
        }

        SelectedLabelButton.IconKey = value.Value.ToString();
        RefreshRecentIcons();
    }

    partial void OnSelectedAccentOptionChanged(string value)
    {
        if (SelectedLabelButton is null)
        {
            return;
        }

        if (!Enum.TryParse<Enum_MainWindowLabelButtonAccent>(value, out var accent))
        {
            return;
        }

        SelectedLabelButton.Accent = accent;
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            IsBusy = true;
            var result = await _settingsCore.GetSettingAsync(
                CoreSettingsKeys.SystemCategory,
                CoreSettingsKeys.LabelView.ExecutablePath
            );

            ExecutablePath = string.IsNullOrWhiteSpace(result.Data?.Value)
                ? DefaultExecutablePath
                : result.Data!.Value;

            var buttonSettings = await _buttonSettings.GetButtonsAsync();
            ReplaceButtons(buttonSettings.ToList(), buttonSettings.FirstOrDefault()?.Id);

            StatusMessage = _labelViewLauncher.IsExecutablePathValid(ExecutablePath)
                ? "LabelView executable found."
                : "LabelView executable was not found. Confirm the path below or contact IT.";
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Enum_ErrorSeverity.Medium,
                nameof(LoadSettingsAsync),
                nameof(ViewModel_Settings_LabelViewExecutable)
            );
            ExecutablePath = DefaultExecutablePath;
            StatusMessage = "Failed to load LabelView settings.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ReplaceButtons(List<Model_MainWindowLabelButton> buttons, string? selectedId)
    {
        NormalizeSortOrder(buttons);
        LabelButtons = new ObservableCollection<Model_MainWindowLabelButton>(buttons);
        RefreshRecentIcons();

        if (string.IsNullOrWhiteSpace(selectedId))
        {
            SelectedLabelButton = LabelButtons.FirstOrDefault();
            return;
        }

        SelectedLabelButton = LabelButtons.FirstOrDefault(button => button.Id == selectedId)
            ?? LabelButtons.FirstOrDefault();
    }

    private static void NormalizeSortOrder(List<Model_MainWindowLabelButton> buttons)
    {
        for (var index = 0; index < buttons.Count; index++)
        {
            buttons[index].SortOrder = index;
        }
    }

    private void RefreshRecentIcons()
    {
        var recentIcons = LabelButtons
            .GroupBy(button => button.IconKey, StringComparer.OrdinalIgnoreCase)
            .Select(
                group =>
                    new Model_IconDefinition
                    {
                        IconName = group.First().IconKey,
                    }
            )
            .Take(8)
            .ToList();

        RecentlyUsedIcons = new ObservableCollection<Model_IconDefinition>(recentIcons);
    }
}

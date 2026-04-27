using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Helpers.UI;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Dunnage.Contracts;
using MTM_Receiving_Application.Module_Settings.Core.Helpers;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Settings.Dunnage.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Dunnage.ViewModels;

public sealed partial class ViewModel_Settings_Dunnage_KeyboardShortcuts : ViewModel_Shared_Base
{
    private readonly IService_DunnageShortcuts _dunnageShortcuts;

    private bool _isLoading;
    private bool _isPersistingToggle;

    [ObservableProperty]
    private Model_KeyboardShortcutBinding _modeSelectionShortcut = new();

    [ObservableProperty]
    private Model_KeyboardShortcutBinding _clearLabelDataShortcut = new();

    [ObservableProperty]
    private Model_KeyboardShortcutBinding _nextStepShortcut = new();

    [ObservableProperty]
    private Model_KeyboardShortcutBinding _backStepShortcut = new();

    [ObservableProperty]
    private Model_KeyboardShortcutBinding _helpShortcut = new();

    [ObservableProperty]
    private bool _isToggleSimpleNavigationEnabled = true;

    [ObservableProperty]
    private string _nextStepCurrentShortcutText = string.Empty;

    [ObservableProperty]
    private string _backStepCurrentShortcutText = string.Empty;

    [ObservableProperty]
    private string _simpleNavigationNoteText = string.Empty;

    public ViewModel_Settings_Dunnage_KeyboardShortcuts(
        IService_DunnageShortcuts dunnageShortcuts,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _dunnageShortcuts =
            dunnageShortcuts ?? throw new ArgumentNullException(nameof(dunnageShortcuts));
        Title = "Dunnage Keyboard Shortcuts";
        _ = LoadShortcutsAsync();
    }

    public IReadOnlyList<Model_Settings_KeyValueOption> AvailableKeys { get; } =
        Helper_Settings_KeyboardShortcutOptions.CommonKeys;

    partial void OnModeSelectionShortcutChanged(
        Model_KeyboardShortcutBinding? oldValue,
        Model_KeyboardShortcutBinding newValue
    )
    {
        ReplaceBindingSubscription(oldValue, newValue);
    }

    partial void OnClearLabelDataShortcutChanged(
        Model_KeyboardShortcutBinding? oldValue,
        Model_KeyboardShortcutBinding newValue
    )
    {
        ReplaceBindingSubscription(oldValue, newValue);
    }

    partial void OnNextStepShortcutChanged(
        Model_KeyboardShortcutBinding? oldValue,
        Model_KeyboardShortcutBinding newValue
    )
    {
        ReplaceBindingSubscription(oldValue, newValue);
        RefreshNavigationShortcutText();
    }

    partial void OnBackStepShortcutChanged(
        Model_KeyboardShortcutBinding? oldValue,
        Model_KeyboardShortcutBinding newValue
    )
    {
        ReplaceBindingSubscription(oldValue, newValue);
        RefreshNavigationShortcutText();
    }

    partial void OnHelpShortcutChanged(
        Model_KeyboardShortcutBinding? oldValue,
        Model_KeyboardShortcutBinding newValue
    )
    {
        ReplaceBindingSubscription(oldValue, newValue);
    }

    partial void OnIsToggleSimpleNavigationEnabledChanged(bool value)
    {
        RefreshNavigationShortcutText();

        if (_isLoading || _isPersistingToggle)
        {
            return;
        }

        _ = PersistToggleSettingAsync(value);
    }

    [RelayCommand]
    private async Task SaveShortcutsAsync()
    {
        try
        {
            IsBusy = true;
            await _dunnageShortcuts.SaveShortcutsAsync(CreateShortcutModel());
            ShowStatus("Dunnage keyboard shortcuts saved.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save Dunnage keyboard shortcuts.",
                Enum_ErrorSeverity.Error,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        try
        {
            IsBusy = true;
            var defaults = Model_Settings_DunnageShortcuts.CreateDefault();
            await _dunnageShortcuts.SaveShortcutsAsync(defaults);
            ApplyShortcutModel(defaults);
            ShowStatus("Dunnage keyboard shortcuts reset to defaults.");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to reset Dunnage keyboard shortcuts.",
                Enum_ErrorSeverity.Error,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadShortcutsAsync()
    {
        try
        {
            _isLoading = true;
            var shortcuts = await _dunnageShortcuts.GetShortcutsAsync();
            ApplyShortcutModel(shortcuts ?? Model_Settings_DunnageShortcuts.CreateDefault());
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to load Dunnage keyboard shortcuts.",
                Enum_ErrorSeverity.Warning,
                ex,
                false
            );
            ApplyShortcutModel(Model_Settings_DunnageShortcuts.CreateDefault());
        }
        finally
        {
            _isLoading = false;
        }
    }

    private Model_Settings_DunnageShortcuts CreateShortcutModel() =>
        new()
        {
            ModeSelectionShortcut = ModeSelectionShortcut.Clone(),
            ClearLabelDataShortcut = ClearLabelDataShortcut.Clone(),
            NextStepShortcut = NextStepShortcut.Clone(),
            BackStepShortcut = BackStepShortcut.Clone(),
            HelpShortcut = HelpShortcut.Clone(),
            IsToggleSimpleNavigationEnabled = IsToggleSimpleNavigationEnabled,
        };

    private async Task PersistToggleSettingAsync(bool value)
    {
        try
        {
            _isPersistingToggle = true;
            await _dunnageShortcuts.SaveToggleSimpleNavigationEnabledAsync(value);
            ShowStatus(
                value
                    ? "Ctrl+T simple navigation toggle enabled."
                    : "Ctrl+T simple navigation toggle disabled."
            );
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save the simple navigation toggle setting.",
                Enum_ErrorSeverity.Warning,
                ex,
                false
            );
        }
        finally
        {
            _isPersistingToggle = false;
        }
    }

    private void ApplyShortcutModel(Model_Settings_DunnageShortcuts shortcuts)
    {
        _isLoading = true;
        ModeSelectionShortcut = shortcuts.ModeSelectionShortcut.Clone();
        ClearLabelDataShortcut = shortcuts.ClearLabelDataShortcut.Clone();
        NextStepShortcut = shortcuts.NextStepShortcut.Clone();
        BackStepShortcut = shortcuts.BackStepShortcut.Clone();
        HelpShortcut = shortcuts.HelpShortcut.Clone();
        IsToggleSimpleNavigationEnabled = shortcuts.IsToggleSimpleNavigationEnabled;
        RefreshNavigationShortcutText();
        _isLoading = false;
    }

    private void ReplaceBindingSubscription(
        Model_KeyboardShortcutBinding? oldValue,
        Model_KeyboardShortcutBinding? newValue
    )
    {
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= ShortcutBinding_PropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += ShortcutBinding_PropertyChanged;
        }
    }

    private void ShortcutBinding_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _ = e;
        RefreshNavigationShortcutText();
    }

    private void RefreshNavigationShortcutText()
    {
        NextStepCurrentShortcutText = Helper_KeyboardShortcuts.ToDisplayText(NextStepShortcut);
        BackStepCurrentShortcutText = Helper_KeyboardShortcuts.ToDisplayText(BackStepShortcut);

        SimpleNavigationNoteText = IsToggleSimpleNavigationEnabled
            ? "When Ctrl+T is active during the workflow, Next Step switches to Right Arrow and Back Step switches to Left Arrow until Ctrl+T is pressed again."
            : "Ctrl+T is disabled, so Next Step and Back Step stay on the configured shortcuts shown above.";
    }
}

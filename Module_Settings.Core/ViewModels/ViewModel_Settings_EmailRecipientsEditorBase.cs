using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Settings.Core.Helpers;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;
using MTM_Receiving_Application.Module_Settings.Core.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Core.ViewModels;

public abstract partial class ViewModel_Settings_EmailRecipientsEditorBase : ViewModel_Shared_Base
{
    private readonly IService_EmailRecipientSettingsOperations _recipientSettingsService;
    private bool _suppressEmailCustomizationTracking;
    private bool _isEmailCustomized;

    [ObservableProperty]
    private ObservableCollection<Model_EmailRecipientSetting> _toRecipients = [];

    [ObservableProperty]
    private ObservableCollection<Model_EmailRecipientSetting> _ccRecipients = [];

    [ObservableProperty]
    private int _editingRecipientId;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _recipientType = "To";

    protected ViewModel_Settings_EmailRecipientsEditorBase(
        IService_EmailRecipientSettingsOperations recipientSettingsService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _recipientSettingsService =
            recipientSettingsService
            ?? throw new ArgumentNullException(nameof(recipientSettingsService));
        StatusMessage = "Loading recipients...";
        _ = LoadRecipientsAsync();
    }

    public IReadOnlyList<string> RecipientTypeOptions { get; } = ["To", "CC"];

    public bool HasRecipients => ToRecipients.Count > 0 || CcRecipients.Count > 0;

    public bool IsEditing => EditingRecipientId > 0;

    public int ToRecipientCount => ToRecipients.Count;

    public int CcRecipientCount => CcRecipients.Count;

    public string EditorTitle => IsEditing ? "Edit Recipient" : "Add Recipient";

    public string SaveButtonText => IsEditing ? "Save Changes" : "Add Recipient";

    partial void OnFirstNameChanged(string value)
    {
        UpdateEmailIfUsingDefault();
        SaveRecipientCommand.NotifyCanExecuteChanged();
    }

    partial void OnLastNameChanged(string value)
    {
        UpdateEmailIfUsingDefault();
        SaveRecipientCommand.NotifyCanExecuteChanged();
    }

    partial void OnEmailChanged(string value)
    {
        if (!_suppressEmailCustomizationTracking)
        {
            _isEmailCustomized = !string.Equals(
                value?.Trim(),
                Helper_EmailRecipientFormatting.GenerateDefaultEmail(FirstName, LastName),
                StringComparison.OrdinalIgnoreCase
            );
        }

        SaveRecipientCommand.NotifyCanExecuteChanged();
    }

    partial void OnRecipientTypeChanged(string value)
    {
        SaveRecipientCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private async Task RefreshRecipientsAsync()
    {
        await LoadRecipientsAsync();
    }

    [RelayCommand]
    private void StartNewRecipient()
    {
        ResetEditor();
        ShowStatus("Ready to add a new recipient.");
    }

    [RelayCommand(CanExecute = nameof(CanSaveRecipient))]
    private async Task SaveRecipientAsync()
    {
        if (!CanSaveRecipient())
        {
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus(IsEditing ? "Saving recipient changes..." : "Adding recipient...");

            var recipient = new Model_EmailRecipientSetting
            {
                Id = EditingRecipientId,
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                RecipientType = NormalizeRecipientType(RecipientType),
                Email = Email.Trim(),
            };

            var result = await _recipientSettingsService.SaveRecipientAsync(recipient);
            if (!result.IsSuccess)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "Save recipient");
                return;
            }

            await LoadRecipientsAsync();
            ResetEditor();
            ShowStatus("Recipient saved.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to save the recipient.",
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
    private void EditRecipient(Model_EmailRecipientSetting? recipient)
    {
        if (recipient is null)
        {
            return;
        }

        _suppressEmailCustomizationTracking = true;
        try
        {
            EditingRecipientId = recipient.Id;
            FirstName = recipient.FirstName;
            LastName = recipient.LastName;
            RecipientType = NormalizeRecipientType(recipient.RecipientType);
            Email = recipient.Email;
        }
        finally
        {
            _suppressEmailCustomizationTracking = false;
        }

        _isEmailCustomized = !string.Equals(
            Email,
            Helper_EmailRecipientFormatting.GenerateDefaultEmail(FirstName, LastName),
            StringComparison.OrdinalIgnoreCase
        );

        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        ShowStatus($"Editing {recipient.FullName}.");
    }

    [RelayCommand]
    private async Task DeleteRecipientAsync(Model_EmailRecipientSetting? recipient)
    {
        if (recipient is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ShowStatus($"Removing {recipient.FullName}...");

            var result = await _recipientSettingsService.DeleteRecipientAsync(recipient.Id);
            if (!result.IsSuccess)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "Delete recipient");
                return;
            }

            await LoadRecipientsAsync();
            if (EditingRecipientId == recipient.Id)
            {
                ResetEditor();
            }

            ShowStatus("Recipient deleted.", InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to delete the recipient.",
                Enum_ErrorSeverity.Error,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSaveRecipient()
    {
        return !IsBusy
            && !string.IsNullOrWhiteSpace(FirstName)
            && !string.IsNullOrWhiteSpace(LastName)
            && !string.IsNullOrWhiteSpace(Email)
            && Email.Contains('@', StringComparison.Ordinal)
            && (
                string.Equals(RecipientType, "To", StringComparison.OrdinalIgnoreCase)
                || string.Equals(RecipientType, "CC", StringComparison.OrdinalIgnoreCase)
            );
    }

    private async Task LoadRecipientsAsync()
    {
        try
        {
            IsBusy = true;
            var result = await _recipientSettingsService.GetRecipientsAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                await _errorHandler.HandleDaoErrorAsync(result, "Load recipients");
                return;
            }

            ToRecipients = new ObservableCollection<Model_EmailRecipientSetting>(
                result.Data.Where(recipient =>
                    string.Equals(recipient.RecipientType, "To", StringComparison.OrdinalIgnoreCase)
                )
            );
            CcRecipients = new ObservableCollection<Model_EmailRecipientSetting>(
                result.Data.Where(recipient =>
                    string.Equals(recipient.RecipientType, "CC", StringComparison.OrdinalIgnoreCase)
                )
            );

            OnPropertyChanged(nameof(HasRecipients));
            OnPropertyChanged(nameof(ToRecipientCount));
            OnPropertyChanged(nameof(CcRecipientCount));

            StatusMessage = HasRecipients ? "Recipients loaded." : "No recipients saved yet.";
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to load recipients.",
                Enum_ErrorSeverity.Error,
                ex
            );
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateEmailIfUsingDefault()
    {
        if (_isEmailCustomized)
        {
            return;
        }

        var generatedEmail = Helper_EmailRecipientFormatting.GenerateDefaultEmail(
            FirstName,
            LastName
        );

        _suppressEmailCustomizationTracking = true;
        try
        {
            Email = generatedEmail;
        }
        finally
        {
            _suppressEmailCustomizationTracking = false;
        }
    }

    private void ResetEditor()
    {
        _suppressEmailCustomizationTracking = true;
        try
        {
            EditingRecipientId = 0;
            FirstName = string.Empty;
            LastName = string.Empty;
            RecipientType = "To";
            Email = string.Empty;
        }
        finally
        {
            _suppressEmailCustomizationTracking = false;
        }

        _isEmailCustomized = false;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(EditorTitle));
        OnPropertyChanged(nameof(SaveButtonText));
        SaveRecipientCommand.NotifyCanExecuteChanged();
    }

    private static string NormalizeRecipientType(string value)
    {
        return string.Equals(value, "CC", StringComparison.OrdinalIgnoreCase) ? "CC" : "To";
    }
}

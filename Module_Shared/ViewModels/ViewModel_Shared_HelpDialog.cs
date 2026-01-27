using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Shared.ViewModels;

/// <summary>
/// ViewModel for the centralized help dialog
/// </summary>
public partial class ViewModel_Shared_HelpDialog : ViewModel_Shared_Base
{
    private readonly IService_Help _helpService;

    [ObservableProperty]
    private Model_HelpContent? _helpContent;

    [ObservableProperty]
    private bool _isRelatedHelpAvailable;

    [ObservableProperty]
    private ObservableCollection<Model_HelpContent> _relateDataTransferObjectspics = new();

    [ObservableProperty]
    private bool _canDismiss;

    [ObservableProperty]
    private bool _isDismissed;

    public ViewModel_Shared_HelpDialog(
        IService_Help helpService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _helpService = helpService;
    }

    /// <summary>
    /// Loads help content and related topics
    /// </summary>
    /// <param name="content">The help content to load</param>
    public async Task LoadHelpContentAsync(Model_HelpContent content)
    {
        try
        {
            HelpContent = content;
            CanDismiss = content.HelpType == Module_Core.Models.Enums.Enum_HelpType.Tip;

            // Check if dismissed
            if (CanDismiss && content.Key != null)
            {
                IsDismissed = await _helpService.IsDismissedAsync(content.Key);
            }

            // Load related topics
            RelateDataTransferObjectspics.Clear();
            if (content.RelatedKeys?.Count > 0)
            {
                foreach (var relatedKey in content.RelatedKeys)
                {
                    var relatedContent = _helpService.GetHelpContent(relatedKey);
                    if (relatedContent != null)
                    {
                        RelateDataTransferObjectspics.Add(relatedContent);
                    }
                }
                IsRelatedHelpAvailable = RelateDataTransferObjectspics.Count > 0;
            }
            else
            {
                IsRelatedHelpAvailable = false;
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Medium,
                nameof(LoadHelpContentAsync),
                nameof(ViewModel_Shared_HelpDialog)
            );
        }
    }

    [RelayCommand]
    private async Task ViewRelateDataTransferObjectspicAsync()
    {
        // This will be called from the ListView click event
        await Task.CompletedTask;
    }

    public async Task LoadRelateDataTransferObjectspicAsync(Model_HelpContent? relatedContent)
    {
        if (relatedContent != null)
        {
            await LoadHelpContentAsync(relatedContent);
        }
    }

    [RelayCommand]
    private async Task CopyContentAsync()
    {
        try
        {
            if (HelpContent != null && !string.IsNullOrEmpty(HelpContent.Content))
            {
                var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
                dataPackage.SetText(HelpContent.Content);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);

                StatusMessage = "Content copied to clipboard";
                await _logger.LogInfoAsync("Help content copied to clipboard");
            }
        }
        catch (Exception ex)
        {
            _errorHandler.HandleException(
                ex,
                Module_Core.Models.Enums.Enum_ErrorSeverity.Low,
                nameof(CopyContentAsync),
                nameof(ViewModel_Shared_HelpDialog)
            );
        }
    }

    partial void OnIsDismissedChanged(bool value)
    {
        if (HelpContent != null && !string.IsNullOrEmpty(HelpContent.Key))
        {
            _ = _helpService.SetDismissedAsync(HelpContent.Key, value);
        }
    }
}


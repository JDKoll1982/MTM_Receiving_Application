using System;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Shared.ViewModels;

/// <summary>
/// Base ViewModel providing common functionality for all ViewModels
/// </summary>
public abstract partial class ViewModel_Shared_Base : ObservableObject
{
    protected readonly IService_ErrorHandler _errorHandler;
    protected readonly IService_LoggingUtility _logger;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;

    [ObservableProperty]
    private bool _isStatusOpen;

    [ObservableProperty]
    private string _title = string.Empty;

    private readonly IService_Notification _notificationService;

    protected ViewModel_Shared_Base(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
    {
        ArgumentNullException.ThrowIfNull(errorHandler);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(notificationService);

        _errorHandler = errorHandler;
        _logger = logger;
        _notificationService = notificationService;
    }

    public void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        // Update global notification
        _notificationService.ShowStatus(message, severity);

        // Update local properties
        StatusMessage = message;
        StatusSeverity = severity;
        IsStatusOpen = true;
    }
}


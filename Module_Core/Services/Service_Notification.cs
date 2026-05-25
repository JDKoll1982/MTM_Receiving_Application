using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Core.Services;

public partial class Service_Notification : ObservableObject, IService_Notification
{
    private readonly IService_Dispatcher _dispatcher;
    private Func<Task>? _statusAction;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;

    [ObservableProperty]
    private bool _isStatusOpen;

    [ObservableProperty]
    private string _statusActionLabel = string.Empty;

    [ObservableProperty]
    private bool _isStatusActionVisible;

    public Service_Notification(IService_Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        ClearStatusAction();
        StatusMessage = message;
        StatusSeverity = severity;
        IsStatusOpen = true;

        // Auto-dismiss after 5 seconds if informational or success
        if (severity == InfoBarSeverity.Informational || severity == InfoBarSeverity.Success)
        {
            Task.Delay(5000)
                .ContinueWith(_ =>
                {
                    _dispatcher.TryEnqueue(() =>
                    {
                        IsStatusOpen = false;
                    });
                });
        }
    }

    public void ShowStatusWithAction(
        string message,
        InfoBarSeverity severity,
        string actionLabel,
        Func<Task> action
    )
    {
        ArgumentNullException.ThrowIfNull(action);

        StatusMessage = message;
        StatusSeverity = severity;
        StatusActionLabel = actionLabel;
        IsStatusActionVisible = string.IsNullOrWhiteSpace(actionLabel) is false;
        _statusAction = action;
        IsStatusOpen = true;
    }

    public async Task ExecuteStatusActionAsync()
    {
        if (_statusAction is null)
        {
            return;
        }

        await _statusAction();
    }

    public void ClearStatusAction()
    {
        _statusAction = null;
        StatusActionLabel = string.Empty;
        IsStatusActionVisible = false;
    }

    partial void OnIsStatusOpenChanged(bool value)
    {
        if (!value)
        {
            ClearStatusAction();
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using System.Threading.Tasks;

namespace MTM_Receiving_Application.Module_Core.Services;

public partial class Service_Notification : ObservableObject, IService_Notification
{
    private readonly IService_Dispatcher _dispatcher;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private InfoBarSeverity _statusSeverity = InfoBarSeverity.Informational;

    [ObservableProperty]
    private bool _isStatusOpen;

    public Service_Notification(IService_Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        StatusMessage = message;
        StatusSeverity = severity;
        IsStatusOpen = true;

        // Auto-dismiss after 5 seconds if informational or success
        if (severity == InfoBarSeverity.Informational || severity == InfoBarSeverity.Success)
        {
            Task.Delay(5000).ContinueWith(_ =>
            {
                _dispatcher.TryEnqueue(() =>
                {
                    IsStatusOpen = false;
                });
            });
        }
    }
}

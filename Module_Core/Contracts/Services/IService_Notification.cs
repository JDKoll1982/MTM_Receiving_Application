using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

public interface IService_Notification : INotifyPropertyChanged
{
    public string StatusMessage { get; }
    public InfoBarSeverity StatusSeverity { get; }
    public bool IsStatusOpen { get; set; }
    public string StatusActionLabel { get; }
    public bool IsStatusActionVisible { get; }

    public void ShowStatus(
        string message,
        InfoBarSeverity severity = InfoBarSeverity.Informational
    );

    public void ShowStatusWithAction(
        string message,
        InfoBarSeverity severity,
        string actionLabel,
        Func<Task> action
    );

    public Task ExecuteStatusActionAsync();

    public void ClearStatusAction();
}

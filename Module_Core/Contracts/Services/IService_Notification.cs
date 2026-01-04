using System.ComponentModel;
using MTM_Receiving_Application.Module_Core.Models.Enums;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services;

public interface IService_Notification : INotifyPropertyChanged
{
    public string StatusMessage { get; }
    public InfoBarSeverity StatusSeverity { get; }
    public bool IsStatusOpen { get; set; }

    public void ShowStatus(string message, InfoBarSeverity severity = InfoBarSeverity.Informational);
}

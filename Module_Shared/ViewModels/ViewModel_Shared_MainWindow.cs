using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Systems;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace MTM_Receiving_Application.Module_Shared.ViewModels;

public partial class ViewModel_Shared_MainWindow : ViewModel_Shared_Base
{
    private readonly IService_UserSessionManager _sessionManager;
    public IService_Notification NotificationService { get; }

    [ObservableProperty]
    private string _userDisplayText = "Not Logged In";

    public ViewModel_Shared_MainWindow(
        IService_UserSessionManager sessionManager,
        IService_Notification notificationService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _sessionManager = sessionManager;
        NotificationService = notificationService;
        NotificationService.PropertyChanged += OnNotificationServicePropertyChanged;

        if (_sessionManager.CurrentSession != null)
        {
            UpdateUserDisplay(_sessionManager.CurrentSession.User);
        }
    }

    private void OnNotificationServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IService_Notification.StatusSeverity))
        {
            OnPropertyChanged(nameof(WinUIStatusSeverity));
        }
    }

    public InfoBarSeverity WinUIStatusSeverity
    {
        get
        {
            return NotificationService.StatusSeverity switch
            {
                Module_Core.Models.Enums.InfoBarSeverity.Informational => InfoBarSeverity.Informational,
                Module_Core.Models.Enums.InfoBarSeverity.Success => InfoBarSeverity.Success,
                Module_Core.Models.Enums.InfoBarSeverity.Warning => InfoBarSeverity.Warning,
                Module_Core.Models.Enums.InfoBarSeverity.Error => InfoBarSeverity.Error,
                _ => InfoBarSeverity.Informational
            };
        }
    }

    private void UpdateUserDisplay(Model_User user)
    {
        UserDisplayText = user.DisplayName;
    }
}


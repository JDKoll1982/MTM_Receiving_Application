using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Shared.ViewModels;

public partial class ViewModel_Shared_MainWindow : ViewModel_Shared_Base
{
    private readonly IService_UserSessionManager _sessionManager;
    private readonly IService_UserLoginCoordinator _userLoginCoordinator;
    public IService_Notification NotificationService { get; }

    [ObservableProperty]
    private string _userDisplayText = "Not Logged In";

    public ViewModel_Shared_MainWindow(
        IService_UserSessionManager sessionManager,
        IService_UserLoginCoordinator userLoginCoordinator,
        IService_Notification notificationService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger
    )
        : base(errorHandler, logger, notificationService)
    {
        _sessionManager = sessionManager;
        _userLoginCoordinator = userLoginCoordinator;
        NotificationService = notificationService;
        NotificationService.PropertyChanged += OnNotificationServicePropertyChanged;

        if (_sessionManager.CurrentSession?.User != null)
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
                Module_Core.Models.Enums.InfoBarSeverity.Informational =>
                    InfoBarSeverity.Informational,
                Module_Core.Models.Enums.InfoBarSeverity.Success => InfoBarSeverity.Success,
                Module_Core.Models.Enums.InfoBarSeverity.Warning => InfoBarSeverity.Warning,
                Module_Core.Models.Enums.InfoBarSeverity.Error => InfoBarSeverity.Error,
                _ => InfoBarSeverity.Informational,
            };
        }
    }

    private void UpdateUserDisplay(Model_User user)
    {
        UserDisplayText = user.DisplayName;
    }

    public async Task<bool> LogOutAndPromptForLoginAsync()
    {
        if (IsBusy)
        {
            return false;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Signing out...";

            var result = await _userLoginCoordinator.LogOutAndPromptForLoginAsync(
                "user_requested_logout"
            );

            if (!result.Success || result.Data?.User == null)
            {
                StatusMessage = result.ErrorMessage ?? "Unable to complete the login flow.";
                return false;
            }

            UpdateUserDisplay(result.Data.User);
            StatusMessage = $"Signed in as {result.Data.User.FullName}";
            return true;
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(
                "Failed to switch users.",
                Module_Core.Models.Enums.Enum_ErrorSeverity.Error,
                ex,
                false
            );

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

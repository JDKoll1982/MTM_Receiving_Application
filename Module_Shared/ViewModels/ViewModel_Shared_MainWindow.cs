using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Systems;

namespace MTM_Receiving_Application.Module_Shared.ViewModels;

public partial class ViewModel_Shared_MainWindow : ViewModel_Shared_Base
{
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private string _userDisplayText = "Not Logged In";

    public ViewModel_Shared_MainWindow(
        IService_UserSessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger)
        : base(errorHandler, logger)
    {
        _sessionManager = sessionManager;

        if (_sessionManager.CurrentSession != null)
        {
            UpdateUserDisplay(_sessionManager.CurrentSession.User);
        }
    }

    private void UpdateUserDisplay(Model_User user)
    {
        UserDisplayText = user.DisplayName;
    }
}


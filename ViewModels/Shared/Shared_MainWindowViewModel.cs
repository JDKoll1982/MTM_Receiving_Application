using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models.Systems;

namespace MTM_Receiving_Application.ViewModels.Shared;

public partial class Shared_MainWindowViewModel : Shared_BaseViewModel
{
    private readonly IService_UserSessionManager _sessionManager;

    [ObservableProperty]
    private string _userDisplayText = "Not Logged In";

    public Shared_MainWindowViewModel(
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

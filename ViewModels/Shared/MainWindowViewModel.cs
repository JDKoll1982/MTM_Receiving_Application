using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Contracts.Services;
using MTM_Receiving_Application.Models;

namespace MTM_Receiving_Application.ViewModels.Shared;

public partial class MainWindowViewModel : BaseViewModel
{
    private readonly IService_SessionManager _sessionManager;

    [ObservableProperty]
    private string _userDisplayText = "Not Logged In";

    public MainWindowViewModel(
        IService_SessionManager sessionManager,
        IService_ErrorHandler errorHandler,
        ILoggingService logger) 
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

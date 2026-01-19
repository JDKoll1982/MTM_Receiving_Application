using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Settings.Volvo.ViewModels;

public sealed partial class ViewModel_Settings_Volvo_FilePaths : ViewModel_Shared_Base
{
    public ViewModel_Settings_Volvo_FilePaths(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger) : base(errorHandler, logger)
    {
        Title = "Volvo File Paths";
    }
}

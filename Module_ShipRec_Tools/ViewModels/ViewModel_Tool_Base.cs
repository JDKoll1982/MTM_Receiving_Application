using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.ViewModels;

/// <summary>
/// Base class for all tool ViewModels in the ShipRec Tools module.
/// Inherits common infrastructure (IsBusy, StatusMessage, error handling) from ViewModel_Shared_Base.
/// </summary>
public abstract partial class ViewModel_Tool_Base : ViewModel_Shared_Base
{
    /// <summary>
    /// Display name for the tool, shown in headers when the tool is active.
    /// </summary>
    [ObservableProperty]
    private string _toolTitle = string.Empty;

    /// <summary>
    /// Short description visible in the active tool's header area.
    /// </summary>
    [ObservableProperty]
    private string _toolDescription = string.Empty;

    protected ViewModel_Tool_Base(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService)
        : base(errorHandler, logger, notificationService)
    {
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Reprint;
using MTM_Receiving_Application.Module_Receiving.Contracts;
using MTM_Receiving_Application.Module_Reprint.Models;
using MTM_Receiving_Application.Module_Reprint.Settings;
using MTM_Receiving_Application.Module_Settings.Core.Interfaces;

namespace MTM_Receiving_Application.Module_Reprint.ViewModels;

/// <summary>
/// Receiving mode of the Reprint Labels page.
/// </summary>
public partial class ViewModel_Reprint_Receiving : ViewModel_Reprint_ModuleBase
{
    private readonly IService_Reprint_Receiving _reprintService;

    protected override string SearchBySettingsKey =>
        ReprintSettingsKeys.UserPreferences.ReceivingSearchBy;

    protected override string ColumnOptionsSettingsKey =>
        ReprintSettingsKeys.UserPreferences.ReceivingColumns;

    protected override string ModuleDisplayName => "Receiving";

    public ViewModel_Reprint_Receiving(
        IService_Reprint_Receiving reprintService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService,
        IService_SettingsCoreFacade settingsCore,
        IService_UserSessionManager sessionManager
    )
        : base(errorHandler, logger, notificationService, settingsCore, sessionManager)
    {
        _reprintService = reprintService;
    }

    protected override IReadOnlyList<Model_ReprintSearchByOption> BuildSearchByOptions() =>
    [
        new() { Key = "part", Label = "Part Number" },
        new() { Key = "po", Label = "PO Number" },
        new() { Key = "description", Label = "Part Description" },
        new() { Key = "vendor", Label = "Vendor" },
        new() { Key = "heat", Label = "Heat / Lot" },
    ];

    protected override Task<Model_Dao_Result<List<Model_ReprintHistoryRow>>> LoadHistoryAsync(
        Model_ReprintHistoryFilter filter
    ) => _reprintService.GetReprintHistoryAsync(filter);

    protected override Task<Model_Dao_Result<Model_ReprintBatchResult>> ExecuteReprintAsync(
        IReadOnlyList<string> historyIds
    ) => _reprintService.ReprintAsync(historyIds);
}

using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Contracts.Services;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Services.CustomerPullPack;

public sealed class Service_CustomerPullPackDataSourceResolver
    : IService_CustomerPullPackDataSourceResolver
{
    private readonly IService_AppSettings _appSettings;
    private readonly IService_LoggingUtility? _logger;
    private bool _isResolved;

    public Service_CustomerPullPackDataSourceResolver(
        IService_AppSettings appSettings,
        IService_LoggingUtility? logger = null
    )
    {
        _appSettings = appSettings;
        _logger = logger;
    }

    public Enum_CustomerPullPackDataSourceMode CurrentMode { get; private set; } =
        Enum_CustomerPullPackDataSourceMode.LiveVisual;

    public bool IsMockMode => CurrentMode == Enum_CustomerPullPackDataSourceMode.MockCatalog;

    public void ResolveForWorkflow()
    {
        if (_isResolved)
        {
            return;
        }

        CurrentMode = _appSettings.GetUseInforVisualMockData()
            ? Enum_CustomerPullPackDataSourceMode.MockCatalog
            : Enum_CustomerPullPackDataSourceMode.LiveVisual;
        _isResolved = true;

        _logger?.LogInfo($"Customer Pull n' Pack data source resolved to '{CurrentMode}'.");
    }
}

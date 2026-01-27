using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels.Wizard.Step1;

/// <summary>
/// Displays summary of Step 1 selections (PO, Part, Load Count).
/// Shown as read-only reference in Steps 2 and 3.
/// </summary>
public partial class ViewModel_Receiving_Wizard_Display_Step1Summary : ViewModel_Shared_Base
{
    #region Observable Properties

    /// <summary>
    /// PO Number entered in Step 1.
    /// </summary>
    [ObservableProperty]
    private string _poNumber = string.Empty;

    /// <summary>
    /// Part Number selected in Step 1.
    /// </summary>
    [ObservableProperty]
    private string _partNumber = string.Empty;

    /// <summary>
    /// Load count entered in Step 1.
    /// </summary>
    [ObservableProperty]
    private int _loadCount = 0;

    /// <summary>
    /// Indicates if Non-PO mode is active.
    /// </summary>
    [ObservableProperty]
    private bool _isNonPo = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the Step 1 Summary ViewModel.
    /// </summary>
    public ViewModel_Receiving_Wizard_Display_Step1Summary(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService) : base(errorHandler, logger, notificationService)
    {
        _logger.LogInfo("Step 1 Summary ViewModel initialized");
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates summary with Step 1 data.
    /// </summary>
    public void UpdateSummary(string poNumber, string partNumber, int loadCount, bool isNonPo)
    {
        _poNumber = poNumber;
        _partNumber = partNumber;
        _loadCount = loadCount;
        _isNonPo = isNonPo;

        // Trigger property change notifications for computed properties
        OnPropertyChanged(nameof(PoNumberDisplay));
        OnPropertyChanged(nameof(PartNumberDisplay));
        OnPropertyChanged(nameof(LoadCountDisplay));
    }

    #endregion

    #region Computed Display Properties (defined AFTER backing fields)

    /// <summary>
    /// Formatted PO Number display (shows "NON-PO" if Non-PO mode).
    /// </summary>
    public string PoNumberDisplay => _isNonPo ? "NON-PO" : (string.IsNullOrWhiteSpace(_poNumber) ? "Not entered" : _poNumber);

    /// <summary>
    /// Formatted Part Number display.
    /// </summary>
    public string PartNumberDisplay => string.IsNullOrWhiteSpace(_partNumber) ? "Not selected" : _partNumber;

    /// <summary>
    /// Formatted Load Count display.
    /// </summary>
    public string LoadCountDisplay => _loadCount > 0 ? $"{_loadCount} Load(s)" : "Not entered";

    #endregion
}

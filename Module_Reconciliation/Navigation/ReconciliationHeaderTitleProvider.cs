using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;

namespace MTM_Receiving_Application.Module_Reconciliation.Navigation;

/// <summary>
/// Supplies the module's header title so no view or view model writes one itself
/// (FR-062, R-HDR-01).
/// </summary>
public sealed partial class ReconciliationHeaderTitleProvider : ObservableObject,
    IViewModel_HeaderTitleProvider
{
    private const string ModuleTitle = "Receiving Validator";
    private const string ModuleContext = "from Infor Visual";

    /// <inheritdoc />
    public string CurrentHeaderTitle => ModuleTitle;

    /// <inheritdoc />
    public string? CurrentHeaderContextSubtitle => ModuleContext;
}

using System;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Contracts.ViewModels;
using MTM_Receiving_Application.Module_Reconciliation.Navigation;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Reconciliation.ViewModels;

/// <summary>
/// The module's launcher. It owns the mode cards and opens the working page, which
/// is the same shape the other modules use for their mode selection (FR-062, UI-01,
/// decision D-07). It is also the header title provider the shell resolves from the
/// page, so the title comes through <see cref="ReconciliationHeaderTitleProvider"/>
/// rather than from a literal any view or view model writes itself (R-HDR-01).
/// </summary>
public sealed partial class ViewModel_Reconciliation_Launcher : ViewModel_Shared_Base,
    IViewModel_HeaderTitleProvider
{
    private readonly ReconciliationHeaderTitleProvider _headerTitleProvider;

    /// <summary>Creates the launcher.</summary>
    /// <param name="headerTitleProvider">The module's header title provider.</param>
    /// <param name="errorHandler">The shared error handler.</param>
    /// <param name="logger">The shared logging utility.</param>
    /// <param name="notificationService">The shared notification service.</param>
    public ViewModel_Reconciliation_Launcher(
        ReconciliationHeaderTitleProvider headerTitleProvider,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        ArgumentNullException.ThrowIfNull(headerTitleProvider);

        _headerTitleProvider = headerTitleProvider;
        Title = headerTitleProvider.CurrentHeaderTitle;
    }

    /// <summary>Raised when the operator chooses to work a day.</summary>
    public event EventHandler? WorkAReceivingDayRequested;

    /// <inheritdoc />
    public string CurrentHeaderTitle => _headerTitleProvider.CurrentHeaderTitle;

    /// <inheritdoc />
    public string? CurrentHeaderContextSubtitle =>
        _headerTitleProvider.CurrentHeaderContextSubtitle;

    /// <summary>The one mode the module offers today.</summary>
    public string CardTitle => "Work a Receiving Day";

    /// <summary>What that mode does.</summary>
    public string CardDescription =>
        "Pick a day, review the proposed destinations, and accept only the lines the quantity proof can account for.";

    /// <summary>The accessible name of the mode card.</summary>
    public string CardAccessibilityName => "Work a Receiving Day with the Receiving Validator";

    /// <summary>Opens the working page.</summary>
    [RelayCommand]
    private void OpenWorkingDay() => WorkAReceivingDayRequested?.Invoke(this, EventArgs.Empty);
}

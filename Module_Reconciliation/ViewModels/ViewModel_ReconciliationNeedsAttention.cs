using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Core.Models.Core;
using MTM_Receiving_Application.Module_Core.Models.Enums;
using MTM_Receiving_Application.Module_Reconciliation.Contracts;
using MTM_Receiving_Application.Module_Reconciliation.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Reconciliation.ViewModels;

/// <summary>
/// The Needs Attention window's view model. It lists the lines the module could not
/// answer, states each one's reason, evidence and raw difference, and writes a
/// hand-set location through the same accepted-line path (FR-046, FR-047).
/// </summary>
public sealed partial class ViewModel_ReconciliationNeedsAttention : ViewModel_Shared_Base
{
    private readonly IService_ReconciliationWriteService _writeService;

    /// <summary>Creates the attention view model.</summary>
    public ViewModel_ReconciliationNeedsAttention(
        IService_ReconciliationWriteService writeService,
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService)
    {
        _writeService = writeService;
        Title = "Needs Attention";
    }

    /// <summary>One row per line that still needs a destination.</summary>
    public ObservableCollection<Model_ReconciliationAttentionItem> Items { get; } = [];

    /// <summary>How many lines are still unresolved.</summary>
    [ObservableProperty]
    private int _unresolvedCount;

    /// <summary>The authenticated operator recorded on every write.</summary>
    public string OperatorUserId { get; set; } = Environment.UserName;

    /// <summary>Fills the window with the day's unresolved lines.</summary>
    /// <param name="findings">Every finding the working day produced.</param>
    public void Load(IReadOnlyList<Model_ReconciliationFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);

        Items.Clear();

        foreach (Model_ReconciliationFinding finding in findings.Where(IsUnresolved))
        {
            var item = new Model_ReconciliationAttentionItem { Finding = finding };
            item.SubmitCommand = new AsyncRelayCommand(() => SubmitAsync(item));
            Items.Add(item);
        }

        UnresolvedCount = Items.Count;
    }

    /// <summary>
    /// Writes one line's typed location through the accepted-line path and retires
    /// the row when the write succeeds.
    /// </summary>
    /// <param name="item">The row whose location was typed.</param>
    public async Task SubmitAsync(Model_ReconciliationAttentionItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Model_Dao_Result<Model_ReconciliationFinding> result = await _writeService.AcceptAsync(
            item.Finding,
            item.LocationInput,
            OperatorUserId
        );

        if (result.IsSuccess is false)
        {
            ShowStatus(result.ErrorMessage, InfoBarSeverity.Error);
            return;
        }

        Items.Remove(item);
        UnresolvedCount = Items.Count;
        ShowStatus("The location was set and written.", InfoBarSeverity.Success);
    }

    private static bool IsUnresolved(Model_ReconciliationFinding finding) =>
        finding.Outcome
            is Enum_ReconciliationOutcome.ExtraSurplus
                or Enum_ReconciliationOutcome.NotEnough;
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MTM_Receiving_Application.Module_Core.Contracts.Services;
using MTM_Receiving_Application.Module_Receiving.Models;
using MTM_Receiving_Application.Module_Shared.ViewModels;

namespace MTM_Receiving_Application.Module_Receiving.ViewModels;

/// <summary>
/// Base for the guided receiving steps. Every step shows the same receipt header card, so the
/// receipt's lot markings are held here once instead of being repeated in each step view model.
/// </summary>
/// <remarks>
/// A marking is read off the already-loaded receipt lines. Nothing here resolves or changes one,
/// because a marking is decided once on the save path and then only ever read.
/// </remarks>
public abstract partial class ViewModel_Receiving_StepBase : ViewModel_Shared_Base
{
    /// <summary>
    /// The receipt's distinct lot markings, one entry per shape and color pair, shape first.
    /// Empty when no line on the receipt is marked, which is the normal case.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasReceiptLotMarkings))]
    [NotifyPropertyChangedFor(nameof(ReceiptLotMarkingsText))]
    private ObservableCollection<string> _receiptLotMarkings = new();

    /// <summary>True when the receipt carries at least one marking, which shows the header row.</summary>
    public bool HasReceiptLotMarkings => ReceiptLotMarkings.Count > 0;

    /// <summary>The markings as one line, for the header card to show without a list template.</summary>
    public string ReceiptLotMarkingsText => string.Join(", ", ReceiptLotMarkings);

    protected ViewModel_Receiving_StepBase(
        IService_ErrorHandler errorHandler,
        IService_LoggingUtility logger,
        IService_Notification notificationService
    )
        : base(errorHandler, logger, notificationService) { }

    /// <summary>
    /// Rebuilds the receipt's markings from the loads. A row missing either half of its marking is
    /// skipped, so an unmarked load contributes nothing rather than a half-empty entry.
    /// </summary>
    protected void RefreshReceiptLotMarkings(IEnumerable<Model_ReceivingLoad>? loads)
    {
        var markings = new List<string>();

        foreach (var load in loads ?? [])
        {
            if (
                string.IsNullOrWhiteSpace(load.HeatShapeCode)
                || string.IsNullOrWhiteSpace(load.HeatColorCode)
            )
            {
                continue;
            }

            var marking = $"{load.HeatShapeCode} {load.HeatColorCode}";
            if (!markings.Contains(marking))
            {
                markings.Add(marking);
            }
        }

        ReceiptLotMarkings = new ObservableCollection<string>(markings);
    }
}

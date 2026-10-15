using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using MTM_Receiving_Application.Module_Core.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Enums;
using MTM_Receiving_Application.Module_ShipRec_Tools.Helpers;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Dialogs;

/// <summary>
/// Confirms Update All Lots. The dialog is read-only: it restates the plan, and the only decision is
/// whether to go ahead.
/// </summary>
/// <remarks>
/// The default button is Close, because this rewrites markings on receipts that were already taken and
/// removes lots from the list. A stray Enter must not reach either.
/// </remarks>
public sealed partial class Dialog_LotMarkingRealignConfirm : ContentDialog
{
    private const int MaxListedChanges = 40;

    public Dialog_LotMarkingRealignConfirm(Model_Tool_LotMarkingRealignPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        InitializeComponent();
        Helper_UI_ContentDialogTheme.ApplyTheme(this);

        Title = "Update all lots";

        SummaryText.Text = BuildSummary(preview);
        CreateText.Text = BuildCreateLine(preview);
        RemoveText.Text = BuildRemoveLine(preview);
        SkipText.Text = BuildSkipLine(preview);
        ChangeListText.Text = BuildChangeList(preview);
    }

    private static string BuildSummary(Model_Tool_LotMarkingRealignPreview preview) =>
        preview.RowsToChange == 1
            ? "1 receiving row would change."
            : $"{preview.RowsToChange} receiving rows would change.";

    private static string BuildCreateLine(Model_Tool_LotMarkingRealignPreview preview)
    {
        if (preview.LotsToCreate == 0)
        {
            return string.Empty;
        }

        return preview.LotsToCreate == 1
            ? "1 lot number has no marking yet, so it becomes a lot and is given a shape and color of its own."
            : $"{preview.LotsToCreate} lot numbers have no marking yet, so each becomes a lot and is given a shape and color of its own.";
    }

    private static string BuildRemoveLine(Model_Tool_LotMarkingRealignPreview preview)
    {
        if (preview.LotsToRemove == 0)
        {
            return string.Empty;
        }

        return preview.LotsToRemove == 1
            ? "1 lot no longer appears on any receiving row, so it is removed from the list and its shape and color are freed."
            : $"{preview.LotsToRemove} lots no longer appear on any receiving row, so they are removed from the list and their shapes and colors are freed.";
    }

    private static string BuildSkipLine(Model_Tool_LotMarkingRealignPreview preview)
    {
        if (preview.Skips.Count == 0)
        {
            return string.Empty;
        }

        return preview.Skips.Count == 1
            ? "1 lot is left alone because its marking is protected."
            : $"{preview.Skips.Count} lots are left alone because their markings are protected.";
    }

    private static string BuildChangeList(Model_Tool_LotMarkingRealignPreview preview)
    {
        var lines = new List<string>();

        foreach (var target in preview.Targets)
        {
            lines.Add(DescribeTarget(target));
        }

        foreach (var removal in preview.Removals)
        {
            lines.Add(
                $"{removal.HeatNumber} · {DescribeSupplier(removal.SupplierId)}: removed from the list, freeing {removal.ShapeCode}{removal.ColorCode}."
            );
        }

        if (lines.Count == 0)
        {
            return "Nothing to do. Every marked row already agrees with the list.";
        }

        var builder = new StringBuilder();
        var shown = Math.Min(lines.Count, MaxListedChanges);

        for (var index = 0; index < shown; index++)
        {
            builder.AppendLine(lines[index]);
        }

        if (lines.Count > shown)
        {
            builder.AppendLine($"... and {lines.Count - shown} more.");
        }

        return builder.ToString().TrimEnd();
    }

    private static string DescribeTarget(Model_Tool_LotMarkingRealignTarget target)
    {
        var where = Helper_LotNumberNormalizer.IsBlank(target.Heat)
            ? $"no lot number · {DescribeSupplier(target.SupplierName)}"
            : $"{target.Heat} · {DescribeSupplier(target.SupplierName)}";

        return target.Action switch
        {
            Enum_LotMarkingRealignAction.ClearMarking =>
                $"{where}: the marking comes off {Describe(target.RowCount)}.",
            Enum_LotMarkingRealignAction.CreateLot =>
                $"{where}: becomes a lot and {Describe(target.RowCount)} are given {target.ShapeCode}{target.ColorCode}.",
            _ =>
                $"{where}: {target.PreviousShapeCode}{target.PreviousColorCode} becomes {target.ShapeCode}{target.ColorCode} on {Describe(target.RowCount)}.",
        };
    }

    private static string DescribeSupplier(string supplierName) =>
        string.IsNullOrWhiteSpace(supplierName) ? "no supplier" : supplierName;

    private static string Describe(int rowCount) =>
        rowCount == 1 ? "1 row" : $"{rowCount} rows";
}

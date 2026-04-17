using System;
using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Settings;

/// <summary>
/// Builds settings-aware work-order detail sections for the Material Availability Board modal and print output.
/// </summary>
public static class MaterialAvailabilityWorkOrderDetailBuilder
{
    public static List<Model_Tool_MaterialAvailabilityDetailSection> BuildUiSections(
        Model_Tool_MaterialAvailabilityAssociatedPartRun associatedRun,
        Model_Tool_MaterialAvailabilityFieldSettings settings,
        bool revealLogicOnlyFields
    )
    {
        return BuildSections(
            associatedRun,
            field =>
                settings.UiVisibleFieldIds.Contains(field.Id)
                || (revealLogicOnlyFields && field.IsLogicOnly)
        );
    }

    public static List<Model_Tool_MaterialAvailabilityDetailSection> BuildPrintSections(
        Model_Tool_MaterialAvailabilityAssociatedPartRun associatedRun,
        Model_Tool_MaterialAvailabilityFieldSettings settings
    )
    {
        return BuildSections(
            associatedRun,
            field => settings.PrintVisibleFieldIds.Contains(field.Id)
        );
    }

    private static List<Model_Tool_MaterialAvailabilityDetailSection> BuildSections(
        Model_Tool_MaterialAvailabilityAssociatedPartRun associatedRun,
        Func<Model_Tool_MaterialAvailabilityFieldDefinition, bool> shouldInclude
    )
    {
        ArgumentNullException.ThrowIfNull(associatedRun);
        ArgumentNullException.ThrowIfNull(shouldInclude);

        return MaterialAvailabilityWorkOrderFieldCatalog
            .All.OrderBy(field => field.SortOrder)
            .Select(field => new
            {
                Definition = field,
                Value = ResolveValue(field.Id, associatedRun),
            })
            .Where(item =>
                shouldInclude(item.Definition) && string.IsNullOrWhiteSpace(item.Value) is false
            )
            .GroupBy(item => item.Definition.Category, StringComparer.OrdinalIgnoreCase)
            .Select(group => new Model_Tool_MaterialAvailabilityDetailSection
            {
                Title = group.Key,
                Fields = group
                    .Select(item => new Model_Tool_MaterialAvailabilityDetailField
                    {
                        Id = item.Definition.Id,
                        Label = item.Definition.DisplayName,
                        Value = item.Value,
                        IsLogicOnly = item.Definition.IsLogicOnly,
                    })
                    .ToList(),
            })
            .ToList();
    }

    private static string ResolveValue(
        string fieldId,
        Model_Tool_MaterialAvailabilityAssociatedPartRun associatedRun
    )
    {
        return fieldId switch
        {
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.WorkOrderDisplay =>
                associatedRun.WorkOrderDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.WorkOrderStatus =>
                associatedRun.WorkOrderStatusDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.ParentPartNumber =>
                associatedRun.AssociatedPartNumber,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.ParentPartDescription =>
                associatedRun.AssociatedPartDescription,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.NextDueToRunDate =>
                associatedRun.NextRunDateDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.RequiredDate =>
                associatedRun.RequiredDateDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.WorkOrderStatusEffDate =>
                associatedRun.WorkOrderStatusEffectiveDateDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.ComponentPartNumber =>
                associatedRun.ComponentPartNumber,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.OperationSequence =>
                associatedRun.OperationSequence?.ToString() ?? string.Empty,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.QtyPer =>
                associatedRun.QtyPerDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.FixedQty =>
                associatedRun.FixedQtyDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.CalculatedQty =>
                associatedRun.CalculatedQtyDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.IssuedQty =>
                associatedRun.IssuedQtyDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.AllocatedQty =>
                associatedRun.AllocatedQtyDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.FulfilledQty =>
                associatedRun.FulfilledQtyDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.UsageUnitOfMeasure =>
                string.IsNullOrWhiteSpace(associatedRun.NormalizedUsageUnitOfMeasure)
                    ? associatedRun.UsageUnitOfMeasure
                    : associatedRun.NormalizedUsageUnitOfMeasure,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.ScrapPercent =>
                associatedRun.ScrapPercentDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.OperationType =>
                associatedRun.OperationType,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.ResourceWorkCenter =>
                associatedRun.ResourceId,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.ServiceId =>
                associatedRun.ServiceId,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.OperationWarehouse =>
                associatedRun.OperationWarehouseId,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.RunQtyPerCycle =>
                associatedRun.RunQtyPerCycleDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.SetupRunHours =>
                associatedRun.SetupRunHoursDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.DimensionsTextExpression =>
                associatedRun.DimensionsDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.LengthWidthHeight =>
                associatedRun.LengthWidthHeightDisplay,
            var id when id == MaterialAvailabilityWorkOrderFieldCatalog.DrawingRevision =>
                associatedRun.DrawingRevisionDisplay,
            _ => string.Empty,
        };
    }
}

using System.Collections.Generic;
using System.Linq;
using MTM_Receiving_Application.Module_ShipRec_Tools.Models;

namespace MTM_Receiving_Application.Module_ShipRec_Tools.Settings;

/// <summary>
/// Central catalog of configurable Material Availability work-order detail fields.
/// </summary>
public static class MaterialAvailabilityWorkOrderFieldCatalog
{
    public const string WorkOrderDisplay = "work-order-display";
    public const string WorkOrderStatus = "work-order-status";
    public const string ParentPartNumber = "parent-part-number";
    public const string ParentPartDescription = "parent-part-description";
    public const string NextDueToRunDate = "next-due-to-run-date";
    public const string NextDueDateSource = "next-due-date-source";
    public const string RequiredDate = "required-date";
    public const string WorkOrderStatusEffDate = "work-order-status-effective-date";
    public const string ComponentPartNumber = "component-part-number";
    public const string OperationSequence = "operation-sequence";
    public const string QtyPer = "qty-per";
    public const string FixedQty = "fixed-qty";
    public const string CalculatedQty = "calculated-qty";
    public const string IssuedQty = "issued-qty";
    public const string AllocatedQty = "allocated-qty";
    public const string FulfilledQty = "fulfilled-qty";
    public const string UsageUnitOfMeasure = "usage-unit-of-measure";
    public const string ScrapPercent = "scrap-percent";
    public const string OperationType = "operation-type";
    public const string ResourceWorkCenter = "resource-work-center";
    public const string ServiceId = "service-id";
    public const string OperationWarehouse = "operation-warehouse";
    public const string RunQtyPerCycle = "run-qty-per-cycle";
    public const string SetupRunHours = "setup-run-hours";
    public const string DimensionsTextExpression = "dimensions-text-expression";
    public const string LengthWidthHeight = "length-width-height";
    public const string DrawingRevision = "drawing-revision";

    public static IReadOnlyList<Model_Tool_MaterialAvailabilityFieldDefinition> All { get; } =
    [
        Create(WorkOrderDisplay, "Work Order", "Job Summary", true, true, false, 10),
        Create(WorkOrderStatus, "Work Order Status", "Job Summary", true, true, false, 20),
        Create(
            ParentPartNumber,
            "Parent / Associated Part Number",
            "Job Summary",
            true,
            true,
            false,
            30
        ),
        Create(
            ParentPartDescription,
            "Parent / Associated Part Description",
            "Job Summary",
            true,
            true,
            false,
            40
        ),
        Create(NextDueToRunDate, "Next Due-To-Run Date", "Scheduling", true, true, false, 50),
        Create(NextDueDateSource, "Next Due-Date Source", "Scheduling", true, true, false, 60),
        Create(RequiredDate, "Required Date", "Scheduling", true, true, false, 70),
        Create(
            WorkOrderStatusEffDate,
            "Work Order Status Effective Date",
            "Scheduling",
            true,
            true,
            false,
            80
        ),
        Create(
            ComponentPartNumber,
            "Component / Input Part Number",
            "Requirement / Coil Use",
            true,
            true,
            false,
            90
        ),
        Create(
            OperationSequence,
            "Operation Sequence",
            "Requirement / Coil Use",
            true,
            true,
            false,
            100
        ),
        Create(QtyPer, "Qty Per", "Requirement / Coil Use", false, false, true, 110),
        Create(FixedQty, "Fixed Qty", "Requirement / Coil Use", false, false, true, 120),
        Create(CalculatedQty, "Calculated Qty", "Requirement / Coil Use", false, false, true, 130),
        Create(IssuedQty, "Issued Qty", "Requirement / Coil Use", false, false, true, 140),
        Create(AllocatedQty, "Allocated Qty", "Requirement / Coil Use", false, false, true, 150),
        Create(FulfilledQty, "Fulfilled Qty", "Requirement / Coil Use", false, false, true, 160),
        Create(
            UsageUnitOfMeasure,
            "Usage Unit Of Measure",
            "Requirement / Coil Use",
            false,
            false,
            true,
            170
        ),
        Create(ScrapPercent, "Scrap Percent", "Requirement / Coil Use", false, false, true, 180),
        Create(OperationType, "Operation Type", "Operation Context", true, true, false, 190),
        Create(
            ResourceWorkCenter,
            "Resource / Work Center",
            "Operation Context",
            true,
            true,
            false,
            200
        ),
        Create(
            ServiceId,
            "Service ID / Outside Process",
            "Operation Context",
            true,
            true,
            false,
            210
        ),
        Create(
            OperationWarehouse,
            "Operation Warehouse",
            "Operation Context",
            false,
            false,
            true,
            220
        ),
        Create(RunQtyPerCycle, "Run Qty Per Cycle", "Operation Context", false, false, true, 230),
        Create(
            SetupRunHours,
            "Setup Hours / Run Hours",
            "Operation Context",
            true,
            true,
            false,
            240
        ),
        Create(
            DimensionsTextExpression,
            "Dimensions Text / Expression",
            "Traceability / Dimensions",
            true,
            true,
            false,
            250
        ),
        Create(
            LengthWidthHeight,
            "Length / Width / Height",
            "Traceability / Dimensions",
            true,
            true,
            false,
            260
        ),
        Create(
            DrawingRevision,
            "Drawing ID / Revision",
            "Traceability / Dimensions",
            true,
            true,
            false,
            270
        ),
    ];

    public static IReadOnlyList<string> DefaultUiVisibleIds =>
        All.Where(definition => definition.DefaultUiVisible)
            .Select(definition => definition.Id)
            .ToList();

    public static IReadOnlyList<string> DefaultPrintVisibleIds =>
        All.Where(definition => definition.DefaultPrintVisible)
            .Select(definition => definition.Id)
            .ToList();

    public static IReadOnlyList<string> LogicOnlyFieldIds =>
        All.Where(definition => definition.IsLogicOnly)
            .Select(definition => definition.Id)
            .ToList();

    public static Model_Tool_MaterialAvailabilityFieldDefinition? GetById(string fieldId)
    {
        return All.FirstOrDefault(field =>
            string.Equals(field.Id, fieldId, System.StringComparison.OrdinalIgnoreCase)
        );
    }

    private static Model_Tool_MaterialAvailabilityFieldDefinition Create(
        string id,
        string displayName,
        string category,
        bool defaultUiVisible,
        bool defaultPrintVisible,
        bool isLogicOnly,
        int sortOrder
    )
    {
        return new Model_Tool_MaterialAvailabilityFieldDefinition
        {
            Id = id,
            DisplayName = displayName,
            Category = category,
            DefaultUiVisible = defaultUiVisible,
            DefaultPrintVisible = defaultPrintVisible,
            IsLogicOnly = isLogicOnly,
            SortOrder = sortOrder,
        };
    }
}

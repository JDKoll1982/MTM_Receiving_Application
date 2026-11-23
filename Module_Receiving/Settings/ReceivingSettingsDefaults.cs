using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Receiving.Settings;

/// <summary>
/// Default values for Module_Receiving settings.
/// These are used when values are not present in MySQL and no other source is available.
/// </summary>
public static class ReceivingSettingsDefaults
{
    public static IReadOnlyDictionary<string, string> StringDefaults { get; } =
        new Dictionary<string, string>
        {
            [ReceivingSettingsKeys.Labels.ReceivingLabelPath] = string.Empty,
            [ReceivingSettingsKeys.Labels.MiniReceivingLabelPath] = string.Empty,

            // Workflow shell
            [ReceivingSettingsKeys.UiText.WorkflowHelp] = "Help",
            [ReceivingSettingsKeys.UiText.WorkflowBack] = "Back",
            [ReceivingSettingsKeys.UiText.WorkflowNext] = "Next",
            [ReceivingSettingsKeys.UiText.WorkflowResetLabelData] = "Clear Label Data",

            // Shell action bar
            [ReceivingSettingsKeys.UiText.ShellGuided] = "Guided",
            [ReceivingSettingsKeys.UiText.ShellManual] = "Manual",
            [ReceivingSettingsKeys.UiText.ShellEdit] = "Edit",
            [ReceivingSettingsKeys.UiText.ShellOverlay] = "Overlay",
            [ReceivingSettingsKeys.UiText.ShellGuidedDefaultTooltip] = "make Guided my default",
            [ReceivingSettingsKeys.UiText.ShellManualDefaultTooltip] = "make Manual my default",
            [ReceivingSettingsKeys.UiText.ShellEditDefaultTooltip] = "make Edit my default",
            [ReceivingSettingsKeys.UiText.ShellDefault] = "Default",

            // Completion
            [ReceivingSettingsKeys.UiText.CompletionSuccessTitle] = "Success!",
            [ReceivingSettingsKeys.UiText.CompletionFailureTitle] = "Save Failed",
            [ReceivingSettingsKeys.UiText.CompletionLoadsSavedSuffix] =
                " loads saved successfully.",
            [ReceivingSettingsKeys.UiText.CompletionSaveDetailsTitle] = "Save Details:",
            [ReceivingSettingsKeys.UiText.CompletionLabelQueueLabel] = "Label Queue:",
            [ReceivingSettingsKeys.UiText.CompletionArchiveQueueLabel] = "Archive Queue:",
            [ReceivingSettingsKeys.UiText.CompletionLabelDataLabel] = "Label Data:",
            [ReceivingSettingsKeys.UiText.CompletionDatabaseLabel] = "Database:",
            [ReceivingSettingsKeys.UiText.CompletionSaved] = "Saved",
            [ReceivingSettingsKeys.UiText.CompletionFailed] = "Failed",
            [ReceivingSettingsKeys.UiText.CompletionStartNewEntry] = "Start New Entry",

            // Manual entry
            [ReceivingSettingsKeys.UiText.ManualEntryAddRow] = "Add Row",
            [ReceivingSettingsKeys.UiText.ManualEntryAddMultiple] = "Add Multiple",
            [ReceivingSettingsKeys.UiText.ManualEntryRemoveRow] = "Remove Row",
            [ReceivingSettingsKeys.UiText.ManualEntryAutoFill] = "Auto-Fill",
            [ReceivingSettingsKeys.UiText.ManualEntrySaveAndFinish] = "Save & Finish",
            [ReceivingSettingsKeys.UiText.ManualEntryColumnLoadNumber] = "Load #",
            [ReceivingSettingsKeys.UiText.ManualEntryColumnPartId] = "Part ID",
            [ReceivingSettingsKeys.UiText.ManualEntryColumnWeightQty] = "Weight/Qty",
            [ReceivingSettingsKeys.UiText.ManualEntryColumnHeatLot] = "Heat/Lot",
            [ReceivingSettingsKeys.UiText.ManualEntryColumnPkgType] = "Pkg Type",
            [ReceivingSettingsKeys.UiText.ManualEntryColumnPkgsPerLoad] = "Pkgs/Load",
            [ReceivingSettingsKeys.UiText.ManualEntryColumnWtPerPkg] = "UOM/Pkg",
            [ReceivingSettingsKeys.UiText.ManualEntryColumnPoNumber] = "PO Number",

            // Edit mode
            [ReceivingSettingsKeys.UiText.EditModeLoadDataFrom] = "Load Data From:",
            [ReceivingSettingsKeys.UiText.EditModeCurrentMemory] = "Not Saved to Label Data",
            [ReceivingSettingsKeys.UiText.EditModeCurrentLabels] = "Label Data",
            [ReceivingSettingsKeys.UiText.EditModeHistory] = "History",
            [ReceivingSettingsKeys.UiText.EditModeFilterDate] = "Filter Date:",
            [ReceivingSettingsKeys.UiText.EditModeTo] = "to",
            [ReceivingSettingsKeys.UiText.EditModeLastWeek] = "Last Week",
            [ReceivingSettingsKeys.UiText.EditModeToday] = "Today",
            [ReceivingSettingsKeys.UiText.EditModeThisWeek] = "This Week",
            [ReceivingSettingsKeys.UiText.EditModeShowAll] = "Show All",
            [ReceivingSettingsKeys.UiText.EditModePage] = "Page",
            [ReceivingSettingsKeys.UiText.EditModeOf] = "of",
            [ReceivingSettingsKeys.UiText.EditModeGo] = "Go",
            [ReceivingSettingsKeys.UiText.EditModeSaveAndFinish] = "Save & Finish",
            [ReceivingSettingsKeys.UiText.EditModeRemoveRow] = "Remove Row",
            [ReceivingSettingsKeys.UiText.EditModeColumnLoadNumber] = "Load #",
            [ReceivingSettingsKeys.UiText.EditModeColumnPartId] = "Part ID",
            [ReceivingSettingsKeys.UiText.EditModeColumnWeightQty] = "Weight/Qty",
            [ReceivingSettingsKeys.UiText.EditModeColumnHeatLot] = "Heat/Lot",
            [ReceivingSettingsKeys.UiText.EditModeColumnPkgType] = "Pkg Type",
            [ReceivingSettingsKeys.UiText.EditModeColumnPkgsPerLoad] = "Pkgs/Load",
            [ReceivingSettingsKeys.UiText.EditModeColumnWtPerPkg] = "UOM/Pkg",

            // Default visible columns (comma-separated keys), search-by, sort, and page-size
            [ReceivingSettingsKeys.UiText.EditModeColumnVisibility] =
                "LoadNumber,ReceivedDate,PartID,PONumber,WeightQuantity,HeatLotNumber,PackagesPerLoad,PackageType,WeightPerPackage",
            [ReceivingSettingsKeys.UiText.EditModeSearchByColumn] = "All Fields",
            [ReceivingSettingsKeys.UiText.EditModeSortColumn] = "",
            [ReceivingSettingsKeys.UiText.EditModeSortAscending] = "true",
            [ReceivingSettingsKeys.UiText.EditModePageSize] = "20",

            // Select Part page: newest line receive date first, nothing filtered out
            [ReceivingSettingsKeys.UiText.PartSelectionSortColumn] = "RecvDate",
            [ReceivingSettingsKeys.UiText.PartSelectionSortAscending] = "false",
            [ReceivingSettingsKeys.UiText.PartSelectionOnHandOnly] = "false",
            [ReceivingSettingsKeys.UiText.PartSelectionQualityHoldOnly] = "false",
            [ReceivingSettingsKeys.UiText.PartSelectionLocation] = "",

            // PO entry
            [ReceivingSettingsKeys.UiText.PoEntryPurchaseOrderNumber] = "Purchase Order Number",
            [ReceivingSettingsKeys.UiText.PoEntryStatusLabel] = "Status:",
            [ReceivingSettingsKeys.UiText.PoEntryLoadPo] = "Load PO",
            [ReceivingSettingsKeys.UiText.PoEntrySwitchToNonPo] = "Switch to Non-PO",
            [ReceivingSettingsKeys.UiText.PoEntryPartIdentifier] = "Part Identifier",
            [ReceivingSettingsKeys.UiText.PoEntryPackageTypeAuto] = "Package Type (Auto-detected)",
            [ReceivingSettingsKeys.UiText.PoEntryLookupPart] = "Look Up Part",
            [ReceivingSettingsKeys.UiText.PoEntrySwitchToPo] = "Switch to PO Entry",
            [ReceivingSettingsKeys.UiText.PoEntryAvailableParts] = "Available Parts",
            [ReceivingSettingsKeys.UiText.PoEntryColumnPartId] = "Part ID",
            [ReceivingSettingsKeys.UiText.PoEntryColumnDescription] = "Description",
            [ReceivingSettingsKeys.UiText.PoEntryColumnRemainingQty] = "Remaining Qty",
            [ReceivingSettingsKeys.UiText.PoEntryColumnQtyOrdered] = "Qty Ordered",
            [ReceivingSettingsKeys.UiText.PoEntryColumnLineNumber] = "Line #",
            [ReceivingSettingsKeys.UiText.PoEntryColumnOnHand] = "On Hand",
            [ReceivingSettingsKeys.UiText.PoEntryColumnLocation] = "Location",

            // Load entry
            [ReceivingSettingsKeys.UiText.LoadEntryHeader] = "Number of Labels (1-99)",
            [ReceivingSettingsKeys.UiText.LoadEntryInstruction] =
                "Enter the total number of skids/loads for this part.",

            // Weight/Quantity
            [ReceivingSettingsKeys.UiText.WeightQuantityHeader] = "Weight/Quantity",
            [ReceivingSettingsKeys.UiText.WeightQuantityPlaceholder] = "Enter whole number",

            // Heat/Lot
            [ReceivingSettingsKeys.UiText.HeatLotHeader] = "Load Entries",
            [ReceivingSettingsKeys.UiText.HeatLotAutoFill] = "Auto-Fill",
            [ReceivingSettingsKeys.UiText.HeatLotAutoFillTooltip] =
                "Fill blank heat numbers from rows above",
            [ReceivingSettingsKeys.UiText.HeatLotLoadPrefix] = "Load #{0}",
            [ReceivingSettingsKeys.UiText.HeatLotFieldHeader] = "Heat/Lot Number (Optional)",
            [ReceivingSettingsKeys.UiText.HeatLotFieldPlaceholder] =
                "Enter Heat or Lot Number",
            [ReceivingSettingsKeys.UiText.HeatLotPresetFillers] =
                "[\"Refer to Vendor Tag\",\"N/A\",\"Old Coil\",\"Old Flatstock\",\"Old Product\"]",

            // Package Type
            [ReceivingSettingsKeys.UiText.PackageTypeHeader] = "Package Type:",
            [ReceivingSettingsKeys.UiText.PackageTypeComboHeader] = "Type",
            [ReceivingSettingsKeys.UiText.PackageTypeCustomHeader] = "Custom Name",
            [ReceivingSettingsKeys.UiText.PackageTypeAutoFill] = "Auto-Fill",
            [ReceivingSettingsKeys.UiText.PackageTypeAutoFillTooltip] =
                "Fill blank package counts from rows above",
            [ReceivingSettingsKeys.UiText.PackageTypeSaveAsDefault] =
                "Save as default for this part",
            [ReceivingSettingsKeys.UiText.PackageTypeLoadNumberPrefix] = "#{0}",
            [ReceivingSettingsKeys.UiText.PackageTypePackagesPerLoad] = "Packages per Load",
            [ReceivingSettingsKeys.UiText.PackageTypeWeightPerPackage] = "Weight per Package",

            // Review
            [ReceivingSettingsKeys.UiText.ReviewEntryLabel] = "Entry",
            [ReceivingSettingsKeys.UiText.ReviewOfLabel] = "of",
            [ReceivingSettingsKeys.UiText.ReviewLoadNumber] = "Load Number",
            [ReceivingSettingsKeys.UiText.ReviewPurchaseOrderNumber] = "Purchase Order Number",
            [ReceivingSettingsKeys.UiText.ReviewPartId] = "Part ID",
            [ReceivingSettingsKeys.UiText.ReviewRemainingQuantity] = "Remaining Quantity",
            [ReceivingSettingsKeys.UiText.ReviewWeightQuantity] = "Weight/Quantity",
            [ReceivingSettingsKeys.UiText.ReviewHeatLotNumber] = "Heat/Lot Number",
            [ReceivingSettingsKeys.UiText.ReviewPackagesPerLoad] = "Packages Per Load",
            [ReceivingSettingsKeys.UiText.ReviewPackageType] = "Package Type",
            [ReceivingSettingsKeys.UiText.ReviewPreviousLabel] = "Previous",
            [ReceivingSettingsKeys.UiText.ReviewNextLabel] = "Next",
            [ReceivingSettingsKeys.UiText.ReviewTableViewLabel] = "Table View",
            [ReceivingSettingsKeys.UiText.ReviewSingleViewLabel] = "Single View",
            [ReceivingSettingsKeys.UiText.ReviewSaveToDatabase] = "Save to Database",
            [ReceivingSettingsKeys.UiText.ReviewColumnLoadNumber] = "Load #",
            [ReceivingSettingsKeys.UiText.ReviewColumnPoNumber] = "PO Number",
            [ReceivingSettingsKeys.UiText.ReviewColumnPartId] = "Part ID",
            [ReceivingSettingsKeys.UiText.ReviewColumnRemainingQty] = "Remaining Qty",
            [ReceivingSettingsKeys.UiText.ReviewColumnWeightQty] = "Weight/Qty",
            [ReceivingSettingsKeys.UiText.ReviewColumnHeatLot] = "Heat/Lot #",
            [ReceivingSettingsKeys.UiText.ReviewColumnPkgs] = "Pkgs",
            [ReceivingSettingsKeys.UiText.ReviewColumnPkgType] = "Pkg Type",

            // Workflow step titles
            [ReceivingSettingsKeys.Workflow.StepTitleManualEntry] = "Receiving - Manual Entry",
            [ReceivingSettingsKeys.Workflow.StepTitleEditMode] = "Receiving - Edit Mode",
            [ReceivingSettingsKeys.Workflow.StepTitlePoEntry] = "Receiving - Enter PO Number",
            [ReceivingSettingsKeys.Workflow.StepTitlePartSelection] = "Receiving - Select Part",
            [ReceivingSettingsKeys.Workflow.StepTitleLoadEntry] =
                "Receiving - Enter Number of Loads",
            [ReceivingSettingsKeys.Workflow.StepTitleWeightQuantity] =
                "Receiving - Enter Weight/Quantity",
            [ReceivingSettingsKeys.Workflow.StepTitleHeatLot] =
                "Receiving - Enter Heat/Lot Numbers",
            [ReceivingSettingsKeys.Workflow.StepTitlePackageType] =
                "Receiving - Select Package Type",
            [ReceivingSettingsKeys.Workflow.StepTitleReviewAndSave] = "Receiving - Review & Save",
            [ReceivingSettingsKeys.Workflow.StepTitleSaving] = "Receiving - Saving...",
            [ReceivingSettingsKeys.Workflow.StepTitleComplete] = "Receiving - Complete",

            // Workflow save progress defaults
            [ReceivingSettingsKeys.Workflow.SaveProgressInitializing] = "Initializing...",
            [ReceivingSettingsKeys.Workflow.SaveProgressSavingLabelData] =
                "Saving to label queue...",

            // Workflow dialogs
            [ReceivingSettingsKeys.Workflow.ResetLabelDataDialogTitle] = "Clear Label Data",
            [ReceivingSettingsKeys.Workflow.ResetLabelDataDialogContent] =
                "Are you sure you want to move all current label data to history and clear the active label queue? This action cannot be undone.",
            [ReceivingSettingsKeys.Workflow.ResetLabelDataDialogDelete] = "Clear",
            [ReceivingSettingsKeys.Workflow.ResetLabelDataDialogCancel] = "Cancel",
            [ReceivingSettingsKeys.Workflow.DbSaveFailedDialogTitle] = "Database Save Failed",
            [ReceivingSettingsKeys.Workflow.DbSaveFailedDialogDeleteAnyway] = "Delete Anyway",
            [ReceivingSettingsKeys.Workflow.DbSaveFailedDialogCancel] = "Cancel",

            // Status
            [ReceivingSettingsKeys.Workflow.StatusLabelDataClearedSuccess] =
                "Label data cleared successfully. Rows were archived to history.",
            [ReceivingSettingsKeys.Workflow.StatusLabelDataClearedFailed] =
                "Failed to clear label data to history.",

            // Dialogs
            [ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionTitle] = "Change Mode?",
            [ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionContent] =
                "Selecting a new mode will reset all unsaved data in the current workflow. Do you want to continue?",
            [ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionContinue] = "Continue",
            [ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionCancel] = "Cancel",
            [ReceivingSettingsKeys.Dialogs.ReceiverQtyMismatchTitle] = "Quantity Does Not Match",
            [ReceivingSettingsKeys.Dialogs.ReceiverQtyMismatchContent] =
                "The loads total {0} but the receiver shows {1}. Change the values, or continue as entered.",
            [ReceivingSettingsKeys.Dialogs.ReceiverQtyMismatchChangeValues] = "Change Values",
            [ReceivingSettingsKeys.Dialogs.ReceiverQtyMismatchContinue] = "Continue",
            [ReceivingSettingsKeys.Dialogs.ManualEntryAddMultipleTitle] = "Add Multiple Rows",
            [ReceivingSettingsKeys.Dialogs.ManualEntryAddMultipleAdd] = "Add",
            [ReceivingSettingsKeys.Dialogs.ManualEntryAddMultipleCancel] = "Cancel",

            // Messages (templates)
            [ReceivingSettingsKeys.Messages.ErrorUnableToDisplayDialog] =
                "Unable to display dialog",
            [ReceivingSettingsKeys.Messages.ErrorPoRequired] = "Please enter a PO number.",
            [ReceivingSettingsKeys.Messages.ErrorPartIdRequired] = "Please enter a Part ID.",
            [ReceivingSettingsKeys.Messages.ErrorPoNotFound] = "PO not found or contains no parts.",
            [ReceivingSettingsKeys.Messages.ErrorPoHasNoParts] =
                "Purchase Order {0} has no part numbers. Enter a different PO.",
            [ReceivingSettingsKeys.Messages.ErrorPartNotFound] = "Part not found.",
            [ReceivingSettingsKeys.Messages.InfoPoLoadedWithParts] =
                "Purchase Order {0} loaded with {1} parts.",
            [ReceivingSettingsKeys.Messages.InfoPoSinglePartAutoSelected] =
                "Part {0} was auto selected because it is the only part on this PO.",
            [ReceivingSettingsKeys.Messages.InfoPartFound] = "Part {0} found.",
            [ReceivingSettingsKeys.Messages.InfoNonPoPartReady] =
                "Part {0} selected. Continue to the load step.",
            [ReceivingSettingsKeys.Messages.WarningSameDayReceiving] =
                "Warning: {0:N2} of this part has already been received today on this PO.",
            [ReceivingSettingsKeys.Messages.WarningExceedsOrdered] =
                "Warning: Total quantity ({0:N2}) exceeds PO ordered amount ({1:N2}).",
            [ReceivingSettingsKeys.Messages.InfoNonPoItem] = "Non-PO Item",

            // Accessibility
            [ReceivingSettingsKeys.Accessibility.PoEntryPONumber] = "Purchase Order Number",
            [ReceivingSettingsKeys.Accessibility.PoEntryLoadPo] = "Load Purchase Order",
            [ReceivingSettingsKeys.Accessibility.PoEntryPartId] = "Part Identifier",
            [ReceivingSettingsKeys.Accessibility.PoEntryLookupPart] = "Look Up Part",
            [ReceivingSettingsKeys.Accessibility.PoEntryPartsList] = "Parts List",
            [ReceivingSettingsKeys.Accessibility.LoadEntryNumberOfLoads] = "Number of Labels",
            [ReceivingSettingsKeys.Accessibility.WeightQuantityInput] = "Weight Quantity",
            [ReceivingSettingsKeys.Accessibility.HeatLotNumber] = "Heat Lot Number",
            [ReceivingSettingsKeys.Accessibility.PackageTypeCombo] = "Package Type",
            [ReceivingSettingsKeys.Accessibility.PackageTypeCustomName] = "Custom Package Name",
            [ReceivingSettingsKeys.Accessibility.PackageTypePackagesPerLoad] = "Packages per Load",
            [ReceivingSettingsKeys.Accessibility.ReviewPreviousEntry] = "Previous Entry",
            [ReceivingSettingsKeys.Accessibility.ReviewNextEntry] = "Next Entry",
            [ReceivingSettingsKeys.Accessibility.ReviewSwitchToTable] = "Switch to Table View",
            [ReceivingSettingsKeys.Accessibility.ReviewSwitchToSingle] = "Switch to Single View",
            [ReceivingSettingsKeys.Accessibility.ReviewEntriesTable] = "Receiving Entries Table",
            [ReceivingSettingsKeys.Accessibility.ReviewSaveToDatabase] = "Save to Database",

            // Defaults for non-UI settings
            [ReceivingSettingsKeys.Defaults.DefaultLocation] = "RECV",

            // Integrations defaults
            [ReceivingSettingsKeys.Integrations.ErpConnectionTimeout] = "30",
            [ReceivingSettingsKeys.Integrations.MaxSyncRetries] = "3",

            // Part Number Padding defaults
            [ReceivingSettingsKeys.PartNumberPadding.RulesJson] =
                "[{\"Name\":\"Coil\",\"Prefix\":\"MMC\",\"MaxLength\":10,\"PadChar\":\"0\",\"IsEnabled\":true},{\"Name\":\"Flatstock\",\"Prefix\":\"MMF\",\"MaxLength\":10,\"PadChar\":\"0\",\"IsEnabled\":true}]",

            // Validation defaults
            [ReceivingSettingsKeys.Validation.MinLoadCount] = "1",
            [ReceivingSettingsKeys.Validation.MaxLoadCount] = "99",
            [ReceivingSettingsKeys.Validation.MinQuantity] = "0",
            [ReceivingSettingsKeys.Validation.MaxQuantity] = "999999",

            // Business Rules defaults
            [ReceivingSettingsKeys.BusinessRules.DefaultModeOnStartup] = "Guided",

            // User Preferences defaults
            [ReceivingSettingsKeys.UserPreferences.VendorVariableMappingsJson] =
                "[{\"VendorName\":\"Skana\",\"VariableName\":\"DM #\"}]",

            // Receiver selection
            [ReceivingSettingsKeys.ReceiverSelection.ScreenTitle] = "Receiving - By Receiver",
            [ReceivingSettingsKeys.ReceiverSelection.SearchPlaceholder] = "Search by receiver number",
            [ReceivingSettingsKeys.ReceiverSelection.RowsPerPage] = "Rows per page",
            [ReceivingSettingsKeys.ReceiverSelection.RowsPerPage25] = "25",
            [ReceivingSettingsKeys.ReceiverSelection.RowsPerPage50] = "50",
            [ReceivingSettingsKeys.ReceiverSelection.RowsPerPage100] = "100",
            [ReceivingSettingsKeys.ReceiverSelection.RowsPerPage200] = "200",
            [ReceivingSettingsKeys.ReceiverSelection.Refresh] = "Refresh",
            [ReceivingSettingsKeys.ReceiverSelection.PreviousPage] = "Previous page",
            [ReceivingSettingsKeys.ReceiverSelection.NextPage] = "Next page",
            [ReceivingSettingsKeys.ReceiverSelection.PageIndicator] = "Page {0} of {1}",
            [ReceivingSettingsKeys.ReceiverSelection.EmptyMessage] = "No receiver matched the search.",
            [ReceivingSettingsKeys.ReceiverSelection.FailureMessage] =
                "The recent receivers could not be loaded.",
            [ReceivingSettingsKeys.ReceiverSelection.Retry] = "Retry",
            [ReceivingSettingsKeys.ReceiverSelection.QtyRemainingOnLineLabel] =
                "Qty Remaining on Line",
            [ReceivingSettingsKeys.ReceiverSelection.ColumnReceiverNumber] = "Receiver Number",
            [ReceivingSettingsKeys.ReceiverSelection.ColumnPoNumber] = "PO #",
            [ReceivingSettingsKeys.ReceiverSelection.ColumnLineNumber] = "Line #",
            [ReceivingSettingsKeys.ReceiverSelection.ColumnPartNumber] = "Part Number",
            [ReceivingSettingsKeys.ReceiverSelection.ColumnVendor] = "Vendor",
            [ReceivingSettingsKeys.ReceiverSelection.ColumnLocationReceivedTo] =
                "Location Received to",
            [ReceivingSettingsKeys.ReceiverSelection.ColumnQtyReceived] = "Qty Received",
            [ReceivingSettingsKeys.ReceiverSelection.ColumnQtyOrdered] = "Qty Ordered",
            [ReceivingSettingsKeys.ReceiverSelection.ColumnQtyRemaining] =
                "Qty Remaining on Line",

            [ReceivingSettingsKeys.Workflow.StepTitleReceiverSelection] =
                "Receiving - By Receiver",

            [ReceivingSettingsKeys.UiText.ShellByReceiver] = "By Receiver",
            [ReceivingSettingsKeys.UiText.ShellByReceiverDefaultTooltip] =
                "make By Receiver my default",

            [ReceivingSettingsKeys.Accessibility.ReceiverSelectionSearch] =
                "Receiver number search",
            [ReceivingSettingsKeys.Accessibility.ReceiverSelectionRowsPerPage] = "Rows per page",
            [ReceivingSettingsKeys.Accessibility.ReceiverSelectionList] = "Recent receivers",
        };

    public static IReadOnlyDictionary<string, bool> BoolDefaults { get; } =
        new Dictionary<string, bool>
        {
            // Validation
            [ReceivingSettingsKeys.Validation.RequirePoNumber] = false,
            [ReceivingSettingsKeys.Validation.RequireQuantity] = true,
            [ReceivingSettingsKeys.Validation.RequireHeatLot] = false,
            [ReceivingSettingsKeys.Validation.AllowNegativeQuantity] = false,
            [ReceivingSettingsKeys.Validation.ValidatePoExists] = true,
            [ReceivingSettingsKeys.Validation.ValidatePartExists] = true,
            [ReceivingSettingsKeys.Validation.WarnOnQuantityExceedsPo] = true,
            [ReceivingSettingsKeys.Validation.WarnOnSameDayReceiving] = true,

            // Business Rules
            [ReceivingSettingsKeys.BusinessRules.ConfirmModeChange] = true,
            [ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault] = false,

            // Integrations
            [ReceivingSettingsKeys.Integrations.ErpSyncEnabled] = true,
            [ReceivingSettingsKeys.Integrations.AutoPullPoDataEnabled] = true,
            [ReceivingSettingsKeys.Integrations.AutoPullPartDataEnabled] = true,
            [ReceivingSettingsKeys.Integrations.SyncToInforVisual] = false,
            [ReceivingSettingsKeys.Integrations.RetryFailedSyncs] = true,

            // Part Number Padding
            [ReceivingSettingsKeys.PartNumberPadding.Enabled] = true,

            // Shortcuts
            [ReceivingSettingsKeys.Shortcuts.IsToggleSimpleNavigationEnabled] = true,
        };

    public static IReadOnlyDictionary<string, int> IntDefaults { get; } =
        new Dictionary<string, int>
        {
            // Validation
            [ReceivingSettingsKeys.Validation.MinLoadCount] = 1,
            [ReceivingSettingsKeys.Validation.MaxLoadCount] = 99,
            [ReceivingSettingsKeys.Validation.MinQuantity] = 0,
            [ReceivingSettingsKeys.Validation.MaxQuantity] = 999999,

            // Integrations
            [ReceivingSettingsKeys.Integrations.ErpConnectionTimeout] = 30,
            [ReceivingSettingsKeys.Integrations.MaxSyncRetries] = 3,

            // Receiver selection
            [ReceivingSettingsKeys.ReceiverSelection.DefaultPageSize] = 50,
        };
}

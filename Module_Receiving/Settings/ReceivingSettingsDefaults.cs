using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Receiving.Settings;

/// <summary>
/// Default values for Module_Receiving settings.
/// These are used when values are not present in MySQL and no other source is available.
/// </summary>
public static class ReceivingSettingsDefaults
{
    public static IReadOnlyDictionary<string, string> StringDefaults { get; } = new Dictionary<string, string>
    {
        // Mode selection
        [ReceivingSettingsKeys.UiText.ModeSelectionGuidedTitle] = "Guided Wizard",
        [ReceivingSettingsKeys.UiText.ModeSelectionGuidedDescription] = "Step-by-step process for standard receiving workflow.",
        [ReceivingSettingsKeys.UiText.ModeSelectionManualTitle] = "Manual Entry",
        [ReceivingSettingsKeys.UiText.ModeSelectionManualDescription] = "Customizable grid for bulk data entry and editing.",
        [ReceivingSettingsKeys.UiText.ModeSelectionEditTitle] = "Edit Mode",
        [ReceivingSettingsKeys.UiText.ModeSelectionEditDescription] = "Edit existing loads without adding new ones.",
        [ReceivingSettingsKeys.UiText.ModeSelectionSetDefault] = "Set as default mode",

        // Workflow shell
        [ReceivingSettingsKeys.UiText.WorkflowHelp] = "Help",
        [ReceivingSettingsKeys.UiText.WorkflowBack] = "Back",
        [ReceivingSettingsKeys.UiText.WorkflowNext] = "Next",
        [ReceivingSettingsKeys.UiText.WorkflowModeSelection] = "Mode Selection",
        [ReceivingSettingsKeys.UiText.WorkflowResetXls] = "Reset XLS",

        // Completion
        [ReceivingSettingsKeys.UiText.CompletionSuccessTitle] = "Success!",
        [ReceivingSettingsKeys.UiText.CompletionFailureTitle] = "Save Failed",
        [ReceivingSettingsKeys.UiText.CompletionLoadsSavedSuffix] = " loads saved successfully.",
        [ReceivingSettingsKeys.UiText.CompletionSaveDetailsTitle] = "Save Details:",
        [ReceivingSettingsKeys.UiText.CompletionLocalXlsLabel] = "Local XLS:",
        [ReceivingSettingsKeys.UiText.CompletionNetworkXlsLabel] = "Network XLS:",
        [ReceivingSettingsKeys.UiText.CompletionXlsFileLabel] = "XLS File:",
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
        [ReceivingSettingsKeys.UiText.ManualEntryColumnWtPerPkg] = "Wt/Pkg",
        [ReceivingSettingsKeys.UiText.ManualEntryColumnPoNumber] = "PO Number",

        // Edit mode
        [ReceivingSettingsKeys.UiText.EditModeLoadDataFrom] = "Load Data From:",
        [ReceivingSettingsKeys.UiText.EditModeCurrentMemory] = "Not Saved to XLS",
        [ReceivingSettingsKeys.UiText.EditModeCurrentLabels] = "XLS Data",
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
        [ReceivingSettingsKeys.UiText.EditModeColumnWtPerPkg] = "Wt/Pkg",

        // Default visible columns (comma-separated keys), search-by, sort, and page-size
        [ReceivingSettingsKeys.UiText.EditModeColumnVisibility] =
            "LoadNumber,ReceivedDate,PartID,PONumber,WeightQuantity,HeatLotNumber,PackagesPerLoad,PackageType,WeightPerPackage",
        [ReceivingSettingsKeys.UiText.EditModeSearchByColumn] = "All Fields",
        [ReceivingSettingsKeys.UiText.EditModeSortColumn] = "",
        [ReceivingSettingsKeys.UiText.EditModeSortAscending] = "true",
        [ReceivingSettingsKeys.UiText.EditModePageSize] = "20",

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

        // Load entry
        [ReceivingSettingsKeys.UiText.LoadEntryHeader] = "Number of Loads (1-99)",
        [ReceivingSettingsKeys.UiText.LoadEntryInstruction] = "Enter the total number of skids/loads for this part.",

        // Weight/Quantity
        [ReceivingSettingsKeys.UiText.WeightQuantityHeader] = "Weight/Quantity",
        [ReceivingSettingsKeys.UiText.WeightQuantityPlaceholder] = "Enter whole number",

        // Heat/Lot
        [ReceivingSettingsKeys.UiText.HeatLotHeader] = "Load Entries",
        [ReceivingSettingsKeys.UiText.HeatLotAutoFill] = "Auto-Fill",
        [ReceivingSettingsKeys.UiText.HeatLotAutoFillTooltip] = "Fill blank heat numbers from rows above",
        [ReceivingSettingsKeys.UiText.HeatLotLoadPrefix] = "Load #{0}",
        [ReceivingSettingsKeys.UiText.HeatLotFieldHeader] = "Heat/Lot Number (Optional)",
        [ReceivingSettingsKeys.UiText.HeatLotFieldPlaceholder] = "Enter heat/lot number or leave blank",

        // Package Type
        [ReceivingSettingsKeys.UiText.PackageTypeHeader] = "Package Type (Applied to all loads)",
        [ReceivingSettingsKeys.UiText.PackageTypeComboHeader] = "Type",
        [ReceivingSettingsKeys.UiText.PackageTypeCustomHeader] = "Custom Name",
        [ReceivingSettingsKeys.UiText.PackageTypeSaveAsDefault] = "Save as default for this part",
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
        [ReceivingSettingsKeys.UiText.ReviewAddAnother] = "Add Another Part/PO",
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
        [ReceivingSettingsKeys.Workflow.StepTitleModeSelection] = "Receiving - Mode Selection",
        [ReceivingSettingsKeys.Workflow.StepTitleManualEntry] = "Receiving - Manual Entry",
        [ReceivingSettingsKeys.Workflow.StepTitleEditMode] = "Receiving - Edit Mode",
        [ReceivingSettingsKeys.Workflow.StepTitlePoEntry] = "Receiving - Enter PO Number",
        [ReceivingSettingsKeys.Workflow.StepTitlePartSelection] = "Receiving - Select Part",
        [ReceivingSettingsKeys.Workflow.StepTitleLoadEntry] = "Receiving - Enter Number of Loads",
        [ReceivingSettingsKeys.Workflow.StepTitleWeightQuantity] = "Receiving - Enter Weight/Quantity",
        [ReceivingSettingsKeys.Workflow.StepTitleHeatLot] = "Receiving - Enter Heat/Lot Numbers",
        [ReceivingSettingsKeys.Workflow.StepTitlePackageType] = "Receiving - Select Package Type",
        [ReceivingSettingsKeys.Workflow.StepTitleReviewAndSave] = "Receiving - Review & Save",
        [ReceivingSettingsKeys.Workflow.StepTitleSaving] = "Receiving - Saving...",
        [ReceivingSettingsKeys.Workflow.StepTitleComplete] = "Receiving - Complete",

        // Workflow save progress defaults
        [ReceivingSettingsKeys.Workflow.SaveProgressInitializing] = "Initializing...",
        [ReceivingSettingsKeys.Workflow.SaveProgressSavingXls] = "Saving to local and network XLS...",

        // Workflow dialogs
        [ReceivingSettingsKeys.Workflow.ResetXlsDialogTitle] = "Reset XLS Files",
        [ReceivingSettingsKeys.Workflow.ResetXlsDialogContent] = "Are you sure you want to delete the local and network XLS files? This action cannot be undone.",
        [ReceivingSettingsKeys.Workflow.ResetXlsDialogDelete] = "Delete",
        [ReceivingSettingsKeys.Workflow.ResetXlsDialogCancel] = "Cancel",
        [ReceivingSettingsKeys.Workflow.DbSaveFailedDialogTitle] = "Database Save Failed",
        [ReceivingSettingsKeys.Workflow.DbSaveFailedDialogDeleteAnyway] = "Delete Anyway",
        [ReceivingSettingsKeys.Workflow.DbSaveFailedDialogCancel] = "Cancel",

        // Status
        [ReceivingSettingsKeys.Workflow.StatusXlsDeletedSuccess] = "XLS files deleted successfully.",
        [ReceivingSettingsKeys.Workflow.StatusXlsDeletedFailed] = "Failed to delete XLS files or files not found.",
        [ReceivingSettingsKeys.Workflow.StatusWorkflowCleared] = "Workflow cleared. Please select a mode.",

        // Dialogs
        [ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionTitle] = "Confirm Mode Selection",
        [ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionContent] = "Selecting a new mode will reset all unsaved data in the current workflow. Do you want to continue?",
        [ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionContinue] = "Continue",
        [ReceivingSettingsKeys.Dialogs.ConfirmModeSelectionCancel] = "Cancel",
        [ReceivingSettingsKeys.Dialogs.ConfirmChangeModeTitle] = "Change Mode?",
        [ReceivingSettingsKeys.Dialogs.ConfirmChangeModeContent] = "Returning to mode selection will clear all current work in progress. This cannot be undone. Are you sure?",
        [ReceivingSettingsKeys.Dialogs.ConfirmChangeModeConfirm] = "Yes, Change Mode",
        [ReceivingSettingsKeys.Dialogs.ConfirmChangeModeCancel] = "Cancel",
        [ReceivingSettingsKeys.Dialogs.ManualEntryAddMultipleTitle] = "Add Multiple Rows",
        [ReceivingSettingsKeys.Dialogs.ManualEntryAddMultipleAdd] = "Add",
        [ReceivingSettingsKeys.Dialogs.ManualEntryAddMultipleCancel] = "Cancel",
        [ReceivingSettingsKeys.Dialogs.ReviewAddAnotherTitle] = "Add Another Part/PO",
        [ReceivingSettingsKeys.Dialogs.ReviewAddAnotherContent] = "Current form data will be cleared to start a new entry. Your reviewed loads are preserved. Continue?",
        [ReceivingSettingsKeys.Dialogs.ReviewAddAnotherContinue] = "Continue",
        [ReceivingSettingsKeys.Dialogs.ReviewAddAnotherCancel] = "Cancel",

        // Messages (templates)
        [ReceivingSettingsKeys.Messages.ErrorUnableToDisplayDialog] = "Unable to display dialog",
        [ReceivingSettingsKeys.Messages.ErrorPoRequired] = "Please enter a PO number.",
        [ReceivingSettingsKeys.Messages.ErrorPartIdRequired] = "Please enter a Part ID.",
        [ReceivingSettingsKeys.Messages.ErrorPoNotFound] = "PO not found or contains no parts.",
        [ReceivingSettingsKeys.Messages.ErrorPartNotFound] = "Part not found.",
        [ReceivingSettingsKeys.Messages.InfoPoLoadedWithParts] = "Purchase Order {0} loaded with {1} parts.",
        [ReceivingSettingsKeys.Messages.InfoPartFound] = "Part {0} found.",
        [ReceivingSettingsKeys.Messages.WarningSameDayReceiving] = "Warning: {0:N2} of this part has already been received today on this PO.",
        [ReceivingSettingsKeys.Messages.WarningExceedsOrdered] = "Warning: Total quantity ({0:N2}) exceeds PO ordered amount ({1:N2}).",
        [ReceivingSettingsKeys.Messages.InfoNonPoItem] = "Non-PO Item",

        // Accessibility
        [ReceivingSettingsKeys.Accessibility.ModeSelectionGuidedButton] = "Guided Wizard Mode",
        [ReceivingSettingsKeys.Accessibility.ModeSelectionManualButton] = "Manual Entry Mode",
        [ReceivingSettingsKeys.Accessibility.ModeSelectionEditButton] = "Edit Mode",
        [ReceivingSettingsKeys.Accessibility.PoEntryPONumber] = "Purchase Order Number",
        [ReceivingSettingsKeys.Accessibility.PoEntryLoadPo] = "Load Purchase Order",
        [ReceivingSettingsKeys.Accessibility.PoEntryPartId] = "Part Identifier",
        [ReceivingSettingsKeys.Accessibility.PoEntryLookupPart] = "Look Up Part",
        [ReceivingSettingsKeys.Accessibility.PoEntryPartsList] = "Parts List",
        [ReceivingSettingsKeys.Accessibility.LoadEntryNumberOfLoads] = "Number of Loads",
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
        [ReceivingSettingsKeys.Accessibility.ReviewAddAnother] = "Add Another Part or PO",
        [ReceivingSettingsKeys.Accessibility.ReviewSaveToDatabase] = "Save to Database",

        // Defaults for non-UI settings
        [ReceivingSettingsKeys.Defaults.DefaultReceivingMode] = "Guided",
        [ReceivingSettingsKeys.Defaults.CsvSaveLocation] = "",

        // Integrations defaults
        [ReceivingSettingsKeys.Integrations.ErpConnectionTimeout] = "30",
        [ReceivingSettingsKeys.Integrations.MaxSyncRetries] = "3",

        // Part Number Padding defaults
        [ReceivingSettingsKeys.PartNumberPadding.RulesJson] = "[{\"Prefix\":\"MMC\",\"MaxLength\":10,\"PadChar\":\"0\",\"IsEnabled\":true}]",

        // Validation defaults
        [ReceivingSettingsKeys.Validation.MinLoadCount] = "1",
        [ReceivingSettingsKeys.Validation.MaxLoadCount] = "99",
        [ReceivingSettingsKeys.Validation.MinQuantity] = "0",
        [ReceivingSettingsKeys.Validation.MaxQuantity] = "999999",

        // Business Rules defaults
        [ReceivingSettingsKeys.BusinessRules.AutoSaveIntervalSeconds] = "300",
        [ReceivingSettingsKeys.BusinessRules.DefaultModeOnStartup] = "ModeSelection",
    };

    public static IReadOnlyDictionary<string, bool> BoolDefaults { get; } = new Dictionary<string, bool>
    {
        // Validation
        [ReceivingSettingsKeys.Validation.RequirePoNumber] = false,
        [ReceivingSettingsKeys.Validation.RequirePartId] = true,
        [ReceivingSettingsKeys.Validation.RequireQuantity] = true,
        [ReceivingSettingsKeys.Validation.RequireHeatLot] = false,
        [ReceivingSettingsKeys.Validation.AllowNegativeQuantity] = false,
        [ReceivingSettingsKeys.Validation.ValidatePoExists] = true,
        [ReceivingSettingsKeys.Validation.ValidatePartExists] = true,
        [ReceivingSettingsKeys.Validation.WarnOnQuantityExceedsPo] = true,
        [ReceivingSettingsKeys.Validation.WarnOnSameDayReceiving] = true,

        // Business Rules
        [ReceivingSettingsKeys.BusinessRules.AutoSaveEnabled] = false,
        [ReceivingSettingsKeys.BusinessRules.SaveToCsvEnabled] = true,
        [ReceivingSettingsKeys.BusinessRules.SaveToNetworkCsvEnabled] = true,
        [ReceivingSettingsKeys.BusinessRules.SaveToDatabaseEnabled] = true,
        [ReceivingSettingsKeys.BusinessRules.RememberLastMode] = true,
        [ReceivingSettingsKeys.BusinessRules.ConfirmModeChange] = true,
        [ReceivingSettingsKeys.BusinessRules.AutoFillHeatLotEnabled] = true,
        [ReceivingSettingsKeys.BusinessRules.SavePackageTypeAsDefault] = false,
        [ReceivingSettingsKeys.BusinessRules.ShowReviewTableByDefault] = false,
        [ReceivingSettingsKeys.BusinessRules.AllowEditAfterSave] = true,

        // Integrations
        [ReceivingSettingsKeys.Integrations.ErpSyncEnabled] = true,
        [ReceivingSettingsKeys.Integrations.AutoPullPoDataEnabled] = true,
        [ReceivingSettingsKeys.Integrations.AutoPullPartDataEnabled] = true,
        [ReceivingSettingsKeys.Integrations.SyncToInforVisual] = false,
        [ReceivingSettingsKeys.Integrations.RetryFailedSyncs] = true,

        // Part Number Padding
        [ReceivingSettingsKeys.PartNumberPadding.Enabled] = true,
    };

    public static IReadOnlyDictionary<string, int> IntDefaults { get; } = new Dictionary<string, int>
    {
        // Validation
        [ReceivingSettingsKeys.Validation.MinLoadCount] = 1,
        [ReceivingSettingsKeys.Validation.MaxLoadCount] = 99,
        [ReceivingSettingsKeys.Validation.MinQuantity] = 0,
        [ReceivingSettingsKeys.Validation.MaxQuantity] = 999999,

        // Business Rules
        [ReceivingSettingsKeys.BusinessRules.AutoSaveIntervalSeconds] = 300,

        // Integrations
        [ReceivingSettingsKeys.Integrations.ErpConnectionTimeout] = 30,
        [ReceivingSettingsKeys.Integrations.MaxSyncRetries] = 3,
    };
}

using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Receiving.Settings;

/// <summary>
/// Default values for Module_Receiving settings.
/// These are used when values are not present in MySQL and no other source is available.
/// </summary>
public static class Helper_Receiving_Infrastructure_SettingsDefaults
{
    public static IReadOnlyDictionary<string, string> StringDefaults { get; } = new Dictionary<string, string>
    {
        // Mode selection
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ModeSelectionGuidedTitle] = "Guided Wizard",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ModeSelectionGuidedDescription] = "Step-by-step process for standard receiving workflow.",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ModeSelectionManualTitle] = "Manual Entry",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ModeSelectionManualDescription] = "Customizable grid for bulk data entry and editing.",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ModeSelectionEditTitle] = "Edit Mode",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ModeSelectionEditDescription] = "Edit existing loads without adding new ones.",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ModeSelectionSetDefault] = "Set as default mode",

        // Workflow shell
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.WorkflowHelp] = "Help",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.WorkflowBack] = "Back",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.WorkflowNext] = "Next",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.WorkflowModeSelection] = "Mode Selection",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.WorkflowResetCsv] = "Reset CSV",

        // Completion
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.CompletionSuccessTitle] = "Success!",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.CompletionFailureTitle] = "Save Failed",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.CompletionLoadsSavedSuffix] = " loads saved successfully.",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.CompletionSaveDetailsTitle] = "Save Details:",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.CompletionLocalCsvLabel] = "Local CSV:",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.CompletionNetworkCsvLabel] = "Network CSV:",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.CompletionDatabaseLabel] = "Database:",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.CompletionSaved] = "Saved",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.CompletionFailed] = "Failed",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.CompletionStartNewEntry] = "Start New Entry",

        // Manual entry
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryAddRow] = "Add Row",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryAddMultiple] = "Add Multiple",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryRemoveRow] = "Remove Row",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryAutoFill] = "Auto-Fill",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntrySaveAndFinish] = "Save & Finish",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryColumnLoadNumber] = "Load #",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryColumnPartId] = "Part ID",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryColumnWeightQty] = "Weight/Qty",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryColumnHeatLot] = "Heat/Lot",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryColumnPkgType] = "Pkg Type",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryColumnPkgsPerLoad] = "Pkgs/Load",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ManualEntryColumnWtPerPkg] = "Wt/Pkg",

        // Edit mode
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeLoadDataFrom] = "Load Data From:",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeCurrentMemory] = "Current Memory",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeCurrentLabels] = "Current Labels",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeHistory] = "History",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeFilterDate] = "Filter Date:",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeTo] = "to",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeLastWeek] = "Last Week",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeToday] = "Today",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeThisWeek] = "This Week",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeShowAll] = "Show All",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModePage] = "Page",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeOf] = "of",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeGo] = "Go",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeSaveAndFinish] = "Save & Finish",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeRemoveRow] = "Remove Row",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeColumnLoadNumber] = "Load #",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeColumnPartId] = "Part ID",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeColumnWeightQty] = "Weight/Qty",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeColumnHeatLot] = "Heat/Lot",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeColumnPkgType] = "Pkg Type",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeColumnPkgsPerLoad] = "Pkgs/Load",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.EditModeColumnWtPerPkg] = "Wt/Pkg",

        // PO entry
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryPurchaseOrderNumber] = "Purchase Order Number",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryStatusLabel] = "Status:",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryLoadPo] = "Load PO",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntrySwitchToNonPo] = "Switch to Non-PO",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryPartIdentifier] = "Part Identifier",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryPackageTypeAuto] = "Package Type (Auto-detected)",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryLookupPart] = "Look Up Part",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntrySwitchToPo] = "Switch to PO Entry",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryAvailableParts] = "Available Parts",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryColumnPartId] = "Part ID",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryColumnDescription] = "Description",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryColumnRemainingQty] = "Remaining Qty",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryColumnQtyOrdered] = "Qty Ordered",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PoEntryColumnLineNumber] = "Line #",

        // Load entry
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.LoadEntryHeader] = "Number of Loads (1-99)",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.LoadEntryInstruction] = "Enter the total number of skids/loads for this part.",

        // Weight/Quantity
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.WeightQuantityHeader] = "Weight/Quantity",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.WeightQuantityPlaceholder] = "Enter whole number",

        // Heat/Lot
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotHeader] = "Load Entries",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotAutoFill] = "Auto-Fill",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotAutoFillTooltip] = "Fill blank heat numbers from rows above",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotLoadPrefix] = "Load #{0}",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotFieldHeader] = "Heat/Lot Number (Optional)",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.HeatLotFieldPlaceholder] = "Enter heat/lot number or leave blank",

        // Package Type
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PackageTypeHeader] = "Package Type (Applied to all loads)",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PackageTypeComboHeader] = "Type",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PackageTypeCustomHeader] = "Custom Name",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PackageTypeSaveAsDefault] = "Save as default for this part",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PackageTypeLoadNumberPrefix] = "#{0}",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PackageTypePackagesPerLoad] = "Packages per Load",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.PackageTypeWeightPerPackage] = "Weight per Package",

        // Review
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewEntryLabel] = "Entry",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewOfLabel] = "of",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewLoadNumber] = "Load Number",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewPurchaseOrderNumber] = "Purchase Order Number",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewPartId] = "Part ID",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewRemainingQuantity] = "Remaining Quantity",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewWeightQuantity] = "Weight/Quantity",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewHeatLotNumber] = "Heat/Lot Number",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewPackagesPerLoad] = "Packages Per Load",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewPackageType] = "Package Type",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewPreviousLabel] = "Previous",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewNextLabel] = "Next",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewTableViewLabel] = "Table View",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewSingleViewLabel] = "Single View",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewAddAnother] = "Add Another Part/PO",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewSaveToDatabase] = "Save to Database",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnLoadNumber] = "Load #",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnPoNumber] = "PO Number",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnPartId] = "Part ID",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnRemainingQty] = "Remaining Qty",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnWeightQty] = "Weight/Qty",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnHeatLot] = "Heat/Lot #",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnPkgs] = "Pkgs",
        [Helper_Receiving_Infrastructure_SettingsKeys.UiText.ReviewColumnPkgType] = "Pkg Type",

        // Workflow step titles
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitleModeSelection] = "Receiving - Mode Selection",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitleManualEntry] = "Receiving - Manual Entry",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitleEditMode] = "Receiving - Edit Mode",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitlePoEntry] = "Receiving - Enter PO Number",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitlePartSelection] = "Receiving - Select Part",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitleLoadEntry] = "Receiving - Enter Number of Loads",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitleWeightQuantity] = "Receiving - Enter Weight/Quantity",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitleHeatLot] = "Receiving - Enter Heat/Lot Numbers",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitlePackageType] = "Receiving - Select Package Type",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitleReviewAndSave] = "Receiving - Review & Save",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitleSaving] = "Receiving - Saving...",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StepTitleComplete] = "Receiving - Complete",

        // Workflow save progress defaults
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.SaveProgressInitializing] = "Initializing...",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.SaveProgressSavingCsv] = "Saving to local and network CSV...",

        // Workflow dialogs
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.ResetCsvDialogTitle] = "Reset CSV Files",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.ResetCsvDialogContent] = "Are you sure you want to delete the local and network CSV files? This action cannot be undone.",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.ResetCsvDialogDelete] = "Delete",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.ResetCsvDialogCancel] = "Cancel",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.DbSaveFailedDialogTitle] = "Database Save Failed",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.DbSaveFailedDialogDeleteAnyway] = "Delete Anyway",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.DbSaveFailedDialogCancel] = "Cancel",

        // Status
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StatusCsvDeletedSuccess] = "CSV files deleted successfully.",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StatusCsvDeletedFailed] = "Failed to delete CSV files or files not found.",
        [Helper_Receiving_Infrastructure_SettingsKeys.Workflow.StatusWorkflowCleared] = "Workflow cleared. Please select a mode.",

        // Dialogs
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ConfirmModeSelectionTitle] = "Confirm Mode Selection",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ConfirmModeSelectionContent] = "Selecting a new mode will reset all unsaved data in the current workflow. Do you want to continue?",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ConfirmModeSelectionContinue] = "Continue",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ConfirmModeSelectionCancel] = "Cancel",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ConfirmChangeModeTitle] = "Change Mode?",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ConfirmChangeModeContent] = "Returning to mode selection will clear all current work in progress. This cannot be undone. Are you sure?",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ConfirmChangeModeConfirm] = "Yes, Change Mode",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ConfirmChangeModeCancel] = "Cancel",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ManualEntryAddMultipleTitle] = "Add Multiple Rows",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ManualEntryAddMultipleAdd] = "Add",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ManualEntryAddMultipleCancel] = "Cancel",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ReviewAddAnotherTitle] = "Add Another Part/PO",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ReviewAddAnotherContent] = "Current form data will be cleared to start a new entry. Your reviewed loads are preserved. Continue?",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ReviewAddAnotherContinue] = "Continue",
        [Helper_Receiving_Infrastructure_SettingsKeys.Dialogs.ReviewAddAnotherCancel] = "Cancel",

        // Messages (templates)
        [Helper_Receiving_Infrastructure_SettingsKeys.Messages.ErrorUnableToDisplayDialog] = "Unable to display dialog",
        [Helper_Receiving_Infrastructure_SettingsKeys.Messages.ErrorPoRequired] = "Please enter a PO number.",
        [Helper_Receiving_Infrastructure_SettingsKeys.Messages.ErrorPartIdRequired] = "Please enter a Part ID.",
        [Helper_Receiving_Infrastructure_SettingsKeys.Messages.ErrorPoNotFound] = "PO not found or contains no parts.",
        [Helper_Receiving_Infrastructure_SettingsKeys.Messages.ErrorPartNotFound] = "Part not found.",
        [Helper_Receiving_Infrastructure_SettingsKeys.Messages.InfoPoLoadedWithParts] = "Purchase Order {0} loaded with {1} parts.",
        [Helper_Receiving_Infrastructure_SettingsKeys.Messages.InfoPartFound] = "Part {0} found.",
        [Helper_Receiving_Infrastructure_SettingsKeys.Messages.WarningSameDayReceiving] = "Warning: {0:N2} of this part has already been received today on this PO.",
        [Helper_Receiving_Infrastructure_SettingsKeys.Messages.WarningExceedsOrdered] = "Warning: Total quantity ({0:N2}) exceeds PO ordered amount ({1:N2}).",
        [Helper_Receiving_Infrastructure_SettingsKeys.Messages.InfoNonPoItem] = "Non-PO Item",

        // Accessibility
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ModeSelectionGuidedButton] = "Guided Wizard Mode",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ModeSelectionManualButton] = "Manual Entry Mode",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ModeSelectionEditButton] = "Edit Mode",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.PoEntryPONumber] = "Purchase Order Number",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.PoEntryLoadPo] = "Load Purchase Order",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.PoEntryPartId] = "Part Identifier",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.PoEntryLookupPart] = "Look Up Part",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.PoEntryPartsList] = "Parts List",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.LoadEntryNumberOfLoads] = "Number of Loads",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.WeightQuantityInput] = "Weight Quantity",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.HeatLotNumber] = "Heat Lot Number",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.PackageTypeCombo] = "Package Type",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.PackageTypeCustomName] = "Custom Package Name",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.PackageTypePackagesPerLoad] = "Packages per Load",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewPreviousEntry] = "Previous Entry",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewNextEntry] = "Next Entry",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewSwitchToTable] = "Switch to Table View",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewSwitchToSingle] = "Switch to Single View",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewEntriesTable] = "Receiving Entries Table",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewAddAnother] = "Add Another Part or PO",
        [Helper_Receiving_Infrastructure_SettingsKeys.Accessibility.ReviewSaveToDatabase] = "Save to Database",

        // Defaults for non-UI settings
        [Helper_Receiving_Infrastructure_SettingsKeys.Defaults.DefaultPackageType] = "Pallet",
        [Helper_Receiving_Infrastructure_SettingsKeys.Defaults.DefaultPackagesPerLoad] = "1",
        [Helper_Receiving_Infrastructure_SettingsKeys.Defaults.DefaultWeightPerPackage] = "0",
        [Helper_Receiving_Infrastructure_SettingsKeys.Defaults.DefaultUnitOfMeasure] = "LBS",
        [Helper_Receiving_Infrastructure_SettingsKeys.Defaults.DefaultLocation] = "RECEIVING",
        [Helper_Receiving_Infrastructure_SettingsKeys.Defaults.DefaultLoadNumberPrefix] = "L",
        [Helper_Receiving_Infrastructure_SettingsKeys.Defaults.DefaultReceivingMode] = "Guided",

        // Integrations defaults
        [Helper_Receiving_Infrastructure_SettingsKeys.Integrations.ErpConnectionTimeout] = "30",
        [Helper_Receiving_Infrastructure_SettingsKeys.Integrations.MaxSyncRetries] = "3",

        // Validation defaults
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.MinLoadCount] = "1",
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.MaxLoadCount] = "99",
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.MinQuantity] = "0",
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.MaxQuantity] = "999999",

        // Business Rules defaults
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoSaveIntervalSeconds] = "300",
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.DefaultModeOnStartup] = "ModeSelection",
    };

    public static IReadOnlyDictionary<string, bool> BoolDefaults { get; } = new Dictionary<string, bool>
    {
        // Validation
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequirePoNumber] = false,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequirePartId] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequireQuantity] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.RequireHeatLot] = false,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.AllowNegativeQuantity] = false,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.ValidatePoExists] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.ValidatePartExists] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.WarnOnQuantityExceedsPo] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.WarnOnSameDayReceiving] = true,

        // Business Rules
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoSaveEnabled] = false,
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToCsvEnabled] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToNetworkCsvEnabled] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SaveToDatabaseEnabled] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.RememberLastMode] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.ConfirmModeChange] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoFillHeatLotEnabled] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.SavePackageTypeAsDefault] = false,
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.ShowReviewTableByDefault] = false,
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AllowEditAfterSave] = true,

        // Integrations
        [Helper_Receiving_Infrastructure_SettingsKeys.Integrations.ErpSyncEnabled] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.Integrations.AutoPullPoDataEnabled] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.Integrations.AutoPullPartDataEnabled] = true,
        [Helper_Receiving_Infrastructure_SettingsKeys.Integrations.SyncToInforVisual] = false,
        [Helper_Receiving_Infrastructure_SettingsKeys.Integrations.RetryFailedSyncs] = true,
    };

    public static IReadOnlyDictionary<string, int> IntDefaults { get; } = new Dictionary<string, int>
    {
        // Validation
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.MinLoadCount] = 1,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.MaxLoadCount] = 99,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.MinQuantity] = 0,
        [Helper_Receiving_Infrastructure_SettingsKeys.Validation.MaxQuantity] = 999999,

        // Business Rules
        [Helper_Receiving_Infrastructure_SettingsKeys.BusinessRules.AutoSaveIntervalSeconds] = 300,

        // Integrations
        [Helper_Receiving_Infrastructure_SettingsKeys.Integrations.ErpConnectionTimeout] = 30,
        [Helper_Receiving_Infrastructure_SettingsKeys.Integrations.MaxSyncRetries] = 3,
    };
}

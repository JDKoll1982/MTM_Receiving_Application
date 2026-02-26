namespace MTM_Receiving_Application.Module_Receiving.Settings;

/// <summary>
/// Central key definitions for all Module_Receiving settings and UI text.
/// </summary>
public static class ReceivingSettingsKeys
{
    public static class UiText
    {
        public const string ModeSelectionGuidedTitle = "Receiving.UiText.ModeSelection.GuidedTitle";
        public const string ModeSelectionGuidedDescription = "Receiving.UiText.ModeSelection.GuidedDescription";
        public const string ModeSelectionManualTitle = "Receiving.UiText.ModeSelection.ManualTitle";
        public const string ModeSelectionManualDescription = "Receiving.UiText.ModeSelection.ManualDescription";
        public const string ModeSelectionEditTitle = "Receiving.UiText.ModeSelection.EditTitle";
        public const string ModeSelectionEditDescription = "Receiving.UiText.ModeSelection.EditDescription";
        public const string ModeSelectionSetDefault = "Receiving.UiText.ModeSelection.SetDefault";

        public const string WorkflowHelp = "Receiving.UiText.Workflow.Help";
        public const string WorkflowBack = "Receiving.UiText.Workflow.Back";
        public const string WorkflowNext = "Receiving.UiText.Workflow.Next";
        public const string WorkflowModeSelection = "Receiving.UiText.Workflow.ModeSelection";
        public const string WorkflowResetXls = "Receiving.UiText.Workflow.ResetXls";

        public const string CompletionSuccessTitle = "Receiving.UiText.Completion.SuccessTitle";
        public const string CompletionFailureTitle = "Receiving.UiText.Completion.FailureTitle";
        public const string CompletionLoadsSavedSuffix = "Receiving.UiText.Completion.LoadsSavedSuffix";
        public const string CompletionSaveDetailsTitle = "Receiving.UiText.Completion.SaveDetailsTitle";
        public const string CompletionLocalXlsLabel = "Receiving.UiText.Completion.LocalXlsLabel";
        public const string CompletionNetworkXlsLabel = "Receiving.UiText.Completion.NetworkXlsLabel";
        public const string CompletionXlsFileLabel = "Receiving.UiText.Completion.XlsFileLabel";
        public const string CompletionDatabaseLabel = "Receiving.UiText.Completion.DatabaseLabel";
        public const string CompletionSaved = "Receiving.UiText.Completion.Saved";
        public const string CompletionFailed = "Receiving.UiText.Completion.Failed";
        public const string CompletionStartNewEntry = "Receiving.UiText.Completion.StartNewEntry";

        public const string ManualEntryAddRow = "Receiving.UiText.ManualEntry.AddRow";
        public const string ManualEntryAddMultiple = "Receiving.UiText.ManualEntry.AddMultiple";
        public const string ManualEntryRemoveRow = "Receiving.UiText.ManualEntry.RemoveRow";
        public const string ManualEntryAutoFill = "Receiving.UiText.ManualEntry.AutoFill";
        public const string ManualEntrySaveAndFinish = "Receiving.UiText.ManualEntry.SaveAndFinish";

        public const string ManualEntryColumnLoadNumber = "Receiving.UiText.ManualEntry.Column.LoadNumber";
        public const string ManualEntryColumnPoNumber = "Receiving.UiText.ManualEntry.Column.PoNumber";
        public const string ManualEntryColumnPartId = "Receiving.UiText.ManualEntry.Column.PartId";
        public const string ManualEntryColumnWeightQty = "Receiving.UiText.ManualEntry.Column.WeightQty";
        public const string ManualEntryColumnHeatLot = "Receiving.UiText.ManualEntry.Column.HeatLot";
        public const string ManualEntryColumnPkgType = "Receiving.UiText.ManualEntry.Column.PkgType";
        public const string ManualEntryColumnPkgsPerLoad = "Receiving.UiText.ManualEntry.Column.PkgsPerLoad";
        public const string ManualEntryColumnWtPerPkg = "Receiving.UiText.ManualEntry.Column.WtPerPkg";

        public const string EditModeLoadDataFrom = "Receiving.UiText.EditMode.LoadDataFrom";
        public const string EditModeCurrentMemory = "Receiving.UiText.EditMode.CurrentMemory";
        public const string EditModeCurrentLabels = "Receiving.UiText.EditMode.CurrentLabels";
        public const string EditModeHistory = "Receiving.UiText.EditMode.History";
        public const string EditModeFilterDate = "Receiving.UiText.EditMode.FilterDate";
        public const string EditModeTo = "Receiving.UiText.EditMode.To";
        public const string EditModeLastWeek = "Receiving.UiText.EditMode.LastWeek";
        public const string EditModeToday = "Receiving.UiText.EditMode.Today";
        public const string EditModeThisWeek = "Receiving.UiText.EditMode.ThisWeek";
        public const string EditModeShowAll = "Receiving.UiText.EditMode.ShowAll";
        public const string EditModePage = "Receiving.UiText.EditMode.Page";
        public const string EditModeOf = "Receiving.UiText.EditMode.Of";
        public const string EditModeGo = "Receiving.UiText.EditMode.Go";
        public const string EditModeSaveAndFinish = "Receiving.UiText.EditMode.SaveAndFinish";
        public const string EditModeRemoveRow = "Receiving.UiText.EditMode.RemoveRow";

        public const string EditModeColumnLoadNumber = "Receiving.UiText.EditMode.Column.LoadNumber";
        public const string EditModeColumnPartId = "Receiving.UiText.EditMode.Column.PartId";
        public const string EditModeColumnWeightQty = "Receiving.UiText.EditMode.Column.WeightQty";
        public const string EditModeColumnHeatLot = "Receiving.UiText.EditMode.Column.HeatLot";
        public const string EditModeColumnPkgType = "Receiving.UiText.EditMode.Column.PkgType";
        public const string EditModeColumnPkgsPerLoad = "Receiving.UiText.EditMode.Column.PkgsPerLoad";
        public const string EditModeColumnWtPerPkg = "Receiving.UiText.EditMode.Column.WtPerPkg";

        // Column visibility, search, sort, and page-size preferences (persisted per user)
        public const string EditModeColumnVisibility = "Receiving.UiText.EditMode.ColumnVisibility";
        public const string EditModeSearchByColumn = "Receiving.UiText.EditMode.SearchByColumn";
        public const string EditModeSortColumn = "Receiving.UiText.EditMode.SortColumn";
        public const string EditModeSortAscending = "Receiving.UiText.EditMode.SortAscending";
        public const string EditModePageSize = "Receiving.UiText.EditMode.PageSize";

        public const string PoEntryPurchaseOrderNumber = "Receiving.UiText.PoEntry.PurchaseOrderNumber";
        public const string PoEntryStatusLabel = "Receiving.UiText.PoEntry.StatusLabel";
        public const string PoEntryLoadPo = "Receiving.UiText.PoEntry.LoadPo";
        public const string PoEntrySwitchToNonPo = "Receiving.UiText.PoEntry.SwitchToNonPo";
        public const string PoEntryPartIdentifier = "Receiving.UiText.PoEntry.PartIdentifier";
        public const string PoEntryPackageTypeAuto = "Receiving.UiText.PoEntry.PackageTypeAuto";
        public const string PoEntryLookupPart = "Receiving.UiText.PoEntry.LookupPart";
        public const string PoEntrySwitchToPo = "Receiving.UiText.PoEntry.SwitchToPo";
        public const string PoEntryAvailableParts = "Receiving.UiText.PoEntry.AvailableParts";

        public const string PoEntryColumnPartId = "Receiving.UiText.PoEntry.Column.PartId";
        public const string PoEntryColumnDescription = "Receiving.UiText.PoEntry.Column.Description";
        public const string PoEntryColumnRemainingQty = "Receiving.UiText.PoEntry.Column.RemainingQty";
        public const string PoEntryColumnQtyOrdered = "Receiving.UiText.PoEntry.Column.QtyOrdered";
        public const string PoEntryColumnLineNumber = "Receiving.UiText.PoEntry.Column.LineNumber";

        public const string LoadEntryHeader = "Receiving.UiText.LoadEntry.Header";
        public const string LoadEntryInstruction = "Receiving.UiText.LoadEntry.Instruction";

        public const string WeightQuantityHeader = "Receiving.UiText.WeightQuantity.Header";
        public const string WeightQuantityPlaceholder = "Receiving.UiText.WeightQuantity.Placeholder";

        public const string HeatLotHeader = "Receiving.UiText.HeatLot.Header";
        public const string HeatLotAutoFill = "Receiving.UiText.HeatLot.AutoFill";
        public const string HeatLotAutoFillTooltip = "Receiving.UiText.HeatLot.AutoFillTooltip";
        public const string HeatLotLoadPrefix = "Receiving.UiText.HeatLot.LoadPrefix";
        public const string HeatLotFieldHeader = "Receiving.UiText.HeatLot.FieldHeader";
        public const string HeatLotFieldPlaceholder = "Receiving.UiText.HeatLot.FieldPlaceholder";

        public const string PackageTypeHeader = "Receiving.UiText.PackageType.Header";
        public const string PackageTypeComboHeader = "Receiving.UiText.PackageType.ComboHeader";
        public const string PackageTypeCustomHeader = "Receiving.UiText.PackageType.CustomHeader";
        public const string PackageTypeSaveAsDefault = "Receiving.UiText.PackageType.SaveAsDefault";
        public const string PackageTypeLoadNumberPrefix = "Receiving.UiText.PackageType.LoadNumberPrefix";
        public const string PackageTypePackagesPerLoad = "Receiving.UiText.PackageType.PackagesPerLoad";
        public const string PackageTypeWeightPerPackage = "Receiving.UiText.PackageType.WeightPerPackage";

        public const string ReviewEntryLabel = "Receiving.UiText.Review.EntryLabel";
        public const string ReviewOfLabel = "Receiving.UiText.Review.OfLabel";
        public const string ReviewLoadNumber = "Receiving.UiText.Review.LoadNumber";
        public const string ReviewPurchaseOrderNumber = "Receiving.UiText.Review.PurchaseOrderNumber";
        public const string ReviewPartId = "Receiving.UiText.Review.PartId";
        public const string ReviewRemainingQuantity = "Receiving.UiText.Review.RemainingQuantity";
        public const string ReviewWeightQuantity = "Receiving.UiText.Review.WeightQuantity";
        public const string ReviewHeatLotNumber = "Receiving.UiText.Review.HeatLotNumber";
        public const string ReviewPackagesPerLoad = "Receiving.UiText.Review.PackagesPerLoad";
        public const string ReviewPackageType = "Receiving.UiText.Review.PackageType";
        public const string ReviewPreviousLabel = "Receiving.UiText.Review.PreviousLabel";
        public const string ReviewNextLabel = "Receiving.UiText.Review.NextLabel";
        public const string ReviewTableViewLabel = "Receiving.UiText.Review.TableViewLabel";
        public const string ReviewSingleViewLabel = "Receiving.UiText.Review.SingleViewLabel";
        public const string ReviewAddAnother = "Receiving.UiText.Review.AddAnother";
        public const string ReviewSaveToDatabase = "Receiving.UiText.Review.SaveToDatabase";

        public const string ReviewColumnLoadNumber = "Receiving.UiText.Review.Column.LoadNumber";
        public const string ReviewColumnPoNumber = "Receiving.UiText.Review.Column.PoNumber";
        public const string ReviewColumnPartId = "Receiving.UiText.Review.Column.PartId";
        public const string ReviewColumnRemainingQty = "Receiving.UiText.Review.Column.RemainingQty";
        public const string ReviewColumnWeightQty = "Receiving.UiText.Review.Column.WeightQty";
        public const string ReviewColumnHeatLot = "Receiving.UiText.Review.Column.HeatLot";
        public const string ReviewColumnPkgs = "Receiving.UiText.Review.Column.Pkgs";
        public const string ReviewColumnPkgType = "Receiving.UiText.Review.Column.PkgType";
    }

    public static class Workflow
    {
        public const string StepTitleModeSelection = "Receiving.Workflow.StepTitle.ModeSelection";
        public const string StepTitleManualEntry = "Receiving.Workflow.StepTitle.ManualEntry";
        public const string StepTitleEditMode = "Receiving.Workflow.StepTitle.EditMode";
        public const string StepTitlePoEntry = "Receiving.Workflow.StepTitle.PoEntry";
        public const string StepTitlePartSelection = "Receiving.Workflow.StepTitle.PartSelection";
        public const string StepTitleLoadEntry = "Receiving.Workflow.StepTitle.LoadEntry";
        public const string StepTitleWeightQuantity = "Receiving.Workflow.StepTitle.WeightQuantity";
        public const string StepTitleHeatLot = "Receiving.Workflow.StepTitle.HeatLot";
        public const string StepTitlePackageType = "Receiving.Workflow.StepTitle.PackageType";
        public const string StepTitleReviewAndSave = "Receiving.Workflow.StepTitle.ReviewAndSave";
        public const string StepTitleSaving = "Receiving.Workflow.StepTitle.Saving";
        public const string StepTitleComplete = "Receiving.Workflow.StepTitle.Complete";

        public const string SaveProgressInitializing = "Receiving.Workflow.SaveProgress.Initializing";
        public const string SaveProgressSavingXls = "Receiving.Workflow.SaveProgress.SavingXls";

        public const string ResetXlsDialogTitle = "Receiving.Workflow.Dialog.ResetXls.Title";
        public const string ResetXlsDialogContent = "Receiving.Workflow.Dialog.ResetXls.Content";
        public const string ResetXlsDialogDelete = "Receiving.Workflow.Dialog.ResetXls.Delete";
        public const string ResetXlsDialogCancel = "Receiving.Workflow.Dialog.ResetXls.Cancel";

        public const string DbSaveFailedDialogTitle = "Receiving.Workflow.Dialog.DbSaveFailed.Title";
        public const string DbSaveFailedDialogDeleteAnyway = "Receiving.Workflow.Dialog.DbSaveFailed.DeleteAnyway";
        public const string DbSaveFailedDialogCancel = "Receiving.Workflow.Dialog.DbSaveFailed.Cancel";

        public const string StatusXlsDeletedSuccess = "Receiving.Workflow.Status.XlsDeletedSuccess";
        public const string StatusXlsDeletedFailed = "Receiving.Workflow.Status.XlsDeletedFailed";
        public const string StatusWorkflowCleared = "Receiving.Workflow.Status.WorkflowCleared";
    }

    public static class Dialogs
    {
        public const string ConfirmModeSelectionTitle = "Receiving.Dialogs.ConfirmModeSelection.Title";
        public const string ConfirmModeSelectionContent = "Receiving.Dialogs.ConfirmModeSelection.Content";
        public const string ConfirmModeSelectionContinue = "Receiving.Dialogs.ConfirmModeSelection.Continue";
        public const string ConfirmModeSelectionCancel = "Receiving.Dialogs.ConfirmModeSelection.Cancel";

        public const string ConfirmChangeModeTitle = "Receiving.Dialogs.ConfirmChangeMode.Title";
        public const string ConfirmChangeModeContent = "Receiving.Dialogs.ConfirmChangeMode.Content";
        public const string ConfirmChangeModeConfirm = "Receiving.Dialogs.ConfirmChangeMode.Confirm";
        public const string ConfirmChangeModeCancel = "Receiving.Dialogs.ConfirmChangeMode.Cancel";

        public const string ManualEntryAddMultipleTitle = "Receiving.Dialogs.ManualEntry.AddMultiple.Title";
        public const string ManualEntryAddMultipleAdd = "Receiving.Dialogs.ManualEntry.AddMultiple.Add";
        public const string ManualEntryAddMultipleCancel = "Receiving.Dialogs.ManualEntry.AddMultiple.Cancel";

        public const string ReviewAddAnotherTitle = "Receiving.Dialogs.Review.AddAnother.Title";
        public const string ReviewAddAnotherContent = "Receiving.Dialogs.Review.AddAnother.Content";
        public const string ReviewAddAnotherContinue = "Receiving.Dialogs.Review.AddAnother.Continue";
        public const string ReviewAddAnotherCancel = "Receiving.Dialogs.Review.AddAnother.Cancel";
    }

    public static class Messages
    {
        public const string ErrorUnableToDisplayDialog = "Receiving.Messages.Error.UnableToDisplayDialog";
        public const string ErrorPoRequired = "Receiving.Messages.Error.PoRequired";
        public const string ErrorPartIdRequired = "Receiving.Messages.Error.PartIdRequired";
        public const string ErrorPoNotFound = "Receiving.Messages.Error.PoNotFound";
        public const string ErrorPartNotFound = "Receiving.Messages.Error.PartNotFound";

        public const string InfoPoLoadedWithParts = "Receiving.Messages.Info.PoLoadedWithParts";
        public const string InfoPartFound = "Receiving.Messages.Info.PartFound";

        public const string WarningSameDayReceiving = "Receiving.Messages.Warning.SameDayReceiving";
        public const string WarningExceedsOrdered = "Receiving.Messages.Warning.ExceedsOrdered";
        public const string InfoNonPoItem = "Receiving.Messages.Info.NonPoItem";
    }

    public static class Accessibility
    {
        public const string ModeSelectionGuidedButton = "Receiving.Accessibility.ModeSelection.GuidedButton";
        public const string ModeSelectionManualButton = "Receiving.Accessibility.ModeSelection.ManualButton";
        public const string ModeSelectionEditButton = "Receiving.Accessibility.ModeSelection.EditButton";

        public const string PoEntryPONumber = "Receiving.Accessibility.PoEntry.PONumber";
        public const string PoEntryLoadPo = "Receiving.Accessibility.PoEntry.LoadPo";
        public const string PoEntryPartId = "Receiving.Accessibility.PoEntry.PartId";
        public const string PoEntryLookupPart = "Receiving.Accessibility.PoEntry.LookupPart";
        public const string PoEntryPartsList = "Receiving.Accessibility.PoEntry.PartsList";

        public const string LoadEntryNumberOfLoads = "Receiving.Accessibility.LoadEntry.NumberOfLoads";
        public const string WeightQuantityInput = "Receiving.Accessibility.WeightQuantity.Input";
        public const string HeatLotNumber = "Receiving.Accessibility.HeatLot.Number";
        public const string PackageTypeCombo = "Receiving.Accessibility.PackageType.Combo";
        public const string PackageTypeCustomName = "Receiving.Accessibility.PackageType.CustomName";
        public const string PackageTypePackagesPerLoad = "Receiving.Accessibility.PackageType.PackagesPerLoad";

        public const string ReviewPreviousEntry = "Receiving.Accessibility.Review.PreviousEntry";
        public const string ReviewNextEntry = "Receiving.Accessibility.Review.NextEntry";
        public const string ReviewSwitchToTable = "Receiving.Accessibility.Review.SwitchToTable";
        public const string ReviewSwitchToSingle = "Receiving.Accessibility.Review.SwitchToSingle";
        public const string ReviewEntriesTable = "Receiving.Accessibility.Review.EntriesTable";
        public const string ReviewAddAnother = "Receiving.Accessibility.Review.AddAnother";
        public const string ReviewSaveToDatabase = "Receiving.Accessibility.Review.SaveToDatabase";
    }

    public static class Validation
    {
        public const string RequirePoNumber = "Receiving.Validation.RequirePoNumber";
        public const string RequirePartId = "Receiving.Validation.RequirePartId";
        public const string RequireQuantity = "Receiving.Validation.RequireQuantity";
        public const string RequireHeatLot = "Receiving.Validation.RequireHeatLot";
        public const string AllowNegativeQuantity = "Receiving.Validation.AllowNegativeQuantity";
        public const string ValidatePoExists = "Receiving.Validation.ValidatePoExists";
        public const string ValidatePartExists = "Receiving.Validation.ValidatePartExists";
        public const string WarnOnQuantityExceedsPo = "Receiving.Validation.WarnOnQuantityExceedsPo";
        public const string WarnOnSameDayReceiving = "Receiving.Validation.WarnOnSameDayReceiving";
        public const string MinLoadCount = "Receiving.Validation.MinLoadCount";
        public const string MaxLoadCount = "Receiving.Validation.MaxLoadCount";
        public const string MinQuantity = "Receiving.Validation.MinQuantity";
        public const string MaxQuantity = "Receiving.Validation.MaxQuantity";
    }

    public static class BusinessRules
    {
        public const string AutoSaveEnabled = "Receiving.BusinessRules.AutoSaveEnabled";
        public const string AutoSaveIntervalSeconds = "Receiving.BusinessRules.AutoSaveIntervalSeconds";
        public const string SaveToLabelTableEnabled = "Receiving.BusinessRules.SaveToLabelTableEnabled";
        public const string SaveToNetworkLabelTableEnabled = "Receiving.BusinessRules.SaveToNetworkLabelTableEnabled";
        public const string SaveToDatabaseEnabled = "Receiving.BusinessRules.SaveToDatabaseEnabled";
        public const string DefaultModeOnStartup = "Receiving.BusinessRules.DefaultModeOnStartup";
        public const string RememberLastMode = "Receiving.BusinessRules.RememberLastMode";
        public const string ConfirmModeChange = "Receiving.BusinessRules.ConfirmModeChange";
        public const string AutoFillHeatLotEnabled = "Receiving.BusinessRules.AutoFillHeatLotEnabled";
        public const string SavePackageTypeAsDefault = "Receiving.BusinessRules.SavePackageTypeAsDefault";
        public const string ShowReviewTableByDefault = "Receiving.BusinessRules.ShowReviewTableByDefault";
        public const string AllowEditAfterSave = "Receiving.BusinessRules.AllowEditAfterSave";
    }

    public static class Defaults
    {
        public const string DefaultReceivingMode = "Receiving.Defaults.DefaultReceivingMode";
        public const string LabelTableSaveLocation = "Receiving.Defaults.LabelTableSaveLocation";
        public const string XlsSaveLocation = "Receiving.Defaults.XlsSaveLocation";
    }

    public static class Integrations
    {
        public const string ErpSyncEnabled = "Receiving.Integrations.ErpSyncEnabled";
        public const string AutoPullPoDataEnabled = "Receiving.Integrations.AutoPullPoDataEnabled";
        public const string AutoPullPartDataEnabled = "Receiving.Integrations.AutoPullPartDataEnabled";
        public const string SyncToInforVisual = "Receiving.Integrations.SyncToInforVisual";
        public const string ErpConnectionTimeout = "Receiving.Integrations.ErpConnectionTimeout";
        public const string RetryFailedSyncs = "Receiving.Integrations.RetryFailedSyncs";
        public const string MaxSyncRetries = "Receiving.Integrations.MaxSyncRetries";
    }

    public static class PartNumberPadding
    {
        public const string Enabled = "Receiving.PartNumberPadding.Enabled";
        public const string RulesJson = "Receiving.PartNumberPadding.RulesJson";
    }
}

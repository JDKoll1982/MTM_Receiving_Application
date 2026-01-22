# Script to add all Receiving settings to the manifest
# This will read ReceivingSettingsDefaults.cs and generate manifest entries

$manifestPath = "c:\Users\johnk\source\repos\MTM_Receiving_Application\Module_Settings.Core\Defaults\settings.manifest.json"
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json

# Track existing
$existingKeys = @($manifest.settings | ForEach-Object { $_.key })
Write-Host "Existing settings: $($existingKeys.Count)" -ForegroundColor Cyan

# Create new entries array
$newEntries = @()

# Helper
function Add-Entry {
    param($Key, $DisplayName, $DefaultValue, $DataType = "String", $Scope = "System", $Permission = "Admin")
    if ($existingKeys -notcontains $Key) {
        $script:newEntries += [PSCustomObject]@{
            category        = "Receiving"
            key             = $Key
            displayName     = $DisplayName
            defaultValue    = $DefaultValue
            dataType        = $DataType
            scope           = $Scope
            permissionLevel = $Permission
            isSensitive     = $false
            validationRules = $DataType
        }
    }
}

# Workflow UI Text
Add-Entry "Receiving.UiText.Workflow.Help" "Workflow - Help Button" "Help"
Add-Entry "Receiving.UiText.Workflow.Back" "Workflow - Back Button" "Back"
Add-Entry "Receiving.UiText.Workflow.Next" "Workflow - Next Button" "Next"
Add-Entry "Receiving.UiText.Workflow.ModeSelection" "Workflow - Mode Selection Button" "Mode Selection"
Add-Entry "Receiving.UiText.Workflow.ResetCsv" "Workflow - Reset CSV Button" "Reset CSV"

# Completion UI Text
Add-Entry "Receiving.UiText.Completion.SuccessTitle" "Completion - Success Title" "Success!"
Add-Entry "Receiving.UiText.Completion.FailureTitle" "Completion - Failure Title" "Save Failed"
Add-Entry "Receiving.UiText.Completion.LoadsSavedSuffix" "Completion - Loads Saved Suffix" " loads saved successfully."
Add-Entry "Receiving.UiText.Completion.SaveDetailsTitle" "Completion - Save Details Title" "Save Details:"
Add-Entry "Receiving.UiText.Completion.LocalCsvLabel" "Completion - Local CSV Label" "Local CSV:"
Add-Entry "Receiving.UiText.Completion.NetworkCsvLabel" "Completion - Network CSV Label" "Network CSV:"
Add-Entry "Receiving.UiText.Completion.DatabaseLabel" "Completion - Database Label" "Database:"
Add-Entry "Receiving.UiText.Completion.Saved" "Completion - Saved Status" "Saved"
Add-Entry "Receiving.UiText.Completion.Failed" "Completion - Failed Status" "Failed"
Add-Entry "Receiving.UiText.Completion.StartNewEntry" "Completion - Start New Entry Button" "Start New Entry"

# Manual Entry UI Text
Add-Entry "Receiving.UiText.ManualEntry.AddRow" "Manual Entry - Add Row" "Add Row"
Add-Entry "Receiving.UiText.ManualEntry.AddMultiple" "Manual Entry - Add Multiple" "Add Multiple"
Add-Entry "Receiving.UiText.ManualEntry.RemoveRow" "Manual Entry - Remove Row" "Remove Row"
Add-Entry "Receiving.UiText.ManualEntry.AutoFill" "Manual Entry - Auto-Fill" "Auto-Fill"
Add-Entry "Receiving.UiText.ManualEntry.SaveAndFinish" "Manual Entry - Save & Finish" "Save & Finish"
Add-Entry "Receiving.UiText.ManualEntry.Column.LoadNumber" "Manual Entry - Load # Column" "Load #"
Add-Entry "Receiving.UiText.ManualEntry.Column.PartId" "Manual Entry - Part ID Column" "Part ID"
Add-Entry "Receiving.UiText.ManualEntry.Column.WeightQty" "Manual Entry - Weight/Qty Column" "Weight/Qty"
Add-Entry "Receiving.UiText.ManualEntry.Column.HeatLot" "Manual Entry - Heat/Lot Column" "Heat/Lot"
Add-Entry "Receiving.UiText.ManualEntry.Column.PkgType" "Manual Entry - Pkg Type Column" "Pkg Type"
Add-Entry "Receiving.UiText.ManualEntry.Column.PkgsPerLoad" "Manual Entry - Pkgs/Load Column" "Pkgs/Load"
Add-Entry "Receiving.UiText.ManualEntry.Column.WtPerPkg" "Manual Entry - Wt/Pkg Column" "Wt/Pkg"

# Edit Mode UI Text
Add-Entry "Receiving.UiText.EditMode.LoadDataFrom" "Edit Mode - Load Data From" "Load Data From:"
Add-Entry "Receiving.UiText.EditMode.CurrentMemory" "Edit Mode - Current Memory" "Current Memory"
Add-Entry "Receiving.UiText.EditMode.CurrentLabels" "Edit Mode - Current Labels" "Current Labels"
Add-Entry "Receiving.UiText.EditMode.History" "Edit Mode - History" "History"
Add-Entry "Receiving.UiText.EditMode.FilterDate" "Edit Mode - Filter Date" "Filter Date:"
Add-Entry "Receiving.UiText.EditMode.To" "Edit Mode - To" "to"
Add-Entry "Receiving.UiText.EditMode.LastWeek" "Edit Mode - Last Week" "Last Week"
Add-Entry "Receiving.UiText.EditMode.Today" "Edit Mode - Today" "Today"
Add-Entry "Receiving.UiText.EditMode.ThisWeek" "Edit Mode - This Week" "This Week"
Add-Entry "Receiving.UiText.EditMode.ShowAll" "Edit Mode - Show All" "Show All"
Add-Entry "Receiving.UiText.EditMode.Page" "Edit Mode - Page" "Page"
Add-Entry "Receiving.UiText.EditMode.Of" "Edit Mode - Of" "of"
Add-Entry "Receiving.UiText.EditMode.Go" "Edit Mode - Go" "Go"
Add-Entry "Receiving.UiText.EditMode.SaveAndFinish" "Edit Mode - Save & Finish" "Save & Finish"
Add-Entry "Receiving.UiText.EditMode.RemoveRow" "Edit Mode - Remove Row" "Remove Row"
Add-Entry "Receiving.UiText.EditMode.Column.LoadNumber" "Edit Mode - Load # Column" "Load #"
Add-Entry "Receiving.UiText.EditMode.Column.PartId" "Edit Mode - Part ID Column" "Part ID"
Add-Entry "Receiving.UiText.EditMode.Column.WeightQty" "Edit Mode - Weight/Qty Column" "Weight/Qty"
Add-Entry "Receiving.UiText.EditMode.Column.HeatLot" "Edit Mode - Heat/Lot Column" "Heat/Lot"
Add-Entry "Receiving.UiText.EditMode.Column.PkgType" "Edit Mode - Pkg Type Column" "Pkg Type"
Add-Entry "Receiving.UiText.EditMode.Column.PkgsPerLoad" "Edit Mode - Pkgs/Load Column" "Pkgs/Load"
Add-Entry "Receiving.UiText.EditMode.Column.WtPerPkg" "Edit Mode - Wt/Pkg Column" "Wt/Pkg"

# PO Entry UI Text
Add-Entry "Receiving.UiText.PoEntry.PurchaseOrderNumber" "PO Entry - Purchase Order Number" "Purchase Order Number"
Add-Entry "Receiving.UiText.PoEntry.StatusLabel" "PO Entry - Status Label" "Status:"
Add-Entry "Receiving.UiText.PoEntry.LoadPo" "PO Entry - Load PO" "Load PO"
Add-Entry "Receiving.UiText.PoEntry.SwitchToNonPo" "PO Entry - Switch to Non-PO" "Switch to Non-PO"
Add-Entry "Receiving.UiText.PoEntry.PartIdentifier" "PO Entry - Part Identifier" "Part Identifier"
Add-Entry "Receiving.UiText.PoEntry.PackageTypeAuto" "PO Entry - Package Type Auto" "Package Type (Auto-detected)"
Add-Entry "Receiving.UiText.PoEntry.LookupPart" "PO Entry - Look Up Part" "Look Up Part"
Add-Entry "Receiving.UiText.PoEntry.SwitchToPo" "PO Entry - Switch to PO" "Switch to PO Entry"
Add-Entry "Receiving.UiText.PoEntry.AvailableParts" "PO Entry - Available Parts" "Available Parts"
Add-Entry "Receiving.UiText.PoEntry.Column.PartId" "PO Entry - Part ID Column" "Part ID"
Add-Entry "Receiving.UiText.PoEntry.Column.Description" "PO Entry - Description Column" "Description"
Add-Entry "Receiving.UiText.PoEntry.Column.RemainingQty" "PO Entry - Remaining Qty Column" "Remaining Qty"
Add-Entry "Receiving.UiText.PoEntry.Column.QtyOrdered" "PO Entry - Qty Ordered Column" "Qty Ordered"
Add-Entry "Receiving.UiText.PoEntry.Column.LineNumber" "PO Entry - Line # Column" "Line #"

# Load Entry UI Text
Add-Entry "Receiving.UiText.LoadEntry.Header" "Load Entry - Header" "Number of Loads (1-99)"
Add-Entry "Receiving.UiText.LoadEntry.Instruction" "Load Entry - Instruction" "Enter the total number of skids/loads for this part."

# Weight/Quantity UI Text
Add-Entry "Receiving.UiText.WeightQuantity.Header" "Weight/Quantity - Header" "Weight/Quantity"
Add-Entry "Receiving.UiText.WeightQuantity.Placeholder" "Weight/Quantity - Placeholder" "Enter whole number"

# Heat/Lot UI Text
Add-Entry "Receiving.UiText.HeatLot.Header" "Heat/Lot - Header" "Load Entries"
Add-Entry "Receiving.UiText.HeatLot.AutoFill" "Heat/Lot - Auto-Fill" "Auto-Fill"
Add-Entry "Receiving.UiText.HeatLot.AutoFillTooltip" "Heat/Lot - Auto-Fill Tooltip" "Fill blank heat numbers from rows above"
Add-Entry "Receiving.UiText.HeatLot.LoadPrefix" "Heat/Lot - Load Prefix" "Load #{0}"
Add-Entry "Receiving.UiText.HeatLot.FieldHeader" "Heat/Lot - Field Header" "Heat/Lot Number (Optional)"
Add-Entry "Receiving.UiText.HeatLot.FieldPlaceholder" "Heat/Lot - Field Placeholder" "Enter heat/lot number or leave blank"

# Package Type UI Text
Add-Entry "Receiving.UiText.PackageType.Header" "Package Type - Header" "Package Type (Applied to all loads)"
Add-Entry "Receiving.UiText.PackageType.ComboHeader" "Package Type - Combo Header" "Type"
Add-Entry "Receiving.UiText.PackageType.CustomHeader" "Package Type - Custom Header" "Custom Name"
Add-Entry "Receiving.UiText.PackageType.SaveAsDefault" "Package Type - Save as Default" "Save as default for this part"
Add-Entry "Receiving.UiText.PackageType.LoadNumberPrefix" "Package Type - Load Number Prefix" "#{0}"
Add-Entry "Receiving.UiText.PackageType.PackagesPerLoad" "Package Type - Packages Per Load" "Packages per Load"
Add-Entry "Receiving.UiText.PackageType.WeightPerPackage" "Package Type - Weight Per Package" "Weight per Package"

# Review UI Text
Add-Entry "Receiving.UiText.Review.EntryLabel" "Review - Entry Label" "Entry"
Add-Entry "Receiving.UiText.Review.OfLabel" "Review - Of Label" "of"
Add-Entry "Receiving.UiText.Review.LoadNumber" "Review - Load Number" "Load Number"
Add-Entry "Receiving.UiText.Review.PurchaseOrderNumber" "Review - Purchase Order Number" "Purchase Order Number"
Add-Entry "Receiving.UiText.Review.PartId" "Review - Part ID" "Part ID"
Add-Entry "Receiving.UiText.Review.RemainingQuantity" "Review - Remaining Quantity" "Remaining Quantity"
Add-Entry "Receiving.UiText.Review.WeightQuantity" "Review - Weight/Quantity" "Weight/Quantity"
Add-Entry "Receiving.UiText.Review.HeatLotNumber" "Review - Heat/Lot Number" "Heat/Lot Number"
Add-Entry "Receiving.UiText.Review.PackagesPerLoad" "Review - Packages Per Load" "Packages Per Load"
Add-Entry "Receiving.UiText.Review.PackageType" "Review - Package Type" "Package Type"
Add-Entry "Receiving.UiText.Review.PreviousLabel" "Review - Previous Label" "Previous"
Add-Entry "Receiving.UiText.Review.NextLabel" "Review - Next Label" "Next"
Add-Entry "Receiving.UiText.Review.TableViewLabel" "Review - Table View Label" "Table View"
Add-Entry "Receiving.UiText.Review.SingleViewLabel" "Review - Single View Label" "Single View"
Add-Entry "Receiving.UiText.Review.AddAnother" "Review - Add Another" "Add Another Part/PO"
Add-Entry "Receiving.UiText.Review.SaveToDatabase" "Review - Save to Database" "Save to Database"
Add-Entry "Receiving.UiText.Review.Column.LoadNumber" "Review - Load # Column" "Load #"
Add-Entry "Receiving.UiText.Review.Column.PoNumber" "Review - PO Number Column" "PO Number"
Add-Entry "Receiving.UiText.Review.Column.PartId" "Review - Part ID Column" "Part ID"
Add-Entry "Receiving.UiText.Review.Column.RemainingQty" "Review - Remaining Qty Column" "Remaining Qty"
Add-Entry "Receiving.UiText.Review.Column.WeightQty" "Review - Weight/Qty Column" "Weight/Qty"
Add-Entry "Receiving.UiText.Review.Column.HeatLot" "Review - Heat/Lot Column" "Heat/Lot #"
Add-Entry "Receiving.UiText.Review.Column.Pkgs" "Review - Pkgs Column" "Pkgs"
Add-Entry "Receiving.UiText.Review.Column.PkgType" "Review - Pkg Type Column" "Pkg Type"

# Workflow Step Titles
Add-Entry "Receiving.Workflow.StepTitle.ModeSelection" "Workflow - Mode Selection Step" "Receiving - Mode Selection"
Add-Entry "Receiving.Workflow.StepTitle.ManualEntry" "Workflow - Manual Entry Step" "Receiving - Manual Entry"
Add-Entry "Receiving.Workflow.StepTitle.EditMode" "Workflow - Edit Mode Step" "Receiving - Edit Mode"
Add-Entry "Receiving.Workflow.StepTitle.PoEntry" "Workflow - PO Entry Step" "Receiving - Enter PO Number"
Add-Entry "Receiving.Workflow.StepTitle.PartSelection" "Workflow - Part Selection Step" "Receiving - Select Part"
Add-Entry "Receiving.Workflow.StepTitle.LoadEntry" "Workflow - Load Entry Step" "Receiving - Enter Number of Loads"
Add-Entry "Receiving.Workflow.StepTitle.WeightQuantity" "Workflow - Weight/Quantity Step" "Receiving - Enter Weight/Quantity"
Add-Entry "Receiving.Workflow.StepTitle.HeatLot" "Workflow - Heat/Lot Step" "Receiving - Enter Heat/Lot Numbers"
Add-Entry "Receiving.Workflow.StepTitle.PackageType" "Workflow - Package Type Step" "Receiving - Select Package Type"
Add-Entry "Receiving.Workflow.StepTitle.ReviewAndSave" "Workflow - Review & Save Step" "Receiving - Review & Save"
Add-Entry "Receiving.Workflow.StepTitle.Saving" "Workflow - Saving Step" "Receiving - Saving..."
Add-Entry "Receiving.Workflow.StepTitle.Complete" "Workflow - Complete Step" "Receiving - Complete"

# Workflow Dialogs
Add-Entry "Receiving.Workflow.SaveProgress.Initializing" "Workflow - Initializing" "Initializing..."
Add-Entry "Receiving.Workflow.SaveProgress.SavingCsv" "Workflow - Saving CSV" "Saving to local and network CSV..."
Add-Entry "Receiving.Workflow.Dialog.ResetCsv.Title" "Workflow - Reset CSV Dialog Title" "Reset CSV Files"
Add-Entry "Receiving.Workflow.Dialog.ResetCsv.Content" "Workflow - Reset CSV Dialog Content" "Are you sure you want to delete the local and network CSV files? This action cannot be undone."
Add-Entry "Receiving.Workflow.Dialog.ResetCsv.Delete" "Workflow - Reset CSV Dialog Delete" "Delete"
Add-Entry "Receiving.Workflow.Dialog.ResetCsv.Cancel" "Workflow - Reset CSV Dialog Cancel" "Cancel"
Add-Entry "Receiving.Workflow.Dialog.DbSaveFailed.Title" "Workflow - DB Save Failed Dialog Title" "Database Save Failed"
Add-Entry "Receiving.Workflow.Dialog.DbSaveFailed.DeleteAnyway" "Workflow - DB Save Failed Delete Anyway" "Delete Anyway"
Add-Entry "Receiving.Workflow.Dialog.DbSaveFailed.Cancel" "Workflow - DB Save Failed Cancel" "Cancel"
Add-Entry "Receiving.Workflow.Status.CsvDeletedSuccess" "Workflow - CSV Deleted Success" "CSV files deleted successfully."
Add-Entry "Receiving.Workflow.Status.CsvDeletedFailed" "Workflow - CSV Deleted Failed" "Failed to delete CSV files or files not found."
Add-Entry "Receiving.Workflow.Status.WorkflowCleared" "Workflow - Workflow Cleared" "Workflow cleared. Please select a mode."

# Dialogs
Add-Entry "Receiving.Dialogs.ConfirmModeSelection.Title" "Dialogs - Confirm Mode Selection Title" "Confirm Mode Selection"
Add-Entry "Receiving.Dialogs.ConfirmModeSelection.Content" "Dialogs - Confirm Mode Selection Content" "Selecting a new mode will reset all unsaved data in the current workflow. Do you want to continue?"
Add-Entry "Receiving.Dialogs.ConfirmModeSelection.Continue" "Dialogs - Confirm Mode Selection Continue" "Continue"
Add-Entry "Receiving.Dialogs.ConfirmModeSelection.Cancel" "Dialogs - Confirm Mode Selection Cancel" "Cancel"
Add-Entry "Receiving.Dialogs.ConfirmChangeMode.Title" "Dialogs - Confirm Change Mode Title" "Change Mode?"
Add-Entry "Receiving.Dialogs.ConfirmChangeMode.Content" "Dialogs - Confirm Change Mode Content" "Returning to mode selection will clear all current work in progress. This cannot be undone. Are you sure?"
Add-Entry "Receiving.Dialogs.ConfirmChangeMode.Confirm" "Dialogs - Confirm Change Mode Confirm" "Yes, Change Mode"
Add-Entry "Receiving.Dialogs.ConfirmChangeMode.Cancel" "Dialogs - Confirm Change Mode Cancel" "Cancel"
Add-Entry "Receiving.Dialogs.ManualEntry.AddMultiple.Title" "Dialogs - Manual Entry Add Multiple Title" "Add Multiple Rows"
Add-Entry "Receiving.Dialogs.ManualEntry.AddMultiple.Add" "Dialogs - Manual Entry Add Multiple Add" "Add"
Add-Entry "Receiving.Dialogs.ManualEntry.AddMultiple.Cancel" "Dialogs - Manual Entry Add Multiple Cancel" "Cancel"
Add-Entry "Receiving.Dialogs.Review.AddAnother.Title" "Dialogs - Review Add Another Title" "Add Another Part/PO"
Add-Entry "Receiving.Dialogs.Review.AddAnother.Content" "Dialogs - Review Add Another Content" "Current form data will be cleared to start a new entry. Your reviewed loads are preserved. Continue?"
Add-Entry "Receiving.Dialogs.Review.AddAnother.Continue" "Dialogs - Review Add Another Continue" "Continue"
Add-Entry "Receiving.Dialogs.Review.AddAnother.Cancel" "Dialogs - Review Add Another Cancel" "Cancel"

# Messages
Add-Entry "Receiving.Messages.Error.UnableToDisplayDialog" "Messages - Unable to Display Dialog" "Unable to display dialog"
Add-Entry "Receiving.Messages.Error.PoRequired" "Messages - PO Required" "Please enter a PO number."
Add-Entry "Receiving.Messages.Error.PartIdRequired" "Messages - Part ID Required" "Please enter a Part ID."
Add-Entry "Receiving.Messages.Error.PoNotFound" "Messages - PO Not Found" "PO not found or contains no parts."
Add-Entry "Receiving.Messages.Error.PartNotFound" "Messages - Part Not Found" "Part not found."
Add-Entry "Receiving.Messages.Info.PoLoadedWithParts" "Messages - PO Loaded With Parts" "Purchase Order {0} loaded with {1} parts."
Add-Entry "Receiving.Messages.Info.PartFound" "Messages - Part Found" "Part {0} found."
Add-Entry "Receiving.Messages.Warning.SameDayReceiving" "Messages - Same Day Receiving Warning" "Warning: {0:N2} of this part has already been received today on this PO."
Add-Entry "Receiving.Messages.Warning.ExceedsOrdered" "Messages - Exceeds Ordered Warning" "Warning: Total quantity ({0:N2}) exceeds PO ordered amount ({1:N2})."
Add-Entry "Receiving.Messages.Info.NonPoItem" "Messages - Non-PO Item" "Non-PO Item"

# Accessibility
Add-Entry "Receiving.Accessibility.ModeSelection.GuidedButton" "Accessibility - Mode Selection Guided" "Guided Wizard Mode"
Add-Entry "Receiving.Accessibility.ModeSelection.ManualButton" "Accessibility - Mode Selection Manual" "Manual Entry Mode"
Add-Entry "Receiving.Accessibility.ModeSelection.EditButton" "Accessibility - Mode Selection Edit" "Edit Mode"
Add-Entry "Receiving.Accessibility.PoEntry.PONumber" "Accessibility - PO Entry PO Number" "Purchase Order Number"
Add-Entry "Receiving.Accessibility.PoEntry.LoadPo" "Accessibility - PO Entry Load PO" "Load Purchase Order"
Add-Entry "Receiving.Accessibility.PoEntry.PartId" "Accessibility - PO Entry Part ID" "Part Identifier"
Add-Entry "Receiving.Accessibility.PoEntry.LookupPart" "Accessibility - PO Entry Lookup Part" "Look Up Part"
Add-Entry "Receiving.Accessibility.PoEntry.PartsList" "Accessibility - PO Entry Parts List" "Parts List"
Add-Entry "Receiving.Accessibility.LoadEntry.NumberOfLoads" "Accessibility - Load Entry Number of Loads" "Number of Loads"
Add-Entry "Receiving.Accessibility.WeightQuantity.Input" "Accessibility - Weight/Quantity Input" "Weight Quantity"
Add-Entry "Receiving.Accessibility.HeatLot.Number" "Accessibility - Heat/Lot Number" "Heat Lot Number"
Add-Entry "Receiving.Accessibility.PackageType.Combo" "Accessibility - Package Type Combo" "Package Type"
Add-Entry "Receiving.Accessibility.PackageType.CustomName" "Accessibility - Package Type Custom Name" "Custom Package Name"
Add-Entry "Receiving.Accessibility.PackageType.PackagesPerLoad" "Accessibility - Package Type Packages Per Load" "Packages per Load"
Add-Entry "Receiving.Accessibility.Review.PreviousEntry" "Accessibility - Review Previous Entry" "Previous Entry"
Add-Entry "Receiving.Accessibility.Review.NextEntry" "Accessibility - Review Next Entry" "Next Entry"
Add-Entry "Receiving.Accessibility.Review.SwitchToTable" "Accessibility - Review Switch to Table" "Switch to Table View"
Add-Entry "Receiving.Accessibility.Review.SwitchToSingle" "Accessibility - Review Switch to Single" "Switch to Single View"
Add-Entry "Receiving.Accessibility.Review.EntriesTable" "Accessibility - Review Entries Table" "Receiving Entries Table"
Add-Entry "Receiving.Accessibility.Review.AddAnother" "Accessibility - Review Add Another" "Add Another Part or PO"
Add-Entry "Receiving.Accessibility.Review.SaveToDatabase" "Accessibility - Review Save to Database" "Save to Database"

# Validation Settings
Add-Entry "Receiving.Validation.RequirePoNumber" "Validation - Require PO Number" "false" "Bool"
Add-Entry "Receiving.Validation.RequirePartId" "Validation - Require Part ID" "true" "Bool"
Add-Entry "Receiving.Validation.RequireQuantity" "Validation - Require Quantity" "true" "Bool"
Add-Entry "Receiving.Validation.RequireHeatLot" "Validation - Require Heat/Lot" "false" "Bool"
Add-Entry "Receiving.Validation.AllowNegativeQuantity" "Validation - Allow Negative Quantity" "false" "Bool"
Add-Entry "Receiving.Validation.ValidatePoExists" "Validation - Validate PO Exists" "true" "Bool"
Add-Entry "Receiving.Validation.ValidatePartExists" "Validation - Validate Part Exists" "true" "Bool"
Add-Entry "Receiving.Validation.WarnOnQuantityExceedsPo" "Validation - Warn on Quantity Exceeds PO" "true" "Bool"
Add-Entry "Receiving.Validation.WarnOnSameDayReceiving" "Validation - Warn on Same Day Receiving" "true" "Bool"
Add-Entry "Receiving.Validation.MinLoadCount" "Validation - Minimum Load Count" "1" "Int"
Add-Entry "Receiving.Validation.MaxLoadCount" "Validation - Maximum Load Count" "99" "Int"
Add-Entry "Receiving.Validation.MinQuantity" "Validation - Minimum Quantity" "0" "Int"
Add-Entry "Receiving.Validation.MaxQuantity" "Validation - Maximum Quantity" "999999" "Int"

# Business Rules Settings
Add-Entry "Receiving.BusinessRules.AutoSaveEnabled" "Business Rules - Auto Save Enabled" "false" "Bool"
Add-Entry "Receiving.BusinessRules.AutoSaveIntervalSeconds" "Business Rules - Auto Save Interval" "300" "Int"
Add-Entry "Receiving.BusinessRules.SaveToCsvEnabled" "Business Rules - Save to CSV Enabled" "true" "Bool"
Add-Entry "Receiving.BusinessRules.SaveToNetworkCsvEnabled" "Business Rules - Save to Network CSV Enabled" "true" "Bool"
Add-Entry "Receiving.BusinessRules.SaveToDatabaseEnabled" "Business Rules - Save to Database Enabled" "true" "Bool"
Add-Entry "Receiving.BusinessRules.DefaultModeOnStartup" "Business Rules - Default Mode on Startup" "ModeSelection"
Add-Entry "Receiving.BusinessRules.RememberLastMode" "Business Rules - Remember Last Mode" "true" "Bool" "User" "User"
Add-Entry "Receiving.BusinessRules.ConfirmModeChange" "Business Rules - Confirm Mode Change" "true" "Bool" "User" "User"
Add-Entry "Receiving.BusinessRules.AutoFillHeatLotEnabled" "Business Rules - Auto-Fill Heat/Lot Enabled" "true" "Bool"
Add-Entry "Receiving.BusinessRules.SavePackageTypeAsDefault" "Business Rules - Save Package Type as Default" "false" "Bool" "User" "User"
Add-Entry "Receiving.BusinessRules.ShowReviewTableByDefault" "Business Rules - Show Review Table by Default" "false" "Bool" "User" "User"
Add-Entry "Receiving.BusinessRules.AllowEditAfterSave" "Business Rules - Allow Edit After Save" "true" "Bool"

# Defaults Settings
Add-Entry "Receiving.Defaults.DefaultPackageType" "Defaults - Default Package Type" "Pallet"
Add-Entry "Receiving.Defaults.DefaultPackagesPerLoad" "Defaults - Default Packages Per Load" "1"
Add-Entry "Receiving.Defaults.DefaultWeightPerPackage" "Defaults - Default Weight Per Package" "0"
Add-Entry "Receiving.Defaults.DefaultUnitOfMeasure" "Defaults - Default Unit of Measure" "LBS"
Add-Entry "Receiving.Defaults.DefaultLocation" "Defaults - Default Location" "RECEIVING"
Add-Entry "Receiving.Defaults.DefaultLoadNumberPrefix" "Defaults - Default Load Number Prefix" "L"
Add-Entry "Receiving.Defaults.DefaultReceivingMode" "Defaults - Default Receiving Mode" "Guided"

# Integrations Settings
Add-Entry "Receiving.Integrations.ErpSyncEnabled" "Integrations - ERP Sync Enabled" "true" "Bool"
Add-Entry "Receiving.Integrations.AutoPullPoDataEnabled" "Integrations - Auto Pull PO Data Enabled" "true" "Bool"
Add-Entry "Receiving.Integrations.AutoPullPartDataEnabled" "Integrations - Auto Pull Part Data Enabled" "true" "Bool"
Add-Entry "Receiving.Integrations.SyncToInforVisual" "Integrations - Sync to Infor Visual" "false" "Bool"
Add-Entry "Receiving.Integrations.ErpConnectionTimeout" "Integrations - ERP Connection Timeout" "30" "Int"
Add-Entry "Receiving.Integrations.RetryFailedSyncs" "Integrations - Retry Failed Syncs" "true" "Bool"
Add-Entry "Receiving.Integrations.MaxSyncRetries" "Integrations - Max Sync Retries" "3" "Int"

Write-Host "`n✓ Created $($newEntries.Count) new manifest entries" -ForegroundColor Green

# Add to manifest
$allSettings = @($manifest.settings) + $newEntries
$manifest.settings = $allSettings

# Save
$manifest | ConvertTo-Json -Depth 10 | Set-Content $manifestPath -Encoding UTF8
Write-Host "✓ Updated manifest with $($allSettings.Count) total settings" -ForegroundColor Green

# Validate
try {
    $test = Get-Content $manifestPath -Raw | ConvertFrom-Json
    Write-Host "✓ Manifest JSON is valid" -ForegroundColor Green
}
catch {
    Write-Host "✗ ERROR: Manifest JSON is invalid: $_" -ForegroundColor Red
}

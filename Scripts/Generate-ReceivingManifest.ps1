# Generate Receiving Settings Manifest Entries
# This script generates JSON entries for all Receiving settings keys

$manifestEntries = @()

# Helper function to create a manifest entry
function New-ManifestEntry {
    param(
        [string]$Key,
        [string]$DisplayName,
        [string]$DefaultValue,
        [string]$DataType = "String",
        [string]$Scope = "System",
        [string]$PermissionLevel = "Admin"
    )
    
    return [PSCustomObject]@{
        category        = "Receiving"
        key             = $Key
        displayName     = $DisplayName
        defaultValue    = $DefaultValue
        dataType        = $DataType
        scope           = $Scope
        permissionLevel = $PermissionLevel
        isSensitive     = $false
        validationRules = $DataType
    }
}

# UiText.Workflow entries
$manifestEntries += New-ManifestEntry "Receiving.UiText.Workflow.Help" "Workflow - Help Button" "Help"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Workflow.Back" "Workflow - Back Button" "Back"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Workflow.Next" "Workflow - Next Button" "Next"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Workflow.ModeSelection" "Workflow - Mode Selection Button" "Mode Selection"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Workflow.ResetLabelTable" "Workflow - Reset Label Table Button" "Reset Label Table"

# UiText.Completion entries
$manifestEntries += New-ManifestEntry "Receiving.UiText.Completion.SuccessTitle" "Completion - Success Title" "Success!"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Completion.FailureTitle" "Completion - Failure Title" "Save Failed"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Completion.LoadsSavedSuffix" "Completion - Loads Saved Suffix" " loads saved successfully."
$manifestEntries += New-ManifestEntry "Receiving.UiText.Completion.SaveDetailsTitle" "Completion - Save Details Title" "Save Details:"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Completion.LocalLabelTableLabel" "Completion - Local Label Table Label" "Local Label Table:"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Completion.NetworkLabelTableLabel" "Completion - Network Label Table Label" "Network Label Table:"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Completion.DatabaseLabel" "Completion - Database Label" "Database:"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Completion.Saved" "Completion - Saved Status" "Saved"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Completion.Failed" "Completion - Failed Status" "Failed"
$manifestEntries += New-ManifestEntry "Receiving.UiText.Completion.StartNewEntry" "Completion - Start New Entry Button" "Start New Entry"

# UiText.ManualEntry entries
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.AddRow" "Manual Entry - Add Row Button" "Add Row"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.AddMultiple" "Manual Entry - Add Multiple Button" "Add Multiple"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.RemoveRow" "Manual Entry - Remove Row Button" "Remove Row"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.AutoFill" "Manual Entry - Auto-Fill Button" "Auto-Fill"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.SaveAndFinish" "Manual Entry - Save & Finish Button" "Save & Finish"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.Column.LoadNumber" "Manual Entry - Load # Column" "Load #"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.Column.PartId" "Manual Entry - Part ID Column" "Part ID"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.Column.WeightQty" "Manual Entry - Weight/Qty Column" "Weight/Qty"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.Column.HeatLot" "Manual Entry - Heat/Lot Column" "Heat/Lot"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.Column.PkgType" "Manual Entry - Pkg Type Column" "Pkg Type"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.Column.PkgsPerLoad" "Manual Entry - Pkgs/Load Column" "Pkgs/Load"
$manifestEntries += New-ManifestEntry "Receiving.UiText.ManualEntry.Column.WtPerPkg" "Manual Entry - Wt/Pkg Column" "Wt/Pkg"

# Validation entries
$manifestEntries += New-ManifestEntry "Receiving.Validation.RequirePoNumber" "Validation - Require PO Number" "false" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.RequirePartId" "Validation - Require Part ID" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.RequireQuantity" "Validation - Require Quantity" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.RequireHeatLot" "Validation - Require Heat/Lot" "false" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.AllowNegativeQuantity" "Validation - Allow Negative Quantity" "false" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.ValidatePoExists" "Validation - Validate PO Exists" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.ValidatePartExists" "Validation - Validate Part Exists" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.WarnOnQuantityExceedsPo" "Validation - Warn on Quantity Exceeds PO" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.WarnOnSameDayReceiving" "Validation - Warn on Same Day Receiving" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.MinLoadCount" "Validation - Minimum Load Count" "1" "Int" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.MaxLoadCount" "Validation - Maximum Load Count" "99" "Int" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.MinQuantity" "Validation - Minimum Quantity" "0" "Int" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Validation.MaxQuantity" "Validation - Maximum Quantity" "999999" "Int" "System" "Admin"

# BusinessRules entries
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.AutoSaveEnabled" "Business Rules - Auto Save Enabled" "false" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.AutoSaveIntervalSeconds" "Business Rules - Auto Save Interval (Seconds)" "300" "Int" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.SaveToLabelTableEnabled" "Business Rules - Save to Label Table Enabled" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.SaveToNetworkLabelTableEnabled" "Business Rules - Save to Network Label Table Enabled" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.SaveToDatabaseEnabled" "Business Rules - Save to Database Enabled" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.DefaultModeOnStartup" "Business Rules - Default Mode on Startup" "ModeSelection" "String" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.RememberLastMode" "Business Rules - Remember Last Mode" "true" "Bool" "User" "User"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.ConfirmModeChange" "Business Rules - Confirm Mode Change" "true" "Bool" "User" "User"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.AutoFillHeatLotEnabled" "Business Rules - Auto-Fill Heat/Lot Enabled" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.SavePackageTypeAsDefault" "Business Rules - Save Package Type as Default" "false" "Bool" "User" "User"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.ShowReviewTableByDefault" "Business Rules - Show Review Table by Default" "false" "Bool" "User" "User"
$manifestEntries += New-ManifestEntry "Receiving.BusinessRules.AllowEditAfterSave" "Business Rules - Allow Edit After Save" "true" "Bool" "System" "Admin"

# Defaults entries
$manifestEntries += New-ManifestEntry "Receiving.Defaults.DefaultPackageType" "Defaults - Default Package Type" "Pallet" "String" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Defaults.DefaultPackagesPerLoad" "Defaults - Default Packages Per Load" "1" "String" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Defaults.DefaultWeightPerPackage" "Defaults - Default Weight Per Package" "0" "String" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Defaults.DefaultUnitOfMeasure" "Defaults - Default Unit of Measure" "LBS" "String" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Defaults.DefaultLocation" "Defaults - Default Location" "RECEIVING" "String" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Defaults.DefaultLoadNumberPrefix" "Defaults - Default Load Number Prefix" "L" "String" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Defaults.DefaultReceivingMode" "Defaults - Default Receiving Mode" "Guided" "String" "System" "Admin"

# Integrations entries
$manifestEntries += New-ManifestEntry "Receiving.Integrations.ErpSyncEnabled" "Integrations - ERP Sync Enabled" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Integrations.AutoPullPoDataEnabled" "Integrations - Auto Pull PO Data Enabled" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Integrations.AutoPullPartDataEnabled" "Integrations - Auto Pull Part Data Enabled" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Integrations.SyncToInforVisual" "Integrations - Sync to Infor Visual" "false" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Integrations.ErpConnectionTimeout" "Integrations - ERP Connection Timeout (Seconds)" "30" "Int" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Integrations.RetryFailedSyncs" "Integrations - Retry Failed Syncs" "true" "Bool" "System" "Admin"
$manifestEntries += New-ManifestEntry "Receiving.Integrations.MaxSyncRetries" "Integrations - Max Sync Retries" "3" "Int" "System" "Admin"

# Output as JSON
$manifestEntries | ConvertTo-Json -Depth 10

Write-Host "`n✓ Generated $($manifestEntries.Count) manifest entries" -ForegroundColor Green

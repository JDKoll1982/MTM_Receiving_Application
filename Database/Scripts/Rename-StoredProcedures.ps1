# ============================================================================
# Script: Rename-StoredProcedures.ps1
# Purpose: Standardize SP naming convention and update all references
# Convention: sp_<Module>_<Entity>_<Action>
# Example: sp_Dunnage_CustomFields_Insert
# ============================================================================

param(
    [switch]$WhatIf = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"
$repoRoot = "c:\Users\johnk\source\repos\MTM_Receiving_Application"
$spFolder = Join-Path $repoRoot "Database\StoredProcedures"

# Define naming convention mappings for each module
$moduleConventions = @{
    "Authentication" = @{
        Prefix   = "sp_Auth"
        Mappings = @{
            "sp_CreateNewUser.sql"                      = "sp_Auth_User_Create.sql"
            "sp_get_user_default_mode.sql"              = "sp_Auth_User_GetDefaultMode.sql"
            "sp_GetDepartments.sql"                     = "sp_Auth_Department_GetAll.sql"
            "sp_GetSharedTerminalNames.sql"             = "sp_Auth_Terminal_GetShared.sql"
            "sp_GetUserByWindowsUsername.sql"           = "sp_Auth_User_GetByWindowsUsername.sql"
            "sp_LogUserActivity.sql"                    = "sp_Auth_Activity_Log.sql"
            "sp_seed_user_default_modes.sql"            = "sp_Auth_User_SeedDefaultModes.sql"
            "sp_update_user_default_dunnage_mode.sql"   = "sp_Auth_User_UpdateDefaultDunnageMode.sql"
            "sp_update_user_default_mode.sql"           = "sp_Auth_User_UpdateDefaultMode.sql"
            "sp_update_user_default_receiving_mode.sql" = "sp_Auth_User_UpdateDefaultReceivingMode.sql"
            "sp_UpsertUser.sql"                         = "sp_Auth_User_Upsert.sql"
            "sp_UpsertWorkstationConfig.sql"            = "sp_Auth_Workstation_Upsert.sql"
            "sp_ValidateUserPin.sql"                    = "sp_Auth_User_ValidatePin.sql"
        }
    }
    "Dunnage"        = @{
        Prefix   = "sp_Dunnage"
        Mappings = @{
            "sp_custom_fields_insert.sql"                 = "sp_Dunnage_CustomFields_Insert.sql"
            "sp_dunnage_custom_fields_get_by_type.sql"    = "sp_Dunnage_CustomFields_GetByType.sql"
            "sp_dunnage_line_Insert.sql"                  = "sp_Dunnage_Line_Insert.sql"
            "sp_dunnage_loads_delete.sql"                 = "sp_Dunnage_Loads_Delete.sql"
            "sp_dunnage_loads_get_all.sql"                = "sp_Dunnage_Loads_GetAll.sql"
            "sp_dunnage_loads_get_by_date_range.sql"      = "sp_Dunnage_Loads_GetByDateRange.sql"
            "sp_dunnage_loads_get_by_id.sql"              = "sp_Dunnage_Loads_GetById.sql"
            "sp_dunnage_loads_insert_batch.sql"           = "sp_Dunnage_Loads_InsertBatch.sql"
            "sp_dunnage_loads_insert.sql"                 = "sp_Dunnage_Loads_Insert.sql"
            "sp_dunnage_loads_update.sql"                 = "sp_Dunnage_Loads_Update.sql"
            "sp_dunnage_parts_count_transactions.sql"     = "sp_Dunnage_Parts_CountTransactions.sql"
            "sp_dunnage_parts_delete.sql"                 = "sp_Dunnage_Parts_Delete.sql"
            "sp_dunnage_parts_get_all.sql"                = "sp_Dunnage_Parts_GetAll.sql"
            "sp_dunnage_parts_get_by_id.sql"              = "sp_Dunnage_Parts_GetById.sql"
            "sp_dunnage_parts_get_by_type.sql"            = "sp_Dunnage_Parts_GetByType.sql"
            "sp_dunnage_parts_get_transaction_count.sql"  = "sp_Dunnage_Parts_GetTransactionCount.sql"
            "sp_dunnage_parts_insert.sql"                 = "sp_Dunnage_Parts_Insert.sql"
            "sp_dunnage_parts_search.sql"                 = "sp_Dunnage_Parts_Search.sql"
            "sp_dunnage_parts_update.sql"                 = "sp_Dunnage_Parts_Update.sql"
            "sp_dunnage_specs_count_parts_using_spec.sql" = "sp_Dunnage_Specs_CountPartsUsingSpec.sql"
            "sp_dunnage_specs_delete_by_id.sql"           = "sp_Dunnage_Specs_DeleteById.sql"
            "sp_dunnage_specs_delete_by_type.sql"         = "sp_Dunnage_Specs_DeleteByType.sql"
            "sp_dunnage_specs_get_all_keys.sql"           = "sp_Dunnage_Specs_GetAllKeys.sql"
            "sp_dunnage_specs_get_all.sql"                = "sp_Dunnage_Specs_GetAll.sql"
            "sp_dunnage_specs_get_by_id.sql"              = "sp_Dunnage_Specs_GetById.sql"
            "sp_dunnage_specs_get_by_type.sql"            = "sp_Dunnage_Specs_GetByType.sql"
            "sp_dunnage_specs_insert.sql"                 = "sp_Dunnage_Specs_Insert.sql"
            "sp_dunnage_specs_update.sql"                 = "sp_Dunnage_Specs_Update.sql"
            "sp_dunnage_type_Delete.sql"                  = "sp_Dunnage_Types_Delete.sql"
            "sp_dunnage_type_GetAll.sql"                  = "sp_Dunnage_Types_GetAll.sql"
            "sp_dunnage_type_GetById.sql"                 = "sp_Dunnage_Types_GetById.sql"
            "sp_dunnage_type_Insert.sql"                  = "sp_Dunnage_Types_Insert.sql"
            "sp_dunnage_type_Update.sql"                  = "sp_Dunnage_Types_Update.sql"
            "sp_dunnage_type_UsageCount.sql"              = "sp_Dunnage_Types_GetUsageCount.sql"
            "sp_dunnage_types_check_duplicate.sql"        = "sp_Dunnage_Types_CheckDuplicate.sql"
            "sp_dunnage_types_count_parts.sql"            = "sp_Dunnage_Types_CountParts.sql"
            "sp_dunnage_types_count_transactions.sql"     = "sp_Dunnage_Types_CountTransactions.sql"
            "sp_dunnage_types_delete.sql"                 = "sp_Dunnage_Types_Delete.sql"
            "sp_dunnage_types_get_all.sql"                = "sp_Dunnage_Types_GetAll.sql"
            "sp_dunnage_types_get_by_id.sql"              = "sp_Dunnage_Types_GetById.sql"
            "sp_dunnage_types_get_part_count.sql"         = "sp_Dunnage_Types_GetPartCount.sql"
            "sp_dunnage_types_get_transaction_count.sql"  = "sp_Dunnage_Types_GetTransactionCount.sql"
            "sp_dunnage_types_insert.sql"                 = "sp_Dunnage_Types_Insert.sql"
            "sp_dunnage_types_update.sql"                 = "sp_Dunnage_Types_Update.sql"
            "sp_inventoried_dunnage_check.sql"            = "sp_Dunnage_Inventory_Check.sql"
            "sp_inventoried_dunnage_delete.sql"           = "sp_Dunnage_Inventory_Delete.sql"
            "sp_inventoried_dunnage_get_all.sql"          = "sp_Dunnage_Inventory_GetAll.sql"
            "sp_inventoried_dunnage_get_by_part.sql"      = "sp_Dunnage_Inventory_GetByPart.sql"
            "sp_inventoried_dunnage_insert.sql"           = "sp_Dunnage_Inventory_Insert.sql"
            "sp_inventoried_dunnage_update.sql"           = "sp_Dunnage_Inventory_Update.sql"
            "sp_user_preferences_get_recent_icons.sql"    = "sp_Dunnage_UserPreferences_GetRecentIcons.sql"
            "sp_user_preferences_upsert.sql"              = "sp_Dunnage_UserPreferences_Upsert.sql"
        }
    }
    "Routing"        = @{
        Prefix   = "sp_Routing"
        Note     = "Already standardized - keeping sp_routing_* lowercase pattern"
        Mappings = @{}
    }
    "Settings"       = @{
        Prefix   = "sp_Settings"
        Mappings = @{
            "sp_RoutingRule_Delete.sql"            = "sp_Settings_RoutingRule_Delete.sql"
            "sp_RoutingRule_FindMatch.sql"         = "sp_Settings_RoutingRule_FindMatch.sql"
            "sp_RoutingRule_GetAll.sql"            = "sp_Settings_RoutingRule_GetAll.sql"
            "sp_RoutingRule_GetById.sql"           = "sp_Settings_RoutingRule_GetById.sql"
            "sp_RoutingRule_GetByPartNumber.sql"   = "sp_Settings_RoutingRule_GetByPartNumber.sql"
            "sp_RoutingRule_Insert.sql"            = "sp_Settings_RoutingRule_Insert.sql"
            "sp_RoutingRule_Update.sql"            = "sp_Settings_RoutingRule_Update.sql"
            "sp_ScheduledReport_Delete.sql"        = "sp_Settings_ScheduledReport_Delete.sql"
            "sp_ScheduledReport_GetActive.sql"     = "sp_Settings_ScheduledReport_GetActive.sql"
            "sp_ScheduledReport_GetAll.sql"        = "sp_Settings_ScheduledReport_GetAll.sql"
            "sp_ScheduledReport_GetById.sql"       = "sp_Settings_ScheduledReport_GetById.sql"
            "sp_ScheduledReport_GetDue.sql"        = "sp_Settings_ScheduledReport_GetDue.sql"
            "sp_ScheduledReport_Insert.sql"        = "sp_Settings_ScheduledReport_Insert.sql"
            "sp_ScheduledReport_ToggleActive.sql"  = "sp_Settings_ScheduledReport_ToggleActive.sql"
            "sp_ScheduledReport_Update.sql"        = "sp_Settings_ScheduledReport_Update.sql"
            "sp_ScheduledReport_UpdateLastRun.sql" = "sp_Settings_ScheduledReport_UpdateLastRun.sql"
            "sp_SettingsAuditLog_Get.sql"          = "sp_Settings_AuditLog_Get.sql"
            "sp_SettingsAuditLog_GetBySetting.sql" = "sp_Settings_AuditLog_GetBySetting.sql"
            "sp_SettingsAuditLog_GetByUser.sql"    = "sp_Settings_AuditLog_GetByUser.sql"
            "sp_SystemSettings_GetAll.sql"         = "sp_Settings_System_GetAll.sql"
            "sp_SystemSettings_GetByCategory.sql"  = "sp_Settings_System_GetByCategory.sql"
            "sp_SystemSettings_GetByKey.sql"       = "sp_Settings_System_GetByKey.sql"
            "sp_SystemSettings_ResetToDefault.sql" = "sp_Settings_System_ResetToDefault.sql"
            "sp_SystemSettings_SetLocked.sql"      = "sp_Settings_System_SetLocked.sql"
            "sp_SystemSettings_UpdateValue.sql"    = "sp_Settings_System_UpdateValue.sql"
            "sp_UserSettings_Get.sql"              = "sp_Settings_User_Get.sql"
            "sp_UserSettings_GetAllForUser.sql"    = "sp_Settings_User_GetAllForUser.sql"
            "sp_UserSettings_Reset.sql"            = "sp_Settings_User_Reset.sql"
            "sp_UserSettings_ResetAll.sql"         = "sp_Settings_User_ResetAll.sql"
            "sp_UserSettings_Set.sql"              = "sp_Settings_User_Set.sql"
        }
    }
    "Volvo"          = @{
        Prefix   = "sp_Volvo"
        Mappings = @{
            "sp_volvo_part_component_delete_by_parent.sql" = "sp_Volvo_PartComponent_DeleteByParent.sql"
            "sp_volvo_part_component_get.sql"              = "sp_Volvo_PartComponent_Get.sql"
            "sp_volvo_part_component_insert.sql"           = "sp_Volvo_PartComponent_Insert.sql"
            "sp_volvo_part_master_get_all.sql"             = "sp_Volvo_PartMaster_GetAll.sql"
            "sp_volvo_part_master_get_by_id.sql"           = "sp_Volvo_PartMaster_GetById.sql"
            "sp_volvo_part_master_insert.sql"              = "sp_Volvo_PartMaster_Insert.sql"
            "sp_volvo_part_master_set_active.sql"          = "sp_Volvo_PartMaster_SetActive.sql"
            "sp_volvo_part_master_update.sql"              = "sp_Volvo_PartMaster_Update.sql"
            "sp_volvo_settings_get.sql"                    = "sp_Volvo_Settings_Get.sql"
            "sp_volvo_settings_get_all.sql"                = "sp_Volvo_Settings_GetAll.sql"
            "sp_volvo_settings_reset.sql"                  = "sp_Volvo_Settings_Reset.sql"
            "sp_volvo_settings_upsert.sql"                 = "sp_Volvo_Settings_Upsert.sql"
            "sp_volvo_shipment_complete.sql"               = "sp_Volvo_Shipment_Complete.sql"
            "sp_volvo_shipment_delete.sql"                 = "sp_Volvo_Shipment_Delete.sql"
            "sp_volvo_shipment_get_pending.sql"            = "sp_Volvo_Shipment_GetPending.sql"
            "sp_volvo_shipment_history_get.sql"            = "sp_Volvo_Shipment_GetHistory.sql"
            "sp_volvo_shipment_insert.sql"                 = "sp_Volvo_Shipment_Insert.sql"
            "sp_volvo_shipment_line_delete.sql"            = "sp_Volvo_ShipmentLine_Delete.sql"
            "sp_volvo_shipment_line_get_by_shipment.sql"   = "sp_Volvo_ShipmentLine_GetByShipment.sql"
            "sp_volvo_shipment_line_insert.sql"            = "sp_Volvo_ShipmentLine_Insert.sql"
            "sp_volvo_shipment_line_update.sql"            = "sp_Volvo_ShipmentLine_Update.sql"
            "sp_volvo_shipment_update.sql"                 = "sp_Volvo_Shipment_Update.sql"
        }
    }
    "Receiving"      = @{
        Prefix   = "sp_Receiving"
        Mappings = @{
            "receiving_line_Insert.sql"          = "sp_Receiving_Line_Insert.sql"
            "sp_DeletePackageTypePreference.sql" = "sp_Receiving_PackageTypePreference_Delete.sql"
            "sp_DeleteReceivingLoad.sql"         = "sp_Receiving_Load_Delete.sql"
            "sp_GetAllReceivingLoads.sql"        = "sp_Receiving_Load_GetAll.sql"
            "sp_GetPackageTypePreference.sql"    = "sp_Receiving_PackageTypePreference_Get.sql"
            "sp_GetReceivingHistory.sql"         = "sp_Receiving_History_Get.sql"
            "sp_InsertReceivingLoad.sql"         = "sp_Receiving_Load_Insert.sql"
            "sp_SavePackageTypePreference.sql"   = "sp_Receiving_PackageTypePreference_Save.sql"
            "sp_UpdateReceivingLoad.sql"         = "sp_Receiving_Load_Update.sql"
        }
        Note     = "sp_Receiving_PackageTypeMappings_* already follow convention"
    }
}

function Get-StoredProcedureName {
    param([string]$FilePath)

    $content = Get-Content $FilePath -Raw
    if ($content -match 'CREATE PROCEDURE\s+`?([^`\s(]+)`?') {
        return $Matches[1]
    }
    return $null
}

function Update-StoredProcedureFile {
    param(
        [string]$FilePath,
        [string]$OldName,
        [string]$NewName
    )

    $content = Get-Content $FilePath -Raw

    # Replace in DROP PROCEDURE
    $content = $content -replace "DROP PROCEDURE IF EXISTS\s+`?$([regex]::Escape($OldName))`?", "DROP PROCEDURE IF EXISTS ``$NewName``"

    # Replace in CREATE PROCEDURE
    $content = $content -replace "CREATE PROCEDURE\s+`?$([regex]::Escape($OldName))`?", "CREATE PROCEDURE ``$NewName``"

    Set-Content $FilePath $content -NoNewline
}

function Find-AllReferences {
    param(
        [string]$OldName,
        [string]$SearchPath
    )

    $references = @()
    $extensions = @("*.cs", "*.sql", "*.xaml", "*.json", "*.md")

    foreach ($ext in $extensions) {
        $files = Get-ChildItem -Path $SearchPath -Filter $ext -Recurse -File -ErrorAction SilentlyContinue
        foreach ($file in $files) {
            $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
            if ($content -match [regex]::Escape($OldName)) {
                $references += $file.FullName
            }
        }
    }

    return $references
}

function Update-FileReferences {
    param(
        [string]$FilePath,
        [string]$OldName,
        [string]$NewName
    )

    $content = Get-Content $FilePath -Raw
    $updated = $content -replace [regex]::Escape($OldName), $NewName

    if ($content -ne $updated) {
        Set-Content $FilePath $updated -NoNewline
        return $true
    }
    return $false
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Stored Procedure Standardization Tool" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

if ($WhatIf) {
    Write-Host "[WHATIF MODE] No changes will be made" -ForegroundColor Yellow
    Write-Host ""
}

$renameLog = @()

# Process each module
foreach ($module in $moduleConventions.Keys) {
    $moduleFolder = Join-Path $spFolder $module

    if (-not (Test-Path $moduleFolder)) {
        Write-Host "[-] Module folder not found: $module" -ForegroundColor Red
        continue
    }

    Write-Host "[+] Processing module: $module" -ForegroundColor Green

    $mappings = $moduleConventions[$module].Mappings
    if (-not $mappings) {
        Write-Host "    [!] No mappings defined - skipping" -ForegroundColor Yellow
        continue
    }

    foreach ($oldFile in $mappings.Keys) {
        $newFile = $mappings[$oldFile]
        $oldPath = Join-Path $moduleFolder $oldFile
        $newPath = Join-Path $moduleFolder $newFile

        if (-not (Test-Path $oldPath)) {
            Write-Host "    [!] File not found: $oldFile" -ForegroundColor Yellow
            continue
        }

        # Extract SP names from files
        $oldSpName = Get-StoredProcedureName $oldPath
        $newSpName = [System.IO.Path]::GetFileNameWithoutExtension($newFile)

        if (-not $oldSpName) {
            Write-Host "    [!] Could not extract SP name from: $oldFile" -ForegroundColor Yellow
            continue
        }

        Write-Host "    [*] $oldFile -> $newFile" -ForegroundColor Cyan
        Write-Host "        SP: $oldSpName -> $newSpName" -ForegroundColor Gray

        # Step 1: Update SP definition inside file
        if (-not $WhatIf) {
            Update-StoredProcedureFile -FilePath $oldPath -OldName $oldSpName -NewName $newSpName
        }

        # Step 2: Find all references in codebase
        Write-Host "        [*] Searching for references..." -ForegroundColor Gray
        $references = Find-AllReferences -OldName $oldSpName -SearchPath $repoRoot

        if ($references.Count -gt 0) {
            Write-Host "        [+] Found $($references.Count) reference(s)" -ForegroundColor Green

            # Update references
            foreach ($refFile in $references) {
                $relativePath = $refFile.Replace($repoRoot, "").TrimStart('\')
                if (-not $WhatIf) {
                    $updated = Update-FileReferences -FilePath $refFile -OldName $oldSpName -NewName $newSpName
                    if ($updated) {
                        Write-Host "            [✓] Updated: $relativePath" -ForegroundColor Green
                    }
                } else {
                    Write-Host "            [WHATIF] Would update: $relativePath" -ForegroundColor Yellow
                }
            }
        } else {
            Write-Host "        [-] No references found" -ForegroundColor Gray
        }

        # Step 3: Rename file
        if (-not $WhatIf) {
            Move-Item $oldPath $newPath -Force
            Write-Host "        [✓] File renamed" -ForegroundColor Green
        } else {
            Write-Host "        [WHATIF] Would rename file" -ForegroundColor Yellow
        }

        # Log the change
        $renameLog += [PSCustomObject]@{
            Module     = $module
            OldFile    = $oldFile
            NewFile    = $newFile
            OldSP      = $oldSpName
            NewSP      = $newSpName
            References = $references.Count
        }

        Write-Host ""
    }
}

# ============================================================================
# SUMMARY
# ============================================================================

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total SPs processed: $($renameLog.Count)" -ForegroundColor Green
Write-Host "Total references updated: $($renameLog | Measure-Object -Property References -Sum | Select-Object -ExpandProperty Sum)" -ForegroundColor Green
Write-Host ""

# Export log
$logPath = Join-Path $repoRoot "Database\Scripts\sp-rename-log.csv"
$renameLog | Export-Csv $logPath -NoTypeInformation
Write-Host "[+] Detailed log exported to: $logPath" -ForegroundColor Green

Write-Host ""
Write-Host "Done!" -ForegroundColor Green

# Preview script - shows what will be renamed
$scriptPath = Join-Path $PSScriptRoot "Rename-StoredProcedures.ps1"
& $scriptPath -WhatIf

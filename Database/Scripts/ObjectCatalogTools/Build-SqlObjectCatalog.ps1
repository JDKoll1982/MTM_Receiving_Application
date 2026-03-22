[CmdletBinding()]
param(
    [string]$RepoRoot = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)),
    [string]$CatalogPath = (Join-Path $PSScriptRoot 'sql-object-catalog.json')
)

Set-StrictMode -Version Latest

. (Join-Path $PSScriptRoot 'SqlObjectCatalogTools.ps1')

$databaseRoot = Join-Path $RepoRoot 'Database'
$catalog = Save-SqlObjectCatalog -RepoRoot $RepoRoot -DatabaseRoot $databaseRoot -OutputPath $CatalogPath

[PSCustomObject]@{
    CatalogPath = $CatalogPath
    EntryCount  = $catalog.entries.Count
    GeneratedAt = $catalog.generatedAt
}

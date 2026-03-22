# ============================================================================
# Script: New-CopilotFormsModuleMetadataReport.ps1
# Purpose: Generate a report comparing live module files to CopilotForms split
#          metadata without modifying any metadata files.
# ============================================================================

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$ModuleName,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "outputs")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-RelativePath {
    param(
        [Parameter(Mandatory)]
        [string]$BasePath,

        [Parameter(Mandatory)]
        [string]$TargetPath
    )

    $baseUri = [System.Uri]([System.IO.Path]::GetFullPath($BasePath).TrimEnd('\') + '\')
    $targetUri = [System.Uri]([System.IO.Path]::GetFullPath($TargetPath))
    $relativeUri = $baseUri.MakeRelativeUri($targetUri)
    return [System.Uri]::UnescapeDataString($relativeUri.ToString()).Replace('/', '\')
}

function Get-ModuleSourceFiles {
    param(
        [Parameter(Mandatory)]
        [string]$ModulePath,

        [Parameter(Mandatory)]
        [string]$RepoRootPath
    )

    if (-not (Test-Path $ModulePath)) {
        throw "Module path not found: $ModulePath"
    }

    $includeExtensions = @('.cs', '.xaml', '.json')
    $includeFileNames = @('README.md', 'README_CQRS_INFRASTRUCTURE.md', 'SETTABLE_OBJECTS_REPORT.md', 'SETTABLE_OBJECTS_INVENTORY.md')

    $files = Get-ChildItem -Path $ModulePath -Recurse -File | Where-Object {
        $includeExtensions -contains $_.Extension -or $includeFileNames -contains $_.Name
    }

    return $files | ForEach-Object {
        Get-RelativePath -BasePath $RepoRootPath -TargetPath $_.FullName
    } | Sort-Object -Unique
}

function Get-MetadataServiceReferences {
    param(
        [Parameter(Mandatory)]
        [object]$MetadataObject
    )

    $allReferences = New-Object System.Collections.Generic.List[string]

    $relatedServicesProperty = $MetadataObject.PSObject.Properties['relatedServices']
    if ($null -ne $relatedServicesProperty -and $relatedServicesProperty.Value) {
        foreach ($serviceName in $relatedServicesProperty.Value) {
            if (-not [string]::IsNullOrWhiteSpace($serviceName)) {
                $allReferences.Add([string]$serviceName)
            }
        }
    }

    $subFeaturesProperty = $MetadataObject.PSObject.Properties['subFeatures']
    if ($null -ne $subFeaturesProperty -and $subFeaturesProperty.Value) {
        foreach ($subFeature in $subFeaturesProperty.Value) {
            $subFeatureServicesProperty = $subFeature.PSObject.Properties['relatedServices']
            if ($null -ne $subFeatureServicesProperty -and $subFeatureServicesProperty.Value) {
                foreach ($serviceName in $subFeatureServicesProperty.Value) {
                    if (-not [string]::IsNullOrWhiteSpace($serviceName)) {
                        $allReferences.Add([string]$serviceName)
                    }
                }
            }
        }
    }

    return $allReferences | Sort-Object -Unique
}

function Get-MetadataFileReferences {
    param(
        [Parameter(Mandatory)]
        [object]$MetadataObject
    )

    $allReferences = New-Object System.Collections.Generic.List[string]

    $relatedFilesProperty = $MetadataObject.PSObject.Properties['relatedFiles']
    if ($null -ne $relatedFilesProperty -and $relatedFilesProperty.Value) {
        foreach ($filePath in $relatedFilesProperty.Value) {
            if (-not [string]::IsNullOrWhiteSpace($filePath)) {
                $allReferences.Add(([string]$filePath).Replace('/', '\'))
            }
        }
    }

    $subFeaturesProperty = $MetadataObject.PSObject.Properties['subFeatures']
    if ($null -ne $subFeaturesProperty -and $subFeaturesProperty.Value) {
        foreach ($subFeature in $subFeaturesProperty.Value) {
            $subFeatureFilesProperty = $subFeature.PSObject.Properties['relatedFiles']
            if ($null -ne $subFeatureFilesProperty -and $subFeatureFilesProperty.Value) {
                foreach ($filePath in $subFeatureFilesProperty.Value) {
                    if (-not [string]::IsNullOrWhiteSpace($filePath)) {
                        $allReferences.Add(([string]$filePath).Replace('/', '\'))
                    }
                }
            }
        }
    }

    return $allReferences | Sort-Object -Unique
}

function Get-ExpectedServiceContractPath {
    param(
        [Parameter(Mandatory)]
        [string]$ModuleNameValue,

        [Parameter(Mandatory)]
        [string]$ServiceName
    )

    return Join-Path $ModuleNameValue (Join-Path 'Contracts' ($ServiceName + '.cs'))
}

function New-ModuleMetadataReport {
    param(
        [Parameter(Mandatory)]
        [string]$ModuleNameValue,

        [Parameter(Mandatory)]
        [string]$RepositoryRoot
    )

    $modulePath = Join-Path $RepositoryRoot $ModuleNameValue
    $metadataPath = Join-Path $RepositoryRoot (Join-Path 'docs\CopilotForms\data\module-metadata' $ModuleNameValue)
    $indexPath = Join-Path $metadataPath 'index.json'

    if (-not (Test-Path $metadataPath)) {
        throw "Module metadata folder not found: $metadataPath"
    }

    if (-not (Test-Path $indexPath)) {
        throw "Module metadata index not found: $indexPath"
    }

    $index = Get-Content -Path $indexPath -Raw | ConvertFrom-Json
    $sourceFiles = Get-ModuleSourceFiles -ModulePath $modulePath -RepoRootPath $RepositoryRoot
    $metadataJsonFiles = Get-ChildItem -Path $metadataPath -File -Filter '*.json' | Where-Object {
        $_.Name -ne 'index.json'
    } | Sort-Object Name

    $featureReports = New-Object System.Collections.Generic.List[object]
    $allReferencedFiles = New-Object System.Collections.Generic.List[string]
    $allReferencedServices = New-Object System.Collections.Generic.List[string]

    foreach ($featureFile in $metadataJsonFiles) {
        $metadata = Get-Content -Path $featureFile.FullName -Raw | ConvertFrom-Json
        $referencedFiles = Get-MetadataFileReferences -MetadataObject $metadata
        $referencedServices = Get-MetadataServiceReferences -MetadataObject $metadata

        foreach ($filePath in $referencedFiles) {
            $allReferencedFiles.Add($filePath)
        }

        foreach ($serviceName in $referencedServices) {
            $allReferencedServices.Add($serviceName)
        }

        $missingFiles = @(
            foreach ($filePath in $referencedFiles) {
                $absoluteFilePath = Join-Path $RepositoryRoot $filePath
                if (-not (Test-Path $absoluteFilePath)) {
                    $filePath.Replace('\', '/')
                }
            }
        ) | Sort-Object -Unique

        $missingServiceContracts = @(
            foreach ($serviceName in $referencedServices) {
                $contractRelativePath = Get-ExpectedServiceContractPath -ModuleNameValue $ModuleNameValue -ServiceName $serviceName
                $contractAbsolutePath = Join-Path $RepositoryRoot $contractRelativePath
                if (-not (Test-Path $contractAbsolutePath)) {
                    $contractRelativePath.Replace('\', '/')
                }
            }
        ) | Sort-Object -Unique

        $featureReports.Add([PSCustomObject]@{
                FeatureId               = [string]$metadata.id
                Name                    = [string]$metadata.name
                MetadataFile            = $featureFile.Name
                ReferencedFiles         = @($referencedFiles | ForEach-Object { $_.Replace('\', '/') })
                ReferencedServices      = @($referencedServices)
                MissingReferencedFiles  = @($missingFiles)
                MissingServiceContracts = @($missingServiceContracts)
            })
    }

    $indexedFeatureFiles = @($index.featureFiles)
    $actualFeatureFiles = @($metadataJsonFiles.Name)

    $unindexedMetadataFiles = @(
        foreach ($featureFileName in $actualFeatureFiles) {
            if ($indexedFeatureFiles -notcontains $featureFileName) {
                $featureFileName
            }
        }
    ) | Sort-Object -Unique

    $missingMetadataFilesFromIndex = @(
        foreach ($featureFileName in $indexedFeatureFiles) {
            if ($actualFeatureFiles -notcontains $featureFileName) {
                $featureFileName
            }
        }
    ) | Sort-Object -Unique

    $referencedFileSet = @($allReferencedFiles | Sort-Object -Unique)
    $unreferencedSourceFiles = @(
        foreach ($sourceFile in $sourceFiles) {
            if ($referencedFileSet -notcontains $sourceFile) {
                $sourceFile.Replace('\', '/')
            }
        }
    ) | Sort-Object -Unique

    $serviceFiles = @(Get-ChildItem -Path (Join-Path $modulePath 'Services') -File -Filter '*.cs' -ErrorAction SilentlyContinue)
    $contractFiles = @(Get-ChildItem -Path (Join-Path $modulePath 'Contracts') -File -Filter '*.cs' -ErrorAction SilentlyContinue)

    $liveServiceNames = @(
        foreach ($serviceFile in $serviceFiles) {
            $serviceFile.BaseName
        }
        foreach ($contractFile in $contractFiles) {
            $contractFile.BaseName
        }
    ) | Sort-Object -Unique
    $referencedServiceSet = @($allReferencedServices | Sort-Object -Unique)

    $unreferencedServices = @(
        foreach ($serviceName in $liveServiceNames) {
            if ($referencedServiceSet -notcontains $serviceName) {
                $serviceName
            }
        }
    ) | Sort-Object -Unique

    $featureReportsArray = @($featureReports | ForEach-Object { $_ })
    $indexFeatureFilesArray = @($indexedFeatureFiles | ForEach-Object { $_ })
    $actualFeatureFilesArray = @($actualFeatureFiles | ForEach-Object { $_ })
    $unindexedMetadataFilesArray = @($unindexedMetadataFiles | ForEach-Object { $_ })
    $missingMetadataFilesFromIndexArray = @($missingMetadataFilesFromIndex | ForEach-Object { $_ })
    $unreferencedSourceFilesArray = @($unreferencedSourceFiles | ForEach-Object { $_ })
    $unreferencedServicesArray = @($unreferencedServices | ForEach-Object { $_ })

    return [PSCustomObject]@{
        Module                        = $ModuleNameValue
        GeneratedAt                   = (Get-Date).ToString('s')
        ModulePath                    = $modulePath
        MetadataPath                  = $metadataPath
        SourceFileCount               = $sourceFiles.Count
        FeatureReports                = $featureReportsArray
        IndexFeatureFiles             = $indexFeatureFilesArray
        ActualFeatureFiles            = $actualFeatureFilesArray
        UnindexedMetadataFiles        = $unindexedMetadataFilesArray
        MissingMetadataFilesFromIndex = $missingMetadataFilesFromIndexArray
        UnreferencedSourceFiles       = $unreferencedSourceFilesArray
        UnreferencedServices          = $unreferencedServicesArray
    }
}

function Convert-ReportToMarkdown {
    param(
        [Parameter(Mandatory)]
        [object]$Report
    )

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# CopilotForms Module Metadata Report")
    $lines.Add('')
    $lines.Add("- Generated: $($Report.GeneratedAt)")
    $lines.Add("- Module: $($Report.Module)")
    $lines.Add("- Source file count: $($Report.SourceFileCount)")
    $lines.Add("- Metadata folder: $($Report.MetadataPath)")
    $lines.Add('')
    $lines.Add('## Summary')
    $lines.Add('')
    $lines.Add("- Unindexed metadata files: $($Report.UnindexedMetadataFiles.Count)")
    $lines.Add("- Missing metadata files from index: $($Report.MissingMetadataFilesFromIndex.Count)")
    $lines.Add("- Unreferenced source files: $($Report.UnreferencedSourceFiles.Count)")
    $lines.Add("- Unreferenced services/contracts: $($Report.UnreferencedServices.Count)")
    $lines.Add('')

    $lines.Add('## Index Findings')
    $lines.Add('')
    if ($Report.UnindexedMetadataFiles.Count -eq 0 -and $Report.MissingMetadataFilesFromIndex.Count -eq 0) {
        $lines.Add('- Index file list matches the metadata files on disk.')
    }
    else {
        if ($Report.UnindexedMetadataFiles.Count -gt 0) {
            $lines.Add('- Metadata files missing from index.json:')
            foreach ($item in $Report.UnindexedMetadataFiles) {
                $lines.Add("  - $item")
            }
        }
        if ($Report.MissingMetadataFilesFromIndex.Count -gt 0) {
            $lines.Add('- index.json entries whose files are missing on disk:')
            foreach ($item in $Report.MissingMetadataFilesFromIndex) {
                $lines.Add("  - $item")
            }
        }
    }
    $lines.Add('')

    $lines.Add('## Feature Findings')
    $lines.Add('')
    foreach ($feature in $Report.FeatureReports) {
        $lines.Add("### $($feature.Name) ($($feature.MetadataFile))")
        $lines.Add('')
        if ($feature.MissingReferencedFiles.Count -eq 0 -and $feature.MissingServiceContracts.Count -eq 0) {
            $lines.Add('- No broken references detected.')
        }
        else {
            if ($feature.MissingReferencedFiles.Count -gt 0) {
                $lines.Add('- Missing referenced files:')
                foreach ($item in $feature.MissingReferencedFiles) {
                    $lines.Add("  - $item")
                }
            }
            if ($feature.MissingServiceContracts.Count -gt 0) {
                $lines.Add('- Missing service or contract files referenced by name:')
                foreach ($item in $feature.MissingServiceContracts) {
                    $lines.Add("  - $item")
                }
            }
        }
        $lines.Add('')
    }

    $lines.Add('## Live Source Files Not Referenced By Metadata')
    $lines.Add('')
    if ($Report.UnreferencedSourceFiles.Count -eq 0) {
        $lines.Add('- Every scanned source file is referenced somewhere in the current metadata.')
    }
    else {
        foreach ($item in $Report.UnreferencedSourceFiles) {
            $lines.Add("- $item")
        }
    }
    $lines.Add('')

    $lines.Add('## Live Services Or Contracts Not Referenced By Metadata')
    $lines.Add('')
    if ($Report.UnreferencedServices.Count -eq 0) {
        $lines.Add('- Every scanned service or contract name appears in metadata.')
    }
    else {
        foreach ($item in $Report.UnreferencedServices) {
            $lines.Add("- $item")
        }
    }
    $lines.Add('')

    $lines.Add('## Update Guidance')
    $lines.Add('')
    $lines.Add('- Use this report to decide which feature JSON files or index entries are stale.')
    $lines.Add('- Do not treat this report as an auto-fix. Review the actual module code before changing summaries, hints, or related file lists.')
    $lines.Add('- Broken references are usually safe to fix first, then review unreferenced source files to decide whether they belong in existing features or require new metadata entries.')

    return ($lines -join [Environment]::NewLine)
}

$report = New-ModuleMetadataReport -ModuleNameValue $ModuleName -RepositoryRoot $RepoRoot

if (-not (Test-Path $OutputDirectory)) {
    New-Item -Path $OutputDirectory -ItemType Directory -Force | Out-Null
}

$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$baseName = "$ModuleName-copilotforms-metadata-report-$timestamp"
$markdownPath = Join-Path $OutputDirectory ($baseName + '.md')
$jsonPath = Join-Path $OutputDirectory ($baseName + '.json')

$markdownContent = Convert-ReportToMarkdown -Report $report
$jsonContent = $report | ConvertTo-Json -Depth 10

Set-Content -Path $markdownPath -Value $markdownContent -Encoding UTF8
Set-Content -Path $jsonPath -Value $jsonContent -Encoding UTF8

[PSCustomObject]@{
    Module                        = $ModuleName
    MarkdownReport                = $markdownPath
    JsonReport                    = $jsonPath
    UnindexedMetadataFiles        = $report.UnindexedMetadataFiles.Count
    MissingMetadataFilesFromIndex = $report.MissingMetadataFilesFromIndex.Count
    UnreferencedSourceFiles       = $report.UnreferencedSourceFiles.Count
    UnreferencedServices          = $report.UnreferencedServices.Count
}
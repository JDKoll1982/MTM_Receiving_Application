# ============================================================================
# Script: New-CopilotFormsModuleMetadataReport.ps1
# Purpose: Generate a report comparing live module files to CopilotForms split
#          metadata without modifying any metadata files.
# ============================================================================

[CmdletBinding(DefaultParameterSetName = 'SingleModule')]
param(
    [Parameter(Mandatory, ParameterSetName = 'SingleModule')]
    [ValidateNotNullOrEmpty()]
    [string]$ModuleName,

    [Parameter(Mandatory, ParameterSetName = 'AllModules')]
    [switch]$AllModules,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "outputs")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:TrackedModulePaths = @(
    'Infrastructure',
    'Module_Core',
    'Module_Dunnage',
    'Module_Receiving',
    'Module_Reporting',
    'Module_Settings.Core',
    'Module_Settings.DeveloperTools',
    'Module_Settings.Dunnage',
    'Module_Settings.Receiving',
    'Module_Settings.Volvo',
    'Module_Settings.Reporting',
    'Module_Shared',
    'Module_ShipRec_Tools',
    'Module_Volvo',
    'Database',
    'MTM_Receiving_Application.Tests'
)

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

    $includeExtensions = @('.cs', '.xaml', '.json', '.sql')
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

function Get-ExpectedServiceReferencePaths {
    param(
        [Parameter(Mandatory)]
        [string]$ModuleNameValue,

        [Parameter(Mandatory)]
        [string]$ServiceName
    )

    return @(
        Join-Path $ModuleNameValue (Join-Path 'Contracts' ($ServiceName + '.cs'))
        Join-Path $ModuleNameValue (Join-Path 'Contracts\Services' ($ServiceName + '.cs'))
        Join-Path $ModuleNameValue (Join-Path 'Interfaces' ($ServiceName + '.cs'))
        Join-Path $ModuleNameValue (Join-Path 'Services' ($ServiceName + '.cs'))
    ) | Sort-Object -Unique
}

function Get-ModuleAreaGroups {
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]]$RelativePaths,

        [Parameter(Mandatory)]
        [string]$ModuleNameValue
    )

    $groups = [ordered]@{}

    foreach ($relativePath in $RelativePaths) {
        $normalizedPath = $relativePath.Replace('\', '/')
        $segments = $normalizedPath.Split('/')
        $areaName = 'Other'

        if ($segments.Count -ge 2 -and $segments[0] -eq $ModuleNameValue) {
            $areaName = $segments[1]
        }

        if (-not $groups.Contains($areaName)) {
            $groups[$areaName] = New-Object System.Collections.Generic.List[string]
        }

        $groups[$areaName].Add($normalizedPath)
    }

    return @(
        foreach ($areaName in ($groups.Keys | Sort-Object)) {
            [PSCustomObject]@{
                Area  = $areaName
                Count = $groups[$areaName].Count
                Items = @($groups[$areaName] | Sort-Object -Unique)
            }
        }
    )
}

function Get-ServiceNameGroups {
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]]$ServiceNames
    )

    $groups = [ordered]@{}

    foreach ($serviceName in $ServiceNames) {
        $groupName = if ($serviceName.StartsWith('IService_')) {
            'Contracts'
        }
        elseif ($serviceName.StartsWith('Service_')) {
            'Services'
        }
        else {
            'Other'
        }

        if (-not $groups.Contains($groupName)) {
            $groups[$groupName] = New-Object System.Collections.Generic.List[string]
        }

        $groups[$groupName].Add($serviceName)
    }

    return @(
        foreach ($groupName in ($groups.Keys | Sort-Object)) {
            [PSCustomObject]@{
                Group = $groupName
                Count = $groups[$groupName].Count
                Items = @($groups[$groupName] | Sort-Object -Unique)
            }
        }
    )
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

    if (-not (Test-Path $modulePath)) {
        throw "Module path not found: $modulePath"
    }

    $sourceFiles = Get-ModuleSourceFiles -ModulePath $modulePath -RepoRootPath $RepositoryRoot

    if (-not (Test-Path $metadataPath)) {
        $sourceFilesArray = @($sourceFiles | ForEach-Object { $_.Replace('\', '/') })
        return [PSCustomObject]@{
            Module                        = $ModuleNameValue
            GeneratedAt                   = (Get-Date).ToString('s')
            ModulePath                    = $modulePath
            MetadataPath                  = $metadataPath
            MetadataStatus                = 'MissingMetadataFolder'
            SourceFileCount               = $sourceFiles.Count
            FeatureReports                = @()
            IndexFeatureFiles             = @()
            ActualFeatureFiles            = @()
            UnindexedMetadataFiles        = @()
            MissingMetadataFilesFromIndex = @()
            UnreferencedSourceFiles       = $sourceFilesArray
            UnreferencedSourceFileGroups  = @(Get-ModuleAreaGroups -RelativePaths @($sourceFiles) -ModuleNameValue $ModuleNameValue)
            UnreferencedServices          = @()
            UnreferencedServiceGroups     = @()
        }
    }

    if (-not (Test-Path $indexPath)) {
        $sourceFilesArray = @($sourceFiles | ForEach-Object { $_.Replace('\', '/') })
        return [PSCustomObject]@{
            Module                        = $ModuleNameValue
            GeneratedAt                   = (Get-Date).ToString('s')
            ModulePath                    = $modulePath
            MetadataPath                  = $metadataPath
            MetadataStatus                = 'MissingMetadataIndex'
            SourceFileCount               = $sourceFiles.Count
            FeatureReports                = @()
            IndexFeatureFiles             = @()
            ActualFeatureFiles            = @()
            UnindexedMetadataFiles        = @()
            MissingMetadataFilesFromIndex = @()
            UnreferencedSourceFiles       = $sourceFilesArray
            UnreferencedSourceFileGroups  = @(Get-ModuleAreaGroups -RelativePaths @($sourceFiles) -ModuleNameValue $ModuleNameValue)
            UnreferencedServices          = @()
            UnreferencedServiceGroups     = @()
        }
    }

    $index = Get-Content -Path $indexPath -Raw | ConvertFrom-Json
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
                $matchingServiceFiles = @(
                    Get-ChildItem -Path (Join-Path $modulePath 'Contracts') -Recurse -File -Filter ($serviceName + '.cs') -ErrorAction SilentlyContinue
                    Get-ChildItem -Path (Join-Path $modulePath 'Interfaces') -Recurse -File -Filter ($serviceName + '.cs') -ErrorAction SilentlyContinue
                    Get-ChildItem -Path (Join-Path $modulePath 'Services') -Recurse -File -Filter ($serviceName + '.cs') -ErrorAction SilentlyContinue
                ) | Sort-Object FullName -Unique

                if (-not $matchingServiceFiles) {
                    $candidateRelativePaths = @(Get-ExpectedServiceReferencePaths -ModuleNameValue $ModuleNameValue -ServiceName $serviceName)
                    $candidateRelativePaths[0].Replace('\', '/')
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

    $serviceFiles = @(Get-ChildItem -Path (Join-Path $modulePath 'Services') -Recurse -File -Filter '*.cs' -ErrorAction SilentlyContinue)
    $contractFiles = @(Get-ChildItem -Path (Join-Path $modulePath 'Contracts') -Recurse -File -Filter '*.cs' -ErrorAction SilentlyContinue)
    $interfaceFiles = @(Get-ChildItem -Path (Join-Path $modulePath 'Interfaces') -Recurse -File -Filter '*.cs' -ErrorAction SilentlyContinue)

    $liveServiceNames = @(
        foreach ($serviceFile in $serviceFiles) {
            if ($serviceFile.BaseName -like 'Service_*') {
                $serviceFile.BaseName
            }
        }
        foreach ($contractFile in $contractFiles) {
            if ($contractFile.BaseName -like 'IService_*') {
                $contractFile.BaseName
            }
        }
        foreach ($interfaceFile in $interfaceFiles) {
            if ($interfaceFile.BaseName -like 'IService_*') {
                $interfaceFile.BaseName
            }
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
    $unreferencedSourceFileGroupsArray = @(Get-ModuleAreaGroups -RelativePaths $unreferencedSourceFilesArray -ModuleNameValue $ModuleNameValue)
    $unreferencedServiceGroupsArray = @(Get-ServiceNameGroups -ServiceNames $unreferencedServicesArray)

    return [PSCustomObject]@{
        Module                        = $ModuleNameValue
        GeneratedAt                   = (Get-Date).ToString('s')
        ModulePath                    = $modulePath
        MetadataPath                  = $metadataPath
        MetadataStatus                = 'MetadataPresent'
        SourceFileCount               = $sourceFiles.Count
        FeatureReports                = $featureReportsArray
        IndexFeatureFiles             = $indexFeatureFilesArray
        ActualFeatureFiles            = $actualFeatureFilesArray
        UnindexedMetadataFiles        = $unindexedMetadataFilesArray
        MissingMetadataFilesFromIndex = $missingMetadataFilesFromIndexArray
        UnreferencedSourceFiles       = $unreferencedSourceFilesArray
        UnreferencedSourceFileGroups  = $unreferencedSourceFileGroupsArray
        UnreferencedServices          = $unreferencedServicesArray
        UnreferencedServiceGroups     = $unreferencedServiceGroupsArray
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
    $lines.Add("- Metadata status: $($Report.MetadataStatus)")
    $lines.Add("- Source file count: $($Report.SourceFileCount)")
    $lines.Add("- Metadata folder: $($Report.MetadataPath)")
    $lines.Add('')

    if ($Report.MetadataStatus -ne 'MetadataPresent') {
        $lines.Add('## Metadata Availability')
        $lines.Add('')
        if ($Report.MetadataStatus -eq 'MissingMetadataFolder') {
            $lines.Add('- No split CopilotForms metadata folder exists for this path yet.')
            $lines.Add('- Review the source-file groups below to decide whether this path should get a new metadata folder and index.json.')
        }
        elseif ($Report.MetadataStatus -eq 'MissingMetadataIndex') {
            $lines.Add('- The split metadata folder exists, but index.json is missing.')
            $lines.Add('- Restore or create index.json before treating this path as an active split-metadata module.')
        }
        $lines.Add('')
    }

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

    $lines.Add('## Unreferenced Source Files By Area')
    $lines.Add('')
    if ($Report.UnreferencedSourceFileGroups.Count -eq 0) {
        $lines.Add('- No unreferenced source-file groups to report.')
    }
    else {
        foreach ($group in $Report.UnreferencedSourceFileGroups) {
            $lines.Add("### $($group.Area) ($($group.Count))")
            $lines.Add('')
            foreach ($item in $group.Items) {
                $lines.Add("- $item")
            }
            $lines.Add('')
        }
    }

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

    $lines.Add('## Unreferenced Services By Type')
    $lines.Add('')
    if ($Report.UnreferencedServiceGroups.Count -eq 0) {
        $lines.Add('- No unreferenced service groups to report.')
    }
    else {
        foreach ($group in $Report.UnreferencedServiceGroups) {
            $lines.Add("### $($group.Group) ($($group.Count))")
            $lines.Add('')
            foreach ($item in $group.Items) {
                $lines.Add("- $item")
            }
            $lines.Add('')
        }
    }

    $lines.Add('## Update Guidance')
    $lines.Add('')
    $lines.Add('- Use this report to decide which feature JSON files or index entries are stale.')
    $lines.Add('- Do not treat this report as an auto-fix. Review the actual module code before changing summaries, hints, or related file lists.')
    $lines.Add('- Broken references are usually safe to fix first, then review unreferenced source files to decide whether they belong in existing features or require new metadata entries.')

    return ($lines -join [Environment]::NewLine)
}

function Get-AllTrackedModules {
    param(
        [Parameter(Mandatory)]
        [string]$RepositoryRoot
    )

    return @(
        foreach ($path in $script:TrackedModulePaths) {
            if (Test-Path (Join-Path $RepositoryRoot $path)) {
                $path
            }
        }
    )
}

function Write-ModuleReportFiles {
    param(
        [Parameter(Mandatory)]
        [object]$Report,

        [Parameter(Mandatory)]
        [string]$TargetOutputDirectory
    )

    if (-not (Test-Path $TargetOutputDirectory)) {
        New-Item -Path $TargetOutputDirectory -ItemType Directory -Force | Out-Null
    }

    $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $baseName = "$($Report.Module)-copilotforms-metadata-report-$timestamp"
    $markdownPath = Join-Path $TargetOutputDirectory ($baseName + '.md')
    $jsonPath = Join-Path $TargetOutputDirectory ($baseName + '.json')

    $markdownContent = Convert-ReportToMarkdown -Report $Report
    $jsonContent = $Report | ConvertTo-Json -Depth 10

    Set-Content -Path $markdownPath -Value $markdownContent -Encoding UTF8
    Set-Content -Path $jsonPath -Value $jsonContent -Encoding UTF8

    return [PSCustomObject]@{
        Module                        = $Report.Module
        MetadataStatus                = $Report.MetadataStatus
        MarkdownReport                = $markdownPath
        JsonReport                    = $jsonPath
        UnindexedMetadataFiles        = $Report.UnindexedMetadataFiles.Count
        MissingMetadataFilesFromIndex = $Report.MissingMetadataFilesFromIndex.Count
        UnreferencedSourceFiles       = $Report.UnreferencedSourceFiles.Count
        UnreferencedServices          = $Report.UnreferencedServices.Count
    }
}

function Write-AllModulesSummaryReport {
    param(
        [Parameter(Mandatory)]
        [object[]]$ModuleResults,

        [Parameter(Mandatory)]
        [string]$TargetOutputDirectory
    )

    if (-not (Test-Path $TargetOutputDirectory)) {
        New-Item -Path $TargetOutputDirectory -ItemType Directory -Force | Out-Null
    }

    $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $baseName = "AllModules-copilotforms-metadata-report-$timestamp"
    $markdownPath = Join-Path $TargetOutputDirectory ($baseName + '.md')
    $jsonPath = Join-Path $TargetOutputDirectory ($baseName + '.json')

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('# CopilotForms All Modules Metadata Report')
    $lines.Add('')
    $lines.Add("- Generated: $((Get-Date).ToString('s'))")
    $lines.Add("- Module count: $($ModuleResults.Count)")
    $lines.Add('')
    $lines.Add('## Module Summary')
    $lines.Add('')

    foreach ($result in ($ModuleResults | Sort-Object Module)) {
        $lines.Add("### $($result.Module)")
        $lines.Add('')
        $lines.Add("- Metadata status: $($result.MetadataStatus)")
        $lines.Add("- Markdown report: $($result.MarkdownReport)")
        $lines.Add("- JSON report: $($result.JsonReport)")
        $lines.Add("- Unindexed metadata files: $($result.UnindexedMetadataFiles)")
        $lines.Add("- Missing metadata files from index: $($result.MissingMetadataFilesFromIndex)")
        $lines.Add("- Unreferenced source files: $($result.UnreferencedSourceFiles)")
        $lines.Add("- Unreferenced services/contracts: $($result.UnreferencedServices)")
        $lines.Add('')
    }

    $markdownContent = $lines -join [Environment]::NewLine
    $jsonContent = $ModuleResults | ConvertTo-Json -Depth 10

    Set-Content -Path $markdownPath -Value $markdownContent -Encoding UTF8
    Set-Content -Path $jsonPath -Value $jsonContent -Encoding UTF8

    return [PSCustomObject]@{
        MarkdownReport = $markdownPath
        JsonReport     = $jsonPath
        ModuleCount    = $ModuleResults.Count
    }
}

if ($PSCmdlet.ParameterSetName -eq 'AllModules') {
    $trackedModules = Get-AllTrackedModules -RepositoryRoot $RepoRoot
    $moduleResults = New-Object System.Collections.Generic.List[object]

    foreach ($currentModule in $trackedModules) {
        $report = New-ModuleMetadataReport -ModuleNameValue $currentModule -RepositoryRoot $RepoRoot
        $moduleResult = Write-ModuleReportFiles -Report $report -TargetOutputDirectory $OutputDirectory
        $moduleResults.Add($moduleResult)
    }

    $moduleResultsArray = @($moduleResults | ForEach-Object { $_ })
    $summary = Write-AllModulesSummaryReport -ModuleResults $moduleResultsArray -TargetOutputDirectory $OutputDirectory

    [PSCustomObject]@{
        Mode           = 'AllModules'
        ModuleCount    = $moduleResultsArray.Count
        MarkdownReport = $summary.MarkdownReport
        JsonReport     = $summary.JsonReport
        Modules        = $trackedModules
        ModuleReports  = $moduleResultsArray
    }
}
else {
    $report = New-ModuleMetadataReport -ModuleNameValue $ModuleName -RepositoryRoot $RepoRoot
    Write-ModuleReportFiles -Report $report -TargetOutputDirectory $OutputDirectory
}
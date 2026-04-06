[CmdletBinding()]
param(
    [Parameter()]
    [string]$CsvPath = (Join-Path $PSScriptRoot '..\..\..\docs\GoogleSheetsVersion\CSV Imports\Receiving Data\Receiving Data - History 2025.csv'),

    [Parameter()]
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\..\..\Module_Settings.Core\Defaults\inforvisual.mock-data.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-FullPath {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return [System.IO.Path]::GetFullPath($Path)
}

function ConvertTo-CanonicalPONumber {
    param(
        [Parameter(Mandatory)]
        [string]$PONumber
    )

    $normalized = $PONumber.Trim().ToUpperInvariant()
    if ($normalized.StartsWith('PO-')) {
        return $normalized
    }

    if ($normalized -match '^\d+$') {
        return 'PO-' + $normalized.PadLeft(6, '0')
    }

    if ($normalized -match '^(\d+)([Bb])$') {
        return 'PO-' + $matches[1].PadLeft(6, '0') + $matches[2].ToUpperInvariant()
    }

    return $normalized
}

function Test-IsSupportedPONumber {
    param(
        [Parameter(Mandatory)]
        [string]$PONumber
    )

    $normalized = $PONumber.Trim().ToUpperInvariant()
    return (
        $normalized -match '^PO-\d+[Bb]?$' -or
        $normalized -match '^\d+[Bb]?$'
    )
}

function Get-PartType {
    param(
        [Parameter(Mandatory)]
        [string]$PartId
    )

    if ($PartId.StartsWith('MMC', [System.StringComparison]::OrdinalIgnoreCase)) {
        return 'Coil'
    }

    if ($PartId.StartsWith('MMF', [System.StringComparison]::OrdinalIgnoreCase)) {
        return 'Sheet'
    }

    return 'Material'
}

function ConvertTo-CanonicalLocation {
    param(
        [Parameter(Mandatory)]
        [string]$Location
    )

    return ([System.Text.RegularExpressions.Regex]::Replace(
            $Location.Trim().ToUpperInvariant(),
            '\s+',
            ' '
        ))
}

function Get-PrimaryLocation {
    param(
        [Parameter(Mandatory)]
        [object[]]$Rows
    )

    return ($Rows |
        Group-Object -Property Location |
        Sort-Object -Property Count, Name -Descending |
        Select-Object -First 1).Name
}

function New-VendorTemplates {
    return @(
        [ordered]@{ VendorId = 'MOCK-VENDOR-001'; VendorName = 'Acme Heat Treating Co.'; VendorCity = 'Detroit'; VendorState = 'MI' },
        [ordered]@{ VendorId = 'MOCK-VENDOR-002'; VendorName = 'Precision Plating Inc.'; VendorCity = 'Grand Rapids'; VendorState = 'MI' },
        [ordered]@{ VendorId = 'MOCK-VENDOR-003'; VendorName = 'Allied Metal Services'; VendorCity = 'Toledo'; VendorState = 'OH' },
        [ordered]@{ VendorId = 'MOCK-VENDOR-004'; VendorName = 'Midwest Coating Group'; VendorCity = 'Fort Wayne'; VendorState = 'IN' }
    )
}

$resolvedCsvPath = Resolve-FullPath -Path $CsvPath
$resolvedOutputPath = Resolve-FullPath -Path $OutputPath

if (-not (Test-Path -Path $resolvedCsvPath)) {
    throw "CSV file not found: $resolvedCsvPath"
}

$vendorTemplates = New-VendorTemplates
$csvRows = Import-Csv -Path $resolvedCsvPath

$normalizedRows = foreach ($row in $csvRows) {
    if (
        [string]::IsNullOrWhiteSpace($row.'Material ID') -or
        [string]::IsNullOrWhiteSpace($row.'PO Number') -or
        [string]::IsNullOrWhiteSpace($row.'Initial Location') -or
        [string]::IsNullOrWhiteSpace($row.'Date') -or
        [string]::IsNullOrWhiteSpace($row.Quantity) -or
        -not (Test-IsSupportedPONumber -PONumber $row.'PO Number')
    ) {
        continue
    }

    [pscustomobject]@{
        PartId   = $row.'Material ID'.Trim().ToUpperInvariant()
        PONumber = ConvertTo-CanonicalPONumber -PONumber $row.'PO Number'
        Location = ConvertTo-CanonicalLocation -Location $row.'Initial Location'
        Quantity = [decimal]$row.Quantity
        Date     = [datetime]::Parse($row.'Date')
        Heat     = $row.Heat
        Employee = $row.Employee
    }
}

if (-not $normalizedRows) {
    throw 'The source CSV did not produce any usable mock-data rows.'
}

$sortedRows = $normalizedRows | Sort-Object -Property PONumber, PartId, Date, Location
$purchaseOrders = New-Object System.Collections.Generic.List[object]
$partCatalog = @{}
$outsideServiceHistory = New-Object System.Collections.Generic.List[object]

$poGroups = $sortedRows | Group-Object -Property PONumber | Sort-Object -Property Name
$poIndex = 0

foreach ($poGroup in $poGroups) {
    $vendorTemplate = $vendorTemplates[$poIndex % $vendorTemplates.Count]
    $poRows = @($poGroup.Group)
    $poDate = ($poRows | Sort-Object -Property Date | Select-Object -First 1).Date
    $poStatus = if (($poIndex % 4) -eq 0) { 'P' } else { 'O' }
    $poParts = New-Object System.Collections.Generic.List[object]
    $lineNumber = 1

    foreach ($partGroup in ($poRows | Group-Object -Property PartId | Sort-Object -Property Name)) {
        $rowsForPart = @($partGroup.Group | Sort-Object -Property Date, Location)
        $partId = $partGroup.Name
        $description = "Derived from receiving history for $partId"
        $defaultLocation = Get-PrimaryLocation -Rows $rowsForPart
        $dueDate = ($rowsForPart | Sort-Object -Property Date | Select-Object -First 1).Date
        $totalQuantity = [decimal](($rowsForPart | Measure-Object -Property Quantity -Sum).Sum)
        $maxQuantity = [decimal](($rowsForPart | Measure-Object -Property Quantity -Maximum).Maximum)

        if ($maxQuantity -lt 1) {
            $maxQuantity = $totalQuantity
        }

        $orderedQuantity = [decimal][math]::Ceiling([double]($totalQuantity * (1.15 + ((($lineNumber - 1) % 3) * 0.10))))
        if ($orderedQuantity -le $totalQuantity) {
            $orderedQuantity = [decimal]($totalQuantity + $maxQuantity)
        }

        $remainingQuantity = [int][math]::Max([double]($orderedQuantity - $totalQuantity), 1)
        $partType = Get-PartType -PartId $partId

        $partObject = [ordered]@{
            PartID            = $partId
            POLineNumber      = $lineNumber.ToString()
            PartType          = $partType
            QtyOrdered        = $orderedQuantity
            UnitOfMeasure     = 'EA'
            Description       = $description
            DefaultLocationId = $defaultLocation
            RemainingQuantity = $remainingQuantity
            DueDate           = $dueDate.ToString('yyyy-MM-dd')
        }

        $poParts.Add($partObject)

        if (-not $partCatalog.ContainsKey($partId)) {
            $partCatalog[$partId] = [ordered]@{
                PartID            = $partId
                POLineNumber      = 'N/A'
                PartType          = $partType
                QtyOrdered        = $orderedQuantity
                UnitOfMeasure     = 'EA'
                Description       = $description
                DefaultLocationId = $defaultLocation
                RemainingQuantity = $remainingQuantity
                DueDate           = $dueDate.ToString('yyyy-MM-dd')
            }
        }

        $outsideServiceHistory.Add(
            [ordered]@{
                VendorID       = $vendorTemplate.VendorId
                VendorName     = $vendorTemplate.VendorName
                VendorCity     = $vendorTemplate.VendorCity
                VendorState    = $vendorTemplate.VendorState
                DispatchID     = ('SD-{0:000000}' -f (($poIndex + 1) * 100 + $lineNumber))
                DispatchDate   = $dueDate.ToString('yyyy-MM-dd')
                PartNumber     = $partId
                QuantitySent   = $maxQuantity
                DispatchStatus = if (($lineNumber % 2) -eq 0) { 'Closed' } else { 'Open' }
            }
        )

        $lineNumber++
    }

    $purchaseOrders.Add(
        [ordered]@{
            PONumber                 = $poGroup.Name
            Vendor                   = $vendorTemplate.VendorName
            Status                   = $poStatus
            HeaderPromiseDate        = $poDate.ToString('yyyy-MM-dd')
            HeaderDesiredReceiveDate = $poDate.ToString('yyyy-MM-dd')
            FreeOnBoard              = 'RECEIVING DOCK'
            Parts                    = $poParts
        }
    )

    $poIndex++
}

$locations = @($sortedRows | Select-Object -ExpandProperty Location -Unique | Sort-Object)
$parts = @($partCatalog.Values | Sort-Object -Property PartID)
$defaultPurchaseOrderNumber = ($purchaseOrders | Select-Object -First 1).PONumber

$catalog = [ordered]@{
    DefaultPurchaseOrderNumber = $defaultPurchaseOrderNumber
    Locations                  = $locations
    PurchaseOrders             = $purchaseOrders
    Parts                      = $parts
    OutsideServiceHistory      = $outsideServiceHistory
}

$outputDirectory = Split-Path -Path $resolvedOutputPath -Parent
if (-not (Test-Path -Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}

$json = $catalog | ConvertTo-Json -Depth 8
Set-Content -Path $resolvedOutputPath -Value $json -Encoding utf8

[pscustomobject]@{
    CsvPath                    = $resolvedCsvPath
    OutputPath                 = $resolvedOutputPath
    PurchaseOrderCount         = $purchaseOrders.Count
    PartCount                  = $parts.Count
    LocationCount              = $locations.Count
    OutsideServiceHistoryCount = $outsideServiceHistory.Count
    DefaultPurchaseOrderNumber = $defaultPurchaseOrderNumber
}
[CmdletBinding()]
param(
    [Parameter()]
    [string]$Path = ".",

    [Parameter()]
    [switch]$PassThru
)

Write-Verbose "Validating MVVM architecture under path: $Path"

$excludedDirNames = @(
    ".git",
    ".vs",
    ".vscode",
    ".idea",
    "bin",
    "obj",
    "packages",
    ".nuget",
    "TestResults",
    ".playwright-mcp"
)

function Test-IsExcludedPath {
    param(
        [Parameter(Mandatory)]
        [string]$FullName
    )

    foreach ($dirName in $excludedDirNames) {
        if ($FullName -match "\\$([regex]::Escape($dirName))\\") {
            return $true
        }
    }

    return $false
}

$violations = New-Object System.Collections.Generic.List[string]

# Helper function to check if {Binding} is inside a specific control type
function Test-InsideControl {
    param(
        [string]$Content,
        [int]$Position,
        [string]$ControlPattern
    )
    
    $precedingContent = $Content.Substring(0, $Position)
    
    # Match opening tags with optional namespace prefix: <ItemsRepeater or <x:ItemsRepeater
    $opens = [regex]::Matches($precedingContent, "<\s*(?:\w+:)?$ControlPattern(?:\s|>)").Count
    # Match closing tags with optional namespace prefix: </ItemsRepeater> or </x:ItemsRepeater>
    $closes = [regex]::Matches($precedingContent, "</\s*(?:\w+:)?$ControlPattern>").Count
    
    return $opens -gt $closes
}

# List of control types where {Binding} is allowed (x:Bind doesn't support item-level binding)
$allowedCollectionControls = @(
    'DataGrid\w*',          # DataGrid, DataGridTextColumn, DataGridTemplateColumn, etc.
    'ItemsControl',          # ItemsControl
    'ItemsRepeater',         # ItemsRepeater
    'ListView',              # ListView
    'GridView'               # GridView
)

$viewModelFiles = Get-ChildItem -Path $Path -Recurse -File -Filter "*.cs" |
    Where-Object { $_.FullName -match "\\ViewModels\\" } |
    Where-Object { $_.Name -match "ViewModel" } |
    Where-Object { -not (Test-IsExcludedPath -FullName $_.FullName) }

foreach ($file in $viewModelFiles) {
    $content = Get-Content -Path $file.FullName -Raw

    # Only enforce rules on concrete ViewModel classes (avoid interfaces/helpers/tests that happen to include ViewModel in the name).
    if ($content -notmatch "\bclass\s+\w*ViewModel\w*\b") {
        continue
    }

    if ($content -match "Helper_Database_") {
        $violations.Add("$($file.FullName): ViewModel references Helper_Database_* (forbidden)")
    }

    if ($content -match "\bDao_\w+\b") {
        $violations.Add("$($file.FullName): ViewModel references Dao_* directly (forbidden)")
    }

    if ($content -notmatch "\bpartial\s+class\b") {
        $violations.Add("$($file.FullName): ViewModel is missing 'partial class'")
    }
}

$xamlFiles = Get-ChildItem -Path $Path -Recurse -File -Filter "*.xaml" |
    Where-Object { -not (Test-IsExcludedPath -FullName $_.FullName) }

foreach ($file in $xamlFiles) {
    $content = Get-Content -Path $file.FullName -Raw

    # {Binding} is generally forbidden in this repo in favor of {x:Bind}.
    # Exception: Collection control contexts where {x:Bind} cannot bind to dynamic row items:
    #   1. DataGrid (columns and templates)
    #   2. ItemsControl, ListView, GridView templates
    #   3. ItemsRepeater templates
    # Also allowed: ElementName bindings (not supported by {x:Bind})
    $bindingMatches = [regex]::Matches($content, "\{Binding\s")
    foreach ($m in $bindingMatches) {
        $i = $m.Index
        
        # Check if {Binding} is inside any allowed collection control
        $isInsideAllowedControl = $false
        foreach ($controlPattern in $allowedCollectionControls) {
            if (Test-InsideControl -Content $content -Position $i -ControlPattern $controlPattern) {
                $isInsideAllowedControl = $true
                break
            }
        }

        # Also allow ElementName and RelativeSource bindings (x:Bind doesn't support these)
        if (-not $isInsideAllowedControl) {
            $start = [Math]::Max(0, $i - 100)
            $len = [Math]::Min($i - $start + 50, $content.Length - $start)
            $localContext = $content.Substring($start, $len)
            $isElementNameBinding = $localContext -match 'ElementName\s*='
            $isRelativeSourceBinding = $localContext -match 'RelativeSource\s*='
            
            if ($isElementNameBinding -or $isRelativeSourceBinding) {
                $isInsideAllowedControl = $true
            }
        }

        if (-not $isInsideAllowedControl) {
            $violations.Add("$($file.FullName): XAML uses `\{Binding\} outside allowed collection control context (forbidden; use `\{x:Bind\})")
            break
        }
    }
}

if ($violations.Count -eq 0) {
    Write-Output "No MVVM violations found."
    return
}

Write-Warning "Found $($violations.Count) MVVM violation(s)."

foreach ($v in $violations) {
    Write-Output $v
}

if ($PassThru.IsPresent) {
    Write-Output ([PSCustomObject]@{
        ViolationCount = $violations.Count
        Violations     = $violations
    })
}

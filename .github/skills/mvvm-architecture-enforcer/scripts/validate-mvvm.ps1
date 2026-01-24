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

$viewModelFiles = Get-ChildItem -Path $Path -Recurse -File -Filter "*.cs" |
    Where-Object { $_.Name -match "ViewModel" } |
    Where-Object { -not (Test-IsExcludedPath -FullName $_.FullName) }

foreach ($file in $viewModelFiles) {
    $content = Get-Content -Path $file.FullName -Raw

    if ($content -match "Helper_Database_") {
        $violations.Add("$($file.FullName): ViewModel references Helper_Database_* (forbidden)")
    }

    if ($content -match "\bDao_\w+\b") {
        $violations.Add("$($file.FullName): ViewModel references Dao_* directly (likely violation)")
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
    # Exception: DataGrid column definitions (CommunityToolkit / WPF) take a Binding object
    # via an attribute like: <...DataGridTextColumn Binding="{Binding Foo}" />.
    # In those cases {x:Bind} is not supported.
    $bindingMatches = [regex]::Matches($content, "\{Binding\s")
    foreach ($m in $bindingMatches) {
        $i = $m.Index
        # Use a small look-behind window so unrelated DataGrid columns elsewhere in the file
        # don't accidentally "authorize" other {Binding ...} usages.
        $start = [Math]::Max(0, $i - 250)
        $len = [Math]::Min($content.Length - $start, 350)
        $context = $content.Substring($start, $len)

        # Allowed only when the {Binding ...} is part of a DataGrid*Column Binding="..." attribute.
        $isAllowed = $context -match '<\s*(?:controls:|\w+:)?DataGrid(?:Text|CheckBox|ComboBox|Hyperlink)Column\b[^>]*\bBinding\s*=\s*"[^\"]*\{Binding\s'

        if (-not $isAllowed) {
            $violations.Add("$($file.FullName): XAML uses `\{Binding\} outside allowed DataGrid column Binding='`\{Binding ...\}' (forbidden; use `\{x:Bind\})")
            break
        }
    }
}

if ($violations.Count -eq 0) {
    Write-Output "No MVVM violations found."
    return
}

Write-Warning "Found $($violations.Count) potential MVVM violation(s)."

foreach ($v in $violations) {
    Write-Output $v
}

if ($PassThru.IsPresent) {
    Write-Output ([PSCustomObject]@{
        ViolationCount = $violations.Count
        Violations     = $violations
    })
}

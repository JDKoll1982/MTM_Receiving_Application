<#
.SYNOPSIS
    Generates workflow diagram markdown files from a Repomix output file.

.DESCRIPTION
    Creates one workflow file per workflow name under .repomix/diagrams/{Type}/Workflow_{Name}.md
    Uses Mermaid-based templates modeled after Module_Volvo/WorkflowDiagrams.

.PARAMETER RepomixOutputPath
    Path to the Repomix output file (markdown).
    Defaults to .repomix/outputs/code-only/repomix-output-code-only.md when omitted.

.PARAMETER Type
    Output type folder name under .repomix/diagrams (e.g., Volvo, Receiving, Core).
    If omitted, you can select modules via the interactive menu.

.PARAMETER WorkflowNames
    Explicit workflow names to generate. If omitted, names can be read from WorkflowConfigPath
    or auto-discovered from the Repomix output file.

.PARAMETER WorkflowConfigPath
    JSON file containing an array of { name, template } items.

.PARAMETER Template
    Default template to use when WorkflowNames are supplied without per-item template.
    Valid values: workflow, dialog

.PARAMETER TemplateRoot
    Folder containing templates (default: .repomix/diagrams/templates).

.PARAMETER AutoDiscover
    When set, scans the Repomix output file for candidate workflow names.

.PARAMETER ModuleNames
    Optional list of module folder names (e.g., Module_Volvo) or short names (e.g., Volvo)
    to generate workflow files for.

.PARAMETER Interactive
    When set, shows an interactive menu of modules to generate workflow files for.
#>

[CmdletBinding()]
param(
    [string]$RepomixOutputPath,

    [string]$Type,

    [string[]]$WorkflowNames,

    [string]$WorkflowConfigPath,

    [ValidateSet("workflow", "dialog", "view", "viewmodel", "handler", "command", "query", "validator", "dao", "model", "enum", "test", "xaml")]
    [string]$Template = "workflow",

    [string]$TemplateRoot = ".repomix/diagrams/templates",

    [switch]$AutoDiscover,

    [string[]]$ModuleNames,

    [switch]$Interactive
)

Set-StrictMode -Version Latest

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$defaultRepomixOutputPath = Join-Path $projectRoot ".repomix/outputs/code-only/repomix-output-code-only.md"
$repomixFilePattern = '(?ms)^## File: (.+?)\r?\n```[^\r\n]*\r?\n(.*?)\r?\n```'
$methodPattern = '(?m)\b(?:public|private|protected|internal)\s+(?:async\s+)?(?:[A-Za-z0-9_<>\[\]\?]+)\s+([A-Za-z0-9_]+)\s*\('
$testMethodPattern = '(?ms)\[(?:Fact|Theory)\]\s*\r?\n\s*(?:public|internal)\s+(?:async\s+)?(?:[A-Za-z0-9_<>\[\]\?]+)\s+([A-Za-z0-9_]+)\s*\('
$relayCommandPattern = '(?ms)\[RelayCommand\]\s*\r?\n\s*(?:private|public|internal)\s+(?:async\s+)?(?:[A-Za-z0-9_<>\[\]\?]+)\s+([A-Za-z0-9_]+)\s*\('
$ruleForPattern = '(?m)RuleFor\s*\(\s*\w+\s*=>\s*\w+\.([A-Za-z0-9_]+)\s*\)'
$xamlEventPattern = '(?m)\s(?:Click|Tapped|Loaded|SelectionChanged|TextChanged)="([A-Za-z0-9_]+)"'
$xamlCommandPattern = '(?m)Command="\{x:Bind\s+([A-Za-z0-9_]+)\b'
$ifConditionPattern = '(?m)^\s*(?:else\s+)?if\s*\(.*$'
$enumMemberPattern = '(?ms)enum\s+[A-Za-z0-9_]+\s*\{(.*?)\}'
$propertyPattern = '(?m)\bpublic\s+[A-Za-z0-9_<>\[\]\?]+\s+([A-Za-z0-9_]+)\s*\{'
$genericStepExclusions = @(
    "ViewModel",
    "OnPropertyChanged",
    "Equals",
    "GetHashCode",
    "ToString"
)

$stepKeywordMap = @{
    viewmodel = @(
        "Command", "Async", "Load", "Save", "Add", "Remove", "Update", "Initialize", "Submit",
        "Search", "Filter", "Validate", "Preview", "Generate", "Complete", "Navigate", "Open",
        "Close", "On", "Create", "Edit", "Refresh", "Sync", "Retry", "Cancel", "Reset", "Start",
        "Stop", "Pause", "Resume", "Confirm", "Select", "Toggle", "Enable", "Disable", "Apply"
    )
    handler   = @(
        "Handle", "Execute", "Run", "Get", "Save", "Update", "Add", "Remove", "Delete",
        "Generate", "Complete", "Create", "Edit", "Publish", "Dispatch", "Process", "Resolve",
        "Validate", "Sync", "Refresh", "Load", "Fetch"
    )
    command   = @(
        "Handle", "Execute", "Run", "Save", "Update", "Add", "Remove", "Delete",
        "Generate", "Complete", "Create", "Edit", "Publish", "Dispatch", "Process", "Confirm",
        "Approve", "Reject", "Submit", "Cancel", "Retry"
    )
    query     = @(
        "Handle", "Execute", "Get", "Load", "Fetch", "Search", "Filter", "List",
        "Find", "Lookup", "Read", "Retrieve", "Select", "Count", "Exists", "Query"
    )
    dao       = @(
        "Get", "Insert", "Update", "Delete", "Save", "Load", "Map", "Fetch",
        "Select", "Upsert", "Find", "Read", "Retrieve", "Query", "List", "BulkInsert"
    )
    test      = @(
        "Should", "When", "Given", "Then", "Arrange", "Act", "Assert", "Verify",
        "Expect", "Ensure", "Scenario", "Spec", "Test", "Validate"
    )
    xaml      = @(
        "Click", "Tapped", "Loaded", "SelectionChanged", "TextChanged",
        "PointerPressed", "PointerReleased", "PointerMoved", "KeyDown", "KeyUp",
        "GotFocus", "LostFocus", "Checked", "Unchecked", "ValueChanged", "DragOver",
        "Drop", "SizeChanged"
    )
    dialog    = @(
        "Open", "Close", "Save", "Cancel", "Add", "Remove", "Delete", "Edit",
        "Apply", "Confirm", "Select", "Validate", "Submit", "Ok", "Load", "Copy",
        "View", "Set", "Related", "Dismiss", "Help"
    )
}

function Resolve-TemplatePath {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("workflow", "dialog")]
        [string]$TemplateName,
        [Parameter(Mandatory = $true)]
        [string]$Root
    )

    switch ($TemplateName) {
        "workflow" { return Join-Path $Root "workflow.template.md" }
        "dialog" { return Join-Path $Root "workflow-dialog.template.md" }
        "view" { return Join-Path $Root "workflow-view.template.md" }
        "viewmodel" { return Join-Path $Root "workflow-viewmodel.template.md" }
        "handler" { return Join-Path $Root "workflow-handler.template.md" }
        "command" { return Join-Path $Root "workflow-command.template.md" }
        "query" { return Join-Path $Root "workflow-query.template.md" }
        "validator" { return Join-Path $Root "workflow-validator.template.md" }
        "dao" { return Join-Path $Root "workflow-dao.template.md" }
        "model" { return Join-Path $Root "workflow-model.template.md" }
        "enum" { return Join-Path $Root "workflow-enum.template.md" }
        "test" { return Join-Path $Root "workflow-test.template.md" }
        "xaml" { return Join-Path $Root "workflow-xaml.template.md" }
        default { return Join-Path $Root "workflow.template.md" }
    }
}

function Normalize-SpacedLetters {
    param([Parameter(Mandatory = $true)][string]$Value)

    $trimmed = $Value.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmed)) {
        return $Value
    }

    $tokens = @($trimmed -split '\s+')
    $shortTokens = @($tokens | Where-Object { $_.Length -le 2 })

    if ($tokens.Length -ge 4 -and $shortTokens.Length -ge [math]::Ceiling($tokens.Length * 0.6)) {
        return ($tokens -join "")
    }

    $collapsedSingles = $trimmed -replace '(?<=\b[A-Za-z0-9])\s+(?=[A-Za-z0-9]\b)', ''
    return $collapsedSingles
}

function Normalize-StepNames {
    param([string[]]$Steps)

    $normalized = @()
    foreach ($step in @($Steps)) {
        if (-not $step) {
            continue
        }

        $clean = Normalize-SpacedLetters -Value $step
        $clean = $clean.Trim()
        $clean = $clean.TrimEnd(".")

        if ([string]::IsNullOrWhiteSpace($clean)) {
            continue
        }

        if ($genericStepExclusions -contains $clean) {
            continue
        }

        if ($clean -match '^(Helper_|Model_)') {
            continue
        }

        $normalized += $clean
    }

    return ($normalized | Select-Object -Unique)
}

function Normalize-CompareToken {
    param([Parameter(Mandatory = $true)][string]$Value)

    $clean = Normalize-SpacedLetters -Value $Value
    $clean = $clean -replace "_", ""
    return $clean.ToLowerInvariant()
}

function Filter-StepsByKeywords {
    param(
        [string[]]$Steps,
        [string[]]$IncludeKeywords
    )

    if (-not $IncludeKeywords -or $IncludeKeywords.Length -eq 0) {
        return @($Steps)
    }

    $pattern = ($IncludeKeywords | ForEach-Object { [regex]::Escape($_) }) -join "|"
    return @($Steps | Where-Object { $_ -match $pattern })
}

function Normalize-WorkflowName {
    param([Parameter(Mandatory = $true)][string]$Name)

    $normalized = ($Name -replace "[^A-Za-z0-9]+", "_").Trim("_")
    if ([string]::IsNullOrWhiteSpace($normalized)) {
        return "Workflow"
    }

    return $normalized
}

function Format-StepLabel {
    param([Parameter(Mandatory = $true)][string]$Value)

    if ($Value -like "*_*" ) {
        return $Value.Trim()
    }

    return (Normalize-SpacedLetters -Value $Value)
}

function Convert-MermaidLabel {
    param([Parameter(Mandatory = $true)][string]$Value)

    $escaped = $Value.Replace('"', '\"')
    $escaped = $escaped.Replace('<', '&lt;').Replace('>', '&gt;')
    return '"' + $escaped + '"'
}

function Get-WorkflowType {
    param(
        [Parameter(Mandatory = $true)][string]$WorkflowName,
        [string]$SourcePath
    )

    if ($SourcePath -and $SourcePath.EndsWith(".xaml", [System.StringComparison]::OrdinalIgnoreCase)) {
        return "xaml"
    }

    if ($WorkflowName -match '(_xaml$|\.xaml$)') {
        return "xaml"
    }

    switch -Regex ($WorkflowName) {
        'Dialog' { return "dialog" }
        'ViewModel$' { return "viewmodel" }
        'View$' { return "view" }
        'Validator' { return "validator" }
        'Handler' { return "handler" }
        'Command' { return "command" }
        'Query' { return "query" }
        '^Dao_' { return "dao" }
        '^Model_' { return "model" }
        '^Enum_' { return "enum" }
        'Tests?$' { return "test" }
        default { return "workflow" }
    }
}

function Get-DecisionConditions {
    param(
        [AllowEmptyString()][string]$Content,
        [string]$SourcePath
    )

    $candidateContent = $Content
    if ($SourcePath -and (Test-Path $SourcePath)) {
        $candidateContent = Get-Content -LiteralPath $SourcePath -Raw
    }

    if ([string]::IsNullOrWhiteSpace($candidateContent)) {
        return @()
    }

    $matches = [regex]::Matches($candidateContent, $ifConditionPattern)
    $conditions = @()
    foreach ($match in $matches) {
        $line = $match.Value.Trim()
        if ($line.EndsWith("{")) {
            $line = $line.TrimEnd("{").TrimEnd()
        }
        $conditions += $line
    }

    return ($conditions | Select-Object -First 3)
}

function Get-RepomixFileIndex {
    param([Parameter(Mandatory = $true)][string]$RepomixPath)

    $content = Get-Content -LiteralPath $RepomixPath -Raw
    $entries = @()

    $fileMatches = [regex]::Matches(
        $content,
        $repomixFilePattern
    )

    foreach ($match in $fileMatches) {
        $entries += [pscustomobject]@{
            Path    = $match.Groups[1].Value.Trim()
            Content = $match.Groups[2].Value
        }
    }

    return $entries
}

function Find-WorkflowSection {
    param(
        [Parameter(Mandatory = $true)][object[]]$Index,
        [Parameter(Mandatory = $true)][string]$WorkflowName,
        [string]$ModuleFolder,
        [string]$WorkflowType
    )

    $candidates = $Index
    if ($ModuleFolder) {
        $prefix = ($ModuleFolder.TrimEnd([char[]]@('/', '\')) + "/")
        $candidates = $candidates | Where-Object { $_.Path.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase) }
    }

    $normalizedName = $WorkflowName -replace '\.xaml$', '' -replace '_xaml$', ''

    function Find-ByFileName {
        param([object[]]$Items, [string[]]$FileNames)
        foreach ($fileName in $FileNames) {
            $match = $Items | Where-Object { [System.IO.Path]::GetFileName($_.Path).Equals($fileName, [System.StringComparison]::OrdinalIgnoreCase) } | Select-Object -First 1
            if ($match) { return $match }
        }
        return $null
    }

    $fileNames = @()
    switch ($WorkflowType) {
        "dialog" {
            $base = if ($normalizedName -match '^ViewModel_') { $normalizedName } else { "ViewModel_$normalizedName" }
            $fileNames += "$base.cs"
            $baseView = if ($normalizedName -match '^View_') { $normalizedName } else { "View_$normalizedName" }
            $fileNames += "$baseView.xaml.cs"
            $fileNames += "$baseView.xaml"
        }
        "viewmodel" {
            $base = if ($normalizedName -match '^ViewModel_') { $normalizedName } else { "ViewModel_$normalizedName" }
            $fileNames += "$base.cs"
        }
        "view" {
            $base = if ($normalizedName -match '^View_') { $normalizedName } else { "View_$normalizedName" }
            $fileNames += "$base.xaml.cs"
            $fileNames += "$base.xaml"
        }
        "xaml" {
            $base = if ($normalizedName -match '^View_') { $normalizedName } else { "View_$normalizedName" }
            $fileNames += "$base.xaml"
            $fileNames += "$normalizedName.xaml"
        }
        "handler" {
            $base = if ($normalizedName -match 'Handler$') { $normalizedName } else { "$normalizedName`Handler" }
            $fileNames += "$base.cs"
        }
        "validator" {
            $base = if ($normalizedName -match 'Validator$') { $normalizedName } else { "$normalizedName`Validator" }
            $fileNames += "$base.cs"
        }
        "command" {
            $fileNames += "$normalizedName.cs"
        }
        "query" {
            $fileNames += "$normalizedName.cs"
        }
        "dao" {
            $base = if ($normalizedName -match '^Dao_') { $normalizedName } else { "Dao_$normalizedName" }
            $fileNames += "$base.cs"
        }
        "model" {
            $base = if ($normalizedName -match '^Model_') { $normalizedName } else { "Model_$normalizedName" }
            $fileNames += "$base.cs"
        }
        "enum" {
            $base = if ($normalizedName -match '^Enum_') { $normalizedName } else { "Enum_$normalizedName" }
            $fileNames += "$base.cs"
        }
        default {
            $fileNames += "$normalizedName.cs"
            $fileNames += "ViewModel_$normalizedName.cs"
            $fileNames += "View_$normalizedName.xaml.cs"
            $fileNames += "View_$normalizedName.xaml"
            $fileNames += "$normalizedName.xaml"
        }
    }

    $match = Find-ByFileName -Items $candidates -FileNames $fileNames
    if ($match) {
        return $match
    }

    $namePatterns = @(
        "ViewModel_$normalizedName",
        "View_$normalizedName",
        $normalizedName
    )

    foreach ($pattern in $namePatterns) {
        $match = $candidates | Where-Object { $_.Path -match [regex]::Escape($pattern) } | Select-Object -First 1
        if ($match) {
            return $match
        }
    }

    return $null
}

function Get-MethodNamesFromContent {
    param([AllowEmptyString()][string]$Content)

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return @()
    }

    $methodMatches = [regex]::Matches(
        $Content,
        $methodPattern
    )

    $names = @()
    foreach ($match in $methodMatches) {
        $names += $match.Groups[1].Value
    }

    $names = $names | Where-Object { $_ -notmatch "^get_|^set_" } | Select-Object -Unique
    return $names
}

function Get-ViewModelCommandNames {
    param([AllowEmptyString()][string]$Content)

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return @()
    }

    $matches = [regex]::Matches($Content, $relayCommandPattern)
    $names = @()
    foreach ($match in $matches) {
        $names += $match.Groups[1].Value
    }

    return ($names | Select-Object -Unique)
}

function Get-TestMethodNames {
    param([AllowEmptyString()][string]$Content)

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return @()
    }

    $matches = [regex]::Matches($Content, $testMethodPattern)
    $names = @()
    foreach ($match in $matches) {
        $names += $match.Groups[1].Value
    }

    return ($names | Select-Object -Unique)
}

function Get-ValidatorFields {
    param([AllowEmptyString()][string]$Content)

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return @()
    }

    $matches = [regex]::Matches($Content, $ruleForPattern)
    $names = @()
    foreach ($match in $matches) {
        $names += $match.Groups[1].Value
    }

    return ($names | Select-Object -Unique)
}

function Get-XamlEvents {
    param([AllowEmptyString()][string]$Content)

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return @()
    }

    $matches = [regex]::Matches($Content, $xamlEventPattern)
    $names = @()
    foreach ($match in $matches) {
        $names += $match.Groups[1].Value
    }

    $commandMatches = [regex]::Matches($Content, $xamlCommandPattern)
    foreach ($match in $commandMatches) {
        $names += $match.Groups[1].Value
    }

    return ($names | Select-Object -Unique)
}

function Get-ModelProperties {
    param([AllowEmptyString()][string]$Content)

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return @()
    }

    $matches = [regex]::Matches($Content, $propertyPattern)
    $names = @()
    foreach ($match in $matches) {
        $names += $match.Groups[1].Value
    }

    return ($names | Select-Object -Unique)
}

function Get-EnumMembers {
    param(
        [AllowEmptyString()][string]$Content,
        [string]$SourcePath
    )

    $candidateContent = $Content
    if ([string]::IsNullOrWhiteSpace($candidateContent) -and $SourcePath -and (Test-Path $SourcePath)) {
        $candidateContent = Get-Content -LiteralPath $SourcePath -Raw
    }

    if ([string]::IsNullOrWhiteSpace($candidateContent)) {
        return @()
    }

    $enumMatch = [regex]::Match($candidateContent, $enumMemberPattern)
    if (-not $enumMatch.Success) {
        return @()
    }

    $body = $enumMatch.Groups[1].Value
    $lines = $body -split "\r?\n"
    $members = @()
    foreach ($line in $lines) {
        $trimmed = ($line -replace "//.*$", "").Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) {
            continue
        }

        foreach ($chunk in ($trimmed -split ',')) {
            $part = $chunk.Trim()
            if ($part -match '^([A-Za-z_][A-Za-z0-9_]*)(\s*=\s*[-]?[0-9]+)?$') {
                $members += $part
            }
        }
    }

    return ($members | Select-Object -Unique)
}

function Build-MermaidDiagram {
    param([string[]]$Steps)

    $lines = @()
    $lines += "flowchart TD"
    $lines += "    Start([Start])"

    $nodeIndex = 1
    $previous = "Start"

    foreach ($step in $Steps) {
        $nodeId = "Step$nodeIndex"
        $label = Convert-MermaidLabel -Value (Format-StepLabel -Value $step)
        $lines += "    $nodeId[$label]"
        $lines += "    $previous --> $nodeId"
        $previous = $nodeId
        $nodeIndex++
    }

    $lines += "    End([End])"
    $lines += "    $previous --> End"

    return ($lines -join "`n")
}

function Build-MermaidDiagramWithDecisions {
    param(
        [string[]]$Steps,
        [string[]]$Decisions
    )

    $decisionList = @($Decisions)
    if (-not $decisionList -or $decisionList.Length -eq 0) {
        return Build-MermaidDiagram -Steps $Steps
    }

    $lines = @()
    $lines += "flowchart TD"
    $lines += "    Start([Start])"

    $nodeIndex = 1
    $previous = "Start"

    foreach ($step in $Steps) {
        $nodeId = "Step$nodeIndex"
        $label = Convert-MermaidLabel -Value (Format-StepLabel -Value $step)
        $lines += "    $nodeId[$label]"
        $lines += "    $previous --> $nodeId"
        $previous = $nodeId
        $nodeIndex++

        if ($decisionList.Length -ge $nodeIndex - 1) {
            $decisionText = $decisionList[$nodeIndex - 2]
            if ($decisionText) {
                $decisionLabel = Convert-MermaidLabel -Value (Normalize-SpacedLetters -Value $decisionText)
                $decisionId = "Decision$nodeIndex"
                $yesId = "Yes$nodeIndex"
                $noId = "No$nodeIndex"
                $lines += "    $decisionId{$decisionLabel}"
                $lines += "    $previous --> $decisionId"
                $lines += ('    {0}["Condition met"]' -f $yesId)
                $lines += ('    {0}["Condition not met"]' -f $noId)
                $lines += "    $decisionId -->|Condition met| $yesId"
                $lines += "    $decisionId -->|Condition not met| $noId"
                $lines += "    $yesId --> $previous"
                $lines += "    $noId --> $previous"
            }
        }
    }

    $lines += "    End([End])"
    $lines += "    $previous --> End"

    return ($lines -join "`n")
}

function Build-UserFriendlySteps {
    param([string[]]$Steps)

    $stepList = @($Steps)
    if (-not $stepList -or $stepList.Length -eq 0) {
        return "1. Review the workflow entry points and fill in details."
    }

    $lines = @()
    $index = 1
    foreach ($step in $stepList) {
        $label = Format-StepLabel -Value $step
        $lines += "$index. $label."
        $index++
    }

    return ($lines -join "`n")
}

function Build-RequiredInfoRows {
    param([string[]]$Steps)

    $stepList = @($Steps)
    if (-not $stepList -or $stepList.Length -eq 0) {
        return "| Review | Entry point | n/a | n/a | n/a | Fill from code | "
    }

    $rows = @()
    foreach ($step in $stepList) {
        $label = Format-StepLabel -Value $step
        $rows += "| $label | Invoke $label | n/a | n/a | Method: $step | See implementation | "
    }

    return ($rows -join "`n")
}

function Get-GeneratedWorkflowContent {
    param(
        [Parameter(Mandatory = $true)][object[]]$Index,
        [Parameter(Mandatory = $true)][string]$WorkflowName,
        [string]$ModuleFolder,
        [string]$WorkflowType
    )

    $section = Find-WorkflowSection -Index $Index -WorkflowName $WorkflowName -ModuleFolder $ModuleFolder -WorkflowType $WorkflowType
    if (-not $section -and $ModuleFolder) {
        $section = Find-WorkflowSection -Index $Index -WorkflowName $WorkflowName -ModuleFolder $null -WorkflowType $WorkflowType
    }

    $stepsSection = $section
    if ($WorkflowType -eq "dialog") {
        $vmSection = $Index | Where-Object { $_.Path -match "ViewModel_$WorkflowName\.cs$" } | Select-Object -First 1
        if ($vmSection) {
            $stepsSection = $vmSection
        }
        else {
            $viewCodeBehind = $Index | Where-Object { $_.Path -match "View_$WorkflowName\.xaml\.cs$" } | Select-Object -First 1
            if ($viewCodeBehind) {
                $stepsSection = $viewCodeBehind
            }
        }
    }

    if (-not $section) {
        $fallbackSteps = @($WorkflowName)
        return [pscustomobject]@{
            MermaidDiagram    = Build-MermaidDiagram -Steps $fallbackSteps
            UserFriendlySteps = Build-UserFriendlySteps -Steps $fallbackSteps
            RequiredInfoRows  = Build-RequiredInfoRows -Steps $fallbackSteps
            ThingsToFix       = "- None detected."
        }
    }

    $content = if ($stepsSection -and $stepsSection.Content) { $stepsSection.Content } else { "" }
    $sourcePath = if ($stepsSection -and $stepsSection.Path) { Join-Path $projectRoot $stepsSection.Path } else { $null }
    $steps = @()
    switch ($WorkflowType) {
        "viewmodel" { $steps = Get-ViewModelCommandNames -Content $content }
        "validator" { $steps = Get-ValidatorFields -Content $content }
        "dao" { $steps = Get-MethodNamesFromContent -Content $content }
        "handler" { $steps = Get-MethodNamesFromContent -Content $content }
        "command" { $steps = Get-MethodNamesFromContent -Content $content }
        "query" { $steps = Get-MethodNamesFromContent -Content $content }
        "test" { $steps = Get-TestMethodNames -Content $content }
        "xaml" { $steps = Get-XamlEvents -Content $content }
        "view" { $steps = Get-MethodNamesFromContent -Content $content }
        "model" { $steps = Get-ModelProperties -Content $content }
        "enum" { $steps = Get-EnumMembers -Content $content -SourcePath $sourcePath }
        default { $steps = Get-MethodNamesFromContent -Content $content }
    }

    if ($WorkflowType -eq "dialog" -and (-not $steps -or $steps.Length -eq 0)) {
        $steps = Get-MethodNamesFromContent -Content $content
    }

    if ($WorkflowType -eq "viewmodel" -and (-not $steps -or $steps.Length -eq 0)) {
        $steps = Get-MethodNamesFromContent -Content $content
    }

    $steps = Normalize-StepNames -Steps $steps
    $unfilteredSteps = $steps
    $workflowToken = Normalize-CompareToken -Value $WorkflowName
    $steps = @($steps | Where-Object { (Normalize-CompareToken -Value $_) -ne $workflowToken })
    $keywordFilter = $stepKeywordMap[$WorkflowType]
    $steps = Filter-StepsByKeywords -Steps $steps -IncludeKeywords $keywordFilter

    if (-not $steps -or $steps.Length -eq 0) {
        $steps = $unfilteredSteps
    }

    if ($WorkflowType -eq "dialog" -and (-not $steps -or $steps.Length -eq 0)) {
        $steps = @()
    }

    $topSteps = @($steps | Select-Object -First 8)
    if (-not $topSteps -or $topSteps.Length -eq 0) {
        $topSteps = @($WorkflowName)
    }

    $decisions = Get-DecisionConditions -Content $content -SourcePath $sourcePath
    $diagram = Build-MermaidDiagramWithDecisions -Steps $topSteps -Decisions $decisions

    return [pscustomobject]@{
        MermaidDiagram    = $diagram
        UserFriendlySteps = Build-UserFriendlySteps -Steps $topSteps
        RequiredInfoRows  = Build-RequiredInfoRows -Steps $topSteps
        ThingsToFix       = "- None detected."
    }
}

function Get-WorkflowItemsFromConfig {
    param([Parameter(Mandatory = $true)][string]$ConfigPath)

    if (-not (Test-Path $ConfigPath)) {
        throw "Workflow config not found: $ConfigPath"
    }

    $json = Get-Content -LiteralPath $ConfigPath -Raw
    $items = $json | ConvertFrom-Json

    if (-not $items) {
        return @()
    }

    return $items
}

function Get-WorkflowNamesFromRepomix {
    param([Parameter(Mandatory = $true)][string]$RepomixPath)

    if (-not (Test-Path $RepomixPath)) {
        throw "Repomix output not found: $RepomixPath"
    }

    $content = Get-Content -LiteralPath $RepomixPath -Raw

    $fileMatches = [regex]::Matches($content, "^## File: (.+)$", [System.Text.RegularExpressions.RegexOptions]::Multiline)
    if ($fileMatches.Count -eq 0) {
        return @()
    }

    $keywords = @(
        "Workflow",
        "History",
        "Settings",
        "Preview",
        "Generate",
        "Shipment",
        "Part",
        "Dialog",
        "Wizard",
        "Entry",
        "Search",
        "AutoSuggest",
        "Pending",
        "Resume",
        "Initial"
    )

    $pattern = "(" + ($keywords -join "|") + ")"

    $names = @()
    foreach ($match in $fileMatches) {
        $path = $match.Groups[1].Value.Trim()
        $fileName = [System.IO.Path]::GetFileNameWithoutExtension($path)

        if ($fileName -notmatch $pattern) {
            continue
        }

        $fileName = $fileName -replace "^(ViewModel_|View_)", ""
        $names += $fileName
    }

    return ($names | Sort-Object -Unique)
}

function Get-WorkflowNamesFromRepomixByModule {
    param(
        [Parameter(Mandatory = $true)][string]$RepomixPath,
        [Parameter(Mandatory = $true)][string]$ModuleFolder
    )

    if (-not (Test-Path $RepomixPath)) {
        throw "Repomix output not found: $RepomixPath"
    }

    $content = Get-Content -LiteralPath $RepomixPath -Raw

    $fileMatches = [regex]::Matches($content, "^## File: (.+)$", [System.Text.RegularExpressions.RegexOptions]::Multiline)
    if ($fileMatches.Count -eq 0) {
        return @()
    }

    $keywords = @(
        "Workflow",
        "History",
        "Settings",
        "Preview",
        "Generate",
        "Shipment",
        "Part",
        "Dialog",
        "Wizard",
        "Entry",
        "Search",
        "AutoSuggest",
        "Pending",
        "Resume",
        "Initial"
    )

    $pattern = "(" + ($keywords -join "|") + ")"
    $modulePrefix = ($ModuleFolder.TrimEnd([char[]]@('/', '\')) + "/")

    $names = @()
    foreach ($match in $fileMatches) {
        $path = $match.Groups[1].Value.Trim()
        if (-not $path.StartsWith($modulePrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
            continue
        }

        $fileName = [System.IO.Path]::GetFileNameWithoutExtension($path)

        if ($fileName -notmatch $pattern) {
            continue
        }

        $fileName = $fileName -replace "^(ViewModel_|View_)", ""
        $names += $fileName
    }

    return ($names | Sort-Object -Unique)
}

function Get-ProjectModules {
    param([Parameter(Mandatory = $true)][string]$RootPath)

    if (-not (Test-Path $RootPath)) {
        throw "Project root not found: $RootPath"
    }

    return Get-ChildItem -Path $RootPath -Directory |
    Where-Object { $_.Name -like "Module_*" } |
    Sort-Object -Property Name |
    Select-Object -ExpandProperty Name
}

function Normalize-ModuleName {
    param([Parameter(Mandatory = $true)][string]$Name)

    if ($Name -match "^Module_") {
        return $Name
    }

    return "Module_{0}" -f $Name
}

function Prompt-ModuleSelection {
    param([Parameter(Mandatory = $true)][string[]]$AvailableModules)

    if (-not $AvailableModules -or $AvailableModules.Count -eq 0) {
        throw "No modules found to select."
    }

    Write-Host "Select modules to generate workflow files:" -ForegroundColor Cyan
    for ($i = 0; $i -lt $AvailableModules.Count; $i++) {
        $index = $i + 1
        Write-Host ("[{0}] {1}" -f $index, $AvailableModules[$i])
    }

    Write-Host "Enter numbers (comma-separated) or 'all' to select every module." -ForegroundColor Gray
    $input = Read-Host "Selection"

    if ([string]::IsNullOrWhiteSpace($input)) {
        return @()
    }

    if ($input.Trim().ToLowerInvariant() -eq "all") {
        return $AvailableModules
    }

    $indices = @($input.Split(",") | ForEach-Object { $_.Trim() } | Where-Object { $_ -match "^\d+$" })
    if (-not $indices -or $indices.Length -eq 0) {
        return @()
    }

    $selected = @()
    foreach ($indexText in $indices) {
        $index = [int]$indexText
        if ($index -lt 1 -or $index -gt $AvailableModules.Count) {
            continue
        }

        $selected += $AvailableModules[$index - 1]
    }

    return ($selected | Sort-Object -Unique)
}

function New-WorkflowFile {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$TemplateName,
        [Parameter(Mandatory = $true)][string]$TemplateRootPath,
        [Parameter(Mandatory = $true)][string]$OutputFolder,
        [Parameter(Mandatory = $true)][string]$RepomixPath,
        [Parameter(Mandatory = $true)][string]$TypeValue,
        [Parameter(Mandatory = $true)][object[]]$RepomixIndex,
        [string]$ModuleFolder,
        [string]$WorkflowType
    )

    $templatePath = Resolve-TemplatePath -TemplateName $TemplateName -Root $TemplateRootPath
    if (-not (Test-Path $templatePath)) {
        throw "Template not found: $templatePath"
    }

    $template = Get-Content -LiteralPath $templatePath -Raw
    $safeName = Normalize-WorkflowName -Name $Name

    $workflowTitle = ($Name -replace "_", " ").Trim()
    if ([string]::IsNullOrWhiteSpace($workflowTitle)) {
        $workflowTitle = "Workflow"
    }

    $generated = Get-GeneratedWorkflowContent -Index $RepomixIndex -WorkflowName $Name -ModuleFolder $ModuleFolder -WorkflowType $WorkflowType

    $outputFile = Join-Path $OutputFolder ("Workflow_{0}.md" -f $safeName)
    $generatedOn = (Get-Date).ToString("yyyy-MM-dd")

    $stepCount = 0
    if ($generated.UserFriendlySteps) {
        $stepCount = ($generated.UserFriendlySteps -split "`n").Count
    }

    $hasDecision = $false
    if ($generated.MermaidDiagram) {
        $hasDecision = $generated.MermaidDiagram -match '\|Yes\|' -or $generated.MermaidDiagram -match '\{'
    }

    if ($stepCount -le 1 -and -not $hasDecision) {
        if (Test-Path $outputFile) {
            Remove-Item -LiteralPath $outputFile -Force
        }
        Write-Host "Skipping $safeName (only $stepCount step, no decisions)." -ForegroundColor Yellow
        return $null
    }

    $content = $template
    $content = $content.Replace("{{WorkflowTitle}}", $workflowTitle)
    $content = $content.Replace("{{RepomixOutputPath}}", $RepomixPath)
    $content = $content.Replace("{{Type}}", $TypeValue)
    $content = $content.Replace("{{GeneratedOn}}", $generatedOn)
    $content = $content.Replace("{{MermaidDiagram}}", $generated.MermaidDiagram)
    $content = $content.Replace("{{ThingsToFix}}", $generated.ThingsToFix)
    $content = $content.Replace("{{UserFriendlySteps}}", $generated.UserFriendlySteps)
    $content = $content.Replace("{{RequiredInfoRows}}", $generated.RequiredInfoRows)

    if (-not (Test-Path $OutputFolder)) {
        New-Item -ItemType Directory -Path $OutputFolder -Force | Out-Null
    }
    Set-Content -LiteralPath $outputFile -Value $content -Encoding UTF8
    return $outputFile
}

if ([string]::IsNullOrWhiteSpace($RepomixOutputPath)) {
    $RepomixOutputPath = $defaultRepomixOutputPath
}

if (-not [System.IO.Path]::IsPathRooted($RepomixOutputPath)) {
    $RepomixOutputPath = Join-Path $projectRoot $RepomixOutputPath
}

if (-not (Test-Path $RepomixOutputPath)) {
    throw "Repomix output not found: $RepomixOutputPath"
}

if (-not [System.IO.Path]::IsPathRooted($TemplateRoot)) {
    $TemplateRoot = Join-Path $projectRoot $TemplateRoot
}

if ($WorkflowConfigPath -and -not [System.IO.Path]::IsPathRooted($WorkflowConfigPath)) {
    $WorkflowConfigPath = Join-Path $projectRoot $WorkflowConfigPath
}

if (-not (Test-Path $TemplateRoot)) {
    throw "Template root not found: $TemplateRoot"
}

$repomixIndex = Get-RepomixFileIndex -RepomixPath $RepomixOutputPath

$outputRoot = Join-Path $projectRoot (Join-Path ".repomix/diagrams" $Type)

$workflowItems = @()
if ($Interactive.IsPresent -or ($ModuleNames -and $ModuleNames.Count -gt 0) -or [string]::IsNullOrWhiteSpace($Type)) {
    $availableModules = Get-ProjectModules -RootPath $projectRoot
    $selectedModules = @()

    if ($ModuleNames -and $ModuleNames.Count -gt 0) {
        $selectedModules = $ModuleNames | ForEach-Object { Normalize-ModuleName -Name $_ }
    }
    else {
        $selectedModules = Prompt-ModuleSelection -AvailableModules $availableModules
    }

    $selectedModules = @($selectedModules)
    if (-not $selectedModules -or $selectedModules.Length -eq 0) {
        throw "No modules selected."
    }

    $created = @()
    foreach ($module in $selectedModules) {
        $moduleType = $module -replace "^Module_", ""
        $outputRoot = Join-Path $projectRoot (Join-Path ".repomix/diagrams" $moduleType)

        if ($WorkflowConfigPath) {
            $workflowItems = Get-WorkflowItemsFromConfig -ConfigPath $WorkflowConfigPath
        }
        elseif ($WorkflowNames -and $WorkflowNames.Count -gt 0) {
            $workflowItems = $WorkflowNames | ForEach-Object { @{ name = $_; template = $Template } }
        }
        else {
            $names = Get-WorkflowNamesFromRepomixByModule -RepomixPath $RepomixOutputPath -ModuleFolder $module
            $workflowItems = $names | ForEach-Object { @{ name = $_; template = $Template } }
        }

        foreach ($item in $workflowItems) {
            $name = $item.name
            $templateName = if ($item.template) { $item.template } else { $Template }

            if (-not $name) {
                continue
            }

            $sourceMatch = Find-WorkflowSection -Index $repomixIndex -WorkflowName $name -ModuleFolder $module
            $sourcePath = if ($sourceMatch) { $sourceMatch.Path } else { $null }
            $workflowType = Get-WorkflowType -WorkflowName $name -SourcePath $sourcePath
            $templateName = if ($item.template) { $item.template } else { $workflowType }

            $created += New-WorkflowFile `
                -Name $name `
                -TemplateName $templateName `
                -TemplateRootPath $TemplateRoot `
                -OutputFolder $outputRoot `
                -RepomixPath $RepomixOutputPath `
                -TypeValue $moduleType `
                -RepomixIndex $repomixIndex `
                -ModuleFolder $module `
                -WorkflowType $workflowType
        }
    }

    Write-Host "Created $($created.Count) workflow files:" -ForegroundColor Green
    $created | ForEach-Object { Write-Host "- $_" }
}
else {
    if ($WorkflowConfigPath) {
        $workflowItems = Get-WorkflowItemsFromConfig -ConfigPath $WorkflowConfigPath
    }
    elseif ($WorkflowNames -and $WorkflowNames.Count -gt 0) {
        $workflowItems = $WorkflowNames | ForEach-Object { @{ name = $_; template = $Template } }
    }
    elseif ($AutoDiscover.IsPresent) {
        $names = Get-WorkflowNamesFromRepomix -RepomixPath $RepomixOutputPath
        $workflowItems = $names | ForEach-Object { @{ name = $_; template = $Template } }
    }
    else {
        throw "Provide WorkflowNames, WorkflowConfigPath, or -AutoDiscover."
    }

    $created = @()
    foreach ($item in $workflowItems) {
        $name = $item.name
        $templateName = if ($item.template) { $item.template } else { $Template }

        if (-not $name) {
            continue
        }

        $sourceMatch = Find-WorkflowSection -Index $repomixIndex -WorkflowName $name -ModuleFolder $null
        $sourcePath = if ($sourceMatch) { $sourceMatch.Path } else { $null }
        $workflowType = Get-WorkflowType -WorkflowName $name -SourcePath $sourcePath
        $templateName = if ($item.template) { $item.template } else { $workflowType }

        $created += New-WorkflowFile `
            -Name $name `
            -TemplateName $templateName `
            -TemplateRootPath $TemplateRoot `
            -OutputFolder $outputRoot `
            -RepomixPath $RepomixOutputPath `
            -TypeValue $Type `
            -RepomixIndex $repomixIndex `
            -WorkflowType $workflowType
    }

    Write-Host "Created $($created.Count) workflow files:" -ForegroundColor Green
    $created | ForEach-Object { Write-Host "- $_" }
}

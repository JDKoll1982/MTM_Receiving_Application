# Infor Visual Integration Discovery

**Purpose:** Run this prompt in a GitHub Copilot agent session on the machine where Infor Visual
is installed. The agent will execute each PowerShell block via the terminal and collect results
into a structured report that answers every open question in the integration approach documents.

**Before running:** Open Infor Visual and navigate to the **Receiver Entry** screen so the window
is visible and active. Do not close it during the discovery session.

---

## Instructions for the Agent

Work through each section below in order. For every PowerShell block:
1. Run the command exactly as written in the terminal.
2. Capture the full output.
3. Record findings in the **Results** subsection immediately below it.
4. If a command fails or returns nothing, note that and move on — do not stop.

At the end, compile everything into `docs/InforVisual/Integration/Visual_Discovery_Report.md`.

---

## Section 1 — Visual Executable and Process

### Find the running Visual process

```powershell
Get-Process | Where-Object { $_.MainWindowTitle -ne '' } |
    Select-Object -Property Name, Id, MainWindowTitle, Path |
    Format-List
```

**Looking for:** The exact process name (e.g. `VMFG`, `Visual`, `InforVisual`) and the exact
window title string as it appears in the OS. Both are required for `AppActivate` in the macro.

### Find all Visual-related executables on disk

```powershell
@('\\visual\visual908$\VMFG', 'C:\Program Files', 'C:\Program Files (x86)', 'D:\', 'E:\') | ForEach-Object {
    if (Test-Path $_) {
        Get-ChildItem -Path $_ -Recurse -ErrorAction SilentlyContinue -Filter '*.exe' |
            Where-Object { $_.Name -match 'visual|vmfg|infor' -or
                           $_.DirectoryName -match 'visual|vmfg|infor' } |
            Select-Object FullName, LastWriteTime, @{N='SizeMB';E={[math]::Round($_.Length/1MB,2)}}
    }
}
```

**Looking for:** Installation path, executable name, and version (LastWriteTime is a proxy).

### Get Visual executable version

```powershell
$exePaths = Get-ChildItem '\\visual\visual908$\VMFG', 'C:\Program Files', 'C:\Program Files (x86)' -Recurse `
    -ErrorAction SilentlyContinue -Filter '*.exe' |
    Where-Object { $_.Name -match 'vmfg|visual' -and $_.Length -gt 1MB }

foreach ($exe in $exePaths) {
    $v = (Get-Item $exe.FullName).VersionInfo
    [PSCustomObject]@{
        Path           = $exe.FullName
        FileVersion    = $v.FileVersion
        ProductVersion = $v.ProductVersion
        ProductName    = $v.ProductName
        CompanyName    = $v.CompanyName
    }
}
```

**Looking for:** Confirmed product version (e.g. `9.0.8.xxx`) to match against Infor documentation.

### Check the registry for Visual installation entries

```powershell
$paths = @(
    'HKLM:\SOFTWARE\Infor',
    'HKLM:\SOFTWARE\WOW6432Node\Infor',
    'HKLM:\SOFTWARE\MAPICS',
    'HKLM:\SOFTWARE\WOW6432Node\MAPICS',
    'HKCU:\SOFTWARE\Infor'
)
foreach ($p in $paths) {
    if (Test-Path $p) {
        Get-ChildItem -Path $p -Recurse -ErrorAction SilentlyContinue |
            Get-ItemProperty -ErrorAction SilentlyContinue
    }
}
```

**Looking for:** Installation directory, version number, site ID, license keys, macro settings.

---

## Section 2 — Window Title and UI Automation Identifiers

### Dump all visible window titles

```powershell
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$desktop = [System.Windows.Automation.AutomationElement]::RootElement
$condition = [System.Windows.Automation.Condition]::TrueCondition
$windows = $desktop.FindAll(
    [System.Windows.Automation.TreeScope]::Children,
    $condition
)

$windows | ForEach-Object {
    [PSCustomObject]@{
        Name           = $_.Current.Name
        AutomationId   = $_.Current.AutomationId
        ClassName      = $_.Current.ClassName
        ProcessId      = $_.Current.ProcessId
        ControlType    = $_.Current.ControlType.ProgrammaticName
    }
} | Sort-Object Name | Format-Table -AutoSize
```

**Looking for:** The exact `Name` (window title) of the Receiver Entry screen.

### Enumerate all UI controls in the Visual foreground window

> **Note:** Visual must be open and the Receiver Entry window must be in the foreground
> before running this block.

```powershell
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes

$desktop = [System.Windows.Automation.AutomationElement]::RootElement
$condition = [System.Windows.Automation.Condition]::TrueCondition

# Get the foreground window
$hwnd = (Get-Process | Where-Object { $_.MainWindowTitle -ne '' } |
    Sort-Object { $_.MainWindowTitle.Length } -Descending |
    Select-Object -First 1).MainWindowHandle

$foreground = [System.Windows.Automation.AutomationElement]::FromHandle($hwnd)

$allElements = $foreground.FindAll(
    [System.Windows.Automation.TreeScope]::Descendants,
    $condition
)

$results = $allElements | ForEach-Object {
    [PSCustomObject]@{
        Name         = $_.Current.Name
        AutomationId = $_.Current.AutomationId
        ClassName    = $_.Current.ClassName
        ControlType  = $_.Current.ControlType.ProgrammaticName
        IsEnabled    = $_.Current.IsEnabled
        IsOffscreen  = $_.Current.IsOffscreen
    }
} | Where-Object { -not $_.IsOffscreen }

$results | Format-Table -AutoSize
$results | Export-Csv -Path "$env:TEMP\visual_uia_dump.csv" -NoTypeInformation
Write-Host "Full dump saved to: $env:TEMP\visual_uia_dump.csv"
```

**Looking for:** `AutomationId` values for the PO number field, received date, employee/user field,
and all grid columns in the Receiver Entry screen. These are the control IDs needed for Approach 3
(UI Automation) and are useful for verifying control names for Approach 2 (VBScript).

### Read the UIA dump for Edit controls only

```powershell
Import-Csv "$env:TEMP\visual_uia_dump.csv" |
    Where-Object { $_.ControlType -match 'Edit|DataItem|DataGrid|ComboBox' } |
    Select-Object Name, AutomationId, ClassName, ControlType |
    Format-Table -AutoSize
```

**Looking for:** Filterable list of all input controls and grid cells visible on Receiver Entry.

---

## Section 3 — VBScript / Macro Infrastructure

### Confirm wscript.exe is available

```powershell
Get-Command wscript.exe, cscript.exe -ErrorAction SilentlyContinue |
    Select-Object Name, Source, Version | Format-Table -AutoSize
```

### Check if Visual has a macro host directory

```powershell
$installPaths = @(
    '\\visual\visual908$\VMFG',
    'C:\Program Files\Infor',
    'C:\Program Files (x86)\Infor',
    'C:\VISUAL',
    'D:\VISUAL',
    'C:\Program Files\MAPICS'
)
foreach ($base in $installPaths) {
    if (Test-Path $base) {
        Get-ChildItem -Path $base -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.Extension -in '.vbs', '.vba', '.bas', '.hta', '.mac' } |
            Select-Object FullName, LastWriteTime, Length
    }
}
```

**Looking for:** Existing macro files shipped with Visual; macro host directory; any `.vbs` or
`.hta` files that show the directory structure Infor uses for scripts.

### Check for Visual's macro configuration in its INI or config files

```powershell
$installPaths = @(
    '\\visual\visual908$\VMFG',
    'C:\Program Files\Infor',
    'C:\Program Files (x86)\Infor',
    'C:\VISUAL',
    'D:\VISUAL'
)
foreach ($base in $installPaths) {
    if (Test-Path $base) {
        Get-ChildItem -Path $base -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.Extension -in '.ini', '.cfg', '.config', '.xml', '.json' -and
                           $_.Length -lt 500KB } |
            Select-Object FullName, LastWriteTime |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 30
    }
}
```

### Check execution policy and AV/script-blocking restrictions

```powershell
Get-ExecutionPolicy -List | Format-Table -AutoSize

# Check if AppLocker or WDAC policies block VBScript
Get-AppLockerPolicy -Effective -ErrorAction SilentlyContinue | Format-List

# Check Windows Defender Application Control
if (Test-Path 'HKLM:\SYSTEM\CurrentControlSet\Control\CI\Policy') {
    Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\CI\Policy'
}
```

**Looking for:** Whether `wscript.exe` can run unsigned `.vbs` files on this workstation.

---

## Section 4 — Import / EDI Facility

### Search Visual's directory for import-related files and documentation

```powershell
$installPaths = @(
    '\\visual\visual908$\VMFG',
    'C:\Program Files\Infor',
    'C:\Program Files (x86)\Infor',
    'C:\VISUAL',
    'D:\VISUAL'
)
$keywords = 'import', 'export', 'edi', 'transaction', 'receiver', 'receipt', 'batch'

foreach ($base in $installPaths) {
    if (Test-Path $base) {
        Get-ChildItem -Path $base -Recurse -ErrorAction SilentlyContinue |
            Where-Object {
                $name = $_.Name.ToLower()
                $keywords | Where-Object { $name -like "*$_*" }
            } |
            Select-Object FullName, Extension, LastWriteTime |
            Sort-Object Extension
    }
}
```

**Looking for:** Import template files (`.imp`, `.fmt`, `.xsd`, `.xml`), import utility executables,
or documentation PDFs that describe the receiver import file format.

### Search for XSD schema files that define import formats

```powershell
$installPaths = @(
    '\\visual\visual908$\VMFG',
    'C:\Program Files\Infor',
    'C:\Program Files (x86)\Infor',
    'C:\VISUAL',
    'D:\VISUAL'
)
foreach ($base in $installPaths) {
    if (Test-Path $base) {
        Get-ChildItem -Path $base -Recurse -ErrorAction SilentlyContinue -Filter '*.xsd' |
            Select-Object FullName, LastWriteTime
    }
}
```

### Look for network import drop-folder configuration

```powershell
# Check mapped drives
Get-PSDrive -PSProvider FileSystem | Format-Table -AutoSize

# Check if any Visual config references an import folder path
$installPaths = @(
    '\\visual\visual908$\VMFG',
    'C:\Program Files\Infor',
    'C:\Program Files (x86)\Infor',
    'C:\VISUAL',
    'D:\VISUAL'
)
foreach ($base in $installPaths) {
    if (Test-Path $base) {
        Get-ChildItem -Path $base -Recurse -ErrorAction SilentlyContinue -Filter '*.ini' |
            ForEach-Object {
                $content = Get-Content $_.FullName -ErrorAction SilentlyContinue
                if ($content -match 'import|folder|path|drop') {
                    Write-Host "=== $($_.FullName) ==="
                    $content | Select-String 'import|folder|path|drop' -CaseSensitive:$false
                }
            }
    }
}
```

---

## Section 5 — ION / Infor OS

### Check for ION-related services

```powershell
Get-Service | Where-Object { $_.Name -match 'ION|infor|ming|LifeCycle' } |
    Select-Object Name, DisplayName, Status | Format-Table -AutoSize
```

### Check for ION-related processes

```powershell
Get-Process | Where-Object { $_.Name -match 'ION|infor|ming' } |
    Select-Object Name, Id, Path | Format-Table -AutoSize
```

### Check for ION registry keys

```powershell
$paths = @(
    'HKLM:\SOFTWARE\Infor\ION',
    'HKLM:\SOFTWARE\WOW6432Node\Infor\ION',
    'HKCU:\SOFTWARE\Infor\ION'
)
foreach ($p in $paths) {
    if (Test-Path $p) {
        Write-Host "=== Found: $p ==="
        Get-ItemProperty -Path $p
    } else {
        Write-Host "Not found: $p"
    }
}
```

### Check for ION API config files

```powershell
@('C:\', 'D:\') | ForEach-Object {
    Get-ChildItem -Path $_ -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match 'ion.*config|ionapi|ion.*settings' } |
        Select-Object FullName, LastWriteTime
}
```

**Looking for:** Any sign that ION is installed, licensed, or reachable from this workstation.
If none of these commands return results, ION is not present on this machine.

---

## Section 6 — Network and Server Connectivity

### Identify the Visual database server

```powershell
# Check connection strings used by Visual processes
Get-Process | Where-Object { $_.Name -match 'vmfg|visual' } |
    ForEach-Object {
        try {
            $handles = $_.Modules | Select-Object -ExpandProperty FileName -ErrorAction SilentlyContinue
            $handles
        } catch {}
    }

# Look for ODBC data sources configured for Visual
Get-OdbcDsn -ErrorAction SilentlyContinue | Format-Table -AutoSize

# Check ODBC registry
Get-ItemProperty 'HKLM:\SOFTWARE\ODBC\ODBC.INI\ODBC Data Sources' -ErrorAction SilentlyContinue
Get-ItemProperty 'HKLM:\SOFTWARE\WOW6432Node\ODBC\ODBC.INI\ODBC Data Sources' -ErrorAction SilentlyContinue
```

### Confirm SQL Server reachability

```powershell
# Replace VISUAL with the actual server name found above
Test-NetConnection -ComputerName 'VISUAL' -Port 1433 -WarningAction SilentlyContinue |
    Select-Object ComputerName, RemotePort, TcpTestSucceeded
```

### List current mapped network drives

```powershell
Get-PSDrive -PSProvider FileSystem |
    Where-Object { $_.Root -match '^\\\\' } |
    Select-Object Name, Root, Description | Format-Table -AutoSize

net use 2>&1
```

**Looking for:** Any mapped drives that could serve as the handoff file drop location for
Approach 2 or the import folder for Approach 5.

---

## Section 7 — Visual's Receiver Entry Screen (Manual Inspection Steps)

These cannot be automated — complete them with Visual open on the Receiver Entry screen.

Run this to open the Accessibility Insights or Inspect tool for manual inspection:

```powershell
# Try to open Inspect.exe (ships with Windows SDK)
$inspectPaths = @(
    "${env:ProgramFiles(x86)}\Windows Kits\10\bin\*\x64\inspect.exe",
    "${env:ProgramFiles(x86)}\Windows Kits\10\bin\*\x86\inspect.exe",
    "$env:ProgramFiles\Windows Kits\10\bin\*\x64\inspect.exe"
)
$found = $inspectPaths | ForEach-Object { Get-Item $_ -ErrorAction SilentlyContinue } |
    Select-Object -First 1

if ($found) {
    Write-Host "Opening Inspect.exe: $($found.FullName)"
    Start-Process $found.FullName
} else {
    Write-Host "Inspect.exe not found. Install Windows 10 SDK or use Accessibility Insights."
    Write-Host "Download: https://accessibilityinsights.io/downloads/"
}
```

**Manual steps to complete with Inspect.exe or Accessibility Insights open:**

1. Click each field in the Receiver Entry **header** and record:
   - Field label (visible on screen)
   - `AutomationId`
   - `ClassName`
   - `ControlType`
   - Tab order (press Tab from the first field and number each stop)

2. Click each column header and cell in the Receiver Entry **grid** and record the same.

3. Note the exact keyboard shortcut or menu path to open Receiver Entry from Visual's main menu.

4. Note whether the grid accepts input via `Tab`/`Enter` or requires clicking into each cell.

5. Screenshot the Receiver Entry screen with field labels visible.

Record all findings in the results below.

---

## Section 8 — Compile Results Report

After all sections above are complete, run the following to bundle the collected output:

```powershell
$reportDir  = "$env:TEMP\visual_discovery_$(Get-Date -Format 'yyyyMMdd_HHmm')"
New-Item -ItemType Directory -Path $reportDir -Force | Out-Null

# Copy UIA dump if it exists
if (Test-Path "$env:TEMP\visual_uia_dump.csv") {
    Copy-Item "$env:TEMP\visual_uia_dump.csv" -Destination $reportDir
}

# Write a summary placeholder
@"
# Visual Discovery Report — $(Get-Date -Format 'yyyy-MM-dd HH:mm')

## Machine
$env:COMPUTERNAME

## Section 1 — Executable
<!-- Paste output here -->

## Section 2 — Window Title and UIA Controls
<!-- Paste output or note: see visual_uia_dump.csv -->

## Section 3 — VBScript / Macro
<!-- Paste output here -->

## Section 4 — Import / EDI
<!-- Paste output here -->

## Section 5 — ION
<!-- Paste output here -->

## Section 6 — Network
<!-- Paste output here -->

## Section 7 — Manual Inspection
<!-- Paste field name / AutomationId table here -->

## Open Questions Resolved
<!-- Check off from Approach_2, Approach_5, Approach_6 open questions lists -->
"@ | Out-File -FilePath "$reportDir\Visual_Discovery_Report.md" -Encoding utf8

Write-Host "Report template created at: $reportDir\Visual_Discovery_Report.md"
explorer.exe $reportDir
```

Save the completed `Visual_Discovery_Report.md` to:

```
docs/InforVisual/Integration/Visual_Discovery_Report.md
```

Then share it in a new Copilot session with the message:
> "Here is the Visual discovery report. Use it to finalize implementation of Approach 2."

---

## What This Prompt Resolves

| Open Question | Section That Answers It |
|---|---|
| Exact process name and window title for `AppActivate` | Section 1, Section 2 |
| `AutomationId` values for all Receiver Entry fields | Section 2 |
| Tab order and field names for `SendKeys` macro | Section 2 + Section 7 |
| Whether VBScript / `wscript.exe` runs on this workstation | Section 3 |
| Whether Visual has an import facility and what format it uses | Section 4 |
| Whether ION is present or licensed | Section 5 |
| SQL Server server name (for handoff file connectivity) | Section 6 |
| Available network shares for drop-folder | Section 6 |

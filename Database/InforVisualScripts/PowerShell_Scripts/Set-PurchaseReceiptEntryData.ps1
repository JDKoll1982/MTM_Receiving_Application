[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

throw @'
Set-PurchaseReceiptEntryData.ps1 is deprecated.

Use the new Inventory Transaction Entry Python scripts instead:
    1. .\.venv32\Scripts\python.exe Database\InforVisualScripts\Python_Scripts\discover_inventory_transaction_entry_layout.py
    2. .\.venv32\Scripts\python.exe Database\InforVisualScripts\Python_Scripts\set_inventory_transaction_entry.py --dry-run
    3. .\.venv32\Scripts\python.exe Database\InforVisualScripts\Python_Scripts\set_inventory_transaction_entry.py
'@

# Edit these values before running the script.
$PurchaseOrderNumber = 'PO-069300'
$RowNumber = 1
$LineNumber = '1'
$QuantityReceived = '2000'
$LocationId = 'RECV'
$ConfigPath = Join-Path $PSScriptRoot 'PurchaseReceiptEntryLayout.json'

Add-Type -AssemblyName System.Windows.Forms

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Text;

public static class NativeMethods
{
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetCursorPos(out POINT point);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr WindowFromPoint(POINT point);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetDlgCtrlID(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, string lParam, uint flags, uint timeout, out IntPtr result);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
}
'@

$mouseLeftDown = 0x0002
$mouseLeftUp = 0x0004
$wmSetText = 0x000C
$smtoAbortIfHung = 0x0002
$swRestore = 9

function Get-WindowTextValue {
    param(
        [AllowNull()]
        [System.Nullable[IntPtr]]$Handle
    )

    if ($null -eq $Handle -or $Handle.Value -eq [IntPtr]::Zero) {
        return ''
    }

    $length = [NativeMethods]::GetWindowTextLength($Handle.Value)
    $builder = [System.Text.StringBuilder]::new([Math]::Max($length + 1, 256))
    [void][NativeMethods]::GetWindowText($Handle.Value, $builder, $builder.Capacity)
    return $builder.ToString()
}

function Get-ClassNameValue {
    param(
        [AllowNull()]
        [System.Nullable[IntPtr]]$Handle
    )

    if ($null -eq $Handle -or $Handle.Value -eq [IntPtr]::Zero) {
        return ''
    }

    $builder = [System.Text.StringBuilder]::new(256)
    [void][NativeMethods]::GetClassName($Handle.Value, $builder, $builder.Capacity)
    return $builder.ToString()
}

function Get-WindowRectObject {
    param(
        [AllowNull()]
        [System.Nullable[IntPtr]]$Handle
    )

    if ($null -eq $Handle -or $Handle.Value -eq [IntPtr]::Zero) {
        throw 'Cannot get the window rectangle for an empty window handle.'
    }

    $rect = New-Object NativeMethods+RECT
    if (-not [NativeMethods]::GetWindowRect($Handle.Value, [ref]$rect)) {
        throw "Failed to get the window rectangle for handle $($Handle.Value)."
    }

    return [pscustomobject]@{
        Left = $rect.Left
        Top = $rect.Top
        Right = $rect.Right
        Bottom = $rect.Bottom
    }
}

function ConvertTo-SendKeysLiteral {
    param(
        [Parameter(Mandatory)]
        [string]$Value
    )

    $escaped = $Value.
        Replace('{', '{{}').
        Replace('}', '{}}').
        Replace('+', '{+}').
        Replace('^', '{^}').
        Replace('%', '{%}').
        Replace('~', '{~}').
        Replace('(', '{(}').
        Replace(')', '{)}')

    return $escaped
}

function Invoke-LeftClick {
    param(
        [Parameter(Mandatory)]
        [int]$X,

        [Parameter(Mandatory)]
        [int]$Y,

        [Parameter()]
        [switch]$DoubleClick
    )

    [void][NativeMethods]::SetCursorPos($X, $Y)
    Start-Sleep -Milliseconds 100
    [NativeMethods]::mouse_event($mouseLeftDown, 0, 0, 0, [UIntPtr]::Zero)
    [NativeMethods]::mouse_event($mouseLeftUp, 0, 0, 0, [UIntPtr]::Zero)

    if ($DoubleClick.IsPresent) {
        Start-Sleep -Milliseconds 100
        [NativeMethods]::mouse_event($mouseLeftDown, 0, 0, 0, [UIntPtr]::Zero)
        [NativeMethods]::mouse_event($mouseLeftUp, 0, 0, 0, [UIntPtr]::Zero)
    }
}

function Get-AnchorPoint {
    param(
        [Parameter(Mandatory)]
        [pscustomobject]$WindowRect,

        [Parameter(Mandatory)]
        [pscustomobject]$Anchor,

        [Parameter()]
        [int]$RowOffset = 0
    )

    return [pscustomobject]@{
        X = [int]($WindowRect.Left + $Anchor.RelativeX)
        Y = [int]($WindowRect.Top + $Anchor.RelativeY + $RowOffset)
    }
}

function Set-TextFieldValue {
    param(
        [Parameter(Mandatory)]
        [System.__ComObject]$Shell,

        [Parameter(Mandatory)]
        [pscustomobject]$Point,

        [Parameter(Mandatory)]
        [string]$Value,

        [Parameter()]
        [switch]$DoubleClick,

        [Parameter()]
        [switch]$UsePaste,

        [Parameter()]
        [int]$ExpectedControlId = -1,

        [Parameter()]
        [switch]$UseDirectSet
    )

    Invoke-LeftClick -X $Point.X -Y $Point.Y -DoubleClick:$DoubleClick
    Start-Sleep -Milliseconds 200

    if ($UseDirectSet.IsPresent -and $ExpectedControlId -ge 0) {
        $pointValue = New-Object NativeMethods+POINT
        $pointValue.X = $Point.X
        $pointValue.Y = $Point.Y
        $targetHandle = [NativeMethods]::WindowFromPoint($pointValue)
        $targetClass = ''
        $targetControlId = -1

        if ($null -ne $targetHandle -and $targetHandle -ne [IntPtr]::Zero) {
            $targetClass = Get-ClassNameValue -Handle $targetHandle
            $targetControlId = [NativeMethods]::GetDlgCtrlID($targetHandle)
        }

        if ($targetHandle -ne [IntPtr]::Zero -and $targetClass -eq 'Edit' -and $targetControlId -eq $ExpectedControlId) {
            $result = [IntPtr]::Zero
            $sendResult = [NativeMethods]::SendMessageTimeout($targetHandle, $wmSetText, [IntPtr]::Zero, $Value, $smtoAbortIfHung, 2000, [ref]$result)
            if ($sendResult -ne [IntPtr]::Zero) {
                Start-Sleep -Milliseconds 150
                return
            }
        }
    }

    $Shell.SendKeys('^a')
    Start-Sleep -Milliseconds 100

    if ($UsePaste.IsPresent) {
        Set-Clipboard -Value $Value
        Start-Sleep -Milliseconds 150
        $Shell.SendKeys('^a')
        Start-Sleep -Milliseconds 150
        $Shell.SendKeys('^v')
        Start-Sleep -Milliseconds 500
    }
    else {
        $Shell.SendKeys((ConvertTo-SendKeysLiteral -Value $Value))
    }

    Start-Sleep -Milliseconds 150
}

function Resolve-PurchaseReceiptWindowHandle {
    param(
        [Parameter(Mandatory)]
        [pscustomobject]$Config
    )

    $exactHandle = [NativeMethods]::FindWindow($null, [string]$Config.WindowTitle)
    if ($exactHandle -ne [IntPtr]::Zero) {
        return $exactHandle
    }

    $matchHandle = [IntPtr]::Zero
    $targetTitle = [string]$Config.WindowTitle
    $targetPrefix = ($targetTitle -split ' - Infor VISUAL')[0]

    $callback = [NativeMethods+EnumWindowsProc]{
        param([IntPtr]$hWnd, [IntPtr]$lParam)

        if (-not [NativeMethods]::IsWindowVisible($hWnd)) {
            return $true
        }

        $title = Get-WindowTextValue -Handle $hWnd
        if ([string]::IsNullOrWhiteSpace($title)) {
            return $true
        }

        if ($title -eq $targetTitle -or $title.StartsWith($targetPrefix)) {
            $script:matchHandle = $hWnd
            return $false
        }

        return $true
    }

    [void][NativeMethods]::EnumWindows($callback, [IntPtr]::Zero)

    if ($script:matchHandle -ne [IntPtr]::Zero) {
        return $script:matchHandle
    }

    return [IntPtr]::Zero
}

function Activate-PurchaseReceiptWindow {
    param(
        [Parameter(Mandatory)]
        [IntPtr]$WindowHandle,

        [Parameter(Mandatory)]
        [string]$WindowTitle,

        [Parameter(Mandatory)]
        [System.__ComObject]$Shell
    )

    [void][NativeMethods]::ShowWindow($WindowHandle, $swRestore)
    Start-Sleep -Milliseconds 200
    [void][NativeMethods]::SetForegroundWindow($WindowHandle)
    Start-Sleep -Milliseconds 200
    [void]$Shell.AppActivate($WindowTitle)
    Start-Sleep -Milliseconds 400

    $foregroundHandle = [NativeMethods]::GetForegroundWindow()
    $foregroundTitle = Get-WindowTextValue -Handle $foregroundHandle
    $expectedPrefix = ($WindowTitle -split ' - Infor VISUAL')[0]

    if (($null -eq $foregroundHandle -or $foregroundHandle -eq [IntPtr]::Zero) -or ($foregroundHandle -ne $WindowHandle -and -not $foregroundTitle.StartsWith($expectedPrefix))) {
        throw "Could not bring Purchase Receipt Entry to the foreground. Current foreground window is '$foregroundTitle'."
    }
}

if (-not (Test-Path -Path $ConfigPath)) {
    throw "Layout config not found at $ConfigPath. Run Discover-PurchaseReceiptEntryLayout.ps1 first."
}

$config = Get-Content -Path $ConfigPath -Raw | ConvertFrom-Json
if ($RowNumber -lt 1) {
    throw 'RowNumber must be 1 or greater.'
}

if (-not [string]::IsNullOrWhiteSpace($LineNumber)) {
    Write-Warning 'LineNumber is retained for operator reference only. The Ln# cell is read-only and will not be edited.'
}

$windowHandle = Resolve-PurchaseReceiptWindowHandle -Config $config
if ($windowHandle -eq [IntPtr]::Zero) {
    throw "Could not find the Purchase Receipt Entry window '$($config.WindowTitle)'."
}

$windowRect = Get-WindowRectObject -Handle $windowHandle
$rowOffset = ($RowNumber - 1) * [int]$config.RowHeight
$shell = New-Object -ComObject WScript.Shell

Activate-PurchaseReceiptWindow -WindowHandle $windowHandle -WindowTitle ([string]$config.WindowTitle) -Shell $shell

$purchaseOrderPoint = Get-AnchorPoint -WindowRect $windowRect -Anchor $config.Anchors.PurchaseOrderField
$rowSelectorPoint = Get-AnchorPoint -WindowRect $windowRect -Anchor $config.Anchors.FirstRowSelector -RowOffset $rowOffset
$quantityPoint = Get-AnchorPoint -WindowRect $windowRect -Anchor $config.Anchors.FirstRowQuantityCell -RowOffset $rowOffset
$locationPoint = Get-AnchorPoint -WindowRect $windowRect -Anchor $config.Anchors.FirstRowLocationCell -RowOffset $rowOffset

Set-TextFieldValue -Shell $shell -Point $purchaseOrderPoint -Value $PurchaseOrderNumber -DoubleClick

Invoke-LeftClick -X $rowSelectorPoint.X -Y $rowSelectorPoint.Y
Start-Sleep -Milliseconds 150

if (-not [string]::IsNullOrWhiteSpace($QuantityReceived)) {
    Set-TextFieldValue -Shell $shell -Point $quantityPoint -Value $QuantityReceived -DoubleClick
}

if (-not [string]::IsNullOrWhiteSpace($LocationId)) {
    Set-TextFieldValue -Shell $shell -Point $locationPoint -Value $LocationId -DoubleClick -ExpectedControlId ([int]$config.Anchors.FirstRowLocationCell.ChildControlId) -UseDirectSet
}

Write-Host "Updated Purchase Receipt Entry with PO '$PurchaseOrderNumber', row $RowNumber, quantity '$QuantityReceived', location '$LocationId'."

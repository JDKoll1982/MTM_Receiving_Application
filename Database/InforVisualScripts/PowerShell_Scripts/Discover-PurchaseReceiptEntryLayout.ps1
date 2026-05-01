[CmdletBinding()]
param(
    [Parameter()]
    [string]$WindowTitle = 'Purchase Receipt Entry - Infor VISUAL - MTMFG',

    [Parameter()]
    [string]$ConfigPath = (Join-Path $PSScriptRoot 'PurchaseReceiptEntryLayout.json'),

    [Parameter()]
    [ValidateRange(1, 10)]
    [int]$CaptureDelaySeconds = 3,

    [Parameter()]
    [ValidateRange(1, 10)]
    [int]$MaxAttempts = 3
)

$ErrorActionPreference = 'Stop'

Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
using System.Text;

public static class NativeMethods
{
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

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetCursorPos(out POINT point);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr WindowFromPoint(POINT point);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetDlgCtrlID(IntPtr hWnd);
}
'@

$gaRoot = 2

function Get-WindowTextValue {
    param(
        [Parameter(Mandatory)]
        [IntPtr]$Handle
    )

    $length = [NativeMethods]::GetWindowTextLength($Handle)
    $builder = [System.Text.StringBuilder]::new([Math]::Max($length + 1, 256))
    [void][NativeMethods]::GetWindowText($Handle, $builder, $builder.Capacity)
    return $builder.ToString()
}

function Get-ClassNameValue {
    param(
        [Parameter(Mandatory)]
        [IntPtr]$Handle
    )

    $builder = [System.Text.StringBuilder]::new(256)
    [void][NativeMethods]::GetClassName($Handle, $builder, $builder.Capacity)
    return $builder.ToString()
}

function Get-RectObject {
    param(
        [Parameter(Mandatory)]
        [IntPtr]$Handle
    )

    $rect = New-Object NativeMethods+RECT
    if (-not [NativeMethods]::GetWindowRect($Handle, [ref]$rect)) {
        throw "Failed to read RECT for handle $Handle."
    }

    return [pscustomobject]@{
        Left = $rect.Left
        Top = $rect.Top
        Right = $rect.Right
        Bottom = $rect.Bottom
        Width = $rect.Right - $rect.Left
        Height = $rect.Bottom - $rect.Top
    }
}

function Read-Anchor {
    param(
        [Parameter(Mandatory)]
        [string]$Prompt,

        [Parameter(Mandatory)]
        [string]$ExpectedWindowTitle
    )

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        Write-Host ''
        Write-Host $Prompt -ForegroundColor Cyan
        Write-Host "Attempt $attempt of $MaxAttempts" -ForegroundColor DarkCyan
        Write-Host 'Press Enter to start the countdown, then move the mouse over the requested Visual target.' -ForegroundColor Yellow
        [void](Read-Host)

        for ($seconds = $CaptureDelaySeconds; $seconds -ge 1; $seconds--) {
            Write-Host ("Capturing in {0}..." -f $seconds) -ForegroundColor DarkYellow
            Start-Sleep -Seconds 1
        }

        $point = New-Object NativeMethods+POINT
        if (-not [NativeMethods]::GetCursorPos([ref]$point)) {
            throw 'Failed to read the cursor position.'
        }

        $childHandle = [NativeMethods]::WindowFromPoint($point)
        $rootHandle = [NativeMethods]::GetAncestor($childHandle, $gaRoot)
        $rootRect = Get-RectObject -Handle $rootHandle
        $rootTitle = Get-WindowTextValue -Handle $rootHandle

        if ($rootTitle -ne $ExpectedWindowTitle) {
            Write-Warning "Expected window '$ExpectedWindowTitle' but captured '$rootTitle'."
            if ($attempt -lt $MaxAttempts) {
                Write-Host 'Retrying capture...' -ForegroundColor Yellow
                continue
            }

            throw "Expected window '$ExpectedWindowTitle' but captured '$rootTitle'."
        }

        return [pscustomobject]@{
            Prompt = $Prompt
            ScreenX = $point.X
            ScreenY = $point.Y
            RelativeX = $point.X - $rootRect.Left
            RelativeY = $point.Y - $rootRect.Top
            ChildHandle = ('0x{0:X8}' -f [int]$childHandle)
            ChildClass = Get-ClassNameValue -Handle $childHandle
            ChildControlId = [NativeMethods]::GetDlgCtrlID($childHandle)
            RootHandle = ('0x{0:X8}' -f [int]$rootHandle)
            RootTitle = $rootTitle
        }
    }

    throw "Failed to capture '$Prompt' after $MaxAttempts attempts."
}

function Read-RowHeightAnchor {
    param(
        [Parameter(Mandatory)]
        [pscustomobject]$FirstRowAnchor,

        [Parameter(Mandatory)]
        [string]$ExpectedWindowTitle
    )

    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $secondRowAnchor = Read-Anchor -Prompt '3. Second row selector area (same far-left area on row 2)' -ExpectedWindowTitle $ExpectedWindowTitle
        if ($secondRowAnchor.RelativeY -gt $FirstRowAnchor.RelativeY) {
            return $secondRowAnchor
        }

        Write-Warning 'The second-row selector was not captured below the first-row selector.'
        if ($attempt -lt $MaxAttempts) {
            Write-Host 'Please try the second-row capture again.' -ForegroundColor Yellow
        }
    }

    throw 'Failed to capture a valid second-row selector below the first-row selector.'
}

Write-Host 'Purchase Receipt Entry layout discovery' -ForegroundColor Green
Write-Host "Target window: $WindowTitle"
Write-Host "Config output: $ConfigPath"

$purchaseOrderField = Read-Anchor -Prompt '1. Purchase Order field' -ExpectedWindowTitle $WindowTitle
$firstRowSelector = Read-Anchor -Prompt '2. First row selector area (far-left row cell area)' -ExpectedWindowTitle $WindowTitle
$secondRowSelector = Read-RowHeightAnchor -FirstRowAnchor $firstRowSelector -ExpectedWindowTitle $WindowTitle
$firstRowQuantityCell = Read-Anchor -Prompt '4. First row Quantity Received cell' -ExpectedWindowTitle $WindowTitle
$firstRowLocationCell = Read-Anchor -Prompt '5. First row Location cell' -ExpectedWindowTitle $WindowTitle

$rowHeight = [int]($secondRowSelector.RelativeY - $firstRowSelector.RelativeY)

$config = [pscustomobject]@{
    WindowTitle = $WindowTitle
    DiscoveredAt = (Get-Date).ToString('o')
    RowHeight = $rowHeight
    Anchors = [pscustomobject]@{
        PurchaseOrderField = $purchaseOrderField
        FirstRowSelector = $firstRowSelector
        SecondRowSelector = $secondRowSelector
        FirstRowQuantityCell = $firstRowQuantityCell
        FirstRowLocationCell = $firstRowLocationCell
    }
}

$config | ConvertTo-Json -Depth 6 | Set-Content -Path $ConfigPath -Encoding UTF8

Write-Host ''
Write-Host 'Discovery complete.' -ForegroundColor Green
Write-Host "Saved layout config to $ConfigPath"
Write-Host "RowHeight = $rowHeight"

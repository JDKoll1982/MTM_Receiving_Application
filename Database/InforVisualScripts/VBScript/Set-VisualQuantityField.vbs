Option Explicit

Const DefaultEditHandleHex = "00B00A2C"
Const DefaultParentHandleHex = "007909FC"
Const DefaultProcessName = "VMRCVENT.EXE"
Const DefaultWindowTitle = "Purchase Receipt Entry - Infor VISUAL - MTMFG"
Const DefaultTargetControlId = "32791"
Const DefaultValue = "2000"

Dim editHandleHex
Dim parentHandleHex
Dim processName
Dim windowTitle
Dim targetControlId
Dim quantityValue
Dim shell
Dim fileSystem
Dim environment
Dim tempScriptPath
Dim tempScript
Dim command
Dim exitCode

Set shell = CreateObject("WScript.Shell")
Set fileSystem = CreateObject("Scripting.FileSystemObject")
Set environment = shell.Environment("Process")

editHandleHex = ReadSetting(environment, "VISUAL_EDIT_HANDLE", DefaultEditHandleHex)
parentHandleHex = ReadSetting(environment, "VISUAL_PARENT_HANDLE", DefaultParentHandleHex)
processName = ReadSetting(environment, "VISUAL_PROCESS_NAME", DefaultProcessName)
windowTitle = ReadSetting(environment, "VISUAL_WINDOW_TITLE", DefaultWindowTitle)
targetControlId = ReadSetting(environment, "VISUAL_TARGET_CONTROL_ID", DefaultTargetControlId)
quantityValue = ReadSetting(environment, "VISUAL_QUANTITY", DefaultValue)

tempScriptPath = shell.ExpandEnvironmentStrings("%TEMP%") & "\Set-VisualQuantityField.ps1"
Set tempScript = fileSystem.CreateTextFile(tempScriptPath, True)

' VBScript cannot call WM_SETTEXT directly, so it delegates the handle-based window message to PowerShell.
tempScript.WriteLine "$ErrorActionPreference = 'Stop'"
tempScript.WriteLine "Add-Type -TypeDefinition @'"
tempScript.WriteLine "using System;"
tempScript.WriteLine "using System.Runtime.InteropServices;"
tempScript.WriteLine "public static class NativeMethods"
tempScript.WriteLine "{"
tempScript.WriteLine "    [DllImport(""user32.dll"", SetLastError = true)]"
tempScript.WriteLine "    public static extern bool IsWindow(IntPtr hWnd);"
tempScript.WriteLine ""
tempScript.WriteLine "    [DllImport(""user32.dll"", SetLastError = true)]"
tempScript.WriteLine "    public static extern bool SetForegroundWindow(IntPtr hWnd);"
tempScript.WriteLine ""
tempScript.WriteLine "    [DllImport(""user32.dll"", CharSet = CharSet.Unicode, SetLastError = true)]"
tempScript.WriteLine "    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);"
tempScript.WriteLine ""
tempScript.WriteLine "    [DllImport(""user32.dll"", SetLastError = true)]"
tempScript.WriteLine "    public static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);"
tempScript.WriteLine ""
tempScript.WriteLine "    public delegate bool EnumChildProc(IntPtr hwnd, IntPtr lParam);"
tempScript.WriteLine ""
tempScript.WriteLine "    [DllImport(""user32.dll"", SetLastError = true)]"
tempScript.WriteLine "    public static extern int GetDlgCtrlID(IntPtr hwnd);"
tempScript.WriteLine ""
tempScript.WriteLine "    [DllImport(""user32.dll"", CharSet = CharSet.Unicode, SetLastError = true)]"
tempScript.WriteLine "    public static extern int GetClassName(IntPtr hwnd, System.Text.StringBuilder className, int maxCount);"
tempScript.WriteLine ""
tempScript.WriteLine "    [DllImport(""user32.dll"", SetLastError = true)]"
tempScript.WriteLine "    public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);"
tempScript.WriteLine ""
tempScript.WriteLine "    [DllImport(""user32.dll"", CharSet = CharSet.Unicode, SetLastError = true)]"
tempScript.WriteLine "    public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, string lParam, uint flags, uint timeout, out IntPtr result);"
tempScript.WriteLine ""
tempScript.WriteLine "    [StructLayout(LayoutKind.Sequential)]"
tempScript.WriteLine "    public struct RECT"
tempScript.WriteLine "    {"
tempScript.WriteLine "        public int Left;"
tempScript.WriteLine "        public int Top;"
tempScript.WriteLine "        public int Right;"
tempScript.WriteLine "        public int Bottom;"
tempScript.WriteLine "    }"
tempScript.WriteLine "}"
tempScript.WriteLine "'@"
tempScript.WriteLine ""
tempScript.WriteLine "$editHandle = [IntPtr]::new([Convert]::ToInt64($args[0], 16))"
tempScript.WriteLine "$quantity = $args[1]"
tempScript.WriteLine "$parentHandle = [IntPtr]::new([Convert]::ToInt64($args[2], 16))"
tempScript.WriteLine "$processName = $args[3]"
tempScript.WriteLine "$windowTitle = $args[4]"
tempScript.WriteLine "$targetControlId = [int]$args[5]"
tempScript.WriteLine "$wmSetText = 0x000C"
tempScript.WriteLine "$smtoAbortIfHung = 0x0002"
tempScript.WriteLine "$result = [IntPtr]::Zero"
tempScript.WriteLine "$fallbackWroteValue = $false"
tempScript.WriteLine "$discoveredHandle = [IntPtr]::Zero"
tempScript.WriteLine ""
tempScript.WriteLine "if (-not [NativeMethods]::IsWindow($editHandle)) {"
tempScript.WriteLine "    $searchRoot = [NativeMethods]::FindWindow($null, $windowTitle)"
tempScript.WriteLine "    if ($searchRoot -eq [IntPtr]::Zero) {"
tempScript.WriteLine "        $searchRoot = $parentHandle"
tempScript.WriteLine "    }"
tempScript.WriteLine ""
tempScript.WriteLine "    if ([NativeMethods]::IsWindow($searchRoot)) {"
tempScript.WriteLine "        $callback = [NativeMethods+EnumChildProc]{"
tempScript.WriteLine "            param([IntPtr]$hwnd, [IntPtr]$lParam)"
tempScript.WriteLine "            $className = [System.Text.StringBuilder]::new(256)"
tempScript.WriteLine "            $rect = New-Object NativeMethods+RECT"
tempScript.WriteLine "            [void][NativeMethods]::GetClassName($hwnd, $className, $className.Capacity)"
tempScript.WriteLine "            [void][NativeMethods]::GetWindowRect($hwnd, [ref]$rect)"
tempScript.WriteLine "            if ([NativeMethods]::GetDlgCtrlID($hwnd) -eq $targetControlId -and $className.ToString() -eq 'Edit' -and $rect.Left -ge 0 -and $rect.Top -ge 0) {"
tempScript.WriteLine "                $script:discoveredHandle = $hwnd"
tempScript.WriteLine "                return $false"
tempScript.WriteLine "            }"
tempScript.WriteLine "            return $true"
tempScript.WriteLine "        }"
tempScript.WriteLine "        [void][NativeMethods]::EnumChildWindows($searchRoot, $callback, [IntPtr]::Zero)"
tempScript.WriteLine "        if ($script:discoveredHandle -ne [IntPtr]::Zero) {"
tempScript.WriteLine "            $editHandle = $script:discoveredHandle"
tempScript.WriteLine "        }"
tempScript.WriteLine "    }"
tempScript.WriteLine "}"
tempScript.WriteLine ""
tempScript.WriteLine "if ([NativeMethods]::IsWindow($editHandle)) {"
tempScript.WriteLine "    if ([NativeMethods]::IsWindow($parentHandle)) {"
tempScript.WriteLine "        [void][NativeMethods]::SetForegroundWindow($parentHandle)"
tempScript.WriteLine "    }"
tempScript.WriteLine ""
tempScript.WriteLine "    $sendResult = [NativeMethods]::SendMessageTimeout($editHandle, $wmSetText, [IntPtr]::Zero, $quantity, $smtoAbortIfHung, 2000, [ref]$result)"
tempScript.WriteLine "    if ($sendResult -ne [IntPtr]::Zero) {"
tempScript.WriteLine "        Write-Host ""Set quantity '$quantity' on handle 0x$($args[0])."""
tempScript.WriteLine "        exit 0"
tempScript.WriteLine "    }"
tempScript.WriteLine "}"
tempScript.WriteLine ""
tempScript.WriteLine "$visualProcess = Get-Process | Where-Object { $_.ProcessName -ieq [System.IO.Path]::GetFileNameWithoutExtension($processName) } | Select-Object -First 1"
tempScript.WriteLine "if ($null -eq $visualProcess) {"
tempScript.WriteLine "    throw ""Edit handle 0x$($args[0]) is stale and process '$processName' is not running for SendKeys fallback."""
tempScript.WriteLine "}"
tempScript.WriteLine ""
tempScript.WriteLine "$wshell = New-Object -ComObject WScript.Shell"
tempScript.WriteLine "if (-not $wshell.AppActivate([int]$visualProcess.Id)) {"
tempScript.WriteLine "    throw ""Edit handle 0x$($args[0]) is stale and could not activate process '$processName'."""
tempScript.WriteLine "}"
tempScript.WriteLine ""
tempScript.WriteLine "Start-Sleep -Milliseconds 200"
tempScript.WriteLine "$wshell.SendKeys('^a')"
tempScript.WriteLine "Start-Sleep -Milliseconds 100"
tempScript.WriteLine "$wshell.SendKeys($quantity)"
tempScript.WriteLine "$fallbackWroteValue = $true"
tempScript.WriteLine ""
tempScript.WriteLine "if ($fallbackWroteValue) {"
tempScript.WriteLine "    Write-Host ""Set quantity '$quantity' using SendKeys fallback for process '$processName'."""
tempScript.WriteLine "}"

tempScript.Close

command = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File """ & tempScriptPath & """ """ & editHandleHex & """ """ & quantityValue & """ """ & parentHandleHex & """ """ & processName & """ """ & windowTitle & """ """ & targetControlId & """"
exitCode = shell.Run(command, 0, True)

On Error Resume Next
fileSystem.DeleteFile tempScriptPath, True
On Error GoTo 0

If exitCode <> 0 Then
    Err.Raise vbObjectError + exitCode, "Set-VisualQuantityField.vbs", "Failed to set the quantity field. Exit code: " & exitCode
End If

Function ReadSetting(processEnvironment, variableName, fallbackValue)
    Dim rawValue

    rawValue = Trim(processEnvironment(variableName))
    If Len(rawValue) = 0 Then
        ReadSetting = fallbackValue
        Exit Function
    End If

    ReadSetting = rawValue
End Function
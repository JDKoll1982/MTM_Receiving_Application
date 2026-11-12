<#
.SYNOPSIS
    Stops the shared Serena MCP server started by start-serena-http.ps1.

.PARAMETER Port
    MCP listen port. Must match the URL in .vscode/mcp.json.
#>
[CmdletBinding()]
param(
    [int]$Port = 24300
)

$ErrorActionPreference = 'Stop'

$connection = Get-NetTCPConnection -State Listen -LocalPort $Port -ErrorAction SilentlyContinue |
    Select-Object -First 1

if (-not $connection) {
    Write-Host "No Serena MCP server is listening on port $Port."
    exit 0
}

$process = Get-CimInstance Win32_Process -Filter "ProcessId=$($connection.OwningProcess)" -ErrorAction SilentlyContinue
if (-not $process -or $process.CommandLine -notlike '*serena*') {
    Write-Warning "Port $Port is in use by a non-Serena process (PID $($connection.OwningProcess)) - not stopping it."
    exit 1
}

Stop-Process -Id $connection.OwningProcess -Force
Write-Host "Stopped Serena MCP server on port $Port (PID $($connection.OwningProcess))."

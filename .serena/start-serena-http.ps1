<#
.SYNOPSIS
    Ensures exactly one shared Serena MCP server is running for this workspace.

.DESCRIPTION
    Idempotent launcher used by the ".vscode/mcp.json" HTTP transport and the
    "Serena: ensure MCP server is running" folder-open task.

    If the configured port is already served by a Serena process, the existing
    instance is reused and nothing is started. Otherwise Serena is launched with
    the streamable-http transport so every future chat connects to the same
    process instead of spawning a new one.

.PARAMETER Port
    MCP listen port. Must match the URL in .vscode/mcp.json.

.PARAMETER ForceRestart
    Stop the process currently listening on the port, then start a fresh server.
#>
[CmdletBinding()]
param(
    [int]$Port = 24300,
    [string]$ProjectPath,
    [switch]$ForceRestart
)

$ErrorActionPreference = 'Stop'

if (-not $ProjectPath) {
    $ProjectPath = Split-Path -Parent $PSScriptRoot
}
if (-not $ProjectPath) {
    $ProjectPath = (Get-Location).Path
}

$logDirectory = Join-Path $PSScriptRoot 'logs'
$stdoutLog = Join-Path $logDirectory 'mcp-server.log'
$stderrLog = Join-Path $logDirectory 'mcp-server.err.log'

function Get-ListeningProcessId {
    param([int]$TargetPort)

    $connection = Get-NetTCPConnection -State Listen -LocalPort $TargetPort -ErrorAction SilentlyContinue |
        Select-Object -First 1

    if ($connection) { return $connection.OwningProcess }
    return $null
}

function Test-IsSerenaProcess {
    param([int]$ProcessId)

    $process = Get-CimInstance Win32_Process -Filter "ProcessId=$ProcessId" -ErrorAction SilentlyContinue
    if (-not $process) { return $false }
    return ($process.CommandLine -like '*serena*')
}

function Test-PortAcceptsConnections {
    param([int]$TargetPort)

    $client = New-Object System.Net.Sockets.TcpClient
    try {
        $client.Connect('127.0.0.1', $TargetPort)
        return $client.Connected
    }
    catch { return $false }
    finally { $client.Dispose() }
}

$existingProcessId = Get-ListeningProcessId -TargetPort $Port

if ($existingProcessId -and $ForceRestart) {
    Write-Host "Stopping Serena MCP server (PID $existingProcessId) on port $Port..."
    Stop-Process -Id $existingProcessId -Force -ErrorAction SilentlyContinue
    $deadline = (Get-Date).AddSeconds(15)
    while ((Get-Date) -lt $deadline -and (Get-ListeningProcessId -TargetPort $Port)) {
        Start-Sleep -Milliseconds 250
    }
    $existingProcessId = Get-ListeningProcessId -TargetPort $Port
}

if ($existingProcessId) {
    if (Test-IsSerenaProcess -ProcessId $existingProcessId) {
        Write-Host "Serena MCP server already running on port $Port (PID $existingProcessId) - reusing it."
        exit 0
    }
    Write-Warning "Port $Port is in use by a non-Serena process (PID $existingProcessId). Use -ForceRestart or change -Port."
    exit 1
}

New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null

$serenaArguments = @(
    'start-mcp-server',
    '--transport', 'streamable-http',
    '--port', "$Port",
    '--context', 'ide',
    '--project', $ProjectPath,
    '--open-web-dashboard', 'false'
)

# Launch through uvx from Git so the same Serena build is used as the previous
# stdio configuration. A globally installed 'serena' may be an older release
# that cannot read this workspace's .serena/project.yml schema.
$uvxCommand = Get-Command uvx -ErrorAction SilentlyContinue
if (-not $uvxCommand) {
    Write-Error "uvx was not found on PATH. Install uv (winget install astral-sh.uv) or provide a compatible 'serena' executable."
    exit 1
}

$executable = $uvxCommand.Source
$launchArguments = @('--from', 'git+https://github.com/oraios/serena', 'serena') + $serenaArguments

Write-Host "Starting Serena MCP server on port $Port..."
Start-Process -FilePath $executable `
    -ArgumentList $launchArguments `
    -WorkingDirectory $ProjectPath `
    -WindowStyle Hidden `
    -RedirectStandardOutput $stdoutLog `
    -RedirectStandardError $stderrLog | Out-Null

$startDeadline = (Get-Date).AddSeconds(120)
while ((Get-Date) -lt $startDeadline) {
    if (Test-PortAcceptsConnections -TargetPort $Port) {
        Write-Host "Serena MCP server ready at http://127.0.0.1:$Port/mcp"
        exit 0
    }
    Start-Sleep -Milliseconds 500
}

Write-Warning "Serena MCP server did not open port $Port within 120s. See $stderrLog."
exit 1

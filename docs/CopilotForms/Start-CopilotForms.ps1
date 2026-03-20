<#
.SYNOPSIS
    Starts a local web server for CopilotForms and opens the index page.
.DESCRIPTION
    Serves files from docs/CopilotForms over http://localhost so the browser can
    load the shared JSON configuration without using the manual local-config fallback.
    The script keeps running until you stop it with Ctrl+C.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateRange(1024, 65535)]
    [int]$Port = 8421,

    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]$RootPath = $PSScriptRoot,

    [Parameter()]
    [switch]$NoBrowser
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-CopilotFormsContentType {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    $extension = [System.IO.Path]::GetExtension($Path).ToLowerInvariant()
    switch ($extension) {
        '.html' { return 'text/html; charset=utf-8' }
        '.css' { return 'text/css; charset=utf-8' }
        '.js' { return 'application/javascript; charset=utf-8' }
        '.json' { return 'application/json; charset=utf-8' }
        '.md' { return 'text/markdown; charset=utf-8' }
        '.svg' { return 'image/svg+xml' }
        '.png' { return 'image/png' }
        '.jpg' { return 'image/jpeg' }
        '.jpeg' { return 'image/jpeg' }
        '.ico' { return 'image/x-icon' }
        default { return 'application/octet-stream' }
    }
}

function Get-CopilotFormsRequestPath {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [System.Uri]$Url
    )

    $rawPath = [System.Uri]::UnescapeDataString($Url.AbsolutePath.TrimStart('/'))
    if ([string]::IsNullOrWhiteSpace($rawPath)) {
        return 'index.html'
    }

    return $rawPath.Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

$resolvedRootCandidate = [System.IO.Path]::GetFullPath($RootPath)
if (-not (Test-Path -Path $resolvedRootCandidate -PathType Container)) {
    throw "CopilotForms root path not found: $resolvedRootCandidate"
}

$resolvedRoot = (Resolve-Path -Path $resolvedRootCandidate).Path

$listener = [System.Net.HttpListener]::new()
$prefix = "http://localhost:$Port/"
$listener.Prefixes.Add($prefix)

Write-Host "Starting CopilotForms server on port $Port..." -ForegroundColor Yellow

try {
    $listener.Start()
}
catch {
    throw "Unable to start local server on $prefix. Choose a different port with -Port. $($_.Exception.Message)"
}

$indexUrl = "$prefix`index.html"

Write-Host "LISTENING on $prefix" -ForegroundColor Green
Write-Host ''
Write-Host 'CopilotForms local server started.' -ForegroundColor Green
Write-Host "Root:  $resolvedRoot" -ForegroundColor Cyan
Write-Host "URL:   $indexUrl" -ForegroundColor Cyan
Write-Host 'Stop:  Press Ctrl+C' -ForegroundColor Yellow
Write-Host ''

if (-not $NoBrowser.IsPresent) {
    Start-Process $indexUrl | Out-Null
}

try {
    while ($listener.IsListening) {
        $context = $listener.GetContext()
        $response = $context.Response

        try {
            $requestPath = Get-CopilotFormsRequestPath -Url $context.Request.Url
            $candidatePath = [System.IO.Path]::GetFullPath((Join-Path $resolvedRoot $requestPath))

            if (-not $candidatePath.StartsWith($resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
                $response.StatusCode = 403
                $buffer = [System.Text.Encoding]::UTF8.GetBytes('Forbidden')
                $response.OutputStream.Write($buffer, 0, $buffer.Length)
                continue
            }

            if ((Test-Path -Path $candidatePath -PathType Container)) {
                $candidatePath = Join-Path $candidatePath 'index.html'
            }

            if (-not (Test-Path -Path $candidatePath -PathType Leaf)) {
                $response.StatusCode = 404
                $buffer = [System.Text.Encoding]::UTF8.GetBytes('Not Found')
                $response.OutputStream.Write($buffer, 0, $buffer.Length)
                continue
            }

            $bytes = [System.IO.File]::ReadAllBytes($candidatePath)
            $response.ContentType = Get-CopilotFormsContentType -Path $candidatePath
            $response.ContentLength64 = $bytes.Length
            $response.StatusCode = 200
            $response.OutputStream.Write($bytes, 0, $bytes.Length)
        }
        catch {
            $response.StatusCode = 500
            $buffer = [System.Text.Encoding]::UTF8.GetBytes("Server Error: $($_.Exception.Message)")
            $response.OutputStream.Write($buffer, 0, $buffer.Length)
        }
        finally {
            $response.OutputStream.Close()
        }
    }
}
finally {
    if ($listener.IsListening) {
        $listener.Stop()
    }
    $listener.Close()
}
param(
    [Parameter(Mandatory = $true)][string]$Sql,
    [int]$Timeout = 120,
    [string]$Server = 'VISUAL',
    [string]$Database = 'MTMFG',
    [string]$User = $env:MTMFG_USER,
    [string]$Password = $env:MTMFG_PASSWORD
)

if (-not $User) { $User = Read-Host 'MTMFG SQL login' }
if (-not $Password) {
    $secure = Read-Host 'MTMFG SQL password' -AsSecureString
    $Password = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    )
}

$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
$builder['Server'] = $Server
$builder['Database'] = $Database
$builder['User ID'] = $User
$builder['Password'] = $Password
$builder['TrustServerCertificate'] = $true
$builder['ApplicationIntent'] = 'ReadOnly'
$builder['Connect Timeout'] = 20

$conn = New-Object System.Data.SqlClient.SqlConnection $builder.ConnectionString
$conn.Open()
try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $Sql
    $cmd.CommandTimeout = $Timeout
    $dt = New-Object System.Data.DataTable
    $dt.Load($cmd.ExecuteReader())

    Write-Output ("ROWS=" + $dt.Rows.Count)
    if ($dt.Rows.Count -eq 0) { return }

    $cols = @($dt.Columns | ForEach-Object { $_.ColumnName })
    $widths = @()
    foreach ($c in $cols) {
        $max = $c.Length
        foreach ($row in $dt.Rows) {
            $v = [string]$row[$c]
            if ($v.Length -gt $max) { $max = $v.Length }
        }
        if ($max -gt 40) { $max = 40 }
        $widths += $max
    }

    $header = ''
    for ($i = 0; $i -lt $cols.Count; $i++) { $header += $cols[$i].PadRight($widths[$i] + 2) }
    Write-Output $header
    Write-Output ('-' * [Math]::Min(200, $header.Length))

    foreach ($row in $dt.Rows) {
        $line = ''
        for ($i = 0; $i -lt $cols.Count; $i++) {
            $v = [string]$row[$cols[$i]]
            if ($v.Length -gt $widths[$i]) { $v = $v.Substring(0, $widths[$i]) }
            $line += $v.PadRight($widths[$i] + 2)
        }
        Write-Output $line
    }
}
finally {
    $conn.Close()
    $conn.Dispose()
}

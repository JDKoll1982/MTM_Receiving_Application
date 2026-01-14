$connString = "Server=VISUAL;Database=MTMFG;User Id=SHOP2;Password=SHOP;TrustServerCertificate=True;ApplicationIntent=ReadOnly;"

try {
    $conn = New-Object System.Data.SqlClient.SqlConnection($connString)
    $conn.Open()
    Write-Host "Connected to database."
    
    $poId = "PO-067354"
    $woId = "WO-068285"

    # 1. Inspect PURC_ORDER_LINE
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT * FROM PURC_ORDER_LINE WHERE PURC_ORDER_ID = '$poId'"
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $dtPO = New-Object System.Data.DataTable
    $adapter.Fill($dtPO) | Out-Null
    
    Write-Host "`n--- PURC_ORDER_LINE (First 5 columns + relevance) ---"
    if ($dtPO.Rows.Count -gt 0) {
        $dtPO | Select-Object PURC_ORDER_ID, LINE_NO, PART_ID, GL_EXPENSE_ACCT_ID, PROJ_REF_SUB_ID, WORK_ORDER_ID | Format-Table -AutoSize
    } else {
        Write-Host "No lines found for PO $poId"
    }

    # 2. Inspect DEMAND_SUPPLY_LINK
    $cmd.CommandText = "SELECT * FROM DEMAND_SUPPLY_LINK WHERE SUPPLY_BASE_ID LIKE '%$poId%'"
    $adapter.SelectCommand = $cmd
    $dtLink = New-Object System.Data.DataTable
    $adapter.Fill($dtLink) | Out-Null

    Write-Host "`n--- DEMAND_SUPPLY_LINK ---"
    if ($dtLink.Rows.Count -gt 0) {
        $row = $dtLink.Rows[0]
        Write-Host "SUPPLY_NO Type: $($row['SUPPLY_NO'].GetType().Name)"
        Write-Host "SUPPLY_NO Value: '$($row['SUPPLY_NO'])'"
        $dtLink | Select-Object SUPPLY_BASE_ID, SUPPLY_NO, DEMAND_BASE_ID, SUPPLY_TYPE, DEMAND_TYPE, QTY | Format-Table -AutoSize
    } else {
        Write-Host "No links found for Supply Base ID matching $poId"
    }

    # 3. Inspect WORK_ORDER
    $cmd.CommandText = "SELECT * FROM WORK_ORDER WHERE BASE_ID LIKE '%$woId%'"
    $adapter.SelectCommand = $cmd
    $dtWO = New-Object System.Data.DataTable
    $adapter.Fill($dtWO) | Out-Null
    
    Write-Host "`n--- WORK_ORDER ---"
    if ($dtWO.Rows.Count -gt 0) {
        $dtWO | Select-Object BASE_ID, ID, PART_ID, STATUS | Format-Table -AutoSize
    } else {
        Write-Host "No WO found for Base ID matching $woId"
    }
    
    # Check for direct link in PURC_ORDER_LINE ?
    # Check if PROJ_REF_SUB_ID in PURC_ORDER_LINE matches correct WO ID
    
} catch {
    Write-Error $_.Exception.Message
} finally {
    if ($conn) { $conn.Close() }
}

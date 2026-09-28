# Generates new Steps 2 (parts) and 3 (custom fields) for 03_seed_dunnage_data.sql
# by converting JSON spec_values to udc1..udc10 values and dunnage_specs to custom fields.
# Reads the existing seed, transforms, writes a new file.
param(
    [string]$SeedPath = 'C:\Users\jkoll\source\repos\MTM_Receiving_Application\Database\Database_Deployment\Sql_Files\SeedData\03_seed_dunnage_data.sql',
    [string]$OutPath   = 'C:\Users\jkoll\source\repos\MTM_Receiving_Application\Database\Database_Deployment\Sql_Files\SeedData\03_seed_dunnage_data.udc.sql'
)

$src = Get-Content -Path $SeedPath -Raw -Encoding UTF8

# ---- type -> ordered field keys (from the original dunnage_specs section) ----
$typeFields = @{
    '@t1'  = @('dimensions', 'customer')
    '@t2'  = @('dimensions', 'customer')
    '@t3'  = @('length', 'width', 'height', 'wall_type', 'customer', 'part_number')
    '@t4'  = @('height_type')
    '@t5'  = @('application')
    '@t6'  = @('length', 'width', 'mil', 'style')
    '@t7'  = @('width', 'material', 'style')
    '@t8'  = @('width', 'depth', 'length')
    '@t9'  = @('type', 'variant', 'part_family', 'piece')
    '@t10' = @('rack_number', 'customer')
    '@t11' = @('customer', 'owner', 'style')
    '@t12' = @('length', 'width', 'height', 'customer')
    '@t13' = @('customer', 'height_type', 'style')
}

# ---- type -> field definition (FieldType, Unit, choices) in the same order ----
$typeDefs = @{
    '@t1'  = @(@{ Key='dimensions'; Type='Text'; Unit='' }, @{ Key='customer'; Type='Text'; Unit='' })
    '@t2'  = @(@{ Key='dimensions'; Type='Text'; Unit='' }, @{ Key='customer'; Type='Text'; Unit='' })
    '@t3'  = @(
        @{ Key='length'; Type='Number'; Unit='in' }, @{ Key='width'; Type='Number'; Unit='in' },
        @{ Key='height'; Type='Number'; Unit='in' }, @{ Key='wall_type'; Type='Choices'; Unit=''; Choices=@('Single Wall','Double Wall') },
        @{ Key='customer'; Type='Text'; Unit='' }, @{ Key='part_number'; Type='Text'; Unit='' })
    '@t4'  = @(@{ Key='height_type'; Type='Choices'; Unit=''; Choices=@('Short','Tall') })
    '@t5'  = @(@{ Key='application'; Type='Choices'; Unit=''; Choices=@('Hand-Held','Auto-Wrapper') })
    '@t6'  = @(
        @{ Key='length'; Type='Number'; Unit='in' }, @{ Key='width'; Type='Number'; Unit='in' },
        @{ Key='mil'; Type='Number'; Unit='' }, @{ Key='style'; Type='Choices'; Unit=''; Choices=@('Lay Flat','Gaylord') })
    '@t7'  = @(
        @{ Key='width'; Type='Text'; Unit='' }, @{ Key='material'; Type='Choices'; Unit=''; Choices=@('Tape','Steel','Nylon') },
        @{ Key='style'; Type='Choices'; Unit=''; Choices=@('Banding','Strapping') })
    '@t8'  = @(
        @{ Key='width'; Type='Number'; Unit='in' }, @{ Key='depth'; Type='Number'; Unit='in' },
        @{ Key='length'; Type='Number'; Unit='in' })
    '@t9'  = @(
        @{ Key='type'; Type='Text'; Unit='' }, @{ Key='variant'; Type='Number'; Unit='' },
        @{ Key='part_family'; Type='Text'; Unit='' }, @{ Key='piece'; Type='Choices'; Unit=''; Choices=@('Top / Cover','Bottom / Box') })
    '@t10' = @(@{ Key='rack_number'; Type='Text'; Unit='' }, @{ Key='customer'; Type='Text'; Unit='' })
    '@t11' = @(
        @{ Key='customer'; Type='Text'; Unit='' }, @{ Key='owner'; Type='Choices'; Unit=''; Choices=@('Customer','MTM') },
        @{ Key='style'; Type='Text'; Unit='' })
    '@t12' = @(
        @{ Key='length'; Type='Number'; Unit='in' }, @{ Key='width'; Type='Number'; Unit='in' },
        @{ Key='height'; Type='Number'; Unit='in' }, @{ Key='customer'; Type='Text'; Unit='' })
    '@t13' = @(
        @{ Key='customer'; Type='Text'; Unit='' }, @{ Key='height_type'; Type='Choices'; Unit=''; Choices=@('Short','Tall','Half') },
        @{ Key='style'; Type='Text'; Unit='' })
}

# display name from a raw spec key
function Get-DisplayName([string]$key) {
    switch ($key.ToLowerInvariant()) {
        'dimensions' { 'Dimensions' } 'customer' { 'Customer' } 'length' { 'Length' }
        'width' { 'Width' } 'height' { 'Height' } 'wall_type' { 'Wall Type' }
        'part_number' { 'Part Number' } 'height_type' { 'Height Type' } 'application' { 'Application' }
        'mil' { 'Mil' } 'style' { 'Style' } 'material' { 'Material' } 'depth' { 'Depth' }
        'type' { 'Type' } 'variant' { 'Variant' } 'part_family' { 'Part Family' } 'piece' { 'Piece' }
        'rack_number' { 'Rack Number' } 'owner' { 'Owner' }
        default { (Get-Culture).TextInfo.ToTitleCase($key.ToLowerInvariant()) }
    }
}

function Convert-SqlLiteral([string]$v) {
    if ($null -eq $v -or $v -eq '') { return 'NULL' }
    return "'" + ($v -replace "'", "''") + "'"
}

# ---- Step 2: regenerate parts ----
$partRegex = [regex]"(?s)CALL sp_Dunnage_Parts_Insert\(\s*'([^']*)',\s*(@t\d+),\s*'([^']*)',\s*([^,]+),\s*'([^']*)',\s*([^,]+),\s*'([^']*)',\s*@id\s*\)"
$step2 = New-Object System.Text.StringBuilder
$step2.AppendLine('--   Step 2 - dunnage_parts (UDC values mapped from spec_values by type field order)') | Out-Null

foreach ($m in $partRegex.Matches($src)) {
    $partId = $m.Groups[1].Value
    $typeVar = $m.Groups[2].Value
    $specJson = $m.Groups[3].Value
    $image = $m.Groups[4].Value.Trim()
    $qty = $m.Groups[5].Value
    $loc = $m.Groups[6].Value.Trim()
    $user = $m.Groups[7].Value

    $fields = $typeFields[$typeVar]
    $dict = @{}
    if ($specJson -and $specJson -ne '{}') {
        try { $dict = $specJson | ConvertFrom-Json -ErrorAction Stop | ForEach-Object { $_.PSObject.Properties } | ForEach-Object { @{ $_.Name = $_.Value } } }
        catch { $dict = @{} }
    }
    # ConvertFrom-Json returns a PSCustomObject; flatten to hashtable
    $ht = @{}
    foreach ($k in $dict.Keys) { $ht[$k] = $dict[$k] }

    $udcValues = for ($i = 1; $i -le 10; $i++) {
        if ($i -le $fields.Count) {
            $key = $fields[$i - 1]
            if ($ht.ContainsKey($key)) { Convert-SqlLiteral ([string]$ht[$key]) } else { 'NULL' }
        } else { 'NULL' }
    }

    $step2.AppendLine('CALL sp_Dunnage_Parts_Insert(') | Out-Null
    $step2.AppendLine("    $((Convert-SqlLiteral $partId)),") | Out-Null
    $step2.AppendLine("    $typeVar,") | Out-Null
    foreach ($u in $udcValues) { $step2.AppendLine("    $u,") | Out-Null }
    $step2.AppendLine("    $image,") | Out-Null
    $step2.AppendLine("    $((Convert-SqlLiteral $qty)),") | Out-Null
    $step2.AppendLine("    $loc,") | Out-Null
    $step2.AppendLine("    $((Convert-SqlLiteral $user)),") | Out-Null
    $step2.AppendLine('    @id') | Out-Null
    $step2.AppendLine(');') | Out-Null
    $step2.AppendLine('') | Out-Null
}

# ---- Step 3: custom fields + choices ----
$step3 = New-Object System.Text.StringBuilder
$step3.AppendLine('--   Step 3 - dunnage_custom_fields + dunnage_custom_field_choices (UDC definitions)') | Out-Null
foreach ($typeVar in @('@t1','@t2','@t3','@t4','@t5','@t6','@t7','@t8','@t9','@t10','@t11','@t12','@t13')) {
    if (-not $typeDefs.ContainsKey($typeVar)) { continue }
    $slot = 1
    foreach ($def in $typeDefs[$typeVar]) {
        $name = Get-DisplayName $def.Key
        $step3.AppendLine("CALL sp_Dunnage_CustomFields_Insert($typeVar, $(Convert-SqlLiteral $name), $(Convert-SqlLiteral $def.Type), $slot, 0, $(Convert-SqlLiteral $def.Unit), NULL, NULL, NULL, NULL, 'seed', @cfid, @cfs, @cfm);") | Out-Null
        $order = 1
        foreach ($choice in $def.Choices) {
            $step3.AppendLine("CALL sp_Dunnage_CustomFieldChoices_Insert(@cfid, $(Convert-SqlLiteral $choice), $order);") | Out-Null
            $order++
        }
        $slot++
    }
}

# ---- splice: keep header (through Step 1 end) + new Step 2 + new Step 3 + Steps 4-5 ----
$headerEnd = $src.IndexOf('CALL sp_Dunnage_Parts_Insert(')
# locate the body Step 4 section (single-space marker, not the header's indented Step 4 line)
$step4Start = $src.IndexOf("`r`n-- Step 4")
if ($step4Start -lt 0) { $step4Start = $src.IndexOf("`n-- Step 4") }
if ($headerEnd -lt 0 -or $step4Start -lt 0) { throw 'Could not locate seed section boundaries' }

$header = $src.Substring(0, $headerEnd)
$tail = $src.Substring($step4Start)

# update the header's Insert list to reflect custom fields instead of dunnage_specs
$header = $header -replace 'Step 3 - dunnage_specs \(per-type spec templates derived from part spec_values\)', 'Step 3 - dunnage_custom_fields + dunnage_custom_field_choices (UDC definitions)'
$header = $header -replace 'Step 3 \— dunnage_specs \(per-type spec templates derived from part spec_values\)', 'Step 3 \— dunnage_custom_fields + dunnage_custom_field_choices (UDC definitions)'

$newContent = $header + $step2.ToString() + $step3.ToString() + $tail

Set-Content -Path $OutPath -Value $newContent -Encoding UTF8
Write-Output "Wrote $OutPath ($($newContent.Length) chars)"

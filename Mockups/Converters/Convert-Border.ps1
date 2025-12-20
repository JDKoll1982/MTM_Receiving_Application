# Convert-Border.ps1
# Converts XAML Border to Penpot rectangle shape

function Convert-Border {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Id,
    
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
    [int]$X = 0,
    [int]$Y = 0,
    [int]$Width = 200,
    [int]$Height = 100,
    
    [string]$ParentId,
    [string]$FrameId,
    [string]$PageId,
    
    [string]$Background = "#FFFFFF",
    [string]$BorderBrush = "#E0E0E0",
    [int]$BorderThickness = 1,
    [int]$CornerRadius = 0,
    [array]$ChildIds = @()
)

$strokesJson = if ($BorderThickness -gt 0) {
    "[{`"strokeColor`": `"$BorderBrush`", `"strokeWidth`": $BorderThickness}]"
} else {
    "[]"
}

$shapesJson = if ($ChildIds.Count -gt 0) {
    $shapesStr = $ChildIds | ForEach-Object { "`"$_`"" }
    ", `"shapes`": [" + ($shapesStr -join ", ") + "]"
} else {
    ""
}

$json = @"
{
  "id": "$Id",
  "name": "$Name",
  "type": "rect",
  "x": $X,
  "y": $Y,
  "width": $Width,
  "height": $Height,
  "rotation": 0,
  "selrect": {
    "x": $X,
    "y": $Y,
    "width": $Width,
    "height": $Height,
    "x1": $X,
    "y1": $Y,
    "x2": $($X + $Width),
    "y2": $($Y + $Height)
  },
  "points": [
    {"x": $X, "y": $Y},
    {"x": $($X + $Width), "y": $Y},
    {"x": $($X + $Width), "y": $($Y + $Height)},
    {"x": $X, "y": $($Y + $Height)}
  ],
  "transform": {"a": 1.0, "b": 0.0, "c": 0.0, "d": 1.0, "e": 0.0, "f": 0.0},
  "transformInverse": {"a": 1.0, "b": 0.0, "c": 0.0, "d": 1.0, "e": 0.0, "f": 0.0},
  "parentId": "$ParentId",
  "frameId": "$FrameId",
  "pageId": "$PageId",
  "r1": $CornerRadius,
  "r2": $CornerRadius,
  "r3": $CornerRadius,
  "r4": $CornerRadius,
  "fills": [{"fillColor": "$Background"}],
  "strokes": $strokesJson$shapesJson
}
"@

    return $json
}

# Convert-StackPanel.ps1
# Converts XAML StackPanel to Penpot group/container

function Convert-StackPanel {
    param(
        [Parameter(Mandatory=$true)]
        [string]$Id,
    
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
    [int]$X = 0,
    [int]$Y = 0,
    [int]$Width = 400,
    [int]$Height = 300,
    
    [string]$ParentId,
    [string]$FrameId,
    [string]$PageId,
    
    [string]$Orientation = "Vertical",
    [int]$Spacing = 8,
    [array]$ChildIds = @()
)

$shapesJson = if ($ChildIds.Count -gt 0) {
    $shapesStr = $ChildIds | ForEach-Object { "`"$_`"" }
    ", `"shapes`": [" + ($shapesStr -join ", ") + "]"
} else {
    ""
}

# StackPanel as transparent container/frame
$json = @"
{
  "id": "$Id",
  "name": "$Name",
  "type": "frame",
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
  "fills": [],
  "strokes": []$shapesJson
}
"@

    return $json
}
